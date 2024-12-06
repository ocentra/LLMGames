using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager;
using OcentraAI.LLMGames.Scriptable;
using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.Players
{
    public class ComputerLLMPlayer : LLMPlayer
    {
        private ComputerPlayerState currentState = ComputerPlayerState.CanTakeAction;

        public ComputerLLMPlayer(AuthPlayerData authPlayerData, int initialCoins, int index)
            : base(authPlayerData, PlayerType.Computer, initialCoins, index)
        {
            AdjustCoins(initialCoins);
        }

        public async UniTask MakeDecision(int currentBet, CancellationTokenSource globalCancellationTokenSource)
        {
            try
            {
                if (IsCancellationRequested(globalCancellationTokenSource) || currentState != ComputerPlayerState.CanTakeAction)
                {
                    return;
                }

                await SimulateThinkingTime(globalCancellationTokenSource);

                if (!HasSeenHand)
                {
                    await DecideInitialAction(currentBet, globalCancellationTokenSource);
                }
                else
                {
                    await DecideMainAction(currentBet, globalCancellationTokenSource);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("ComputerPlayer decision making was canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ComputerPlayer MakeDecision: {ex.Message}");
            }
        }

        private async UniTask DecideInitialAction(int currentBet, CancellationTokenSource globalCancellationTokenSource)
        {
            if (IsCancellationRequested(globalCancellationTokenSource))
            {
                return;
            }

            if (Random.value > 0.3f)
            {
                await TakeActionSeeHand(currentBet, globalCancellationTokenSource);
            }
            else
            {
                TakeActionPlayBlind();
            }
        }

        private async UniTask DecideMainAction(int currentBet, CancellationTokenSource globalCancellationTokenSource)
        {
            if (IsCancellationRequested(globalCancellationTokenSource))
            {
                return;
            }

            int handValue = CalculateHandValue();

            if (handValue >= 50)
            {
                if (Random.value > 0.7f)
                {
                    TakeActionShow();
                }
                else
                {
                    DecideOnBetOrRaise(currentBet, 0.8f);
                }
            }
            else if (handValue >= 25)
            {
                if (FloorCard != null && ShouldSwapCard())
                {
                    SwapWithFloorCard();
                    DecideOnBetOrRaise(currentBet, 0.6f);
                }
                else if (Random.value > 0.4f)
                {
                    DecideOnBetOrRaise(currentBet, 0.6f);
                }
                else
                {
                    await TakeActionDrawFromDeck(currentBet, globalCancellationTokenSource);
                }
            }
            else
            {
                if (FloorCard != null && ShouldSwapCard())
                {
                    SwapWithFloorCard();
                    TakeActionBet();
                }
                else if (FloorCard == null || Random.value > 0.7f)
                {
                    await TakeActionDrawFromDeck(currentBet, globalCancellationTokenSource);
                }
                else
                {
                    TakeActionFold();
                }
            }
        }

        private void DecideOnBetOrRaise(int currentBet, float raiseChance)
        {
            if (Random.value < raiseChance)
            {
                TakeActionRaise(currentBet);
            }
            else
            {
                TakeActionBet();
            }
        }

        private bool ShouldSwapCard()
        {
            if (FloorCard == null)
            {
                return false;
            }

            int worstCardValue = Hand.Min();
            return FloorCard.Rank.Value > worstCardValue;
        }

        private void SwapWithFloorCard()
        {
            if (FloorCard == null)
            {
                return;
            }

            PickAndSwap(FloorCard, Hand.FindWorstCard());
        }

        private async UniTask TakeActionSeeHand(int currentBet, CancellationTokenSource globalCancellationTokenSource)
        {
            if (IsCancellationRequested(globalCancellationTokenSource))
            {
                return;
            }

            TakeAction(PlayerAction.SeeHand);
            await SimulateThinkingTime(globalCancellationTokenSource, 1f);
            await DecideMainAction(currentBet, globalCancellationTokenSource);
        }

        private void TakeActionPlayBlind()
        {
            TakeAction(PlayerAction.PlayBlind);
        }

        private void TakeActionBet()
        {
            TakeAction(PlayerAction.Bet);
        }

        private void TakeActionFold()
        {
            TakeAction(PlayerAction.Fold);
        }

        private void TakeActionShow()
        {
            TakeAction(PlayerAction.Show);
        }

        private async UniTask TakeActionDrawFromDeck(int currentBet, CancellationTokenSource globalCancellationTokenSource)
        {
            TakeAction(PlayerAction.DrawFromDeck);
            await SimulateThinkingTime(globalCancellationTokenSource, 1f);
            await HandlePostDraw(currentBet, globalCancellationTokenSource);
        }

        private async UniTask HandlePostDraw(int currentBet, CancellationTokenSource globalCancellationTokenSource)
        {
            if (IsCancellationRequested(globalCancellationTokenSource))
            {
                return;
            }

            if (ShouldSwapCard())
            {
                SwapWithFloorCard();
                await SimulateThinkingTime(globalCancellationTokenSource, 0.5f);
            }

            int handValue = CalculateHandValue();
            if (handValue >= 40)
            {
                DecideOnBetOrRaise(currentBet, 0.7f);
            }
            else if (handValue >= 20)
            {
                TakeActionBet();
            }
            else
            {
                TakeActionFold();
            }
        }

        private void TakeActionRaise(int currentBet)
        {
            int raiseAmount = (int)(currentBet * Random.Range(1.5f, 3f));
            EventBus.Instance.Publish(new PlayerActionRaiseBetEvent(GetType(), raiseAmount.ToString()));
            currentState = ComputerPlayerState.ActionTaken;
        }

        private void TakeAction(PlayerAction action)
        {
            EventBus.Instance.Publish(new PlayerActionEvent<PlayerAction>(GetType(), action));
            currentState = action == PlayerAction.DrawFromDeck
                ? ComputerPlayerState.DrawnFromDeck
                : ComputerPlayerState.ActionTaken;
        }

        public override void ShowHand(bool showHands = false)
        {
            base.ShowHand(showHands);
          
            if (showHands)
            {
                //EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent<LLMPlayer>(this, true));

            }

        }

        public override void PickAndSwap(Card floorCard, Card swapCard)
        {
            base.PickAndSwap(floorCard, swapCard);
           // EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent<LLMPlayer>(this));
        }

        public void ResetState()
        {
            currentState = ComputerPlayerState.CanTakeAction;
        }

        private async UniTask SimulateThinkingTime(CancellationTokenSource globalCancellationTokenSource, float seconds = 0)
        {
            float thinkingTime = seconds == 0 ? Random.Range(2f, 5f) : seconds;
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(thinkingTime), cancellationToken: globalCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Thinking time simulation was canceled.");
            }
        }

        private bool IsCancellationRequested(CancellationTokenSource globalCancellationToken)
        {
            return globalCancellationToken?.IsCancellationRequested ?? false;
        }

        private enum ComputerPlayerState
        {
            CanTakeAction,
            ActionTaken,
            DrawnFromDeck
        }
    }
}
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class ComputerPlayer : Player
    {
        public ComputerPlayer(PlayerData playerData, int initialCoins)
            : base(playerData, PlayerType.Computer, initialCoins)
        {
            AdjustCoins(initialCoins);
        }
        private enum ComputerPlayerState
        {
            CanTakeAction,
            ActionTaken,
            DrawnFromDeck
        }

        private ComputerPlayerState currentState = ComputerPlayerState.CanTakeAction;

        public async Task MakeDecision(int currentBet)
        {
            try
            {
                if (IsCancellationRequested() || currentState != ComputerPlayerState.CanTakeAction)
                {
                    return;
                }

                await SimulateThinkingTime();

                if (!HasSeenHand)
                {
                    await DecideInitialAction(currentBet);
                }
                else
                {
                    await DecideMainAction(currentBet);
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

        private async Task DecideInitialAction(int currentBet)
        {
            if (IsCancellationRequested())
            {
                return;
            }

            if (UnityEngine.Random.value > 0.3f)
            {
                await TakeActionSeeHand(currentBet);
            }
            else
            {
                TakeActionPlayBlind();
            }
        }

        private async Task DecideMainAction(int currentBet)
        {
            if (IsCancellationRequested())
            {
                return;
            }

            int handValue = CalculateHandValue();

            if (handValue >= 50)
            {
                if (UnityEngine.Random.value > 0.7f)
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
                else if (UnityEngine.Random.value > 0.4f)
                {
                    DecideOnBetOrRaise(currentBet, 0.6f);
                }
                else
                {
                    await TakeActionDrawFromDeck(currentBet);
                }
            }
            else
            {
                if (FloorCard != null && ShouldSwapCard())
                {
                    SwapWithFloorCard();
                    TakeActionBet();
                }
                else if (FloorCard == null || UnityEngine.Random.value > 0.7f)
                {
                    await TakeActionDrawFromDeck(currentBet);
                }
                else
                {
                    TakeActionFold();
                }
            }
        }

        private void DecideOnBetOrRaise(int currentBet, float raiseChance)
        {
            if (UnityEngine.Random.value < raiseChance)
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
            if (FloorCard == null) return false;
            int worstCardValue = Hand.Min();
            return FloorCard.Rank.Value > worstCardValue;
        }

        private void SwapWithFloorCard()
        {
            if (FloorCard == null) return;
            PickAndSwap(FloorCard, Hand.FindWorstCard());
        }

        private async Task TakeActionSeeHand(int currentBet)
        {
            if (IsCancellationRequested())
            {
                return;
            }

            TakeAction(PlayerAction.SeeHand);
            await SimulateThinkingTime(1f);
            await DecideMainAction(currentBet);
        }

        private void TakeActionPlayBlind() => TakeAction(PlayerAction.PlayBlind);
        private void TakeActionBet() => TakeAction(PlayerAction.Bet);
        private void TakeActionFold() => TakeAction(PlayerAction.Fold);
        private void TakeActionShow() => TakeAction(PlayerAction.Show);

        private async Task TakeActionDrawFromDeck(int currentBet)
        {
            TakeAction(PlayerAction.DrawFromDeck);
            await SimulateThinkingTime(1f);
            await HandlePostDraw(currentBet);
        }

        private async Task HandlePostDraw(int currentBet)
        {
            if (IsCancellationRequested())
            {
                return;
            }

            if (ShouldSwapCard())
            {
                SwapWithFloorCard();
                await SimulateThinkingTime(0.5f);
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
            int raiseAmount = (int)(currentBet * UnityEngine.Random.Range(1.5f, 3f));
            EventBus.Publish(new PlayerActionRaiseBet(GetType(), raiseAmount.ToString()));
            currentState = ComputerPlayerState.ActionTaken;
        }

        private void TakeAction(PlayerAction action)
        {
            EventBus.Publish(new PlayerActionEvent(GetType(), action));
            currentState = action == PlayerAction.DrawFromDeck ? ComputerPlayerState.DrawnFromDeck : ComputerPlayerState.ActionTaken;
        }

        public override void ShowHand(bool showHands = false)
        {
            base.ShowHand(showHands);
            EventBus.Publish(new UpdatePlayerHandDisplay(this, showHands));
        }

        public override void PickAndSwap(Card floorCard, Card swapCard)
        {
            base.PickAndSwap(floorCard, swapCard);
            EventBus.Publish(new UpdatePlayerHandDisplay(this));
        }

        public void ResetState()
        {
            currentState = ComputerPlayerState.CanTakeAction;
        }

        private async Task SimulateThinkingTime(float seconds = 0)
        {
            float thinkingTime = seconds == 0 ? UnityEngine.Random.Range(2f, 5f) : seconds;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(thinkingTime), GameManager.Instance.GlobalCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("Thinking time simulation was canceled.");
            }
        }

        private bool IsCancellationRequested()
        {
            return GameManager.Instance.GlobalCancellationTokenSource?.IsCancellationRequested ?? false;
        }
    }
}
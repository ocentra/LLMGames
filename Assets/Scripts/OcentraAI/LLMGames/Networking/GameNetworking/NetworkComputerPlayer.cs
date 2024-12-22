using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkComputerPlayer : NetworkPlayer, IComputerPlayerData
{
    [ShowInInspector] private int RaiseAmount { get; set; }


    // AI-specific properties
    [ShowInInspector]
    public int DifficultyLevel { get; set; } = 1;

    [ShowInInspector]
    public string AIModelName { get; set; } = "DefaultAI";

    public async UniTask<bool> SimulateComputerPlayerTurn(ulong currentPlayerID, int currentBet)
    {
        if (!IsServer)
        {
            return false;
        }


        if (currentPlayerID == PlayerId.Value)
        {
            try
            {
                await MakeDecision(currentBet);

                PlayerDecisionEvent eventToPublish = null;

                switch (PlayerDecision.Name)
                {
                    // Special case: RaiseBet with a custom event
                    case nameof(PlayerDecision.RaiseBet):

                        eventToPublish = new PlayerDecisionRaiseBetEvent(PlayerDecision, RaiseAmount);

                        break;

                    case nameof(PlayerDecision.SeeHand):
                        eventToPublish = new PlayerDecisionBettingEvent(PlayerDecision);
                        break;

                    // General betting decisions
                    case nameof(PlayerDecision.ShowCall):
                    case nameof(PlayerDecision.Fold):
                    case nameof(PlayerDecision.PlayBlind):
                    case nameof(PlayerDecision.Bet):
                    case nameof(PlayerDecision.DrawFromDeck):
                        eventToPublish = new PlayerDecisionBettingEvent(PlayerDecision);
                        break;

                    // Wildcard-related decisions
                    case nameof(PlayerDecision.WildCard0):
                    case nameof(PlayerDecision.WildCard1):
                    case nameof(PlayerDecision.WildCard2):
                    case nameof(PlayerDecision.WildCard3):
                    case nameof(PlayerDecision.Trump):
                        eventToPublish = new PlayerDecisionWildcardEvent(PlayerDecision);
                        break;

                    // UI-related decisions
                    case nameof(PlayerDecision.ShowAllFloorCards):
                    case nameof(PlayerDecision.PurchaseCoins):
                        eventToPublish = new PlayerDecisionUIEvent(PlayerDecision);
                        break;

                    // Default fallback
                    default:
                        eventToPublish = new PlayerDecisionEvent(PlayerDecision);
                        break;
                }
                

                await EventBus.Instance.PublishAsync(new ProcessDecisionEvent(eventToPublish, PlayerId.Value));

                await UniTask.Yield();

                return true;

            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in OnComputerPlayerTurn: {ex.Message}");
            }
        }

        return false;
    }







    public async UniTask MakeDecision(int currentBet)
    {
        await SimulateThinkingTime();

        if (!HasSeenHand.Value)
        {
            await DecideInitialAction(currentBet);
        }
        else
        {
            await DecideMainAction(currentBet);
        }
    }

    private async UniTask DecideInitialAction(int currentBet)
    {

        if (Random.value > 0.3f)
        {
            await TakeActionSeeHand(currentBet);
        }
        else
        {
            PlayerDecision = PlayerDecision.PlayBlind;
        }
    }

    private async UniTask DecideMainAction(int currentBet)
    {


        int handValue = CalculateHandValue();

        if (handValue >= 50)
        {
            if (Random.value > 0.7f)
            {
                PlayerDecision = PlayerDecision.ShowCall;
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
                await TakeActionDrawFromDeck(currentBet);
            }
        }
        else
        {
            if (FloorCard != null && ShouldSwapCard())
            {
                SwapWithFloorCard();
                PlayerDecision = PlayerDecision.Bet;
            }
            else if (FloorCard == null || Random.value > 0.7f)
            {
                await TakeActionDrawFromDeck(currentBet);
            }
            else
            {
                PlayerDecision = PlayerDecision.Fold;
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
            PlayerDecision = PlayerDecision.Bet;
        }
    }

    private async UniTask TakeActionSeeHand(int currentBet)
    {
        PlayerDecision = PlayerDecision.SeeHand;
        await SimulateThinkingTime(1f);
        await DecideMainAction(currentBet);
    }

    private async void SwapWithFloorCard()
    {
        if (FloorCard == null)
        {
            return;
        }

        await PickAndSwap(FloorCard, Hand.FindWorstCard());
    }



    private void TakeActionRaise(int currentBet)
    {
        RaiseAmount = (int)(currentBet * Random.Range(1.5f, 3f));
    }

    private async UniTask TakeActionDrawFromDeck(int currentBet)
    {
        PlayerDecision = PlayerDecision.DrawFromDeck;
        await SimulateThinkingTime(1f);
        await HandlePostDraw(currentBet);
    }

    private async UniTask HandlePostDraw(int currentBet)
    {


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

            PlayerDecision = PlayerDecision.Bet;
        }
        else
        {
            PlayerDecision = PlayerDecision.Fold;
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

    private async UniTask SimulateThinkingTime(float seconds = 0)
    {
        float thinkingTime = seconds == 0 ? Random.Range(2f, 5f) : seconds;
        await UniTask.WaitForSeconds(thinkingTime);
        await UniTask.Yield();
    }





}

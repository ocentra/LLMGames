using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GamesNetworking;
using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkComputerPlayer : NetworkPlayer, IComputerPlayerData
{


    // AI-specific properties
    [ShowInInspector]
    public int DifficultyLevel { get; set; } = 1;

    [ShowInInspector]
    public string AIModelName { get; set; } = "DefaultAI";

    
    
    //todo have a auto dession





    public async UniTask MakeDecision(int currentBet, CancellationTokenSource globalCancellationTokenSource)
    {
        try
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
        catch (OperationCanceledException)
        {
            Debug.Log("ComputerPlayer decision making was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in ComputerPlayer MakeDecision: {ex.Message}");
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

    private void SwapWithFloorCard()
    {
        if (FloorCard == null)
        {
            return;
        }

        PickAndSwap(FloorCard, Hand.FindWorstCard());
    }

    public void PickAndSwap(Card floorCard, Card swapCard)
    {

        // EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent<LLMPlayer>(this));
    }


    private void TakeActionRaise(int currentBet)
    {
        int raiseAmount = (int)(currentBet * Random.Range(1.5f, 3f));
        // EventBus.Instance.Publish(new PlayerActionRaiseBetEvent(GetType(), raiseAmount.ToString()));

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

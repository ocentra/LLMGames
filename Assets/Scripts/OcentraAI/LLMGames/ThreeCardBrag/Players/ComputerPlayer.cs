using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class ComputerPlayer : Player
    {
        public async Task MakeDecision(int currentBet)
        {
            await SimulateThinkingAndMakeDecision(currentBet);
        }

        private async Task SimulateThinkingAndMakeDecision(int currentBet)
        {
            // Simulate thinking time
            float thinkingTime = Random.Range(2f, 5f);
            await Task.Delay((int)(thinkingTime * 1000));

            if (!HasSeenHand)
            {
                if (Random.value > 0.3f)
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.SeeHand));
                    await SimulateThinkingAndMakeDecision(currentBet);
                }
                else
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.PlayBlind));
                }
                return;
            }

            int handValue = CalculateHandValue();

            if (handValue >= 50)
            {
                if (Random.value > 0.7f)
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Show));
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
                }
                else if (Random.value > 0.4f)
                {
                    DecideOnBetOrRaise(currentBet, 0.6f);
                }
                else
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.DrawFromDeck));
                    await HandleDrawnCard(currentBet);
                }
            }
            else
            {
                if (FloorCard != null && ShouldSwapCard())
                {
                    SwapWithFloorCard();
                }
                else if (FloorCard == null || Random.value > 0.7f)
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.DrawFromDeck));
                    await HandleDrawnCard(currentBet);
                }
                else
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Fold));
                }
            }
        }

        private void DecideOnBetOrRaise(int currentBet, float raiseChance)
        {
            if (Random.value < raiseChance)
            {
                int raiseAmount = (int)(currentBet * Random.Range(1.5f, 3f));
                EventBus.Publish(new PlayerActionRaiseBet(GetType(), raiseAmount.ToString()));
            }
            else
            {
                EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Bet));
            }
        }

        private bool ShouldSwapCard()
        {
            if (FloorCard == null) return false;
            int worstCardValue = Hand.Min(card => card.GetRankValue());
            return FloorCard.GetRankValue() > worstCardValue;
        }

        private void SwapWithFloorCard()
        {
            if (FloorCard == null) return;
            int worstCardIndex = Hand.FindIndex(c => c.GetRankValue() == Hand.Min(card => card.GetRankValue()));
            EventBus.Publish(new PlayerActionPickAndSwap(GetType(), FloorCard, Hand[worstCardIndex]));
        }

        private async Task HandleDrawnCard(int currentBet)
        {
            // Simulate thinking about the drawn card
            await Task.Delay(Random.Range(1000, 3000));

            if (FloorCard != null && ShouldSwapCard())
            {
                SwapWithFloorCard();
            }

            // Make a decision after drawing/swapping
            int handValue = CalculateHandValue();
            if (handValue >= 40)
            {
                DecideOnBetOrRaise(currentBet, 0.7f);
            }
            else if (handValue >= 20)
            {
                EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Bet));
            }
            else
            {
                EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Fold));
            }
        }

        public override void SeeHand()
        {
            base.SeeHand();
            EventBus.Publish(new UpdatePlayerHandDisplay(this));
        }

        public override void ShowHand(bool isRoundEnd = false)
        {
            base.ShowHand(isRoundEnd);
            EventBus.Publish(new UpdatePlayerHandDisplay(this));
        }

        public override void PickAndSwap(Card floorCard, Card swapCard)
        {
            base.PickAndSwap(floorCard, swapCard);
            EventBus.Publish(new UpdatePlayerHandDisplay(this));
        }
    }
}
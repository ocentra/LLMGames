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

        public async void MakeDecision(int currentBet)
        {
            await SimulateThinkingAndMakeDecision();
        }

        private async Task SimulateThinkingAndMakeDecision()
        {
            // Simulate thinking time
            float thinkingTime = Random.Range(1f, 3f); // Random thinking time between 1 and 10 seconds
            await Task.Delay((int)(thinkingTime * 1000));

            if (!HasSeenHand)
            {
                if (Random.value > 0.5f)
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.SeeHand));

                    await SimulateThinkingAndMakeDecision();
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
                EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Show));

            }
            else if (handValue >= 25)
            {
                if (Random.value > 0.5f)
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.Bet));

                }
                else
                {
                    EventBus.Publish(new PlayerActionRaiseBet(typeof(HumanPlayer), (-1).ToString()));
                }

            }
            else
            {
                if (FloorCard != null)
                {
                    int worstCardIndex = FindWorstCardIndex();
                    int worstCardValue = Hand[worstCardIndex].GetRankValue();
                    int floorCardValue = FloorCard.GetRankValue();

                    if (floorCardValue > worstCardValue)
                    {
                        EventBus.Publish(new PlayerActionPickAndSwap(GetType(), FloorCard, Hand[worstCardIndex]));

                    }
                    else
                    {
                        EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.DrawFromDeck));

                    }
                }
                else
                {
                    EventBus.Publish(new PlayerActionEvent(GetType(), PlayerAction.DrawFromDeck));

                }
            }

        }

        private int FindWorstCardIndex()
        {
            return Hand.FindIndex(c => c.GetRankValue() == Hand.Min(card => card.GetRankValue()));
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

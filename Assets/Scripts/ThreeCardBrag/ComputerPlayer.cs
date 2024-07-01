using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ThreeCardBrag
{
    public class ComputerPlayer : Player
    {
        private Card FloorCard => GameController.Instance.DeckManager.FloorCard;

        public void MakeDecision(int currentBet)
        {
            // todo send the hand and all prompt to LLM , make a prompt class and define prompt
            // Start the async method to simulate thinking for now
            SimulateThinkingAndMakeDecision();
        }

        private async void SimulateThinkingAndMakeDecision()
        {
            // Simulate thinking time
            float thinkingTime = Random.Range(1f, 10f); // Random thinking time between 1 and 3 seconds
            await Task.Delay((int)(thinkingTime * 1000));

            // Make the decision after the delay
            int handValue = CalculateHandValue();

            if (!HasSeenHand)
            {
                TakeAction(Random.value > 0.5f ? PlayerAction.PlayBlind : PlayerAction.SeeHand);
            }
            else if (handValue >= 35)
            {
                TakeAction(PlayerAction.Show);
            }
            else if (handValue >= 25)
            {
                TakeAction(Random.value > 0.5f ? PlayerAction.Call : PlayerAction.Raise);
            }
            else
            {
                if (Random.value > 0.3f)
                {
                    TakeAction(PlayerAction.DrawFromDeck);
                }
                else
                {
                    if (FloorCard != null)
                    {
                        int worstCardIndex = FindWorstCardIndex();
                        GameController.Instance.DeckManager.SetSwapCard(Hand[worstCardIndex]);
                        TakeAction(PlayerAction.PickAndSwap);
                    }
                }
            }

            GameController.Instance.UIController.SetComputerSeenHand(HasSeenHand);


            GameController.Instance.UIController.ActionTaken = true;
        }

        private int FindWorstCardIndex()
        {
            return Hand.FindIndex(c => c.GetRankValue() == Hand.Min(card => card.GetRankValue()));
        }

        public override void SeeHand()
        {
            base.SeeHand();
            // Don't trigger UI update for computer's hand
        }

        public override void ShowHand(bool isRoundEnd = false)
        {
            base.ShowHand(isRoundEnd);
            GameController.Instance.UIController.UpdateComputerHandDisplay(isRoundEnd);
        }
    }
}

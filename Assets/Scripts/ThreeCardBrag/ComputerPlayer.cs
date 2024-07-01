using System.Linq;
using UnityEngine;

namespace ThreeCardBrag
{
    public class ComputerPlayer : Player
    {
        private Card FloorCard => GameController.Instance.DeckManager.FloorCard;


        public void MakeDecision(int currentBet)
        {
            int handValue = CalculateHandValue();

            if (!HasSeenHand)
            {
                if (Random.value > 0.5f)
                {
                    TakeAction(PlayerAction.PlayBlind);

                }
                else
                {
                    TakeAction(PlayerAction.SeeHand);
                }
            }
            else if (handValue >= 35)
            {
                TakeAction(PlayerAction.Show);
            }
            else if (handValue >= 25)
            {
                if (Random.value > 0.5f)
                {
                    TakeAction(PlayerAction.Call);
                }
                else
                {
                    TakeAction(PlayerAction.Raise);
                }
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
                        TakeAction(PlayerAction.PickAndSwap);
                    }
                }
            }

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

        public override void ShowHand()
        {
            // Only log to console, don't update UI
            Debug.Log($"Computer's hand value: {CalculateHandValue()}");
        }
    }
}
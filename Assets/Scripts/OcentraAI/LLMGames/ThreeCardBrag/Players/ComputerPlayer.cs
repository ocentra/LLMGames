using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class ComputerPlayer : Player
    {
        private Card FloorCard => GameManager.Instance.DeckManager.FloorCard;

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
                    TakeAction(PlayerAction.SeeHand);
                    await SimulateThinkingAndMakeDecision();
                    GameManager.Instance.UIController.SetComputerSeenHand(true);
                }
                else
                {
                    TakeAction(PlayerAction.PlayBlind);
                    GameManager.Instance.UIController.ActionCompletionSource.SetResult(true);
                }
                return;
            }

            int handValue = CalculateHandValue();

            if (handValue >= 50)
            {
                TakeAction(PlayerAction.Show);
            }
            else if (handValue >= 25)
            {
                TakeAction(Random.value > 0.5f ? PlayerAction.Bet : PlayerAction.Raise);
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
                        GameManager.Instance.DeckManager.SetSwapCard(Hand[worstCardIndex]);
                        TakeAction(PlayerAction.PickAndSwap);
                    }
                    else
                    {
                        TakeAction(PlayerAction.DrawFromDeck);
                    }
                }
                else
                {
                    TakeAction(PlayerAction.DrawFromDeck);
                }
            }

            GameManager.Instance.UIController.ActionCompletionSource.SetResult(true);
        }

        private int FindWorstCardIndex()
        {
            return Hand.FindIndex(c => c.GetRankValue() == Hand.Min(card => card.GetRankValue()));
        }

        public override void Raise()
        {
            float randomMultiplier = Random.Range(0.25f, 3f);
            // because raise have to be double + if just doubble its normal bet!
            int newBet = (int)(GameManager.Instance.CurrentBet * 2 + GameManager.Instance.CurrentBet * randomMultiplier);

            GameManager.Instance.SetCurrentBet(newBet);
            base.Raise();
        }

        public override void SeeHand()
        {
            base.SeeHand();
            // Don't trigger UI update for computer's hand
        }

        public override void ShowHand(bool isRoundEnd = false)
        {
            base.ShowHand(isRoundEnd);
            GameManager.Instance.UIController.UpdateComputerHandDisplay(isRoundEnd);
        }
    }
}

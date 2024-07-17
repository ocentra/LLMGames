using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class Player
    {


        [ShowInInspector, ReadOnly]
        public string PlayerName { get; private set; }
        [ShowInInspector, ReadOnly]
        public List<Card> Hand { get; private set; } = new List<Card>();

        [ShowInInspector, ReadOnly]
        public int Coins { get; private set; }
        [ShowInInspector, ReadOnly]
        public bool HasSeenHand { get; private set; } = false;
        [ShowInInspector, ReadOnly]
        public bool HasBetOnBlind { get; private set; } = true;

        protected Card FloorCard { get;  set; } 

        public virtual void SetName(string playerName)
        {
            PlayerName = playerName;
        }



        public virtual void SeeHand()
        {
            HasSeenHand = true;
        }

        public virtual void Bet(int amount)
        {
            AdjustCoins(-amount);

        }

        public virtual void Raise(int amount)
        {
            AdjustCoins(-amount);
        }

        public virtual void Fold()
        {
            Hand.Clear();
        }

        public virtual void DrawFromDeck()
        {
            EventBus.Publish(new SetFloorCard());
        }

        public virtual void PickAndSwap(Card floorCard, Card swapCard)
        {
            if (floorCard != null && swapCard != null)
            {
                SwapCard(floorCard, swapCard);

                EventBus.Publish(new SetFloorCard(swapCard));
            }
        }




        public virtual void BetOnBlind(int amount)
        {
            AdjustCoins(-amount);
            HasBetOnBlind = true;

        }

        public int CalculateHandValue()
        {
            int handValue = Hand.Sum(card => card.GetRankValue());
            BaseBonusRule[] bonusRules = GameInfo.Instance.BonusRules;

            foreach (BaseBonusRule rule in bonusRules)
            {
                if (rule.Evaluate(Hand))
                {
                    handValue += rule.BonusValue;
                }
            }

            return handValue;
        }

        public int GetHighestCardValue()
        {
            return Hand.Max(card => card.GetRankValue());
        }

        public virtual void ShowHand(bool isRoundEnd = false)
        {
            foreach (Card card in Hand)
            {
                // Debug.Log($"{PlayerName}'s card: {card.Rank} of {card.Suit}");
            }
            //Debug.Log($"{PlayerName}'s hand value: {CalculateHandValue()}");


        }

        public void ResetForNewRound()
        {
            Hand.Clear();
            HasSeenHand = false;
            HasBetOnBlind = true;
        }

        public void AdjustCoins(int amount)
        {
            Coins += amount;


        }

        public void SwapCard(Card floorCard, Card swapCard )
        {
            for (int index = 0; index < Hand.Count; index++)
            {
                Card cardInHand = Hand[index];
                if (cardInHand.Suit == swapCard.Suit && cardInHand.Rank == swapCard.Rank)
                {
                    Hand[index] = floorCard;
                    break;
                }
            }

        }
    }
}

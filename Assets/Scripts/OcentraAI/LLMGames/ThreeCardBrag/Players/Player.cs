using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class Player
    {
        [ShowInInspector, ReadOnly] public PlayerData PlayerData { get; private set; }
        [ShowInInspector, ReadOnly] public string Id { get; private set; }
        [ShowInInspector, ReadOnly] public string PlayerName { get; private set; }
        [ShowInInspector, ReadOnly] public PlayerType Type { get; private set; }
        [ShowInInspector, ReadOnly] public List<Card> Hand { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public int Coins { get; private set; }
        [ShowInInspector, ReadOnly] public bool HasSeenHand { get; private set; } = false;
        [ShowInInspector, ReadOnly] public bool HasBetOnBlind { get; private set; } = true;
        [ShowInInspector, ReadOnly] public bool HasFolded { get; private set; } = false;
        protected Card FloorCard { get; set; }

        public Player(PlayerData playerData, PlayerType type, int initialCoins)
        {
            PlayerData = playerData;
            Id = playerData.PlayerID;
            PlayerName = playerData.PlayerName;
            Type = type;
            Coins = initialCoins;
        }

        public virtual void SetName(string playerName)
        {
            PlayerName = playerName;
        }

        public virtual void SeeHand()
        {
            HasSeenHand = true;
        }

        public bool CanAffordBet(int betAmount) => Coins >= betAmount;

        public virtual void Bet(int amount)
        {
            if (CanAffordBet(amount))
            {
                AdjustCoins(-amount);
            }
        }

        public virtual void Raise(int amount)
        {
            if (CanAffordBet(amount))
            {
                AdjustCoins(-amount);
            }
        }

        public virtual void Fold()
        {
            HasFolded = true;
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
            if (CanAffordBet(amount))
            {
                AdjustCoins(-amount);
                HasBetOnBlind = true;
            }
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
            // Debug.Log($"{PlayerName}'s hand value: {CalculateHandValue()}");
        }

        public void ResetForNewRound(DeckManager deckManager)
        {
            Hand.Clear();

            for (int i = 0; i < 3; i++)
            {
                Hand.Add(deckManager.DrawCard());
            }

            HasSeenHand = false;
            HasBetOnBlind = true;
            HasFolded = false;
        }

        public void SetInitialCoins(int amount)
        {
            Coins = amount;
        }
        public void AdjustCoins(int amount)
        {
            Coins += amount;
        }

        public void SwapCard(Card floorCard, Card swapCard)
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

        public virtual void DrawCard(Card card)
        {
            Hand.Add(card);
        }
    }

    public enum PlayerType
    {
        Human,
        Computer
    }
}
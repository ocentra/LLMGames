using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
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

        [ShowInInspector, ReadOnly] public int HandRankSum { get; private set; }
        [ShowInInspector, ReadOnly] public int HandValue { get; private set; }

        public Dictionary<string, Card> WildCards { get; private set; }

        public Dictionary<string, Card> WildCardInHand { get; private set; } 

        [ShowInInspector, ReadOnly]
        public List<BaseBonusRule> AppliedRules { get; private set; } = new List<BaseBonusRule>();

        protected Card FloorCard { get; set; }

        public Player(PlayerData playerData, PlayerType type, int initialCoins)
        {
            PlayerData = playerData;
            Id = playerData.PlayerID;
            PlayerName = playerData.PlayerName;
            Type = type;
            Coins = initialCoins;
            WildCardInHand = new Dictionary<string, Card>();
            SubscribeToEvents();
        }

        ~Player()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<UpdateWildCards>(OnUpdateWildCards);

        }



        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<UpdateWildCards>(OnUpdateWildCards);

        }


        private void CheckForWildCardsInHand()
        {
            WildCardInHand = new Dictionary<string, Card>();

            foreach (KeyValuePair<string, Card> card in WildCards)
            {
                if (Hand.Any(handCard => handCard.Id == card.Value.Id))
                {
                    WildCardInHand.Add(card.Key, card.Value);
                }
            }


        }

        private void OnUpdateWildCards(UpdateWildCards obj)
        {
            WildCards = obj.WildCards;
        }
        
        public virtual void SeeHand()
        {
            HasSeenHand = true;
        }

        public static string GetFormattedHand(List<Card> cards)
        {
            return string.Join(" ", cards.Select(card => card.RankSymbol));
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
            AppliedRules = new List<BaseBonusRule>();
            HandRankSum = Hand.Sum(card => card.GetRankValue());
            HandValue = HandRankSum;
            List<BaseBonusRule> bonusRules = GameManager.Instance.GameMode.BonusRules;
            foreach (BaseBonusRule rule in bonusRules)
            {
                if (rule.Evaluate(Hand , out BonusDetails bonusDetails))
                {
                    HandValue += bonusDetails.TotalBonus;
                    AppliedRules.Add(rule);
                }
            }
            return HandValue;
        }

        public int GetHighestCardValue()
        {
            return Hand.Max(card => card.GetRankValue());
        }

        public virtual void ShowHand(bool showHands = false)
        {
            foreach (Card card in Hand)
            {
                // Debug.Log($"{PlayerName}'s card: {card.Rank} of {card.Suit}");
            }
            // Debug.Log($"{PlayerName}'s hand value: {CalculateHandValue()}");
        }

        public virtual void ResetForNewRound(DeckManager deckManager, List<Card> customHand = null)
        {
            Hand = new List<Card>();
            AppliedRules = new List<BaseBonusRule>();
            HandRankSum = 0;

            // this is temp solution for quick testing on dev mode 
            if (customHand is { Count: >= 3 })
            {
                Hand = new List<Card>(customHand);
                deckManager.RemoveCardsFromDeck(customHand);
                HasSeenHand = true;
            }
            else
            {
                for (int i = 0; i < GameManager.Instance.GameMode.NumberOfCards; i++)
                {
                    Hand.Add(deckManager.DrawCard());
                }
               
            }

            Hand = Hand.OrderByDescending(card => card.Rank).ToList();

            CheckForWildCardsInHand();
            HasBetOnBlind = false;
            HasFolded = false;
            HasSeenHand = false;
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

            CheckForWildCardsInHand();
        }


    }

    public enum PlayerType
    {
        Human,
        Computer
    }
}
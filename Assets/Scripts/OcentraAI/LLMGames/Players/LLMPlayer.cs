using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Manager;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.Players
{
    public class LLMPlayer
    {
        public LLMPlayer(AuthPlayerData authPlayerData, PlayerType type, int initialCoins, int index)
        {
            PlayerIndex = index;
            AuthPlayerData = authPlayerData;
            Type = type;
            Coins = initialCoins;
            WildCardInHand = new Dictionary<string, Card>();
            SubscribeToEvents();
        }

        [SerializeField]
        [ReadOnly]
        [ShowInInspector]
        public int PlayerIndex { get; private set; }

        [ShowInInspector] [ReadOnly] public AuthPlayerData AuthPlayerData { get; private set; }
        [ShowInInspector] [ReadOnly] public PlayerType Type { get; private set; }
        [ShowInInspector] [ReadOnly] public Hand Hand { get; private set; }
        [ShowInInspector] [ReadOnly] public int Coins { get; private set; }
        [ShowInInspector] [ReadOnly] public bool HasSeenHand { get; private set; }
        [ShowInInspector] [ReadOnly] public bool HasBetOnBlind { get; private set; } = true;
        [ShowInInspector] [ReadOnly] public bool HasFolded { get; private set; }

        [ShowInInspector] [ReadOnly] public int HandRankSum { get; private set; }
        [ShowInInspector] [ReadOnly] public int HandValue { get; private set; }

        [ShowInInspector] [ReadOnly] public List<BonusDetail> BonusDetails { get; private set; }

        public Dictionary<string, Card> WildCards { get; private set; }

        public Dictionary<string, Card> WildCardInHand { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        public List<BaseBonusRule> AppliedRules { get; private set; } = new List<BaseBonusRule>();

        protected Card FloorCard { get; set; }

        ~LLMPlayer()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<UpdateWildCardsEvent<GameMode, Card>>(OnUpdateWildCards);
        }


        private void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<UpdateWildCardsEvent<GameMode, Card>>(OnUpdateWildCards);
        }


        private void CheckForWildCardsInHand()
        {
            WildCardInHand = new Dictionary<string, Card>();

            foreach (KeyValuePair<string, Card> card in WildCards)
            {
                if (Hand.Any(card.Value.Id))
                {
                    WildCardInHand.Add(card.Key, card.Value);
                }
            }
        }

        private void OnUpdateWildCards(UpdateWildCardsEvent<GameMode, Card> obj)
        {
            WildCards = obj.WildCards;
        }

        public virtual void SeeHand()
        {
            HasSeenHand = true;
        }


        public bool CanAffordBet(int betAmount)
        {
            return Coins >= betAmount;
        }

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
            EventBus.Instance.Publish(new SetFloorCardEvent<Card>());
        }

        public virtual void PickAndSwap(Card floorCard, Card swapCard)
        {
            if (floorCard != null && swapCard != null)
            {
                SwapCard(floorCard, swapCard);
                EventBus.Instance.Publish(new SetFloorCardEvent<Card>(swapCard));
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
            BonusDetails = new List<BonusDetail>();
            HandRankSum = Hand.Sum();
            HandValue = HandRankSum;
            List<BaseBonusRule> bonusRules = GameManager.Instance.GameMode.BonusRules;
            foreach (BaseBonusRule rule in bonusRules)
            {
                if (rule.Evaluate(Hand, out BonusDetail bonusDetails))
                {
                    HandValue += bonusDetails.TotalBonus;

                    if (!AppliedRules.Contains(rule))
                    {
                        AppliedRules.Add(rule);
                    }

                    if (!BonusDetails.Contains(bonusDetails))
                    {
                        BonusDetails.Add(bonusDetails);
                    }
                }
            }

            return HandValue;
        }

        public int GetHighestCardValue()
        {
            return Hand.Max();
        }

        public virtual void ShowHand(bool showHands = false)
        {

            
        }

        public virtual void ResetForNewRound(DeckManager deckManager, Hand customHand = null)
        {
            AppliedRules = new List<BaseBonusRule>();
            HandRankSum = 0;

            // this is temp solution for quick testing on dev mode 
            if (customHand != null)
            {
                Hand = customHand;
                deckManager.RemoveCardsFromDeck(customHand.GetCards().ToList());
                HasSeenHand = true;
            }
            else
            {
                List<Card> cards = new List<Card>();
                for (int i = 0; i < GameManager.Instance.GameMode.NumberOfCards; i++)
                {
                    cards.Add(deckManager.DrawCard());
                }

                Hand = new Hand(cards);
            }

            Hand.OrderByDescending();

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
            for (int index = 0; index < Hand.Count(); index++)
            {
                Card cardInHand = Hand.GetCard(index);
                if (cardInHand.Suit == swapCard.Suit && cardInHand.Rank == swapCard.Rank)
                {
                    Hand.ReplaceCard(index, floorCard);
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
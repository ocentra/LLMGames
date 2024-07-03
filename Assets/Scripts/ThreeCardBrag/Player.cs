using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThreeCardBrag
{
    public class Player
    {
        private DeckManager DeckManager => GameManager.Instance.DeckManager;

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

        public event Action<PlayerAction> OnActionTaken;
        public event Action OnCoinsChanged;

        public void TakeAction(PlayerAction action)
        {
            GameManager.Instance.UIController.ShowMessage($"Player {PlayerName} Took Action {action}", 5f);
            OnActionTaken?.Invoke(action);
        }

        public virtual void SetName(string playerName)
        {
            PlayerName = playerName;
        }



        public virtual void SeeHand()
        {
            HasSeenHand = true;
            ShowHand();
        }

        public virtual void Bet()
        {
            AdjustCoins(-GameManager.Instance.CurrentBet);
        }

        public virtual void Raise()
        {
            AdjustCoins(-GameManager.Instance.CurrentBet);
        }

        public virtual void Fold()
        {
            Hand.Clear();
        }

        public virtual void DrawFromDeck()
        {
            DeckManager.SetFloorCard(DeckManager.DrawCard());
            GameManager.Instance.UIController.UpdateFloorCard();
        }

        public virtual void PickAndSwap()
        {
            Card floorCard = DeckManager.FloorCard;
            Card swapCard = DeckManager.SwapCard;

            if (floorCard != null && swapCard != null)
            {
                DeckManager.AddToFloorCardList(swapCard);

                int swapCardIndex = -1;
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand[i] == swapCard)
                    {
                        swapCardIndex = i;
                        break;
                    }
                }

                if (swapCardIndex >= 0)
                {
                    Hand[swapCardIndex] = floorCard;
                }
                else
                {
                    Debug.LogError($" no card matches swapCard {swapCard.Suit} of {swapCard.Rank} in hand");
                    return;
                }

                DeckManager.SetFloorCard(null);
                DeckManager.SetSwapCard(null);


                GameManager.Instance.UIController.UpdateGameState();
            }
        }




        public virtual void BetOnBlind()
        {
            AdjustCoins(-GameManager.Instance.CurrentBet);
            HasBetOnBlind = true;
        }

        public int CalculateHandValue()
        {
            int handValue = Hand.Sum(card => card.GetRankValue());
            var bonusRules = GameInfo.Instance.BonusRules;

            foreach (var rule in bonusRules)
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
            foreach (var card in Hand)
            {
                Debug.Log($"{PlayerName}'s card: {card.Rank} of {card.Suit}");
            }
            Debug.Log($"{PlayerName}'s hand value: {CalculateHandValue()}");
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
            OnCoinsChanged?.Invoke();
        }
    }
}

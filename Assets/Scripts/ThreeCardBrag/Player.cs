using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThreeCardBrag
{
    public class Player
    {
        private DeckManager DeckManager => GameController.Instance.DeckManager;

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
            GameController.Instance.UIController.ShowMessage($"Player {PlayerName} Took Action {action}");
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
            AdjustCoins(-GameController.Instance.CurrentBet);
        }

        public virtual void Raise()
        {
            AdjustCoins(-GameController.Instance.CurrentBet);
        }

        public virtual void Fold()
        {
            Hand.Clear();
        }

        public virtual void DrawFromDeck()
        {
            DeckManager.SetFloorCard(DeckManager.DrawCard());
            GameController.Instance.UIController.UpdateFloorCard();
        }

        public virtual void PickAndSwap()
        {


            if (GameController.Instance.UIController.SwapCardIndex >= 0 && GameController.Instance.UIController.SwapCardIndex < Hand.Count)
            {
                Card floorCard = DeckManager.FloorCard;
                if (floorCard != null)
                {
                    DeckManager.AddToFloorCardList(Hand[GameController.Instance.UIController.SwapCardIndex]);
                    Hand[GameController.Instance.UIController.SwapCardIndex] = floorCard;
                    DeckManager.SetFloorCard(null);
                    GameController.Instance.UIController.UpdateGameState();
                }
            }
        }

        public virtual void Show()
        {
            ShowHand();
        }

        public virtual void BetOnBlind()
        {
            AdjustCoins(-GameController.Instance.CurrentBet);
            HasBetOnBlind = true;
        }

        public int CalculateHandValue()
        {
            return Hand.Sum(card => card.GetRankValue());
        }

        public int GetHighestCardValue()
        {
            return Hand.Max(card => card.GetRankValue());
        }

        public virtual void ShowHand()
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

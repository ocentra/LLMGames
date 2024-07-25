using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public class DevModeManager : MonoBehaviour
    {
        public static DevModeManager Instance { get; private set; }

        [System.Serializable]
        public class DevCard
        {
            [ValueDropdown("GetAvailableSuits")]
            public Suit Suit = Suit.None;

            [ValueDropdown("GetAvailableRanks")]
            public Rank Rank = Rank.None;

            [ShowInInspector, ReadOnly]
            public string CardText => GetCardText();

            private string GetCardText()
            {
                if (Suit == Suit.None || Rank == Rank.None)
                    return "Not Set";

                Card tempCard = ScriptableObject.CreateInstance<Card>();
                tempCard.Suit = Suit;
                tempCard.Rank = Rank;
                string text = tempCard.GetRankSymbol();
                Object.DestroyImmediate(tempCard);
                return text;
            }

            private static List<Suit> GetAvailableSuits()
            {
                return System.Enum.GetValues(typeof(Suit)).Cast<Suit>().ToList();
            }

            private List<Rank> GetAvailableRanks()
            {
                return System.Enum.GetValues(typeof(Rank)).Cast<Rank>().ToList();
            }

            [Button("Clear")]
            public void Clear()
            {
                Suit = Suit.None;
                Rank = Rank.None;
            }
        }

        [Header("Dev Hand")]
        [TableList(ShowIndexLabels = true)]
        [ValidateInput("ValidateDevHand", "Duplicate cards are not allowed in the dev hand.")]
        public DevCard[] DevHand = new DevCard[3] { new DevCard(), new DevCard(), new DevCard() };

        [Header("Dev Mode Settings")]
        [SerializeField] private bool isDevModeActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private bool ValidateDevHand(DevCard[] devHand)
        {
            var uniqueCards = new HashSet<string>();
            foreach (var card in devHand)
            {
                if (card.Suit != Suit.None && card.Rank != Rank.None)
                {
                    string cardId = $"{card.Suit}_{card.Rank}";
                    if (!uniqueCards.Add(cardId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public List<Card> GetDevHand()
        {
            if (!isDevModeActive)
                return null;

            List<Card> hand = new List<Card>();
            foreach (var devCard in DevHand)
            {
                if (devCard.Suit != Suit.None && devCard.Rank != Rank.None)
                {
                    Card card = Deck.Instance.CardTemplates.FirstOrDefault(c => c.Suit == devCard.Suit && c.Rank == devCard.Rank);
                    if (card != null)
                    {
                        hand.Add(card);
                    }
                }
            }

            return hand.Count == 3 ? hand : null;
        }

        public void ApplyDevHandToPlayer(Player player, DeckManager deckManager)
        {
            List<Card> devHand = GetDevHand();
            if (isDevModeActive && devHand != null && devHand.Count == 3 && player is HumanPlayer)
            {
                player.ResetForNewRound(deckManager, devHand);
                deckManager.RemoveCardsFromDeck(devHand);
                Debug.Log($"Applied dev hand to player: {string.Join(", ", devHand.ConvertAll(card => card.GetRankSymbol()))}");
            }
            else
            {
                player.ResetForNewRound(deckManager);
            }
        }

        [Button("Reset Dev Hand")]
        public void ResetDevHand()
        {
            foreach (var card in DevHand)
            {
                card.Clear();
            }
        }

        public bool IsDevModeActive() => isDevModeActive;

        [Button("Toggle Dev Mode")]
        public void ToggleDevMode()
        {
            isDevModeActive = !isDevModeActive;
        }
    }
}
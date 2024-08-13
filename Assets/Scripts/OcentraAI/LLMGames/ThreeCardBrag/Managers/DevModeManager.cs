using OcentraAI.LLMGames.GameModes;
using System.Collections.Generic;
using System.Linq;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using OcentraAI.LLMGames.GameModes.Rules;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public class DevModeManager : SerializedMonoBehaviour
    {
        public static DevModeManager Instance { get; private set; }


        [TableList(ShowIndexLabels = true)]
        [ValidateInput("ValidateDevHand", "Duplicate cards are not allowed in the dev hand.")]
        [OdinSerialize, ShowInInspector] public DevCard[] DevHand { get; set; } = new DevCard[3] { new DevCard(), new DevCard(), new DevCard() };

        [OdinSerialize, ShowInInspector] public DevCard[] DevHandComputer { get; set; } = new DevCard[3] { new DevCard(), new DevCard(), new DevCard() };


        [OdinSerialize, ShowInInspector] public DevCard TrumpDevCard { get; private set; } = new DevCard();

        [OdinSerialize, ShowInInspector] public bool IsDevModeActive { get; set; } = false;

        [OdinSerialize, ShowInInspector, Required] public GameMode GameMode { get; set; }

        [OdinSerialize] public bool UseTrumpForPlayer = false;
        [OdinSerialize] public bool UseTrumpForComputer = false;

        [OdinSerialize] public int SelectedPlayerRuleIndex;
        [OdinSerialize] public int SelectedComputerRuleIndex;

        [OdinSerialize] public List<Card> DeckCards { get; set; }

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

        private bool ValidateDevHand()
        {
            HashSet<string> uniqueCards = new HashSet<string>();
            foreach (DevCard card in DevHand)
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

        private List<Card> GetDevHand(DevCard[] devCards)
        {
            if (!IsDevModeActive)
                return null;

            List<Card> hand = new List<Card>();
            foreach (DevCard devCard in devCards)
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

        public bool TryGetDevHands(out Hand playerHuman, out Hand computer)
        {
            playerHuman = null;
            computer = null;
            if (IsDevModeActive)
            {
                playerHuman = new Hand(GetDevHand(DevHand));

                computer = new Hand(GetDevHand(DevHandComputer));
                return true;
            }

            return false;
        }

        [Button("Reset Dev Hand")]
        public void ResetDevHand()
        {
            foreach (DevCard card in DevHand)
            {
                card.Clear();
            }
        }

        [Button("Reset Dev Hand")]
        public void ResetDevComputerHand()
        {
            foreach (DevCard card in DevHandComputer)
            {
                card.Clear();
            }
        }


        public void ResetDeck()
        {
            DeckCards = new List<Card>(Deck.Instance.CardTemplates);
        }



        [Button("Toggle Dev Mode")]
        public void ToggleDevMode()
        {
            IsDevModeActive = !IsDevModeActive;
        }


    }
}


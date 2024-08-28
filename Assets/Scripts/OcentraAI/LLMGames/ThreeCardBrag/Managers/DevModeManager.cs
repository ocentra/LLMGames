using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

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

        public bool InitializeDevModeHands(PlayerManager playerManager, HumanPlayer humanPlayer, ComputerPlayer computerPlayer)
        {
            
            bool humanInitialized = false;
            bool compInitialized = false;

            if (TryGetDevHands(out Hand humanHand, out Hand computerHand))
            {
                if (humanHand != null)
                {
                    humanInitialized = playerManager.TryInitializePlayerHand(humanPlayer, humanHand);
                }

                if (computerHand != null)
                {
                    compInitialized = playerManager.TryInitializePlayerHand(computerPlayer, computerHand);
                }

                return humanInitialized || compInitialized;
            }

            return false;
        }

        public bool IsPlayerHandInitialized(Player player)
        {
            // Check if the player's hand has been initialized in dev mode
            if (player == null)
                return false;

            if (player is HumanPlayer)
                return DevHand != null && DevHand.All(card => card.Suit != Suit.None && card.Rank != Rank.None);

            if (player is ComputerPlayer)
                return DevHandComputer != null && DevHandComputer.All(card => card.Suit != Suit.None && card.Rank != Rank.None);

            return false;
        }





        private bool TryGetDevHands(out Hand playerHuman, out Hand computer)
        {
            playerHuman = null;
            computer = null;
            if (enabled)
            {
                playerHuman = new Hand(GetDevHand(DevHand));

                computer = new Hand(GetDevHand(DevHandComputer));
                return true;
            }

            return false;
        }

        public void ResetDevHand()
        {
            foreach (DevCard card in DevHand)
            {
                card.Clear();
            }
        }

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

        


        public void SetTrumpDevCard(Suit newSuit, Rank newRank)
        {
            TrumpDevCard = new DevCard(newSuit, newRank);
            EditorUtility.SetDirty(this);
        }
    }
}


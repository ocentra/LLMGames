
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.DevTools
{
    [CreateAssetMenu(fileName = nameof(DevModeManager), menuName = "DevTools/DevModeManager")]
    [GlobalConfig("Assets/Resources/")]
    public class DevModeManager : CustomGlobalConfig<DevModeManager>
    {
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [OdinSerialize][HideInInspector] public List<Card> DeckCards;

        [ReadOnly]
        [OdinSerialize]
        [ShowInInspector]
        public Card[] DevHand;

        [ReadOnly]
        [OdinSerialize]
        [ShowInInspector]
        public Card[] DevHandComputer;

        [OdinSerialize][HideInInspector] public bool DevModeEnabled = true;

        [OdinSerialize]
        [HideInInspector]
        [Required]
        public GameMode GameMode;

        [OdinSerialize][HideInInspector] public int SelectedComputerRuleIndex;

        [OdinSerialize][HideInInspector] public int SelectedPlayerRuleIndex;

        [ReadOnly]
        [OdinSerialize]
        [ShowInInspector]
        public Card TrumpDevCard;

        [OdinSerialize][HideInInspector] public bool UseTrumpForComputer;


        [OdinSerialize][HideInInspector] public bool UseTrumpForPlayer;

        private void OnEnable()
        {
            Init();
        }


        [Button("Initialize Dev Mode")]
        [ShowIf(nameof(ShouldShowInitializeButton))]
        private void Init()
        {
            if (DevHand == null || DevHand.Length == 0)
            {
                DevHand = new[]
                {
                    Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard()
                };
            }

            if (DevHandComputer == null || DevHandComputer.Length == 0)
            {
                DevHandComputer = new[]
                {
                    Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard()
                };
            }

            if (TrumpDevCard == null)
            {
                TrumpDevCard = Deck.Instance.GetRandomCard();
            }

            if (DeckCards == null || DeckCards.Count == 0)
            {
                ResetDeck();
            }

            if (GameMode == null)
            {
                GameMode = FindAnyGameMode();
            }

            SaveChanges();
        }

        private bool ShouldShowInitializeButton()
        {
            if (DevHand == null || DevHandComputer == null || GameMode == null)
            {
                return true;
            }

            if (DevHand.Any(card => card == null || card.Suit == Suit.None || card.Rank == Rank.None))
            {
                return true;
            }

            if (DevHandComputer.Any(card => card == null || card.Suit == Suit.None || card.Rank == Rank.None))
            {
                return true;
            }

            return false;
        }


        private GameMode FindAnyGameMode()
        {
            GameMode foundGameMode = Resources.FindObjectsOfTypeAll<GameMode>().FirstOrDefault();

            if (foundGameMode == null)
            {
                GameLoggerScriptable.LogWarning("No GameMode found. Please assign one.",this);
            }

            return foundGameMode;
        }


        private List<Card> GetDevHand(Card[] devCards)
        {
            List<Card> hand = new List<Card>();
            foreach (Card devCard in devCards)
            {
                if (devCard.Suit != Suit.None && devCard.Rank != Rank.None)
                {
                    Card card = Deck.Instance.GetCard(devCard.Suit, devCard.Rank);
                    if (card != null)
                    {
                        hand.Add(card);
                    }
                }
            }

            if (hand.Count < 3)
            {
                GameLoggerScriptable.LogError($"Dev hand has {hand.Count} cards, expected 3.", this);
                return null;
            }

            return hand;
        }


        //public bool InitializeDevModeHands(PlayerManager playerManager, DeckManager deckManager, HumanLLMPlayer humanLLMPlayer,
        //    ComputerLLMPlayer computerLLMPlayer)
        //{
        //    try
        //    {
        //        GameLoggerScriptable.Log("Initializing Dev Mode Hands...", this);
        //        bool humanInitialized = false;
        //        bool compInitialized = false;

        //        if (TryGetDevHands(out Hand humanHand, out Hand computerHand))
        //        {
        //            if (humanHand != null)
        //            {
        //                GameLoggerScriptable.Log("Initializing Human Player Hand in Dev Mode...", this);
        //                humanInitialized = playerManager.TryInitializePlayerHand(humanLLMPlayer, humanHand, deckManager);
        //            }

        //            if (computerHand != null)
        //            {
        //                GameLoggerScriptable.Log("Initializing Computer Player Hand in Dev Mode...", this);
        //                compInitialized = playerManager.TryInitializePlayerHand(computerLLMPlayer, computerHand, deckManager);
        //            }

        //            GameLoggerScriptable.Log(
        //                $"Initialization Complete: HumanInitialized={humanInitialized}, CompInitialized={compInitialized}", this);
        //            return humanInitialized || compInitialized;
        //        }

        //        GameLoggerScriptable.Log("Dev Hands could not be retrieved.", this);
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        GameLoggerScriptable.LogError($"Error in InitializeDevModeHands: {ex.Message}\nStackTrace: {ex.StackTrace}", this);
        //        return false;
        //    }
        //}


        public bool IsPlayerHandInitialized(IPlayerBase player)
        {
            // Check if the player's hand has been initialized in dev mode
            if (player == null)
            {
                return false;
            }

            if (player is IHumanPlayerData)
            {
                if (DevHand == null)
                {
                    return false;
                }

                foreach (var card in DevHand)
                {
                    if (card.Suit == Suit.None || card.Rank == Rank.None)
                    {
                        return false;
                    }
                }

                return true;
            }

            if (player is IComputerPlayerData)
            {
                if (DevHandComputer == null)
                {
                    return false;
                }

                foreach (var card in DevHandComputer)
                {
                    if (card.Suit == Suit.None || card.Rank == Rank.None)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }


        private bool TryGetDevHands(out Hand playerHuman, out Hand computer)
        {
            playerHuman = null;
            computer = null;

            if (DevModeEnabled)
            {
                List<Card> playerHandCards = GetDevHand(DevHand);
                List<Card> computerHandCards = GetDevHand(DevHandComputer);

                if (playerHandCards == null || playerHandCards.Count < 3)
                {
                    GameLoggerScriptable.LogError("Failed to retrieve a valid player hand in Dev Mode.", this);
                    return false;
                }

                if (computerHandCards == null || computerHandCards.Count < 3)
                {
                    GameLoggerScriptable.LogError("Failed to retrieve a valid computer hand in Dev Mode.", this);
                    return false;
                }

                playerHuman = new Hand(playerHandCards);
                computer = new Hand(computerHandCards);

                return true;
            }

            return false;
        }


        public void ResetDevHand()
        {
            for (int index = 0; index < DevHand.Length; index++)
            {
                DevHand[index] = Deck.Instance.GetRandomCard();
            }

            SaveChanges();
        }

        public void ResetDevComputerHand()
        {
            for (int index = 0; index < DevHandComputer.Length; index++)
            {
                DevHandComputer[index] = Deck.Instance.GetRandomCard();
            }

            SaveChanges();
        }


        public void ResetDeck()
        {
            DeckCards = new List<Card>(Deck.Instance.CardTemplates);
            SaveChanges();
        }

        public void SetTrumpDevCard()
        {
            TrumpDevCard = Deck.Instance.GetRandomCard();
            SaveChanges();
        }


        public void SetTrumpDevCard(Suit newSuit, Rank newRank)
        {
            TrumpDevCard = Deck.Instance.GetCard(newSuit, newRank);
            SaveChanges();
        }

      

    }
}


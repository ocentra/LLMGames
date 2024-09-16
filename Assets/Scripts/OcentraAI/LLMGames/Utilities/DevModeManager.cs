
#if UNITY_EDITOR


using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [CreateAssetMenu(fileName = nameof(DevModeManager), menuName = "ThreeCardBrag/DevModeManager")]
    [GlobalConfig("Assets/Resources/")]
    public class DevModeManager : CustomGlobalConfig<DevModeManager>
    {
        [ReadOnly, OdinSerialize, ShowInInspector]
        public Card[] DevHand;

        [ReadOnly, OdinSerialize, ShowInInspector]
        public Card[] DevHandComputer;

        [ReadOnly, OdinSerialize, ShowInInspector]
        public Card TrumpDevCard;

        [OdinSerialize, HideInInspector, Required]
        public GameMode GameMode;


        [OdinSerialize, HideInInspector] public bool UseTrumpForPlayer ;
        [OdinSerialize, HideInInspector] public bool UseTrumpForComputer ;

        [OdinSerialize, HideInInspector] public int SelectedPlayerRuleIndex;
        [OdinSerialize, HideInInspector] public int SelectedComputerRuleIndex;

        [OdinSerialize, HideInInspector] public List<Card> DeckCards;

        [OdinSerialize, HideInInspector]
        public bool DevModeEnabled  = true;

        private void OnEnable()
        {
            Init();
        }


        [Button("Initialize Dev Mode"), ShowIf(nameof(ShouldShowInitializeButton))]

        private void Init()
        {
            if (DevHand == null || DevHand.Length == 0)
            {
                DevHand = new Card[] { Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard() };
            }

            if (DevHandComputer == null || DevHandComputer.Length == 0)
            {
                DevHandComputer = new Card[] { Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard(), Deck.Instance.GetRandomCard() };
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
                Debug.LogWarning("No GameMode found. Please assign one.");
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
                LogError($"Dev hand has {hand.Count} cards, expected 3.", nameof(GetDevHand));
                return null;
            }

            return hand;
        }


        public bool InitializeDevModeHands(PlayerManager playerManager, HumanPlayer humanPlayer, ComputerPlayer computerPlayer)
        {
            try
            {
                Log("Initializing Dev Mode Hands...");
                bool humanInitialized = false;
                bool compInitialized = false;

                if (TryGetDevHands(out Hand humanHand, out Hand computerHand))
                {
                    if (humanHand != null)
                    {
                        Log("Initializing Human Player Hand in Dev Mode...");
                        humanInitialized = playerManager.TryInitializePlayerHand(humanPlayer, humanHand);
                    }

                    if (computerHand != null)
                    {
                        Log("Initializing Computer Player Hand in Dev Mode...");
                        compInitialized = playerManager.TryInitializePlayerHand(computerPlayer, computerHand);
                    }

                    Log($"Initialization Complete: HumanInitialized={humanInitialized}, CompInitialized={compInitialized}");
                    return humanInitialized || compInitialized;
                }

                Log("Dev Hands could not be retrieved.");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error in InitializeDevModeHands: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }


        public bool IsPlayerHandInitialized(Player player)
        {
            // Check if the player's hand has been initialized in dev mode
            if (player == null)
                return false;

            if (player is HumanPlayer)
            {
                if (DevHand == null)
                    return false;

                foreach (var card in DevHand)
                {
                    if (card.Suit == Suit.None || card.Rank == Rank.None)
                        return false;
                }

                return true;
            }

            if (player is ComputerPlayer)
            {
                if (DevHandComputer == null)
                    return false;

                foreach (var card in DevHandComputer)
                {
                    if (card.Suit == Suit.None || card.Rank == Rank.None)
                        return false;
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
                    LogError("Failed to retrieve a valid player hand in Dev Mode.", nameof(TryGetDevHands));
                    return false;
                }

                if (computerHandCards == null || computerHandCards.Count < 3)
                {
                    LogError("Failed to retrieve a valid computer hand in Dev Mode.", nameof(TryGetDevHands));
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

#endif



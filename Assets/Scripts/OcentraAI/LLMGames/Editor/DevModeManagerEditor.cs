#if UNITY_EDITOR


using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static System.String;

namespace OcentraAI.LLMGames.DevTools
{
    [CustomEditor(typeof(DevModeManager))]
    public class DevModeManagerEditor : OdinEditor
    {
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        public Dictionary<string, BaseBonusRule> RuleDictionary;
        public DevModeManager Manager => (DevModeManager)target;

        public int SelectedPlayerRuleIndex
        {
            get => Manager.SelectedPlayerRuleIndex;
            set => Manager.SelectedPlayerRuleIndex = value;
        }

        public int SelectedComputerRuleIndex
        {
            get => Manager.SelectedComputerRuleIndex;
            set => Manager.SelectedComputerRuleIndex = value;
        }

        #region Unity Editor Methods

        protected override void OnEnable()
        {
            PopulateRuleDictionary();

            // Remove cards in hand from the deck
            RemoveUsedCards(Manager.DevHand);
            RemoveUsedCards(Manager.DevHandComputer);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            bool newDevModeEnabled = EditorGUILayout.Toggle("Enable Dev Mode", Manager.DevModeEnabled);
            if (newDevModeEnabled != Manager.DevModeEnabled)
            {
                Manager.DevModeEnabled = GameSettings.Instance.DevModeEnabled = newDevModeEnabled;
                Manager.SaveChanges();
                GameSettings.Instance.SaveChanges();
            }

            Manager.GameMode =
                (GameMode)EditorGUILayout.ObjectField("Game Mode", Manager.GameMode, typeof(GameMode), false);

            if (Manager.GameMode == null)
            {
                EditorGUILayout.HelpBox("Please assign a GameMode to use bonus rules.", MessageType.Warning);
                return;
            }

            if (RuleDictionary == null || RuleDictionary.Count == 0)
            {
                PopulateRuleDictionary();
            }

            GUILayout.Space(10);

            if (RuleDictionary != null)
            {
                DrawPlayerHand();
                DrawComputerHand();
                DrawTrumpCard();
                DrawResetButtons();
            }

            if (EditorGUI.EndChangeCheck())
            {
                GameSettings.Instance.SaveChanges();
                Manager.SaveChanges();
            }
        }

        #endregion

        #region Hand Drawing Methods

        private void DrawPlayerHand()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player  ", EditorStyles.boldLabel);

            string[] ruleKeys = GetRuleKeys();
            SelectedPlayerRuleIndex = EditorGUILayout.Popup(SelectedPlayerRuleIndex, ruleKeys);

            bool canUseTrump = IsTrumpCardAvailable() && !IsUsingTrumpOfAKind(SelectedComputerRuleIndex);
            EditorGUI.BeginDisabledGroup(!canUseTrump);
            bool useTrumpForPlayer = GUILayout.Toggle(Manager.UseTrumpForPlayer, "Use Trump");
            EditorGUI.EndDisabledGroup();

            if (useTrumpForPlayer != Manager.UseTrumpForPlayer)
            {
                HandleTrumpSelection(useTrumpForPlayer, true);
            }

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                ApplySelectedRule(true, Manager.UseTrumpForPlayer);
            }

            GUILayout.EndHorizontal();

            ValidateDevHand(Manager.DevHand, Manager.DevHandComputer, out string playerErrorMessage);
            DrawDevHand(Manager.DevHand, playerErrorMessage);
        }

        private void DrawComputerHand()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Computer", EditorStyles.boldLabel);

            string[] ruleKeys = GetRuleKeys();
            SelectedComputerRuleIndex = EditorGUILayout.Popup(SelectedComputerRuleIndex, ruleKeys);

            bool canUseTrump = IsTrumpCardAvailable() && !IsUsingTrumpOfAKind(SelectedPlayerRuleIndex);
            EditorGUI.BeginDisabledGroup(!canUseTrump);
            bool useTrumpForComputer = GUILayout.Toggle(Manager.UseTrumpForComputer, "Use Trump");
            EditorGUI.EndDisabledGroup();

            if (useTrumpForComputer != Manager.UseTrumpForComputer)
            {
                HandleTrumpSelection(useTrumpForComputer, false);
            }

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                ApplySelectedRule(false, Manager.UseTrumpForComputer);
            }

            GUILayout.EndHorizontal();

            ValidateDevHand(Manager.DevHandComputer, Manager.DevHand, out string computerErrorMessage);
            DrawDevHand(Manager.DevHandComputer, computerErrorMessage);
        }

        private void DrawDevHand(Card[] hand, string errorMessage)
        {
            if (!IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }

            GUILayout.BeginHorizontal();
            foreach (Card devCard in hand)
            {
                GUILayout.BeginVertical();
                devCard.Suit = DrawCardSuitPopup(devCard.Suit);
                devCard.Rank = DrawCardRankPopup(devCard.Rank);
                GUILayout.Label(new GUIContent(devCard.RankSymbol), GetRichTextStyle());
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawTrumpCard()
        {
            GUILayout.Label("Trump", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (Manager.TrumpDevCard != null)
            {
                Suit newSuit = DrawCardSuitPopup(Manager.TrumpDevCard.Suit);
                Rank newRank = DrawCardRankPopup(Manager.TrumpDevCard.Rank);

                if (newSuit != Manager.TrumpDevCard.Suit || newRank != Manager.TrumpDevCard.Rank)
                {
                    Manager.SetTrumpDevCard(newSuit, newRank);
                }
            }

            if (Manager.TrumpDevCard != null)
            {
                GUILayout.Label(new GUIContent(Manager.TrumpDevCard.RankSymbol), GetRichTextStyle());
            }

            GUILayout.EndHorizontal();
        }


        private void DrawResetButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset Dev Hand", GUILayout.Width(150)))
            {
                Manager.ResetDevHand();
            }

            if (GUILayout.Button("Reset Computer Hand", GUILayout.Width(200)))
            {
                Manager.ResetDevComputerHand();
            }

            if (GUILayout.Button("Reset All", GUILayout.Width(200)))
            {
                ResetBothHandsAndDeck();
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Utility Methods

        private void PopulateRuleDictionary()
        {
            RuleDictionary = new Dictionary<string, BaseBonusRule>();
            if (Manager.GameMode != null && Manager.GameMode.BonusRules != null)
            {
                RuleDictionary.Add("None", null);
                foreach (BaseBonusRule rule in Manager.GameMode.BonusRules)
                {
                    RuleDictionary.Add(rule.RuleName, rule);
                }
            }
        }

        private void HandleTrumpSelection(bool useTrump, bool isPlayer)
        {
            // If turning off "Use Trump", always allow it
            if (!useTrump)
            {
                SetUseTrumpForHand(isPlayer, false);
                RegenerateHandIfNeeded(isPlayer);
                return;
            }

            // If turning on "Use Trump", check if it's allowed
            bool canUseTrump = isPlayer
                ? !IsUsingTrumpOfAKind(SelectedComputerRuleIndex)
                : !IsUsingTrumpOfAKind(SelectedPlayerRuleIndex);

            if (canUseTrump)
            {
                SetUseTrumpForHand(isPlayer, true);
                RegenerateHandIfNeeded(isPlayer);
            }
            else
            {
                GameLoggerScriptable.LogWarning($"Cannot enable 'Use Trump' for {(isPlayer ? "Player" : "Computer")}. TrumpOfAKind is already in use by the other hand.", null);
            }
        }

        private string[] GetRuleKeys()
        {
            string[] ruleKeys = new string[RuleDictionary.Count];
            int index = 0;
            foreach (var key in RuleDictionary.Keys)
            {
                ruleKeys[index++] = key;
            }

            return ruleKeys;
        }

        private bool IsTrumpCardAvailable()
        {
            return Manager.TrumpDevCard != null && Manager.TrumpDevCard.Rank != Rank.None &&
                   Manager.TrumpDevCard.Suit != Suit.None;
        }

        private static GUIStyle GetRichTextStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel) {richText = true};
        }

        private bool IsUsingTrumpOfAKind(int ruleIndex)
        {
            if (ruleIndex < 0 || ruleIndex >= RuleDictionary.Count)
            {
                return false;
            }

            string ruleName = RuleDictionary.Keys.ElementAt(ruleIndex);
            return RuleDictionary.TryGetValue(ruleName, out BaseBonusRule rule) &&
                   rule?.RuleName == nameof(TrumpOfAKind);
        }

        private void SetUseTrumpForHand(bool isPlayerHand, bool useTrump)
        {
            if (isPlayerHand)
            {
                Manager.UseTrumpForPlayer = useTrump;
            }
            else
            {
                Manager.UseTrumpForComputer = useTrump;
            }
        }

        #endregion

        #region Card Manipulation Methods

        private bool AreCardsAvailable(Card[] devCards, Card[] otherHand)
        {
            HashSet<string> usedCards = new HashSet<string>();
            foreach (Card card in otherHand)
            {
                usedCards.Add(card.Id);
            }

            foreach (Card devCard in devCards)
            {
                if (usedCards.Contains(devCard.Id))
                {
                    return false;
                }

                bool cardInDeck = false;
                for (int i = 0; i < Manager.DeckCards.Count; i++)
                {
                    if (Manager.DeckCards[i].Suit == devCard.Suit && Manager.DeckCards[i].Rank == devCard.Rank)
                    {
                        cardInDeck = true;
                        break;
                    }
                }

                if (!cardInDeck)
                {
                    return false;
                }

                usedCards.Add(devCard.Id);
            }

            return true;
        }

        private Rank DrawCardRankPopup(Rank currentRank)
        {
            List<Rank> standardRanks = Rank.GetStandardRanks();

            int currentIndex = -1;
            for (int i = 0; i < standardRanks.Count; i++)
            {
                if (standardRanks[i] == currentRank)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                currentIndex = 0;
            }

            string[] rankNames = new string[standardRanks.Count];
            for (int i = 0; i < standardRanks.Count; i++)
            {
                rankNames[i] = standardRanks[i].Name;
            }

            int newIndex = EditorGUILayout.Popup(currentIndex, rankNames);

            Rank selectedRank = standardRanks[newIndex];

            return selectedRank;
        }

        private Suit DrawCardSuitPopup(Suit currentSuit)
        {
            List<Suit> standardSuits = Suit.GetStandardSuits();

            int currentIndex = -1;
            for (int i = 0; i < standardSuits.Count; i++)
            {
                if (standardSuits[i] == currentSuit)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                currentIndex = 0;
            }

            string[] suitNames = new string[standardSuits.Count];
            for (int i = 0; i < standardSuits.Count; i++)
            {
                suitNames[i] = standardSuits[i].Name;
            }

            int newIndex = EditorGUILayout.Popup(currentIndex, suitNames);

            Suit selectedSuit = standardSuits[newIndex];


            return selectedSuit;
        }

        private void RegenerateHandIfNeeded(bool isPlayerHand)
        {
            Card[] currentHand = isPlayerHand ? Manager.DevHand : Manager.DevHandComputer;
            bool containsTrump = false;

            for (int i = 0; i < currentHand.Length; i++)
            {
                if (currentHand[i].Suit == Manager.TrumpDevCard.Suit &&
                    currentHand[i].Rank == Manager.TrumpDevCard.Rank)
                {
                    containsTrump = true;
                    break;
                }
            }

            if (containsTrump)
            {
                ApplySelectedRule(isPlayerHand);
            }
        }

        private void ApplySelectedRule(bool isPlayerHand, bool useTrump = false)
        {
            string selectedRuleName = null;
            int index = isPlayerHand ? SelectedPlayerRuleIndex : SelectedComputerRuleIndex;
            int i = 0;

            foreach (var key in RuleDictionary.Keys)
            {
                if (i == index)
                {
                    selectedRuleName = key;
                    break;
                }

                i++;
            }

            if (selectedRuleName != null &&
                RuleDictionary.TryGetValue(selectedRuleName, out BaseBonusRule selectedRule))
            {
                if (selectedRule == null)
                {
                    if (isPlayerHand)
                    {
                        Manager.ResetDevHand();
                    }
                    else
                    {
                        Manager.ResetDevComputerHand();
                    }
                }
                else
                {
                    if (selectedRule.RuleName == nameof(TrumpOfAKind))
                    {
                        // Check if the other hand is already using TrumpOfAKind
                        bool otherHandUsingTrumpOfAKind = isPlayerHand
                            ? IsUsingTrumpOfAKind(SelectedComputerRuleIndex)
                            : IsUsingTrumpOfAKind(SelectedPlayerRuleIndex);

                        if (otherHandUsingTrumpOfAKind)
                        {
                            GameLoggerScriptable.LogWarning($"Cannot apply TrumpOfAKind to {(isPlayerHand ? "Player" : "Computer")} hand. It's already in use by the other hand.", this);
                            return;
                        }

                        useTrump = true;
                        SetUseTrumpForHand(isPlayerHand, true);
                        GameLoggerScriptable.Log($"'Use Trump' has been automatically enabled for the TrumpOfAKind rule for {(isPlayerHand ? "Player" : "Computer")}.", this);
                    }

                    SetupHandForRule(selectedRule, isPlayerHand, useTrump);
                }
            }
        }

        private void SetupHandForRule(BaseBonusRule rule, bool isPlayerHand, bool useTrump = false)
        {
            string trumpCardText = useTrump ? Manager.TrumpDevCard.RankSymbol : null;

            Card[] otherHand = isPlayerHand ? Manager.DevHandComputer : Manager.DevHand;
            AddCardBackToDeck(isPlayerHand ? Manager.DevHand : Manager.DevHandComputer);

            Card[] devCards = null;
            bool validHand = false;
            int attempts = 0;
            const int maxAttempts = 20;
            string[] cardSymbols = {Empty};
            while (!validHand && attempts < maxAttempts)
            {
                cardSymbols = rule.CreateExampleHand(Manager.GameMode.NumberOfCards, trumpCardText, false);
                devCards = HandUtility.ConvertFromSymbols(cardSymbols).GetCards();
                validHand = AreCardsAvailable(devCards, otherHand);

                // Ensure the trump card is not in the hand if useTrump is false
                if (!useTrump && ContainsTrumpCard(devCards))
                {
                    validHand = false;
                }

                if (!validHand)
                {
                    attempts++;
                }
            }

            if (validHand && devCards != null && devCards.Length == Manager.GameMode.NumberOfCards)
            {
                RemoveUsedCards(devCards);
                AssignHand(isPlayerHand, devCards);
            }
            else
            {
                GameLoggerScriptable.LogWarning($"Failed to generate a valid hand after multiple attempts. cardSymbols {Join("", cardSymbols)} devCards {Join(", ", GetDevCardTexts(devCards))}", null);
            }
        }

        private bool ContainsTrumpCard(Card[] hand)
        {
            foreach (var card in hand)
            {
                if (card.Suit == Manager.TrumpDevCard.Suit && card.Rank == Manager.TrumpDevCard.Rank)
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveUsedCards(Card[] devCards)
        {
            if (devCards != null)
            {
                foreach (Card devCard in devCards)
                {
                    if (Manager.DeckCards != null)
                    {
                        for (int i = Manager.DeckCards.Count - 1; i >= 0; i--)
                        {
                            if (Manager.DeckCards[i].Suit == devCard.Suit && Manager.DeckCards[i].Rank == devCard.Rank)
                            {
                                Manager.DeckCards.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }

            // If deck is empty, reset it
            if (Manager.DeckCards is {Count: 0})
            {
                Manager.ResetDeck();
            }
        }

        private void AddCardBackToDeck(IEnumerable<Card> devCards)
        {
            foreach (Card devCard in devCards)
            {
                Card card = Deck.Instance.GetCard(devCard.Suit, devCard.Rank);

                if (card != null)
                {
                    bool cardExists = false;
                    foreach (Card deckCard in Manager.DeckCards)
                    {
                        if (deckCard.Suit == card.Suit && deckCard.Rank == card.Rank)
                        {
                            cardExists = true;
                            break;
                        }
                    }

                    if (!cardExists)
                    {
                        Manager.DeckCards.Add(card);
                    }
                }
            }
        }

        private void AssignHand(bool isPlayerHand, Card[] devCards)
        {
            if (devCards == null || devCards.Length != Manager.GameMode.NumberOfCards)
            {
                GameLoggerScriptable.LogError($"Invalid devCards array: {(devCards == null ? "null" : $"length {devCards.Length}")}", null);
                return;
            }

            if (isPlayerHand)
            {
                Manager.DevHand = devCards;
            }
            else
            {
                Manager.DevHandComputer = devCards;
            }
        }

        private bool ValidateDevHand(Card[] devHand, Card[] otherHand, out string errorMessage)
        {
            errorMessage = Empty;

            if (devHand == null)
            {
                errorMessage = "Dev hand is null.";
                return false;
            }

            if (devHand.Length != Manager.GameMode.NumberOfCards)
            {
                errorMessage =
                    $"Dev hand must contain exactly {Manager.GameMode.NumberOfCards} cards. Current count: {devHand.Length}";
                return false;
            }

            HashSet<string> uniqueCards = new HashSet<string>();
            for (int i = 0; i < devHand.Length; i++)
            {
                Card card = devHand[i];
                if (card == null)
                {
                    errorMessage = $"Card at index {i} is null.";
                    return false;
                }

                if (card.Suit == Suit.None || card.Rank == Rank.None)
                {
                    errorMessage = $"Card at index {i} has invalid Suit or Rank.";
                    return false;
                }

                if (!uniqueCards.Add(card.Id))
                {
                    errorMessage = $"Duplicate card found: {card.Id}";
                    return false;
                }

                bool cardInOtherHand = false;
                for (int j = 0; j < otherHand.Length; j++)
                {
                    if (otherHand[j].Id == card.Id)
                    {
                        cardInOtherHand = true;
                        break;
                    }
                }

                if (cardInOtherHand)
                {
                    errorMessage = $"Card {card.Id} is already in the other player's hand.";
                    return false;
                }
            }

            return true;
        }

        private string[] GetDevCardTexts(Card[] devCards)
        {
            if (devCards == null)
            {
                return Array.Empty<string>();
            }

            string[] texts = new string[devCards.Length];
            for (int i = 0; i < devCards.Length; i++)
            {
                texts[i] = devCards[i].RankSymbol;
            }

            return texts;
        }

        private void ResetBothHandsAndDeck()
        {
            Manager.ResetDevHand();
            Manager.ResetDevComputerHand();
            Manager.ResetDeck();
            Manager.SetTrumpDevCard();
        }

        #endregion
    }
}

#endif
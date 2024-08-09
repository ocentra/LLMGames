#if UNITY_EDITOR

using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static System.String;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [CustomEditor(typeof(DevModeManager))]
    public class DevModeManagerEditor : OdinEditor
    {
        public DevModeManager Manager => (DevModeManager)target;
        public Dictionary<string, BaseBonusRule> RuleDictionary;
        public int SelectedPlayerRuleIndex { get => Manager.SelectedPlayerRuleIndex; set => Manager.SelectedPlayerRuleIndex = value; }
        public int SelectedComputerRuleIndex { get => Manager.SelectedComputerRuleIndex; set => Manager.SelectedComputerRuleIndex = value; }


        protected override void OnEnable()
        {
            PopulateRuleDictionary();
            Manager.ResetDeck(); // Initialize DeckCards when the editor is enabled

            // Remove cards in hand from the deck
            RemoveUsedCards(Manager.DevHand);
            RemoveUsedCards(Manager.DevHandComputer);
        }

        public override void OnInspectorGUI()
        {
            Manager.GameMode = (GameMode)EditorGUILayout.ObjectField("Game Mode", Manager.GameMode, typeof(GameMode), false);

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

            GUIStyle combinedStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true,
                fontSize = 25
            };

            bool trumpCardAvailable = Manager.TrumpDevCard.Rank != Rank.None && Manager.TrumpDevCard.Suit != Suit.None;

            if (RuleDictionary != null)
            {
                // Player Hand Section
                GUILayout.BeginHorizontal();
                GUILayout.Label("Player  ", EditorStyles.boldLabel, GUILayout.Width(100));
                SelectedPlayerRuleIndex = EditorGUILayout.Popup(SelectedPlayerRuleIndex, RuleDictionary.Keys.ToArray(), GUILayout.Width(300));

                // Use Trump checkbox for player
                EditorGUI.BeginDisabledGroup(!trumpCardAvailable);
                bool useTrumpForPlayer = GUILayout.Toggle(Manager.UseTrumpForPlayer, "Use Trump");
                EditorGUI.EndDisabledGroup();

                if (useTrumpForPlayer != Manager.UseTrumpForPlayer)
                {
                    HandleTrumpSelection(useTrumpForPlayer, true);
                }

                if (GUILayout.Button("Apply", GUILayout.Width(100)))
                {
                    ApplySelectedRule(true, Manager.UseTrumpForPlayer); // true indicates the player's hand
                }

                GUILayout.EndHorizontal();
                DrawDevHand(Manager.DevHand, combinedStyle);

                // Computer Hand Section
                GUILayout.BeginHorizontal();
                GUILayout.Label("Computer", EditorStyles.boldLabel, GUILayout.Width(100));
                SelectedComputerRuleIndex = EditorGUILayout.Popup(SelectedComputerRuleIndex, RuleDictionary.Keys.ToArray(), GUILayout.Width(300));

                // Use Trump checkbox for computer
                EditorGUI.BeginDisabledGroup(!trumpCardAvailable);
                bool useTrumpForComputer = GUILayout.Toggle(Manager.UseTrumpForComputer, "Use Trump");
                EditorGUI.EndDisabledGroup();

                if (useTrumpForComputer != Manager.UseTrumpForComputer)
                {
                    HandleTrumpSelection(useTrumpForComputer, false);
                }

                if (GUILayout.Button("Apply", GUILayout.Width(100)))
                {
                    ApplySelectedRule(false, Manager.UseTrumpForComputer); // false indicates the computer's hand
                }
                GUILayout.EndHorizontal();
            }

            DrawDevHand(Manager.DevHandComputer, combinedStyle);

            GUILayout.Label("Trump", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            Manager.TrumpDevCard.Suit = (Suit)EditorGUILayout.EnumPopup(Manager.TrumpDevCard.Suit, GUILayout.Width(100));
            Manager.TrumpDevCard.Rank = (Rank)EditorGUILayout.EnumPopup(Manager.TrumpDevCard.Rank, GUILayout.Width(50));
            GUILayout.Label(new GUIContent(Manager.TrumpDevCard.CardText), combinedStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();


            Manager.IsDevModeActive = GUILayout.Toggle(Manager.IsDevModeActive, "Dev Mode Active", GUILayout.Width(150));


            if (GUILayout.Button("Reset Dev Hand", GUILayout.Width(150)))
            {
                Manager.ResetDevHand();
                Manager.ResetDeck();
            }

            if (GUILayout.Button("Reset Computer Hand", GUILayout.Width(200)))
            {
                Manager.ResetDevComputerHand();
                Manager.ResetDeck();
            }

            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(Manager);
            }
        }

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
            if (useTrump)
            {
                // If one is selected, the other must be deselected
                if (isPlayer)
                {
                    Manager.UseTrumpForPlayer = true;
                    Manager.UseTrumpForComputer = false;
                }
                else
                {
                    Manager.UseTrumpForComputer = true;
                    Manager.UseTrumpForPlayer = false;
                }
            }
            else
            {
                // If unchecking, re-add the trump card to the deck

                if (isPlayer)
                {
                    Manager.UseTrumpForPlayer = false;
                }
                else
                {
                    Manager.UseTrumpForComputer = false;
                }

                RegenerateHandIfNeeded(isPlayer);

            }

        }

        private void RegenerateHandIfNeeded(bool isPlayerHand)
        {
            DevCard[] currentHand = isPlayerHand ? Manager.DevHand : Manager.DevHandComputer;
            bool containsTrump = currentHand.Any(card => card.Suit == Manager.TrumpDevCard.Suit && card.Rank == Manager.TrumpDevCard.Rank);

            if (containsTrump)
            {
                ApplySelectedRule(isPlayerHand);
            }
        }

        private void ApplySelectedRule(bool isPlayerHand, bool useTrump = false)
        {
            string selectedRuleName = isPlayerHand ? RuleDictionary.Keys.ElementAt(SelectedPlayerRuleIndex) : RuleDictionary.Keys.ElementAt(SelectedComputerRuleIndex);

            if (RuleDictionary.TryGetValue(selectedRuleName, out BaseBonusRule selectedRule))
            {
                if (selectedRule == null)
                {
                    if (isPlayerHand)
                        Manager.ResetDevHand();
                    else
                        Manager.ResetDevComputerHand();
                }
                else
                {
                    SetupHandForRule(selectedRule, isPlayerHand, useTrump);
                }
            }
        }

        private void SetupHandForRule(BaseBonusRule rule, bool isPlayerHand, bool useTrump = false)
        {
            string trumpCardText = useTrump ? Manager.TrumpDevCard.GetCardText(false) : null;

            AddCardBackToDeck(isPlayerHand ? Manager.DevHand : Manager.DevHandComputer);

            // Attempt to create a valid hand that uses cards available in the deck
            DevCard[] devCards = null;
            bool validHand = false;
            int attempts = 0;
            const int maxAttempts = 10;
            string[] cardSymbols = new[] { Empty };
            while (!validHand && attempts < maxAttempts)
            {
                cardSymbols = rule.CreateExampleHand(Manager.GameMode.NumberOfCards, trumpCardText, false);
                devCards = DevCard.ConvertToDevCardFromSymbols(cardSymbols);

                // Check if all the cards in the generated hand are available in the deck
                validHand = AreCardsAvailable(devCards);
                if (!validHand)
                {
                    attempts++;
                }
            }

            if (validHand && devCards != null)
            {
                RemoveUsedCards(devCards);
                AssignHand(isPlayerHand, devCards);
                EditorUtility.SetDirty(Manager);
            }
            else
            {
                Debug.LogWarning($"Failed to generate a valid hand after multiple attempts. cardSymbols {Join("", cardSymbols)} devCards  {devCards?.Select(c => c.CardText)}");
            }
        }

        private bool AreCardsAvailable(DevCard[] devCards)
        {
            foreach (DevCard devCard in devCards)
            {
                if (!Manager.DeckCards.Any(card => card.Suit == devCard.Suit && card.Rank == devCard.Rank))
                {
                    return false;
                }
            }
            return true;
        }

        private void RemoveUsedCards(DevCard[] devCards)
        {
            foreach (DevCard devCard in devCards)
            {
                Card cardToRemove = Manager.DeckCards.FirstOrDefault(card => card.Suit == devCard.Suit && card.Rank == devCard.Rank);
                if (cardToRemove != null)
                {
                    Manager.DeckCards.Remove(cardToRemove);
                }
            }

            // If deck is empty, reset it
            if (Manager.DeckCards.Count == 0)
            {
                Manager.ResetDeck();
            }
        }

        private void AddCardBackToDeck(IEnumerable<DevCard> devCards)
        {
            foreach (DevCard devCard in devCards)
            {
                Card card = Deck.Instance.GetCard(devCard.Suit, devCard.Rank);

                if (card != null && !Manager.DeckCards.Contains(card))
                {
                    Manager.DeckCards.Add(card);
                }
            }
        }
        
        private void AssignHand(bool isPlayerHand, DevCard[] devCards)
        {
            if (isPlayerHand)
            {
                Manager.DevHand = devCards;
            }
            else
            {
                Manager.DevHandComputer = devCards;
            }
        }

        private void DrawDevHand(DevCard[] hand, GUIStyle combinedStyle)
        {
            GUILayout.BeginHorizontal();
            foreach (DevCard devCard in hand)
            {
                GUILayout.BeginHorizontal();
                devCard.Suit = (Suit)EditorGUILayout.EnumPopup(devCard.Suit, GUILayout.Width(100));
                devCard.Rank = (Rank)EditorGUILayout.EnumPopup(devCard.Rank, GUILayout.Width(50));
                GUILayout.Label(new GUIContent(devCard.CardText), combinedStyle);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif

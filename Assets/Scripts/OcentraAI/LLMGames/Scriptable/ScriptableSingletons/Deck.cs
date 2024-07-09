using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using OcentraAI.LLMGames.Utilities;
using UnityEditor;
using UnityEngine;
using System;

namespace OcentraAI.LLMGames.Scriptable.ScriptableSingletons
{
    [CreateAssetMenu(fileName = nameof(Deck), menuName = "ThreeCardBrag/Deck")]
    [CustomGlobalConfig("Assets/Resources/")]
    public class Deck : CustomGlobalConfig<Deck>
    {
        [ShowInInspector]
        public List<Card> CardTemplates = new List<Card>();

        [ShowInInspector, Required]
        public Card BackCard;

        [Button]
        private void LoadCardsFromResources()
        {
            var allCards = Resources.LoadAll<Card>("Cards").ToList();

            // Filter out the BackCard
            BackCard = allCards.FirstOrDefault(card => card.name == nameof(BackCard));
            CardTemplates = allCards.Where(card => card.name != nameof(BackCard)).ToList();

            if (CardTemplates.Count == 0)
            {
                Debug.LogError("No card assets found in Resources/Cards. Please ensure card assets are in the correct location.");
                CreateAllCards();
            }
            else if (ValidateDeck() == false)
            {
                CreateMissingCards();
            }

            SaveChanges();
        }

        [Button]
        private bool ValidateDeck()
        {
            if (CardTemplates.Count != 52) 
            {
                Debug.LogError($"Invalid number of cards in the deck. Expected 52, but found {CardTemplates.Count}");
                return false;
            }


            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (!CardTemplates.Any(card => card.Suit == suit && card.Rank == rank))
                    {
                        Debug.LogError($"Missing card: {rank} of {suit}");
                        return false;
                    }
                }
            }


            var cardGroups = CardTemplates.GroupBy(card => new { suit = card.Suit, rank = card.Rank });
            foreach (var group in cardGroups)
            {
                if (group.Count() > 1)
                {
                    Debug.LogError($"Duplicate card found: {group.Key.rank} of {group.Key.suit}");
                    return false;
                }
            }

            return true;
        }

        private void CreateMissingCards()
        {

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    Card existingCard = CardTemplates.FirstOrDefault(card => card.Suit == suit && card.Rank == rank);
                    if (existingCard == null)
                    {
                        CreateCard(suit, rank);
                    }
                    else if (existingCard.Sprite == null)
                    {
                        existingCard.AssignSprite();
                    }
                }
            }


            SaveChanges();
        }

        private void CreateAllCards()
        {

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    CreateCard(suit, rank);
                }
            }

            SaveChanges();
        }


        private void CreateCard(Suit suit, Rank rank)
        {
            Card newCard = CreateInstance<Card>();
            newCard.Init(suit, rank);
            string path = $"Assets/Resources/Cards/{rank.ToString()}_of_{suit.ToString()}.asset";
            AssetDatabase.CreateAsset(newCard, path);
            CardTemplates.Add(newCard);
            Debug.Log($"Created card: {rank.ToString()} of {suit.ToString()} at {path}");
        }

        private void SaveChanges()
        {
#if UNITY_EDITOR

            EditorUtility.SetDirty(this);
            if (BackCard != null)
            {
                EditorUtility.SetDirty(BackCard);
            }
            foreach (var card in CardTemplates)
            {
                EditorUtility.SetDirty(card);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}

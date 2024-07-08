using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using ThreeCardBrag.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThreeCardBrag.ScriptableSingletons
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
            if (CardTemplates.Count != 52) // 52 cards in a standard deck
            {
                Debug.LogError($"Invalid number of cards in the deck. Expected 52, but found {CardTemplates.Count}");
                return false;
            }

            string[] expectedSuits = new[] { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] expectedRanks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (string suit in expectedSuits)
            {
                foreach (string rank in expectedRanks)
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
            string[] suits = new[] { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] ranks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (string suit in suits)
            {
                foreach (string rank in ranks)
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
            string[] suits = new[] { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] ranks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (string suit in suits)
            {
                foreach (string rank in ranks)
                {
                    CreateCard(suit, rank);
                }
            }

            SaveChanges();
        }

        private void CreateCard(string suit, string rank)
        {
            Card newCard = CreateInstance<Card>();
            newCard.Init(suit, rank);
            string path = $"Assets/Resources/Cards/{rank}_of_{suit}.asset";
            AssetDatabase.CreateAsset(newCard, path);
            CardTemplates.Add(newCard);
            Debug.Log($"Created card: {rank} of {suit} at {path}");
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

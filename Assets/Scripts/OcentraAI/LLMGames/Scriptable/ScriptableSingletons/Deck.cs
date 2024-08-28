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
            List<Card> allCards = Resources.LoadAll<Card>("Cards").ToList();

            // Assign or create the BackCard
            BackCard = allCards.FirstOrDefault(card => card.name == nameof(BackCard));
            if (BackCard == null)
            {
                CreateBackCard();
            }

            CardTemplates = allCards.Where(card => card.name != nameof(BackCard)).ToList();

            if (CardTemplates.Count == 0)
            {
               // Debug.Log("No card assets found in Resources/Cards. Creating New Assets.");
                CreateAllCards();
            }
            else if (ValidateStandardDeck() == false)
            {
                CreateMissingCards();
            }

            SaveChanges();
        }

        private void CreateBackCard()
        {
            BackCard = CreateInstance<Card>();
            BackCard.name = nameof(BackCard);
            BackCard.Init(Suit.None, new Rank(0, nameof(BackCard), "nameof(BackCard)"));
            string path = $"Assets/Resources/Cards/{nameof(BackCard)}.asset";
            AssetDatabase.CreateAsset(BackCard, path);
            Debug.Log($"Created BackCard at {path}");
        }

        [Button]
        private bool ValidateStandardDeck()
        {
            if (CardTemplates.Count != 52)
            {
                Debug.LogError($"Invalid number of cards in the deck. Expected 52, but found {CardTemplates.Count}");
                return false;
            }

            foreach (Suit suit in Suit.GetStandardSuits())
            {
                if (suit == Suit.None)
                {
                    continue;
                }

                foreach (Rank rank in Rank.GetStandardRanks())
                {
                    if (rank == Rank.None)
                    {
                        continue;
                    }

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
            foreach (Suit suit in Suit.GetStandardSuits())
            {
                if (suit == Suit.None)
                {
                    continue;
                }

                foreach (Rank rank in Rank.GetStandardRanks())
                {
                    if (rank == Rank.None)
                    {
                        continue;
                    }
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
            foreach (Suit suit in Suit.GetStandardSuits())
            {
                if (suit == Suit.None)
                {
                    continue;
                }

                foreach (Rank rank in Rank.GetStandardRanks())
                {
                    if (rank == Rank.None)
                    {
                        continue;
                    }
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
            //Debug.Log($"Created card: {rank.Name} of {suit.ToString()} at {path}");
        }

        private void SaveChanges()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            if (BackCard != null)
            {
                EditorUtility.SetDirty(BackCard);
            }
            foreach (Card card in CardTemplates)
            {
                EditorUtility.SetDirty(card);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public Card GetCard(Suit suit, Rank rank)
        {
            return CardTemplates.FirstOrDefault(card =>
            {
                if (card.Suit == suit && card.Rank == rank) return true;
                return false;
            });
        }
    }
}

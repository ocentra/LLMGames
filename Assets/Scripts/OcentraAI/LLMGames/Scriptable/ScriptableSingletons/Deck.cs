using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable.ScriptableSingletons
{
    [CreateAssetMenu(fileName = nameof(Deck), menuName = "ThreeCardBrag/Deck")]
    [GlobalConfig("Assets/Resources/")]
    public class Deck : CustomGlobalConfig<Deck>
    {
        [ShowInInspector]
        public List<Card> CardTemplates = new List<Card>();

        [ShowInInspector, Required]
        public Card BackCard;

        [ShowInInspector, Required]
        public Card NullCard;

#if UNITY_EDITOR

        private List<Card> remainingCards = new List<Card>();
        private List<Card> drawnCards = new List<Card>();


        [Button]
        private void LoadCardsFromResources()
        {
            List<Card> allCards = Resources.LoadAll<Card>("Cards").ToList();

            // Assign or create the BackCard
            NullCard = allCards.FirstOrDefault(card => card.name == nameof(NullCard));
            if (NullCard == null)
            {
                CreateNullCard();
            }

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
            BackCard.Init(Suit.None, new Rank(0, nameof(BackCard), nameof(BackCard)));
            string path = $"Assets/Resources/Cards/{nameof(BackCard)}.asset";
            UnityEditor.AssetDatabase.CreateAsset(BackCard, path);
            Debug.Log($"Created BackCard at {path}");
        }


        private void CreateNullCard()
        {
            NullCard = CreateInstance<Card>();
            NullCard.name = nameof(NullCard);
            NullCard.Init(Suit.None, new Rank(0, nameof(NullCard), nameof(NullCard)));
            string path = $"Assets/Resources/Cards/{nameof(NullCard)}.asset";
            UnityEditor.AssetDatabase.CreateAsset(NullCard, path);
            Debug.Log($"Created NullCard at {path}");
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
            UnityEditor.AssetDatabase.CreateAsset(newCard, path);
            CardTemplates.Add(newCard);
            //Debug.Log($"Created card: {rank.Name} of {suit.ToString()} at {path}");
        }

        [Button]
        public Card GetRandomCard()
        {
            if (remainingCards.Count == 0)
            {
                ResetDeck();
            }

            int randomIndex = UnityEngine.Random.Range(0, remainingCards.Count);
            Card randomCard = remainingCards[randomIndex];

            remainingCards.RemoveAt(randomIndex);
            drawnCards.Add(randomCard);

            return randomCard;
        }

        private void ResetDeck()
        {
            remainingCards = new List<Card>(CardTemplates);
            drawnCards.Clear();

            ShuffleDeck(remainingCards);
        }

        private void ShuffleDeck(List<Card> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }
        }








#endif


        public override void SaveChanges()
        {
#if UNITY_EDITOR

            if (BackCard != null)
            {
                UnityEditor.EditorUtility.SetDirty(BackCard);
            }
            foreach (Card card in CardTemplates)
            {
                UnityEditor.EditorUtility.SetDirty(card);
            }

            base.SaveChanges();
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




using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable
{
    [CreateAssetMenu(fileName = nameof(Card), menuName = "ThreeCardBrag/Card")]
    public class Card : ScriptableObject
    {
        public Suit Suit;

        public Rank Rank;

        [Required] public Sprite Sprite;

        public int GetRankValue()
        {
            return (int)Rank;
        }

        public string GetColor()
        {
            return Suit switch
            {
                Suit.Hearts => "Red",
                Suit.Diamonds => "Red",
                Suit.Clubs => "Black",
                Suit.Spades => "Black",

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void Init(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
            AssignSprite();
            SaveChanges();
        }

        [Button, ShowIf("@this.Sprite == null")]
        public void AssignSprite()
        {
            string formattedRank = Rank switch
            {
                Rank.A => "Ace",
                Rank.K => "King",
                Rank.Q => "Queen",
                Rank.J => "Jack",
                _ => ((int)Rank).ToString()
            };

            string path = name == "BackCard" ? $"Assets/Images/Cards/BackCard.png" : $"Assets/Images/Cards/{formattedRank}_of_{Suit}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                Sprite = sprite;
                Debug.Log($"Assigned sprite from {path}");
                SaveChanges();
            }
            else
            {
                Debug.LogWarning($"Sprite not found at {path}");
            }
        }

        private void SaveChanges()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
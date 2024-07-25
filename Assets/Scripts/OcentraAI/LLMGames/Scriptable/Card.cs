using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;
using static OcentraAI.LLMGames.Utility;

namespace OcentraAI.LLMGames.Scriptable
{
    [CreateAssetMenu(fileName = nameof(Card), menuName = "ThreeCardBrag/Card")]
    public class Card : ScriptableObject
    {
        public Suit Suit;

        public Rank Rank;

        [Required] public Sprite Sprite;

        public string Id => $"{Suit}_{Rank}";

        public int GetRankValue()
        {
            return (int)Rank;
        }

        public string GetColorString()
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

        public Color GetColorValue()
        {
            return Suit switch
            {
                Suit.Hearts => Color.red,
                Suit.Diamonds => Color.red,
                Suit.Clubs => Color.black,
                Suit.Spades => Color.black,

                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public string GetRankSymbol()
        {
            string symbol = Suit switch
            {
                Suit.Clubs => ColouredMessage($"♣", Color.black),
                Suit.Diamonds => ColouredMessage($"♦", Color.red),
                Suit.Hearts => ColouredMessage($"♥", Color.red),
                Suit.Spades => ColouredMessage($"♠", Color.black),
                _ => ""
            };

            string formattedRank = Rank switch
            {
                Rank.A => "A",
                Rank.K => "K",
                Rank.Q => "Q",
                Rank.J => "J",
                _ => ((int)Rank).ToString()
            };

            return $"{ColouredMessage($"{formattedRank}", GetColorValue())}{symbol}";
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
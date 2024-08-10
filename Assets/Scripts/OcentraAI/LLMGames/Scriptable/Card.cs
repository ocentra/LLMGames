using OcentraAI.LLMGames.GameModes.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using static OcentraAI.LLMGames.Utilities.CardUtility;

namespace OcentraAI.LLMGames.Scriptable
{
    [CreateAssetMenu(fileName = nameof(Card), menuName = "ThreeCardBrag/Card")]
    public class Card : ScriptableObject
    {
        public Suit Suit;
        public Rank Rank;

        [Required]
        public Sprite Sprite;

        [ShowInInspector]
        public string RankSymbol => GetRankSymbol(Suit, Rank);

        public string Id => $"{Suit}_{Rank}";

        public int GetRankValue()
        {
            return (int)Rank;
        }

        public void Init(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
            AssignSprite();
            this.SaveChanges();
        }

        [Button, ShowIf("@this.Sprite == null")]
        public void AssignSprite()
        {
            string formattedRank = Rank switch
            {
                Rank.A => "A",
                Rank.K => "K",
                Rank.Q => "Q",
                Rank.J => "J",
                _ => ((int)Rank).ToString()
            };

            string path = name == "BackCard" ? $"Assets/Images/Cards/BackCard.png" : $"Assets/Images/Cards/{formattedRank}_of_{Suit}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                Sprite = sprite;
                Debug.Log($"Assigned sprite from {path}");
                this.SaveChanges();
            }
            else
            {
                Debug.LogWarning($"Sprite not found at {path}");
            }
        }

  
    }
}
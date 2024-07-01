using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ThreeCardBrag
{
    [CreateAssetMenu(fileName = nameof(Card), menuName = "ThreeCardBrag/Card")]
    public class Card : ScriptableObject
    {
        [ShowInInspector]
        public string Suit { get; private set; }

        [ShowInInspector]
        public string Rank { get; private set; }

        [ShowInInspector, Required]
        public Sprite Sprite { get; private set; }

        public int GetRankValue()
        {
            return Rank switch
            {
                "A" => 14,
                "K" => 13,
                "Q" => 12,
                "J" => 11,
                _ => int.TryParse(Rank, out int result) ? result : 0
            };
        }

        public void Init(string suit, string rank)
        {
            Suit = suit;
            Rank = rank;
            AssignSprite();
        }

        [Button, ShowIf("@this.Sprite == null")]
        public void AssignSprite()
        {
            string formattedRank = Rank switch
            {
                "A" => "Ace",
                "K" => "King",
                "Q" => "Queen",
                "J" => "Jack",
                _ => Rank
            };

            string path = name == "BackCard" ? $"Assets/Images/Cards/BackCard.png" :$"Assets/Images/Cards/{formattedRank}_of_{Suit}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                Sprite = sprite;
                Debug.Log($"Assigned sprite from {path}");
            }
            else
            {
                Debug.LogWarning($"Sprite not found at {path}");
            }
        }
    }
}
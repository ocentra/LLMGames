using Sirenix.OdinInspector;
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

        [ShowInInspector] public string RankSymbol => GetRankSymbol(Suit, Rank);

        public string Id => $"{Suit}_{Rank}";

        public int GetRankValue()
        {
            return (int)Rank;
        }

        public string GetColorString()
        {
            if (Suit is Suit.Hearts or Suit.Diamonds)
            {
                return "Red";
            }

            return "Black";
        }

        public static Color GetColorValue(Suit suit)
        {
            if (suit is Suit.Hearts or Suit.Diamonds)
            {
                return Color.red;
            }

            return Color.black;
        }


        public static string GetRankSymbol(Suit suit, Rank rank)
        {
            if (suit == Suit.None || rank == Rank.None)
            {
                return "None";
            }
            string symbol;
            if (suit == Suit.Hearts)
            {
                symbol = ColouredMessage($"♥", Color.red);
            }
            else if (suit == Suit.Diamonds)
            {
                symbol = ColouredMessage($"♦", Color.red);
            }
            else if (suit == Suit.Clubs)
            {
                symbol = ColouredMessage($"♣", Color.black);
            }
            else if (suit == Suit.Spades)
            {
                symbol = ColouredMessage($"♠", Color.black);
            }
            else
            {
                symbol = "";
            }

            string formattedRank;
            if (rank == Rank.A)
            {
                formattedRank = "A";
            }
            else if (rank == Rank.K)
            {
                formattedRank = "K";
            }
            else if (rank == Rank.Q)
            {
                formattedRank = "Q";
            }
            else if (rank == Rank.J)
            {
                formattedRank = "J";
            }
            else
            {
                formattedRank = ((int)rank).ToString();
            }
            string rankSymbol = $"{ColouredMessage($"{formattedRank}", GetColorValue(suit))}{symbol}";

            //Debug.Log($"GetRankSymbol {rankSymbol} Rank {Rank.ToString()} Suit {Suit} ");
            return rankSymbol; 
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
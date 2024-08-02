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

        public static Rank GetRankFromChar(char rankChar)
        {
            return rankChar switch
            {
                '2' => Rank.Two,
                '3' => Rank.Three,
                '4' => Rank.Four,
                '5' => Rank.Five,
                '6' => Rank.Six,
                '7' => Rank.Seven,
                '8' => Rank.Eight,
                '9' => Rank.Nine,
                'J' => Rank.J,
                'Q' => Rank.Q,
                'K' => Rank.K,
                'A' => Rank.A,
                _ => Rank.None // Default to None for invalid ranks
            };
        }

        public static Suit GetSuitFromChar(char suitChar)
        {
            return suitChar switch
            {
                '♠' => Suit.Spades,
                '♥' => Suit.Hearts,
                '♦' => Suit.Diamonds,
                '♣' => Suit.Clubs,
                _ => Suit.None // Default to None for invalid suits
            };
        }


        public static string GetRankSymbol(Suit suit, Rank rank, bool coloured = true)
        {
            if (suit == Suit.None || rank == Rank.None)
            {
                return "None";
            }

            string symbol;
            if (suit == Suit.Hearts)
            {
                symbol = coloured ? ColouredMessage("♥", Color.red) : "♥";
            }
            else if (suit == Suit.Diamonds)
            {
                symbol = coloured ? ColouredMessage("♦", Color.red) : "♦";
            }
            else if (suit == Suit.Clubs)
            {
                symbol = coloured ? ColouredMessage("♣", Color.black) : "♣";
            }
            else if (suit == Suit.Spades)
            {
                symbol = coloured ? ColouredMessage("♠", Color.black) : "♠";
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

            string rankSymbol = coloured ? $"{ColouredMessage($"{formattedRank}", GetColorValue(suit))}{symbol}" : $"{formattedRank}{symbol}";

            // Debug.Log($"GetRankSymbol {rankSymbol} Rank {Rank.ToString()} Suit {Suit} ");
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
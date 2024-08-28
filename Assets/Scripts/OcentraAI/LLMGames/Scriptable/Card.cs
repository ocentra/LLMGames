using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable
{
    [CreateAssetMenu(fileName = nameof(Card), menuName = "ThreeCardBrag/Card")]
    public class Card : SerializedScriptableObject, IComparable<Card>
    {
        // Backing fields for serialized properties
        [SerializeField] private Suit suit;
        [SerializeField] private Rank rank;
        [SerializeField] private Sprite sprite;
        [SerializeField, ReadOnly] private string path;
        [SerializeField, ReadOnly] private string rankSymbol;
        [SerializeField, ReadOnly] private string id;

        // Properties for accessing the fields
        public Suit Suit
        {
            get => suit;
            set => suit = value;
        }

        public Rank Rank
        {
            get => rank;
            set
            {
                rank = value;
                id = $"{rank.Alias}_{suit.Symbol}";
                rankSymbol = CardUtility.GetRankSymbol(suit, rank);
                path = rank.Name == "BackCard" ? $"Assets/Images/Cards/BackCard.png" : $"Assets/Images/Cards/{rank.Alias}_of_{suit.Name.ToLower() + "s"}.png";
            }
        }

        public Sprite Sprite
        {
            get => sprite;
            set => sprite = value;
        }

        [ReadOnly]
        public string Path => path;

        [ReadOnly]
        public string RankSymbol => rankSymbol;

        [ReadOnly]
        public string Id => id;

        // Constructor
        public void Init(Suit s, Rank r)
        {
            Suit = s;
            Rank = r;
            AssignSprite();
            SaveChanges();
        }
        public int GetRankValue()
        {
            return Rank.Value;
        }
        // Method to assign the sprite
        [Button, ShowIf("@this.Sprite == null")]
        public void AssignSprite()
        {
            Sprite assetAtPath = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (assetAtPath != null)
            {
                Sprite = assetAtPath;
                SaveChanges();
            }
            else
            {
                Debug.LogWarning($"Sprite not found at {path}");
            }
        }

        // Comparison method
        public int CompareTo(Card other)
        {
            if (other == null) return 1;
            int rankComparison = Rank.CompareTo(other.Rank);
            if (rankComparison != 0) return rankComparison;
            return Suit.CompareTo(other.Suit);
        }

        public void SaveChanges()
        {


#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}

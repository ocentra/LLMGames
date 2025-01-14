using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Manager.Utilities;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable
{
    [CreateAssetMenu(fileName = nameof(Card), menuName = "LLMGames/Card")]
    public class Card : SerializedScriptableObject, IComparable<Card>, ISaveScriptable
    {
        [SerializeField, HideInInspector] private string id;
        [SerializeField, HideInInspector] private string path;
        [SerializeField, HideInInspector] private Rank rank;
        [SerializeField, HideInInspector] private string rankSymbol;
        [SerializeField, HideInInspector] private Texture2D texture2D;
        [SerializeField, HideInInspector] private Suit suit;

        [ShowInInspector]
        public Suit Suit
        {
            get => suit;
            set => suit = value;
        }

        [ShowInInspector]
        public Rank Rank
        {
            get => rank;
            set
            {
                rank = value;
                id = $"{rank.Alias}_{suit.Symbol}";
                rankSymbol = CardUtility.GetRankSymbol(suit, rank);
                path = rank.Name == "BackCard"
                    ? "Assets/Images/Cards/BackCard.png"
                    : $"Assets/Images/Cards/{rank.Alias}_of_{suit.Name.ToLower() + "s"}.png";
            }
        }

        [ShowInInspector]
        public Texture2D Texture2D
        {
            get => texture2D;
            set => texture2D = value;
        }

        [ReadOnly,ShowInInspector] public string Path => path;
        public string RankSymbol => rankSymbol;
        [ReadOnly, ShowInInspector] public string Id => id;

        public int CompareTo(Card other)
        {
            if (other == null)
            {
                return 1;
            }

            int rankComparison = Rank.CompareTo(other.Rank);
            if (rankComparison != 0)
            {
                return rankComparison;
            }

            return Suit.CompareTo(other.Suit);
        }

        public void SaveChanges()
        {
#if UNITY_EDITOR

            EditorSaveManager.RequestSave(this).Forget();

#endif
        }


        public void Init(Suit s, Rank r)
        {
            Suit = s;
            Rank = r;
#if UNITY_EDITOR

            AssignSprite();
            SaveChanges();
#endif
        }



        // Method to assign the Texture2D
        [Button]
        [ShowIf("@this.Texture2D == null")]
        public void AssignSprite()
        {
#if UNITY_EDITOR
            Texture2D assetAtPath = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (assetAtPath != null)
            {
                texture2D = assetAtPath;
                SaveChanges();
            }
            else
            {
                Debug.LogWarning($"Sprite not found at {path}");
            }
#endif
        }



    }
}
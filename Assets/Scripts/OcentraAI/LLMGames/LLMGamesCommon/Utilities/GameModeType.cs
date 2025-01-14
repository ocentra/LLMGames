using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class GameModeType : IGameModeType, IEquatable<GameModeType>
    {
        // Static instances
        public static readonly GameModeType None = new GameModeType(0, nameof(None), GameGenre.None);

        // Card Games
        public static readonly GameModeType Poker = new GameModeType(1, nameof(Poker), GameGenre.CardGames);
        public static readonly GameModeType Bridge = new GameModeType(2, nameof(Bridge), GameGenre.CardGames);
        public static readonly GameModeType Rummy = new GameModeType(3, nameof(Rummy), GameGenre.CardGames);
        public static readonly GameModeType ThreeCardBrag = new GameModeType(4, nameof(ThreeCardBrag), GameGenre.CardGames);

        // Word Games
        public static readonly GameModeType Scrabble = new GameModeType(100, nameof(Scrabble), GameGenre.WordGames);
        public static readonly GameModeType Crosswords = new GameModeType(101, nameof(Crosswords), GameGenre.WordGames);
        public static readonly GameModeType WordSearch = new GameModeType(102, nameof(WordSearch), GameGenre.WordGames);

        // MindFul Games
        public static readonly GameModeType Meditation = new GameModeType(200, nameof(Meditation), GameGenre.MindFul);
        public static readonly GameModeType BreathingExercises = new GameModeType(201, nameof(BreathingExercises), GameGenre.MindFul);
        public static readonly GameModeType RelaxationGames = new GameModeType(202, nameof(RelaxationGames), GameGenre.MindFul);

        // Voice Games
        public static readonly GameModeType VoiceCommands = new GameModeType(300, nameof(VoiceCommands), GameGenre.Voice);
        public static readonly GameModeType SpeechRecognition = new GameModeType(301, nameof(SpeechRecognition), GameGenre.Voice);
        public static readonly GameModeType VoiceTrivia = new GameModeType(302, nameof(VoiceTrivia), GameGenre.Voice);


        [ShowInInspector, ReadOnly] private int id;
        [ShowInInspector, ReadOnly] private string name;
        [ShowInInspector, ReadOnly] private GameGenre gameGenre;


        private GameModeType(int id, string name, GameGenre gameGenre)
        {
            this.id = id;
            this.name = name;
            this.gameGenre = gameGenre;
        }

        public int Id => id;
        public string Name => name;

        public GameGenre GameGenre => gameGenre;
        public int GenreId => gameGenre.Id;


        public static GameModeType[] GetAll()
        {
            FieldInfo[] fields = typeof(GameModeType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            GameModeType[] subcategories = new GameModeType[fields.Length];
            int count = 0;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.FieldType == typeof(GameModeType))
                {
                    subcategories[count++] = (GameModeType)field.GetValue(null);
                }
            }

            if (count < subcategories.Length)
            {
                GameModeType[] result = new GameModeType[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = subcategories[i];
                }
                return result;
            }

            return subcategories;
        }


        public static IEnumerable<GameModeType> GetSubcategoriesForGenre(GameGenre genre)
        {
            FieldInfo[] fields = typeof(GameModeType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.FieldType == typeof(GameModeType))
                {
                    GameModeType subcategory = (GameModeType)field.GetValue(null);
                    if (subcategory.gameGenre == genre)
                    {
                        yield return subcategory;
                    }
                }
            }
        }


        public static GameModeType FromId(int id)
        {
            foreach (GameModeType subcategory in GetAll())
            {
                if (subcategory.Id == id)
                {
                    return subcategory;
                }
            }

            Debug.LogWarning($"GameModeType with ID {id} not found. Returning None.");
            return None;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameModeType);
        }

        public bool Equals(GameModeType other)
        {
            return other != null && id == other.Id;
        }

        public static bool operator ==(GameModeType left, GameModeType right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(GameModeType left, GameModeType right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }




    }
}

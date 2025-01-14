using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class GameGenre : IGameGenres, IEquatable<GameGenre>
    {
        // Static instances for genres
        public static readonly GameGenre None = new GameGenre(0, nameof(None));
        public static readonly GameGenre CardGames = new GameGenre(1, nameof(CardGames));
        public static readonly GameGenre WordGames = new GameGenre(2, nameof(WordGames));
        public static readonly GameGenre MindFul = new GameGenre(3, nameof(MindFul));
        public static readonly GameGenre Voice = new GameGenre(4, nameof(Voice));

        [ShowInInspector, ReadOnly] private int id;
        [ShowInInspector, ReadOnly] private string name;



        private GameGenre(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int Id => id;
        public string Name => name;



        public static GameGenre[] GetAll()
        {
            FieldInfo[] fields = typeof(GameGenre).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            GameGenre[] gameGenres = new GameGenre[fields.Length];
            int count = 0;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.FieldType == typeof(GameGenre))
                {
                    gameGenres[count++] = (GameGenre)field.GetValue(null);
                }
            }

            if (count < gameGenres.Length)
            {
                GameGenre[] result = new GameGenre[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = gameGenres[i];
                }
                return result;
            }

            return gameGenres;
        }


        public static GameGenre FromId(int id)
        {
            foreach (GameGenre genre in GetAll())
            {
                if (genre.Id == id)
                {
                    return genre;
                }
            }

            Debug.LogWarning($"GameGenre with ID {id} not found. Returning None.");
            return None as GameGenre;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameGenre);
        }

        public bool Equals(GameGenre other)
        {
            return other != null && id == other.Id;
        }

        public static bool operator ==(GameGenre left, GameGenre right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(GameGenre left, GameGenre right)
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

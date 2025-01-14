using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.Commons
{
    public class GameInfoPair : SerializedScriptableObject, ISaveScriptable, IEquatable<GameInfoPair>
    {
        public GameGenre GameGenre;
        [OnValueChanged(nameof(SaveChanges))]
        public Info Info;
        public readonly Guid ID = Guid.NewGuid();
        public event Action<GameInfoPair> OnValueChanged;

        public static GameInfoPair Create(ScriptableObject parent, GameGenre key, Info value)
        {
            GameInfoPair instance = CreateInstance<GameInfoPair>();
            instance.name = key.Name;
            instance.GameGenre = key;
            instance.Info = value;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(instance, parent);
            UnityEditor.EditorUtility.SetDirty(parent);
            
#endif

            return instance;
        }



        public void SaveChanges()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            OnValueChanged?.Invoke(this);
#endif
        }

        public bool Equals(GameInfoPair other)
        {
            if (other == null) return false;
            return GameGenre == other.GameGenre && Info == other.Info;
        }

        public override bool Equals(object obj)
        {
            if (obj is GameInfoPair pair)
            {
                return Equals(pair);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"GameGenre: {GameGenre}, Value: {Info}";
        }
    }
}
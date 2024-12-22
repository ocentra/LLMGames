using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    public class SetTrumpCardEvent<T> : EventArgsBase where T :ScriptableObject
    {
        public T TrumpCard { get; }
        public SetTrumpCardEvent(T trumpCard)
        {
            TrumpCard = trumpCard;
        }


    }
}
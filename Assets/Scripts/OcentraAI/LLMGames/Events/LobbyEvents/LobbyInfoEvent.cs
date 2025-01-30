using OcentraAI.LLMGames.GameModes;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    public class LobbyInfoEvent : EventArgsBase
    {

        public IButton3DSimple Button3DSimple { get; }
        public LobbyInfoEvent(IButton3DSimple button3DSimple)
        {
            Button3DSimple = button3DSimple;
        }


    }

    public class LobbyPlayerUpdateEvent : EventArgsBase
    {
        public enum UpdateType { Add, Remove }
        public IButton3DSimple Button3DSimple { get; }
        public UpdateType Type { get; }

        public LobbyPlayerUpdateEvent(IButton3DSimple button3DSimple,UpdateType type)
            
        {
            Type = type;
            Button3DSimple = button3DSimple;
        }
    }



}
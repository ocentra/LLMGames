using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class HolderStatusEntry
    {
        public Renderer Renderer;
        public List<LobbyStatusInfo> StatusInfoList = new List<LobbyStatusInfo>();
    }
}
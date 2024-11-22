using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.Networking.Manager
{
    public class NetworkBehaviourWithLogger : NetworkBehaviour
    {
        [ShowInInspector] public bool ToEditor { get; set; } = true;
        [ShowInInspector] public bool ToFile { get; set; }

    }
}
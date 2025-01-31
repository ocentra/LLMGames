using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public interface IHumanPlayerData : IPlayerBase
    {
        NetworkVariable<FixedString64Bytes> AuthenticatedPlayerId { get; }
        Player LobbyPlayerData { get; }
        bool IsLocalPlayer { get; }
       
    }
}
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public interface IPlayerData
    {
        NetworkVariable<FixedString64Bytes> AuthenticatedPlayerId { get;}
        ulong OwnerClientId { get; }
        NetworkVariable<FixedString64Bytes>  PlayerName { get; }
        NetworkVariable<int> PlayerIndex { get; }
        Player LobbyPlayerData { get; }
        void SetPlayerIndex(int playerIndex);
    }
}
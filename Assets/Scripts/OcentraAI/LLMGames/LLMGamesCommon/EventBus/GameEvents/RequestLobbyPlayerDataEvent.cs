using Cysharp.Threading.Tasks;
using System;
using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public class RequestLobbyPlayerDataEvent : EventArgsBase
    {
        public string PlayerId;

        public UniTaskCompletionSource<(bool success , Player player)> PlayerDataSource { get; }

        public RequestLobbyPlayerDataEvent(UniTaskCompletionSource<(bool success, Player player)> playerDataSource, string playerId)
        {
            PlayerDataSource = playerDataSource;
            PlayerId = playerId;
        }
    }
}
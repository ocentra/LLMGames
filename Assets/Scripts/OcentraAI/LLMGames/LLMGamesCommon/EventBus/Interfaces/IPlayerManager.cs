using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    public interface IPlayerManager
    {
        void RegisterPlayer(IPlayerBase player);
        bool UnregisterPlayer(IPlayerBase player);
        IReadOnlyList<IPlayerData> GetAllHumanPlayers();
        IReadOnlyList<IComputerPlayerData> GetAllComputerPlayers();
        IReadOnlyList<IPlayerBase> GetAllPlayers();
        IReadOnlyList<IPlayerBase> GetActivePlayers();
        bool TryGetPlayer(ulong playerId, out IPlayerBase playerBase);
        bool AreAllPlayersSynced { get; }
        ScriptableObject GetGameMode();
    }
}
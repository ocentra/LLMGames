using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    public interface IPlayerBase : IPlayerEvent
    {
        IPlayerManager PlayerManager { get; set; }
        NetworkVariable<ulong> PlayerId { get; set; }

        NetworkVariable<FixedString64Bytes> PlayerName { get; }

        NetworkVariable<int> PlayerIndex { get; }
        void SetPlayerIndex(int playerIndex);

        GameObject GameObject { get; set; }

        NetworkVariable<bool> IsPlayerRegistered { get; set; }
        void SetPlayerRegistered(bool value = true);

        NetworkVariable<bool> ReadyForNewGame { get; set; }
        void SetReadyForGame(bool value = true);

        NetworkVariable<bool> IsPlayerTurn { get; set; }
        void SetIsPlayerTurn(bool value = true);

        NetworkVariable<bool> HasSeenHand { get; set; }
       

    }
}
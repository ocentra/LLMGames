using Unity.Collections;
using Unity.Netcode;

namespace OcentraAI.LLMGames.Events
{
    public interface IPlayerBase : IPlayerEvent
    {
        NetworkVariable<ulong> PlayerId { get; set; }
        NetworkVariable<FixedString64Bytes> PlayerName { get; }
        NetworkVariable<int> PlayerIndex { get; }
        void SetPlayerIndex(int playerIndex);
        NetworkVariable<int> Coins { get; }
        void SetCoins(int playerIndex);
        int GetCoins();
        NetworkVariable<bool> IsPlayerRegistered { get; set; }
        void RegisterPlayer(IPlayerManager playerManager, string displayName);
        NetworkVariable<bool> ReadyForNewGame { get; set; }
        void SetReadyForGame(bool value = true);
        NetworkVariable<bool> IsPlayerTurn { get; set; }
        void SetIsPlayerTurn(bool value = true);
        NetworkVariable<bool> HasSeenHand { get; set; }
        string GetCard(int i);
        public NetworkVariable<int> LastDecision { get; }
        public void SetLastDecision(int value);
        public NetworkVariable<bool> HasTakenBettingDecision { get; set; }
        public void SetHasTakenBettingDecision(bool value);

        NetworkVariable<bool> IsBankrupt { get; set; }
        void SetBankrupt(bool value = true);

        void AutoBet();

        NetworkVariable<bool> HasFolded { get; set; }
    }
}
using OcentraAI.LLMGames.Events;
using Unity.Netcode;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [System.Serializable]
    public struct PlayerDataDTO : INetworkSerializable
    {
        public int PlayerIndex;
        public ulong PlayerId;

        public PlayerDataDTO(IPlayerBase playerBase)
        {
            PlayerIndex = playerBase.PlayerIndex.Value;
            PlayerId = playerBase.PlayerId.Value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerIndex);
            serializer.SerializeValue(ref PlayerId);
        }
    }
}
using Unity.Netcode;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [System.Serializable]
    public struct PlayerDataDTO : INetworkSerializable
    {
        public int PlayerIndex;
        public ulong OwnerClientId;

        public PlayerDataDTO(int playerIndex, ulong ownerClientId)
        {
            PlayerIndex = playerIndex;
            OwnerClientId = ownerClientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerIndex);
            serializer.SerializeValue(ref OwnerClientId);
        }
    }
}
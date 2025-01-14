using Unity.Netcode;

namespace OcentraAI.LLMGames.GameModes
{
    public interface IGameModeType : INetworkSerializable
    {
        int Id { get; }
        string Name { get; }
        public int GenreId { get; }

    }
}
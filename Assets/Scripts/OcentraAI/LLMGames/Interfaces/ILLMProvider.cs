using Unity.Netcode;

namespace OcentraAI.LLMGames
{
    public interface ILLMProvider : INetworkSerializable
    {
        int ProviderId { get; }
        string Name { get; }
    }
}
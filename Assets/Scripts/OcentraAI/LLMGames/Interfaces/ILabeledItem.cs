using System;
using Unity.Netcode;

namespace OcentraAI.LLMGames.GameModes
{
    public interface ILabeledItem : INetworkSerializable
    {
        public int Id { get; }
        public string Name { get; }

    }
}
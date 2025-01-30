using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace OcentraAI.LLMGames.GameModes
{
    public interface ILabeledItem : INetworkSerializable
    {
        public int Id { get; }
        public string Name { get; }

    }

    public interface ILobbyInfoEntry
    {
        public (string key, string value, string icon) GetKeyValueTuple();
    }
}
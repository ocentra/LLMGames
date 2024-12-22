using System;

namespace OcentraAI.LLMGames.Events
{
    public class CreateLobbyEvent : EventArgsBase
    {
        public LobbyOptions Options { get; }

        public CreateLobbyEvent(LobbyOptions options)
        {
            Options = options;
        }
    }
}
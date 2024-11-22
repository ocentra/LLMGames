using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public class ProfileCreatedEvent : EventArgsBase
    {
        public Player Player { get; }
        public ProfileCreatedEvent(Player player)
        {
            Player = player;

        }

    }
}
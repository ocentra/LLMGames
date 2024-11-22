namespace OcentraAI.LLMGames.Events
{
    public class StartLobbyAsHostEvent : EventArgsBase
    {
        public string LobbyId;

        public StartLobbyAsHostEvent(string lobbyId)
        {
            LobbyId = lobbyId;
        }
    }
}
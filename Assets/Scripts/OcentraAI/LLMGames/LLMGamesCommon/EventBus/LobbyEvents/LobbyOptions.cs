namespace OcentraAI.LLMGames.Events
{
    public struct LobbyOptions
    {
        public string LobbyName { get; }
        public string GameMode { get; }
        public int MaxPlayers { get; }
        public bool IsPrivate { get; }
        public string OptionsPassword { get; }

        public LobbyOptions(string lobbyName, string gameMode, int maxPlayers, bool isPrivate = false, string optionsPassword = null)
        {
            LobbyName = lobbyName;
            GameMode = gameMode;
            MaxPlayers = maxPlayers;
            IsPrivate = isPrivate;
            OptionsPassword = optionsPassword;
        }
    }
}
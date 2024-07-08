namespace OcentraAI.LLMGames.Authentication
{
    [System.Serializable]
    public class PlayerData
    {
        public string PlayerName;
        public string Email;
        public string PlayerID { get; internal set; }
    }
}
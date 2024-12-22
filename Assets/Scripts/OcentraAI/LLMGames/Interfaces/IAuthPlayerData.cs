namespace OcentraAI.LLMGames.Authentication
{
    public interface IAuthPlayerData
    {
        public string Email { get; }
        public string PlayerID { get; }
        public string PlayerName { get; }

        void Update(string userName, string email);
    }
}
namespace OcentraAI.LLMGames.Events
{
    public struct UserCredentials
    {
        public string Username { get; }
        public string Password { get; }
        public bool IsValid { get; }

        public UserCredentials(string username, string password, bool isValid)
        {
            Username = username;
            Password = password;
            IsValid = isValid;
        }
    }
}
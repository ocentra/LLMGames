using Sirenix.OdinInspector;
using System;

namespace OcentraAI.LLMGames.Authentication
{
    [Serializable]
    public class AuthPlayerData : IAuthPlayerData
    {
        [ShowInInspector] public string Email { get; private set; }
        [ShowInInspector] public string PlayerID { get; private set; }
        [ShowInInspector] public string PlayerName { get; private set; }

        public AuthPlayerData(string playerID, string playerName, string email)
        {
            Email = email;
            PlayerID = playerID;
            PlayerName = playerName;
        }

        public AuthPlayerData(string playerID, string playerName)
        {
            Email = "email";
            PlayerID = playerID;
            PlayerName = playerName;
        }

        public void Update(string userName, string email)
        {
            PlayerName = userName;
            Email = email;
        }
    }
}
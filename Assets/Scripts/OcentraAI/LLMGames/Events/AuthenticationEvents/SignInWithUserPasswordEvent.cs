using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithUserPasswordEvent : EventArgsBase
    {
        public string Username { get; }
        public string Password { get; }
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInWithUserPasswordEvent(string username, string password)
        {
            Username = username;
            Password = password;
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
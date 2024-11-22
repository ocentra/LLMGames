using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithSteamEvent : EventArgsBase
    {
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInWithSteamEvent()
        {
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
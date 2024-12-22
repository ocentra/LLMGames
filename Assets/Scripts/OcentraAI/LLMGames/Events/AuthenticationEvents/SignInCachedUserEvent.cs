using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInCachedUserEvent : EventArgsBase
    {
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInCachedUserEvent()
        {
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
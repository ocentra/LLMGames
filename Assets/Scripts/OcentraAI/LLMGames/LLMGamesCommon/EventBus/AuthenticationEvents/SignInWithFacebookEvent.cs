using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithFacebookEvent : EventArgsBase
    {
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInWithFacebookEvent()
        {
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
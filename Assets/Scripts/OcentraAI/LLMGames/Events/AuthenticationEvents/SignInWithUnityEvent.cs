using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithUnityEvent : EventArgsBase
    {
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInWithUnityEvent()
        {
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
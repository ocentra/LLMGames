using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithUnityEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IAuthResult> CompletionSource { get; }

        public SignInWithUnityEvent()
        {
            CompletionSource = new UniTaskCompletionSource<IAuthResult>();
        }
    }
}
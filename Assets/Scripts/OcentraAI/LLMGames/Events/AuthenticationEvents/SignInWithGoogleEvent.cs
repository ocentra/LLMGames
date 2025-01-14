using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithGoogleEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IAuthResult> CompletionSource { get; }

        public SignInWithGoogleEvent()
        {
            CompletionSource = new UniTaskCompletionSource<IAuthResult>();
        }
    }
}
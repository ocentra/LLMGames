using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInAsGuestEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IAuthResult> CompletionSource { get; }

        public SignInAsGuestEvent(UniTaskCompletionSource<IAuthResult> completionSource)
        {
            CompletionSource = completionSource;
        }
    }
}
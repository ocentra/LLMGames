using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SignInAsGuestEvent : EventArgsBase
    {
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInAsGuestEvent()
        {
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
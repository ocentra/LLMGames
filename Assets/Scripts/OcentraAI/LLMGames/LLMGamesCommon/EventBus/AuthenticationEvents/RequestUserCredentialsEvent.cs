using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class RequestUserCredentialsEvent : EventArgsBase
    {
        public UniTaskCompletionSource<UserCredentials> CompletionSource { get; }

        public RequestUserCredentialsEvent()
        {
            CompletionSource = new UniTaskCompletionSource<UserCredentials>();
        }
    }
}
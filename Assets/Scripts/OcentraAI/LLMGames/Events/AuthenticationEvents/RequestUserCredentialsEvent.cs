using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class RequestUserCredentialsEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IUserCredentials> CompletionSource { get; }

        public RequestUserCredentialsEvent()
        {
            CompletionSource = new UniTaskCompletionSource<IUserCredentials>();
        }
    }
}
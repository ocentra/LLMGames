using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class RequestAdditionalUserInfoEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(string userName, string email)> CompletionSource { get; }
        public bool IsGuest { get; }

        public RequestAdditionalUserInfoEvent(bool isGuest)
        {
            IsGuest = isGuest;
            CompletionSource = new UniTaskCompletionSource<(string userName, string email)>();
        }
    }
}
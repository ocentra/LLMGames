using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class InputLobbyPasswordEvent : EventArgsBase
    {

        public UniTaskCompletionSource<string> PasswordSetSource;
        public InputLobbyPasswordEvent(UniTaskCompletionSource<string> passwordSetSource)
        {
            PasswordSetSource = passwordSetSource;

        }

    }
}
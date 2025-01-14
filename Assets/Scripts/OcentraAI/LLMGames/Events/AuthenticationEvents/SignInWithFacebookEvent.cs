using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithFacebookEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IAuthResult> CompletionSource { get; }

        public SignInWithFacebookEvent()
        {
            CompletionSource = new UniTaskCompletionSource<IAuthResult>();
        }
    }
}
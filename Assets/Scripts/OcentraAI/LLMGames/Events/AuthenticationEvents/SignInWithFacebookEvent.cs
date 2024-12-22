using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class SignInWithFacebookEvent : EventArgsBase
    {
        public UniTaskCompletionSource<AuthResult> CompletionSource { get; }

        public SignInWithFacebookEvent()
        {
            CompletionSource = new UniTaskCompletionSource<AuthResult>();
        }
    }
}
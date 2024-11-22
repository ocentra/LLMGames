namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationErrorUIEvent : EventArgsBase
    {
        public AuthenticationErrorUIEvent (string message, float delay =10f)
        {
            Message = message;
            Delay = delay;
        }

        public string Message { get; }
        public float Delay { get; }
    }
}
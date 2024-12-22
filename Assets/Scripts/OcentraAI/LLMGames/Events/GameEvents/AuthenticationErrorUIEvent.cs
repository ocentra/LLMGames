namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationErrorUIEvent : EventArgsBase
    {
        public string Message { get; }
        public float Delay { get; }
        public AuthenticationErrorUIEvent(string message, float delay = 10f)
        {
            Message = message;
            Delay = delay;
        }


    }
}
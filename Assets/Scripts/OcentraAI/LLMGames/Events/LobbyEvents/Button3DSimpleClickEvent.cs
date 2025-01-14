namespace OcentraAI.LLMGames.Events
{
    public class Button3DSimpleClickEvent : EventArgsBase
    {
        public IButton3DSimple Button3DSimple { get; }
        public Button3DSimpleClickEvent(IButton3DSimple button3DSimple)
        {
            Button3DSimple = button3DSimple;
        }

    }
}
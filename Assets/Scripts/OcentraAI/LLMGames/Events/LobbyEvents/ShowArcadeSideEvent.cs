namespace OcentraAI.LLMGames.Events
{
    public class ShowArcadeSideEvent : EventArgsBase
    {
        public bool Show { get; }
        public ShowArcadeSideEvent(bool show)
        {
            Show = show;
        }

    }
}
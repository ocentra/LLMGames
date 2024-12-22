namespace OcentraAI.LLMGames.Events
{
    public class ShowScreenEvent : EventArgsBase
    {
        public string ScreenToShow { get; }
       
        public ShowScreenEvent(string screenToShow)
        {
            ScreenToShow = screenToShow;
           
        }
    }
}
namespace OcentraAI.LLMGames.Events
{
    public class ShowFriendsEvent : EventArgsBase
    {
        public bool Show { get; }
        public ShowFriendsEvent(bool show)
        {
            Show = show;
        }

    }
}
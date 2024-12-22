namespace OcentraAI.LLMGames.Events
{
    public interface IUIScreen
    {
        void ShowScreen();
        void HideScreen();
        void ToggleScreen();
        bool IsScreenInstanceVisible();
        bool IsInitialized { get; }
        string ScreenId { get; }
    }
}
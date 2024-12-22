namespace OcentraAI.LLMGames.Events
{
    public interface IPlayerUI
    {
        int PlayerIndex { get; }
        void SetPlayerIndex(int index);
    }
}
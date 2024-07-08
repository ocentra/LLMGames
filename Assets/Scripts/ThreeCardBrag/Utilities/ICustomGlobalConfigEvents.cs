namespace ThreeCardBrag.Utilities
{
    public interface ICustomGlobalConfigEvents
    {
        void OnConfigAutoCreated();
        void OnConfigInstanceFirstAccessed();
    }
}
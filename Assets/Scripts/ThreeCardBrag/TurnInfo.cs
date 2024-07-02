using System.Threading.Tasks;

namespace ThreeCardBrag
{
    [System.Serializable]
    public struct TurnInfo
    {
        public Player CurrentPlayer;
        public float ElapsedTime;
        public TaskCompletionSource<bool> TurnCompletionSource;
        public TurnInfo(Player currentPlayer)
        {
            CurrentPlayer = currentPlayer;
            ElapsedTime = 0f;
            TurnCompletionSource = new TaskCompletionSource<bool>();
        }
    }
}
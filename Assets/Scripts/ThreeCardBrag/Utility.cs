using System.Threading.Tasks;

namespace ThreeCardBrag
{
    public static class Utility
    {
        public static async Task Delay(float seconds)
        {
            await Task.Delay((int)(seconds * 1000));
        }
    }
}
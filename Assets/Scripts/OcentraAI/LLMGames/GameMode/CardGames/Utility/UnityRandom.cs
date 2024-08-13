namespace OcentraAI.LLMGames.GameModes
{
    public class UnityRandom : IRandom
    {

        public int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
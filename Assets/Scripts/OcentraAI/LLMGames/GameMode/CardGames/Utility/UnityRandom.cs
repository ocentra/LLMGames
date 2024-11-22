using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    public class UnityRandom : IRandom
    {
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}
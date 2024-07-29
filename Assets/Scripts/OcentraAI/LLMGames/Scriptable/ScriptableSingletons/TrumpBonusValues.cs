using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Rules
{
    [Serializable]
    public class TrumpBonusValues
    {
        public int TrumpCardBonus = 10; 
        public int RankAdjacentBonus = 5;
        public int ThreeOfKindBonus = 15;
        public int SequenceBonus = 15;
        public int StraightFlushBonus = 15;
        public int CardInMiddleBonus = 5;
        public int WildCardBonus = 10;
        public int SameColorBonus = 5;
    }


}
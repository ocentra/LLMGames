using System;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Rules
{
    [Serializable]
    public class TrumpBonusValues
    {
        [Tooltip("Bonus for having the Trump card in hand")]
        public int TrumpCardBonus = 10;

        [Tooltip("Bonus for having a card adjacent in rank to the Trump card")]
        public int RankAdjacentBonus = 5;

        [Tooltip("Bonus for Three of a Kind with Trump card")]
        public int ThreeOfKindBonus = 15;

        [Tooltip("Bonus for a Sequence with Trump card")]
        public int SequenceBonus = 15;

        [Tooltip("Bonus for a Straight Flush with Trump card")]
        public int StraightFlushBonus = 15;

        [Tooltip("Bonus for having the Trump card in the middle of a sequence")]
        public int CardInMiddleBonus = 5;

        [Tooltip("Bonus for using the Trump card as a wild card")]
        public int WildCardBonus = 10;

        [Tooltip("Bonus for having cards of the same color")]
        public int SameColorBonus = 5;

        [Tooltip("Bonus for a Pair with Trump card")]
        public int PairBonus = 5;

        [Tooltip("Bonus for Four of a Kind with Trump card")]
        public int FourOfKindBonus = 20;

        [Tooltip("Bonus for Five of a Kind with Trump card")]
        public int FiveOfKindBonus = 25;

        [Tooltip("Bonus for FullHouseBonus")] 
        public int FullHouseBonus = 100;

        public int StraightBonus = 25;
        public int FlushBonus = 20;
        public int RoyalFlushBonus = 35;
        public int HighCardBonus = 10;

        public int GetBonusForSet(int setSize)
        {
            switch (setSize)
            {
                case 2: return PairBonus;
                case 3: return ThreeOfKindBonus;
                case 4: return FourOfKindBonus;
                case 5: return FiveOfKindBonus;
                default: return 0;
            }
        }
    }
}
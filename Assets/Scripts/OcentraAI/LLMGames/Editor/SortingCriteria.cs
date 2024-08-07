using System;

namespace OcentraAI.LLMGames.GameModes
{
    [Flags]
    public enum SortingCriteria
    {
        None = 0,
        NameAscending = 1,
        NameDescending = 2,
        PriorityAscending = 4,
        PriorityDescending = 8,
        BonusValueAscending = 16,
        BonusValueDescending = 32
    }
}
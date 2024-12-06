using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateScoreDataEvent<T> : EventArgsBase
    {
        public int CurrentRound { get; set; }
        public int Pot { get; set; }
        public int CurrentBet { get; set; }
        public int TotalRounds { get; set; }

        public List<T> RoundRecords { get; set; }

        public UpdateScoreDataEvent(int pot, int currentBet, int totalRounds, int currentRound, List<T> roundRecords = null)
        {
            CurrentRound = currentRound;
            RoundRecords = roundRecords;
            Pot = pot;
            CurrentBet = currentBet;
            TotalRounds = totalRounds;

        }
    }
}
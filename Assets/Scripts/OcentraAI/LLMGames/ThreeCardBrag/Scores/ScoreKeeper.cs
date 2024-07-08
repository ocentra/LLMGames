using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class ScoreKeeper 
    {

        public List<RoundScore> RoundScores = new List<RoundScore>();
        public int HumanTotalWins = 0;
        public int ComputerTotalWins = 0;
        public RoundScore RoundScore = new RoundScore();




        public void AddToTotalRoundScores(Player player, int pot)
        {
            RoundScore.Winner = player;
            RoundScore.Pot = pot;
            RoundScores.Add(RoundScore);
            switch (player)
            {
                case HumanPlayer:
                    HumanTotalWins++;
                    break;
                case ComputerPlayer:
                    ComputerTotalWins++;
                    break;
            }

            RoundScore = new RoundScore();
        }

        public void ResetScores()
        {
            RoundScores.Clear();
            HumanTotalWins = 0;
            ComputerTotalWins = 0;
        }
    }
}
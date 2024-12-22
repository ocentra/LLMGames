using OcentraAI.LLMGames.Events;
using System.Collections.Generic;
using Unity.Netcode;

namespace OcentraAI.LLMGames.GamesNetworking
{
    public class Leaderboard : ILeaderboard
    {
        public Dictionary<ulong, ILeaderboardEntry> Entries { get; }



        public Leaderboard(List<INetworkRoundRecord> roundRecords)
        {
            Dictionary<ulong, LeaderboardEntry> unsortedEntries = new Dictionary<ulong, LeaderboardEntry>();

            foreach (INetworkRoundRecord roundRecord in roundRecords)
            {
                ulong winnerId = roundRecord.WinnerId == default ? ulong.MaxValue : roundRecord.WinnerId;

                if (unsortedEntries.TryGetValue(winnerId, out LeaderboardEntry entry))
                {
                    entry.Update(1, roundRecord.PotAmount);
                }
                else
                {
                    string displayName = winnerId == ulong.MaxValue ? "Tie" : roundRecord.Winner;
                    unsortedEntries[winnerId] = new LeaderboardEntry(1, roundRecord.PotAmount, displayName);
                }
            }

            List<KeyValuePair<ulong, LeaderboardEntry>> sortedEntries = new List<KeyValuePair<ulong, LeaderboardEntry>>(unsortedEntries);

            sortedEntries.Sort((a, b) =>
            {
                int winComparison = b.Value.Wins.CompareTo(a.Value.Wins);
                return winComparison != 0 ? winComparison : b.Value.TotalWinnings.CompareTo(a.Value.TotalWinnings);
            });

            Entries = new Dictionary<ulong, ILeaderboardEntry>();
            foreach (KeyValuePair<ulong, LeaderboardEntry> kvp in sortedEntries)
            {
                Entries[kvp.Key] = kvp.Value;
            }
        }

        public bool HasClearWinner()
        {

            IReadOnlyList<NetworkClient> clients = NetworkManager.Singleton.ConnectedClientsList;

            if (clients.Count == 1)
            {
                return true; // Single player remaining is the clear winner
            }

            int maxWins = -1;
            int maxWinnings = -1;

            ILeaderboardEntry topPlayerByWins = null;
            ILeaderboardEntry topPlayerByWinnings = null;

            foreach (ILeaderboardEntry entry in Entries.Values)
            {
                if (entry.Wins > maxWins)
                {
                    maxWins = entry.Wins;
                    topPlayerByWins = entry;
                }
                else if (entry.Wins == maxWins)
                {
                    topPlayerByWins = null;
                }

                if (entry.TotalWinnings > maxWinnings)
                {
                    maxWinnings = entry.TotalWinnings;
                    topPlayerByWinnings = entry;
                }
                else if (entry.TotalWinnings == maxWinnings)
                {
                    topPlayerByWinnings = null;
                }
            }

            // A clear winner exists if the same player has both the most wins and the most total winnings
            return topPlayerByWins != null && topPlayerByWins == topPlayerByWinnings;
        }


        private class LeaderboardEntry : ILeaderboardEntry
        {
            public int Wins { get; private set; }
            public int TotalWinnings { get; private set; }
            public string DisplayName { get; }

            public LeaderboardEntry(int wins, int totalWinnings, string displayName)
            {
                Wins = wins;
                TotalWinnings = totalWinnings;
                DisplayName = displayName;
            }

            public void Update(int wins, int totalWinnings)
            {
                Wins += wins;
                TotalWinnings += totalWinnings;
            }
        }
    }
}

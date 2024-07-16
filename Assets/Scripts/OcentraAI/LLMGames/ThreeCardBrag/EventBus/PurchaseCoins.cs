using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PurchaseCoins : EventArgs
    {
        public int Amount { get; }

        public PurchaseCoins(int amount)
        {
            Amount = amount;
        }
    }
}
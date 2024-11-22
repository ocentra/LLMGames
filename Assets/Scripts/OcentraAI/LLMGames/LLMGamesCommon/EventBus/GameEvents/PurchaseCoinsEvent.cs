using System;

namespace OcentraAI.LLMGames.Events
{
    public class PurchaseCoinsEvent : EventArgsBase
    {
        public PurchaseCoinsEvent(int amount)
        {
            Amount = amount;
            
        }

        public int Amount { get; }
    }
}
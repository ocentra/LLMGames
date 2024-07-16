using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class SetFloorCard : EventArgs
    {
        public bool SetNull { get; } = false;
        public SetFloorCard(bool setNull = false)
        {
            SetNull = setNull;
        }
    }
}
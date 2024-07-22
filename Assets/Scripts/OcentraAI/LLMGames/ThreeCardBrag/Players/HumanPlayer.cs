using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class HumanPlayer : Player
    {

        public HumanPlayer(PlayerData playerData,int initialCoins)
            : base(playerData, PlayerType.Human, initialCoins)
        {
            SetInitialCoins(initialCoins);
        }


        public override void SeeHand()
        {
            base.SeeHand();
            EventBus.Publish(new UpdatePlayerHandDisplay(this));
            ShowHand();
        }

        public override void ShowHand(bool isRoundEnd = false)
        {
            base.ShowHand(isRoundEnd);
            EventBus.Publish(new UpdatePlayerHandDisplay(this, isRoundEnd));
        }

        public override void PickAndSwap(Card floorCard, Card swapCard)
        {
            base.PickAndSwap(floorCard, swapCard);
            EventBus.Publish(new UpdatePlayerHandDisplay(this));

        }

    }
}
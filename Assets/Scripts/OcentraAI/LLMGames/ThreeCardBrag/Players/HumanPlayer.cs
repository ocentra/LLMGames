using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class HumanPlayer : Player
    {

        public HumanPlayer(PlayerData playerData, int initialCoins)
            : base(playerData, PlayerType.Human, initialCoins)
        {
            SetInitialCoins(initialCoins);
        }


        public override void SeeHand()
        {
            base.SeeHand();
            EventBus.Publish(new UpdatePlayerHandDisplay(this));

            if (WildCardInHand != null)
            {
                EventBus.Publish(new UpdateWildCardsHighlight(WildCardInHand, true));
            }
            ShowHand();
        }

        public override void ShowHand(bool showHands = false)
        {
            base.ShowHand(showHands);
            EventBus.Publish(new UpdatePlayerHandDisplay(this, showHands));
        }

        public override void PickAndSwap(Card floorCard, Card swapCard)
        {
            base.PickAndSwap(floorCard, swapCard);

            if (WildCardInHand != null)
            {
                EventBus.Publish(new UpdateWildCardsHighlight(WildCardInHand, true));
            }

            EventBus.Publish(new UpdatePlayerHandDisplay(this));

        }



    }
}
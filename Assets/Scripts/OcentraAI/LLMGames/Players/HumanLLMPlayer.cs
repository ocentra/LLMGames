using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Scriptable;

namespace OcentraAI.LLMGames.Players
{
    public class HumanLLMPlayer : LLMPlayer
    {
        public HumanLLMPlayer(AuthPlayerData authPlayerData, int initialCoins, int index)
            : base(authPlayerData, PlayerType.Human, initialCoins, index)
        {
            SetInitialCoins(initialCoins);
        }


        public override void SeeHand()
        {
            base.SeeHand();
           // EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent<LLMPlayer>(this));



            ShowHand();
        }

        public override void ShowHand(bool showHands = false)
        {
            base.ShowHand(showHands);
            if (showHands )
            {
              //  EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent<LLMPlayer>(this, showHands));

            }
        }

        public override void PickAndSwap(Card floorCard, Card swapCard)
        {
            base.PickAndSwap(floorCard, swapCard);

            if (WildCardInHand != null)
            {
               
            }

          //  EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent<LLMPlayer>(this));
        }
    }
}
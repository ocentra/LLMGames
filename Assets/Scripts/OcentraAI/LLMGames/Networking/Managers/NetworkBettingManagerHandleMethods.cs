using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GamesNetworking;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System.Threading.Tasks;

namespace OcentraAI.LLMGames.Networking.Manager
{
    public partial class NetworkBettingManager : NetworkManagerBase
    {

        public async UniTask HandleRaiseBet(PlayerDecisionRaiseBetEvent raiseBetEvent, NetworkPlayer playerBase)
        {
            if (raiseBetEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for RaiseBet.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            int raiseAmount = (int)raiseBetEvent.Amount;



            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} raised the bet by {raiseAmount}", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleSeeHand(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer playerBase)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for SeeHand.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            playerBase.HasSeenHand.Value = true;
            bool success = await EventBus.Instance.PublishAsync(new UpdatePlayerHandDisplayEvent(playerBase));

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} chose to see their hand.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandlePlayBlind(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer playerBase)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for PlayBlind.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            playerBase.HasSeenHand.Value = false;
            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} chose to play blind.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleBet(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer playerBase)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Bet.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }



            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} placed a bet.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleFold(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer playerBase)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Fold.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} folded.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }



        public async UniTask HandleShowCall(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer playerBase)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for ShowCall.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} called.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard0(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer playerBase)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard0.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard0.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard1(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer playerBase)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard1.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard1.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard2(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer playerBase)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard2.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard2.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard3(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer playerBase)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard3.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard3.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleTrump(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer playerBase)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Trump.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played Trump.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleShowAllFloorCards(PlayerDecisionUIEvent uiEvent, NetworkPlayer playerBase)
        {
            if (uiEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for ShowAllFloorCards.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            bool success = await EventBus.Instance.PublishAsync(new ShowAllFloorCardEvent());

            await UniTask.Yield();
        }

        public async UniTask HandlePurchaseCoins(PlayerDecisionUIEvent uiEvent, NetworkPlayer playerBase)
        {
            if (uiEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for PurchaseCoins.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} purchased coins.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        private async UniTask HandlePickAndSwap(PlayerDecisionPickAndSwapEvent playerDecisionEvent, NetworkPlayer networkPlayer)
        {
            string draggedCardString = playerDecisionEvent.DraggedCard;
            string cardInHandString = playerDecisionEvent.CardInHand;

            Card draggedCard = CardUtility.GetCardFromSymbol(draggedCardString);
            Card handCard = CardUtility.GetCardFromSymbol(cardInHandString);

            networkPlayer.PickAndSwap(draggedCard, handCard);

            await UniTask.Yield();
        }

        private async UniTask HandleDrawFromDeck(PlayerDecisionBettingEvent playerDecisionEvent, NetworkPlayer networkPlayer)
        {
            bool success = await EventBus.Instance.PublishAsync(new SetFloorCardEvent<Card>());
            await UniTask.Yield();
        }
    }
}
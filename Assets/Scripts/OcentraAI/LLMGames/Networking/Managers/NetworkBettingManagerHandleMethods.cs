using Codice.CM.Common;
using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GamesNetworking;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System.Threading.Tasks;
using static System.String;

namespace OcentraAI.LLMGames.Networking.Manager
{
    public partial class NetworkBettingManager : NetworkManagerBase
    {

        public async UniTask HandleRaiseBet(PlayerDecisionRaiseBetEvent raiseBetEvent, NetworkPlayer networkPlayer)
        {
            if (raiseBetEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for RaiseBet.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            int raiseAmount = (int)raiseBetEvent.Amount;

            (bool Success, string ErrorMessage) result = await NetworkScoreManager.HandleRaiseBet(raiseAmount, networkPlayer);

            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(raiseBetEvent.Decision,networkPlayer, true));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} raised the bet by {raiseAmount}", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleSeeHand(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer networkPlayer)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for SeeHand.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            networkPlayer.HasSeenHand.Value = true;
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(bettingEvent.Decision, networkPlayer, false));

            bool success = await EventBus.Instance.PublishAsync(new UpdatePlayerHandDisplayEvent(networkPlayer));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} chose to see their hand.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandlePlayBlind(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer networkPlayer)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for PlayBlind.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            (bool Success, string ErrorMessage) result = await NetworkScoreManager.HandlePlayBlind(networkPlayer);

            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(bettingEvent.Decision, networkPlayer, true));

            networkPlayer.HasSeenHand.Value = false;
            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} chose to play blind.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleBet(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer networkPlayer)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Bet.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            (bool Success, string ErrorMessage) result = await NetworkScoreManager.HandleBet(networkPlayer);

            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(bettingEvent.Decision, networkPlayer, true));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} placed a bet.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleFold(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer networkPlayer)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Fold.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }


            bool result = await NetworkScoreManager.HandleFold(Empty, networkPlayer, true);

            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(bettingEvent.Decision, networkPlayer, true));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} folded.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }



        public async UniTask HandleShowCall(PlayerDecisionBettingEvent bettingEvent, NetworkPlayer networkPlayer)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for ShowCall.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            (bool Success, string ErrorMessage) result = await NetworkScoreManager.HandleShowCall(networkPlayer);

            if (result.Success)
            {
                await EventBus.Instance.PublishAsync(new DecisionTakenEvent(bettingEvent.Decision, networkPlayer, true));
               
            }
           

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} called.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard0(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer networkPlayer)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard0.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }
            //todo Further
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(wildcardEvent.Decision, networkPlayer, false));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} played WildCard0.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard1(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer networkPlayer)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard1.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }
            //todo Further
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(wildcardEvent.Decision, networkPlayer, false));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} played WildCard1.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard2(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer networkPlayer)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard2.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }
            //todo Further
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(wildcardEvent.Decision, networkPlayer, false));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} played WildCard2.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleWildCard3(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer networkPlayer)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard3.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }
            //todo Further
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(wildcardEvent.Decision, networkPlayer, false));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} played WildCard3.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleTrump(PlayerDecisionWildcardEvent wildcardEvent, NetworkPlayer networkPlayer)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Trump.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            //todo Further
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(wildcardEvent.Decision, networkPlayer, false));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} played Trump.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        public async UniTask HandleShowAllFloorCards(PlayerDecisionUIEvent uiEvent, NetworkPlayer networkPlayer)
        {
            if (uiEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for ShowAllFloorCards.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }

            (bool Success, string ErrorMessage) result = await NetworkScoreManager.HandleShowAllFloorCards(networkPlayer);

            if (result.Success)
            {
                bool success = await EventBus.Instance.PublishAsync(new ShowAllFloorCardEvent());
            }
            else
            {
                await ShowMessage(result.ErrorMessage, PlayerDecision.Fold.Name);
            }


            await UniTask.Yield();
        }

        public async UniTask HandlePurchaseCoins(PlayerDecisionUIEvent uiEvent, NetworkPlayer networkPlayer)
        {
            if (uiEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for PurchaseCoins.", this, ToEditor, ToFile, UseStackTrace);
                return;
            }
            await EventBus.Instance.PublishAsync(new DecisionTakenEvent(uiEvent.Decision, networkPlayer, false));

            GameLoggerScriptable.Log($"Player {networkPlayer.PlayerName.Value.Value} purchased coins.", this, ToEditor, ToFile, UseStackTrace);
            await UniTask.Yield();
        }

        private async UniTask HandlePickAndSwap(PlayerDecisionPickAndSwapEvent playerDecisionEvent, NetworkPlayer networkPlayer)
        {
            string draggedCardString = playerDecisionEvent.DraggedCard;
            string cardInHandString = playerDecisionEvent.CardInHand;

            Card draggedCard = CardUtility.GetCardFromSymbol(draggedCardString);
            Card handCard = CardUtility.GetCardFromSymbol(cardInHandString);

            bool success = await networkPlayer.PickAndSwap(draggedCard, handCard);

            if (success)
            {
                await EventBus.Instance.PublishAsync(new DecisionTakenEvent(playerDecisionEvent.Decision, networkPlayer, false));
            }

            await UniTask.Yield();
        }

        private async UniTask HandleDrawFromDeck(PlayerDecisionBettingEvent playerDecisionEvent, NetworkPlayer networkPlayer)
        {
            bool success = await EventBus.Instance.PublishAsync(new SetFloorCardEvent<Card>());
            if (success)
            {
                await EventBus.Instance.PublishAsync(new DecisionTakenEvent(playerDecisionEvent.Decision, networkPlayer, false));
            }
            await UniTask.Yield();
        }
    }
}
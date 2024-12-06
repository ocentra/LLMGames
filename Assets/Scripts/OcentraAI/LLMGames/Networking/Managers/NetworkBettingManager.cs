using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GamesNetworking;
using UnityEngine;


namespace OcentraAI.LLMGames.Networking.Manager
{
    public partial class NetworkBettingManager : NetworkManagerBase
    {
        
        public override void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<ProcessDecisionEvent>(OnProcessDecisionEvent);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<ProcessDecisionEvent>(OnProcessDecisionEvent);
        }

        protected async UniTask OnProcessDecisionEvent(ProcessDecisionEvent processDecisionEvent)
        {
            if (!IsServer) return;

            if (NetworkPlayerManager.TryGetPlayer(processDecisionEvent.PlayerId, out IPlayerBase playerBase))
            {
                PlayerDecisionEvent playerDecisionEvent = processDecisionEvent.DecisionEvent;
                PlayerDecision decision = PlayerDecision.FromId(playerDecisionEvent.Decision.DecisionId);

                GameLoggerScriptable.Log($" {playerBase.PlayerName.Value.Value} PlayerDecision {decision.Name} Processed ", this, ToEditor, ToFile, UseStackTrace);

                NetworkPlayer networkPlayer = playerBase as NetworkPlayer;
                if (networkPlayer == null)
                {
                    GameLoggerScriptable.LogError("The playerBase is not of type NetworkPlayer.", this, ToEditor, ToFile, UseStackTrace);
                    return;

                }



                switch (decision.Name)
                {
                    case nameof(PlayerDecision.RaiseBet):
                        await HandleRaiseBet(playerDecisionEvent as PlayerDecisionRaiseBetEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.SeeHand):
                        await HandleSeeHand(playerDecisionEvent as PlayerDecisionBettingEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.DrawFromDeck):
                        await HandleDrawFromDeck(playerDecisionEvent as PlayerDecisionBettingEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.PickAndSwap):
                        await HandlePickAndSwap(playerDecisionEvent as PlayerDecisionPickAndSwapEvent, networkPlayer);
                        break;


                    case nameof(PlayerDecision.PlayBlind):
                        await HandlePlayBlind(playerDecisionEvent as PlayerDecisionBettingEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.Bet):
                        await HandleBet(playerDecisionEvent as PlayerDecisionBettingEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.Fold):
                        await HandleFold(playerDecisionEvent as PlayerDecisionBettingEvent, networkPlayer);
                        break;
                        
                    case nameof(PlayerDecision.ShowCall):
                        await HandleShowCall(playerDecisionEvent as PlayerDecisionBettingEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.WildCard0):
                        await HandleWildCard0(playerDecisionEvent as PlayerDecisionWildcardEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.WildCard1):
                        await HandleWildCard1(playerDecisionEvent as PlayerDecisionWildcardEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.WildCard2):
                        await HandleWildCard2(playerDecisionEvent as PlayerDecisionWildcardEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.WildCard3):
                        await HandleWildCard3(playerDecisionEvent as PlayerDecisionWildcardEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.Trump):
                        await HandleTrump(playerDecisionEvent as PlayerDecisionWildcardEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.ShowAllFloorCards):
                        await HandleShowAllFloorCards(playerDecisionEvent as PlayerDecisionUIEvent, networkPlayer);
                        break;

                    case nameof(PlayerDecision.PurchaseCoins):
                        await HandlePurchaseCoins(playerDecisionEvent as PlayerDecisionUIEvent, networkPlayer);
                        break;

                    default:
                        Debug.LogWarning($"Unhandled decision: {decision.Name}");
                        break;
                }
            }

            await UniTask.Yield();
        }

        

    }
}
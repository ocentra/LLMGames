using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.UI
{
    public class PlayerDecisionButton : Button3D
    {
        [Header("Player Decision Settings")]
        [SerializeField, ShowInInspector, ValueDropdown(nameof(GetAvailableDecisions)), PropertyOrder(-1)]
        [Tooltip("The PlayerDecision this button represents.")]
        private int decisionId = 0;

        public IEnumerable<ValueDropdownItem<int>> GetAvailableDecisions()
        {
            List<ValueDropdownItem<int>> dropdownItems = new List<ValueDropdownItem<int>>();

            foreach (PlayerDecision decision in PlayerDecision.GetAllDecisions())
            {
                dropdownItems.Add(new ValueDropdownItem<int>(decision.Name, decision.DecisionId));
            }

            return dropdownItems;
        }

        public PlayerDecision PlayerDecision { get => PlayerDecision.FromId(decisionId); set => decisionId = value.DecisionId; }
        [SerializeField, ShowInInspector] protected UIManager UIManager;
        protected override void Init()
        {
            if (UIManager == null)
            {
                UIManager = FindFirstObjectByType<UIManager>();
            }
            base.Init();
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable) return;

            PlayerDecisionEvent eventToPublish = null;

            switch (PlayerDecision.Name)
            {
                // Special case: RaiseBet with a custom event
                case nameof(PlayerDecision.RaiseBet):

                    if (UIManager.TryGetRaiseAmount(out int raiseAmount))
                    {
                        eventToPublish = new PlayerDecisionRaiseBetEvent(PlayerDecision, raiseAmount);
                       
                    }

                    break;

                case nameof(PlayerDecision.SeeHand):
                    eventToPublish = new PlayerDecisionBettingEvent(PlayerDecision);
                    break;

                // General betting decisions
                case nameof(PlayerDecision.ShowCall):
                case nameof(PlayerDecision.Fold):
                case nameof(PlayerDecision.PlayBlind):
                case nameof(PlayerDecision.Bet):
                case nameof(PlayerDecision.DrawFromDeck):
                    eventToPublish = new PlayerDecisionBettingEvent(PlayerDecision);
                    break;

                // Wildcard-related decisions
                case nameof(PlayerDecision.WildCard0):
                case nameof(PlayerDecision.WildCard1):
                case nameof(PlayerDecision.WildCard2):
                case nameof(PlayerDecision.WildCard3):
                case nameof(PlayerDecision.Trump):
                    eventToPublish = new PlayerDecisionWildcardEvent(PlayerDecision);
                    break;

                // UI-related decisions
                case nameof(PlayerDecision.ShowAllFloorCards):
                case nameof(PlayerDecision.PurchaseCoins):
                    eventToPublish = new PlayerDecisionUIEvent(PlayerDecision);
                    break;

                // Default fallback
                default:
                    eventToPublish = new PlayerDecisionEvent(PlayerDecision);
                    break;
            }


            if (eventToPublish != null)
            {
                UniTask<bool> success = EventBus.Instance.PublishAsync(eventToPublish);
            }

            base.OnPointerClick(eventData);


        }
        


        #region temp

        // temp for copying values can delete or comment later


        [Button]
        public void CopyFromExistingButton3D()
        {
            Button3D existingButton = GetComponent<Button3D>();

            if (existingButton == null || existingButton == this)
            {
                Debug.LogWarning("No valid Button3D found to copy from.");
                return;
            }

            // Serialize the state of the existing Button3D
            Dictionary<string, object> serializedState = existingButton.SerializeToDictionary();

            // Apply the state to this PlayerDecisionButton
            DeserializeFromDictionary(serializedState);

            Debug.Log($"Successfully copied state from {existingButton.name} to {name}.");
        }

        #endregion


    }
}
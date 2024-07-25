using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class RoundLeaderboardScreen : UIScreen<RoundLeaderboardScreen>
    {
        [ShowInInspector, Required] private Button NewGame { get; set; }

        [ShowInInspector, Required] private Button ContinueRound { get; set; }

        [Required, ShowInInspector] private TextMeshProUGUI Message { get; set; }

        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        void OnValidate()
        {
            Init();
            InitReferences();
        }

        void OnEnable()
        {
            EventBus.Subscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Subscribe<OfferNewGame>(OnOfferNewGame);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Unsubscribe<OfferNewGame>(OnOfferNewGame);
        }


        private void InitReferences()
        {
            NewGame = transform.FindChildRecursively<Button>(nameof(NewGame));
            ContinueRound = transform.FindChildRecursively<Button>(nameof(ContinueRound));
            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));

            if (NewGame != null)
            {
                NewGame.onClick.AddListener(OnNewGame);
                NewGame.gameObject.SetActive(false);
            }
            if (ContinueRound != null)
            {
                ContinueRound.onClick.AddListener(OnContinueRound);
                ContinueRound.gameObject.SetActive(false);
            }
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
           // Debug.Log("RoundLeaderboardScreen is shown");
        }

        public override void OnHideScreen(bool first)
        {
            base.OnHideScreen(first);
           // Debug.Log("RoundLeaderboardScreen is hidden");
        }



        private void OnOfferContinuation(OfferContinuation e)
        {
            ShowScreen();

            ShowMessage(e.Message, e.Delay);
            if (ContinueRound != null)
            {
                ContinueRound.gameObject.SetActive(true);
            }

            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(false);
            }
        }

        private void OnOfferNewGame(OfferNewGame e)
        {
            ShowScreen();

            ShowMessage(e.Message, e.Delay);

            if (ContinueRound != null)
            {
                ContinueRound.gameObject.SetActive(false);
            }

            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(true);
            }

        }

        private void ShowMessage(string message, float delay = 5f)
        {

            if (Message != null)
            {
                Message.text = message;
            }
            // StartCoroutine(HideMessageAfterDelay(delay));

        }

        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideScreen();
        }

        private void OnContinueRound()
        {
            PlaySelectionSound(); //todo
            EventBus.Publish(new PlayerActionNewRound());
            HideScreen();
        }

        private void OnNewGame()
        {
            PlaySelectionSound();
            EventBus.Publish(new PlayerActionStartNewGame());
            HideScreen();
        }
    }
}
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class RoundLeaderboardScreen : UIScreen<RoundLeaderboardScreen>
    {
        [ShowInInspector, Required] private Button NewGame { get; set; }

        [ShowInInspector, Required] private Button ContinueRound { get; set; }

        [Required, ShowInInspector] private GameObject RoundStats { get; set; }
        [Required, ShowInInspector] private GameObject Headers { get; set; }
        [Required, ShowInInspector] private GameObject Empty { get; set; }

        [Required, ShowInInspector] private Transform ScoreHolderContent { get; set; }


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
            ScoreHolderContent = transform.FindChildRecursively<Transform>(nameof(ScoreHolderContent));
            NewGame = transform.FindChildRecursively<Button>(nameof(NewGame));
            ContinueRound = transform.FindChildRecursively<Button>(nameof(ContinueRound));
            RoundStats = Resources.Load<GameObject>($"Prefabs/{nameof(RoundStats)}");
            Headers = Resources.Load<GameObject>($"Prefabs/{nameof(Headers)}");
            Empty = Resources.Load<GameObject>($"Prefabs/{nameof(Empty)}");


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

            ShowStats(e);
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

            ShowStats(e);

            if (ContinueRound != null)
            {
                ContinueRound.gameObject.SetActive(false);
            }

            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(true);
            }

        }

        private void ShowStats(OfferNewGame e)
        {
            //string message = ColouredMessage("Game Over!", Color.red) +
            //                 ColouredMessage($"{winner.PlayerName}", Color.white, true) +
            //                 ColouredMessage($"wins the game with {winCount} rounds!", Color.cyan) +
            //                 $"{Environment.NewLine}" +
            //                 ColouredMessage("Play New Game of 10 rounds ?", Color.red, true);


            ScoreHolderContent.DestroyChild();


            GameObject headers = Instantiate(Headers, ScoreHolderContent);
            headers.transform.SetAsFirstSibling();


            //foreach (var VARIABLE in e.GameManager.ScoreManager)
            //{
                
            //}
            GameObject roundStatGameObject = Instantiate(RoundStats, ScoreHolderContent);
            var roundStats = roundStatGameObject.GetComponent<RoundStats>();
            roundStats.ShowStat(e);

            GameObject empty = Instantiate(Empty, ScoreHolderContent);
            empty.transform.SetAsLastSibling();

            //  StartCoroutine(HideMessageAfterDelay(e.Delay));

        }

        private void ShowStats(OfferContinuation e)
        {
            ScoreHolderContent.DestroyChild();

            GameObject headers = Instantiate(Headers, ScoreHolderContent);
            headers.transform.SetAsFirstSibling();

            GameObject roundStatGameObject = Instantiate(RoundStats, ScoreHolderContent);
            var roundStats = roundStatGameObject.GetComponent<RoundStats>();
            roundStats.ShowStat(e);

            GameObject empty = Instantiate(Empty, ScoreHolderContent);
            empty.transform.SetAsLastSibling();

            // StartCoroutine(HideMessageAfterDelay(e.Delay));
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
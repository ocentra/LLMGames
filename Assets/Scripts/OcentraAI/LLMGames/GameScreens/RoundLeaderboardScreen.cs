using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static OcentraAI.LLMGames.Utility;

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

        [Required, ShowInInspector] private TextMeshProUGUI HeadingText { get; set; }
        
        [Required, ShowInInspector] private TextMeshProUGUI EndingText { get; set; }
        private bool isButtonClicked = false;


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
            HeadingText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HeadingText));
            EndingText = transform.transform.FindChildRecursively<TextMeshProUGUI>(nameof(EndingText));
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

            if (HeadingText !=null)
            {
                HeadingText.text = $"Round Over";
            }

            RoundRecord roundRecord = ScoreManager.GetLastRound();

           

            Player winner = roundRecord.Winner;

            string message = ColouredMessage($"{winner.PlayerName} ", Color.green) + ColouredMessage($"Won the Round Pot: ", Color.white) + ColouredMessage($" {roundRecord.PotAmount} Coins ", Color.yellow) +
                             $"{Environment.NewLine}" + ColouredMessage("Remaining Rounds : ", Color.white) + ColouredMessage($"{TurnManager.MaxRounds - TurnManager.CurrentRound} ", Color.cyan) + ColouredMessage(" Continue Next rounds ?", Color.white) ;

            ShowStats(roundRecord);

            if (EndingText != null)
            {
                EndingText.text = message;
            }


            if (ContinueRound != null)
            {
                ContinueRound.gameObject.SetActive(true);

            }

            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(false);

            }

            isButtonClicked = false;

        }



        private void OnOfferNewGame(OfferNewGame e)
        {
            ShowScreen();

            if (HeadingText != null)
            {
                HeadingText.text = $"Game Over";
            }

            ShowStats(ScoreManager.GetRoundRecords());

            (string winnerId, int winCount) = ScoreManager.GetOverallWinner();
            Player winner = winnerId == PlayerManager.HumanPlayer.Id ? PlayerManager.HumanPlayer : PlayerManager.ComputerPlayer;

            string message = ColouredMessage($"{winner.PlayerName}", Color.white, true) +
                             ColouredMessage($"wins the game with {winCount} rounds!", Color.cyan) +
                             $"{Environment.NewLine}" +
                             ColouredMessage("Play New Game of 10 rounds ?", Color.red, true);

            if (EndingText != null)
            {
                EndingText.text = message;
            }

            if (ContinueRound != null)
            {
                ContinueRound.gameObject.SetActive(false);
               
            }

            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(true);
              
            }

            isButtonClicked = false;
        }

        private void ShowStats(List<RoundRecord> roundRecords)
        {
            
            ScoreHolderContent.DestroyChildren();
            GameObject headers = Instantiate(Headers, ScoreHolderContent);
            headers.transform.SetAsFirstSibling();


            foreach (RoundRecord roundRecord in roundRecords)
            {
                GameObject roundStatGameObject = Instantiate(RoundStats, ScoreHolderContent);
                RoundStats roundStats = roundStatGameObject.GetComponent<RoundStats>();
                if (roundStats!=null)
                {
                    roundStats.Init();

                    roundStats.ShowStat(roundRecord);

                }
            }


            GameObject empty = Instantiate(Empty, ScoreHolderContent);
            empty.transform.SetAsLastSibling();

            //  StartCoroutine(HideMessageAfterDelay(e.Delay));

        }

        private void ShowStats(RoundRecord roundRecord)
        {
            ScoreHolderContent.DestroyChildren();

            GameObject headers = Instantiate(Headers, ScoreHolderContent);
            headers.transform.SetAsFirstSibling();

            GameObject roundStatGameObject = Instantiate(RoundStats, ScoreHolderContent);
            RoundStats roundStats = roundStatGameObject.GetComponent<RoundStats>();

            if (roundStats != null)
            {
                roundStats.Init();
                roundStats.ShowStat(roundRecord);

            }


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
            if (isButtonClicked) return;

            isButtonClicked = true;
            PlaySelectionSound();
            EventBus.Publish(new PlayerActionNewRound());
            HideScreen();
        }

        private void OnNewGame()
        {
            if (isButtonClicked) return;

            isButtonClicked = true;
            PlaySelectionSound();
            EventBus.Publish(new PlayerActionStartNewGame());
            HideScreen();
        }


    }
}
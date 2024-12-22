using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static OcentraAI.LLMGames.Utilities.Utility;


namespace OcentraAI.LLMGames.Screens
{
    public class RoundLeaderboardScreen : UI3DScreen<RoundLeaderboardScreen>
    {


        [Required, ShowInInspector] private bool IsButtonClicked { get; set; }
        [Required, ShowInInspector] private Button NewGame { get; set; }
        [Required, ShowInInspector] private Button ContinueRound { get; set; }
        [Required, ShowInInspector] private GameObject RoundStats { get; set; }
        [Required, ShowInInspector] private GameObject Headers { get; set; }
        [Required, ShowInInspector] private GameObject Empty { get; set; }
        [Required, ShowInInspector] private Transform ScoreHolderContent { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI HeadingText { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI EndingText { get; set; }
        [OdinSerialize, ShowInInspector] private float SpacerHeight { get; set; } = 25f;
        [OdinSerialize, ShowInInspector] private float RoundStatHeight { get; set; } = 150f;

        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            InitReferences();
        }
        

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<OfferContinuationEvent>(OnOfferContinuation);
            EventRegistrar.Subscribe<OfferNewGameEvent>(OnOfferNewGame);
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


            VerticalLayoutGroup verticalLayout = ScoreHolderContent.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = ScoreHolderContent.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            verticalLayout.spacing = SpacerHeight;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childControlHeight = true;

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



        private void OnOfferContinuation(OfferContinuationEvent e)
        {
            ShowScreen();

            if (HeadingText != null)
            {
                HeadingText.text = "Round Over";
            }

            INetworkRoundRecord roundRecord = e.RoundRecord;


            string message = ColouredMessage($"{roundRecord.Winner} ", Color.green) +
                             ColouredMessage("Won the Round Pot: ", Color.white) +
                             ColouredMessage($" {roundRecord.PotAmount} Coins ", Color.yellow) +
                             $"{Environment.NewLine}" + ColouredMessage("Remaining Rounds : ", Color.white) +
                             ColouredMessage($"[{roundRecord.MaxRounds} - {roundRecord.RoundNumber}] = {roundRecord.MaxRounds - roundRecord.RoundNumber} ", Color.cyan) +
                             ColouredMessage(" Continue Next rounds ?", Color.white);

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

            IsButtonClicked = false;
        }


        private void OnOfferNewGame(OfferNewGameEvent e)
        {
            ShowScreen();


            if (HeadingText != null)
            {
                HeadingText.text = "Game Over";
            }
            List<INetworkRoundRecord> roundRecord = e.RoundRecord;

            ShowStats(roundRecord);


            (IPlayerBase OverallWinner, int WinCount) roundRecordOverallWinner = e.OverallWinner;

            string message = ColouredMessage($"{roundRecordOverallWinner.OverallWinner.PlayerName}", Color.white, true) +
                             ColouredMessage($"wins the game with {roundRecordOverallWinner.WinCount} rounds!", Color.cyan) +
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

            IsButtonClicked = false;
        }


        private void ShowStats(List<INetworkRoundRecord> roundRecords)
        {
            ScoreHolderContent.DestroyChildren();

            // Create headers once
            GameObject headers = Instantiate(Headers, ScoreHolderContent);
            headers.transform.SetAsFirstSibling();

            foreach (INetworkRoundRecord record in roundRecords)
            {
                GameObject roundStatGameObject = Instantiate(RoundStats, ScoreHolderContent);
                RoundStats roundStats = roundStatGameObject.GetComponent<RoundStats>();

                if (roundStats != null)
                {
                    GameObject spacer = new GameObject("Spacer");
                    RectTransform spacerRect = spacer.AddComponent<RectTransform>();
                    spacerRect.SetParent(ScoreHolderContent, false);
                    spacerRect.sizeDelta = new Vector2(0, SpacerHeight);

                    roundStats.ShowStat(record);

                    RectTransform rectTransform = roundStatGameObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, RoundStatHeight);
                    }
                }
            }

            GameObject empty = Instantiate(Empty, ScoreHolderContent);
            empty.transform.SetAsLastSibling();
        }




        private void ShowStats(INetworkRoundRecord roundRecord)
        {
            ScoreHolderContent.DestroyChildren();

            GameObject headers = Instantiate(Headers, ScoreHolderContent);
            headers.transform.SetAsFirstSibling();

            GameObject roundStatGameObject = Instantiate(RoundStats, ScoreHolderContent);
            RoundStats roundStats = roundStatGameObject.GetComponent<RoundStats>();

            if (roundStats != null)
            {
                GameObject spacer = new GameObject("Spacer");
                RectTransform spacerRect = spacer.AddComponent<RectTransform>();
                spacerRect.SetParent(ScoreHolderContent, false);
                spacerRect.sizeDelta = new Vector2(0, SpacerHeight);

                roundStats.ShowStat(roundRecord);

                RectTransform rectTransform = roundStatGameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, RoundStatHeight);
                }
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
            if (IsButtonClicked)
            {
                return;
            }

            IsButtonClicked = true;
            PlaySelectionSound();
            EventBus.Instance.Publish(new PlayerActionNewRoundEvent());
            HideScreen();
        }

        private void OnNewGame()
        {
            if (IsButtonClicked)
            {
                return;
            }

            IsButtonClicked = true;
            PlaySelectionSound();
            EventBus.Instance.Publish(new PlayerActionStartNewGameEvent());
            HideScreen();
        }
    }
}
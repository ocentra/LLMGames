using OcentraAI.LLMGames.Manager;
using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    public abstract class BaseBonusRule : SerializedScriptableObject
    {
        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public abstract int MinNumberOfCard { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract int BonusValue { get; protected set; }
        [OdinSerialize] [ShowInInspector] public abstract int Priority { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public abstract string RuleName { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public string Description { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public GameMode GameMode { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        public GameRulesContainer Examples { get; protected set; } = new GameRulesContainer();

        private bool IsGameModeAssigned()
        {
            return GameMode != null;
        }


        public void UpdateRule(int bonusValue, int priority)
        {
            BonusValue = bonusValue;
            Priority = priority;
        }

        public bool SetGameMode(GameMode gameMode)
        {
            GameMode = gameMode;
            return Initialize(gameMode);
        }


        [Button(ButtonSizes.Large)]
        [ShowIf(nameof(IsGameModeAssigned))]
        public void Initialize()
        {
            Initialize(GameMode);
        }

        public abstract bool Initialize(GameMode gameMode);
        public abstract bool Evaluate(Hand hand, out BonusDetail bonusDetail);

        public abstract string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true);


        protected Card GetTrumpCard()
        {
            return DeckManager.Instance.WildCards.GetValueOrDefault("TrumpCard");
            //todo put it in some const class no string literals
        }

        protected BonusDetail CreateBonusDetails(string ruleName, int baseBonus, int priority,
            List<string> descriptions, string bonusCalculationDescriptions, int additionalBonus = 0)
        {
            return new BonusDetail
            {
                RuleName = ruleName,
                BaseBonus = baseBonus,
                AdditionalBonus = additionalBonus,
                BonusDescriptions = descriptions,
                Priority = priority,
                BonusCalculationDescriptions = bonusCalculationDescriptions
            };
        }

        protected bool TryCreateExample(string ruleName, string description, int bonusValue,
            List<string> playerExamples,
            List<string> llmExamples, List<string> playerTrumpExamples,
            List<string> llmTrumpExamples, bool useTrump)
        {
            bool IsApplicable(List<string> examplesList)
            {
                return examplesList is {Count: > 0};
            }

            string GetExamplesDescription(List<string> examples, List<string> trumpExamples, bool useTrumpExamples)
            {
                if (!IsApplicable(examples))
                {
                    return "Rule Not Applicable to this GameMode";
                }

                string examplesDescription = string.Join($"{Environment.NewLine}", examples);
                if (useTrumpExamples && IsApplicable(trumpExamples))
                {
                    examplesDescription += $"{Environment.NewLine}Trump Examples:{Environment.NewLine}" +
                                           $"{string.Join($"{Environment.NewLine}", trumpExamples)}{Environment.NewLine}";
                }

                return examplesDescription;
            }

            bool hasPlayerExamples = IsApplicable(playerExamples);
            bool hasLlmExamples = IsApplicable(llmExamples);

            if (!hasPlayerExamples && !hasLlmExamples)
            {
                return false;
            }

            string playerDescription = $"{ruleName} {description}{Environment.NewLine}" +
                                       $"{ruleName} Bonus: {bonusValue}{Environment.NewLine}" +
                                       $"Examples:{Environment.NewLine}" +
                                       $"{GetExamplesDescription(playerExamples, playerTrumpExamples, useTrump)}";

            string llmDescription = $"{ruleName} {description}{Environment.NewLine}" +
                                    $"{ruleName} Bonus: {bonusValue}{Environment.NewLine}" +
                                    $"Examples:{Environment.NewLine}" +
                                    $"{GetExamplesDescription(llmExamples, llmTrumpExamples, useTrump)}";

            Examples = new GameRulesContainer {Player = playerDescription, LLM = llmDescription};
            return true;
        }
    }
}
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.ThreeCardBrag.Rules;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;

namespace OcentraAI.LLMGames.GameModes
{
    public abstract class GameMode : SerializedScriptableObject
    {
        #region Fields and Properties

        // abstract props
        [OdinSerialize, ShowInInspector] public abstract int MaxRounds { get; protected set; }

        [OdinSerialize, ShowInInspector] public abstract float TurnDuration { get; protected set; }

        [OdinSerialize, ShowInInspector] public abstract int InitialPlayerCoins { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly] public abstract string GameName { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int MinPlayers { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int MaxPlayers { get; protected set; }
        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int NumberOfCards { get; protected set; }

        [OdinSerialize, ShowInInspector] public abstract bool UseTrump { get; protected set; } 

        // public props

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer GameRules { get; protected set; } 

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer GameDescription { get; protected set; } 

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer StrategyTips { get; protected set; } 

        [ShowInInspector]
        public List<BaseBonusRule> BonusRules { get; protected set; } = new List<BaseBonusRule>();

        [ShowInInspector, ReadOnly]
        public CardRanking[] CardRankings { get; protected set; }

        [ShowInInspector, ReadOnly]
        public TrumpBonusValues TrumpBonusValues { get; protected set; } = new TrumpBonusValues();

        // protected props 

        [OdinSerialize, ShowInInspector, ReadOnly]
        protected Dictionary<PossibleMoves, string> MoveValidityConditions { get; set; }

        [OdinSerialize, ShowInInspector, ReadOnly]
        protected Dictionary<DifficultyLevels, string> BluffSettingConditions { get; set; }

        [OdinSerialize, ShowInInspector, ReadOnly]
        protected Dictionary<HandType, string> ExampleHandOdds { get; set; }



        #endregion

        #region Initialization Methods


    
        public abstract void Initialize(List<BaseBonusRule> bonusRulesTemplate);

        protected virtual void InitializeBonusRules(List<BaseBonusRule> bonusRulesTemplate)
        {
            string gameModePath = AssetDatabase.GetAssetPath(this);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(gameModePath);
            List<BaseBonusRule> existingChildRules = assets.OfType<BaseBonusRule>().Where(r => r.name != string.Empty).ToList();

            Dictionary<string, BaseBonusRule> existingRulesDict = existingChildRules.ToDictionary(r => r.name);
            Dictionary<string, BaseBonusRule> templateRulesDict = bonusRulesTemplate.ToDictionary(r => r.name);

            List<BaseBonusRule> updatedRules = new List<BaseBonusRule>();

            foreach (BaseBonusRule templateRule in bonusRulesTemplate)
            {
                if (existingRulesDict.TryGetValue(templateRule.name, out BaseBonusRule existingRule))
                {
                    existingRule.UpdateRule(templateRule);
                    updatedRules.Add(existingRule);
                }
                else
                {
                    BaseBonusRule newRule = Instantiate(templateRule);
                    newRule.name = templateRule.RuleName;
                    newRule.SetGameMode(this);
                    updatedRules.Add(newRule);
                    AssetDatabase.AddObjectToAsset(newRule, this);
                }
            }

            foreach (BaseBonusRule existingRule in existingChildRules)
            {
                if (!templateRulesDict.ContainsKey(existingRule.name))
                {
                    DestroyImmediate(existingRule, true);
                }
            }

            BonusRules = updatedRules;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }





        protected virtual void InitializeGameRules()
        {
            GameRules = new GameRulesContainer();
        }

        protected virtual void InitializeGameDescription()
        {
            GameDescription = new GameRulesContainer();
        }

        protected virtual void InitializeStrategyTips()
        {
            StrategyTips = new GameRulesContainer();
        }

        protected virtual void InitializeMoveValidityConditions()
        {
            MoveValidityConditions = new Dictionary<PossibleMoves, string>();
        }

        protected virtual void InitializeBluffSettingConditions()
        {
            BluffSettingConditions = new Dictionary<DifficultyLevels, string>();
        }

        protected virtual void InitializeExampleHandOdds()
        {
            ExampleHandOdds = new Dictionary<HandType, string>();
        }

        protected virtual void InitializeCardRankings()
        {
            CardRankings = new CardRanking[]
            {
                new CardRanking { CardName = "A", Value = 14 },
                new CardRanking { CardName = "K", Value = 13 },
                new CardRanking { CardName = "Q", Value = 12 },
                new CardRanking { CardName = "J", Value = 11 },
                new CardRanking { CardName = "10", Value = 10 },
                new CardRanking { CardName = "9", Value = 9 },
                new CardRanking { CardName = "8", Value = 8 },
                new CardRanking { CardName = "7", Value = 7 },
                new CardRanking { CardName = "6", Value = 6 },
                new CardRanking { CardName = "5", Value = 5 },
                new CardRanking { CardName = "4", Value = 4 },
                new CardRanking { CardName = "3", Value = 3 },
                new CardRanking { CardName = "2", Value = 2 }
            };
        }

        protected void InitializeGameMode(List<BaseBonusRule> bonusRulesTemplate)
        {
            InitializeBonusRules(bonusRulesTemplate);
            InitializeGameRules();
            InitializeGameDescription();
            InitializeStrategyTips();
            InitializeCardRankings();
            InitializeMoveValidityConditions();
            InitializeBluffSettingConditions();
            InitializeExampleHandOdds();
            SaveChanges();
        }

        #endregion

        #region Public Methods

        public Dictionary<DifficultyLevels, string> GetBluffSettingConditions()
        {
            return BluffSettingConditions;
        }

        public Dictionary<HandType, string> GetExampleHandOdds()
        {
            return ExampleHandOdds;
        }

        public Dictionary<PossibleMoves, string> GetMoveValidityConditions()
        {
            return MoveValidityConditions;
        }

        public T GetBonusRule<T>() where T : BaseBonusRule
        {
            return BonusRules.OfType<T>().FirstOrDefault();
        }

        #endregion

        #region Utility Methods

#if UNITY_EDITOR
        public virtual void SaveChanges()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif

        #endregion
    }
}

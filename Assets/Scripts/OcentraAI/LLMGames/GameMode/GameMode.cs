using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.ThreeCardBrag.Rules;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    public abstract class GameMode : SerializedScriptableObject
    {
        #region Fields and Properties

        // abstract props
        [OdinSerialize, ShowInInspector] public abstract int MaxRounds { get; protected set; }

        [OdinSerialize, ShowInInspector] public abstract float TurnDuration { get; protected set; }

        [OdinSerialize, ShowInInspector] public abstract int InitialPlayerCoins { get; protected set; }
        [OdinSerialize, ShowInInspector] public abstract bool UseTrump { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly] public abstract string GameName { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int MinPlayers { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int MaxPlayers { get; protected set; }
        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int NumberOfCards { get; protected set; }



        // public props

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer GameRules { get; protected set; } 

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer GameDescription { get; protected set; } 

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer StrategyTips { get; protected set; } 

        [OdinSerialize,ShowInInspector]
        public List<BaseBonusRule> BonusRules { get; protected set; } = new List<BaseBonusRule>();

        [OdinSerialize,ShowInInspector, ReadOnly]
        public CardRanking[] CardRankings { get; protected set; }

        [OdinSerialize, ShowInInspector, ReadOnly]
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


    
        public abstract bool TryInitialize(List<BaseBonusRule> bonusRulesTemplate);

        protected virtual bool TryInitializeBonusRules(List<BaseBonusRule> bonusRulesTemplate)
        {
            string gameModePath = AssetDatabase.GetAssetPath(this);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(gameModePath);
            List<BaseBonusRule> existingChildRules = assets.OfType<BaseBonusRule>().Where(r => r.RuleName != string.Empty).ToList();

            // Ensure unique keys for existing rules
            Dictionary<string, BaseBonusRule> existingRulesDict = new Dictionary<string, BaseBonusRule>();
            foreach (var rule in existingChildRules)
            {
                if (!existingRulesDict.TryAdd(rule.RuleName, rule))
                {
                    Debug.LogWarning($"Duplicate rule name found in existing rules: {rule.RuleName}. Skipping duplicate.");
                }
            }

            // Ensure unique keys for template rules
            Dictionary<string, BaseBonusRule> templateRulesDict = new Dictionary<string, BaseBonusRule>();
            foreach (var rule in bonusRulesTemplate)
            {
                if (!templateRulesDict.TryAdd(rule.RuleName, rule))
                {
                    Debug.LogWarning($"Duplicate rule name found in template rules: {rule.RuleName}. Skipping duplicate.");
                }
            }

            List<BaseBonusRule> updatedRules = new List<BaseBonusRule>();

            foreach (BaseBonusRule templateRule in bonusRulesTemplate)
            {
                if (existingRulesDict.TryGetValue(templateRule.RuleName, out BaseBonusRule existingRule))
                {
                    existingRule.UpdateRule(templateRule);
                    updatedRules.Add(existingRule);
                }
                else
                {
                    BaseBonusRule newRule = Instantiate(templateRule);
                    newRule.name = templateRule.RuleName;
                    if (newRule.SetGameMode(this))
                    {
                        updatedRules.Add(newRule);
                        AssetDatabase.AddObjectToAsset(newRule, this);
                    }
                    else
                    {
                        return false;
                    }


                }
            }

            foreach (BaseBonusRule existingRule in existingChildRules)
            {
                if (!templateRulesDict.ContainsKey(existingRule.RuleName))
                {
                    DestroyImmediate(existingRule, true);
                }
            }

            BonusRules = updatedRules;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
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

        protected bool TryInitializeGameMode(List<BaseBonusRule> bonusRulesTemplate)
        {
            if (!TryInitializeBonusRules(bonusRulesTemplate))
            {
                return false;
            }

            
            InitializeGameRules();
            InitializeGameDescription();
            InitializeStrategyTips();
            InitializeCardRankings();
            InitializeMoveValidityConditions();
            InitializeBluffSettingConditions();
            InitializeExampleHandOdds();
            SaveChanges();

            return true;
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

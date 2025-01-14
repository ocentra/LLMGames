using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Manager.Utilities;
using OcentraAI.LLMGames.ThreeCardBrag.Rules;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static System.String;

namespace OcentraAI.LLMGames.GameModes
{
    public abstract class GameMode : SerializedScriptableObject, ISaveScriptable
    {
        #region Utility Methods

        [Button]
        public virtual void SaveChanges()
        {
#if UNITY_EDITOR
            EditorSaveManager.RequestSave(this).Forget();
#endif
        }

        #endregion

        #region Fields and Properties
      
        [SerializeField, ShowInInspector, ValueDropdown(nameof(GetAvailableGameModeType)), PropertyOrder(-1)]
        [Tooltip("The GameModeType this button represents.")]
        private int id = 0;

        public IEnumerable<ValueDropdownItem<int>> GetAvailableGameModeType()
        {
            List<ValueDropdownItem<int>> dropdownItems = new List<ValueDropdownItem<int>>();

            foreach (GameModeType genre in GameModeType.GetAll())
            {
                dropdownItems.Add(new ValueDropdownItem<int>(genre.Name, genre.Id));
            }

            return dropdownItems;
        }
        [ShowInInspector, PropertyOrder(-1)]
        public GameModeType GameModeType { get => GameModeType.FromId(id); set => id = value.Id; }


        [OdinSerialize,HideInInspector]
        public Dictionary<BaseBonusRule, CustomRuleState> BaseBonusRulesTemplate { get; set; } =
            new Dictionary<BaseBonusRule, CustomRuleState>();

        [OdinSerialize] [HideInInspector] public float Height { get; set; } = 100;

        [OdinSerialize]
        [HideInInspector]
        public SortingCriteria CurrentSortingCriteria { get; set; } = SortingCriteria.PriorityAscending;

        [FolderPath(AbsolutePath = true)]
        [ReadOnly]
        [OdinSerialize]
        [ShowInInspector]
        public string RulesTemplatePath { get; set; } = "Assets/Resources/GameMode";

        [OdinSerialize] [HideInInspector] public bool Foldout { get; set; }

        // abstract props
        [OdinSerialize] [ShowInInspector] public abstract int MaxRounds { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract float TurnDuration { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract int InitialPlayerCoins { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract int BaseBet { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract int BaseBlindMultiplier { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract bool UseTrump { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract bool UseMagicCards { get; protected set; }

        [OdinSerialize] [ShowInInspector] public abstract bool DrawFromDeckAllowed { get; protected set; }


        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public abstract string GameName { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public abstract int MaxPlayers { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public abstract int NumberOfCards { get; protected set; }


        // public props

        [OdinSerialize] [ShowInInspector] public GameRulesContainer GameRules { get; protected set; }

        [OdinSerialize] [ShowInInspector] public GameRulesContainer GameDescription { get; protected set; }

        [OdinSerialize] [ShowInInspector] public GameRulesContainer StrategyTips { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        public List<BaseBonusRule> BonusRules { get; protected set; } = new List<BaseBonusRule>();

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public CardRanking[] CardRankings { get; protected set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        public TrumpBonusValues TrumpBonusValues { get; protected set; } = new TrumpBonusValues();

        // protected props 

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        protected Dictionary<PossibleMoves, string> MoveValidityConditions { get; set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        protected Dictionary<DifficultyLevels, string> BluffSettingConditions { get; set; }

        [OdinSerialize]
        [ShowInInspector]
        [ReadOnly]
        protected Dictionary<HandType, string> ExampleHandOdds { get; set; }

        #endregion

        #region Initialization Methods

        public abstract bool TryInitialize();

        protected virtual bool TryInitializeBonusRules()
        {
#if UNITY_EDITOR

            List<BaseBonusRule> bonusRulesTemplate = new List<BaseBonusRule>();
            foreach (var ruleStatePair in BaseBonusRulesTemplate)
            {
                if (ruleStatePair.Value.IsSelected)
                {
                    bonusRulesTemplate.Add(ruleStatePair.Key);
                }
            }

            string gameModePath = AssetDatabase.GetAssetPath(this);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(gameModePath);

            // Remove any corrupt or invalid assets
            foreach (Object asset in assets)
            {
                if (asset is BaseBonusRule baseBonusRule)
                {
                    if (baseBonusRule.GameMode == null)
                    {
                        DestroyImmediate(asset, true);
                    }
                }

                if (asset == null || asset.GetType() == typeof(MonoScript) || IsNullOrEmpty(asset.name))
                {
                    Debug.LogWarning($"Found and removing invalid asset: {asset}");
                    DestroyImmediate(asset, true);
                }
            }

            assets = AssetDatabase.LoadAllAssetsAtPath(gameModePath);

            List<BaseBonusRule> existingChildRules =
                assets.OfType<BaseBonusRule>().Where(r => r.RuleName != Empty).ToList();

            // Ensure unique keys for existing rules
            Dictionary<string, BaseBonusRule> existingRulesDict = new Dictionary<string, BaseBonusRule>();
            foreach (var rule in existingChildRules)
            {
                if (!existingRulesDict.TryAdd(rule.RuleName, rule))
                {
                    Debug.LogWarning(
                        $"Duplicate rule name found in existing rules: {rule.RuleName}. Skipping duplicate.");
                }
            }

            // Ensure unique keys for template rules
            Dictionary<string, BaseBonusRule> templateRulesDict = new Dictionary<string, BaseBonusRule>();
            foreach (var rule in bonusRulesTemplate)
            {
                if (!templateRulesDict.TryAdd(rule.RuleName, rule))
                {
                    Debug.LogWarning(
                        $"Duplicate rule name found in template rules: {rule.RuleName}. Skipping duplicate.");
                }
            }

            List<BaseBonusRule> updatedRules = new List<BaseBonusRule>();

            foreach (BaseBonusRule templateRule in bonusRulesTemplate)
            {
                if (existingRulesDict.TryGetValue(templateRule.RuleName, out BaseBonusRule existingRule))
                {
                    existingRule.UpdateRule(templateRule.BonusValue, templateRule.Priority);
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
            SaveChanges();


#endif
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
            CardRankings = new[]
            {
                new CardRanking {CardName = "A", Value = 14}, new CardRanking {CardName = "K", Value = 13},
                new CardRanking {CardName = "Q", Value = 12}, new CardRanking {CardName = "J", Value = 11},
                new CardRanking {CardName = "10", Value = 10}, new CardRanking {CardName = "9", Value = 9},
                new CardRanking {CardName = "8", Value = 8}, new CardRanking {CardName = "7", Value = 7},
                new CardRanking {CardName = "6", Value = 6}, new CardRanking {CardName = "5", Value = 5},
                new CardRanking {CardName = "4", Value = 4}, new CardRanking {CardName = "3", Value = 3},
                new CardRanking {CardName = "2", Value = 2}
            };
        }

        protected bool TryInitializeGameMode()
        {
            if (!TryInitializeBonusRules())
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
    }
}
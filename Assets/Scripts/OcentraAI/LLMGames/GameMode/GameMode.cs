using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.ThreeCardBrag.Rules;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;

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
        public GameRulesContainer GameRules { get; protected set; } = new GameRulesContainer();

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer GameDescription { get; protected set; } = new GameRulesContainer();

        [OdinSerialize, ShowInInspector]
        public GameRulesContainer StrategyTips { get; protected set; } = new GameRulesContainer();

        [ShowInInspector, ReadOnly]
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

        protected abstract void Initialize();

        protected virtual void InitializeBonusRules() { }

        protected virtual void InitializeGameRules() { }

        protected virtual void InitializeGameDescription() { }

        protected virtual void InitializeStrategyTips() { }

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
                new CardRanking { CardName = "Ace", Value = 14 },
                new CardRanking { CardName = "King", Value = 13 },
                new CardRanking { CardName = "Queen", Value = 12 },
                new CardRanking { CardName = "Jack", Value = 11 },
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

        protected void InitializeGameMode()
        {
            InitializeBonusRules();
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
        [Button(ButtonSizes.Medium)]
        protected virtual void SaveChanges()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif

        #endregion
    }
}

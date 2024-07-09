using UnityEngine;
using System;
using System.Collections.Generic;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.LLMServices.Rules;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OcentraAI.LLMGames.Scriptable.ScriptableSingletons
{
    [CreateAssetMenu(fileName = nameof(GameInfo), menuName = "ThreeCardBrag/GameInfo")]
    [CustomGlobalConfig("Assets/Resources/")]
    public class GameInfo : CustomGlobalConfig<GameInfo>
    {
        [TextArea(10, 20)]
        public string GameRules;

        [TextArea(5, 10)]
        public string GameDescription;

        [TextArea(5, 10)]
        public string StrategyTips;

        [OdinSerialize, ShowInInspector]
        private Dictionary<PossibleMoves, string> moveValidityConditions = new Dictionary<PossibleMoves, string>();

        [OdinSerialize, ShowInInspector]
        private Dictionary<DifficultyLevels, string> bluffSettingConditions = new Dictionary<DifficultyLevels, string>();

        [OdinSerialize, ShowInInspector]
        private Dictionary<HandType, string> exampleHandOdds = new Dictionary<HandType, string>();

        [OdinSerialize, ShowInInspector]
        public BaseBonusRule[] BonusRules;

        [ShowInInspector]
        public CardRanking[] CardRankings;

        [ShowInInspector]
        public BonusValues CommonBonuses;

        [Button(ButtonSizes.Small)]
        public void InitializeGameInfo()
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

        private void InitializeCommonBonuses()
        {
            CommonBonuses = new BonusValues
            {
                TrumpCardBonus = 5,
                SameColorBonus = 5,
                WildCardBonus = 5,
                RankAdjacentBonus = 5
            };
        }

        private void InitializeBonusRules()
        {
            InitializeCommonBonuses();

            BonusRules = new BaseBonusRule[]
            {
                new StraightFlushRule(),
                new DifferentColorsSequenceRule(),
                new PairInHandRule(),
                new ThreeConsecutiveSameSuitRule(),
                new ThreeOfAKindRule(),
                new SameColorsSequenceRule(),
                
            };
        }



        private void InitializeGameRules()
        {
            GameRules = $"Three Card Brag Rules:{Environment.NewLine}{Environment.NewLine}" +
                $"1. Each player is dealt 3 cards at the start of each round.{Environment.NewLine}" +
                $"2. Players can bet blind or see their hand.{Environment.NewLine}" +
                $"3. Blind betting doubles the current bet.{Environment.NewLine}" +
                $"4. Players who have seen their hand bet double the current bet.{Environment.NewLine}" +
                $"5. Players can draw a new card to the floor.{Environment.NewLine}" +
                $"6. Players can pick and swap a card from the floor.{Environment.NewLine}" +
                $"7. Players can fold, call, or raise.{Environment.NewLine}" +
                $"8. Highest hand value wins (Ace high).{Environment.NewLine}" +
                $"9. In case of a tie, the highest card wins.{Environment.NewLine}" +
                $"10. The game ends when a player runs out of coins or after a set number of rounds.{Environment.NewLine}" +
                $"11. The trailing player can continue if they have more coins.{Environment.NewLine}" +
                $"12. Players start with 1000 coins.{Environment.NewLine}";
        }

        private void InitializeGameDescription()
        {
            GameDescription = $"Three Card Brag is an exciting card game that combines elements of poker and bluffing.{Environment.NewLine}" +
                              $"Players aim to make the best three-card hand while betting and bluffing their way to victory.{Environment.NewLine}" +
                              "Most interesting part is we will be playing against Large language model LLM i.e Chat GPT ";
        }

        private void InitializeStrategyTips()
        {
            StrategyTips = $"- Pay attention to your opponents' betting patterns.{Environment.NewLine}" +
                $"Use the blind betting option strategically to bluff or build the pot.{Environment.NewLine}" +
                $"Consider the odds of improving your hand when deciding to draw or swap cards.{Environment.NewLine}" +
                $"Don't be afraid to fold if you have a weak hand and the bets are high.{Environment.NewLine}" +
                $"Manage your coins wisely to stay in the game for multiple rounds.";
        }

        private void InitializeCardRankings()
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

        private void InitializeMoveValidityConditions()
        {
            moveValidityConditions = new Dictionary<PossibleMoves, string>
            {
                { PossibleMoves.Fold, "Always valid" },
                { PossibleMoves.Call, "Valid when there's a bet to call" },
                { PossibleMoves.Raise, "Valid when you have enough coins to raise" },
                { PossibleMoves.Check, "Valid when there's no bet to call" },
                { PossibleMoves.BetBlind, "Valid only if you haven't seen your hand" },
                { PossibleMoves.SeeHand, "Valid only if you haven't seen your hand" },
                { PossibleMoves.DrawFromDeck, "Valid when there's no floor card" },
                { PossibleMoves.PickFromFloor, "Valid when there's a floor card" },
                { PossibleMoves.SwapCard, "Valid after drawing or picking from floor" },
                { PossibleMoves.ShowHand, "Valid at any time, ends the round" }
            };
        }

        private void InitializeBluffSettingConditions()
        {
            bluffSettingConditions = new Dictionary<DifficultyLevels, string>
            {
                { DifficultyLevels.Easy, "Rarely bluff" },
                { DifficultyLevels.Medium, "Occasionally bluff when the pot odds are favorable" },
                { DifficultyLevels.Hard, "Frequently bluff and try to read opponent's patterns" },
            };
        }

        private void InitializeExampleHandOdds()
        {
            exampleHandOdds = new Dictionary<HandType, string>
            {
                { HandType.StrongHand, "Three of a Kind, Straight Flush" },
                { HandType.MediumHand, "Pair or Flush" },
                { HandType.WeakHand, "High Card or No Bonus" }
            };
        }

        public Dictionary<DifficultyLevels, string> GetBluffSettingConditions()
        {
            return bluffSettingConditions;
        }

        public Dictionary<HandType, string> GetExampleHandOdds()
        {
            return exampleHandOdds;
        }

        public Dictionary<PossibleMoves, string> GetMoveValidityConditions()
        {
            return moveValidityConditions;
        }

        public T GetBonusRule<T>() where T : BaseBonusRule
        {
            return BonusRules.OfType<T>().FirstOrDefault();
        }

        private void SaveChanges()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif

        }
    }

    [Serializable]
    public struct BonusValues
    {
        public int TrumpCardBonus;
        public int SameColorBonus;
        public int WildCardBonus;
        public int RankAdjacentBonus;
    }
}

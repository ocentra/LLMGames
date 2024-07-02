using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeCardBrag
{
    public class AIHelper
    {
        private GameInfo GameInfo { get; set; }
        GameManager GameManager { get; set; }
        public AIHelper(GameInfo gameInfo, GameManager gameManager)
        {
            GameInfo = gameInfo;
            GameManager = gameManager;
        }

        public string GetAIInstructions()
        {
            return $"As an AI player in Three Card Brag GameDescription : {GameInfo.GameDescription} {Environment.NewLine}" +
                   $"1. Evaluate the strength of your hand {GetHandDetails(GameManager)} based on card rankings {GameInfo.CardRankings}.{Environment.NewLine}" +
                   $"Also Keep in mind of GameRules {GameInfo.GameRules} and onusRules {GetBonusRules(GameInfo.BonusRules)}{Environment.NewLine}" +
                   $"2. Consider the current bet {GameManager.CurrentBet} and pot size {GameManager.Pot} when making decisions.{Environment.NewLine}" +
                   $"3. Use probability to estimate the likelihood of improving your hand when drawing or swapping use StrategyTips {GameInfo.StrategyTips} {Environment.NewLine}" +
                   $"4. Implement bluffing strategies based on the {GetDifficultyLevel()}, especially when playing blind.{Environment.NewLine}" +
                   $"4.5 you Can use this as guide of BluffSettingConditions: {BluffSetting(GameInfo.BluffSettingConditions)} {Environment.NewLine}" +
                   $"5. Adapt your strategy based on the human player's behavior and betting patterns. use Current game state: {GetGameStateDetails(GameManager)} {Environment.NewLine}" +
                   $"6. Manage your coins {GameManager.ComputerPlayer.Coins} to ensure you can play multiple rounds. {Environment.NewLine}" +
                   $" Game is in Round {GameManager.ScoreKeeper.ComputerTotalWins + GameManager.ScoreKeeper.HumanTotalWins} of {GameManager.MaxRounds}.{Environment.NewLine}" +
                   $" You have won {GameManager.ScoreKeeper.ComputerTotalWins} Opponent have own {GameManager.ScoreKeeper.HumanTotalWins} you have {GameManager.ComputerPlayer.Coins} and He's got {GameManager.HumanPlayer.Coins}" +
                   $"7. Calculate pot odds to make informed betting decisions Example Hand odd {GetExampleHandDescriptions(GameInfo.ExampleHandOdds)}.{Environment.NewLine}" +
                   $"8. You Need to understand and evaluate all possibilities {GetPossibleMoves(GameInfo.MoveValidityConditions)}and then ONLY ONLY IMPORTANT reply with one of the move word, {GetMoveWord()}";
        }

        private string GetBonusRules(BonusRule[] rules)
        {
            string bonusRules = $"BonusRule {Environment.NewLine}";
            for (var index = 0; index < rules.Length; index++)
            {
                var bonusRule = rules[index];
                bonusRules += $"bonusRule {index + 1}: {bonusRule.Description} Points {bonusRule.BonusValue} {Environment.NewLine}";
            }
            return bonusRules;
        }
        private string GetExampleHandDescriptions(Dictionary<HandType, string> exampleHandOdds)
        {
            return string.Join(Environment.NewLine, exampleHandOdds.Select(hd => $"{hd.Key}: {hd.Value}"));
        }

        private string GetPossibleMoves(Dictionary<PossibleMoves, string> moveValidityConditions)
        {
            return string.Join(Environment.NewLine, moveValidityConditions.Select(mvc => $"{mvc.Key}: {mvc.Value}"));
        }

        private string BluffSetting(Dictionary<DifficultyLevels, string> bluffSettingConditions)
        {
            return string.Join(Environment.NewLine, bluffSettingConditions.Select(mvc => $"{mvc.Key}: {mvc.Value}"));
        }

        private string GetMoveWord()
        {
            return string.Join(" or ", Enum.GetNames(typeof(PossibleMoves)));
        }

        private string GetDifficultyLevel()
        {
            return string.Join(" or ", Enum.GetNames(typeof(PossibleMoves)));
        }

        private static string GetHandDetails(GameManager gameManager)
        {
            return string.Join(", ", gameManager.ComputerPlayer.Hand.Select((card, index) => $"Card {index + 1}: {card.Rank} of {card.Suit}"));
        }

        private static string GetGameStateDetails(GameManager gameManager)
        {
            return $"Pot: {GetPotDetails(gameManager)}{Environment.NewLine}" +
                   $"Deck: {GetDeckDetails(gameManager)}{Environment.NewLine}" +
                   $"Floor: {GetFloorDetails(gameManager)}{Environment.NewLine}" +
                   $"Players: {GetPlayerDetails(gameManager)}";
        }

        private static string GetPotDetails(GameManager gameManager)
        {
            return $"Current pot: {gameManager.Pot} coins, Current bet: {gameManager.CurrentBet} coins";
        }

        private static string GetDeckDetails(GameManager gameManager)
        {
            return $"Remaining cards in deck: {gameManager.DeckManager.RemainingCards}";
        }

        private static string GetFloorDetails(GameManager gameManager)
        {
            string floorCardDetails = gameManager.DeckManager.FloorCard != null
                ? $"{gameManager.DeckManager.FloorCard.Rank} of {gameManager.DeckManager.FloorCard.Suit}"
                : "No card";

            return $"Floor card: {floorCardDetails}, Cards in floor pile: {gameManager.DeckManager.FloorCardsCount}";
        }

        private static string GetPlayerDetails(GameManager gameManager)
        {
            return $"Human: {gameManager.HumanPlayer.Coins} coins, Computer: {gameManager.ComputerPlayer.Coins} coins, " +
                   $"Human playing blind: {!gameManager.HumanPlayer.HasSeenHand}, Computer playing blind: {!gameManager.ComputerPlayer.HasSeenHand}";
        }

    }
}
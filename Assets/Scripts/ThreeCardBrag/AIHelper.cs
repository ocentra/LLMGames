using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeCardBrag
{
    public class AIHelper
    {
        private GameInfo GameInfo { get; set; }
        GameController GameController { get; set; }
        public AIHelper(GameInfo gameInfo, GameController gameController)
        {
            GameInfo = gameInfo;
            GameController = gameController;
        }

        public string GetAIInstructions()
        {
            return $"As an AI player in Three Card Brag GameDescription : {GameInfo.GameDescription} {Environment.NewLine}" +
                   $"1. Evaluate the strength of your hand {GetHandDetails(GameController)} based on card rankings {GameInfo.CardRankings}.{Environment.NewLine}" +
                   $"Also Keep in mind of GameRules {GameInfo.GameRules} and onusRules {GetBonusRules(GameInfo.BonusRules)}{Environment.NewLine}" +
                   $"2. Consider the current bet {GameController.CurrentBet} and pot size {GameController.Pot} when making decisions.{Environment.NewLine}" +
                   $"3. Use probability to estimate the likelihood of improving your hand when drawing or swapping use StrategyTips {GameInfo.StrategyTips} {Environment.NewLine}" +
                   $"4. Implement bluffing strategies based on the {GetDifficultyLevel()}, especially when playing blind.{Environment.NewLine}" +
                   $"4.5 you Can use this as guide of BluffSettingConditions: {BluffSetting(GameInfo.BluffSettingConditions)} {Environment.NewLine}" +
                   $"5. Adapt your strategy based on the human player's behavior and betting patterns. use Current game state: {GetGameStateDetails(GameController)} {Environment.NewLine}" +
                   $"6. Manage your coins {GameController.ComputerPlayer.Coins} to ensure you can play multiple rounds. {Environment.NewLine}" +
                   $" Game is in Round {GameController.ScoreKeeper.ComputerTotalWins + GameController.ScoreKeeper.HumanTotalWins} of {GameController.MaxRounds}.{Environment.NewLine}" +
                   $" You have won {GameController.ScoreKeeper.ComputerTotalWins} Opponent have own {GameController.ScoreKeeper.HumanTotalWins} you have {GameController.ComputerPlayer.Coins} and He's got {GameController.HumanPlayer.Coins}" +
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

        private static string GetHandDetails(GameController gameController)
        {
            return string.Join(", ", gameController.ComputerPlayer.Hand.Select((card, index) => $"Card {index + 1}: {card.Rank} of {card.Suit}"));
        }

        private static string GetGameStateDetails(GameController gameController)
        {
            return $"Pot: {GetPotDetails(gameController)}{Environment.NewLine}" +
                   $"Deck: {GetDeckDetails(gameController)}{Environment.NewLine}" +
                   $"Floor: {GetFloorDetails(gameController)}{Environment.NewLine}" +
                   $"Players: {GetPlayerDetails(gameController)}";
        }

        private static string GetPotDetails(GameController gameController)
        {
            return $"Current pot: {gameController.Pot} coins, Current bet: {gameController.CurrentBet} coins";
        }

        private static string GetDeckDetails(GameController gameController)
        {
            return $"Remaining cards in deck: {gameController.DeckManager.RemainingCards}";
        }

        private static string GetFloorDetails(GameController gameController)
        {
            string floorCardDetails = gameController.DeckManager.FloorCard != null
                ? $"{gameController.DeckManager.FloorCard.Rank} of {gameController.DeckManager.FloorCard.Suit}"
                : "No card";

            return $"Floor card: {floorCardDetails}, Cards in floor pile: {gameController.DeckManager.FloorCardsCount}";
        }

        private static string GetPlayerDetails(GameController gameController)
        {
            return $"Human: {gameController.HumanPlayer.Coins} coins, Computer: {gameController.ComputerPlayer.Coins} coins, " +
                   $"Human playing blind: {!gameController.HumanPlayer.HasSeenHand}, Computer playing blind: {!gameController.ComputerPlayer.HasSeenHand}";
        }

    }
}
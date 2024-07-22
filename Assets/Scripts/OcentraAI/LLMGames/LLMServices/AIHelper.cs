using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    public class AIHelper
    {
        private GameInfo GameInfo { get; set; }
        private GameManager GameManager { get; set; }

        public AIHelper(GameInfo gameInfo, GameManager gameManager)
        {
            GameInfo = gameInfo;
            GameManager = gameManager;
        }

        public string GetSystemMessage()
        {
            return $"You are an expert AI player in a Three Card Brag game. " +
                   $"Your goal is to make the best betting decisions based on the strength of your hand, the game rules, and the behavior of the human player. " +
                   $"Game Rules: {GameInfo.GameRules}. " +
                   $"Card Rankings: {GameInfo.CardRankings}. " +
                   $"Bonus Rules: {GetBonusRules(GameInfo.BonusRules)}. " +
                   $"Strategy Tips: {GameInfo.StrategyTips}. " +
                   $"Bluffing Strategies: {BluffSetting(GameInfo.GetBluffSettingConditions())}. " +
                   $"Example Hand Descriptions: {GetExampleHandDescriptions(GameInfo.GetExampleHandOdds())}. " +
                   $"Possible Moves: {GetPossibleMoves(GameInfo.GetMoveValidityConditions())}. " +
                   $"Difficulty Levels: {GetDifficultyLevel()}";
        }

        public string GetUserPrompt()
        {
            return $"Current Hand: {GetHandDetails(GameManager)}. " +
                   $"Current Hand Value: {GameManager.PlayerManager.ComputerPlayer.CalculateHandValue()}. " +
                   $"Current Bet: {GameManager.ScoreManager.CurrentBet}, Pot Size: {GameManager.ScoreManager.Pot}. " +
                   $"Your Coins: {GameManager.PlayerManager.ComputerPlayer.Coins}, Opponent's Coins: {GameManager.PlayerManager.HumanPlayer.Coins}. " +
                   $"Current Game State: {GetGameStateDetails(GameManager)}. " +
                   $"Move Options: {GetMoveWord()}";
        }

        private string GetBonusRules(BaseBonusRule[] rules)
        {
            string bonusRules = $"BonusRule {Environment.NewLine}";
            for (int index = 0; index < rules.Length; index++)
            {
                BaseBonusRule bonusRule = rules[index];
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
            return string.Join(" or ", Enum.GetNames(typeof(DifficultyLevels)));
        }

        private static string GetHandDetails(GameManager gameManager)
        {
            return string.Join(", ", gameManager.PlayerManager.ComputerPlayer.Hand.Select((card, index) => $"Card {index + 1}: {card.Rank} of {card.Suit}"));
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
            return $"Current pot: {gameManager.ScoreManager.Pot} coins, Current bet: {gameManager.ScoreManager.CurrentBet} coins";
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
            return $"Human: {gameManager.PlayerManager.HumanPlayer.Coins} coins, Computer: {gameManager.PlayerManager.ComputerPlayer.Coins} coins, " +
                   $"Human playing blind: {!gameManager.PlayerManager.HumanPlayer.HasSeenHand}, Computer playing blind: {!gameManager.PlayerManager.ComputerPlayer.HasSeenHand}";
        }


        public (string systemMessage, string userPrompt) GetAIInstructions()
        {
            string systemMessage = GetSystemMessage();
            string userPrompt = GetUserPrompt();
            return (systemMessage, userPrompt);
        }

    }
}

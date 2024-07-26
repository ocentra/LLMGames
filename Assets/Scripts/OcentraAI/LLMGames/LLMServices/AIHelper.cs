using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    public class AIHelper : ManagerBase<AIHelper>
    {
        
        private GameInfo GameInfo => GameInfo.Instance;
        private GameManager GameManager => GameManager.Instance;
        private PlayerManager PlayerManager => PlayerManager.Instance;
        private ScoreManager ScoreManager => ScoreManager.Instance;
        private DeckManager DeckManager => DeckManager.Instance;
        private TurnManager TurnManager => TurnManager.Instance;

        protected override void Awake()
        {
            base.Awake();
           
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
            return $"Current Hand: {GetHandDetails()}. " +
                   $"Current Hand Value: {PlayerManager.ComputerPlayer.CalculateHandValue()}. " +
                   $"Current Bet: {ScoreManager.CurrentBet}, Pot Size: {ScoreManager.Pot}. " +
                   $"Your Coins: {PlayerManager.ComputerPlayer.Coins}, Opponent's Coins: {PlayerManager.HumanPlayer.Coins}. " +
                   $"Current Game State: {GetGameStateDetails()}. " +
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

        private string GetHandDetails()
        {
            return string.Join(", ", PlayerManager.ComputerPlayer.Hand.Select((card, index) => $"Card {index + 1}: {card.Rank} of {card.Suit}"));
        }

        private  string GetGameStateDetails()
        {
            return $"Pot: {GetPotDetails()}{Environment.NewLine}" +
                   $"Deck: {GetDeckDetails()}{Environment.NewLine}" +
                   $"Floor: {GetFloorDetails()}{Environment.NewLine}" +
                   $"Players: {GetPlayerDetails()}";
        }

        private string GetPotDetails()
        {
            return $"Current pot: {ScoreManager.Pot} coins, Current bet: {ScoreManager.CurrentBet} coins";
        }

        private string GetDeckDetails()
        {
            return $"Remaining cards in deck: {DeckManager.RemainingCards}";
        }

        private  string GetFloorDetails()
        {
            string floorCardDetails = DeckManager.FloorCard != null
                ? $"{DeckManager.FloorCard.Rank} of {DeckManager.FloorCard.Suit}"
                : "No card";

            return $"Floor card: {floorCardDetails}, Cards in floor pile: {DeckManager.FloorCardsCount}";
        }

        private string GetPlayerDetails()
        {
            return $"Human: {PlayerManager.HumanPlayer.Coins} coins, Computer: {PlayerManager.ComputerPlayer.Coins} coins, " +
                   $"Human playing blind: {!PlayerManager.HumanPlayer.HasSeenHand}, Computer playing blind: {!PlayerManager.ComputerPlayer.HasSeenHand}";
        }


        public (string systemMessage, string userPrompt) GetAIInstructions()
        {
            string systemMessage = GetSystemMessage();
            string userPrompt = GetUserPrompt();
            return (systemMessage, userPrompt);
        }

    }
}

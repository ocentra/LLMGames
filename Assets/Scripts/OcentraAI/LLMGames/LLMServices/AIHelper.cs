using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    public class AIHelper : SingletonManagerBase<AIHelper>
    {




        public string GetSystemMessage(GameMode gameMode)
        {
           

            return "You are an expert AI player in a Three Card Brag game. " +
                   "Your goal is to make the best betting decisions based on the strength of your hand, the game rules, and the behavior of the human player. " +
                   $"Game Rules: {gameMode.GameRules.LLM}. " +
                   $"Card Rankings: {gameMode.CardRankings}. " +
                   $"Bonus Rules: {GetBonusRules(gameMode.BonusRules)}. " +
                   $"Strategy Tips: {gameMode.StrategyTips}. " +
                   $"Bluffing Strategies: {BluffSetting(gameMode.GetBluffSettingConditions())}. " +
                   $"Example Hand Descriptions: {GetExampleHandDescriptions(gameMode.GetExampleHandOdds())}. " +
                   $"Possible Moves: {GetPossibleMoves(gameMode.GetMoveValidityConditions())}. " +
                   $"Difficulty Levels: {GetDifficultyLevel()}";
        }

        public string GetUserPrompt(ulong playerID)
        {
            return $"Current Hand: {GetHandDetails(playerID)}. " +
                   $"Current Game State: {GetGameStateDetails()}. " +
                   $"Move Options: {GetMoveWord()}";
        }

        private string GetBonusRules(List<BaseBonusRule> rules)
        {
            string bonusRules = $"BonusRule {Environment.NewLine}";
            for (int index = 0; index < rules.Count; index++)
            {
                BaseBonusRule bonusRule = rules[index];
                bonusRules +=
                    $"bonusRule {index + 1}: {bonusRule.Description} Points {bonusRule.BonusValue} {Environment.NewLine}";
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

        private async UniTask<string> GetHandDetails(ulong playerID)
        {
            UniTaskCompletionSource<(bool success,string hand)> dataSource = new UniTaskCompletionSource<(bool success, string hand)>();
            await EventBus.Instance.PublishAsync<RequestPlayerHandDetailEvent>(new RequestPlayerHandDetailEvent(playerID,dataSource));
            (bool success, string hand) dataSourceTask = await dataSource.Task;

            if (dataSourceTask.success)
            {
                string hand = dataSourceTask.hand;
               
                return $"{hand}";
            }
            return "Failed to retrieve Hand details.";
        }

        private async UniTask<string> GetGameStateDetails()
        {
            string scoreManagerDetails = await GetScoreManagerDetails();
            string deckDetails = await GetDeckDetails();
            string floorDetails = await GetFloorDetails();
            string playerDetails = await GetPlayerDetails();

            return $"ScoreManagerDetails: {scoreManagerDetails}{Environment.NewLine}" +
                   $"Deck: {deckDetails}{Environment.NewLine}" +
                   $"FloorCard: {floorDetails}{Environment.NewLine}" +
                   $"Players: {playerDetails}";
        }

        private async UniTask<string> GetScoreManagerDetails()
        {
            UniTaskCompletionSource<(bool success, int pot, int currentBet) > dataSource = new UniTaskCompletionSource<(bool success, int pot, int currentBet)>();
            await EventBus.Instance.PublishAsync<RequestScoreManagerDetailsEvent>(new RequestScoreManagerDetailsEvent(dataSource));
            (bool success, int pot, int currentBet) dataSourceTask = await dataSource.Task;

            if (dataSourceTask.success)
            {
                int pot = dataSourceTask.pot;
                int currentBet = dataSourceTask.currentBet;
                return $"Current pot: {pot} coins, Current bet: {currentBet} coins";
            }
            return "Failed to retrieve deck details.";


           
        }

        private async UniTask<string> GetDeckDetails()
        {
            UniTaskCompletionSource<(bool success, int cards)> dataSource = new UniTaskCompletionSource<(bool success, int cards)>();
            await EventBus.Instance.PublishAsync<RequestRemainingCardsCountEvent>(new RequestRemainingCardsCountEvent(dataSource));
            (bool success, int cards) dataSourceTask = await dataSource.Task;

            if (dataSourceTask.success)
            {
                int remainingCards = dataSourceTask.cards;
                return $"Remaining cards {remainingCards}";
            }

            return "Failed to retrieve deck details.";
        }


        private async UniTask<string> GetFloorDetails()
        {
            UniTaskCompletionSource<(bool success, string card)> dataSource = new UniTaskCompletionSource<(bool success, string card)>();
            await EventBus.Instance.PublishAsync<RequestFloorCardsDetailEvent>(new RequestFloorCardsDetailEvent(dataSource));
            (bool success, string card) dataSourceTask = await dataSource.Task;

            if (dataSourceTask.success)
            {
                string card = dataSourceTask.card;
                
                return $"{card}";
            }

            return "Failed to Floor deck details.";
        }

        private async UniTask<string> GetPlayerDetails()
        {
            UniTaskCompletionSource<(bool success, IReadOnlyList<IPlayerBase> players)> dataSource = new UniTaskCompletionSource<(bool success, IReadOnlyList<IPlayerBase> players)>();
            await EventBus.Instance.PublishAsync<RequestAllPlayersDataEvent>(new RequestAllPlayersDataEvent(dataSource));
            (bool success, IReadOnlyList<IPlayerBase> players) dataSourceTask = await dataSource.Task;

            if (dataSourceTask.success)
            {
                IReadOnlyList<IPlayerBase> playerList = dataSourceTask.players;
                List<string> playerDetails = new List<string>();

                for (int i = 0; i < playerList.Count; i++)
                {
                    IPlayerBase player = playerList[i];
                    bool isComputer = player is IComputerPlayerData;
                    bool isHuman = player is IHumanPlayerData;

                    string playerType = isHuman ? "Human" : isComputer ? "Computer" : "Unknown";
                    int coins = player.GetCoins();
                    bool hasSeenHand = player.HasSeenHand.Value;
                    playerDetails.Add($"{playerType} {i + 1}: {coins} coins, HasSeenHand: {hasSeenHand} , IsBankrupt {player.IsBankrupt}, HasFolded {player.HasFolded} ");
                }

                return string.Join(", ", playerDetails);
            }

            return "Failed to retrieve player details.";
        }



        public (string systemMessage, string userPrompt) GetAIInstructions(GameMode gameMode,ulong playerID)
        {
            string systemMessage = GetSystemMessage(gameMode);
            string userPrompt = GetUserPrompt(playerID);
            return (systemMessage, userPrompt);
        }
    }
}
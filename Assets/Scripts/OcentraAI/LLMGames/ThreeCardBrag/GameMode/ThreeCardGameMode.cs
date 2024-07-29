using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Rules;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [CreateAssetMenu(fileName = nameof(ThreeCardGameMode), menuName = "GameMode/ThreeCardGameMode")]
    public class ThreeCardGameMode : GameMode
    {
        #region Fields and Properties

        public override string GameName { get; protected set; } = "Three Card Brag";

        public override int MinPlayers { get; protected set; } = 2;

        public override int MaxPlayers { get; protected set; } = 6;

        public override int MaxRounds { get; protected set; } = 10;

        public override float TurnDuration { get; protected set; } = 60;

        public override int InitialPlayerCoins { get; protected set; } = 1000;

        public override int NumberOfCards { get; protected set; } = 3;

        [Button(ButtonSizes.Medium)]
        protected override void Initialize()
        {
            InitializeGameMode();
        }

        #endregion

        #region Initialization Methods - Bonus Rules

        protected override void InitializeBonusRules()
        {
            TrumpBonusValues = new TrumpBonusValues();

            BonusRules = new List<BaseBonusRule>
            {
                new ThreeOfAKindRule(30, 100,this),
                new StraightFlushRule(25, 90, this),
                new SameColorsSequenceRule(20, 80, this),
                new DifferentColorsSequenceRule(15, 70, this),
                new PairInHandRule(10, 60, this),
                new FlushRule(5, 70, this)
            };
        }

        #endregion

        #region Initialization Methods - Game Rules

        protected override void InitializeGameRules()
        {
            GameRules = new GameRulesContainer
            {
                Player = $"{GameName} Rules:{System.Environment.NewLine}" +
                         $"1. Each player is dealt 3 cards at the start of each round.{System.Environment.NewLine}" +
                         $"2. Players can bet blind (without seeing their hand) or see their hand before betting.{System.Environment.NewLine}" +
                         $"3. Blind betting doubles the current bet. Players who have seen their hand bet double the current bet.{System.Environment.NewLine}" +
                         $"4. Players can choose to fold, call, or raise at their turn.{System.Environment.NewLine}" +
                         $"5. Players can draw a new card to the floor.{System.Environment.NewLine}" +
                         $"6. Players can pick and swap a card from the floor. After picking and swapping, the player must decide to bet, fold, call, or raise.{System.Environment.NewLine}" +
                         $"7. The highest hand value wins (Ace high). In case of a tie, the highest card wins.{System.Environment.NewLine}" +
                         $"8. The game ends when a player runs out of coins or after a set number of rounds.{System.Environment.NewLine}" +
                         $"9. The trailing player can continue if they have more coins.{System.Environment.NewLine}" +
                         $"10. Players start with {InitialPlayerCoins} coins.{System.Environment.NewLine}" +
                         $"Special Rules for Three Card Brag:{System.Environment.NewLine}" +
                         $"11. Three of a Kind: Three cards of the same rank. Example: {Card.GetRankSymbol(Suit.Diamonds, Rank.J)}, {Card.GetRankSymbol(Suit.Hearts, Rank.J)}, {Card.GetRankSymbol(Suit.Clubs, Rank.J)}{System.Environment.NewLine}" +
                         $"12. Royal Flush: Three cards in sequence of the same suit, starting from Ace. Example: {Card.GetRankSymbol(Suit.Spades, Rank.A)}, {Card.GetRankSymbol(Suit.Spades, Rank.K)}, {Card.GetRankSymbol(Suit.Spades, Rank.Q)}{System.Environment.NewLine}" +
                         $"13. Straight Flush: Three cards in sequence of the same suit. Example: {Card.GetRankSymbol(Suit.Spades, Rank.Nine)}, {Card.GetRankSymbol(Suit.Spades, Rank.Ten)}, {Card.GetRankSymbol(Suit.Spades, Rank.J)}{System.Environment.NewLine}" +
                         $"14. Straight: Three cards in sequence but not all of the same suit. Example: {Card.GetRankSymbol(Suit.Spades, Rank.Four)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Five)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.Six)}{System.Environment.NewLine}" +
                         $"15. Flush: Three cards of the same suit but not in sequence. Example: {Card.GetRankSymbol(Suit.Spades, Rank.Two)}, {Card.GetRankSymbol(Suit.Spades, Rank.Five)}, {Card.GetRankSymbol(Suit.Spades, Rank.Nine)}{System.Environment.NewLine}" +
                         $"16. Pair: Two cards of the same rank. Example: {Card.GetRankSymbol(Suit.Spades, Rank.Q)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.Q)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Seven)}{System.Environment.NewLine}" +
                         $"17. High Card: When no other hand is achieved, the highest card plays. Example: {Card.GetRankSymbol(Suit.Spades, Rank.A)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.Seven)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Four)} (Ace high){System.Environment.NewLine}" +
                         $"18. Trump Card: A designated card that can replace any card to complete a sequence or set. Example: Trump card is {Card.GetRankSymbol(Suit.Hearts, Rank.Six)}. Hand: {Card.GetRankSymbol(Suit.Spades, Rank.Two)}, {Card.GetRankSymbol(Suit.Spades, Rank.Three)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Six)} (Trump card converts to {Card.GetRankSymbol(Suit.Spades, Rank.Four)} to make straight flush: {Card.GetRankSymbol(Suit.Spades, Rank.Two)}, {Card.GetRankSymbol(Suit.Spades, Rank.Three)}, {Card.GetRankSymbol(Suit.Spades, Rank.Four)}){System.Environment.NewLine}" +
                         $"19. Trump in Middle: The Trump card is considered 'in the middle' if it is surrounded by cards on either side that form a continuous sequence. Example: {Card.GetRankSymbol(Suit.Spades, Rank.Five)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Six)}, {Card.GetRankSymbol(Suit.Spades, Rank.Seven)} ({Card.GetRankSymbol(Suit.Hearts, Rank.Six)} Trump card is in the middle){System.Environment.NewLine}" +
                         $"20. Adjacent to Trump: Cards adjacent to the Trump card provide a special point bonus, even if they do not form a valid hand. Example: {Card.GetRankSymbol(Suit.Spades, Rank.Two)}, {Card.GetRankSymbol(Suit.Spades, Rank.Three)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Seven)} ({Card.GetRankSymbol(Suit.Hearts, Rank.Seven)} is adjacent to the Trump card {Card.GetRankSymbol(Suit.Hearts, Rank.Six)} and receives points, but the hand does not form a valid sequence).",

                LLM = $"{GameName}: 3 cards dealt each round. Bet blind or see hand. Blind bet doubles, seen hand bet doubles current bet. Choose to fold, call, raise. Draw new card to floor. Pick and swap card, then bet, fold, call, or raise. Highest hand wins (Ace high). Tie: highest card wins. Game ends when player runs out of coins or after set rounds. Trailing player can continue if more coins. Start with {InitialPlayerCoins} coins." +
                      "Special Rules: Three of a Kind: J♠, J♦, J♣. Royal Flush: A♠, K♠, Q♠. Straight Flush: 9♠, 10♠, J♠. Straight: 4♠, 5♣, 6♦. Flush: 2♠, 5♠, 9♠. Pair: Q♠, Q♦, 7♣. High Card: A♠, 7♦, 4♣ (Ace high)." +
                      "Trump: 6♥ can replace any card. Ex: 2♠, 3♠, 6♥ (6♥ becomes 4♠ for straight flush: 2♠, 3♠, 4♠). Trump in Middle: 5♠, 6♥, 7♠ (6♥ in middle). Adjacent to Trump: 2♠, 3♠, 7♥ (7♥ next to 6♥ gets points, but no valid sequence)."
            };
        }

        #endregion


        #region Initialization Methods - Game Description

        protected override void InitializeGameDescription()
        {
            GameDescription.Player = $"Three Card Brag is an exciting card game that combines elements of poker and bluffing.{System.Environment.NewLine}" +
                              $"Players aim to make the best three-card hand while betting and bluffing their way to victory.{System.Environment.NewLine}" +
                              "Most interesting part is we will be playing against Large language model LLM i.e Chat GPT ";

            GameDescription.LLM = GameDescription.Player;
        }

        #endregion

        #region Initialization Methods - Strategy Tips

        protected override void InitializeStrategyTips()
        {
            StrategyTips.Player = $"Pay attention to your opponents' betting patterns.{System.Environment.NewLine}" +
                           $"Use the blind betting option strategically to bluff or build the pot.{System.Environment.NewLine}" +
                           $"Consider the odds of improving your hand when deciding to draw or swap cards.{System.Environment.NewLine}" +
                           $"Don't be afraid to fold if you have a weak hand and the bets are high.{System.Environment.NewLine}" +
                           $"Manage your coins wisely to stay in the game for multiple rounds.";

            StrategyTips.LLM = GameDescription.Player;
        }

        #endregion

        #region Initialization Methods - Move Validity Conditions

        protected override void InitializeMoveValidityConditions()
        {
            base.InitializeMoveValidityConditions();
            MoveValidityConditions.Add(PossibleMoves.Fold, "Always valid");
            MoveValidityConditions.Add(PossibleMoves.Call, "Valid when there's a bet to call");
            MoveValidityConditions.Add(PossibleMoves.Raise, "Valid when you have enough coins to raise");
            MoveValidityConditions.Add(PossibleMoves.Check, "Valid when there's no bet to call");
            MoveValidityConditions.Add(PossibleMoves.BetBlind, "Valid only if you haven't seen your hand");
            MoveValidityConditions.Add(PossibleMoves.SeeHand, "Valid only if you haven't seen your hand");
            MoveValidityConditions.Add(PossibleMoves.DrawFromDeck, "Valid when there's no floor card");
            MoveValidityConditions.Add(PossibleMoves.PickFromFloor, "Valid when there's a floor card");
            MoveValidityConditions.Add(PossibleMoves.SwapCard, "Valid after drawing or picking from floor");
            MoveValidityConditions.Add(PossibleMoves.ShowHand, "Valid at any time, ends the round");
        }

        #endregion

        #region Initialization Methods - Bluff Setting Conditions

        protected override void InitializeBluffSettingConditions()
        {base.InitializeBluffSettingConditions();
            BluffSettingConditions.Add(DifficultyLevels.Easy, "Rarely bluff");
            BluffSettingConditions.Add(DifficultyLevels.Medium, "Occasionally bluff when the pot odds are favorable");
            BluffSettingConditions.Add(DifficultyLevels.Hard, "Frequently bluff and try to read opponent's patterns");
        }

        #endregion


        #region Initialization Methods - Example Hand Odds

        protected override void InitializeExampleHandOdds()
        {
            base.InitializeExampleHandOdds();
            ExampleHandOdds.Add(HandType.StrongHand, "Three of a Kind, Royal Flush, Straight Flush");
            ExampleHandOdds.Add(HandType.MediumHand, "Pair or Flush");
            ExampleHandOdds.Add(HandType.WeakHand, "High Card or No Bonus");
        }

        #endregion


    }


}
using NUnit.Framework;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules.Tests
{
    public abstract class BaseBonusRuleTest<T> where T : BaseBonusRule
    {
        protected T _rule;
        protected TestGameMode _gameMode;

        [SetUp]
        public void Setup()
        {
            _gameMode = ScriptableObject.CreateInstance<TestGameMode>();
            _gameMode.TryInitialize();
            _rule = ScriptableObject.CreateInstance<T>();
            _rule.SetGameMode(_gameMode);
        }

        [Test]
        public void Evaluate_WithValidHand_ReturnsTrue([NUnit.Framework.Range(3, 9)] int handSize)
        {
            _gameMode.SetNumberOfCards(handSize);
            Hand hand = new Hand(CreateRandomHand(handSize));

            bool result = _rule.Evaluate(hand, out BonusDetail bonusDetails);

            Assert.IsTrue(result);
            Assert.IsNotNull(bonusDetails);
            Assert.AreEqual(_rule.RuleName, bonusDetails.RuleName);
            Assert.AreEqual(_rule.BonusValue, bonusDetails.BaseBonus);

            CleanupHand(hand);
        }
        // todo dont instantiate or create 
        protected void CleanupHand(Hand hand)
        {
            foreach (Card card in hand.Cards)
            {
                Object.DestroyImmediate(card);
            }
        }

        protected List<Card> CreateRandomHand(int size)
        {
            List<Card> hand = new List<Card>();
            List<Card> cardTemplates = Deck.Instance.CardTemplates;

            for (int i = 0; i < size; i++)
            {
                Card card = cardTemplates[Random.Range(0, cardTemplates.Count)];
                hand.Add(Object.Instantiate(card));
            }

            return hand;
        }
    }
}
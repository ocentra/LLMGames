using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [CreateAssetMenu(fileName = nameof(TestGameMode), menuName = "GameMode/TestGameMode")]
    public class TestGameMode : GameMode
    {
        [OdinSerialize, ShowInInspector] public Card TrumpCard => Deck.Instance.GetCard(Suit.Hearts, Rank.Six);

        [OdinSerialize, ShowInInspector]
        public override int MaxRounds { get; protected set; } = 10;

        [OdinSerialize, ShowInInspector]
        public override float TurnDuration { get; protected set; } = 30.0f;

        [OdinSerialize, ShowInInspector]
        public override int InitialPlayerCoins { get; protected set; } = 100;

        [OdinSerialize, ShowInInspector, ReadOnly]
        public override string GameName { get; protected set; } = "Test Game Mode";

        [OdinSerialize, ShowInInspector, ReadOnly]
        public override int MinPlayers { get; protected set; } = 2;

        [OdinSerialize, ShowInInspector, ReadOnly]
        public override int MaxPlayers { get; protected set; } = 4;

        [OdinSerialize, ShowInInspector, ReadOnly]
        public override int NumberOfCards { get; protected set; } = 3;

        [OdinSerialize, ShowInInspector]
        public override bool UseTrump { get; protected set; } = true;

        public override bool TryInitialize(List<BaseBonusRule> bonusRulesTemplate)
        {
            // Initialize any additional properties or configurations specific to the TestGameMode
          return  TryInitializeGameMode(BonusRules);
        }

        public void SetNumberOfCards(int numberOfCards)
        {
            NumberOfCards = numberOfCards;
        }

        public void SetUseTrump(bool useTrump)
        {
            UseTrump = useTrump;
        }
        

    }
}

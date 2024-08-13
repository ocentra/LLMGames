using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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

        public override int InitialPlayerCoins { get; protected set; } = 100;

        public override string GameName { get; protected set; } = $"{nameof(TestGameMode)}";

        public override int MaxPlayers { get; protected set; } = 4;

        public override int NumberOfCards { get; protected set; } = 3;
        public override bool UseTrump { get; protected set; } = true;

        public override int BaseBet { get; protected set; } = 5;
        public override int BaseBlindMultiplier { get; protected set; } = 1;

        public override bool UseMagicCards { get; protected set; } = true;

        public override bool DrawFromDeckAllowed { get; protected set; } = true;

        public override bool TryInitialize()
        {
            // Initialize any additional properties or configurations specific to the TestGameMode
          return  TryInitializeGameMode();
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

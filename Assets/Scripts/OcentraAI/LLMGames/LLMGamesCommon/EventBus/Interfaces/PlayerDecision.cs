using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class PlayerDecision : INetworkSerializable
    {
        //default fallback
        public static readonly PlayerDecision None = new PlayerDecision(0, nameof(None));

        // main betting 
        public static readonly PlayerDecision SeeHand = new PlayerDecision(1, nameof(SeeHand));
        public static readonly PlayerDecision PlayBlind = new PlayerDecision(2, nameof(PlayBlind));
        public static readonly PlayerDecision Bet = new PlayerDecision(3, nameof(Bet));
        public static readonly PlayerDecision Fold = new PlayerDecision(4, nameof(Fold));
        public static readonly PlayerDecision DrawFromDeck = new PlayerDecision(5, nameof(DrawFromDeck));
        public static readonly PlayerDecision ShowCall = new PlayerDecision(6, nameof(ShowCall));
        public static readonly PlayerDecision RaiseBet = new PlayerDecision(7, nameof(RaiseBet));
        public static readonly PlayerDecision PickAndSwap = new PlayerDecision(16, nameof(PickAndSwap));
        public static IEnumerable<PlayerDecision> GetMainBettingDecisions()
        {
            return new List<PlayerDecision> { SeeHand, PlayBlind, Bet, Fold, DrawFromDeck, ShowCall, RaiseBet };
        }
        // extra gameplay
        public static readonly PlayerDecision WildCard0 = new PlayerDecision(10, nameof(WildCard0));
        public static readonly PlayerDecision WildCard1 = new PlayerDecision(11, nameof(WildCard1));
        public static readonly PlayerDecision WildCard2 = new PlayerDecision(12, nameof(WildCard2));
        public static readonly PlayerDecision WildCard3 = new PlayerDecision(13, nameof(WildCard3));
        public static readonly PlayerDecision Trump = new PlayerDecision(14, nameof(Trump));

        public static IEnumerable<PlayerDecision> GetExtraGamePlayDecisions()
        {
            return new List<PlayerDecision> { WildCard0, WildCard1, WildCard2, WildCard3, Trump };
        }

        // UI oriented
        public static readonly PlayerDecision ShowAllFloorCards = new PlayerDecision(15, nameof(ShowAllFloorCards));
        public static readonly PlayerDecision PurchaseCoins = new PlayerDecision(9, nameof(PurchaseCoins));
        public static IEnumerable<PlayerDecision> GetUIOrientedDecisions()
        {
            return new List<PlayerDecision> { ShowAllFloorCards, PurchaseCoins };
        }

        [SerializeField] private int decisionId;

        public int DecisionId
        {
            get => decisionId;
            private set => decisionId = value;
        }

        [ShowInInspector] public string Name { get; private set; }

        private PlayerDecision(int decisionId, string name)
        {
            this.decisionId = decisionId;
            Name = name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref decisionId);
        }

        public static PlayerDecision FromId(int id)
        {
            foreach (var decision in GetAllDecisions())
            {
                if (decision.DecisionId == id)
                {
                    return decision;
                }
            }
            return None;
        }

        public override bool Equals(object obj)
        {
            if (obj is PlayerDecision other)
            {
                return decisionId == other.decisionId;
            }
            return false;
        }

        public static bool operator ==(PlayerDecision left, PlayerDecision right)
        {
            // Handle null cases
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(PlayerDecision left, PlayerDecision right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return DecisionId.GetHashCode();
        }





        public static IEnumerable<PlayerDecision> GetAllDecisions()
        {
            List<PlayerDecision> decisions = new List<PlayerDecision>();
            FieldInfo[] fields = typeof(PlayerDecision).GetFields(BindingFlags.Public |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(PlayerDecision))
                {
                    PlayerDecision decision = (PlayerDecision)field.GetValue(null);
                    decisions.Add(decision);
                }
            }

            return decisions;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}

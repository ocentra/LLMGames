using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    /// Contains operations related to trump cards, such as checking if a rank is adjacent to trump, finding highest non-trump rank, etc.
    /// </summary>
    public static partial class HandUtility
    {


        /// <summary>
        /// Selects high-ranking non-trump cards from the hand.
        /// </summary>
        public static List<Card> SelectHighRankingNonTrumpCards(this Hand hand, Card trumpCard)
        {
            IRandom random = new UnityRandom();

            // Filter out high-ranking cards (J, Q, K, A) that are not the trump card
            List<Card> availableCards = new List<Card>();
            foreach (Card card in hand.GetCards())
            {
                if ((card.Rank == Rank.J || card.Rank == Rank.Q || card.Rank == Rank.K || card.Rank == Rank.A) && card != trumpCard)
                {
                    availableCards.Add(card);
                }
            }

            List<Card> selectedCards = new List<Card>();
            int handSize = hand.GetCards().Length;

            for (int i = 0; i < handSize && availableCards.Count > 0; i++)
            {
                int randomIndex = random.Range(0, availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }

            return selectedCards;
        }

        /// <summary>
        /// Finds the first non-trump rank in the hand or returns Rank.None if none is found.
        /// </summary>
        public static Rank FirstNonTrumpRankOrDefault(this Hand hand, Card trumpCard)
        {
            Card[] cards = hand.GetCards();

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != trumpCard)
                {
                    return cards[i].Rank;
                }
            }

            return Rank.None;
        }

        /// <summary>
        /// Determines if the hand contains a specified trump card.
        /// </summary>
        public static bool HasTrumpCard(this Hand hand, Card trumpCard)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (hand.GetCards()[i].Equals(trumpCard))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the rank is adjacent to the trump card rank.
        /// </summary>
        public static bool IsRankAdjacentToTrump(this Hand hand, Card trumpCard)
        {
            Hand orderedHand = hand.OrderBy();
            for (int i = 0; i < orderedHand.GetCards().Length; i++)
            {
                if (IsRankAdjacent(orderedHand.GetCards()[i].Rank, trumpCard.Rank))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the trump card is in the middle of the hand after sorting.
        /// </summary>
        public static bool IsTrumpInMiddle(this Hand hand, Card trumpCard)
        {
            Hand orderedHand = hand.OrderBy();
            int handSize = orderedHand.Count();
            if (handSize % 2 == 1)
            {
                int middleIndex = handSize / 2;
                return orderedHand.GetCards()[middleIndex].Equals(trumpCard);
            }
            else
            {
                int firstMiddleIndex = (handSize / 2) - 1;
                int secondMiddleIndex = handSize / 2;
                if (orderedHand.GetCards()[firstMiddleIndex].Equals(trumpCard)) return true;
                return orderedHand.GetCards()[secondMiddleIndex].Equals(trumpCard);
            }
        }

        /// <summary>
        /// Determines if a rank is adjacent to another rank, considering Ace as adjacent to Two.
        /// </summary>
        public static bool IsRankAdjacent(Rank rank1, Rank rank2)
        {
            int rankDifference = Math.Abs((int)rank1.Value - (int)rank2.Value);

            bool isNumericallyAdjacent = rankDifference == 1;

            bool isAceAndTwoAdjacent = (rank1 == Rank.A && rank2 == Rank.Two) || (rank1 == Rank.Two && rank2 == Rank.A);

            return isNumericallyAdjacent || isAceAndTwoAdjacent;
        }
    }
}

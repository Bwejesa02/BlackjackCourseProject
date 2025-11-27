using UnityEngine;

namespace Blackjack.Baseline
{
    [System.Serializable]
    public struct CardData
    {
        // 2–14 (11=Jack, 12=Queen, 13=King, 14=Ace)
        public int rank;
        // 0–3: 0=Clubs, 1=Diamonds, 2=Hearts, 3=Spades
        public int suit;

        public CardData(int rank, int suit)
        {
            this.rank = rank;
            this.suit = suit;
        }

        public string GetRankString()
        {
            if (rank >= 2 && rank <= 10) return rank.ToString();
            switch (rank)
            {
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                case 14: return "A";
                default: return "?";
            }
        }

        public string GetSuitSymbol()
        {
            switch (suit)
            {
                case 0: return "♣";
                case 1: return "♦";
                case 2: return "♥";
                case 3: return "♠";
                default: return "?";
            }
        }

        public override string ToString()
        {
            return GetRankString() + GetSuitSymbol();
        }
    }
}

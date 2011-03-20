using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

/*
 * Utility file to fill in the gaps left by the model generated
 * from the FlopPokerHandHistory.xsd schema file.
 *
 * Copyright (C) 2010 Wesley Tansey.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
namespace PokerHandHistory
{
    public partial class PokerHandXML
    {
        public PokerHand this[int index]
        {
            get { return handsField[index]; }
            set { handsField[index] = value; }
        }
    }

    public partial class PokerHand
    {
        public PokerHand()
        {
            this.contextField = new Context();
        }
    }

    public partial class Card
    {
        public Card()
        {
            this.rankField = Rank.None;
            this.suitField = Suit.None;
        }

        public Card(string card)
        {
            switch (card[0])
            {
                case '2': this.rankField = Rank.Two; break;
                case '3': this.rankField = Rank.Three; break;
                case '4': this.rankField = Rank.Four; break;
                case '5': this.rankField = Rank.Five; break;
                case '6': this.rankField = Rank.Six; break;
                case '7': this.rankField = Rank.Seven; break;
                case '8': this.rankField = Rank.Eight; break;
                case '9': this.rankField = Rank.Nine; break;
                case 'T': this.rankField = Rank.Ten; break;
                case 'J': this.rankField = Rank.Jack; break;
                case 'Q': this.rankField = Rank.Queen; break;
                case 'K': this.rankField = Rank.King; break;
                case 'A': this.rankField = Rank.Ace; break;
                case 'W': this.rankField = Rank.Joker; break;
                default: this.rankField = Rank.None; break;
            }

            switch (card[1])
            {
                case 'c': this.suitField = Suit.Clubs; break;
                case 'd': this.suitField = Suit.Diamonds; break;
                case 'h': this.suitField = Suit.Hearts; break;
                case 's': this.suitField = Suit.Spades; break;
                default: this.suitField = Suit.None; break;
            }
        }

        public static Rank GetRank(char rank)
        {
            switch (rank)
            {
                case '2': return Rank.Two;
                case '3': return Rank.Three;
                case '4': return Rank.Four;
                case '5': return Rank.Five;
                case '6': return Rank.Six;
                case '7': return Rank.Seven;
                case '8': return Rank.Eight;
                case '9': return Rank.Nine;
                case 'T': return Rank.Ten;
                case 'J': return Rank.Jack;
                case 'Q': return Rank.Queen;
                case 'K': return Rank.King;
                case 'A': return Rank.Ace;
                case 'W': return Rank.Joker;
            }
            return Rank.None;
        }

        public static Suit GetSuit(char suit)
        {
            switch (suit)
            {
                case 'c': return Suit.Clubs;
                case 'd': return Suit.Diamonds;
                case 'h': return Suit.Hearts;
                case 's': return Suit.Spades;
            }
            return Suit.None;
        }

        public static string RankString(Rank rank)
        {
            switch (rank)
            {
                case Rank.None: return "X";
                    
                case Rank.Two: return "2";
                    
                case Rank.Three: return "3";
                    
                case Rank.Four: return "4";
                    
                case Rank.Five: return "5";
                    
                case Rank.Six: return "6";
                    
                case Rank.Seven: return "7";
                    
                case Rank.Eight: return "8";
                    
                case Rank.Nine: return "9";
                    
                case Rank.Ten: return "T";
                    
                case Rank.Jack: return "J";
                    
                case Rank.Queen: return "Q";
                    
                case Rank.King: return "K";
                    
                case Rank.Ace: return "A";
                    
                case Rank.Joker: return "W";
                    
            }
            return "";
        }

        public static string SuitString(Suit suit)
        {
            switch (suit)
            {
                case Suit.None: return "x";
                    
                case Suit.Clubs: return "c";
                    
                case Suit.Diamonds: return "d";
                    
                case Suit.Hearts: return "h";
                    
                case Suit.Spades: return "s";                    
            }
            return "";
        }

        public static string ToString(Card[] cards)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cards.Length; i++)
            {
                sb.Append(cards[i].ToString());
                if (i < cards.Length - 1)
                    sb.Append(" ");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return RankString(this.rankField) + SuitString(suitField);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Card)
                return rankField == ((Card)obj).rankField && suitField == ((Card)obj).suitField;

            if (obj is string || obj is String)
                return ((string)obj) == this.ToString();
            
            return base.Equals(obj);
        }

        public static bool operator ==(Card c, string s)
        {
            return c.Equals(s);
        }

        public static bool operator !=(Card c, string s)
        {
            return !c.Equals(s);
        }

        public static bool operator ==(Card c1, Card c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Card c1, Card c2)
        {
            return !c1.Equals(c2);
        }

    }

    public partial class Action
    {

        public Action(string player, ActionType type)
        {
            this.playerField = player;
            this.typeField = type;
        }

        public Action(string player, ActionType type, Decimal amount)
        {
            this.playerField = player;
            this.typeField = type;
            this.amountField = amount;
        }
    }

    public partial class PokerHand
    {
        [XmlIgnore]
        public Round Preflop
        {
            get { return roundsField[0]; }
            set { roundsField[0] = value; }
        }

        [XmlIgnore]
        public Round Flop
        {
            get { return roundsField[1]; }
            set { roundsField[1] = value; }
        }

        [XmlIgnore]
        public Round Turn
        {
            get { return roundsField[2]; }
            set { roundsField[2] = value; }
        }

        [XmlIgnore]
        public Round River
        {
            get { return roundsField[3]; }
            set { roundsField[3] = value; }
        }

        /// <summary>
        /// Gets the hand result for this player (if any exists).
        /// </summary>
        public HandResult PlayerResult(string name)
        {
            foreach (HandResult hr in resultsField)
                if (hr.Player == name)
                    return hr;

            return null;
        }
    }

    public partial class HandResult
    {
        public HandResult()
        {
        }

        public HandResult(string player)
        {
            this.playerField = player;
        }
    }
}

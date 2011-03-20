#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using HandHistory;
using HandParserInterface;
using System.Text.RegularExpressions;

/*
 * Test class for the Full Tilt Poker hand history parser.
 *
 * Copyright (C) 2007 Wesley Tansey.
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
namespace HandParsers
{

    [TestFixture]
    public class FullTiltParserTest
    {
        #region Test hands
        public const string NoLimitHand1_Text =
            @"Full Tilt Poker Game #3424241043: Table TestingZone (6 max) - $1/$2 - No Limit Hold'em - 14:49:26 ET - 2007/09/24
Seat 1: scaredGuy ($206)
Seat 2: scaredGuy2 ($209)
Seat 3: SuperCoolPlayer ($281.30)
Seat 4: Lucky Player ($293.75)
Seat 5: scaredGuy4 ($140)
Seat 6: scaredGuy3 ($72.95)
SuperCoolPlayer posts the small blind of $1
Lucky Player posts the big blind of $2
The button is in seat #2
*** HOLE CARDS ***
Dealt to SuperCoolPlayer [6s Qs]
scaredGuy3 folds
scaredGuy folds
scaredGuy2 folds
SuperCoolPlayer calls $1
Lucky Player checks
*** FLOP *** [6h 6c Th]
SuperCoolPlayer bets $2
Lucky Player raises to $10
SuperCoolPlayer calls $8
*** TURN *** [6h 6c Th] [2s]
SuperCoolPlayer checks
Lucky Player bets $14
SuperCoolPlayer calls $14
*** RIVER *** [6h 6c Th 2s] [As]
SuperCoolPlayer has 15 seconds left to act
SuperCoolPlayer bets $40
Lucky Player raises to $267.75, and is all in
SuperCoolPlayer calls $215.30, and is all in
Uncalled bet of $12.45 returned to Lucky Player
*** SHOW DOWN ***
Lucky Player shows [6d Ts] a full house, Sixes full of Tens
SuperCoolPlayer mucks
Lucky Player wins the pot ($559.60) with a full house, Sixes full of Tens
SuperCoolPlayer is sitting out
*** SUMMARY ***
Total pot $562.60 | Rake $3
Board: [6h 6c Th 2s As]
Seat 1: scaredGuy didn't bet (folded)
Seat 2: scaredGuy2 (button) didn't bet (folded)
Seat 3: SuperCoolPlayer (small blind) mucked [6s Qs] - three of a kind, Sixes
Seat 4: Lucky Player (big blind) showed [6d Ts] and won ($559.60) with a full house, Sixes full of Tens
Seat 5: scaredGuy4 is sitting out
Seat 6: scaredGuy3 didn't bet (folded)
";
        #endregion

        //This test is just here to ease my regular expression tinkering.
        [Test]
        public void RegexTest()
        {
            Regex potsExp =
            new Regex(
        @"([a-zA-Z0-9\s]*)\s(wins|ties for)\s(the\s(high\s|low\s)?(main\s|side\s)?pot|(high\s|low\s)?side\spot\s#([0-9]+))\s\(\$?((,?[0-9]?[0-9]?[0-9])+(\.\d\d)?)\)",
            RegexOptions.Compiled);

            Regex shownHandsExp =
            new Regex(@"Seat\s([0-9]+):\s([a-zA-Z0-9\s]*)\s(\([a-z\s]+\)\s)?(showed|mucked)\s\[([2-9AKQJT][cdhs])\s([2-9AKQJT][cdhs])(\s([2-9AKQJT][cdhs])\s([2-9AKQJT][cdhs]))?\]",
            RegexOptions.Compiled);

            Regex stacksExp =
            new Regex(@"Seat\s([0-9]+):\s([a-zA-Z0-9\s]*)\s\(\$?([0-9]+(\.\d\d)?)\)",
            RegexOptions.Compiled);

            //joey whiterock ties for the low side pot ($0.25) with 8,7,6,2,A

            for (Match m = stacksExp.Match(
        @"Seat 6: SofaKingInsane ($1.85)"
        ); m.Success; m = m.NextMatch())
            {
                GroupCollection gc = m.Groups;

                System.Console.WriteLine("The number of captures: " + gc.Count);
                // Group 0 is the entire matched string itself
                // while Group 1 is the first group to be captured.
                for (int i = 0; i < gc.Count; i++)
                {
                    Group g = gc[i];
                    System.Console.WriteLine("[" + i + "]" + g.Value);
                }
            }
        }


        FullTiltParser parser = new FullTiltParser();
        PokerHand NoLimitHand1;

        [TestFixtureSetUp]
        public void SetUp()
        {
            NoLimitHand1 = parser.ParseString(NoLimitHand1_Text).Hands[0];
        }

        [Test]
        public void HandNumber()
        {
            Assert.AreEqual("3424241043", NoLimitHand1.Context.ID);
        }

        [Test]
        public void TableName()
        {
            Assert.AreEqual("Table TestingZone (6 max)", NoLimitHand1.Context.Table);
        }

        [Test]
        public void TableStakes()
        {
            Assert.AreEqual(1, NoLimitHand1.Context.SmallBlind, "Small blind incorrect");
            Assert.AreEqual(2, NoLimitHand1.Context.BigBlind, "Big blind incorrect");
            Assert.AreEqual(0, NoLimitHand1.Context.Ante, "Ante incorrect");
        }

        [Test]
        public void GameFormat()
        {
            Assert.AreEqual(HandHistory.GameFormat.CashGame, NoLimitHand1.Context.Format);
        }

        [Test]
        public void BettingTypeAndPokerVariant()
        {
            Assert.AreEqual(PokerVariant.TexasHoldEm, NoLimitHand1.Context.PokerVariant, "Poker variant incorrect");
            Assert.AreEqual(BettingType.NoLimit, NoLimitHand1.Context.BettingType, "Betting type incorrect");
        }

        [Test]
        public void TimeStamp()
        {
            DateTime expected = new DateTime(2007, 9, 24, 14, 49, 26);
            Assert.AreEqual(expected, NoLimitHand1.Context.TimeStamp);
        }

        [Test]
        public void LoadPlayers()
        {
            Assert.AreEqual(NoLimitHand1.Players[0].Name, "scaredGuy");
            Assert.AreEqual(NoLimitHand1.Players[0].Seat, 1);
            Assert.AreEqual(NoLimitHand1.Players[0].Stack, 206);
            
            Assert.AreEqual(NoLimitHand1.Players[1].Name, "scaredGuy2");
            Assert.AreEqual(NoLimitHand1.Players[1].Seat, 2);
            Assert.AreEqual(NoLimitHand1.Players[1].Stack, 209);

            Assert.AreEqual(NoLimitHand1.Players[2].Name, "SuperCoolPlayer");
            Assert.AreEqual(NoLimitHand1.Players[2].Seat, 3);
            Assert.AreEqual(NoLimitHand1.Players[2].Stack, 281.3);

            Assert.AreEqual(NoLimitHand1.Players[3].Name, "Lucky Player");
            Assert.AreEqual(NoLimitHand1.Players[3].Seat, 4);
            Assert.AreEqual(NoLimitHand1.Players[3].Stack, 293.75);

            Assert.AreEqual(NoLimitHand1.Players[4].Name, "scaredGuy4");
            Assert.AreEqual(NoLimitHand1.Players[4].Seat, 5);
            Assert.AreEqual(NoLimitHand1.Players[4].Stack, 140);

            Assert.AreEqual(NoLimitHand1.Players[5].Name, "scaredGuy3");
            Assert.AreEqual(NoLimitHand1.Players[5].Seat, 6);
            Assert.AreEqual(NoLimitHand1.Players[5].Stack, 72.95);
        }

        [Test]
        public void BlindPostings()
        {
            Assert.AreEqual(NoLimitHand1.Blinds[0].Type, BlindType.SmallBlind);
            Assert.AreEqual(NoLimitHand1.Blinds[0].Player, "SuperCoolPlayer");
            Assert.AreEqual(NoLimitHand1.Blinds[0].Amount, 1);

            Assert.AreEqual(NoLimitHand1.Blinds[1].Type, BlindType.BigBlind);
            Assert.AreEqual(NoLimitHand1.Blinds[1].Player, "Lucky Player");
            Assert.AreEqual(NoLimitHand1.Blinds[1].Amount, 2);
        }

        [Test]
        public void Button()
        {
            Assert.AreEqual(2, NoLimitHand1.Context.Button);
        }

        [Test]
        public void Hero()
        {
            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.Hero);
        }

        [Test]
        public void HoleCards()
        {
            Assert.AreEqual("6s", NoLimitHand1.HoleCards[0].ToString());
            Assert.AreEqual("Qs", NoLimitHand1.HoleCards[1].ToString());
        }

        [Test]
        public void Preflop()
        {
            Assert.AreEqual("scaredGuy3", NoLimitHand1.Rounds[0].Actions[0].Player);
            Assert.AreEqual(ActionType.Fold, NoLimitHand1.Rounds[0].Actions[0].Type);


            Assert.AreEqual("scaredGuy", NoLimitHand1.Rounds[0].Actions[1].Player);
            Assert.AreEqual(ActionType.Fold, NoLimitHand1.Rounds[0].Actions[1].Type);

            Assert.AreEqual("scaredGuy2", NoLimitHand1.Rounds[0].Actions[2].Player);
            Assert.AreEqual(ActionType.Fold, NoLimitHand1.Rounds[0].Actions[2].Type);

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.Rounds[0].Actions[3].Player);
            Assert.AreEqual(ActionType.Call, NoLimitHand1.Rounds[0].Actions[3].Type);
            Assert.AreEqual(1, NoLimitHand1.Rounds[0].Actions[3].Amount);

            Assert.AreEqual("Lucky Player", NoLimitHand1.Rounds[0].Actions[4].Player);
            Assert.AreEqual(ActionType.Check, NoLimitHand1.Rounds[0].Actions[4].Type);

        }

        [Test]
        public void Flop()
        {
            Assert.AreEqual("6h", NoLimitHand1.Flop.CommunityCards[0].ToString());
            Assert.AreEqual("6c", NoLimitHand1.Flop.CommunityCards[1].ToString());
            Assert.AreEqual("Th", NoLimitHand1.Flop.CommunityCards[2].ToString());

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.Flop.Actions[0].Player);
            Assert.AreEqual(ActionType.Bet, NoLimitHand1.Flop.Actions[0].Type);
            Assert.AreEqual(2, NoLimitHand1.Flop.Actions[0].Amount);

            Assert.AreEqual("Lucky Player", NoLimitHand1.Flop.Actions[1].Player);
            Assert.AreEqual(ActionType.Raise, NoLimitHand1.Flop.Actions[1].Type);
            Assert.AreEqual(10, NoLimitHand1.Flop.Actions[1].Amount);

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.Flop.Actions[2].Player);
            Assert.AreEqual(ActionType.Call, NoLimitHand1.Flop.Actions[2].Type);
            Assert.AreEqual(8, NoLimitHand1.Flop.Actions[2].Amount);
        }

        [Test]
        public void Turn()
        {
            Assert.AreEqual("2s", NoLimitHand1.Turn.CommunityCards[0].ToString());

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.Turn.Actions[0].Player);
            Assert.AreEqual(ActionType.Check, NoLimitHand1.Turn.Actions[0].Type);


            Assert.AreEqual("Lucky Player", NoLimitHand1.Turn.Actions[1].Player);
            Assert.AreEqual(ActionType.Bet, NoLimitHand1.Turn.Actions[1].Type);
            Assert.AreEqual(14, NoLimitHand1.Turn.Actions[1].Amount);

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.Turn.Actions[2].Player);
            Assert.AreEqual(ActionType.Call, NoLimitHand1.Turn.Actions[2].Type);
            Assert.AreEqual(14, NoLimitHand1.Turn.Actions[2].Amount);
        }

        [Test]
        public void River()
        {
            Assert.AreEqual("As", NoLimitHand1.River.CommunityCards[0].ToString());

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.River.Actions[0].Player);
            Assert.AreEqual(ActionType.Bet, NoLimitHand1.River.Actions[0].Type);
            Assert.AreEqual(40, NoLimitHand1.River.Actions[0].Amount);

            Assert.AreEqual("Lucky Player", NoLimitHand1.River.Actions[1].Player);
            Assert.AreEqual(ActionType.Raise, NoLimitHand1.River.Actions[1].Type);
            Assert.AreEqual(267.75, NoLimitHand1.River.Actions[1].Amount);
            Assert.IsTrue(NoLimitHand1.River.Actions[1].AllIn);

            Assert.AreEqual("SuperCoolPlayer", NoLimitHand1.River.Actions[2].Player);
            Assert.AreEqual(ActionType.Call, NoLimitHand1.River.Actions[2].Type);
            Assert.AreEqual(215.30, NoLimitHand1.River.Actions[2].Amount);
            Assert.IsTrue(NoLimitHand1.River.Actions[2].AllIn);

            Assert.AreEqual("Lucky Player", NoLimitHand1.River.Actions[3].Player);
            Assert.AreEqual(ActionType.Returned, NoLimitHand1.River.Actions[3].Type);
            Assert.AreEqual(12.45, NoLimitHand1.River.Actions[3].Amount);

        }

        [Test]
        public void Pots()
        {
            Assert.AreEqual(2, NoLimitHand1.Results.Length);
            HandResult LuckyPlayerResult = NoLimitHand1.PlayerResult("Lucky Player");
            Assert.AreEqual(1, LuckyPlayerResult.WonPots.Length);
            Assert.AreEqual(559.60, LuckyPlayerResult.WonPots[0].Amount);
            Assert.AreEqual(0, LuckyPlayerResult.WonPots[0].Number);
        }

        [Test]
        public void ShownHands()
        {
            HandResult LuckyPlayerResult = NoLimitHand1.PlayerResult("Lucky Player");
            HandResult SuperCoolPlayerResult = NoLimitHand1.PlayerResult("SuperCoolPlayer");
            Assert.AreEqual("6d Ts", Card.ToString(LuckyPlayerResult.HoleCards));
            Assert.AreEqual("6s Qs", Card.ToString(SuperCoolPlayerResult.HoleCards));
        }
    }
}
#endif
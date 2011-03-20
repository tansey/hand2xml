using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PokerHandHistory;
using HandParserInterface;
using System.Text.RegularExpressions;

/*
 * Full Tilt Poker hand history parser.
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
namespace HandParsers
{
    /// <summary>
    /// Parses a text hand history from Full Tilt Poker
    /// </summary>
    public class FullTiltParser : IHandParser
    {
        public const string NAME_EXPRESSION = @"([a-zA-Z0-9\s_\-]*)";
        public const string CHIPS_EXPRESSION = @"\$?((,?[0-9]?[0-9]?[0-9])+(\.\d\d)?)";
        Regex stacksExp =
            new Regex(@"Seat\s([0-9]+):\s" + NAME_EXPRESSION + @"\s\(" + CHIPS_EXPRESSION + @"\)",
            RegexOptions.Compiled);

        Regex actionExp =
            new Regex(
        NAME_EXPRESSION + @"\s(posts(\s(the|a)\s(dead\s)?(small|big)\sblind\sof)?|antes|checks|bets|calls|raises\sto|folds)\s?" + CHIPS_EXPRESSION + @"?(,\sand\sis\sall\sin)?",
            RegexOptions.Compiled);

        Regex uncalledExp =
            new Regex(
        @"Uncalled\sbet\sof\s" + CHIPS_EXPRESSION + @"\sreturned\sto\s" + NAME_EXPRESSION,
            RegexOptions.Compiled);

        Regex potsExp =
            new Regex(
         NAME_EXPRESSION + @"\s(wins|ties for)\s(the\s(high\s|low\s)?(main\s|side\s)?pot|(high\s|low\s)?side\spot\s#([0-9]+))\s\(" + CHIPS_EXPRESSION + @"\)",
            RegexOptions.Compiled);

        Regex shownHandsExp =
            new Regex(@"Seat\s([0-9]+):\s" + NAME_EXPRESSION + @"\s(\([a-z\s]+\)\s)?(showed|mucked)\s\[([2-9AKQJT][cdhs])\s([2-9AKQJT][cdhs])(\s([2-9AKQJT][cdhs])\s([2-9AKQJT][cdhs]))?\]",
            RegexOptions.Compiled);

        #region IHandParser Members
        public PokerHandXML ParseString(string hand)
        {
            TextReader reader = new StringReader(hand);
            return Parse(reader);
        }

        public PokerHandXML ParseFile(string filename)
        {
            TextReader file = new StreamReader(filename);
            return Parse(file);
        }

        public PokerHandXML Parse(TextReader file)
        {
            List<PokerHand> hands = new List<PokerHand>();
            
            string line;
            bool inHand = false;
            StringBuilder handText = new StringBuilder();
            List<string> lines = new List<string>();
            Console.WriteLine("Starting parse");
            while ((line = file.ReadLine()) != null)
            {
                //skip blank lines
                if (!inHand && line.Length == 0)
                    continue;

                if (inHand && line.Length == 0)
                {
                    Console.WriteLine("Found hand");
                    PokerHand hand = parseHand(lines, handText.ToString());
                    if(hand != null)
                        hands.Add(hand);
                    inHand = false;
                    continue;
                }

                if (!inHand)
                {
                    lines.Clear();
                    handText = new StringBuilder();
                    inHand = true;
                }

                handText.AppendLine(line);
                lines.Add(line);
            }

            if (inHand)
            {
                Console.WriteLine("Found hand");
                PokerHand hand = parseHand(lines, handText.ToString());
                if (hand != null)
                    hands.Add(hand);
            }

            PokerHandXML result = new PokerHandXML();
            result.Hands = hands.ToArray();
            return result;
        }

        #endregion
        int printed = 0;
        //Note that lines is only passed for efficiency, it could be obtained
        //by just splitting handText
        private PokerHand parseHand(List<string> lines, string handText)
        {
            PokerHand hand = new PokerHand();

            #region setup variables
            int start;
            int end;
            int curLine;
            List<Round> rounds = new List<Round>();
            #endregion

            #region Make sure it's a Full Tilt hand
            if (!handText.StartsWith("Full Tilt Poker"))
                throw new InvalidHandFormatException(handText);
            #endregion

            #region Skip partial hands
            if(lines[0].EndsWith("(partial)"))
                return null;
            #endregion

            hand.Context.Online = true;
            hand.Context.Site = "Full Tilt Poker";

#if DEBUG
            Console.WriteLine("Hand Number");
#endif

            #region Get the hand number
            start = handText.IndexOf('#') + 1;
            end = handText.IndexOf(':');
            hand.Context.ID = handText.Substring(start, end - start);
            //Console.WriteLine("ID: {0}", hand.Context.ID);
            if (printed < 2)
            {
                Console.WriteLine("Hand Text: {0}", handText);
                printed++;
            }
            #endregion

#if DEBUG
            Console.WriteLine("Table Name");
#endif

            #region Get the table name
            start = end + 2;
            end = handText.IndexOf('-', start);
            hand.Context.Table = handText.Substring(start, end - start - 1);
            #endregion

#if DEBUG
            Console.WriteLine("Blinds");
#endif

            #region Get the blinds and game format
            start = end + 2;
            end = handText.IndexOf('-', start);
            string blindsAndAntes = handText.Substring(start, end - start);
            int blindSeparator = blindsAndAntes.IndexOf('/');
            string smallBlindText = blindsAndAntes.Substring(0, blindSeparator);
            if (smallBlindText[0] == '$')
                smallBlindText = smallBlindText.Substring(1);
            hand.Context.SmallBlind = Decimal.Parse(smallBlindText.Trim());

            int bigBlindStart = blindSeparator + 1;
            int bigBlindEnd = blindsAndAntes.IndexOf(' ', bigBlindStart);
            string bigBlindText = blindsAndAntes.Substring(bigBlindStart, bigBlindEnd - bigBlindStart);
            if (bigBlindText[0] == '$')
                bigBlindText = bigBlindText.Substring(1);
            hand.Context.BigBlind = Decimal.Parse(bigBlindText.Trim());

            int anteIndex = blindsAndAntes.IndexOf("Ante");
            if (anteIndex != -1)
            {
                int anteStart = anteIndex + 5;
                string anteText = blindsAndAntes.Substring(anteStart);
                if (anteText[0] == '$')
                    anteText = anteText.Substring(1);
                hand.Context.Ante = Decimal.Parse(anteText.Trim());
            }
            #endregion

#if DEBUG
            Console.WriteLine("Game Format");
#endif

            #region Get the game format
            if (hand.Context.Table.Contains("Sit & Go"))
                hand.Context.Format = GameFormat.SitNGo;
            else if (blindsAndAntes.Contains("$"))
                hand.Context.Format = GameFormat.CashGame;
            else
                hand.Context.Format = GameFormat.MultiTableTournament;
            #endregion

#if DEBUG
            Console.WriteLine("Poker Variant and Betting Type");
#endif

            #region Get the betting type and poker variant
            start = end + 2;
            end = handText.IndexOf('-', start);
            string typeAndVariant = handText.Substring(start, end - start);

            int capIndex = typeAndVariant.IndexOf(" Cap ");
            if (capIndex != -1)
            {
                string capAmountText = handText.Substring(start, capIndex - start);
                hand.Context.CapAmount = Decimal.Parse(capAmountText.Trim().Replace("$", ""));
                hand.Context.CapAmountSpecified = true;
                hand.Context.Capped = true;
            }
            else
            {
                hand.Context.CapAmountSpecified = false;
                hand.Context.Capped = false;
            }

            if(typeAndVariant.Contains("Pot Limit"))
                hand.Context.BettingType = BettingType.PotLimit;
            else if(typeAndVariant.Contains("No Limit"))
                hand.Context.BettingType = BettingType.NoLimit;
            else
                hand.Context.BettingType = BettingType.FixedLimit;

            if (typeAndVariant.Contains("Hold'em"))
                hand.Context.PokerVariant = PokerVariant.TexasHoldEm;
            else if(typeAndVariant.Contains("Omaha Hi"))
                hand.Context.PokerVariant = PokerVariant.OmahaHi;
            else
                hand.Context.PokerVariant = PokerVariant.OmahaHiLo;
            #endregion

#if DEBUG
            Console.WriteLine("Time Stamp");
#endif

            #region Get the date and time
            start = end + 2;
            end = handText.IndexOf(' ', start);
            string timeText = handText.Substring(start, end - start);
            string[] timeTokens = timeText.Split(':');
            int hour = Int32.Parse(timeTokens[0]);
            int minute = Int32.Parse(timeTokens[1]);
            int second = Int32.Parse(timeTokens[2]);

            start = handText.IndexOf('-', end) + 2;
            string dateText = lines[0].Substring(start);
            string[] dateTokens = dateText.Split('/');
            int year = Int32.Parse(dateTokens[0]);
            int month = Int32.Parse(dateTokens[1]);
            int day = Int32.Parse(dateTokens[2]);

            hand.Context.TimeStamp = new DateTime(year, month, day, hour, minute, second);
            #endregion

#if DEBUG
            Console.WriteLine("Players");
#endif

            #region Create the players
            List<Player> players = new List<Player>();
            curLine = 1;
            for (Match m = stacksExp.Match(lines[curLine]); m.Success; m = stacksExp.Match(lines[curLine]))
            {
                if (m.Success)
                {

                    GroupCollection gc = m.Groups;

                    Player p = new Player();
                    p.Seat = Int32.Parse(gc[1].Value);
                    p.Name = gc[2].Value;
                    p.Stack = Decimal.Parse(gc[3].Value);

                    players.Add(p);

                    curLine++;
                }
            }

            hand.Players = players.ToArray();
            #endregion

#if DEBUG
            Console.WriteLine("Blinds and Antes Posted");
#endif

            #region Get the blinds and antes posted
            List<Blind> blinds = new List<Blind>();
            for (; !lines[curLine].StartsWith("The button is in seat #"); curLine++)
            {

                for (Match m = actionExp.Match(lines[curLine]); m.Success; m = m.NextMatch())
                {
                    GroupCollection gc = m.Groups;

                    Blind blind = new Blind();
                    blind.Player = gc[1].Value;

                    if (gc[2].Value == "antes")
                        blind.Type = BlindType.Ante;
                    else if (gc[6].Value == "small")
                        blind.Type = gc[5].Value.Trim() == "dead" ? BlindType.DeadBlind : BlindType.SmallBlind;
                    else if (gc[6].Value == "big")
                        blind.Type = gc[5].Value.Trim() == "dead" ? BlindType.DeadBlind : BlindType.BigBlind;
                    else if (gc[2].Value == "posts")
                        blind.Type = BlindType.LateBlind;
                    else
                        throw new Exception("Unknown blind type: " + lines[curLine]);
                    if (lines[curLine].Contains("dead"))
                    {
                        for (int i = 0; i < gc.Count; i++)
                            Console.WriteLine("{0}: \"{1}\"", i, gc[i].Value);
                        Console.WriteLine("Found as {0}", blind.Type.ToString());
                    }
                    blind.Amount = Decimal.Parse(gc[7].Value);
                    blind.AllIn = gc[9].Value.Length == 15;
                    blinds.Add(blind);
                }
            }
            hand.Blinds = blinds.ToArray();
            #endregion

#if DEBUG
            Console.WriteLine("Button");
#endif

            #region Get the button
            string buttonText = lines[curLine].Substring(lines[curLine].IndexOf('#') + 1);
            hand.Context.Button = Int32.Parse(buttonText);
            curLine++;
            #endregion

#if DEBUG
            Console.WriteLine("Hole Cards and Hero");
#endif

            #region Get the hole cards and the name of the hero
            while (!lines[curLine].StartsWith("Dealt to "))
                curLine++;

            start = "Dealt to ".Length;
            end = lines[curLine].IndexOf(" [", start);
            hand.Hero = lines[curLine].Substring(start, end - start);

            start = end + 2;
            List<Card> holecards = new List<Card>();
            holecards.Add(new Card(lines[curLine].Substring(start, 2)));
            holecards.Add(new Card(lines[curLine].Substring(start + 3, 2)));
            if (hand.Context.PokerVariant != PokerVariant.TexasHoldEm)
            {
                holecards.Add(new Card(lines[curLine].Substring(start + 6, 2)));
                holecards.Add(new Card(lines[curLine].Substring(start + 9, 2)));
            }
            hand.HoleCards = holecards.ToArray();
            curLine++;
            #endregion

#if DEBUG
            Console.WriteLine("Preflop Actions");
#endif
            #region Preflop Actions
            rounds.Add(new Round());
            rounds[0].Actions = getRoundActions(lines, ref curLine);
            #endregion

#if DEBUG
            Console.WriteLine("Flop Actions");
#endif
            #region Flop Actions and Community Cards
            if (lines[curLine].StartsWith("*** FLOP ***"))
            {
                rounds.Add(new Round());

                start = lines[curLine].IndexOf('[') + 1;
                Card[] flop = new Card[3];
                flop[0] = new Card(lines[curLine].Substring(start, 2));
                flop[1] = new Card(lines[curLine].Substring(start + 3, 2));
                flop[2] = new Card(lines[curLine].Substring(start + 6, 2));
                rounds[1].CommunityCards = flop;

                curLine++;

                rounds[1].Actions = getRoundActions(lines, ref curLine);
            }
            #endregion

#if DEBUG
            Console.WriteLine("Turn Actions");
#endif
            #region Turn Actions and Community Card
            if (lines[curLine].StartsWith("*** TURN ***"))
            {
                rounds.Add(new Round());

                start = lines[curLine].LastIndexOf('[') + 1;
                Card[] turn = new Card[1];
                turn[0] = new Card(lines[curLine].Substring(start, 2));
                rounds[2].CommunityCards = turn;

                curLine++;

                rounds[2].Actions = getRoundActions(lines, ref curLine);
            }
            #endregion

#if DEBUG
            Console.WriteLine("River Actions");
#endif
            #region River Actions and Community Card
            if (lines[curLine].StartsWith("*** RIVER ***"))
            {
                rounds.Add(new Round());

                start = lines[curLine].LastIndexOf('[') + 1;
                Card[] river = new Card[1];
                river[0] = new Card(lines[curLine].Substring(start, 2));
                rounds[3].CommunityCards = river;

                curLine++;

                rounds[3].Actions = getRoundActions(lines, ref curLine);
            }
            #endregion

            #region Set rounds
            hand.Rounds = rounds.ToArray();
            #endregion

#if DEBUG
            Console.WriteLine("Results");
#endif

            #region Get pots won
            List<HandResult> results = new List<HandResult>();
            List<List<Pot>> pots = new List<List<Pot>>();
            for (; !lines[curLine].StartsWith("*** SUMMARY"); curLine++)
            {
                Match m = potsExp.Match(lines[curLine]);
                if (m.Success)
                {
                    
                    GroupCollection gc = m.Groups;

                    Pot p = new Pot();
                    p.Amount = Decimal.Parse(gc[8].Value);

                    if (gc[7].Value.Length > 0)
                        p.Number = Int32.Parse(gc[7].Value);
                    else if ((gc[5].Value.Length == 0 && gc[3].Value == "the pot")
                        || gc[5].Value == "main ")
                        p.Number = 0;
                    else if (gc[5].Length > 0)
                        p.Number = 1;

                    HandResult result = null;
                    List<Pot> potList = null;
                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].Player == gc[1].Value)
                        {
                            result = results[i];
                            potList = pots[i];
                            break;
                        }
                    }

                    if (result == null)
                    {
                        result = new HandResult(gc[1].Value);
                        potList = new List<Pot>();

                        results.Add(result);
                        pots.Add(potList);
                    }

                    potList.Add(p);
                }
            }

            //add the pots to the model
            for (int i = 0; i < results.Count; i++)
            {
                results[i].WonPots = pots[i].ToArray();
            }

            //get the rake, if any
            if (hand.Context.Format == GameFormat.CashGame)
            {
                for (; !lines[curLine].StartsWith("Total pot") 
                    || lines[curLine].Contains(":")
                    || !lines[curLine].Contains("| Rake "); curLine++)
                {
                }
                int rakeStart = lines[curLine].LastIndexOf("| Rake ") + "| Rake ".Length;
                string rakeText = lines[curLine].Substring(rakeStart).Replace("$", "");
                hand.Rake = Decimal.Parse(rakeText);
            }

            #endregion

#if DEBUG
            Console.WriteLine("Shown Hands");
#endif

            #region Get the shown down hands
            for (; curLine < lines.Count; curLine++)
            {
                Match m = shownHandsExp.Match(lines[curLine]);
                if (m.Success)
                {

                    GroupCollection gc = m.Groups;

                    List<Card> shownCards = new List<Card>();
                    shownCards.Add(new Card(gc[5].Value));
                    shownCards.Add(new Card(gc[6].Value));
                    if (hand.Context.PokerVariant != PokerVariant.TexasHoldEm)
                    {
                        shownCards.Add(new Card(gc[8].Value));
                        shownCards.Add(new Card(gc[9].Value));
                    }
                    string player = gc[2].Value;

                    HandResult hr = null;
                    foreach (HandResult curResult in results)
                        if (curResult.Player == player)
                        {
                            hr = curResult;
                            break;
                        }

                    if (hr == null)
                    {
                        hr = new HandResult(player);
                        results.Add(hr);
                    }

                    hr.HoleCards = shownCards.ToArray();
                }
            }
            #endregion

            #region Set the results
            hand.Results = results.ToArray();
            #endregion

#if DEBUG
            Console.WriteLine("Done");
#endif


            return hand;
        }

        #region Helper methods
        private Action[] getRoundActions(List<string> lines, ref int curLine)
        {
            List<Action> actions = new List<Action>();
            for (; !lines[curLine].StartsWith("*** "); curLine++)
            {
                if (potsExp.Match(lines[curLine]).Success)
                    break;

                Match m =actionExp.Match(lines[curLine]);
                if ( m.Success )
                {
                    GroupCollection gc = m.Groups;

                    Action a = new Action();
                    a.Player = gc[1].Value;

                    switch (gc[2].Value)
                    {
                        case "folds": a.Type = ActionType.Fold;
                            break;
                        case "checks": a.Type = ActionType.Check;
                            break;
                        case "calls": a.Type = ActionType.Call;
                                      a.Amount = Decimal.Parse(gc[7].Value);
                                      a.AllIn = gc[10].Value.Length == 15;
                            break;
                        case "bets": a.Type = ActionType.Bet;
                                     a.Amount = Decimal.Parse(gc[7].Value);
                                     a.AllIn = gc[10].Value.Length == 15;          
                            break;
                        case "raises to": a.Type = ActionType.Raise;
                                          a.Amount = Decimal.Parse(gc[7].Value);
                                          a.AllIn = gc[10].Value.Length == 15;          
                            break;
                        default: throw new Exception("Unknown action type: " + lines[curLine]);
                    }

                    actions.Add(a);
                    continue;
                }

                m = uncalledExp.Match(lines[curLine]);
                if (m.Success)
                {
                    GroupCollection gc = m.Groups;

                    Action a = new Action();
                    a.Player = gc[4].Value;
                    a.Amount = Decimal.Parse(gc[1].Value);
                    a.Type = ActionType.Returned;

                    actions.Add(a);
                }
                
            }
            return actions.ToArray();
        }

        #endregion

        #region IPlugin Members

        public string Author
        {
            get { return "Wesley Tansey"; }
        }

        public string Description
        {
            get { return "Parses a Full Tilt Poker hand history file."; }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
            
        }

        public string Name
        {
            get { return "Full Tilt Poker"; }
        }

        public string Version
        {
            get { return "v0.1"; }
        }

        #endregion

        #region IHandParser Members

        public string Switch
        {
            get { return "ft"; }
        }

        #endregion
    }
}

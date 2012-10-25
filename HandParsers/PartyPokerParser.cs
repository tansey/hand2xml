using System;
using System.Collections.Generic;
using System.Text;
using HandParserInterface;
using System.Text.RegularExpressions;
using PokerHandHistory;
using System.IO;
using System.Linq;
using Action = PokerHandHistory.Action;

namespace HandParsers
{
    public class PartyPokerParser : IHandParser
    {
        public const string NAME_EXPRESSION = @"([a-zA-Z0-9_\-]*)";
        public const string CHIPS_EXPRESSION = @"\$?((,?[0-9]?[0-9]?[0-9])+(\.\d\d)?)";
        public const string CARD_EXPRESSION = @"[2-9AKQJT][cdhs]";
        public const string HAND_START_TEXT = @"***** Hand History for Game";

        // Group 1: Seat number
        // Group 2: Name
        // Group 3: Chips
        Regex stacksExp =
        new Regex(@"Seat\s([0-9]+):\s" + NAME_EXPRESSION + @"\s\(\s" + CHIPS_EXPRESSION + @"\s\)",
        RegexOptions.Compiled);

        // Group 1: Player name
        // Group 2: Action type
        // Group 3-4: Chips expression
        Regex actionExp =
        new Regex(NAME_EXPRESSION + @"\s(posts\ssmall\sblind|posts\sbig\sblind|posts\sthe\sante|is\sall\-In|folds|checks|bets|calls|raises)\s(\[" + CHIPS_EXPRESSION + @"\])?",
        RegexOptions.Compiled);

        // Group 1: Name expression
        // Group 2: Chips expression
        // Group 6: Either one of: "from the main pot", "from side pot" (optional)
        // Group 7: Side pot id (Optional)
        Regex potsExp =
            new Regex(
         NAME_EXPRESSION + @"\swins\s" + CHIPS_EXPRESSION + @"(\sfrom\s*(the\smain\spot|side\spot\s#([0-9])))?",
            RegexOptions.Compiled);

        Regex shownHandsExp =
            new Regex(NAME_EXPRESSION + string.Format(@"\sshows\s\[\s({0}),\s({0})(,\s({0}),\s({0}))?\s\]", CARD_EXPRESSION),
            RegexOptions.Compiled);

        // Group 1: Hand number
        Regex gameNumExp = new Regex(@"\*\*\*\*\*\sHand\sHistory\sfor\sGame\s([0-9]+)", RegexOptions.Compiled);

        // Group 1: Table name
        Regex tableNameExp = new Regex(@"Table\s+([a-zA-Z0-9\s\-_]+)", RegexOptions.Compiled);

        // Group 1: Big blind (small bet)
        // Group 4: Big bet
        // Group 7: Day of week
        // Group 8: Month
        // Group 9: Day of month
        // Group 10: Hour (24-hour period)
        // Group 11: Minute
        // Group 12: Second
        // Group 13: Time zone
        // Group 14: Year
        const string DAY_OF_WEEK = "Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday";
        const string MONTH = "January|February|March|April|May|June|July|August|September|October|November|December";
        Regex stakesAndDateExp = new Regex(string.Format(@"^\${0}/\${0}\s[a-zA-z\s']+\-\s({1}),\s({2})\s([0-9]+),\s([0-9][0-9]):([0-9][0-9]):([0-9][0-9])\s([A-Z]+)\s([0-9][0-9][0-9][0-9])", CHIPS_EXPRESSION, DAY_OF_WEEK, MONTH), RegexOptions.Compiled);

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

                if (inHand && (line.Length == 0 || line == @"Game #<do not remove this line!> starts."))
                {
                    Console.WriteLine("Found hand");
                    PokerHand hand = parseHand(lines, handText.ToString());
                    if (hand != null)
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

        int printed = 0;
        //Note that lines is only passed for efficiency, it could be obtained
        //by just splitting handText
        private PokerHand parseHand(List<string> lines, string handText)
        {
            PokerHand hand = new PokerHand();

            #region setup variables
            int start;
            int end = 0;
            int curLine;
            List<Round> rounds = new List<Round>();
            #endregion

            #region Make sure it's a Full Tilt hand
            if (!handText.StartsWith(HAND_START_TEXT))
                throw new InvalidHandFormatException(handText);
            #endregion

            //#region Skip partial hands
            //if (lines[0].EndsWith("(partial)"))
            //    return null;
            //#endregion

            hand.Context.Online = true;
            hand.Context.Site = Name;

#if DEBUG
            Console.WriteLine("Hand Number");
#endif

            #region Get the hand number
            hand.Context.ID = gameNumExp.Match(handText).Groups[1].Value;
            //Console.WriteLine("ID: {0}", hand.Context.ID);
            if (printed < 2)
            {
                Console.WriteLine("Hand Text: {0}", handText);
                Console.WriteLine("Hand Number: {0}", hand.Context.ID);
                printed++;
            }
            #endregion

#if DEBUG
            Console.WriteLine("Table Name");
#endif

            #region Get the table name
            hand.Context.Table = tableNameExp.Match(handText).Groups[1].Value;
            #endregion

#if DEBUG
            Console.WriteLine(hand.Context.Table);
            Console.WriteLine("Blinds");
#endif

            #region Get the blinds and game format
            var stakesAndDate = stakesAndDateExp.Match(lines.First(line => line.StartsWith("$")));
            hand.Context.SmallBlind = decimal.Parse(stakesAndDate.Groups[1].Value) / 2.0m; // All old pp stakesAndDate were sb = 0.5 * bb
            hand.Context.BigBlind = decimal.Parse(stakesAndDate.Groups[1].Value);
            // Assume no ante.
            #endregion

#if DEBUG
            Console.WriteLine("Small Blind: {0}", hand.Context.SmallBlind);
            Console.WriteLine("Big Blind: {0}", hand.Context.BigBlind);
            Console.WriteLine("Game Format");
#endif

            #region Get the game format
            // TODO: Support more than cash games
            hand.Context.Format = GameFormat.CashGame;
            #endregion

#if DEBUG
            Console.WriteLine("Poker Variant and Betting Type");
#endif

            #region Get the betting type and poker variant
            // Assume playing fixed limit Texas Hold'em
            hand.Context.CapAmountSpecified = false;
            hand.Context.Capped = false;
            hand.Context.BettingType = BettingType.FixedLimit;
            hand.Context.PokerVariant = PokerVariant.TexasHoldEm;
            #endregion

#if DEBUG
            Console.WriteLine("Time Stamp");
#endif

            #region Get the date and time
            int year = int.Parse(stakesAndDate.Groups[14].Value);
            int month = monthToInt(stakesAndDate.Groups[8].Value);
            int day = int.Parse(stakesAndDate.Groups[9].Value);
            int hour = int.Parse(stakesAndDate.Groups[10].Value);
            int minute = int.Parse(stakesAndDate.Groups[11].Value);
            int second = int.Parse(stakesAndDate.Groups[12].Value);
            string timeZone = stakesAndDate.Groups[13].Value;// Note: Currently not considered.

            hand.Context.TimeStamp = new DateTime(year, month, day, hour, minute, second);
            #endregion

#if DEBUG
            Console.WriteLine("DateTime: {0}", hand.Context.TimeStamp.ToString());
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

            #region Terminate parsing of unsupported poker type

            //if ((!typeAndVariant.Contains("Hold'em") && (!typeAndVariant.Contains("Omaha"))))
            //{
            //    return hand;
            //}

            #endregion

#if DEBUG
            Console.WriteLine("Blinds and Antes Posted");
#endif

            #region Get the blinds and antes posted
            List<Blind> blinds = new List<Blind>();
            for (; !lines[curLine].StartsWith("The button is in seat #"); curLine++)
            {
                if (lines[curLine].Contains(" has been canceled"))
                {
                    return hand;
                }

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
            int tempIndex = curLine;
            bool heroType = true;
            while (!lines[curLine].StartsWith("Dealt to "))
            {
                if (lines[curLine].Equals("*** FLOP ***") || lines[curLine].Equals("*** SUMMARY ***"))
                {
                    curLine = tempIndex;
                    heroType = false;
                    break;
                }
                else
                    curLine++;
            }

            if (heroType)
            {
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
            }
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

                Match m = actionExp.Match(lines[curLine]);
                if (m.Success)
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

                //m = uncalledExp.Match(lines[curLine]);
                //if (m.Success)
                //{
                //    GroupCollection gc = m.Groups;

                //    Action a = new Action();
                //    a.Player = gc[4].Value;
                //    a.Amount = Decimal.Parse(gc[1].Value);
                //    a.Type = ActionType.Returned;

                //    actions.Add(a);
                //}

            }
            return actions.ToArray();
        }

        private int monthToInt(string month)
        {
            switch (month)
            {
                case "January": return 1;
                case "February": return 2;
                case "March": return 3;
                case "April": return 4;
                case "May": return 5;
                case "June": return 6;
                case "July": return 7;
                case "August": return 8;
                case "September": return 9;
                case "October": return 10;
                case "November": return 11;
                case "December": return 12;
                default:
                    break;
            }
            throw new Exception("Unknown month: " + month);
        }

        #endregion

        #region IPlugin Members

        public string Author
        {
            get { return "Wesley Tansey"; }
        }

        public string Description
        {
            get { return "Parses a PartyPoker hand history file (~2006 era)."; }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {

        }

        public string Name
        {
            get { return "PartyPoker"; }
        }

        public string Version
        {
            get { return "v1.0"; }
        }

        #endregion

        #region IHandParser Members

        public string Switch
        {
            get { return "oldpp"; }
        }

        #endregion
    }
}

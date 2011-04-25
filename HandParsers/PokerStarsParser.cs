using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PokerHandHistory;
using HandParserInterface;
using System.Text.RegularExpressions;

namespace HandParsers
{
    public class PokerStarsParser : IHandParser
    {
        // Group 1: Currency (Optional)
        // Group 2: Mathces the whole and fractional part together if there is a decimal digit
        // Group 3: Integer part
        // Group 4: Decimal part
        public const string CHIPS_EXPRESSION = @"(\$|€)?((,?[0-9]?[0-9]?[0-9])+(\.\d\d)?)";

        public const string NAME_EXPRESSION = @"(.*)";

        Regex stacksExp =
        new Regex(@"Seat\s([0-9]+):\s" + NAME_EXPRESSION + @"\s\(" + CHIPS_EXPRESSION + @"\sin chips\)",
        RegexOptions.Compiled);

        // Group 1: Player name
        // Group 2: Action type
        // Group 3-4-5-6: Chips expression
        // Group 7: Matches " to "(Optional, expected to match only in raise type action)
        // Group 8-9-10-11: Chips expression 
        // Group 12: All-in
        Regex actionExp =
        new Regex(NAME_EXPRESSION + @":\s(posts\ssmall\sblind|posts\sbig\sblind|posts\sthe\sante|folds|checks|bets|calls|raises)\s" + CHIPS_EXPRESSION + @"?(\sto\s)?" + CHIPS_EXPRESSION + @"?(\sand\sis\sall-in)?",
        RegexOptions.Compiled);

        // Group 1-2-3-4: Chips Expression
        // Group 5: Name expression
        Regex uncalledExp =
        new Regex(
        @"Uncalled\sbet\s\(" + CHIPS_EXPRESSION + @"\)\sreturned\sto\s" + NAME_EXPRESSION,
        RegexOptions.Compiled);

        // Group 1: Name expression
        // Group 2-3-4-5: Chips expression
        // Group 6: Either one of: "from pot", "from main pot", "from side pot"
        // Group 7: Side pot id (Optional)
        Regex potsExp =
        new Regex(
        NAME_EXPRESSION + @"\scollected\s" + CHIPS_EXPRESSION + @"\s(from\spot|from\smain\spot|from\sside\spot(-\d)?)",
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
            TextReader file = new StreamReader(filename, Encoding.Default);
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

        #endregion
        int printed = 0;
        //Note that lines is only passed for efficiency, it could be obtained by just splitting handText
        private PokerHand parseHand(List<string> lines, string handText)
        {
            PokerHand hand = new PokerHand();

            #region setup variables
            int start;
            int end;
            int curLine;
            char currencySymbol;
            List<Round> rounds = new List<Round>();
            #endregion

            // Edited
            #region Make sure it's a PokerStars hand
            if (!handText.StartsWith("PokerStars"))
                throw new InvalidHandFormatException(handText);
            #endregion

            #region Skip partial hands
            if(lines[0].EndsWith("(partial)"))
                return null;
            #endregion

            // Edited
            hand.Context.Online = true;
            hand.Context.Site = "PokerStars";

            #if DEBUG
                        Console.WriteLine("Hand Number");
            #endif

            // Edited
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

            // Edited
            #region Get the table name

            start = lines[1].IndexOf('\'');
            end = lines[1].IndexOf('\'', 7);

            hand.Context.Table = (lines[1].Substring(start + 1, end - start - 1));

            #endregion

            #if DEBUG
                        Console.WriteLine("Blinds");
            #endif

            // Edited
            #region Get the blinds, antes and currency
            start = handText.IndexOf('(');
            end = handText.IndexOf('/');
            string smallBlind = handText.Substring(start + 1, end - start - 1);

            start = end;
            end = handText.IndexOf(' ', start);
            string bigBlind = handText.Substring(start + 1, end - start - 1);

            if (smallBlind[0].Equals('$') || smallBlind[0].Equals('€'))
            {
                if (smallBlind[0].Equals('$'))
                {
                    hand.Context.Currency = "USD";
                    currencySymbol = '$';
                }
                else
                {
                    hand.Context.Currency = "EUR";
                    currencySymbol = '€';
                }
                hand.Context.SmallBlind = Decimal.Parse(smallBlind.Substring(1).Trim());
                hand.Context.BigBlind = Decimal.Parse(bigBlind.Substring(1).Trim());
            }
            else
            {
                currencySymbol = 'T';
                hand.Context.SmallBlind = Decimal.Parse(smallBlind.Trim());
                hand.Context.BigBlind = Decimal.Parse(bigBlind.Substring(0,bigBlind.Length-1).Trim());
            }
                
            // Antes are not written on the first line in PokerStars hand histories
            // Ante amount will have to be extracted from post blind lines.
            // Smallest possible post blind line index is 4
            curLine = 4;
            while(!lines[curLine].Equals("*** HOLE CARDS ***"))
            {
                if (lines[curLine].Contains(" posts the ante "))
                {
                    start = lines[curLine].IndexOf("ante");
                    string anteText = lines[curLine].Substring(start+5);

                    if (anteText[0].Equals(currencySymbol))
                        anteText = anteText.Substring(1);

                    hand.Context.Ante = Decimal.Parse(anteText.Trim());

                    break;
                }

                curLine++;
            }
            #endregion

            #if DEBUG
                        Console.WriteLine("Game Format");
            #endif

            #region Get the game format
            // Stars does not have different notations for Sit&Go's and MTTs.
            // All tournament hand histories are of the same format.
            if (currencySymbol.Equals('T'))
            {
                if (lines[0].Contains(": Tournament #"))
                    hand.Context.Format = GameFormat.Tournament;
                else
                    hand.Context.Format = GameFormat.PlayMoney;
            }
            else
                hand.Context.Format = GameFormat.CashGame;
            #endregion

            #if DEBUG
                        Console.WriteLine("Poker Variant and Betting Type");
            #endif

            #region Get the betting type and poker variant

            if(hand.Context.Format==GameFormat.CashGame && handText.Contains(" Cap - "))
            {
                start = handText.IndexOf(" - ")+3;
                end = handText.IndexOf(" Cap ");

                string capAmountText = handText.Substring(start, end - start);
                hand.Context.CapAmount = Decimal.Parse(capAmountText.Substring(1).Trim());
                hand.Context.CapAmountSpecified = true;
                hand.Context.Capped = true;
            }
            else
            {
                hand.Context.CapAmountSpecified = false;
                hand.Context.Capped = false;
            }

            string typeAndVariant;

            if (hand.Context.Format == GameFormat.CashGame)
            {
                start = handText.IndexOf(":  ") + 3;
                end = handText.IndexOf(" (", start);
                typeAndVariant = handText.Substring(start, end - start);
            }
            else
            {
                start = handText.IndexOf(hand.Context.Currency) + 4;
                end = handText.IndexOf(" - ", start);
                typeAndVariant = handText.Substring(start, end - start);
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
            start = lines[0].LastIndexOf(" - ") + 3;
            end = lines[0].IndexOf(' ', start);
            string dateText = lines[0].Substring(start, end-start);
            string[] dateTokens = dateText.Split('/');
            int year = Int32.Parse(dateTokens[0]);
            int month = Int32.Parse(dateTokens[1]);
            int day = Int32.Parse(dateTokens[2]);

            start = end;
            end = lines[0].IndexOf(' ', start + 1);
            string timeText = lines[0].Substring(start, end-start);
            string[] timeTokens = timeText.Split(':');
            int hour = Int32.Parse(timeTokens[0]);
            int minute = Int32.Parse(timeTokens[1]);
            int second = Int32.Parse(timeTokens[2]);

            hand.Context.TimeStamp = new DateTime(year, month, day, hour, minute, second);
            #endregion

            #if DEBUG
                        Console.WriteLine("Players");
            #endif

            #region Create the players
            List<Player> players = new List<Player>();
            curLine = 2;
            for (Match m = stacksExp.Match(lines[curLine]); m.Success; m = stacksExp.Match(lines[curLine]))
            {
                if (m.Success)
                {

                    GroupCollection gc = m.Groups;

                    Player p = new Player();
                    p.Seat = Int32.Parse(gc[1].Value);
                    p.Name = gc[2].Value;
                    p.Stack = Decimal.Parse(gc[4].Value);

                    players.Add(p);

                    curLine++;
                }
            }

            hand.Players = players.ToArray();
            #endregion

            #if DEBUG
                        Console.WriteLine("Blinds and Antes Posted");
            #endif

            #region Terminate parsing of unsupported poker type

            if ((!typeAndVariant.Contains("Hold'em") && (!typeAndVariant.Contains("Omaha"))))
            {
                return hand;
            }

            #endregion

            #region Get the blinds and antes posted
            List<Blind> blinds = new List<Blind>();
            for (; !lines[curLine].StartsWith("*** HOLE CARDS ***"); curLine++)
            {
                for (Match m = actionExp.Match(lines[curLine]); m.Success; m = m.NextMatch())
                {
                    GroupCollection gc = m.Groups;

                    Blind blind = new Blind();
                    blind.Player = gc[1].Value;

                    String a = gc[2].Value + "" + gc[3].Value + "" + gc[4].Value + "" + gc[5].Value + "" + gc[6].Value + "" + gc[7].Value + "" + gc[8].Value + "" + gc[9].Value + "" + gc[10].Value + "" + gc[11].Value + "" + gc[12].Value;

                    if (gc[2].Value.Contains("ante"))
                        blind.Type = BlindType.Ante;
                    else if (gc[2].Value.Contains("small"))
                        blind.Type = BlindType.SmallBlind;
                    else if (gc[2].Value.Contains("big"))
                        blind.Type = BlindType.BigBlind;
                    else
                        throw new Exception("Unknown blind type: " + lines[curLine]);
                    if (lines[curLine].Contains("dead"))
                    {
                        for (int i = 0; i < gc.Count; i++)
                            Console.WriteLine("{0}: \"{1}\"", i, gc[i].Value);
                        Console.WriteLine("Found as {0}", blind.Type.ToString());
                    }
                    blind.Amount = Decimal.Parse(gc[4].Value);
                    blind.AllIn = !gc[12].Value.Equals("");
                    blinds.Add(blind);
                }
            }
            hand.Blinds = blinds.ToArray();
            #endregion

            #if DEBUG
                        Console.WriteLine("Button");
            #endif

            #region Get the button
            string buttonText = lines[1].Substring(lines[1].IndexOf('#') + 1,1);
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

            if(heroType)
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
                    p.Amount = Decimal.Parse(gc[3].Value);

                    if (gc[7].Value.Length > 0)
                        p.Number = Int32.Parse(gc[7].Value.Substring(1));
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
                string rakeText = lines[curLine].Substring(rakeStart).Replace(currencySymbol+"", "");
                hand.Rake = Decimal.Parse(rakeText.Trim());
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
                            a.Amount = Decimal.Parse(gc[4].Value);
                            a.AllIn = gc[12].Value.Contains("all-in");
                            break;
                        case "bets": a.Type = ActionType.Bet;
                            a.Amount = Decimal.Parse(gc[4].Value);
                            a.AllIn = gc[12].Value.Contains("all-in");
                            break;
                        case "raises": a.Type = ActionType.Raise;
                            a.Amount = Decimal.Parse(gc[9].Value);
                            a.AllIn = gc[12].Value.Contains("all-in");
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
                    a.Player = gc[5].Value;
                    a.Amount = Decimal.Parse(gc[2].Value);
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
            get { return "Emre Kenci"; }
        }

        public string Description
        {
            get { return "Parses a PokerStars hand history file."; }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {

        }

        public string Name
        {
            get { return "PokerStars Parser"; }
        }

        public string Version
        {
            get { return "v0.1"; }
        }

        #endregion

        #region IHandParser Members

        public string Switch
        {
            get { return "ps"; }
        }

        #endregion
    }
}

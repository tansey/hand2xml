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

        // Group 1: Seat number of the button
        Regex buttonExp = new Regex(@"Seat ([0-9]) is the button", RegexOptions.Compiled);

        // Group 1: Seat number
        // Group 2: Name
        // Group 3: Chips
        Regex stacksExp =
        new Regex(@"Seat\s([0-9]+):\s" + NAME_EXPRESSION + @"\s\(\s" + CHIPS_EXPRESSION + @"\s\)",
        RegexOptions.Compiled);

        // Group 1: Player name
        // Group 2: Action type
        // Group 3: Chips expression
        Regex actionExp =
        new Regex(NAME_EXPRESSION + @"\s(posts\ssmall\sblind|posts\sbig\sblind|posts\sbig\sblind\s\+\sdead|posts\sthe\sante|is\sall\-In|folds|checks|bets|calls|raises)(\s\[" + CHIPS_EXPRESSION + @"\])?\.",
        RegexOptions.Compiled);

        // Group 1: Name expression
        // Group 2: Chips expression
        // Group 6: Either one of: "from the main pot", "from side pot" (optional)
        // Group 7: Side pot id (Optional)
        Regex potsExp =
            new Regex(
         NAME_EXPRESSION + @"\swins\s" + CHIPS_EXPRESSION + @"(\sfrom\s+(the\smain\spot|side\spot\s#([0-9])))?",
            RegexOptions.Compiled);

        // Group 1: Name expression
        // Group 2: shown or mucked
        // Group 3-6: Hole cards
        Regex shownHandsExp =
            new Regex(NAME_EXPRESSION + string.Format(@"\s(shows|doesn't\sshow|does\snot\sshow\scards)(\s\[\s({0}),\s({0})(,\s({0}),\s({0}))?\s\])?", CARD_EXPRESSION),
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


        Regex holeCardsExp = new Regex(string.Format(@"Dealt\sto\s{0}\s\[\s\s({1})\s({1})\s\]", NAME_EXPRESSION, CARD_EXPRESSION), RegexOptions.Compiled);

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

                // filter pp notifications
                if (line.StartsWith(">") || line.StartsWith(@"The Progressive Bad Beat Jackpot has been hit"))
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
            int curLine;
            List<Round> rounds = new List<Round>();
            #endregion

            #region Make sure it's a Full Tilt hand
            if (!handText.StartsWith(HAND_START_TEXT))
                throw new InvalidHandFormatException(handText);
            #endregion

            hand.Context.Online = true;
            hand.Context.Site = Name;

#if DEBUG
            Console.WriteLine("Hand Number");
#endif

            #region Get the hand number
            hand.Context.ID = gameNumExp.Match(handText).Groups[1].Value;
            //Console.WriteLine("ID: {0}", hand.Context.ID);
            if (printed < 200)
            {
                Console.WriteLine("Hand Text: {0}", handText);
                printed++;
            }
            #endregion

#if DEBUG
            Console.WriteLine("Hand Number: {0}", hand.Context.ID);
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
            curLine = 5;
            for (Match m = stacksExp.Match(lines[curLine]); m.Success; m = stacksExp.Match(lines[++curLine]))
            {
                if (m.Success)
                {
                    GroupCollection gc = m.Groups;

                    Player p = new Player();
                    p.Seat = Int32.Parse(gc[1].Value);
                    p.Name = gc[2].Value;
                    p.Stack = Decimal.Parse(gc[3].Value);

                    players.Add(p);
                }
            }

            hand.Players = players.ToArray();
            #endregion

#if DEBUG
            foreach (var player in hand.Players)
                Console.WriteLine("Seat: {0} Name: {1} Chips: {2}", player.Seat, player.Name, player.Stack);
            Console.WriteLine("Blinds and Antes Posted");
#endif

            #region Get the blinds and antes posted
            List<Blind> blinds = new List<Blind>();
            for (; lines[curLine] != "** Dealing down cards **"; curLine++)
            {
                Match m = actionExp.Match(lines[curLine]);
                if (!m.Success)
                    continue;
                    //throw new InvalidHandFormatException("Hand " + hand.Context.ID + ". Unknown blind or ante: " + lines[curLine]);
                
                GroupCollection gc = m.Groups;

                Blind blind = new Blind();
                blind.Player = gc[1].Value;
                if (gc[2].Value == "posts the ante")
                    blind.Type = BlindType.Ante;
                else if (gc[2].Value == "posts small blind")
                    blind.Type = BlindType.SmallBlind;
                else if (gc[2].Value == "posts big blind")
                    blind.Type = BlindType.BigBlind;
                else if (gc[2].Value.StartsWith("posts"))
                    blind.Type = BlindType.LateBlind;
                else
                    throw new Exception("Unknown blind type: " + lines[curLine]);
                //TODO: Handle dead and late blinds appropriately

                blind.Amount = Decimal.Parse(gc[4].Value);

                // TODO: Handle all-in blind posts
                //blind.AllIn = gc[9].Value.Length == 15;
                blind.AllIn = false;

                blinds.Add(blind);
            }
            hand.Blinds = blinds.ToArray();
            #endregion

#if DEBUG
            foreach (var blind in hand.Blinds)
                Console.WriteLine("Player: {0} Amount: {1} Type: {2} All-in: {3}", blind.Player, blind.Amount, blind.Type, blind.AllIn);
            Console.WriteLine("Button");
#endif

            #region Get the button
            hand.Context.Button = Int32.Parse(buttonExp.Match(handText).Groups[1].Value);
            curLine++;
            #endregion

#if DEBUG
            Console.WriteLine("Button: {0}", hand.Context.Button);
            Console.WriteLine("Hole Cards and Hero");
#endif
            #region Get the hole cards and the name of the hero
            Match hcMatch = holeCardsExp.Match(handText);
            if (hcMatch.Success)
            {
                List<Card> holecards = new List<Card>();
                hand.Hero = hcMatch.Groups[1].Value;
                holecards.Add(new Card(hcMatch.Groups[2].Value));
                holecards.Add(new Card(hcMatch.Groups[3].Value));
                hand.HoleCards = holecards.ToArray();
                //TODO: Handle Omaha.
            }
            #endregion

#if DEBUG
            if (hcMatch.Success)
                Console.WriteLine("Hero: {0} HoleCard1: {1} HoleCard2: {2}", hand.Hero, hand.HoleCards[0], hand.HoleCards[1]);
            Console.WriteLine("Preflop Actions");
#endif
            #region Preflop Actions
            curLine = lines.IndexOf("** Dealing down cards **") + (hand.Hero == null ? 1 : 2);
            rounds.Add(new Round());
            rounds[0].Actions = getRoundActions(hand, rounds, lines, ref curLine);
            #endregion

#if DEBUG
            if(rounds[0].CommunityCards != null)
                foreach (var card in rounds[0].CommunityCards)
                    Console.WriteLine("Preflop Card: {0}", card);
            foreach (var action in rounds[0].Actions)
                Console.WriteLine("Preflop Player: {0} Action: {1} Amount: {2} All-In: {3}", action.Player, action.Type, action.Amount, action.AllIn);
            Console.WriteLine("Flop Actions");
#endif
            #region Flop Actions and Community Cards
            if (lines[curLine].StartsWith("** Dealing Flop **"))
            {
                rounds.Add(new Round());

                start = lines[curLine].IndexOf('[') + 2;
                Card[] flop = new Card[3];
                flop[0] = new Card(lines[curLine].Substring(start, 2));
                flop[1] = new Card(lines[curLine].Substring(start + 4, 2));
                flop[2] = new Card(lines[curLine].Substring(start + 8, 2));
                rounds[1].CommunityCards = flop;

                curLine++;

                rounds[1].Actions = getRoundActions(hand, rounds, lines, ref curLine);
            }
            #endregion

#if DEBUG
            if (rounds.Count > 1)
            {
                foreach (var card in rounds[1].CommunityCards)
                    Console.WriteLine("Flop Card: {0}", card);
                foreach (var action in rounds[1].Actions)
                    Console.WriteLine("Flop Player: {0} Action: {1} Amount: {2} All-In: {3}", action.Player, action.Type, action.Amount, action.AllIn);
            }
            Console.WriteLine("Turn Actions");
#endif
            #region Turn Actions and Community Card
            if (lines[curLine].StartsWith("** Dealing Turn **"))
            {
                rounds.Add(new Round());

                start = lines[curLine].LastIndexOf('[') + 2;
                Card[] turn = new Card[1];
                turn[0] = new Card(lines[curLine].Substring(start, 2));
                rounds[2].CommunityCards = turn;

                curLine++;

                rounds[2].Actions = getRoundActions(hand, rounds, lines, ref curLine);
            }
            #endregion

#if DEBUG
            if (rounds.Count > 2)
            {
                foreach (var card in rounds[2].CommunityCards)
                    Console.WriteLine("Turn Card: {0}", card);
                foreach (var action in rounds[2].Actions)
                    Console.WriteLine("Turn Player: {0} Action: {1} Amount: {2} All-In: {3}", action.Player, action.Type, action.Amount, action.AllIn);
            }
            Console.WriteLine("River Actions");
#endif
            #region River Actions and Community Card
            if (lines[curLine].StartsWith("** Dealing River **"))
            {
                rounds.Add(new Round());

                start = lines[curLine].LastIndexOf('[') + 2;
                Card[] river = new Card[1];
                river[0] = new Card(lines[curLine].Substring(start, 2));
                rounds[3].CommunityCards = river;

                curLine++;

                rounds[3].Actions = getRoundActions(hand, rounds, lines, ref curLine);
            }
            #endregion

            #region Set rounds
            hand.Rounds = rounds.ToArray();
            #endregion

#if DEBUG
            if (rounds.Count > 3)
            {
                foreach (var card in rounds[3].CommunityCards)
                    Console.WriteLine("River Card: {0}", card);
                foreach (var action in rounds[3].Actions)
                    Console.WriteLine("River Player: {0} Action: {1} Amount: {2} All-In: {3}", action.Player, action.Type, action.Amount, action.AllIn);
            }
            Console.WriteLine("Shown Hands");
#endif
            List<HandResult> results = new List<HandResult>();

            #region Get the shown down hands
            for (Match match = shownHandsExp.Match(handText); match.Success; match = match.NextMatch())
            {
                GroupCollection gc = match.Groups;

                if (!hand.Players.Select(p => p.Name).Contains(gc[1].Value))
                    continue;

                HandResult hr = new HandResult(gc[1].Value);
                if (gc[2].Value != "does not show cards")
                    hr.HoleCards = new Card[] { new Card(gc[4].Value), new Card(gc[5].Value) };

                results.Add(hr);
            }
            #endregion

#if DEBUG
            foreach (var hr in results)
                Console.WriteLine("Player: {0} Cards: {1}", hr.Player, hr.HoleCards == null ? "" : hr.HoleCards[0].ToString() + " " + hr.HoleCards[1].ToString());
#endif
            
            #region Get pots won
            List<List<Pot>> pots = new List<List<Pot>>();
            for (int i = 0; i < results.Count; i++)
                pots.Add(new List<Pot>());

            for (; curLine < lines.Count; curLine++)
            {
                Match m = potsExp.Match(lines[curLine]);
                if (m.Success)
                {
                    GroupCollection gc = m.Groups;

                    if (!hand.Players.Select(player => player.Name).Contains(gc[1].Value))
                        continue;

                    Pot p = new Pot();
                    p.Amount = Decimal.Parse(gc[2].Value);

                    if(gc[6].Value == "" || gc[6].Value == "the main pot")
                        p.Number = 0;
                    else
                        p.Number = int.Parse(gc[7].Value);

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

                    //if (result == null)
                    //{
                    //    result = new HandResult(gc[1].Value);
                    //    potList = new List<Pot>();

                    //    results.Add(result);
                    //    pots.Add(potList);
                    //}

                    potList.Add(p);
                }
            }

            //add the pots to the model
            for (int i = 0; i < results.Count; i++)
                results[i].WonPots = pots[i].ToArray();

            // Set the results
            hand.Results = results.ToArray();
            
#if DEBUG
            foreach (var result in hand.Results)
                foreach (var pot in result.WonPots)
                    Console.WriteLine("Pot: {0} Player: {1} Amount: {2}", pot.Number, result.Player, pot.Amount);
            Console.WriteLine("Calculating rake");
#endif

            //get the rake, if any
            if (hand.Context.Format == GameFormat.CashGame)
            {
                hand.Rake = hand.Rounds.Sum(r => r.Actions.Sum(act => act.Amount))
                          + hand.Blinds.Sum(b => b.Amount)
                          - hand.Results.Sum(r => r.WonPots.Sum(w => w.Amount));
            }

            #endregion

#if DEBUG
            Console.WriteLine("Rake: {0}", hand.Rake);
            Console.WriteLine("Done");
#endif


            return hand;
        }


        #region Helper methods
        private Action[] getRoundActions(PokerHand hand, List<Round> rounds, List<string> lines, ref int curLine)
        {
            List<Action> actions = new List<Action>();
            for (; !lines[curLine].StartsWith("** Dealing"); curLine++)
            {
                if (potsExp.Match(lines[curLine]).Success)
                    break;

                Match m = actionExp.Match(lines[curLine]);
                if (m.Success)
                {
                    GroupCollection gc = m.Groups;
                    if (hand.Players.FirstOrDefault(p => p.Name == gc[1].Value) == null)
                        continue;

                    Action a = new Action();
                    a.Player = gc[1].Value;

                    //is all-In|folds|checks|bets|calls|raises
                    switch (gc[2].Value)
                    {
                        case "folds": a.Type = ActionType.Fold;
                            break;
                        case "checks": a.Type = ActionType.Check;
                            break;
                        case "calls": a.Type = ActionType.Call;
                            a.Amount = Decimal.Parse(gc[4].Value);
                            break;
                        case "bets": a.Type = ActionType.Bet;
                            a.Amount = Decimal.Parse(gc[4].Value);
                            break;
                        case "raises": a.Type = ActionType.Raise;
                            a.Amount = Decimal.Parse(gc[4].Value);
                            break;
                        case "is all-In":
                            {
                                a.AllIn = true;
                                a.Amount = hand.Players.FirstOrDefault(p => p.Name == a.Player).Stack
                                    - rounds.Sum(r => r.Actions == null ? 0 :
                                                      r.Actions.Sum(act => act.Player == a.Player ? act.Amount : 0))
                                         - actions.Sum(act => act.Player == a.Player ? act.Amount : 0);
                                var lastRaise =  actions.LastOrDefault(act => act.Type == ActionType.Raise);
                                if (lastRaise == null)
                                {
                                    if (rounds.Count == 1)
                                    {
                                        if (a.Amount <= hand.Blinds.First(b => b.Type == BlindType.BigBlind).Amount)
                                            a.Type = ActionType.Call;
                                        else
                                            a.Type = ActionType.Raise;
                                    }
                                    else
                                        a.Type = ActionType.Bet;
                                }
                                else if (lastRaise.Amount < a.Amount)
                                    a.Type = ActionType.Raise;
                                else
                                    a.Type = ActionType.Call;
                            }
                            break;
                        default: throw new Exception("Unknown action type: " + lines[curLine]);
                    }

                    actions.Add(a);
                    continue;
                }
                
                // TODO: Handle returned money. Note that pp does not make this obvious.
            }
            return actions.ToArray();
        }

        private bool isIrrelevant(PokerHand hand, string line)
        {
            foreach (var player in hand.Players)
                if (line.StartsWith(player.Name + ":"))
                    return true;
                else if (line.StartsWith(player.Name + " "))
                    return false;
            return true;
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

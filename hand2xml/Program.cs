using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using PluginArchitecture;
using PluginInterface;
using PokerHandHistory;
using HandParserInterface;
using System.IO;
using System.Linq;

/*
 * Test class for the hand history parser. Every hand parser defines a unique
 * switch (e.g., for Full Tilt it's 'ft').  
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
namespace hand2xml
{
    /// <summary>
    /// Command line program to parse a hand history to XML.
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: hand2xml site inFile outFile");
                return;
            }

            PluginServices host = PluginServices.Instance;
            host.PluginDirectory = AppDomain.CurrentDomain.BaseDirectory;
            host.AddPlugin("HandParserInterface.IHandParser");
            host.FindPlugins();
            AvailablePlugins parsers = host.Plugins["HandParserInterface.IHandParser"];
            IHandParser parser = null;
            foreach (AvailablePlugin plugin in parsers)
            {
                IHandParser curParser = (IHandParser)plugin.Instance;
                if (curParser.Switch == args[0])
                {
                    parser = curParser;
                    break;
                }
            }

            #region Validate parameters
            if (parser == null)
            {
                Console.WriteLine("Unknown parser type: {0}", args[0]);
                return;
            }
            
            if (!File.Exists(args[1]) && !Directory.Exists(args[1]))
            {
                Console.WriteLine("Unknown file or directory: {0}", args[1]);
                return;
            }
            #endregion

            PokerHandXML result;
            TextWriter output = new StreamWriter(args[2]);

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(args[1]);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                List<PokerHand> hands = new List<PokerHand>();
                foreach (string filename in Directory.GetFiles(args[1]))
                    hands.AddRange(parser.ParseFile(filename).Hands);
                result = new PokerHandXML() { Hands = hands.ToArray() };
            }
            else
                result = parser.ParseFile(args[1]);

            
            XmlSerializer serializer = new XmlSerializer(typeof(PokerHandXML));
            serializer.Serialize(output, result);
            output.Flush();
            output.Close();

            
        }
    }
}

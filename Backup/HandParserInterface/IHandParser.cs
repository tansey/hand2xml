using System;
using System.Collections.Generic;
using System.Text;
using PluginInterface;
using HandHistory;

/*
 * Interface for parser plugins to define.
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
namespace HandParserInterface
{
    public interface IHandParser : IPlugin
    {
        /// <summary>
        /// The switch to specify the parser to use at the command line.
        /// e.g., if you have a parser for World's Best Poker Site, a good
        /// switch may be "wb" or "wbps".
        /// </summary>
        string Switch { get;}

        /// <summary>
        /// Parses a hand history text file and returns a collection of 
        /// PokerHand objects able to be serialized to XML.  Note that
        /// the order of the hands should be preserved!
        /// </summary>
        /// <param name="filename">The text file to read in.</param>
        /// <returns>A collection of serializable hand histories.</returns>
        PokerHandXML ParseFile(string filename);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

/*
 * Exception class thrown by hand history parsers.
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
    /// <summary>
    /// Thrown when a user tries to parse a text file which uses a format
    /// not supported or not recognized by a parser.
    /// </summary>
    public class InvalidHandFormatException : Exception
    {
        public InvalidHandFormatException() { }

        public InvalidHandFormatException(string hand) : base(hand) { }
    }
}

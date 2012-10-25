# hand2xml
A C# framework for parsing online poker hand histories to a common XML format. The framework supports any game with holecards and community cards (e.g., Texas Hold'em and Omaha).

The framework, hand2xml, follows a plug-in architecture where each site's parser is treated as a separate plug-in and loaded dynamically. Currently, only Full Tilt support is included, as that's all I've ever needed. It's straight-forward to write additional site parsers, and I'm hoping that if people like the framework then maybe it will get a few more parsers written.

More information and discussion available on [nashcoding](http://www.nashcoding.com/2010/03/06/hand2xml-a-simple-plug-in-framework-for-hand-history-parsing/).

# Supported sites
- Full Tilt Poker
- PokerStars
- PartyPoker (old version, limit only, untested)
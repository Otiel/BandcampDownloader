![Logo](http://i.imgur.com/S6AZHOg.png) Bandcamp Downloader
===========================================================

Description
-----------

_BandcampDownloader_ is a Windows application written in C# using .NET Framework 4 that helps downloading albums from [bandcamp.com](http://bandcamp.com). _BandcampDownloader_ retrieves the 128 kbps MP3 files that are streamed on the website.

Features
--------

* Downloads MP3 files from Bandcamp:
  * From album pages: http://[artist].bandcamp.com/album/[album],
  * From artist pages: http://[artist].bandcamp.com.
* Adds ID3 tags to tracks: Album, Artist, Album Artist, Title, Track number and Year.
* Downloads cover art and save it in tracks tags.

Usage
-----

1. [Download](https://github.com/Otiel/BandcampDownloader/releases/latest) the latest version from the releases page as a zip file.
2. Make sure the following files are located in the same folder:
    * `BandcampDownloader.exe`,
    * `Newtonsoft.Json.dll`,
    * `taglib-sharp.dll`.
3. Run `BandcampDownloader.exe`.

Screenshot
----------

![Screenshot](http://i.imgur.com/sBCKZTg.png)

Dependencies
------------

_BandcampDownloader_ uses:
* [Json.NET](http://james.newtonking.com/json) to deserialize JSON data from the Bandcamp pages.
* [TagLibSharp](https://github.com/mono/taglib-sharp) to tag tracks.

Bugs/Ideas
----------

If you have a bug to report, or simply an idea for an improvement or a new feature, please add them in the [issue tracker](https://github.com/Otiel/BandcampDownloader/issues).

License
-------

_BandcampDownloader_ is licensed under the [WTFPL](http://www.wtfpl.net/) ![WTFPL icon](http://i.imgur.com/AsWaQQl.png).

Piracy
------

You'll do what you want to do with this app, but remember to buy albums from your favorite artists if you want to support them!

And for the artists, Bandcamp [says](https://bandcamp.com/help/audio_basics#steal) it all:
> **One of my fans showed me a totally easy way that someone could STEAL my music off of Bandcamp using RealPlayer 14.1 beta 3, or RipTheWeb.com, or by going into Temporary Internet Files and renaming blah blah blah. What are you doing about this grave problem?**

> Nothing. Since streams on Bandcamp are full-length, rather than 30-second snippets, it's correct that someone could use one of the above methods to access the underlying MP3-128. And sure, we could throw some technical hurdles in their way, but if they hit one of those hurdles, it's not like they'd slap their forehead and open their wallet. Instead, they'd just move on to some other site where those restrictions aren't in place, and you'll have squandered the chance to make your own site the premier destination for those seemingly cheap, but enthusiastic, word-spreading, and potentially later money-spending fans. In other words, the few people employing the above methods are better thought of as an opportunity, not a lost sale. If you're still skeptical, Andrew Dubber's [post on the topic of music piracy](http://newmusicstrategies.com/2008/04/03/should-i-be-worried-about-piracy/) is a must-read.

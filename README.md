Bandcamp Downloader
==================

A Windows app used to download albums from Bandcamp.

Description
-----------

_BandcampDownloader_ is a Windows application written in C# using .NET Framework 4 that helps downloading albums from [bandcamp.com](http://bandcamp.com).

Features
--------

* Downloads albums from Bandcamp album pages.
* Tags tracks with Album, Artist, Album Artist, Title, Track number and Year.
* Downloads cover art and save it in tracks tags.

Usage
-----

1. If you're just looking to use the app right now, download the latest version from the [releases](https://github.com/Otiel/BandcampDownloader/releases) page as a zip file.
2. Make sure the following files are located in the same folder:
    * `BandcampDownloader.exe`,
    * `Newtonsoft.Json.dll`,
    * `policy.2.0.taglib-sharp.dll`,
    * `taglib-sharp.dll`.
3. Run `BandcampDownloader.exe`.

Screenshot
----------

![Screenshot](http://i.imgur.com/DoCj1Pm.png)

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
And for the artists, Bandcamp [says](https://bandcamp.com/help/audio_basics#steal) it all.

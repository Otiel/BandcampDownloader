# Dependencies

BandcampDownloader relies on a number of open-source libraries, all listed below:

* [`Castle.Core`](https://github.com/castleproject/Core): required by `Config.Net`.
* [`Config.Net`](https://github.com/aloneguid/config): used to manage the application configuration file.
* [`Costura`](https://github.com/Fody/Costura): used to embed dependencies (_*.dll_) files in the main _exe_ file.
* [`Fody`](https://github.com/Fody/Fody): required by `Costura`.
* [`HtmlAgilityPack`](https://github.com/zzzprojects/html-agility-pack): used to parse Html from bandcamp.com pages.
* [`ImageResizer`](https://github.com/imazen/resizer): used to resize/compress the album covers.
* [`Json.NET`](https://github.com/JamesNK/Newtonsoft.Json): used to parse Json from bandcamp.com pages.
* [`Markdig`](https://github.com/lunet-io/markdig): required by `Markdig.WPF`.
* [`Markdig.WPF`](https://github.com/Kryptos-FR/markdig.wpf): used to display Markdown.
* [`NLog`](https://github.com/NLog/NLog): used to log events.
* [`PlaylistsNET`](https://github.com/tmk907/PlaylistsNET): used to create playlists.
* [`Resource.Embedder`](https://github.com/MarcStan/Resource.Embedder): used to embed localization resources (_*.resources.dll_) in the main _exe_ file (not supported by `Costura`).
* [`TagLib#`](https://github.com/mono/taglib-sharp): used to save ID3 tags in MP3 files.
* [`WPFLocalizationExtension`](https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension): used to manage the localization.
* [`WpfMessageBox`](https://github.com/Otiel/WpfMessageBox): used to display message boxes.
* [`XAMLMarkupExtensions`](https://github.com/XAMLMarkupExtensions/XAMLMarkupExtensions): required by `WpfLocalizeExtension`.

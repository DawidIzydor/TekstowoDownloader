# TekstowoDownloader
Simple c# html parser to download lyrics from tekstowo.org. It uses taglib-sharp for checking mp3 tags and HtmlAgilityPack to parse HTML

## Usage

Use one of the following functions

### Parser.GetLyricsForDirectory(dir, [forceArtist])

Gets lyrics for every file in specified directory. If forceArtist is specified it'll use the specified forcedArtist as artist instead of tag checking

### Parser.GetLyricsForFile(location, [forceArtist], [forceTitle])

Gets lyrics for specified location. If forceArtist is specified it'll use the specified forcedArtist as artist instead of tag checking. If forceTitle is specified it'll use the specified forceTitle as title instead of tag checking

### GetLyricsForArtistTitle(aritst, title)

Gets lyrics for specified artist and title

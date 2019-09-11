using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace TekstowoDownloader
{
    internal class Parser
    {
        public string GetLyricsForArtistTitle(string songArtist, string songTitle)
        {
            string songSiteHtmlSource = TryGetSongHtmlSource(songArtist, songTitle);
            if (songSiteHtmlSource == "")
            {
                Console.WriteLine("[ERR] Empty return string for " + songArtist + " - " + songTitle);
                return "";
            }

            string songText = GeSongTextHtml(songSiteHtmlSource);
            if (songText == "")
            {
                Console.WriteLine("[ERR] Empty parsing song text for " + songArtist + " - " + songTitle);
                return "";
            }

            return RemoveHtmlTags(songText);
        }

        // ReSharper disable once UnusedParameter.Global
        public void GetLyricsForDirectory(string directory, string forceArtist = "")
        {
            foreach (string filePath in Directory.GetFiles(directory))
            {
                string lyricsPath = GetLyricsPath(filePath);

                if (File.Exists(lyricsPath))
                {
                    Console.WriteLine("[WARN] File exists: " + lyricsPath + ", ignoring");
                    continue;
                }

                WriteLyricsToFile(GetLyricsForFile(filePath), lyricsPath);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string GetLyricsForFile(string location, string forcedArtist = "", string forcedTitle = "")
        {
            string artist = InitArtistFromFile(location, forcedArtist);
            string title = InitTitleFromFile(location, forcedTitle);

            if (title == "" || artist == "")
            {
                Console.Write("[ERR] No artist or title tag for " + location);
                return "";
            }

            return GetLyricsForArtistTitle(artist, title);
        }

        private static string GeSongTextHtml(string siteSource)
        {
            string songTextHtml = "";

            try
            {
                songTextHtml = HtmlNode.CreateNode(siteSource).ChildNodes.FirstOrDefault(c => c.Name == "body")
                    ?.ChildNodes.FirstOrDefault(c => c.Id == "contener")
                    ?.ChildNodes.FirstOrDefault(c => c.Id == "center")
                    ?.ChildNodes.FirstOrDefault(c => c.HasClass("right-column"))
                    ?.ChildNodes.FirstOrDefault(c => c.HasClass("tekst"))
                    ?.ChildNodes.FirstOrDefault(c => c.HasClass("song-text"))?.InnerHtml;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERR] Problem parsing song text:" + ex.Message);
            }

            return songTextHtml;
        }

        private static string GetArtistFromPath(string filePath)
        {
            TagLib.File file = TagLib.File.Create(filePath);
            string artist = file.Tag.AlbumArtists[0];

            if (artist.ToLower() == "various artists" && file.Tag.FirstPerformer != null)
            {
                artist = file.Tag.FirstPerformer.Split(',').FirstOrDefault();
            }

            return artist;
        }

        private static string GetLyricsPath(string filePath)
        {
            string[] temp = filePath.Split('.');
            string lyricTxtFile = "";

            for (int i = 0; i < temp.Length - 1; ++i)
            {
                lyricTxtFile += temp[i];
            }

            lyricTxtFile += ".txt";
            return lyricTxtFile;
        }

        private static string InitArtistFromFile(string filePath, string forcedArtist)
        {
            string artist;

            if (forcedArtist == "")
            {
                artist = TryGetArtistFromPath(filePath);
            }
            else
            {
                artist = forcedArtist;
            }

            return artist;
        }

        private static string InitTitleFromFile(string filePath, string forcedTitle)
        {
            string title;

            if (forcedTitle == "")
            {
                title = TryGetTitleFromPath(filePath);
            }
            else
            {
                title = forcedTitle;
            }

            return title;
        }

        private static string RemoveHtmlTags(string text) =>
            text.Remove(0, text.IndexOf("</h2>", StringComparison.Ordinal) + 5)
                .Replace("<br />", "")
                .Replace("<br>", "")
                .Remove(text.IndexOf("<p>", StringComparison.Ordinal))
                .Trim();

        private string ReplaceUnwantedCharacters(string str)
        {
            str = str.ToLower();

            string ret = "";

            foreach (char c in str)
            {
                if (c >= 'a' && c <= 'z')
                {
                    ret += c;
                }
                else
                {
                    ret += '_';
                }
            }

            return ret;
        }

        private static string TryGetArtistFromPath(string filePath)
        {
            string artist = "";

            try
            {
                artist = GetArtistFromPath(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERR] Problem parsing " + filePath + ": " + ex.Message);
            }

            return artist;
        }

        private string TryGetSongHtmlSource(string songArtist, string songTitle)
        {
            string source = "";

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    source = client.DownloadString("https://www.tekstowo.pl/piosenka," +
                                                   ReplaceUnwantedCharacters(songArtist) + "," +
                                                   ReplaceUnwantedCharacters(songTitle) + ".html");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERR] Problem downloading lyrics for " + songArtist + " - " + songTitle + ": " +
                                  ex.Message);
            }

            return source;
        }

        private static string TryGetTitleFromPath(string filePath)
        {
            string title = "";
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);

                title = file.Tag.Title;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERR] Problem parsing " + filePath + ": " + ex.Message);
            }

            return title;
        }

        private static void WriteLyricsToFile(string lyrics, string lyricsFilePath)
        {
            if (lyrics != "")
            {
                try
                {
                    using (StreamWriter fileWriter = File.CreateText(lyricsFilePath))
                    {
                        fileWriter.Write(lyrics);
                        fileWriter.Close();

                        Console.WriteLine("[INFO] Saved text for " + fileWriter + " to " + lyricsFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERR] Problem parsing " + lyricsFilePath + ", continuing: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("[ERR] Problem parsing " + lyricsFilePath + ", continuing");
            }
        }
    }
}
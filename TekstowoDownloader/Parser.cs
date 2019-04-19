using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TekstowoDownloader
{
    class Parser
    {
        private string ChangeStr(string str)
        {
            str = str.ToLower();

            string ret = "";

            foreach (var c in str)
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

        public void GetLyricsForDirectory(string dir, string forceArtist = "")
        {
            foreach (var el in Directory.GetFiles(dir))
            {
                var temp = el.Split('.');

                var lyricTxtFile = "";

                for (int i = 0; i < temp.Length - 1; ++i)
                {
                    lyricTxtFile += temp[i];
                }

                lyricTxtFile += ".txt";

                if (File.Exists(lyricTxtFile))
                {
                    Console.WriteLine("[WARN] File exists: " + lyricTxtFile + ", ignoring");
                    continue;
                }

                string text = GetLyricsForFile(el);

                if (text != "")
                {
                    try
                    {
                        using (var file = File.CreateText(lyricTxtFile))
                        {
                            file.Write(text);
                            file.Close();

                            Console.WriteLine("[INFO] Saved text for " + el + " to " + lyricTxtFile);
                        }
                    }catch(Exception ex)
                    {
                        Console.WriteLine("[ERR] Problem parsing " + el + ", continuing: "+ex.Message);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("[ERR] Problem parsing " + el + ", continuing");
                    continue;
                }
            }
        }
    
        public string GetLyricsForFile(string location, string forceArtist = "", string forcetitle = "")
        {

            string title = "";
            string artist = "";

            if (forceArtist != "")
            {
                artist = forceArtist;
            }

            if (forcetitle != "")
            {
                title = forcetitle;
            }

            if (forceArtist == "" || forcetitle == "")
            {
                try
                {
                    var file = TagLib.File.Create(location);

                    if (forceArtist == "")
                    {
                        artist = file.Tag.AlbumArtists[0];

                        if(artist.ToLower() == "various artists" && file.Tag.FirstPerformer != null)
                        {
                            artist = file.Tag.FirstPerformer.Split(',').FirstOrDefault();
                        }
                    }

                    if (forcetitle == "")
                    {
                        title = file.Tag.Title;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERR] Problem parsing " + location + ": " + ex.Message);
                    return "";
                }
            }

            if (title == "" || artist == "")
            {
                Console.Write("[ERR] No artist or title tag for " + location);
                return "";
            }

            return GetLyricsForArtistTitle(artist, title);
        }

        public string GetLyricsForArtistTitle(string artist, string title)
        {
            string source = "";

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    source = client.DownloadString("https://www.tekstowo.pl/piosenka," + ChangeStr(artist) + "," + ChangeStr(title) + ".html");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERR] Problem downloading lyrics for " + artist + " - "+ title + ": " + ex.Message);
                return "";
            }

            if (source == "")
            {
                Console.WriteLine("[ERR] Empty return string for " + artist + " - " + title);
                return "";
            }
            string str = "";

            try
            {
                var site = HtmlAgilityPack.HtmlNode.CreateNode(source);


                var body = site.ChildNodes.Where(c => c.Name == "body").FirstOrDefault();

                var div = body.ChildNodes.Where(c => c.Id == "contener").FirstOrDefault();

                div = div.ChildNodes.Where(c => c.Id == "center").FirstOrDefault();

                div = div.ChildNodes.Where(c => c.HasClass("right-column")).FirstOrDefault();

                div = div.ChildNodes.Where(c => c.HasClass("tekst")).FirstOrDefault();

                div = div.ChildNodes.Where(c => c.HasClass("song-text")).FirstOrDefault();

                str = div.InnerHtml;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERR] Problem parsing song text for " + artist + " - " + title + ":" + ex.Message);
                return "";
            }

            if (str == "")
            {
                Console.WriteLine("[ERR] Empty parsing song text for " + artist + " - " + title);
                return "";
            }

            str = str.Remove(0, str.IndexOf("</h2>") + 5);
            str = str.Replace("<br />", "");
            str = str.Replace("<br>", "");

            str = str.Remove(str.IndexOf("<p>"));

            str = str.Trim();

            return str;
        }
    }
}

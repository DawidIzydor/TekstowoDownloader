using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TekstowoDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = @"M:\Zespoly\Various Artists\2008 - Świat wg Nohavicy";

            Parser parser = new Parser();

            parser.GetLyricsForDirectory(dir);

            //foreach (var el in Directory.GetDirectories(dir))
            //{
            //    parser.GetLyricsForDirectory(el);
            //}
        }
    }
}

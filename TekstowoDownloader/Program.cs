namespace TekstowoDownloader
{
    class Program
    {
        static void Main()
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

using feeder_parser;

namespace data_validator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string baseDir = @"C:\Users\Ben\Programs\td7\feeder-parser\data";
            for(int div = 1; div <= 2; div++)
            {
                string divPath = Path.Combine(baseDir, $"div{div}");
                for(int week = 1; week <= 10; week++)
                {
                    string weekPath = Path.Combine(divPath, $"week{week}");
                    if (!Directory.Exists(weekPath)) continue;
                    string[] matchDirs = Directory.GetDirectories(weekPath);
                    foreach(string matchDir in matchDirs)
                    {
                        string headerFilePath = Path.Combine(matchDir, feeder_parser.Program.HEADER_FILE);
                        if (!File.Exists(headerFilePath))
                        {

                            string matchTitle = Path.GetFileName(matchDir);
                            Console.WriteLine($"MISSING INFO.TXT IN week {week} {matchTitle}");
                            continue;
                        }

                        string[] maps = feeder_parser.Program.weekToMaps[week].Select(x => x.ToString()).ToArray();
                        string[] files = Directory.GetFiles(matchDir);
                        if(files.Length != 7)
                        {
                            // find out which maps we are missing data for
                            List<int> missingMaps = [];
                            for(int map = 1; map <= 6; map++)
                            {
                                string currentMap = maps[map - 1];
                                string file = files.FirstOrDefault(x => Path.GetFileName(x).StartsWith(currentMap), "");
                                if(file == "") // missing data for this map
                                {
                                    missingMaps.Add(map);
                                }
                            }
                            // see if we have qlstats data for them in info.txt
                            string[] infoLines = File.ReadAllLines(headerFilePath);
                            string[] qlstatsLines = infoLines.Skip(2).ToArray();
                            foreach(string line in qlstatsLines)
                            {

                            }
                            
                        }
                    }
                }
            }
        }
    }
}

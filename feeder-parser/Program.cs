using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static feeder_parser.Weapons;

namespace feeder_parser
{
    internal class Program
    {
        public const string HEADER_FILE = "info.txt";

        public static readonly string[] WEAPON_NAMES = ["rl", "lg", "rg", "gl", "pg", "mg", "hmg", "sg"];

        public enum Map
        {
            campgrounds,
            asylum,
            trinity,
            nohalls,
            quarantine,
            uprise,
            servitude,
            strain,
            ra3map20a,
            ra3map17c,
            squid,
            ra3map19c
        }

        public static string GetMapString(Map map)
        {
            if (map == Map.squid) return "charon3dm13d";
            if (map == Map.servitude) return "servituderedux";
            return map.ToString();
        }

        public static readonly Dictionary<int, Map[]> weekToMaps = new Dictionary<int, Map[]>
        {
            { 1, [Map.campgrounds, Map.trinity, Map.asylum, Map.nohalls, Map.ra3map20a, Map.uprise] },
            { 2, [Map.strain, Map.squid, Map.campgrounds, Map.nohalls, Map.servitude, Map.trinity] },
            { 3, [Map.squid, Map.asylum, Map.trinity, Map.servitude, Map.uprise, Map.ra3map20a] },
            { 4, [Map.uprise, Map.trinity, Map.campgrounds, Map.asylum, Map.squid, Map.strain] },
            { 5, [Map.ra3map20a, Map.squid, Map.campgrounds, Map.servitude, Map.trinity, Map.asylum] },
            { 6, [Map.ra3map17c, Map.strain, Map.asylum, Map.ra3map19c, Map.ra3map20a, Map.quarantine] },
            { 7, [Map.campgrounds, Map.trinity, Map.squid, Map.uprise, Map.servitude, Map.ra3map20a] },
            { 8, [Map.trinity, Map.uprise, Map.servitude, Map.campgrounds, Map.squid, Map.asylum] },
            { 9, [Map.ra3map19c, Map.nohalls, Map.quarantine, Map.asylum, Map.ra3map17c, Map.strain] },
            { 10, [Map.squid, Map.uprise, Map.servitude, Map.nohalls, Map.strain, Map.asylum] },
        };

        public static string GenerateId(string input)
        {
            const string allowedChars = "abcdefghjkpqrstwxyz23456789";

            using(MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < 6; i++)
                {
                    int index = hashBytes[i] % allowedChars.Length;
                    sb.Append(allowedChars[index]);
                }
                return sb.ToString();
            }
        }

        public static void InsertGameRow(SqliteConnection conn, string gameId, string serverId, string mapId, int date, int mapNum, int week)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO games VALUES (@gameId, @serverId, @mapId, @date, @mapNum, @week)";
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("serverId", serverId);
            cmd.Parameters.AddWithValue("@mapId", mapId);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@mapNum", mapNum);
            cmd.Parameters.AddWithValue("@week", week);

            cmd.ExecuteNonQuery();
        }

        public static void InsertPlayerNameRow(SqliteConnection conn, string playerId, string gameId, string name)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO playerNames VALUES (@playerId, @gameId, @name)";
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@name", name);

            cmd.ExecuteNonQuery();
        }

        public static void InsertPgStatsRow(SqliteConnection conn, string playerId, string gameId, int score, int rank,
            int damageDealt, int damageTaken, int kills, int deaths)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO pgStats VALUES (@playerId, @gameId, @score, @rank, @damageDealt, @damageTaken, " +
                "@kills, @deaths)";
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("score", score);
            cmd.Parameters.AddWithValue("@rank", rank);
            cmd.Parameters.AddWithValue("@damageDealt", damageDealt);
            cmd.Parameters.AddWithValue("@damageTaken", damageTaken);
            cmd.Parameters.AddWithValue("@kills", kills);
            cmd.Parameters.AddWithValue("@deaths", deaths);

            cmd.ExecuteNonQuery();
        }

        public static void InsertPwStatsRow(SqliteConnection conn, string playerId, string gameId, string weaponName,
            int damage, int kills, int shotsFired, int shotsHit)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO pwStats VALUES (@playerId, @gameId, @weaponName, @damage, @kills," +
                "@shotsFired, @shotsHit)";
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@weaponName", weaponName);
            cmd.Parameters.AddWithValue("@damage", damage);
            cmd.Parameters.AddWithValue("@kills", kills);
            cmd.Parameters.AddWithValue("@shotsFired", shotsFired);
            cmd.Parameters.AddWithValue("@shotsHit", shotsHit);

            cmd.ExecuteNonQuery();
        }

        static void Main(string[] args)
        {
            using SqliteConnection conn = new SqliteConnection("Data Source=C:\\Users\\Ben\\Programs\\td7\\feeder-parser\\stats.db");
            conn.Open();
            string baseDir = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(baseDir);
            for(int div = 1; div <= 2; div++)
            {
                string divPath = Path.Combine(baseDir, $"div{div}");
                Directory.CreateDirectory(divPath);
                for (int week = 1; week <= 10; week++)
                {
                    string weekPath = Path.Combine(divPath, $"week{week}");
                    Directory.CreateDirectory(weekPath);
                    string[] matchDirs = Directory.GetDirectories(weekPath);
                    //if (matchDirs.Length == 0) continue;

                    foreach(string matchDir in matchDirs)
                    {
                        if (!File.Exists(Path.Combine(matchDir, HEADER_FILE))) continue;

                        string[] maps = weekToMaps[week].Select(GetMapString).ToArray();
                        string[] files = Directory.GetFiles(matchDir);

                        using SqliteTransaction transaction = conn.BeginTransaction();
                        for(int map = 1; map <= 6; map++)
                        {
                            string mapName = maps[map - 1];
                            string file = files.FirstOrDefault(x => Path.GetFileName(x).StartsWith(mapName), "");
                            if (string.IsNullOrEmpty(file)) continue;

                            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            string jsonString = File.ReadAllText(file);
                            JsonDocument document = JsonDocument.Parse(jsonString);
                            JsonElement root = document.RootElement;

                            MatchStats matchStats = JsonSerializer.Deserialize<MatchStats>(root.GetProperty("matchStats").ToString(), options);
                            PlayerStats[] playerStats = JsonSerializer.Deserialize<PlayerStats[]>(root.GetProperty("playerStats").ToString(), options);

                            int gameEndTimestamp = root.GetProperty("gameEndTimestamp").GetInt32();
                            string serverIp = root.GetProperty("serverIp").GetString();

                            string gameId = GenerateId($"{matchStats.Map}{gameEndTimestamp}");
                            string serverId = GenerateId(serverIp);
                            string mapId = GenerateId(matchStats.Map);

                            InsertGameRow(conn, gameId, serverId, mapId, gameEndTimestamp, map, week);

                            foreach(PlayerStats stats in playerStats)
                            {
                                if (stats.SteamId == "0") continue;
                                InsertPgStatsRow(conn, stats.SteamId, gameId, stats.Score, stats.Rank, stats.Damage.Dealt, stats.Damage.Taken, stats.Kills, stats.Deaths);
                                InsertPlayerNameRow(conn, stats.SteamId, gameId, stats.Name);
                                

                                InsertPwStatsRow(conn, stats.SteamId, gameId, "rl", stats.Weapons.Rocket.Dg, )
                                //foreach(WeaponStats weapon in stats.Weapons.GetWeapons())
                                //{
                                //    string test = nameof(weapon);
                                //    Console.WriteLine(test);
                                //}
                            }

                        }


                        transaction.Commit();
                    }
                }
            }
        }
    }
}

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

        public static readonly string[] WEAPON_NAMES = ["rl", "lg", "rg", "gl", "pg", "mg", "hmg", "sg", "gt"];

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
            // tried to remove characters that look similar to each other
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

        public static void InsertPlayerRow(SqliteConnection conn, string name, string steamId, int teamId, bool isCaptain)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO players VALUES (@steamId, @name, @teamId, @isCaptain)";
            cmd.Parameters.AddWithValue("@steamId", steamId);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@teamId", teamId);
            cmd.Parameters.AddWithValue("@isCaptain", isCaptain);
            cmd.ExecuteNonQuery();
        }

        public static void InsertTeamRow(SqliteConnection conn, int id, string name, string clanTag, int division)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO teams VALUES (@id, @name, @clanTag, @division)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@clanTag", clanTag);
            cmd.Parameters.AddWithValue("@division", division);
            cmd.ExecuteNonQuery();
        }

        public static void InsertMapRow(SqliteConnection conn, int id, string name)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO maps VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
        }

        public static void InsertMatchRow(SqliteConnection conn, int week, int teamId, int teamScore, int enemyTeamId, int enemyTeamScore)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO matches (week, teamId, score, enemyTeamId, enemyTeamScore) " +
                "VALUES (@week, @teamId, @score, @enemyTeamId, @enemyTeamScore)";
            cmd.Parameters.AddWithValue("@week", week);
            cmd.Parameters.AddWithValue("@teamId", teamId);
            cmd.Parameters.AddWithValue("@score", teamScore);
            cmd.Parameters.AddWithValue("@enemyTeamId", enemyTeamId);
            cmd.Parameters.AddWithValue("@enemyTeamScore", enemyTeamScore);
            cmd.ExecuteNonQuery();
        }

        public static void InsertTgStatsRow(SqliteConnection conn, string gameId, int teamId, int score, int enemyTeamId, int enemyTeamScore, string color)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO tgStats VALUES (@gameId, @teamId, @score, @enemyTeamId, @enemyTeamScore, @color)";
            cmd.Parameters.AddWithValue("@gameId", gameId);

            cmd.ExecuteNonQuery();
        }

        static void Main(string[] args)
        {
            using SqliteConnection conn = new SqliteConnection("Data Source=C:\\Users\\bkandel\\Programs\\td7\\feeder-parser\\stats.db");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM games; DELETE FROM pgstats; DELETE FROM pwstats; DELETE FROM playerNames; " +
                "DELETE FROM players; DELETE FROM teams; DELETE FROM maps";
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            string fileDir = @"C:\Users\bkandel\Programs\td7\feeder-parser";
            string[] lines = File.ReadAllLines(Path.Combine(fileDir, "players.csv"));
            for(int i = 1; i < lines.Length; i++) // skip header
            {
                string[] splitLine = lines[i].Split(",");
                InsertPlayerRow(conn, splitLine[0], splitLine[1], int.Parse(splitLine[2]), bool.Parse(splitLine[3]));
                //Console.WriteLine($"{i}: {splitLine[0]}");
            }

            lines = File.ReadAllLines(Path.Combine(fileDir, "teams.csv"));
            for(int i = 1; i < lines.Length; i++) // skip header
            {
                string[] splitLine = lines[i].Split("|");
                // TODO: add clanTag to teams.csv
                InsertTeamRow(conn, int.Parse(splitLine[0]), splitLine[1], "WRONG_CLAN_TAG", int.Parse(splitLine[2]));
                //Console.WriteLine($"{i}: {splitLine[1]}");
            }

            foreach(Map map in Enum.GetValues<Map>())
            {
                // TODO: get rid of this GetMapString nonsense
                InsertMapRow(conn, (int)map, GetMapString(map));
            }



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
                        string headerFilePath = Path.Combine(matchDir, HEADER_FILE);
                        if (!File.Exists(headerFilePath)) continue;

                        // parse header file
                        string[] headerLines = File.ReadAllLines(headerFilePath);
                        int[] teamIds = headerLines[0].Split("-").Select(x => int.Parse(x.Trim())).ToArray();
                        int[] teamScores = headerLines[1].Split("-").Select(x => int.Parse(x.Trim())).ToArray();
                        InsertMatchRow(conn, week, teamIds[0], teamScores[0], teamIds[1], teamScores[1]);

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

                            foreach(PlayerStats pstats in playerStats)
                            {
                                if (pstats.SteamId == "0") continue;
                                InsertPgStatsRow(conn, pstats.SteamId, gameId, pstats.Score, pstats.Rank, pstats.Damage.Dealt, pstats.Damage.Taken, pstats.Kills, pstats.Deaths);
                                InsertPlayerNameRow(conn, pstats.SteamId, gameId, pstats.Name);

                                WeaponStats[] weaponStats = pstats.Weapons.GetWeapons();
                                for(int i = 0; i < WEAPON_NAMES.Length; i++)
                                {
                                    WeaponStats weapon = weaponStats[i];
                                    string weaponName = WEAPON_NAMES[i];
                                    InsertPwStatsRow(conn, pstats.SteamId, gameId, weaponName, weapon.Dg, weapon.K, weapon.S, weapon.H);
                                }
                            }

                        }


                        transaction.Commit();
                    }
                }
            }
        }
    }
}

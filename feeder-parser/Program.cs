using Microsoft.Data.Sqlite;
using System.Drawing;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static feeder_parser.Weapons;
using static System.Formats.Asn1.AsnWriter;

namespace feeder_parser
{
    public class Program
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
            servituderedux,
            strain,
            ra3map20a,
            ra3map17c,
            charon3dm13d,
            ra3map19c
        }

        public static readonly Dictionary<int, Map[]> weekToMaps = new Dictionary<int, Map[]>
        {
            { 1, [Map.campgrounds, Map.trinity, Map.asylum, Map.nohalls, Map.ra3map20a, Map.uprise] },
            { 2, [Map.strain, Map.charon3dm13d, Map.campgrounds, Map.nohalls, Map.servituderedux, Map.trinity] },
            { 3, [Map.charon3dm13d, Map.asylum, Map.trinity, Map.servituderedux, Map.uprise, Map.ra3map20a] },
            { 4, [Map.uprise, Map.trinity, Map.campgrounds, Map.asylum, Map.charon3dm13d, Map.strain] },
            { 5, [Map.ra3map20a, Map.charon3dm13d, Map.campgrounds, Map.servituderedux, Map.trinity, Map.asylum] },
            { 6, [Map.ra3map17c, Map.strain, Map.asylum, Map.ra3map19c, Map.ra3map20a, Map.quarantine] },
            { 7, [Map.campgrounds, Map.trinity, Map.charon3dm13d, Map.uprise, Map.servituderedux, Map.ra3map20a] },
            { 8, [Map.trinity, Map.uprise, Map.servituderedux, Map.campgrounds, Map.charon3dm13d, Map.asylum] },
            { 9, [Map.ra3map19c, Map.nohalls, Map.quarantine, Map.asylum, Map.ra3map17c, Map.strain] },
            { 10, [Map.charon3dm13d, Map.uprise, Map.servituderedux, Map.nohalls, Map.strain, Map.asylum] },
            { 11, [Map.trinity, Map.nohalls, Map.asylum, Map.uprise, Map.servituderedux, Map.campgrounds] }
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

        public static void InsertGameRow(SqliteConnection conn, string gameId, string serverId, int mapId, long date, int mapNum, int week)
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

        public static void InsertPlayerRow(SqliteConnection conn, string name, string playerId, string steamId, int qlstatsId,
            int teamId, bool isCaptain)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO players VALUES (@playerId, @steamId, @qlstatsId, @name, @teamId, @isCaptain)";
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@steamId", steamId);
            cmd.Parameters.AddWithValue("@qlstatsId", qlstatsId);
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

        public static void InsertTgStatsRow(SqliteConnection conn, string gameId, int teamId, int score, int enemyTeamId, int enemyTeamScore, int color)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO tgStats VALUES (@gameId, @teamId, @score, @enemyTeamId, @enemyTeamScore, @color)";
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@teamId", teamId);
            cmd.Parameters.AddWithValue("@score", score);
            cmd.Parameters.AddWithValue("@enemyTeamId", enemyTeamId);
            cmd.Parameters.AddWithValue("@enemyTeamScore", enemyTeamScore);
            cmd.Parameters.AddWithValue("@color", color);
            cmd.ExecuteNonQuery();
        }

        public static bool ServerAlreadyExists(SqliteConnection conn, string serverId)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM servers WHERE id = @serverId";
            cmd.Parameters.AddWithValue("@serverId", serverId);
            using var reader = cmd.ExecuteReader();
            return reader.HasRows;
        }

        public static void InsertServerRow(SqliteConnection conn, string serverId, string serverName)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO servers VALUES (@serverId, @serverName)";
            cmd.Parameters.AddWithValue("@serverId", serverId);
            cmd.Parameters.AddWithValue("@serverName", serverName);
            cmd.ExecuteNonQuery();
        }

        public static void InsertAwardRow(SqliteConnection conn, int awardId, string awardName, string awardDescription)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO awards VALUES (@id, @name, @description)";
            cmd.Parameters.AddWithValue("@id", awardId);
            cmd.Parameters.AddWithValue("@name", awardName);
            cmd.Parameters.AddWithValue("@description", awardDescription);
            cmd.ExecuteNonQuery();
        }

        public static string TransformPlayerName(string qlPlayerName)
        {
            string pattern = @"\^(\d)";
            string[] parts = Regex.Split(qlPlayerName, pattern);

            if(parts.Length == 1 && parts[0] == qlPlayerName)
            {
                return $"<span class='ql7'>{qlPlayerName}</span>";
            }

            string result = "";
            bool inSpan = false;
            string currentClass = "";

            foreach (string part in parts)
            {
                if (Regex.IsMatch(part, @"^\d$") && inSpan)
                {
                    result += "</span>";
                    inSpan = false;
                }
                else if (Regex.IsMatch(part, @"^\d$") && !inSpan)
                {
                    currentClass = "ql" + part;
                    result += "<span class='" + currentClass + "'>";
                    inSpan = true;
                }
                else
                {
                    result += part;
                }
            }

            // Close any remaining span tag
            if (inSpan)
            {
                result += "</span>";
            }

            return result;
        }

        static string? GetSteamIdFromQlStatsId(SqliteConnection conn, int qlstatsId)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT steamId FROM players WHERE qlstatsId = @qlstatsId";
            cmd.Parameters.AddWithValue("@qlstatsId", qlstatsId);
            using var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    return reader.GetString(0);
                }
            }
            return null;
        }

        static void Main(string[] args)
        {
            using SqliteConnection conn = new SqliteConnection(@"Data Source=C:\Users\Ben\Programs\td7\feeder-parser\stats.db");
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM games; DELETE FROM pgstats; DELETE FROM pwstats; DELETE FROM playerNames; " +
                "DELETE FROM players; DELETE FROM teams; DELETE FROM maps; DELETE FROM tgstats; DELETE FROM matches; " +
                "DELETE FROM servers; DELETE FROM awards; DELETE FROM awardStats;";
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            string fileDir = @"C:\Users\Ben\Programs\td7\feeder-parser";
            string[] lines = File.ReadAllLines(Path.Combine(fileDir, "players.csv"));
            for(int i = 1; i < lines.Length; i++) // skip header
            {
                string[] splitLine = lines[i].Split(",");
                string playerId = GenerateId($"{splitLine[1]}{splitLine[3]}");
                InsertPlayerRow(conn, splitLine[0], playerId, splitLine[1], int.Parse(splitLine[2]), 
                    int.Parse(splitLine[3]), bool.Parse(splitLine[4]));

            }

            lines = File.ReadAllLines(Path.Combine(fileDir, "teams.csv"));
            for(int i = 1; i < lines.Length; i++) // skip header
            {
                string[] splitLine = lines[i].Split("|");
                // TODO: add clanTag to teams.csv
                InsertTeamRow(conn, int.Parse(splitLine[0]), splitLine[1], splitLine[2], int.Parse(splitLine[3]));
                //Console.WriteLine($"{i}: {splitLine[1]}");
            }

            lines = File.ReadAllLines(Path.Combine(fileDir, "awards.csv"));
            for(int i = 1; i < lines.Length; i++)
            {
                string[] splitLine = lines[i].Split(",");
                InsertAwardRow(conn, int.Parse(splitLine[0]), splitLine[1], splitLine[2]);
            }

            foreach(Map map in Enum.GetValues<Map>())
            {
                // TODO: get rid of this GetMapString nonsense
                InsertMapRow(conn, (int)map, map.ToString());
            }

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://qlstats.net/");
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            string baseDir = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(baseDir);
            for(int div = 1; div <= 2; div++)
            {
                string divPath = Path.Combine(baseDir, $"div{div}");
                Directory.CreateDirectory(divPath);
                for (int week = 1; week <= 11; week++)
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
                        InsertMatchRow(conn, week, teamIds[1], teamScores[1], teamIds[0], teamScores[0]);

                        string[] qlstatsLines = headerLines.Skip(2).ToArray();
                        Dictionary<int, string> qlstatsMaps = new Dictionary<int, string>(); // maps mapNum to qlstats game id
                        foreach (string line in qlstatsLines)
                        {
                            string[] splitLine = line.Trim().Split(" ");
                            int mapNum = int.Parse(splitLine[0]);
                            qlstatsMaps[mapNum] = splitLine[1];
                        }

                        string[] maps = weekToMaps[week].Select(x => x.ToString()).ToArray();
                        string[] files = Directory.GetFiles(matchDir);

                        using SqliteTransaction transaction = conn.BeginTransaction();

                        for(int map = 1; map <= 6; map++)
                        {
                            string mapName = maps[map - 1];
                            string file = files.FirstOrDefault(x => Path.GetFileName(x).StartsWith(mapName), "");

                            MatchStats matchStats;
                            List<PlayerStats> playerStats;
                            long gameEndTimestamp;
                            string gameId;
                            string serverId;
                            int mapId = (int)weekToMaps[week][map - 1];

                            if (!string.IsNullOrEmpty(file)) // we have json data from the feeder
                            {
                                string jsonString = File.ReadAllText(file);
                                JsonDocument document = JsonDocument.Parse(jsonString);
                                JsonElement root = document.RootElement;

                                matchStats = JsonSerializer.Deserialize<MatchStats>(root.GetProperty("matchStats").ToString(), options);
                                playerStats = JsonSerializer.Deserialize<PlayerStats[]>(root.GetProperty("playerStats").ToString(), options)
                                    .Where(ps => ps.SteamId != "0").ToList();

                                gameEndTimestamp = root.GetProperty("gameEndTimestamp").GetInt64();
                                gameId = GenerateId($"{matchStats.Map}{gameEndTimestamp}");
                                serverId = GenerateId(matchStats.ServerTitle);

                            }else if (qlstatsMaps.ContainsKey(map)) // we have qlstats data
                            {
                                string qlstatsGameId = qlstatsMaps[map];
                                HttpResponseMessage response = httpClient.GetAsync($"/game/{qlstatsGameId}.json").Result;
                                string jsonString = response.Content.ReadAsStringAsync().Result;

                                JsonDocument qlstatsDocument = JsonDocument.Parse(jsonString);
                                JsonElement qlstatsRoot = qlstatsDocument.RootElement;

                                // the goal is to transform the qlstats data into the same format as our feeder json data

                                string startDate = qlstatsRoot.GetProperty("game").GetProperty("start").ToString();
                                gameEndTimestamp = ConvertZuluToEpoch(startDate);
                                gameId = GenerateId($"{map}{startDate}");
                                serverId = GenerateId(qlstatsRoot.GetProperty("server").GetProperty("name").ToString());

                                matchStats = new MatchStats();
                                matchStats.Map = mapName;
                                matchStats.ServerTitle = qlstatsRoot.GetProperty("server").GetProperty("name").ToString();

                                JsonElement tgStats = qlstatsRoot.GetProperty("tgstats");
                                if (tgStats[0].GetProperty("team").GetInt32() == 1)
                                {
                                    matchStats.RedScore = tgStats[0].GetProperty("rounds").GetInt32();
                                    matchStats.BlueScore = tgStats[1].GetProperty("rounds").GetInt32();
                                }
                                else
                                {
                                    matchStats.RedScore = tgStats[1].GetProperty("rounds").GetInt32();
                                    matchStats.BlueScore = tgStats[0].GetProperty("rounds").GetInt32();
                                }

                                playerStats = [];
                                JsonElement pgStats = qlstatsRoot.GetProperty("pgstats");
                                foreach(JsonElement element in pgStats.EnumerateArray())
                                {
                                    PlayerStats pstats = new PlayerStats();
                                    pstats.Damage = new Damage();
                                    pstats.Damage.Dealt = element.GetProperty("pushes").GetInt32();
                                    pstats.Damage.Taken = element.GetProperty("destroys").GetInt32();
                                    pstats.Deaths = element.GetProperty("deaths").GetInt32();
                                    pstats.Kills = element.GetProperty("kills").GetInt32();
                                    pstats.Rank = element.GetProperty("rank").GetInt32();
                                    pstats.Score = element.GetProperty("score").GetInt32();
                                    int qlstatsId = element.GetProperty("player_id").GetInt32();
                                    string? steamId = GetSteamIdFromQlStatsId(conn, qlstatsId);
                                    if (steamId == null)
                                    {
                                        Console.WriteLine($"week{week} {Path.GetFileName(matchDir)} map{map} " +
                                            $"don't recognize qlstatsId {qlstatsId}");
                                        continue;
                                    }
                                    pstats.SteamId = steamId;
                                    pstats.Team = element.GetProperty("team").GetInt32();
                                    pstats.Win = MyTeamWon(pstats.Team, matchStats.RedScore, matchStats.BlueScore) ? 1 : 0;
                                    pstats.Lose = MyTeamWon(pstats.Team, matchStats.RedScore, matchStats.BlueScore) ? 0 : 1;
                                    playerStats.Add(pstats);
                                }

                                bool MyTeamWon(int team, int redScore, int blueScore)
                                {
                                    if(team == 1)
                                    {
                                        return redScore > blueScore;
                                    }
                                    return blueScore > redScore;
                                }

                                JsonElement pwStats = qlstatsRoot.GetProperty("pwstats");
                                foreach(JsonProperty prop in pwStats.EnumerateObject())
                                {
                                    int qlstatsId = int.Parse(prop.Name);
                                    string steamId = GetSteamIdFromQlStatsId(conn, qlstatsId);
                                    if (steamId == null) continue;
                                    PlayerStats pstats = playerStats.First(x => x.SteamId == steamId);
                                    pstats.SteamId = steamId;

                                    JsonElement element = prop.Value;
                                    pstats.Name = element.GetProperty("nick").ToString();

                                    pstats.Weapons = new Weapons();
                                    WeaponStats[] weapons = new WeaponStats[WEAPON_NAMES.Length];
                                    for (int i = 0; i < WEAPON_NAMES.Length; i++)
                                    {
                                        string weapName = WEAPON_NAMES[i];
                                        WeaponStats weap = new WeaponStats();
                                        weap.Dg = 0;
                                        weap.K = 0;
                                        weap.S = 0;
                                        weap.H = 0;

                                        if(element.GetProperty("weapons").TryGetProperty(weapName, out JsonElement weapElement))
                                        {
                                            weap.Dg = int.Parse(weapElement[1].ToString());
                                            weap.K = int.Parse(weapElement[0].ToString());
                                            weap.S = int.Parse(weapElement[3].ToString());
                                            weap.H = int.Parse(weapElement[2].ToString());
                                        }
                                        weapons[i] = weap;
                                    }
                                    pstats.Weapons.SetWeapons(weapons);
                                }

                            }
                            else // we have NO data
                            {
                                Console.WriteLine($"Missing week{week} {Path.GetFileName(matchDir)} map {map}");
                                continue;
                            }

                            // insert rows into the db

                            InsertGameRow(conn, gameId, serverId, mapId, gameEndTimestamp, map, week);

                            InsertTgStatsRow(conn, gameId, teamIds[0], matchStats.RedScore, teamIds[1], matchStats.BlueScore, 0);
                            InsertTgStatsRow(conn, gameId, teamIds[1], matchStats.BlueScore, teamIds[0], matchStats.RedScore, 1);

                            if (!ServerAlreadyExists(conn, serverId))
                            {
                                InsertServerRow(conn, serverId, matchStats.ServerTitle);
                            }

                            foreach (PlayerStats pstats in playerStats)
                            {
                                // if we're on red team, the team id is teamIds[0], blue team id is teamIds[1]
                                int teamId = pstats.Team == 1 ? teamIds[0] : teamIds[1];
                                string playerId = GenerateId($"{pstats.SteamId}{teamId}");
                                pstats.PlayerId = playerId; // set this for later
                                InsertPgStatsRow(conn, playerId, gameId, pstats.Score, pstats.Rank, pstats.Damage.Dealt, pstats.Damage.Taken, pstats.Kills, pstats.Deaths);
                                InsertPlayerNameRow(conn, playerId, gameId, TransformPlayerName(pstats.Name));

                                WeaponStats[] weaponStats = pstats.Weapons.GetWeapons();
                                for(int i = 0; i < WEAPON_NAMES.Length; i++)
                                {
                                    WeaponStats weapon = weaponStats[i];
                                    string weaponName = WEAPON_NAMES[i];
                                    InsertPwStatsRow(conn, playerId, gameId, weaponName, weapon.Dg, weapon.K, weapon.S, weapon.H);
                                }
                            }

                            // lastly, do awards
                            Awards.CheckForAwards(conn, gameId, matchStats, playerStats);
                        }


                        transaction.Commit();
                    }
                }
            }
        }

        // thanks free-tier chat gpt
        private static long ConvertZuluToEpoch(string dateStr)
        {
            DateTime dateTime = DateTime.ParseExact(dateStr, "yyyy-MM-ddTHH:mm:ssZ", null, System.Globalization.DateTimeStyles.RoundtripKind);

            long epochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            long timestamp = (dateTime.Ticks - epochTicks) / TimeSpan.TicksPerSecond;
            return timestamp;
        }
    }
}

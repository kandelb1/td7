using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices.JavaScript;
using System.Xml.Linq;

namespace feeder_parser
{
    internal class Awards
    {
        public static void CheckForAwards(SqliteConnection conn, string gameId, MatchStats matchStats, 
            List<PlayerStats> playerStats)
        {

            foreach(PlayerStats pstats in playerStats)
            {

                //string playerId = pstats.SteamId;
                string playerId = pstats.PlayerId; 
                // 1,I'm The Brut Now,Win a game on Overkill while taking the most damage on your team.
                int damageTaken = pstats.Damage.Taken;
                int mostDamageTaken = playerStats.Where(x => x.Team == pstats.Team).MaxBy(x => x.Damage.Taken).Damage.Taken;
                if (matchStats.Map == "nohalls" && damageTaken >= mostDamageTaken)
                {
                    InsertAwardStats(conn, gameId, playerId, 1);
                }

                // 2,Ban This Guy,Fire at least 1 shot with MG.
                if (pstats.Weapons.Machinegun.S >= 1)
                {
                    InsertAwardStats(conn, gameId, playerId, 2);
                }

                // 3,Mccool's Pineapple's,Get 3 kills with GL in a single game.
                if (pstats.Weapons.Grenade.K >= 3)
                {
                    InsertAwardStats(conn, gameId, playerId, 3);
                }

                // 4,Plasma Pontiff,Get 3 kills with PG in a single game.
                if(pstats.Weapons.Plasma.K >= 3)
                {
                    InsertAwardStats(conn, gameId, playerId, 4);
                }

                // 5,Honorary Zoomer,Finish a game with over 50% LG accuracy.
                //if(pstats.Weapons.Lightning.)
                if(GetAccuracy(pstats.Weapons.Lightning) >= .5f)
                {
                    InsertAwardStats(conn, gameId, playerId, 5);
                }

                // 6,Bad Baiter,Lose a game with RG as your most damaging weapon.
                int rgDamage = pstats.Weapons.Railgun.Dg;
                int highestDamage = pstats.Weapons.GetWeapons().MaxBy(x => x.Dg).Dg;
                if(pstats.Lose == 1 && rgDamage >= highestDamage)
                {
                    InsertAwardStats(conn, gameId, playerId, 6);
                }

                // 7,Good Baiter,Win a game with RG as your most damaging weapon.
                if(pstats.Win == 1 && rgDamage >= highestDamage)
                {
                    InsertAwardStats(conn, gameId, playerId, 7);
                }

                // 8,A Win is a Win,Win a game with 0 frags.
                if(pstats.Win == 1 && pstats.Kills == 0)
                {
                    InsertAwardStats(conn, gameId, playerId, 8);
                }

                // 9,KGB's Dream,Finish a game with over 75% RG accuracy.
                if (GetAccuracy(pstats.Weapons.Railgun) >= .75f)
                {
                    InsertAwardStats(conn, gameId, playerId, 9);
                }

                // 10,net_restart,Finish a game with less than 10% SG accuracy
                if(GetAccuracy(pstats.Weapons.Shotgun) <= .1f)
                {
                    InsertAwardStats(conn, gameId, playerId, 10);
                }

                // 11,Bad Luck Brut,Finish a game with 0% RG accuracy (with 5 or more shots).
                if(GetAccuracy(pstats.Weapons.Railgun) == 0 && pstats.Weapons.Railgun.S >= 5)
                {
                    InsertAwardStats(conn, gameId, playerId, 11);
                }

                // 12,9th Point Curse,Lose a game 9-10.
                if(pstats.Lose == 1 && GetTeamScore(pstats.Team) == 9)
                {
                    InsertAwardStats(conn, gameId, playerId, 12);
                }

                // 13,Mccool's Dream,Win a game at 10-9.
                if(pstats.Win == 1 && GetEnemyTeamScore(pstats.Team) == 9)
                {
                    InsertAwardStats(conn, gameId, playerId, 13);
                }

                // 14,Torrin's Handcam,Finish a game with less than 30% LG accuracy.
                if (GetAccuracy(pstats.Weapons.Lightning) <= .3f)
                {
                    InsertAwardStats(conn, gameId, playerId, 14);
                }

                // 15,Skinner's List,Finish a game with greater than 85% RL accuracy.
                if(GetAccuracy(pstats.Weapons.Rocket) >= .85f)
                {
                    InsertAwardStats(conn, gameId, playerId, 15);
                }

                // 16,Solo's Database,Finish a game with over 50% LG and 75% RG accuracy.
                if(GetAccuracy(pstats.Weapons.Lightning) >= .5f && GetAccuracy(pstats.Weapons.Railgun) >= .75f)
                {
                    InsertAwardStats(conn, gameId, playerId, 16);
                }

                // 17,Gribbled's Disciple,Finish a game without using LG.
                if(pstats.Weapons.Lightning.S == 0)
                {
                    InsertAwardStats(conn, gameId, playerId, 17);
                }

                // deleted 18, Up Close and Personal

                // 19,Back to Fortnite,Lose a game 10-0.
                if(pstats.Lose == 1 && GetTeamScore(pstats.Team) == 0)
                {
                    InsertAwardStats(conn, gameId, playerId, 19);
                }

                // 20,A Glimmer of Hope,Lose a game 1-10.
                if(pstats.Lose == 1 && GetTeamScore(pstats.Team) == 1)
                {
                    InsertAwardStats(conn, gameId, playerId, 20);
                }

                // 21,Spit on Him Father,Win a game 10-0.
                if(pstats.Win == 1 && GetEnemyTeamScore(pstats.Team) == 0)
                {
                    InsertAwardStats(conn, gameId, playerId, 21);
                }
            }


            int GetTeamScore(int team)
            {
                if (team == 1) return matchStats.RedScore;
                return matchStats.BlueScore;
            }

            int GetEnemyTeamScore(int team)
            {
                if (team == 1) return matchStats.BlueScore;
                return matchStats.RedScore;
            }
        }

        private static float GetAccuracy(Weapons.WeaponStats weaponStats)
        {
            float accuracy = (float)weaponStats.H / weaponStats.S;
            return accuracy;
        }



        private static void InsertAwardStats(SqliteConnection conn, string gameId, string playerId, int awardId)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO awardStats VALUES (@gameId, @playerId, @awardId)";
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@awardId", awardId);
            cmd.ExecuteNonQuery();
        }
    }
}

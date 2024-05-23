using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace feeder_parser
{
    public class Damage
    {
        public int Dealt { get; set; }
        public int Taken { get; set; }
    }

    public class Medals
    {
        public int Accuracy { get; set; }
        public int Assists { get; set; }
        public int Captures { get; set; }
        public int Combokill { get; set; }
        public int Defends { get; set; }
        public int Excellent { get; set; }
        public int Firstfrag { get; set; }
        public int Headshot { get; set; }
        public int Humiliation { get; set; }
        public int Impressive { get; set; }
        public int Midair { get; set; }
        public int Perfect { get; set; }
        public int Perforated { get; set; }
        public int Quadgod { get; set; }
        public int Rampage { get; set; }
        public int Revenge { get; set; }
    }

    public class Weapons
    {
        public class WeaponStats
        {
            public int D { get; set; }
            public int Dg { get; set; }
            public int Dr { get; set; }
            public int H { get; set; }
            public int K { get; set; }
            public int P { get; set; }
            public int S { get; set; }
            public int T { get; set; }
        }

        public WeaponStats Gauntlet { get; set; }
        public WeaponStats Grenade { get; set; }
        public WeaponStats Hmg { get; set; }
        public WeaponStats Lightning { get; set; }
        public WeaponStats Machinegun { get; set; }

        public WeaponStats Plasma { get; set; }
        public WeaponStats Railgun { get; set; }
        public WeaponStats Rocket { get; set; }
        public WeaponStats Shotgun { get; set; }

        //WEAPON_NAMES = ["rl", "lg", "rg", "gl", "pg", "mg", "hmg", "sg", "gt"];
        public WeaponStats[] GetWeapons() => [Rocket, Lightning, Railgun, Grenade, Plasma, Machinegun, Hmg, Shotgun, Gauntlet];
        
        public void SetWeapons(WeaponStats[] weapons)
        {
            Rocket = weapons[0];
            Lightning = weapons[1];
            Railgun = weapons[2];
            Grenade = weapons[3];
            Plasma = weapons[4];
            Machinegun = weapons[5];
            Hmg = weapons[6];
            Shotgun = weapons[7];
            Gauntlet = weapons[8];
        }
    }

    public class PlayerStats
    {
        public Damage Damage { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public Medals? Medals { get; set; } // maybe ill use it
        public string Name { get; set; }
        public int Rank { get; set; }
        public int Score { get; set; }
        [JsonPropertyName("STEAM_ID")]
        public string SteamId { get; set; }
        public string PlayerId { get; set; } // set by us
        public int Team { get; set; } // red 1, blue 2
        public Weapons Weapons { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
    }
}

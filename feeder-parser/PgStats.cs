using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace feeder_parser
{
    internal class PgStats
    {
        [JsonPropertyName("pushes")]
        public int DamageGiven { get; set; }

        [JsonPropertyName("destroys")]
        public int DamageTaken { get; set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Rank { get; set; }
        public int Score { get; set; }
        public int Team { get; set; }
    }
}

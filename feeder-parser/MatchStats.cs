using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace feeder_parser
{
    public class MatchStats
    {
        public string Map { get; set; }

        [JsonPropertyName("SERVER_TITLE")]
        public string ServerTitle { get; set; }

        [JsonPropertyName("TSCORE0")]
        public int RedScore { get; set; }

        [JsonPropertyName("TSCORE1")]
        public int BlueScore { get; set; }
    }
}
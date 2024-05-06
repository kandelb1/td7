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
        public bool Aborted { get; set; }

        [JsonPropertyName("CAPTURE_LIMIT")]
        public int CaptureLimit { get; set; }
        [JsonPropertyName("EXIT_MSG")]
        public string ExitMsg { get; set; }

        public string Factory { get; set; }

        [JsonPropertyName("FACTORY_TITLE")]
        public string FactoryTitle { get; set; }

        [JsonPropertyName("FIRST_SCORER")]
        public string FirstScorer { get; set; }

        [JsonPropertyName("FRAG_LIMIT")]
        public int FragLimit { get; set; }

        [JsonPropertyName("GAME_LENGTH")]
        public int GameLength { get; set; }

        [JsonPropertyName("GAME_TYPE")]
        public string GameType { get; set; }

        public int Infected { get; set; }
        public int Instagib { get; set; }

        [JsonPropertyName("LAST_SCORER")]
        public string LastScorer { get; set; }

        [JsonPropertyName("LAST_TEAM_SCORER")]
        public string LastTeamscorer { get; set; }
        public string Map { get; set; }

        [JsonPropertyName("MATCH_GUID")]
        public string MatchGuid { get; set; }

        [JsonPropertyName("SERVER_TITLE")]
        public string ServerTitle { get; set; }
        public int Tscore0 { get; set; } // red
        public int Tscore1 { get; set; } // blue
    }
}


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommandLine.Models
{
    public class PlayerOptions
    {
        public bool DisableMusic { get; set; }

        public bool FastSpellCasting { get; set; }

        public bool RemapUpAToUpSelect { get; set; }

        public bool RemoveFlashingUponDeath { get; set; }

        public bool DisableHUDLag { get; set; }

        public string? Sprite { get; set; }

        public string? TunicColor { get; set; }

        public string? ShieldTunicColor { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BeamSprite BeamSprite { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BeepThreshold BeepThreshold { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BeepFrequency BeepFrequency { get; set; }
    }
}

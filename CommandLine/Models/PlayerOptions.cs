
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RandomizerCore;

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

        [JsonConverter(typeof(StringEnumConverter))]
        public CharacterColor TunicColor { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CharacterColor ShieldTunicColor { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BeamSprites BeamSprite { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BeepThreshold BeepThreshold { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BeepFrequency BeepFrequency { get; set; }
    }
}

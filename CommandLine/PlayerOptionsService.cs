using CommandLine.Models;
using Newtonsoft.Json;
using NLog;
using System.Reflection;
using Z2Randomizer.Core;

namespace CommandLine
{
    public class PlayerOptionsService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static string[] SupportedTunicColors =
        {
            "Default",
            "Green",
            "Dark Green",
            "Aqua",
            "Dark Blue",
            "Purple",
            "Pink",
            "Orange",
            "Red",
            "Turd",
            "Random"
        };

        private CharacterSprite[] spriteOptions;

        public PlayerOptionsService()
        {
            this.spriteOptions = CharacterSprite.Options();
        }

        public PlayerOptions? LoadFromFile(string? path)
        {
            if (path == null)
            {
                logger.Info("Using DefaultPlayerOptions.json to apply player options.");
                var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                path = Path.Combine(directory!, "DefaultPlayerOptions.json");
                logger.Debug(path);
            }

            if (!File.Exists(path))
            {
                throw new Exception($"Player options file {path} does not exist");
            }

            string optionsText = File.ReadAllText(path);
            var options = JsonConvert.DeserializeObject<PlayerOptions>(optionsText);

            Validate(options);

            return options;
        }

        public void ApplyOptionsToConfiguration(PlayerOptions playerOptions, RandomizerConfiguration configuration)
        {
            configuration.BeepFrequency = playerOptions.BeepFrequency switch
            {
                BeepFrequency.Normal => 0x30,
                BeepFrequency.HalfSpeed => 0x60,
                BeepFrequency.QuarterSpeed => 0xC0,
                BeepFrequency.Off => 0,
                _ => 0x30
            };

            configuration.BeepThreshold = playerOptions.BeepThreshold switch
            {
                BeepThreshold.Normal => 0x20,
                BeepThreshold.HalfBar => 0x10,
                BeepThreshold.QuarterBar => 0x08,
                BeepThreshold.TwoBars => 0x40,
                _ => 0x20
            };

            configuration.RemoveFlashing = playerOptions.RemoveFlashingUponDeath;
            configuration.UpAOnController1 = playerOptions.RemapUpAToUpSelect;
            configuration.Tunic = playerOptions.TunicColor;
            configuration.ShieldTunic = playerOptions.ShieldTunicColor;
            configuration.DisableMusic = playerOptions.DisableMusic;
            configuration.DisableHUDLag = playerOptions.DisableHUDLag;
            configuration.FastSpellCasting = playerOptions.FastSpellCasting;

            var sprite = GetSprite(playerOptions.Sprite);
            // If somehow sprite is null, default to Link
            configuration.Sprite = sprite?.SelectionIndex ?? 0;
        }

        private void ValidateTunicColor(string? color)
        {
            if (color == null || !SupportedTunicColors.Contains(color))
            {
                throw new Exception($"Unsupported tunic color. \n" +
                    $"Supported colors are {JsonConvert.SerializeObject(SupportedTunicColors)}");
            }
        }

        public void Validate(PlayerOptions? playerOptions)
        {
            CharacterSprite? selectedSprite = null;
            if (playerOptions != null)
            {
                ValidateTunicColor(playerOptions.TunicColor);
                ValidateTunicColor(playerOptions.ShieldTunicColor);

                selectedSprite = GetSprite(playerOptions.Sprite);
            }

            if (selectedSprite == null)
            {
                var validSprites = spriteOptions.Select(sprite => sprite.DisplayName).ToArray();
                var spriteList = JsonConvert.SerializeObject(validSprites);
                throw new Exception($"Invalid sprite selected. Valid options are \n{spriteList}");
            }
        }

        public CharacterSprite? GetSprite(string? name)
        {
            return this.spriteOptions.FirstOrDefault(sprite => string.Equals(
                sprite.DisplayName, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}

using Z2Randomizer.CommandLine.Models;
using Newtonsoft.Json;
using NLog;
using System.Reflection;
using CrossPlatformUI.Services;
using Desktop.Common;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.CommandLine
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

        private IList<CharacterSprite> spriteOptions = new List<CharacterSprite>();

        public PlayerOptionsService()
        {
            var fileservice = new DesktopFileService();
            IEnumerable<string> spriteFiles = fileservice.ListLocalFiles(IFileSystemService.RandomizerPath.Sprites)?.Result ?? [];
            spriteOptions.Add(CharacterSprite.LINK);
            foreach (var spriteFile in spriteFiles)
            {
                var patch = fileservice.OpenBinaryFile(IFileSystemService.RandomizerPath.Sprites, spriteFile).Result;
                var parsedName = Path.GetFileNameWithoutExtension(spriteFile).Replace("_", " ");
                spriteOptions.Add(new CharacterSprite(parsedName, patch));
            }
            spriteOptions.Add(CharacterSprite.RANDOM);
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
            configuration.BeepFrequency = playerOptions.BeepFrequency;
            configuration.BeepThreshold = playerOptions.BeepThreshold;

            configuration.RemoveFlashing = playerOptions.RemoveFlashingUponDeath;
            configuration.UpAOnController1 = playerOptions.RemapUpAToUpSelect;
            configuration.ChangeItemSprites = playerOptions.ChangeItemSprites;
            configuration.Tunic = playerOptions.TunicColor;
            configuration.TunicOutline = playerOptions.TunicOutlineColor;
            configuration.ShieldTunic = playerOptions.ShieldTunicColor;
            configuration.DisableMusic = playerOptions.DisableMusic;
            configuration.RandomizeMusic = playerOptions.RandomizeMusic;
            configuration.MixCustomAndOriginalMusic = playerOptions.MixCustomAndOriginalMusic;
            configuration.DisableUnsafeMusic = playerOptions.DisableUnsafeMusic;
            configuration.DisableHUDLag = playerOptions.DisableHUDLag;
            configuration.FastSpellCasting = playerOptions.FastSpellCasting;

            var sprite = GetSprite(playerOptions.Sprite);
            // If somehow sprite is null, default to Link
            configuration.Sprite = sprite ?? CharacterSprite.LINK;
            // configuration.Sprite = CharacterSprite.LINK;
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
                // ValidateTunicColor(playerOptions.TunicColor);
                // ValidateTunicColor(playerOptions.ShieldTunicColor);

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
            return spriteOptions.FirstOrDefault(sprite => string.Equals(
                sprite.DisplayName, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}

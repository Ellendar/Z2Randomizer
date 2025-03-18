using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CommandLine;
using CrossPlatformUI;
using Desktop.Common;
using McMaster.Extensions.CommandLineUtils;
using NLog;
using RandomizerCore;
using RandomizerCore.Sidescroll;

namespace Z2Randomizer.CommandLine;

public class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Program))]
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Option(ShortName = "f", Description = "Flag string")]
    public string? Flags { get; }

    [Option(ShortName = "r", Description = "Path to the base ROM file")]
    public string? Rom { get; }

    [Option(ShortName = "c", Description = "Path to a file containing the JSON for a RandomizerConfiguration. See the included examples.")]
    public string? Configuration { get; }

    [Option(ShortName = "o", Description = "Path to the folder to save the ROM")]
    public string? OutputPath { get; }

    [Option(ShortName = "s", Description = "[Optional] Seed used to generate the shuffled ROM")]
    public int? Seed { get; set; }

    [Option(ShortName = "po", Description = "[Optional] Specifies a player options file to use for misc settings")]
    public string? PlayerOptions { get; }

    private RandomizerConfiguration? configuration;
    private byte[]? vanillaRomData;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Program))]
    public int OnExecute()
    {
        SetNlogLogLevel(LogLevel.Info);


        if (Configuration == null)
        {
            if (string.IsNullOrEmpty(Flags))
            {
                logger.Error("The flag string is required");
                return -1;
            }
            configuration = new RandomizerConfiguration(Flags);
        }
        else
        {

            if (!File.Exists(Configuration))
            {
                logger.Error($"The specified Configuration file does not exist: {Configuration}");
                return -4;
            }
            string configurationString = File.ReadAllText(Configuration);
            configuration = JsonSerializer.Deserialize(configurationString, SerializationContext.Default.RandomizerConfiguration);
        }

        if (!Seed.HasValue) 
        {
            var r = new Random();
            Seed = r.Next(1000000000);
        } 
        configuration.Seed = Seed.Value.ToString();

        if (string.IsNullOrEmpty(Rom))
        {
            logger.Error("The ROM path is required");
            return -2;
        }

        if (!File.Exists(Rom))
        {
            logger.Error($"The specified ROM file does not exist: {Rom}");
            return -3;
        }

        vanillaRomData = File.ReadAllBytes(Rom);

        logger.Info($"Flags: {Flags}");
        logger.Info($"Rom: {Rom}");
        logger.Info($"Seed: {Seed}");

        try
        {
            var playerOptionsService = new PlayerOptionsService();
            var playerOptions = playerOptionsService.LoadFromFile(this.PlayerOptions);
            if (playerOptions == null)
            {
                throw new Exception("Could not load player options");
            }

            playerOptionsService.ApplyOptionsToConfiguration(playerOptions, configuration);
        }
        catch (Exception exception)
        {
            logger.Fatal(exception);
            return -4;
        }

        Randomize().Wait();

        return 0;
    }

    public async Task Randomize()
    {
        // Exception? generationException = null;
        // var worker = new BackgroundWorker();
        // worker.DoWork += new DoWorkEventHandler(RandomizationWorker!);
        // worker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged!);
        // worker.WorkerReportsProgress = true;
        // worker.WorkerSupportsCancellation = true;
        // worker.RunWorkerCompleted += (completed_sender, completed_event) =>
        // {
        //     generationException = completed_event.Error;
        // };
        // worker.RunWorkerAsync();
        var cts = new CancellationTokenSource();
        Hyrule.NewAssemblerFn createAsm = (opts, debug) => new DesktopJsEngine(opts, debug);
        var roomsJson = RandomizerCore.Util.ReadAllTextFromFile("PalaceRooms.json");
        var customJson = configuration!.UseCustomRooms ? RandomizerCore.Util.ReadAllTextFromFile("CustomRooms.json") : null;
        var palaceRooms = new PalaceRooms(configuration!.UseCustomRooms ? customJson : roomsJson, configuration!.UseCustomRooms);
        var randomizer = new Hyrule(createAsm,palaceRooms);
        var rom = await randomizer.Randomize(vanillaRomData!, configuration, UpdateProgress, cts.Token);

        if (rom != null)
        {
            
            char os_sep = Path.DirectorySeparatorChar;
            var filename = Rom!;
            var outpath = OutputPath ?? filename[..filename.LastIndexOf(os_sep)];
            string newFileName =  $"{outpath}/Z2_{Seed}_{Flags}.nes";
            File.WriteAllBytes(newFileName, rom);
            logger.Info("File " + "Z2_" + this.Seed + "_" + this.Flags + ".nes" + " has been created!");
        }
        else
        {
            logger.Error("An exception occurred generating the rom");
        }
    }
    
    private async Task UpdateProgress(string str)
    {
        await Task.Run(() => logger.Info(str));
    }

    private static void SetNlogLogLevel(LogLevel level)
    {
        // Uncomment these to enable NLog logging. NLog exceptions are swallowed by default.
        ////NLog.Common.InternalLogger.LogFile = @"C:\Temp\nlog.debug.log";
        ////NLog.Common.InternalLogger.LogLevel = LogLevel.Debug;

        if (level == LogLevel.Off)
        {
            LogManager.SuspendLogging();
        }
        else
        {
            if (!LogManager.IsLoggingEnabled())
            {
                LogManager.SuspendLogging();
            }

            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                // Iterate over all levels up to and including the target, (re)enabling them.
                for (int i = level.Ordinal; i <= 5; i++)
                {
                    rule.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
                }
            }
        }

        LogManager.ReconfigExistingLoggers();
    }
}
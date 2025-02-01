using NLog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Desktop.Common;
using RandomizerCore;
using RandomizerCore.Sidescroll;

namespace Z2Randomizer.Statistics;

/// <summary>
/// This is a hacky version of how this should work. EVENTUALLY the entirety of the options from the config should
/// be stored into a single object that this and Hyrule use as an input, and then Hyrule should ouput a single state object.
/// Then this can persist the state, and the main application carries a writer that just writes the output state to the ROM.
/// This will allow us to use some simple ORM to persist the entirety of the input and output.
/// 
/// For now, we're just picking out a few choice fields that i'm interested in and recording them.
/// </summary>
class Statistics
{
    //private static readonly string FLAGS = "hEAK0thCqbLyhAAL4XpGU+!5@W4xeWvdAALhA"; //Random% vanilla
    //private static readonly string FLAGS = "hEAAp1dAOR4YXs0uhjGs371g+hBswv9svsthABVA"; //Random%
    private static readonly string FLAGS = "hEAAp1dAOR4YXs0uhjGs371g+hBswv9svsthABVA"; //test

    private static readonly string VANILLA_ROM_PATH = "C:\\emu\\NES\\roms\\Zelda2.nes";
    private static readonly string DB_PATH = "C:\\Workspace\\Z2Randomizer_4_4\\Statistics\\db\\stats.sqlite";
    private static readonly int LIMIT = 10;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    static void Main()
    {
        StatisticsDbContext dbContext = new StatisticsDbContext(DB_PATH);

        RandomizerConfiguration config = new RandomizerConfiguration(FLAGS);
        Random random = new Random();
        Hyrule.NewAssemblerFn createAsm = (opts, debug) => new DesktopJsEngine(opts, debug);
        var roomsJson = Util.ReadAllTextFromFile("PalaceRooms.json");
        var customJson = config.UseCustomRooms ? Util.ReadAllTextFromFile("CustomRooms.json") : null;
        var palaceRooms = new PalaceRooms(roomsJson, false);
        var randomizer = new Hyrule(createAsm,palaceRooms);
        logger.Info("Started statistics generation with limit: " + LIMIT);
        try
        {
            for (int i = 0; i < LIMIT; i++)
            {
                int seed = random.Next(1000000000);
                //int seed = 704113586;
                config.Seed = seed.ToString();
                var vanillaRomData = File.ReadAllBytes(VANILLA_ROM_PATH);
                DateTime startTime = DateTime.Now;
                logger.Info("Starting seed# " + i + " at: " + startTime);
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                randomizer.Randomize(vanillaRomData, config, UpdateProgress, tokenSource.Token).Wait(tokenSource.Token);
                DateTime endTime = DateTime.Now;
                Result result = new Result(randomizer);
                result.GenerationTime = (int)(endTime - startTime).TotalMilliseconds;
                dbContext.Add(result);
                dbContext.Add(randomizer.Props);
                logger.Info("Finished seed# " + i + " in: " + result.GenerationTime + "ms");
                //dbContext.SaveChanges();
            }
            dbContext.SaveChanges();
        }
        catch(Exception e) { logger.Error(e); }
    }
    
    private static async Task UpdateProgress(string str)
    {
        await Task.Run(() => logger.Trace(str));
    }
}

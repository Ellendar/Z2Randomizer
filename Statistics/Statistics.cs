using NLog;
using Z2Randomizer.Core;
using System;
using System.ComponentModel;
using System.IO;
using CommandLine;
using System.Threading;

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
    private static readonly string FLAGS = "hEAK0thCqbLyhAAL4XpGU+!5@W4xeWvdAALhA"; //Random% vanilla
    //private static readonly string FLAGS = "hEAK0thCqbs36emL4XpGU+!5@W4xeWvdAALhA"; //Random%
    //"hEAK0sALirpUe5RLkbgZQ+2c4YX@4X4yAASA" v4 only Random%

    private static readonly string VANILLA_ROM_PATH = "C:\\emu\\NES\\roms\\Zelda2.nes";
    private static readonly string DB_PATH = "C:\\Workspace\\Z2Randomizer\\Statistics\\db\\stats.sqlite";
    private static readonly int LIMIT = 1000;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    static void Main()
    {
        StatisticsDbContext dbContext = new StatisticsDbContext(DB_PATH);

        Random random = new Random();
        var engine = new DesktopJsEngine();
        logger.Info("Started statistics generation with limit: " + LIMIT);
        try
        {
            for (int i = 0; i < LIMIT; i++)
            {
                RandomizerConfiguration config = new RandomizerConfiguration(FLAGS);
                int seed = random.Next(1000000000);
                //int seed = 38955385;
                config.Seed = seed;
                var vanillaRomData = File.ReadAllBytes(VANILLA_ROM_PATH);
                DateTime startTime = DateTime.Now;
                logger.Info("Starting seed# " + i + " at: " + startTime);
                Hyrule hyrule = new Hyrule(config, engine);
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                hyrule.Randomize(vanillaRomData, (str) => logger.Trace(str), tokenSource.Token).Wait(tokenSource.Token);
                DateTime endTime = DateTime.Now;
                Result result = new Result(hyrule);
                result.GenerationTime = (int)(endTime - startTime).TotalMilliseconds;
                dbContext.Add(result);
                dbContext.Add(hyrule.Props);
                logger.Info("Finished seed# " + i + " in: " + result.GenerationTime + "ms");
                //dbContext.SaveChanges();
            }
            dbContext.SaveChanges();
        }
        catch(Exception e) { logger.Error(e); }
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Statistics
{
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

        private static readonly string FLAGS = "AAAN6AAFeqGkWVXZt0g$o6XAv@suig$$WA"; //Standard
        private static readonly string VANILLA_ROM_PATH = "C:\\emu\\NES\\roms\\Zelda 2 - The Adventure of Link (U).nes";
        private static readonly string DB_PATH = "C:\\Workspace\\Z2Randomizer\\Statistics\\db\\stats.sqlite";
        private static readonly int LIMIT = 1;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static void Main()
        {
            StatisticsDbContext dbContext = new StatisticsDbContext(DB_PATH);

            Random random = new Random();
            logger.Info("Started statistics generation with limit: " + LIMIT);
            try
            {
                for (int i = 0; i < LIMIT; i++)
                {
                    RandomizerConfiguration config = new RandomizerConfiguration(FLAGS);
                    int seed = random.Next(1000000000);
                    config.Seed = seed;
                    config.FileName = VANILLA_ROM_PATH;
                    BackgroundWorker backgroundWorker = new BackgroundWorker()
                    {
                        WorkerReportsProgress = true,
                        WorkerSupportsCancellation = true
                    };
                    DateTime startTime = DateTime.Now;
                    Hyrule hyrule = new Hyrule(config, backgroundWorker);
                    DateTime endTime = DateTime.Now;
                    Result result = new Result(hyrule);
                    result.GenerationTime = (int)(endTime - startTime).TotalMilliseconds;
                    dbContext.Add(result);
                    dbContext.Add(hyrule.Props);
                    logger.Info("Finished seed# " + i + " in: " + result.GenerationTime + "ms");
                    //dbContext.SaveChanges();
                }
                int k = 0;
                dbContext.SaveChanges();
            }
            catch(Exception e) { logger.Error(e); }
        }
    }
}

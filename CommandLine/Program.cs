using CommandLine;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel;
using Z2Randomizer.Core;

namespace Z2Randomizer.CommandLine;

public class Program
{
    public static int Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);

    [Option(ShortName = "f", Description = "Flag string")]
    public string Flags { get; }

    [Option(ShortName = "r", Description = "Path to the base ROM file")]
    public string Rom { get; }

    [Option(ShortName = "s", Description = "[Optional] Seed used to generate the shuffled ROM")]
    public int? Seed { get; set; }

    [Option(ShortName = "po", Description = "[Optional] Specifies a player options file to use for misc settings")]
    public string PlayerOptions { get; }

    private RandomizerConfiguration? configuration;

    private int OnExecute()
    {
        if (Flags == null || Flags == string.Empty)
        {
            Console.WriteLine("The flag string is required");
            return -1;
        }

        this.configuration = new RandomizerConfiguration(Flags);

        if (!Seed.HasValue) 
        {
            var r = new Random();
            this.Seed = r.Next(1000000000);
        } 
        this.configuration.Seed = Seed.Value;

        if (Rom == null || Rom == string.Empty)
        {
            Console.WriteLine("The ROM path is required");
            return -2;
        } 
        else if (!File.Exists(Rom))
        {
            Console.WriteLine($"The specified ROM file does not exist: {Rom}");
            return -3;
        }

        this.configuration.FileName = Rom;

        Console.WriteLine($"Flags: {Flags}");
        Console.WriteLine($"Rom: {Rom}");
        Console.WriteLine($"Seed: {Seed}");

        var playerOptionsService = new PlayerOptionsService();
        var playerOptions = playerOptionsService.LoadFromFile(this.PlayerOptions);
        playerOptionsService.ApplyOptionsToConfiguration(playerOptions, configuration);

        Randomize();

        return 0;
    }

    public void Randomize()
    {
        Exception generationException = null;
        var worker = new BackgroundWorker();
        worker.DoWork += new DoWorkEventHandler(RandomizationWorker);
        worker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
        worker.WorkerReportsProgress = true;
        worker.WorkerSupportsCancellation = true;
        worker.RunWorkerCompleted += (completed_sender, completed_event) =>
        {
            generationException = completed_event.Error;
        };
        worker.RunWorkerAsync();

        while (worker.IsBusy)
        {
            Thread.Sleep(50);
        }

        if (generationException == null)
        {
            Console.WriteLine("File " + "Z2_" + this.Seed + "_" + this.Flags + ".nes" + " has been created!");
        }
        else
        {
            Console.Error.WriteLine("An exception occurred generating the rom: \n" + generationException.Message);
            Console.Error.WriteLine(generationException.StackTrace);
        }
    }

    private void RandomizationWorker(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;

        new Hyrule(this.configuration, worker, true);
        if (worker.CancellationPending)
        {
            e.Cancel = true;
        }
    }

    private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs eventArgs)
    {
        if (eventArgs.ProgressPercentage == 2)
        {
            Console.WriteLine("Generating Western Hyrule");
        }
        else if (eventArgs.ProgressPercentage == 3)
        {
            Console.WriteLine("Generating Death Mountain");
        }
        else if (eventArgs.ProgressPercentage == 4)
        {
            Console.WriteLine("Generating East Hyrule");
        }
        else if (eventArgs.ProgressPercentage == 5)
        {
            Console.WriteLine("Generating Maze Island");
        }
        else if (eventArgs.ProgressPercentage == 6)
        {
            Console.WriteLine("Shuffling Items and Spells");
        }
        else if (eventArgs.ProgressPercentage == 7)
        {
            Console.WriteLine("Running Seed Completability Checks");
        }
        else if (eventArgs.ProgressPercentage == 8)
        {
            Console.WriteLine("Generating Hints");
        }
        else if (eventArgs.ProgressPercentage == 9)
        {
            Console.WriteLine("Finishing up");
        }
    }
}
using Z2Randomizer.Core;
using System.ComponentModel;

namespace Z2Randomizer.CLI;
class CLI
{
	private static Dictionary<string, string> MiscOptions = new Dictionary<string, string>()
	{
		{"--remove-flashing-upon-death", "y" },
		{"--fast-spell-casting", "y"}
	};

	static bool IsOptionValid(string option)
	{
		if (MiscOptions.ContainsKey(option))
		{
			return true;
		}
		string valid_options = GetMiscOptions();
		string error_message = String.Format("ERROR: Unknown option '{0}'. Valid options are: \n{1}", option, valid_options);
		Console.WriteLine(error_message);
		return false; //Cannot throw exception here since it will be caught in the main function and display the more generic error message.
	}

	static string GetMiscOptions()
	{
		string misc_options = "";
		foreach (KeyValuePair<string, string> valid_option in MiscOptions)
		{
			misc_options += valid_option.Key + "\n";
		}
		return misc_options;
	}
	static bool ParseStringToBool(string str)
	{
		if (str.ToLower() == "y")
		{
			return true;
		}
		return false;
	}
	static void Main(string[] args)
	{
		Random random = new Random();
		try
		{
			if (args.Length < 3)
			{
				Console.WriteLine("ERROR: Must provide at least 3 arguments: Path to original Zelda 2 ROM, flagset and seed. Please consider running the application from a CLI to provide the arguments.");
				Console.ReadLine();
				return;
			}
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Contains("--"))
				{
					string option = args[i].Split("=")[0];
					if (!IsOptionValid(option))
					{
						return;
					}
					string value = args[i].Split("=")[1];
					MiscOptions[option] = value;
				}
			}
			string flags = args[1];
			RandomizerConfiguration config = new RandomizerConfiguration(flags);
			int seed = int.Parse(args[2]);
			config.Seed = seed;
			config.FileName = args[0];
			config.RemoveFlashing = ParseStringToBool(MiscOptions["--remove-flashing-upon-death"]);
			config.FastSpellCasting = ParseStringToBool(MiscOptions["--fast-spell-casting"]);
			BackgroundWorker backgroundWorker = new BackgroundWorker()
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			Hyrule hyrule = new Hyrule(config, backgroundWorker, true);
			Console.WriteLine("ROM generation was successful! You can find your ROM in the same directory as the original ROM. Enjoy!");
		}
		catch (Exception e)
		{
			Console.WriteLine("ERROR: There was an error during the ROM generation process. Please make sure the path to the file is valid, and that the flagset and seeds are valid according to the randomizer version. Also, if you used any options, please remember the expected format is: '--option=value', where value can be 'y' or 'n' depending on your selection, and please make sure to include them after entering the ROM path file, flagset and seed.");
			Console.WriteLine("\nValid options are:");
			string valid_options = GetMiscOptions();
			Console.WriteLine(valid_options);
			Console.WriteLine(e);
			Console.ReadLine();
		}
	}
}

using NLog;
using Z2Randomizer.Core;
using System.ComponentModel;
using System.Diagnostics;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.WinFormUI;

public partial class MainUI : Form
{
    const string DISCORD_URL = @"http://z2r.gg/discord";
    const string WIKI_URL = @"https://bitbucket.org/digshake/z2randomizer/wiki/Home";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private Random r;
    private bool dontrunhandler;
    private readonly String flags = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz1234567890!@#$";
    private bool spawnNextSeed;
    private Thread t;
    private CheckBox[] small;
    private CheckBox[] large;
    private String oldFlags;
    private GeneratingSeedsForm f3;
    private RandomizerConfiguration config;

    private readonly int validFlagStringLength;

    public MainUI()
    {
        if (Properties.Settings.Default.update)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.update = false;
            Properties.Settings.Default.Save();
        }

        validFlagStringLength = new RandomizerConfiguration().Serialize().Length;

        InitializeComponent();
        characterSpriteList.Items.Clear();
        foreach (CharacterSprite sprite in CharacterSprite.Options())
        {
            characterSpriteList.Items.Add(sprite.DisplayName);
        }
        r = new Random();
        startHeartsMinList.SelectedIndex = 3;
        startHeartsMinList.SelectedIndex = 3;
        maxHeartsList.SelectedIndex = 7;

        startingTechsList.SelectedIndex = 0;
        //allowPathEnemiesCheckbox.Enabled = false;
        romFileTextBox.Text = Properties.Settings.Default.filePath;
        tunicColorList.SelectedIndex = Properties.Settings.Default.tunic;
        shieldColorList.SelectedIndex = Properties.Settings.Default.shield;
        fastSpellCheckbox.Checked = Properties.Settings.Default.spells;
        disableLowHealthBeepCheckbox.Checked = Properties.Settings.Default.beep;
        beamSpriteList.SelectedIndex = Properties.Settings.Default.beams;
        disableMusicCheckbox.Checked = Properties.Settings.Default.music;
        upAOnController1Checkbox.Checked = Properties.Settings.Default.upac1;
        customFlags1TextBox.Text = Properties.Settings.Default.custom1;
        customFlags2TextBox.Text = Properties.Settings.Default.custom2;
        customFlags3TextBox.Text = Properties.Settings.Default.custom3;
        seedTextBox.Text = Properties.Settings.Default.lastseed;
        flashingOffCheckbox.Checked = Properties.Settings.Default.noflash;
        useCustomRoomsBox.Checked = Properties.Settings.Default.useCustomRooms;

        small = new CheckBox[] { smallEnemiesBlueJarCheckbox, smallEnemiesRedJarCheckbox, smallEnemiesSmallBagCheckbox, smallEnemiesMediumBagCheckbox,
            smallEnemiesLargeBagCheckbox, smallEnemiesXLBagCheckbox, smallEnemies1UpCheckbox, smallEnemiesKeyCheckbox };
        large = new CheckBox[] { largeEnemiesBlueJarCheckbox, largeEnemiesRedJarCheckbox, largeEnemiesSmallBagCheckbox, largeEnemiesMediumBagCheckbox,
            largeEnemiesLargeBagCheckbox, largeEnemiesXLBagCheckbox, largeEnemies1UpCheckbox, largeEnemiesKeyCheckbox };


        customFlags1TextBox.TextChanged += new System.EventHandler(this.CustomSave1_Click);
        customFlags2TextBox.TextChanged += new System.EventHandler(this.CustomSave2_Click);
        customFlags3TextBox.TextChanged += new System.EventHandler(this.CustomSave3_Click);


        dontrunhandler = false;
        mixLargeAndSmallCheckbox.Enabled = false;
        mixOverworldPalaceItemsCheckbox.Enabled = false;
        shortGPCheckbox.Enabled = false;
        includePbagCavesInShuffleCheckbox.Enabled = false;
        includeGPinShuffleCheckbox.Enabled = false;
        tbirdRequiredCheckbox.Checked = true;
        hiddenPalaceList.SelectedIndex = 0;
        hideKasutoList.SelectedIndex = 0;
        characterSpriteList.SelectedIndex = Properties.Settings.Default.sprite;

        this.Text = "Zelda 2 Randomizer Version "
            + typeof(MainUI).Assembly.GetName().Version.Major + "."
            + typeof(MainUI).Assembly.GetName().Version.Minor + "."
            + typeof(MainUI).Assembly.GetName().Version.Build;

        flagsTextBox.DoubleClick += new System.EventHandler(this.flagBox_Clicked);
        oldFlagsTextbox.DoubleClick += new System.EventHandler(this.oldFlagsTextbox_Clicked);

        shuffleStartingItemsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithCandleCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithGloveCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithRaftCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithBootsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithFluteCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithCrossCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithHammerCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithMagicKeyCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleStartingSpellsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithShieldCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithJumpCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithLifeCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithFairyCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithFireCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithReflectCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithSpellCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWIthThunderCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startHeartsMinList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startHeartsMaxList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        maxHeartsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingTechsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingGemsMinList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingGemsMaxList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeLivesBox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleEnemyHPBox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleAllExpCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleAtkExpNeededCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        lifeExpNeededCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        magicExpNeededCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleXPStealersCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleStealXPAmountCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleSwordImmunityBox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        jumpAlwaysOnCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleLifeRefillCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        disableLowHealthBeepCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        tbirdRequiredCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        experienceDropsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleEncountersCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        allowPathEnemiesCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includeLavaInShuffle.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleOverworldEnemiesCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shufflePalaceEnemiesCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        mixLargeAndSmallCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shufflePalaceItemsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleOverworldItemsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        mixOverworldPalaceItemsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleSmallItemsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleSpellLocationsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        disableMagicContainerRequirementCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        palacesHaveExtraKeysCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleDropFrequencyCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        palacePaletteCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        allowPalaceContinentSwapCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        attackEffectivenessList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        magicEffectivenessList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        lifeEffectivenessList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        restartAtPalacesCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shortGPCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeJarRequirementsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        FireSpellBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        removeTbirdCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        alwaysBeamCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includePbagCavesInShuffleCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includeGPinShuffleCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleDripperEnemyCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleEnemyPalettesCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        hiddenPalaceList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        hideKasutoList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        removeSpellitemsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        useCommunityHintsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        standardizeDropsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeDropsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shufflePbagAmountsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        atkCapList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        magCapList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        lifeCapList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        scaleLevelRequirementsToCapCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        enableTownNameHintsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        enableHelpfulHintsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        enableSpellItemHintsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        encounterRateBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingAttackLevelList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingMagicLevelList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingLifeLevelList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        continentConnectionBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        saneCaveShuffleBox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        hideLessImportantLocationsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        allowBoulderBlockedConnectionsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        westBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        dmBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        eastBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        mazeBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffledVanillaShowsActualTerrain.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleWhichLocationsAreHiddenCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeBossItemCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        useGoodBootsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeSpellSpellEnemyCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        generateBaguWoodsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        palaceStyleList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includeCommunityRoomsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        blockingRoomsInAnyPalaceCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        bossRoomsExitToPalaceCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        swapUpAndDownstabCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        dashAlwaysOnCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        noDuplicateRoomsCheckbox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        generatorsMatchCheckBox.CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        //townSwap.CheckStateChanged += new System.EventHandler(this.updateFlags);



        EnableLevelScaling(null, null);
        EastBiome_SelectedIndexChanged(null, null);
        randomizeDropsCheckbox.CheckedChanged += new System.EventHandler(this.RandomizeDropsChanged);
        for (int i = 0; i < small.Count(); i++)
        {
            small[i].CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
            small[i].CheckStateChanged += new System.EventHandler(this.AtLeastOneChecked);
            large[i].CheckStateChanged += new System.EventHandler(this.UpdateFlagsTextbox);
            large[i].CheckStateChanged += new System.EventHandler(this.AtLeastOneChecked);
        }
        String lastUsed = Properties.Settings.Default.lastused;
        if (lastUsed.Equals(""))
        {
            //updateFlags(null, null);
            BeginnerFlags(null, null);
        }
        else
        {
            dontrunhandler = true;
            flagsTextBox.Text = lastUsed;
            dontrunhandler = false;
        }


        string path = Directory.GetCurrentDirectory();
        logger.Debug(path);
        WinSparkle.win_sparkle_set_appcast_url("https://raw.githubusercontent.com/Ellendar/Z2Randomizer/main/Web/appcast.xml");
        String version = +typeof(MainUI).Assembly.GetName().Version.Major + "."
            + typeof(MainUI).Assembly.GetName().Version.Minor + "."
            + typeof(MainUI).Assembly.GetName().Version.Build;
        WinSparkle.win_sparkle_set_app_details("Z2Randomizer", "Z2Randomizer", version); // THIS CALL NOT IMPLEMENTED YET
        WinSparkle.win_sparkle_init();
    }

    /// <summary>
    /// Disabled for now to support overrides for shuffle starting items. Maybe in the future toggling this box still 
    /// changes some state for ease of use.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void shuffleItemBox_CheckStateChanged(object sender, EventArgs e)
    {
        /*
        startWithCandleCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithGloveCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithRaftCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithBootsCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithFluteCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithCrossCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithHammerCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;
        startWithMagicKeyCheckbox.Enabled = !shuffleStartingItemsCheckbox.Checked;

        if (shuffleStartingItemsCheckbox.Checked)
        {
            startWithCandleCheckbox.Checked = false;
            startWithGloveCheckbox.Checked = false;
            startWithRaftCheckbox.Checked = false;
            startWithBootsCheckbox.Checked = false;
            startWithFluteCheckbox.Checked = false;
            startWithCrossCheckbox.Checked = false;
            startWithHammerCheckbox.Checked = false;
            startWithMagicKeyCheckbox.Checked = false;
        }
        */
    }

    private void fileBtn_Click(object sender, EventArgs e)
    {
        var FD = new System.Windows.Forms.OpenFileDialog();
        if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            romFileTextBox.Text = FD.FileName;
        }
    }

    private void createSeedButton_Click(object sender, EventArgs e)
    {
        seedTextBox.Text = r.Next(1000000000).ToString();
    }

    /// <summary>
    /// Disabled for now to support overrides for shuffle starting spells. Maybe in the future toggling this box still 
    /// changes some state for ease of use.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void spellShuffleBox_CheckStateChanged(object sender, EventArgs e)
    {
        /*
        startWithShieldCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWithJumpCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWithLifeCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWithFairyCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWithFireCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWithReflectCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWithSpellCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;
        startWIthThunderCheckbox.Enabled = !shuffleStartingSpellsCheckbox.Checked;

        if (shuffleStartingSpellsCheckbox.Checked)
        {
            startWithShieldCheckbox.Checked = false;
            startWithJumpCheckbox.Checked = false;
            startWithLifeCheckbox.Checked = false;
            startWithFairyCheckbox.Checked = false;
            startWithFireCheckbox.Checked = false;
            startWithReflectCheckbox.Checked = false;
            startWithSpellCheckbox.Checked = false;
            startWIthThunderCheckbox.Checked = false;
        }
        */
    }

    private void generateBtn_Click(object sender, EventArgs e)
    {
        String flagString = flagsTextBox.Text.Trim();

        Properties.Settings.Default.filePath = romFileTextBox.Text.Trim();
        Properties.Settings.Default.beep = disableLowHealthBeepCheckbox.Checked;
        Properties.Settings.Default.beams = beamSpriteList.SelectedIndex;
        Properties.Settings.Default.spells = fastSpellCheckbox.Checked;
        Properties.Settings.Default.tunic = tunicColorList.SelectedIndex;
        Properties.Settings.Default.shield = shieldColorList.SelectedIndex;
        Properties.Settings.Default.music = disableMusicCheckbox.Checked;
        Properties.Settings.Default.sprite = characterSpriteList.SelectedIndex;
        Properties.Settings.Default.upac1 = upAOnController1Checkbox.Checked;
        Properties.Settings.Default.noflash = flashingOffCheckbox.Checked;
        Properties.Settings.Default.useCustomRooms = useCustomRoomsBox.Checked;
        Properties.Settings.Default.lastused = flagsTextBox.Text.Trim();
        Properties.Settings.Default.lastseed = seedTextBox.Text.Trim();
        Properties.Settings.Default.Save();
        try
        {
            Int32.Parse(seedTextBox.Text.Trim());
        }
        catch (Exception)
        {
            MessageBox.Show("Invalid Seed!");
            return;
        }
        config = ExportConfig();
        // f3 = new GeneratingSeedsForm();
        //f3.Show();

        //f3.setText("Generating seed. This may take a moment.");
        //f3.Width = 250;

        //t = new Thread(() => new Hyrule(props));
        //t.IsBackground = true;
        //t.Start();

        //while (t.IsAlive)
        //{
        //    Application.DoEvents();
        //    if (f3.isClosed)
        //    {
        //        t.Abort();
        //        return;
        //    }
        //}
        //new Hyrule(props, null);
        if (!Validate(flagString))
        {
            return;
        }
        f3 = new GeneratingSeedsForm();
        f3.Show();

        f3.setText("Generating Palaces");
        f3.Width = 250;
        Exception generationException = null;
        backgroundWorker1 = new BackgroundWorker();
        backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundWorker1_DoWork);
        backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
        backgroundWorker1.WorkerReportsProgress = true;
        backgroundWorker1.WorkerSupportsCancellation = true;
        backgroundWorker1.RunWorkerCompleted += (completed_sender, completed_event) =>
        {
            generationException = completed_event.Error;
        };
        backgroundWorker1.RunWorkerAsync();


        while (backgroundWorker1.IsBusy)
        {
            Application.DoEvents();
            if (f3.isClosed)
            {
                backgroundWorker1.CancelAsync();
                return;
            }
        }

        f3.Close();
        if (generationException == null)
        {
            MessageBox.Show("File " + "Z2_" + seedTextBox.Text.Trim() + "_" + flagsTextBox.Text.Trim() + ".nes" + " has been created!");
        }
        else
        {
            MessageBox.Show("An exception occurred generating the rom");
            logger.Error(generationException);
        }
    }


    private void shuffleAllExp_CheckStateChanged(object sender, EventArgs e)
    {
        shuffleAtkExpNeededCheckbox.Checked = shuffleAllExpCheckbox.Checked;
        shuffleAtkExpNeededCheckbox.Enabled = !shuffleAllExpCheckbox.Checked;

        magicExpNeededCheckbox.Checked = shuffleAllExpCheckbox.Checked;
        magicExpNeededCheckbox.Enabled = !shuffleAllExpCheckbox.Checked;

        lifeExpNeededCheckbox.Checked = shuffleAllExpCheckbox.Checked;
        lifeExpNeededCheckbox.Enabled = !shuffleAllExpCheckbox.Checked;
    }


    private void shuffleEncounters_CheckStateChanged(object sender, EventArgs e)
    {
        allowPathEnemiesCheckbox.Enabled = shuffleEncountersCheckbox.Checked;
        includeLavaInShuffle.Enabled = shuffleEncountersCheckbox.Checked;
        if (!shuffleEncountersCheckbox.Checked)
        {
            allowPathEnemiesCheckbox.Checked = false;
            includeLavaInShuffle.Checked = false;
        }
    }

    private void UpdateFlagsTextbox(object sender, EventArgs e)
    {
        if (!dontrunhandler)
        {
            RandomizerConfiguration config = ExportConfig();
            String flags = config.Serialize();
            flagsTextBox.Text = flags;
        }
    }

    private RandomizerConfiguration ExportConfig()
    {
        RandomizerConfiguration configuration = new RandomizerConfiguration();

        configuration.FileName = romFileTextBox.Text.Trim();
        try
        {
            configuration.Seed = Int32.Parse(seedTextBox.Text.Trim());
        }
        catch (FormatException)
        {
            seedTextBox.Text = r.Next(1000000000).ToString();
            configuration.Seed = Int32.Parse(seedTextBox.Text.Trim());
        }

        //Start Configuration
        configuration.ShuffleStartingItems = shuffleStartingItemsCheckbox.Checked;
        configuration.StartWithCandle = startWithCandleCheckbox.Checked;
        configuration.StartWithGlove = startWithGloveCheckbox.Checked;
        configuration.StartWithRaft = startWithRaftCheckbox.Checked;
        configuration.StartWithBoots = startWithBootsCheckbox.Checked;
        configuration.StartWithFlute = startWithFluteCheckbox.Checked;
        configuration.StartWithCross = startWithCrossCheckbox.Checked;
        configuration.StartWithHammer = startWithHammerCheckbox.Checked;
        configuration.StartWithMagicKey = startWithMagicKeyCheckbox.Checked;

        configuration.ShuffleStartingSpells = shuffleStartingSpellsCheckbox.Checked;
        configuration.StartWithShield = startWithShieldCheckbox.Checked;
        configuration.StartWithJump = startWithJumpCheckbox.Checked;
        configuration.StartWithLife = startWithLifeCheckbox.Checked;
        configuration.StartWithFairy = startWithFairyCheckbox.Checked;
        configuration.StartWithFire = startWithFireCheckbox.Checked;
        configuration.StartWithReflect = startWithReflectCheckbox.Checked;
        configuration.StartWithSpell = startWithSpellCheckbox.Checked;
        configuration.StartWithThunder = startWIthThunderCheckbox.Checked;

        configuration.StartingHeartContainersMin = startHeartsMinList.SelectedIndex switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            3 => 4,
            4 => 5,
            5 => 6,
            6 => 7,
            7 => 8,
            8 => null,
            _ => throw new Exception("Invalid StartHeartsMin setting")
        };
        configuration.StartingHeartContainersMax = startHeartsMaxList.SelectedIndex switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            3 => 4,
            4 => 5,
            5 => 6,
            6 => 7,
            7 => 8,
            8 => null,
            _ => throw new Exception("Invalid StartHeartsMax setting")
        };
        configuration.MaxHeartContainers = maxHeartsList.SelectedIndex switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            3 => 4,
            4 => 5,
            5 => 6,
            6 => 7,
            7 => 8,
            8 => null,
            9 => 9,
            10 => 10,
            11 => 11,
            _ => throw new Exception("Invalid StartHeartsMax setting")
        };

        switch (startingTechsList.SelectedIndex)
        {
            case 0:
                configuration.StartWithDownstab = false;
                configuration.StartWithUpstab = false;
                break;
            case 1:
                configuration.StartWithDownstab = true;
                configuration.StartWithUpstab = false;
                break;
            case 2:
                configuration.StartWithDownstab = false;
                configuration.StartWithUpstab = true;
                break;
            case 3:
                configuration.StartWithDownstab = true;
                configuration.StartWithUpstab = true;
                break;
            case 4:
                configuration.StartWithDownstab = null;
                configuration.StartWithUpstab = null;
                break;
            default:
                throw new Exception("Invalid Techs setting");
        }
        configuration.ShuffleStartingLives = randomizeLivesBox.Checked;
        configuration.StartingAttackLevel = startingAttackLevelList.SelectedIndex + 1;
        configuration.StartingMagicLevel = startingMagicLevelList.SelectedIndex + 1;
        configuration.StartingLifeLevel = startingLifeLevelList.SelectedIndex + 1;

        //Overworld
        configuration.PalacesCanSwapContinents = GetTripleCheckState(allowPalaceContinentSwapCheckbox);
        configuration.ShuffleGP = GetTripleCheckState(includeGPinShuffleCheckbox);
        configuration.ShuffleEncounters = GetTripleCheckState(shuffleEncountersCheckbox);
        configuration.AllowUnsafePathEncounters = allowPathEnemiesCheckbox.Checked;
        configuration.IncludeLavaInEncounterShuffle = includeLavaInShuffle.Checked;
        configuration.EncounterRate = encounterRateBox.SelectedIndex switch
        {
            0 => EncounterRate.NORMAL,
            1 => EncounterRate.HALF,
            2 => EncounterRate.NONE,
            3 => EncounterRate.RANDOM,
            _ => throw new Exception("Invalid EncounterRate setting")
        };
        configuration.HidePalace = hiddenPalaceList.SelectedIndex switch
        {
            0 => false,
            1 => true,
            2 => null,
            _ => throw new Exception("Invalid HidePalace setting")
        };
        configuration.HideKasuto = hideKasutoList.SelectedIndex switch
        {
            0 => false,
            1 => true,
            2 => null,
            _ => throw new Exception("Invalid HideKasuto setting")
        };
        configuration.ShuffleWhichLocationIsHidden = GetTripleCheckState(shuffleWhichLocationsAreHiddenCheckbox);
        configuration.HideLessImportantLocations = GetTripleCheckState(hideLessImportantLocationsCheckbox);
        configuration.RestrictConnectionCaveShuffle = GetTripleCheckState(saneCaveShuffleBox);
        configuration.AllowConnectionCavesToBeBoulderBlocked = allowBoulderBlockedConnectionsCheckbox.Checked;
        configuration.GoodBoots = GetTripleCheckState(useGoodBootsCheckbox);
        configuration.GenerateBaguWoods = GetTripleCheckState(generateBaguWoodsCheckbox);
        configuration.ContinentConnectionType = continentConnectionBox.SelectedIndex switch
        {
            0 => ContinentConnectionType.NORMAL,
            1 => ContinentConnectionType.RB_BORDER_SHUFFLE,
            2 => ContinentConnectionType.TRANSPORTATION_SHUFFLE,
            3 => ContinentConnectionType.ANYTHING_GOES,
            _ => throw new Exception("Invalid ContinentConnection setting")
        };
        configuration.WestBiome = westBiome.SelectedIndex switch
        {
            0 => Biome.VANILLA,
            1 => Biome.VANILLA_SHUFFLE,
            2 => Biome.VANILLALIKE,
            3 => Biome.ISLANDS,
            4 => Biome.CANYON,
            5 => Biome.CALDERA,
            6 => Biome.MOUNTAINOUS,
            7 => Biome.RANDOM_NO_VANILLA,
            8 => Biome.RANDOM,
            _ => throw new Exception("Invalid WestBiome setting")
        };
        configuration.EastBiome = eastBiome.SelectedIndex switch
        {
            0 => Biome.VANILLA,
            1 => Biome.VANILLA_SHUFFLE,
            2 => Biome.VANILLALIKE,
            3 => Biome.ISLANDS,
            4 => Biome.CANYON,
            5 => Biome.VOLCANO,
            6 => Biome.MOUNTAINOUS,
            7 => Biome.RANDOM_NO_VANILLA,
            8 => Biome.RANDOM,
            _ => throw new Exception("Invalid EastBiome setting")
        };
        configuration.DMBiome = dmBiome.SelectedIndex switch
        {
            0 => Biome.VANILLA,
            1 => Biome.VANILLA_SHUFFLE,
            2 => Biome.VANILLALIKE,
            3 => Biome.ISLANDS,
            4 => Biome.CANYON,
            5 => Biome.CALDERA,
            6 => Biome.MOUNTAINOUS,
            7 => Biome.RANDOM_NO_VANILLA,
            8 => Biome.RANDOM,
            _ => throw new Exception("Invalid DMBiome setting")
        };
        configuration.MazeBiome = mazeBiome.SelectedIndex switch
        {
            0 => Biome.VANILLA,
            1 => Biome.VANILLA_SHUFFLE,
            2 => Biome.VANILLALIKE,
            3 => Biome.RANDOM,
            _ => throw new Exception("Invalid MazeBiome setting")
        };
        configuration.VanillaShuffleUsesActualTerrain = shuffledVanillaShowsActualTerrain.Checked;

        //Palaces
        configuration.PalaceStyle = palaceStyleList.SelectedIndex switch
        {
            0 => PalaceStyle.VANILLA,
            1 => PalaceStyle.SHUFFLED,
            2 => PalaceStyle.RECONSTRUCTED,
            3 => PalaceStyle.RANDOM,
            _ => throw new Exception("Invalid PalaceStyle setting")
        };
        configuration.IncludeCommunityRooms = GetTripleCheckState(includeCommunityRoomsCheckbox);
        configuration.BlockingRoomsInAnyPalace = blockingRoomsInAnyPalaceCheckbox.Checked;
        configuration.BossRoomsExitToPalace = GetTripleCheckState(bossRoomsExitToPalaceCheckbox);
        configuration.ShortGP = GetTripleCheckState(shortGPCheckbox);
        configuration.TBirdRequired = GetTripleCheckState(tbirdRequiredCheckbox);
        configuration.RemoveTBird = removeTbirdCheckbox.Checked;
        configuration.RestartAtPalacesOnGameOver = restartAtPalacesCheckbox.Checked;
        configuration.ChangePalacePallettes = palacePaletteCheckbox.Checked;
        configuration.RandomizeBossItemDrop = randomizeBossItemCheckbox.Checked;
        configuration.PalacesToCompleteMin = startingGemsMinList.SelectedIndex;
        configuration.PalacesToCompleteMax = startingGemsMaxList.SelectedIndex;
        configuration.NoDuplicateRooms = noDuplicateRoomsCheckbox.Checked;

        //Levels
        configuration.ShuffleAttackExperience = shuffleAtkExpNeededCheckbox.Checked;
        configuration.ShuffleMagicExperience = magicExpNeededCheckbox.Checked;
        configuration.ShuffleLifeExperience = lifeExpNeededCheckbox.Checked;
        if (shuffleAllExpCheckbox.Checked)
        {
            configuration.ShuffleAttackExperience = true;
            configuration.ShuffleMagicExperience = true;
            configuration.ShuffleLifeExperience = true;
        }
        configuration.AttackLevelCap = 8 - atkCapList.SelectedIndex;
        configuration.MagicLevelCap = 8 - magCapList.SelectedIndex;
        configuration.LifeLevelCap = 8 - lifeCapList.SelectedIndex;
        configuration.ScaleLevelRequirementsToCap = scaleLevelRequirementsToCapCheckbox.Checked;
        configuration.AttackEffectiveness = attackEffectivenessList.SelectedIndex switch
        {
            0 => StatEffectiveness.AVERAGE,
            1 => StatEffectiveness.LOW,
            2 => StatEffectiveness.VANILLA,
            3 => StatEffectiveness.HIGH,
            4 => StatEffectiveness.MAX,
            _ => throw new Exception("Invalid AttackEffectiveness setting")
        };
        configuration.MagicEffectiveness = magicEffectivenessList.SelectedIndex switch
        {
            0 => StatEffectiveness.AVERAGE,
            1 => StatEffectiveness.LOW,
            2 => StatEffectiveness.VANILLA,
            3 => StatEffectiveness.HIGH,
            4 => StatEffectiveness.MAX,
            _ => throw new Exception("Invalid MagicEffectiveness setting")
        };
        configuration.LifeEffectiveness = lifeEffectivenessList.SelectedIndex switch
        {
            0 => StatEffectiveness.AVERAGE,
            1 => StatEffectiveness.NONE,
            2 => StatEffectiveness.VANILLA,
            3 => StatEffectiveness.HIGH,
            4 => StatEffectiveness.MAX,
            _ => throw new Exception("Invalid LifeEffectiveness setting")
        };

        //Spells
        configuration.ShuffleLifeRefillAmount = shuffleLifeRefillCheckbox.Checked;
        configuration.ShuffleSpellLocations = GetTripleCheckState(shuffleSpellLocationsCheckbox);
        configuration.DisableMagicContainerRequirements = GetTripleCheckState(disableMagicContainerRequirementCheckbox);
        configuration.RandomizeSpellSpellEnemy = GetTripleCheckState(randomizeSpellSpellEnemyCheckbox);
        configuration.SwapUpAndDownStab = GetTripleCheckState(swapUpAndDownstabCheckbox);
        configuration.FireOption = FireSpellBox.SelectedIndex switch
        {
            0 => FireOption.NORMAL,
            1 => FireOption.PAIR_WITH_RANDOM,
            2 => FireOption.REPLACE_WITH_DASH,
            3 => FireOption.RANDOM,
            _ => throw new Exception("Unrecognized FireOption selection state")
        };

        //Enemies
        configuration.ShuffleOverworldEnemies = GetTripleCheckState(shuffleOverworldEnemiesCheckbox);
        configuration.ShufflePalaceEnemies = GetTripleCheckState(shufflePalaceEnemiesCheckbox);
        configuration.ShuffleDripperEnemy = shuffleDripperEnemyCheckbox.Checked;
        configuration.MixLargeAndSmallEnemies = GetTripleCheckState(mixLargeAndSmallCheckbox);
        configuration.GeneratorsAlwaysMatch = generatorsMatchCheckBox.Checked;
        configuration.ShuffleEnemyHP = shuffleEnemyHPBox.Checked;
        configuration.ShuffleXPStealers = shuffleXPStealersCheckbox.Checked;
        configuration.ShuffleXPStolenAmount = shuffleStealXPAmountCheckbox.Checked;
        configuration.ShuffleSwordImmunity = shuffleSwordImmunityBox.Checked;
        configuration.EnemyXPDrops = experienceDropsList.SelectedIndex switch
        {
            0 => StatEffectiveness.VANILLA,
            1 => StatEffectiveness.NONE,
            2 => StatEffectiveness.LOW,
            3 => StatEffectiveness.AVERAGE,
            4 => StatEffectiveness.HIGH,
            _ => throw new Exception("Invalid EnemyXPDrops setting")
        };

        //Items
        configuration.ShufflePalaceItems = GetTripleCheckState(shufflePalaceItemsCheckbox);
        configuration.ShuffleOverworldItems = GetTripleCheckState(shuffleOverworldItemsCheckbox);
        configuration.MixOverworldAndPalaceItems = GetTripleCheckState(mixOverworldPalaceItemsCheckbox);
        configuration.IncludePBagCavesInItemShuffle = GetTripleCheckState(includePbagCavesInShuffleCheckbox);
        configuration.ShuffleSmallItems = shuffleSmallItemsCheckbox.Checked;
        configuration.PalacesContainExtraKeys = GetTripleCheckState(palacesHaveExtraKeysCheckbox);
        configuration.RandomizeNewKasutoJarRequirements = randomizeJarRequirementsCheckbox.Checked;
        configuration.RemoveSpellItems = GetTripleCheckState(removeSpellitemsCheckbox);
        configuration.ShufflePBagAmounts = GetTripleCheckState(shufflePbagAmountsCheckbox);

        //Drops
        configuration.ShuffleItemDropFrequency = shuffleDropFrequencyCheckbox.Checked;
        configuration.RandomizeDrops = randomizeDropsCheckbox.Checked;
        configuration.StandardizeDrops = standardizeDropsCheckbox.Checked;
        configuration.SmallEnemiesCanDropBlueJar = smallEnemiesBlueJarCheckbox.Checked;
        configuration.SmallEnemiesCanDropRedJar = smallEnemiesRedJarCheckbox.Checked;
        configuration.SmallEnemiesCanDropSmallBag = smallEnemiesSmallBagCheckbox.Checked;
        configuration.SmallEnemiesCanDropMediumBag = smallEnemiesMediumBagCheckbox.Checked;
        configuration.SmallEnemiesCanDropLargeBag = smallEnemiesLargeBagCheckbox.Checked;
        configuration.SmallEnemiesCanDropXLBag = smallEnemiesXLBagCheckbox.Checked;
        configuration.SmallEnemiesCanDrop1up = smallEnemies1UpCheckbox.Checked;
        configuration.SmallEnemiesCanDropKey = smallEnemiesKeyCheckbox.Checked;
        configuration.LargeEnemiesCanDropBlueJar = largeEnemiesBlueJarCheckbox.Checked;
        configuration.LargeEnemiesCanDropRedJar = largeEnemiesRedJarCheckbox.Checked;
        configuration.LargeEnemiesCanDropSmallBag = largeEnemiesSmallBagCheckbox.Checked;
        configuration.LargeEnemiesCanDropMediumBag = largeEnemiesMediumBagCheckbox.Checked;
        configuration.LargeEnemiesCanDropLargeBag = largeEnemiesLargeBagCheckbox.Checked;
        configuration.LargeEnemiesCanDropXLBag = largeEnemiesXLBagCheckbox.Checked;
        configuration.LargeEnemiesCanDrop1up = largeEnemies1UpCheckbox.Checked;
        configuration.LargeEnemiesCanDropKey = largeEnemiesKeyCheckbox.Checked;


        //Hints
        configuration.EnableHelpfulHints = GetTripleCheckState(enableHelpfulHintsCheckbox);
        configuration.EnableSpellItemHints = GetTripleCheckState(enableSpellItemHintsCheckbox);
        configuration.EnableTownNameHints = GetTripleCheckState(enableTownNameHintsCheckbox);
        configuration.UseCommunityHints = useCommunityHintsCheckbox.Checked;

        //Misc
        configuration.DisableLowHealthBeep = disableLowHealthBeepCheckbox.Checked;
        configuration.DisableMusic = disableMusicCheckbox.Checked;
        configuration.JumpAlwaysOn = jumpAlwaysOnCheckbox.Checked;
        configuration.DashAlwaysOn = dashAlwaysOnCheckbox.Checked;
        configuration.FastSpellCasting = fastSpellCheckbox.Checked;
        configuration.ShuffleSpritePalettes = shuffleEnemyPalettesCheckbox.Checked;
        configuration.PermanmentBeamSword = alwaysBeamCheckbox.Checked;
        configuration.UpAOnController1 = upAOnController1Checkbox.Checked;
        configuration.RemoveFlashing = flashingOffCheckbox.Checked;
        configuration.UseCustomRooms = useCustomRoomsBox.Checked;
        configuration.Sprite = characterSpriteList.SelectedIndex;

        configuration.Tunic = tunicColorList.GetItemText(tunicColorList.SelectedItem);
        configuration.ShieldTunic = shieldColorList.GetItemText(shieldColorList.SelectedItem);
        configuration.BeamSprite = beamSpriteList.GetItemText(beamSpriteList.SelectedItem);

        return configuration;
    }

    private void flagBox_Clicked(object send, EventArgs e)
    {
        flagsTextBox.SelectAll();
    }
    private void oldFlagsTextbox_Clicked(object send, EventArgs e)
    {
        oldFlagsTextbox.SelectAll();
    }

    private void convertButton_Click(object send, EventArgs e)
    {
        String oldFlags = oldFlagsTextbox.Text.Trim();
        RandomizerConfiguration oldSettings = ExportConfig();
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags(oldFlags);

        config.DisableLowHealthBeep = oldSettings.DisableLowHealthBeep;
        config.DisableMusic = oldSettings.DisableMusic;
        config.FastSpellCasting = oldSettings.FastSpellCasting;
        config.ShuffleSpritePalettes = oldSettings.ShuffleSpritePalettes;
        config.UpAOnController1 = oldSettings.UpAOnController1;
        config.RemoveFlashing = oldSettings.RemoveFlashing;
        config.Sprite = oldSettings.Sprite;
        config.Tunic = oldSettings.Tunic;
        config.ShieldTunic = oldSettings.ShieldTunic;
        config.BeamSprite = oldSettings.BeamSprite;
        flagsTextBox.Text = config.Serialize();
    }

    private void FlagBox_TextChanged(object sender, EventArgs eventArgs)
    {
        dontrunhandler = true;
        flagsTextBox.Text = flagsTextBox.Text.Trim();
        try
        {
            RandomizerConfiguration configuration = new RandomizerConfiguration(flagsTextBox.Text);

            //Start Configuration
            shuffleStartingItemsCheckbox.Checked = configuration.ShuffleStartingItems;
            startWithCandleCheckbox.Checked = configuration.StartWithCandle;
            startWithGloveCheckbox.Checked = configuration.StartWithGlove;
            startWithRaftCheckbox.Checked = configuration.StartWithRaft;
            startWithBootsCheckbox.Checked = configuration.StartWithBoots;
            startWithFluteCheckbox.Checked = configuration.StartWithFlute;
            startWithCrossCheckbox.Checked = configuration.StartWithCross;
            startWithHammerCheckbox.Checked = configuration.StartWithHammer;
            startWithMagicKeyCheckbox.Checked = configuration.StartWithMagicKey;

            shuffleStartingSpellsCheckbox.Checked = configuration.ShuffleStartingSpells;
            startWithShieldCheckbox.Checked = configuration.StartWithShield;
            startWithJumpCheckbox.Checked = configuration.StartWithJump;
            startWithLifeCheckbox.Checked = configuration.StartWithLife;
            startWithFairyCheckbox.Checked = configuration.StartWithFairy;
            startWithFireCheckbox.Checked = configuration.StartWithFire;
            startWithReflectCheckbox.Checked = configuration.StartWithReflect;
            startWithSpellCheckbox.Checked = configuration.StartWithSpell;
            startWIthThunderCheckbox.Checked = configuration.StartWithThunder;

            startHeartsMinList.SelectedIndex = configuration.StartingHeartContainersMin switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                5 => 4,
                6 => 5,
                7 => 6,
                8 => 7,
                null => 8,
                _ => throw new Exception("Invalid StartHeartsMin setting")
            };
            startHeartsMaxList.SelectedIndex = configuration.StartingHeartContainersMax switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                5 => 4,
                6 => 5,
                7 => 6,
                8 => 7,
                null => 8,
                _ => throw new Exception("Invalid StartHeartsMin setting")
            };
            maxHeartsList.SelectedIndex = configuration.MaxHeartContainers switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                5 => 4,
                6 => 5,
                7 => 6,
                8 => 7,
                null => 8,
                9 => 9,
                10 => 10,
                11 => 11,
                _ => throw new Exception("Invalid MaxHearts setting")
            };
            startingTechsList.SelectedIndex = (configuration.StartWithDownstab, configuration.StartWithUpstab) switch
            {
                (false, false) => 0,
                (true, false) => 1,
                (false, true) => 2,
                (true, true) => 3,
                (_, _) => 4
            };
            randomizeLivesBox.Checked = configuration.ShuffleStartingLives;
            startingAttackLevelList.SelectedIndex = configuration.StartingAttackLevel - 1;
            startingMagicLevelList.SelectedIndex = configuration.StartingMagicLevel - 1;
            startingLifeLevelList.SelectedIndex = configuration.StartingLifeLevel - 1;

            //Overworld
            allowPalaceContinentSwapCheckbox.CheckState = ToCheckState(configuration.PalacesCanSwapContinents);
            includeGPinShuffleCheckbox.CheckState = ToCheckState(configuration.ShuffleGP);
            shuffleEncountersCheckbox.CheckState = ToCheckState(configuration.ShuffleEncounters);
            allowPathEnemiesCheckbox.CheckState = ToCheckState(configuration.AllowUnsafePathEncounters);
            includeLavaInShuffle.CheckState = ToCheckState(configuration.IncludeLavaInEncounterShuffle);
            encounterRateBox.SelectedIndex = configuration.EncounterRate switch
            {
                EncounterRate.NORMAL => 0,
                EncounterRate.HALF => 1,
                EncounterRate.NONE => 2,
                _ => throw new Exception("Invalid EncounterRate setting")
            };
            hiddenPalaceList.SelectedIndex = configuration.HidePalace switch
            {
                false => 0,
                true => 1,
                null => 2,
            };
            hideKasutoList.SelectedIndex = configuration.HideKasuto switch
            {
                false => 0,
                true => 1,
                null => 2,
            };
            shuffleWhichLocationsAreHiddenCheckbox.CheckState = ToCheckState(configuration.ShuffleWhichLocationIsHidden);
            hideLessImportantLocationsCheckbox.CheckState = ToCheckState(configuration.HideLessImportantLocations);
            saneCaveShuffleBox.CheckState = ToCheckState(configuration.RestrictConnectionCaveShuffle);
            allowBoulderBlockedConnectionsCheckbox.Checked = configuration.AllowConnectionCavesToBeBoulderBlocked;
            useGoodBootsCheckbox.CheckState = ToCheckState(configuration.GoodBoots);
            generateBaguWoodsCheckbox.CheckState = ToCheckState(configuration.GenerateBaguWoods);
            continentConnectionBox.SelectedIndex = configuration.ContinentConnectionType switch
            {
                ContinentConnectionType.NORMAL => 0,
                ContinentConnectionType.RB_BORDER_SHUFFLE => 1,
                ContinentConnectionType.TRANSPORTATION_SHUFFLE => 2,
                ContinentConnectionType.ANYTHING_GOES => 3,
                _ => throw new Exception("Invalid ContinentConnection setting")
            };
            westBiome.SelectedIndex = configuration.WestBiome switch
            {
                Biome.VANILLA => 0,
                Biome.VANILLA_SHUFFLE => 1,
                Biome.VANILLALIKE => 2,
                Biome.ISLANDS => 3,
                Biome.CANYON => 4,
                Biome.CALDERA => 5,
                Biome.MOUNTAINOUS => 6,
                Biome.RANDOM_NO_VANILLA => 7,
                Biome.RANDOM => 8,
                _ => throw new Exception("Invalid WestBiome setting")
            };
            eastBiome.SelectedIndex = configuration.EastBiome switch
            {
                Biome.VANILLA => 0,
                Biome.VANILLA_SHUFFLE => 1,
                Biome.VANILLALIKE => 2,
                Biome.ISLANDS => 3,
                Biome.CANYON => 4,
                Biome.VOLCANO => 5,
                Biome.MOUNTAINOUS => 6,
                Biome.RANDOM_NO_VANILLA => 7,
                Biome.RANDOM => 8,
                _ => throw new Exception("Invalid EastBiome setting")
            };
            dmBiome.SelectedIndex = configuration.DMBiome switch
            {
                Biome.VANILLA => 0,
                Biome.VANILLA_SHUFFLE => 1,
                Biome.VANILLALIKE => 2,
                Biome.ISLANDS => 3,
                Biome.CANYON => 4,
                Biome.CALDERA => 5,
                Biome.MOUNTAINOUS => 6,
                Biome.RANDOM_NO_VANILLA => 7,
                Biome.RANDOM => 8,
                _ => throw new Exception("Invalid DMBiome setting")
            };
            mazeBiome.SelectedIndex = configuration.MazeBiome switch
            {
                Biome.VANILLA => 0,
                Biome.VANILLA_SHUFFLE => 1,
                Biome.VANILLALIKE => 2,
                Biome.RANDOM => 3,
                _ => throw new Exception("Invalid MazeBiome setting")
            };
            shuffledVanillaShowsActualTerrain.Checked = configuration.VanillaShuffleUsesActualTerrain;

            //Palaces
            palaceStyleList.SelectedIndex = configuration.PalaceStyle switch
            {
                PalaceStyle.VANILLA => 0,
                PalaceStyle.SHUFFLED => 1,
                PalaceStyle.RECONSTRUCTED => 2,
                PalaceStyle.RANDOM => 3,
                _ => throw new Exception("Invalid PalaceStyle setting")
            };
            includeCommunityRoomsCheckbox.CheckState = ToCheckState(configuration.IncludeCommunityRooms);
            blockingRoomsInAnyPalaceCheckbox.Checked = configuration.BlockingRoomsInAnyPalace;
            bossRoomsExitToPalaceCheckbox.CheckState = ToCheckState(configuration.BossRoomsExitToPalace);
            shortGPCheckbox.CheckState = ToCheckState(configuration.ShortGP);
            tbirdRequiredCheckbox.CheckState = ToCheckState(configuration.TBirdRequired);
            removeTbirdCheckbox.Checked = configuration.RemoveTBird;
            restartAtPalacesCheckbox.Checked = configuration.RestartAtPalacesOnGameOver;
            palacePaletteCheckbox.Checked = configuration.ChangePalacePallettes;
            randomizeBossItemCheckbox.Checked = configuration.RandomizeBossItemDrop;
            startingGemsMinList.SelectedIndex = configuration.PalacesToCompleteMin;
            startingGemsMaxList.SelectedIndex = configuration.PalacesToCompleteMax;
            noDuplicateRoomsCheckbox.CheckState = ToCheckState(configuration.NoDuplicateRooms);

            //Levels
            shuffleAtkExpNeededCheckbox.Checked = configuration.ShuffleAttackExperience;
            magicExpNeededCheckbox.Checked = configuration.ShuffleMagicExperience;
            lifeExpNeededCheckbox.Checked = configuration.ShuffleLifeExperience;
            if (configuration.ShuffleAttackExperience && configuration.ShuffleMagicExperience && configuration.ShuffleLifeExperience)
            {
                shuffleAllExpCheckbox.Checked = true;
            }

            /*
            configuration.AttackLevelCap = 8 - atkCapList.SelectedIndex;
            configuration.MagicLevelCap = 8 - magCapList.SelectedIndex;
            configuration.LifeLevelCap = 8 - lifeCapList.SelectedIndex;
            */
            atkCapList.SelectedIndex = 8 - configuration.AttackLevelCap;
            magCapList.SelectedIndex = 8 - configuration.MagicLevelCap;
            lifeCapList.SelectedIndex = 8 - configuration.LifeLevelCap;
            scaleLevelRequirementsToCapCheckbox.Checked = configuration.ScaleLevelRequirementsToCap;
            attackEffectivenessList.SelectedIndex = configuration.AttackEffectiveness switch
            {
                StatEffectiveness.AVERAGE => 0,
                StatEffectiveness.LOW => 1,
                StatEffectiveness.VANILLA => 2,
                StatEffectiveness.HIGH => 3,
                StatEffectiveness.MAX => 4,
                _ => throw new Exception("Invalid AttackEffectiveness setting")
            };
            magicEffectivenessList.SelectedIndex = configuration.MagicEffectiveness switch
            {
                StatEffectiveness.AVERAGE => 0,
                StatEffectiveness.LOW => 1,
                StatEffectiveness.VANILLA => 2,
                StatEffectiveness.HIGH => 3,
                StatEffectiveness.MAX => 4,
                _ => throw new Exception("Invalid MagicEffectiveness setting")
            };
            lifeEffectivenessList.SelectedIndex = configuration.LifeEffectiveness switch
            {
                StatEffectiveness.AVERAGE => 0,
                StatEffectiveness.NONE => 1,
                StatEffectiveness.VANILLA => 2,
                StatEffectiveness.HIGH => 3,
                StatEffectiveness.MAX => 4,
                _ => throw new Exception("Invalid LifeEffectiveness setting")
            };

            //Spells
            shuffleLifeRefillCheckbox.Checked = configuration.ShuffleLifeRefillAmount;
            shuffleSpellLocationsCheckbox.CheckState = ToCheckState(configuration.ShuffleSpellLocations);
            disableMagicContainerRequirementCheckbox.CheckState = ToCheckState(configuration.DisableMagicContainerRequirements);
            randomizeSpellSpellEnemyCheckbox.CheckState = ToCheckState(configuration.RandomizeSpellSpellEnemy);
            swapUpAndDownstabCheckbox.CheckState = ToCheckState(configuration.SwapUpAndDownStab);
            FireSpellBox.SelectedIndex = configuration.FireOption switch
            {
                FireOption.NORMAL => 0,
                FireOption.PAIR_WITH_RANDOM => 1,
                FireOption.REPLACE_WITH_DASH => 2,
                FireOption.RANDOM => 3,
                _ => throw new Exception("Unrecognized FireOption")
            };

            //Enemies
            shuffleOverworldEnemiesCheckbox.CheckState = ToCheckState(configuration.ShuffleOverworldEnemies);
            shufflePalaceEnemiesCheckbox.CheckState = ToCheckState(configuration.ShufflePalaceEnemies);
            shuffleDripperEnemyCheckbox.Checked = configuration.ShuffleDripperEnemy;
            mixLargeAndSmallCheckbox.CheckState = ToCheckState(configuration.MixLargeAndSmallEnemies);
            generatorsMatchCheckBox.Checked = configuration.GeneratorsAlwaysMatch;
            shuffleEnemyHPBox.Checked = configuration.ShuffleEnemyHP;
            shuffleXPStealersCheckbox.Checked = configuration.ShuffleXPStealers;
            shuffleStealXPAmountCheckbox.Checked = configuration.ShuffleXPStolenAmount;
            shuffleSwordImmunityBox.Checked = configuration.ShuffleSwordImmunity;
            experienceDropsList.SelectedIndex = configuration.EnemyXPDrops switch
            {
                StatEffectiveness.VANILLA => 0,
                StatEffectiveness.NONE => 1,
                StatEffectiveness.LOW => 2,
                StatEffectiveness.AVERAGE => 3,
                StatEffectiveness.HIGH => 4,
                _ => throw new Exception("Invalid EnemyXPDrops setting")
            };

            //Items
            shufflePalaceItemsCheckbox.CheckState = ToCheckState(configuration.ShufflePalaceItems);
            shuffleOverworldItemsCheckbox.CheckState = ToCheckState(configuration.ShuffleOverworldItems);
            mixOverworldPalaceItemsCheckbox.CheckState = ToCheckState(configuration.MixOverworldAndPalaceItems);
            includePbagCavesInShuffleCheckbox.CheckState = ToCheckState(configuration.IncludePBagCavesInItemShuffle);
            shuffleSmallItemsCheckbox.Checked = configuration.ShuffleSmallItems;
            palacesHaveExtraKeysCheckbox.CheckState = ToCheckState(configuration.PalacesContainExtraKeys);
            randomizeJarRequirementsCheckbox.Checked = configuration.RandomizeNewKasutoJarRequirements;
            removeSpellitemsCheckbox.CheckState = ToCheckState(configuration.RemoveSpellItems);
            shufflePbagAmountsCheckbox.CheckState = ToCheckState(configuration.ShufflePBagAmounts);

            //Drops
            shuffleDropFrequencyCheckbox.Checked = configuration.ShuffleItemDropFrequency;
            standardizeDropsCheckbox.Checked = configuration.StandardizeDrops;
            randomizeDropsCheckbox.Checked = configuration.RandomizeDrops;
            smallEnemiesBlueJarCheckbox.Checked = configuration.SmallEnemiesCanDropBlueJar;
            smallEnemiesRedJarCheckbox.Checked = configuration.SmallEnemiesCanDropRedJar;
            smallEnemiesSmallBagCheckbox.Checked = configuration.SmallEnemiesCanDropSmallBag;
            smallEnemiesMediumBagCheckbox.Checked = configuration.SmallEnemiesCanDropMediumBag;
            smallEnemiesLargeBagCheckbox.Checked = configuration.SmallEnemiesCanDropLargeBag;
            smallEnemiesXLBagCheckbox.Checked = configuration.SmallEnemiesCanDropXLBag;
            smallEnemies1UpCheckbox.Checked = configuration.SmallEnemiesCanDrop1up;
            smallEnemiesKeyCheckbox.Checked = configuration.SmallEnemiesCanDropKey;
            largeEnemiesBlueJarCheckbox.Checked = configuration.LargeEnemiesCanDropBlueJar;
            largeEnemiesRedJarCheckbox.Checked = configuration.LargeEnemiesCanDropRedJar;
            largeEnemiesSmallBagCheckbox.Checked = configuration.LargeEnemiesCanDropSmallBag;
            largeEnemiesMediumBagCheckbox.Checked = configuration.LargeEnemiesCanDropMediumBag;
            largeEnemiesLargeBagCheckbox.Checked = configuration.LargeEnemiesCanDropLargeBag;
            largeEnemiesXLBagCheckbox.Checked = configuration.LargeEnemiesCanDropXLBag;
            largeEnemies1UpCheckbox.Checked = configuration.LargeEnemiesCanDrop1up;
            largeEnemiesKeyCheckbox.Checked = configuration.LargeEnemiesCanDropKey;


            //Hints
            enableHelpfulHintsCheckbox.CheckState = ToCheckState(configuration.EnableHelpfulHints);
            enableSpellItemHintsCheckbox.CheckState = ToCheckState(configuration.EnableSpellItemHints);
            enableTownNameHintsCheckbox.CheckState = ToCheckState(configuration.EnableTownNameHints);
            useCommunityHintsCheckbox.Checked = configuration.UseCommunityHints;

            //Misc
            //disableLowHealthBeepCheckbox.Checked = configuration.DisableLowHealthBeep;
            //disableMusicCheckbox.Checked = configuration.DisableMusic;
            jumpAlwaysOnCheckbox.Checked = configuration.JumpAlwaysOn;
            dashAlwaysOnCheckbox.Checked = configuration.DashAlwaysOn;
            //fastSpellCheckbox.Checked = configuration.FastSpellCasting;
            //shuffleEnemyPalettesCheckbox.Checked = configuration.ShuffleSpritePalettes;
            alwaysBeamCheckbox.Checked = configuration.PermanmentBeamSword;
            //upAOnController1Checkbox.Checked = configuration.UpAOnController1;
            //flashingOffCheckbox.Checked = configuration.RemoveFlashing;
            //characterSpriteList.SelectedIndex = configuration.Sprite.SelectionIndex;
            //tunicColorList.SelectedIndex = tunicColorList.FindStringExact(configuration.Tunic);
            //shieldColorList.SelectedIndex = shieldColorList.FindStringExact(configuration.ShieldTunic);
            //beamSpriteList.SelectedIndex = beamSpriteList.FindStringExact(configuration.BeamSprite);
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
            MessageBox.Show("Invalid flags entered!");
        }
        dontrunhandler = false;
    }

    private void UpdateBtn_Click(object sender, EventArgs e)
    {
        WinSparkle.win_sparkle_check_update_with_ui();
    }

    private void ShuffleEnemies_CheckStateChanged(object sender, EventArgs e)
    {
        if (shufflePalaceEnemiesCheckbox.Checked || shuffleOverworldEnemiesCheckbox.Checked)
        {
            mixLargeAndSmallCheckbox.Enabled = true;
            generatorsMatchCheckBox.Enabled = true;
        }
        else
        {
            mixLargeAndSmallCheckbox.Checked = false;
            mixLargeAndSmallCheckbox.Enabled = false;
            generatorsMatchCheckBox.Enabled = false;
            generatorsMatchCheckBox.Enabled = false;
        }
    }

    private void WikiBtn_Click(object sender, EventArgs e)
    {
        var linkForm = new ExternalLinkConfirmationForm(WIKI_URL);
        linkForm.ShowDialog();
    }

    private void DiscordButton_Click(object sender, EventArgs e)
    {
        var linkForm = new ExternalLinkConfirmationForm(DISCORD_URL);
        linkForm.ShowDialog();
    }

    private void PalaceItemBox_CheckStateChanged(object sender, EventArgs e)
    {
        if (shufflePalaceItemsCheckbox.Checked && shuffleOverworldItemsCheckbox.Checked)
        {
            mixOverworldPalaceItemsCheckbox.Enabled = true;
        }
        else
        {
            mixOverworldPalaceItemsCheckbox.Enabled = false;
            mixOverworldPalaceItemsCheckbox.Checked = false;
        }
    }

    private void OverworldItemBox_CheckStateChanged(object sender, EventArgs e)
    {
        if (shufflePalaceItemsCheckbox.Checked && shuffleOverworldItemsCheckbox.Checked)
        {
            mixOverworldPalaceItemsCheckbox.Enabled = true;
        }
        else
        {
            mixOverworldPalaceItemsCheckbox.Enabled = false;
            mixOverworldPalaceItemsCheckbox.Checked = false;
        }

        if (shuffleOverworldItemsCheckbox.Checked)
        {
            includePbagCavesInShuffleCheckbox.Enabled = true;
        }
        else
        {
            includePbagCavesInShuffleCheckbox.Enabled = false;
            includePbagCavesInShuffleCheckbox.Checked = false;
        }
    }

    private void BeginnerFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("jhEhMROm7DZ$MHRBTNBhBAh0PSmA");
        flagsTextBox.Text = config.Serialize();
    }

    private void StandardFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("hAhhD0j9$78$Jp5$$gAhOAdEScuA");
        flagsTextBox.Text = config.Serialize();
    }

    private void MaxRandoFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("iyAqh$j#g7@$ZqTBT!BhOA!0P@@A");
        flagsTextBox.Text = config.Serialize();
    }

    private void RandomPercentFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = new("hEAK0sALvrpUWVXu20Y$8v9ttf9tb7AAJy");
        flagsTextBox.Text = config.Serialize();
    }

    private void TbirdBox_CheckStateChanged(object sender, EventArgs e)
    {
        if (tbirdRequiredCheckbox.Checked)
        {
            removeTbirdCheckbox.Enabled = false;
            removeTbirdCheckbox.Checked = false;
        }
        else
        {
            removeTbirdCheckbox.Enabled = true;
        }
    }

    private void RemoveTbird_CheckStateChanged(object sender, EventArgs e)
    {
        if (removeTbirdCheckbox.Checked)
        {
            tbirdRequiredCheckbox.Enabled = false;
            tbirdRequiredCheckbox.Checked = false;
        }
        else
        {
            tbirdRequiredCheckbox.Enabled = true;
        }
    }

    private void PalaceSwapBox_CheckStateChanged(object sender, EventArgs e)
    {
        if (allowPalaceContinentSwapCheckbox.CheckState == CheckState.Unchecked)
        {
            includeGPinShuffleCheckbox.CheckState = CheckState.Unchecked;
            includeGPinShuffleCheckbox.Enabled = false;
        }
        else
        {
            includeGPinShuffleCheckbox.Enabled = true;
        }

    }

    private void Bulk_Generate_Click(object sender, EventArgs e)
    {
        String flagString = flagsTextBox.Text;

        if (flagString.Length != validFlagStringLength)
        {
            MessageBox.Show("Invalid flags. Aborting seed generation.");
            return;
        }

        for (int i = 0; i < flagString.Length; i++)
        {
            if (!flags.Contains(flagString[i]))
            {
                MessageBox.Show("Invalid flags. Aborting seed generation.");
                return;
            }
        }
        GenerateBatchForm f = new GenerateBatchForm();
        f.ShowDialog();

        int numSeeds = f.numSeeds;
        if (numSeeds > 0)
        {
            config = ExportConfig();
            f3 = new GeneratingSeedsForm();
            f3.Show();
            int i = 0;
            spawnNextSeed = true;
            while (i < numSeeds)
            {

                f3.Text = "Generating seed " + (i + 1) + " of " + numSeeds + "...";


                config.Seed = r.Next(1000000000);
                if (spawnNextSeed)
                {
                    backgroundWorker1 = new BackgroundWorker();
                    backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundWorker1_DoWork);
                    backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
                    backgroundWorker1.WorkerReportsProgress = true;
                    backgroundWorker1.WorkerSupportsCancellation = true;
                    backgroundWorker1.RunWorkerAsync();
                    f3.setText("Generating Palaces");
                    spawnNextSeed = false;
                }
                else
                {
                    Application.DoEvents();
                    if (!backgroundWorker1.IsBusy)
                    {
                        i++;
                        if (i <= numSeeds)
                        {
                            f3.Text = "Generating seed " + (i + 1) + " of " + numSeeds + "...";
                        }
                        spawnNextSeed = true;
                    }
                }

                if (f3.isClosed)
                {
                    backgroundWorker1.CancelAsync();
                    return;
                }

            }
            f3.Close();
            MessageBox.Show("Batch generation complete!");
        }
    }

    private void AtLeastOneChecked(object sender, EventArgs e)
    {
        if (!randomizeDropsCheckbox.Checked)
        {
            CheckBox c = (CheckBox)sender;
            CheckBox[] l = large;
            if (small.Contains(sender))
            {
                l = small;
            }
            int count = 0;
            foreach (CheckBox b in l)
            {
                if (b.Checked)
                {
                    count++;
                }
            }
            if (count == 0)
            {
                c.Checked = true;
            }
        }
    }

    private void RandomizeDropsChanged(object sender, EventArgs e)
    {
        if (randomizeDropsCheckbox.Checked)
        {
            large.Union(small).ToList().ForEach(i => i.Checked = false);
        }
        else
        {
            smallEnemiesBlueJarCheckbox.Checked = true;
            smallEnemiesSmallBagCheckbox.Checked = true;
            largeEnemiesRedJarCheckbox.Checked = true;
            largeEnemiesLargeBagCheckbox.Checked = true;
        }
    }

    private void CustomSave1_Click(object sender, EventArgs e)
    {
        customFlags1TextBox.Text = flagsTextBox.Text;
        Properties.Settings.Default.custom1 = flagsTextBox.Text;
        Properties.Settings.Default.Save();
    }

    private void CustomLoad1_Click(object sender, EventArgs e)
    {
        flagsTextBox.Text = customFlags1TextBox.Text;
    }

    private void CustomSave2_Click(object sender, EventArgs e)
    {
        customFlags2TextBox.Text = flagsTextBox.Text;
        Properties.Settings.Default.custom2 = flagsTextBox.Text;
        Properties.Settings.Default.Save();
    }

    private void CustomSave3_Click(object sender, EventArgs e)
    {
        customFlags3TextBox.Text = flagsTextBox.Text;
        Properties.Settings.Default.custom3 = flagsTextBox.Text;
        Properties.Settings.Default.Save();
    }

    private void CustomLoad2_Click(object sender, EventArgs e)
    {
        flagsTextBox.Text = customFlags2TextBox.Text;
    }

    private void CustomLoad3_Click(object sender, EventArgs e)
    {
        flagsTextBox.Text = customFlags3TextBox.Text;
    }

    /*private void townSwap_CheckStateChanged(object sender, EventArgs e)
    {
        if(townSwap.Checked)
        {
            hideKasutoBox.SelectedIndex = 0;
            hideKasutoBox.Enabled = false;
        }
        else
        {
            hideKasutoBox.Enabled = true;
        }
    }*/

    private void EnableLevelScaling(object sender, EventArgs e)
    {
        if (!atkCapList.GetItemText(atkCapList.SelectedItem).Equals("8")
            || !magCapList.GetItemText(magCapList.SelectedItem).Equals("8")
            || !lifeCapList.GetItemText(lifeCapList.SelectedItem).Equals("8"))
        {
            scaleLevelRequirementsToCapCheckbox.Enabled = true;
        }
        else
        {
            scaleLevelRequirementsToCapCheckbox.Enabled = false;
            scaleLevelRequirementsToCapCheckbox.Checked = false;
        }
    }

    private void SpellItemBox_CheckStateChanged(object sender, EventArgs e)
    {
        if (removeSpellitemsCheckbox.Checked)
        {
            enableSpellItemHintsCheckbox.Enabled = false;
            enableSpellItemHintsCheckbox.Checked = false;
        }
        else
        {
            enableSpellItemHintsCheckbox.Enabled = true;
        }
    }

    private void MazeBiome_SelectedIndexChanged(object sender, EventArgs e)
    {
        CheckVanillaPossible();
    }

    private void EastBiome_SelectedIndexChanged(object sender, EventArgs e)
    {

        //if(eastBiome.SelectedIndex == 1)
        //{
        //    hideKasutoBox.SelectedIndex = 0;
        //    hpCmbo.SelectedIndex = 0;
        //    hideKasutoBox.Enabled = false;
        //    hpCmbo.Enabled = false;
        //}
        //else
        //{
        //    hpCmbo.Enabled = true;
        //    hideKasutoBox.Enabled = true;
        //}
        ShuffleHiddenEnable();
        CheckVanillaPossible();


    }

    private void ShuffleHiddenEnable()
    {
        if (eastBiome.SelectedIndex == 0 || (hiddenPalaceList.SelectedIndex == 0 && hideKasutoList.SelectedIndex == 0))
        {

            shuffleWhichLocationsAreHiddenCheckbox.Enabled = false;
            shuffleWhichLocationsAreHiddenCheckbox.Checked = false;
        }
        else
        {
            shuffleWhichLocationsAreHiddenCheckbox.Enabled = true;
        }
    }

    private void CheckVanillaPossible()
    {
        if (VanillaPossible(eastBiome) || VanillaPossible(westBiome) || VanillaPossible(dmBiome) || VanillaPossible(mazeBiome))
        {
            shuffledVanillaShowsActualTerrain.Enabled = true;
            shuffledVanillaShowsActualTerrain.Checked = true;
        }
        else
        {
            shuffledVanillaShowsActualTerrain.Enabled = false;
            shuffledVanillaShowsActualTerrain.Checked = false;
        }
    }

    private bool VanillaPossible(ComboBox cb)
    {
        if (cb.SelectedIndex == 0 || cb.SelectedIndex == 1 || cb.GetItemText(cb.SelectedItem).Equals("Random (with Vanilla)"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void WestBiome_SelectedIndexChanged(object sender, EventArgs e)
    {
        CheckVanillaPossible();
        if (westBiome.SelectedIndex == 0 || westBiome.SelectedIndex == 1)
        {
            generateBaguWoodsCheckbox.Checked = false;
            generateBaguWoodsCheckbox.Enabled = false;
        }
        else
        {
            generateBaguWoodsCheckbox.Enabled = true;
        }
    }

    private void DmBiome_SelectedIndexChanged(object sender, EventArgs e)
    {
        CheckVanillaPossible();
    }

    private void HpCmbo_SelectedIndexChanged(object sender, EventArgs e)
    {
        ShuffleHiddenEnable();
    }

    private void HideKasutoBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        ShuffleHiddenEnable();
    }

    private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;

        new Hyrule(config, worker);
        if (worker.CancellationPending)
        {
            e.Cancel = true;
        }
    }

    private void BackgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
    {
        if (e.ProgressPercentage == 2)
        {
            f3.setText("Generating Western Hyrule");
        }
        else if (e.ProgressPercentage == 3)
        {
            f3.setText("Generating Death Mountain");
        }
        else if (e.ProgressPercentage == 4)
        {
            f3.setText("Generating East Hyrule");
        }
        else if (e.ProgressPercentage == 5)
        {
            f3.setText("Generating Maze Island");
        }
        else if (e.ProgressPercentage == 6)
        {
            f3.setText("Shuffling Items and Spells");
        }
        else if (e.ProgressPercentage == 7)
        {
            f3.setText("Running Seed Completability Checks");
        }
        else if (e.ProgressPercentage == 8)
        {
            f3.setText("Generating Hints");
        }
        else if (e.ProgressPercentage == 9)
        {
            f3.setText("Finishing up");
        }
    }

    private void PalaceBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (palaceStyleList.SelectedIndex == 0 || palaceStyleList.SelectedIndex == 1)
        {
            includeCommunityRoomsCheckbox.Enabled = false;
            includeCommunityRoomsCheckbox.Checked = false;
            blockingRoomsInAnyPalaceCheckbox.Enabled = false;
            blockingRoomsInAnyPalaceCheckbox.Checked = false;
            bossRoomsExitToPalaceCheckbox.Checked = false;
            bossRoomsExitToPalaceCheckbox.Enabled = false;
            noDuplicateRoomsCheckbox.Checked = false;
            noDuplicateRoomsCheckbox.Enabled = false;
        }
        else
        {
            includeCommunityRoomsCheckbox.Enabled = true;
            blockingRoomsInAnyPalaceCheckbox.Enabled = true;
            bossRoomsExitToPalaceCheckbox.Enabled = true;
            noDuplicateRoomsCheckbox.Enabled = true;
        }

        if (palaceStyleList.SelectedIndex != 0)
        {
            shortGPCheckbox.Enabled = true;
            tbirdRequiredCheckbox.Enabled = true;
        }
        else
        {
            shortGPCheckbox.Checked = false;
            shortGPCheckbox.Enabled = false;
            tbirdRequiredCheckbox.Enabled = false;
            tbirdRequiredCheckbox.Checked = true;
        }
    }

    private static bool? GetTripleCheckState(CheckBox checkBox)
    {
        return checkBox.CheckState switch
        {
            CheckState.Checked => true,
            CheckState.Unchecked => false,
            CheckState.Indeterminate => null,
            _ => throw new ImpossibleException("Invalid CheckState")
        };
    }

    private static CheckState ToCheckState(bool? value)
    {
        return value switch
        {
            false => CheckState.Unchecked,
            true => CheckState.Checked,
            null => CheckState.Indeterminate
        };
    }

    private bool Validate(string flagString)
    {
        if (flagString.Length != validFlagStringLength)
        {
            MessageBox.Show("Invalid flags. Aborting seed generation.");
            return false;
        }

        for (int i = 0; i < flagString.Length; i++)
        {
            if (!flags.Contains(flagString[i]))
            {
                MessageBox.Show("Invalid flags. Aborting seed generation.");
                return false;
            }
        }
        if (startHeartsMinList.SelectedIndex <= 8
            && startHeartsMinList.SelectedIndex <= 8
            && startHeartsMaxList.SelectedIndex < startHeartsMinList.SelectedIndex)
        {
            MessageBox.Show("Start max hearts must be greater than or equal to start min hearts.");
            return false;
        }
        if (startHeartsMinList.SelectedIndex <= 8
            && maxHeartsList.SelectedIndex <= 8
            && maxHeartsList.SelectedIndex < startHeartsMinList.SelectedIndex)
        {
            MessageBox.Show("Seed max hearts must be greater than or equal to start min hearts.");
            return false;
        }
        if (startingGemsMinList.SelectedIndex > startingGemsMaxList.SelectedIndex)
        {
            MessageBox.Show("Required palaces min must be less than or equal to max.");
            return false;
        }
        return true;
    }
}

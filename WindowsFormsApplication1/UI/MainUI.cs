using NLog;
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using Z2Randomizer.Overworld;

namespace Z2Randomizer;

public partial class MainUI : Form
{
    private readonly Logger logger = LogManager.GetCurrentClassLogger();
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

    private int validFlagStringLength;

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
        r = new Random();
        startingHeartsList.SelectedIndex = 3;
        maxHeartsList.SelectedIndex = 7;
        startingGemsList.SelectedIndex = 6;
        startingTechsList.SelectedIndex = 0;
        allowPathEnemiesCheckbox.Enabled = false;
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


        customFlags1TextBox.TextChanged += new System.EventHandler(this.customSave1_Click);
        customFlags2TextBox.TextChanged += new System.EventHandler(this.customSave2_Click);
        customFlags3TextBox.TextChanged += new System.EventHandler(this.customSave3_Click);


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
        CheckBox[] temp = { smallEnemiesBlueJarCheckbox, smallEnemiesRedJarCheckbox, smallEnemiesSmallBagCheckbox, smallEnemiesMediumBagCheckbox, smallEnemiesLargeBagCheckbox, smallEnemiesXLBagCheckbox, smallEnemies1UpCheckbox, smallEnemiesKeyCheckbox };
        small = temp;
        CheckBox[] temp2 = { largeEnemiesBlueJarCheckbox, largeEnemiesRedJarCheckbox, largeEnemiesSmallBagCheckbox, largeEnemiesMediumBagCheckbox, largeEnemiesLargeBagCheckbox, largeEnemiesXLBagCheckbox, largeEnemies1UpCheckbox, largeEnemiesKeyCheckbox };
        large = temp2;


        this.Text = "Zelda 2 Randomizer Version " + typeof(MainUI).Assembly.GetName().Version.Major + "." + typeof(MainUI).Assembly.GetName().Version.Minor + "." + typeof(MainUI).Assembly.GetName().Version.Build;

        flagsTextBox.DoubleClick += new System.EventHandler(this.flagBox_Clicked);
        oldFlagsTextbox.DoubleClick += new System.EventHandler(this.oldFlagsTextbox_Clicked);

        shuffleStartingItemsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithCandleCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithGloveCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithRaftCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithBootsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithFluteCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithCrossCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithHammerCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithMagicKeyCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleStartingSpellsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithShieldCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithJumpCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithLifeCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithFairyCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithFireCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithReflectCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWithSpellCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startWIthThunderCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingHeartsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        maxHeartsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingTechsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingGemsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeLivesBox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleEnemyHPBox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleAllExpCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleAtkExpNeededCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        lifeExpNeededCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        magicExpNeededCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleXPStealersCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleStealXPAmountCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleSwordImmunityBox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        jumpAlwaysOnCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleLifeRefillCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        disableLowHealthBeepCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        tbirdRequiredCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        experienceDropsList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleEncountersCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        allowPathEnemiesCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleOverworldEnemiesCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shufflePalaceEnemiesCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        mixLargeAndSmallCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shufflePalaceItemsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleOverworldItemsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        mixOverworldPalaceItemsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleSmallItemsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleSpellLocationsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        disableMagicContainerRequirementCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        palacesHaveExtraKeysCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleDropFrequencyCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        palacePaletteCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        allowPalaceContinentSwapCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        attackEffectivenessList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        magicEffectivenessList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        lifeEffectivenessList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        restartAtPalacesCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shortGPCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeJarRequirementsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        combineFireCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        removeTbirdCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        alwaysBeamCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includePbagCavesInShuffleCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includeGPinShuffleCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleDripperEnemyCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleEnemyPalettesCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        hiddenPalaceList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        hideKasutoList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        manuallySelectDropsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        removeSpellitemsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        useCommunityHintsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        standardizeDropsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeDropsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shufflePbagAmountsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        atkCapList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        magCapList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        lifeCapList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        scaleLevelRequirementsToCapCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        enableTownNameHintsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        enableHelpfulHintsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        enableSpellItemHintsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        encounterRateBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingAttackLevelList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingMagicLevelList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        startingLifeLevelList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        continentConnectionBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        saneCaveShuffleBox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        hideLessImportantLocationsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        allowBoulderBlockedConnectionsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        westBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        dmBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        eastBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        mazeBiome.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffledBasnill.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        shuffleWhichLocationsAreHiddenCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeBossItemCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        useGoodBootsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        randomizeSpellSpellEnemyCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        generateBaguWoodsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        palaceStyleList.SelectedIndexChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        includeCommunityRoomsCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        blockingRoomsInAnyPalaceCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        bossRoomsExitToPalaceCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        useDashCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
        dashAlwaysOnCheckbox.CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);

        //townSwap.CheckedChanged += new System.EventHandler(this.updateFlags);

        enableLevelScaling(null, null);
        eastBiome_SelectedIndexChanged(null, null);
        for (int i = 0; i < small.Count(); i++)
        {
            small[i].CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
            small[i].CheckedChanged += new System.EventHandler(this.AtLeastOneChecked);
            large[i].CheckedChanged += new System.EventHandler(this.UpdateFlagsTextbox);
            large[i].CheckedChanged += new System.EventHandler(this.AtLeastOneChecked);
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
        //WinSparkle.win_sparkle_set_appcast_url("https://www.dropbox.com/s/w4d9qptlg1kyx0o/appcast.xml?dl=1");
        //WinSparkle.win_sparkle_set_app_details("Company","App", "Version"); // THIS CALL NOT IMPLEMENTED YET
        //WinSparkle.win_sparkle_init();
    }

    private void shuffleItemBox_CheckedChanged(object sender, EventArgs e)
    {
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

    private void spellShuffleBox_CheckedChanged(object sender, EventArgs e)
    {
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
    }

    private void generateBtn_Click(object sender, EventArgs e)
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
        if (startingHeartsList.SelectedIndex != 8 && startingHeartsList.SelectedIndex > maxHeartsList.SelectedIndex && maxHeartsList.SelectedIndex != 8)
        {
            MessageBox.Show("Max hearts must be greater than or equal to starting hearts!");
            return;
        }
        Properties.Settings.Default.filePath = romFileTextBox.Text;
        Properties.Settings.Default.beep = disableLowHealthBeepCheckbox.Checked;
        Properties.Settings.Default.beams = beamSpriteList.SelectedIndex;
        Properties.Settings.Default.spells = fastSpellCheckbox.Checked;
        Properties.Settings.Default.tunic = tunicColorList.SelectedIndex;
        Properties.Settings.Default.shield = shieldColorList.SelectedIndex;
        Properties.Settings.Default.music = disableMusicCheckbox.Checked;
        Properties.Settings.Default.sprite = characterSpriteList.SelectedIndex;
        Properties.Settings.Default.upac1 = upAOnController1Checkbox.Checked;
        Properties.Settings.Default.noflash = flashingOffCheckbox.Checked;
        Properties.Settings.Default.lastused = flagsTextBox.Text;
        Properties.Settings.Default.lastseed = seedTextBox.Text;
        Properties.Settings.Default.Save();
        try
        {
            Int32.Parse(seedTextBox.Text);
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
        f3 = new GeneratingSeedsForm();
        f3.Show();

        f3.setText("Generating Palaces");
        f3.Width = 250;
        Exception generationException = null;
        backgroundWorker1 = new BackgroundWorker();
        backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
        backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        backgroundWorker1.WorkerReportsProgress = true;
        backgroundWorker1.WorkerSupportsCancellation = true;
        backgroundWorker1.RunWorkerCompleted += (completed_sender, completed_event) => {
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
            MessageBox.Show("File " + "Z2_" + seedTextBox.Text + "_" + flagsTextBox.Text + ".nes" + " has been created!");
        }
        else
        {
            MessageBox.Show("An exception occurred generating the rom");
            logger.Error(generationException.StackTrace);
        }
    }


    private void shuffleAllExp_CheckedChanged(object sender, EventArgs e)
    {
        shuffleAtkExpNeededCheckbox.Checked = shuffleAllExpCheckbox.Checked;
        shuffleAtkExpNeededCheckbox.Enabled = !shuffleAllExpCheckbox.Checked;

        magicExpNeededCheckbox.Checked = shuffleAllExpCheckbox.Checked;
        magicExpNeededCheckbox.Enabled = !shuffleAllExpCheckbox.Checked;

        lifeExpNeededCheckbox.Checked = shuffleAllExpCheckbox.Checked;
        lifeExpNeededCheckbox.Enabled = !shuffleAllExpCheckbox.Checked;
    }


    private void shuffleEncounters_CheckedChanged(object sender, EventArgs e)
    {
        allowPathEnemiesCheckbox.Enabled = shuffleEncountersCheckbox.Checked;
        if (!shuffleEncountersCheckbox.Checked)
        {
            allowPathEnemiesCheckbox.Checked = false;
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
        RandomizerConfiguration configuration= new RandomizerConfiguration();

        configuration.FileName = romFileTextBox.Text;
        configuration.Seed = Int32.Parse(seedTextBox.Text);

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

        configuration.StartingHeartContainers =
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
            _ => throw new Exception("Invalid MaxHearts setting")
        };
        switch(startingTechsList.SelectedIndex)
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
        configuration.PalacesCanSwapContinents = allowPalaceContinentSwapCheckbox.Checked;
        configuration.ShuffleGP = shortGPCheckbox.Checked;
        configuration.ShuffleEncounters = shuffleEncountersCheckbox.Checked;
        configuration.AllowUnsafePathEncounters = allowPathEnemiesCheckbox.Checked;
        configuration.EncounterRate = encounterRateBox.SelectedIndex switch
        {
            0 => EncounterRate.NORMAL,
            1 => EncounterRate.HALF,
            2 => EncounterRate.NONE,
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
        configuration.ShuffleWhichLocationIsHidden = shuffleWhichLocationsAreHiddenCheckbox.Checked;
        configuration.HideLessImportantLocations = hideLessImportantLocationsCheckbox.Checked;
        configuration.RestrictConnectionCaveShuffle = saneCaveShuffleBox.Checked;
        configuration.AllowConnectionCavesToBeBoulderBlocked = allowBoulderBlockedConnectionsCheckbox.Checked;
        configuration.GoodBoots = useGoodBootsCheckbox.Checked;
        configuration.GenerateBaguWoods = generateBaguWoodsCheckbox.Checked;
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
            5 => Biome.CALDERA,
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
        configuration.VanillaShuffleUsesActualTerrain = shuffledBasnill.Checked;

        //Palaces
        configuration.PalaceStyle = palaceStyleList.SelectedIndex switch
        {
            0 => PalaceStyle.VANILLA,
            1 => PalaceStyle.SHUFFLED,
            2 => PalaceStyle.RECONSTRUCTED,
            _ => throw new Exception("Invalid PalaceStyle setting")
        };
        configuration.IncludeCommunityRooms = useCommunityHintsCheckbox.Checked;
        configuration.BlockingRoomsInAnyPalace = blockingRoomsInAnyPalaceCheckbox.Checked;
        configuration.BossRoomsExitToPalace = bossRoomsExitToPalaceCheckbox.Checked;
        configuration.ShortGP = shortGPCheckbox.Checked;
        configuration.TBirdRequired = tbirdRequiredCheckbox.Checked;
        configuration.RemoveTBird = removeTbirdCheckbox.Checked;
        configuration.RestartAtPalacesOnGameOver = restartAtPalacesCheckbox.Checked;
        configuration.ChangePalacePallettes = palacePaletteCheckbox.Checked;
        configuration.RandomizeBossItemDrop = randomizeBossItemCheckbox.Checked;
        configuration.StartingGems = startingGemsList.SelectedIndex <= 6 ? startingGemsList.SelectedIndex : null;

        //Levels
        configuration.ShuffleAttackExperience = shuffleAtkExpNeededCheckbox.Checked;
        configuration.ShuffleMagicExperience = magicExpNeededCheckbox.Checked;
        configuration.ShuffleLifeExperience = lifeExpNeededCheckbox.Checked;
        if(shuffleAllExpCheckbox.Checked)
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
        configuration.LifeEffectiveness =lifeEffectivenessList.SelectedIndex switch
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
        configuration.ShuffleSpellLocations = shuffleStartingSpellsCheckbox.Checked;
        configuration.DisableMagicContainerRequirements = disableMagicContainerRequirementCheckbox.Checked;
        configuration.CombineFireWithRandomSpell = combineFireCheckbox.Checked;
        configuration.RandomizeSpellSpellEnemy = randomizeSpellSpellEnemyCheckbox.Checked;
        configuration.ReplaceFireWithDash = useDashCheckbox.Checked;

        //Enemies
        configuration.ShuffleOverworldEnemies = shuffleOverworldEnemiesCheckbox.Checked;
        configuration.ShufflePalaceEnemies = shufflePalaceEnemiesCheckbox.Checked;
        configuration.ShuffleDripperEnemy = shuffleDripperEnemyCheckbox.Checked;
        configuration.MixLargeAndSmallEnemies = mixLargeAndSmallCheckbox.Checked;
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
        configuration.ShufflePalaceItems = shufflePalaceItemsCheckbox.Checked;
        configuration.ShuffleOverworldItems = shuffleOverworldItemsCheckbox.Checked;
        configuration.MixOverworldAndPalaceItems = mixOverworldPalaceItemsCheckbox.Checked;
        configuration.IncludePBagCavesInItemShuffle = includePbagCavesInShuffleCheckbox.Checked;
        configuration.ShuffleSmallItems = shuffleSmallItemsCheckbox.Checked;
        configuration.PalacesContainExtraKeys = palacesHaveExtraKeysCheckbox.Checked;
        configuration.RandomizeNewKasutoJarRequirements = randomizeJarRequirementsCheckbox.Checked;
        configuration.RemoveSpellItems = removeSpellitemsCheckbox.Checked;
        configuration.ShufflePBagAmounts = shufflePbagAmountsCheckbox.Checked;

        //Drops
        configuration.ShuffleItemDropFrequency = shuffleDropFrequencyCheckbox.Checked;
        configuration.RandomizeDrops = randomizeDropsCheckbox.Checked;
        configuration.StandardizeDrops = standardizeDropsCheckbox.Checked;
        if(manuallySelectDropsCheckbox.Checked)
        {
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
        }


        //Hints
        configuration.EnableHelpfulHints = enableHelpfulHintsCheckbox.Checked;
        configuration.EnableSpellItemHints = enableSpellItemHintsCheckbox.Checked;
        configuration.EnableTownNameHints = enableTownNameHintsCheckbox.Checked;
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
        configuration.Sprite = characterSpriteList.SelectedIndex switch
        {
            0 => CharacterSprite.LINK,
            1 => CharacterSprite.ZELDA,
            2 => CharacterSprite.IRON_KNUCKLE,
            3 => CharacterSprite.ERROR,
            4 => CharacterSprite.SAMUS,
            5 => CharacterSprite.SIMON,
            6 => CharacterSprite.STALFOS,
            7 => CharacterSprite.VASE_LADY,
            8 => CharacterSprite.RUTO,
            9 => CharacterSprite.YOSHI,
            10 => CharacterSprite.DRAGONLORD,
            11 => CharacterSprite.MIRIA,
            12 => CharacterSprite.CRYSTALIS,
            13 => CharacterSprite.TACO,
            14 => CharacterSprite.PYRAMID,
            15 => CharacterSprite.LADY_LINK,
            16 => CharacterSprite.HOODIE_LINK,
            17 => CharacterSprite.GLITCH_WITCH,
            _ => CharacterSprite.LINK
        };
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
        String oldFlags = oldFlagsTextbox.Text;
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags(oldFlags);
        flagsTextBox.Text = config.Serialize();
    }

    private void FlagBox_TextChanged(object sender, EventArgs eventArgs)
    {
        dontrunhandler = true;
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

            startingHeartsList.SelectedIndex = configuration.StartingHeartContainers switch
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
                _ => throw new Exception("Invalid MaxHearts setting")
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
            startingMagicLevelList.SelectedIndex = configuration.StartingAttackLevel - 1;
            startingLifeLevelList.SelectedIndex = configuration.StartingAttackLevel - 1;

            //Overworld
            allowPalaceContinentSwapCheckbox.Checked = configuration.PalacesCanSwapContinents;
            includeGPinShuffleCheckbox.Checked = configuration.ShuffleGP;
            shuffleEncountersCheckbox.Checked = configuration.ShuffleEncounters;
            allowPathEnemiesCheckbox.Checked = configuration.AllowUnsafePathEncounters;
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
            shuffleWhichLocationsAreHiddenCheckbox.Checked = configuration.ShuffleWhichLocationIsHidden;
            hideLessImportantLocationsCheckbox.Checked = configuration.HideLessImportantLocations;
            saneCaveShuffleBox.Checked = configuration.RestrictConnectionCaveShuffle;
            allowBoulderBlockedConnectionsCheckbox.Checked = configuration.AllowConnectionCavesToBeBoulderBlocked;
            useGoodBootsCheckbox.Checked = configuration.GoodBoots;
            generateBaguWoodsCheckbox.Checked = configuration.GenerateBaguWoods;
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
                Biome.CALDERA => 5,
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
            shuffledBasnill.Checked = configuration.VanillaShuffleUsesActualTerrain;

            //Palaces
            palaceStyleList.SelectedIndex = configuration.PalaceStyle switch
            {
                PalaceStyle.VANILLA => 0,
                PalaceStyle.SHUFFLED => 1,
                PalaceStyle.RECONSTRUCTED => 2,
                _ => throw new Exception("Invalid PalaceStyle setting")
            };
            includeCommunityRoomsCheckbox.Checked = configuration.IncludeCommunityRooms;
            blockingRoomsInAnyPalaceCheckbox.Checked = configuration.BlockingRoomsInAnyPalace;
            bossRoomsExitToPalaceCheckbox.Checked = configuration.BossRoomsExitToPalace;
            shortGPCheckbox.Checked = configuration.ShortGP;
            tbirdRequiredCheckbox.Checked = configuration.TBirdRequired;
            removeTbirdCheckbox.Checked = configuration.RemoveTBird;
            restartAtPalacesCheckbox.Checked = configuration.RestartAtPalacesOnGameOver;
            palacePaletteCheckbox.Checked = configuration.ChangePalacePallettes;
            randomizeBossItemCheckbox.Checked = configuration.RandomizeBossItemDrop;
            startingGemsList.SelectedIndex = configuration.StartingGems ?? 7;

            //Levels
            shuffleAtkExpNeededCheckbox.Checked = configuration.ShuffleAttackExperience;
            magicExpNeededCheckbox.Checked = configuration.ShuffleMagicExperience;
            lifeExpNeededCheckbox.Checked = configuration.ShuffleLifeExperience;
            if(configuration.ShuffleAttackExperience && configuration.ShuffleMagicExperience && configuration.ShuffleLifeExperience)
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
            shuffleSpellLocationsCheckbox.Checked = configuration.ShuffleSpellLocations;
            disableMagicContainerRequirementCheckbox.Checked = configuration.DisableMagicContainerRequirements;
            combineFireCheckbox.Checked = configuration.CombineFireWithRandomSpell;
            randomizeSpellSpellEnemyCheckbox.Checked = configuration.RandomizeSpellSpellEnemy;
            useDashCheckbox.Checked = configuration.ReplaceFireWithDash;

            //Enemies
            shuffleOverworldEnemiesCheckbox.Checked = configuration.ShuffleOverworldEnemies;
            shufflePalaceEnemiesCheckbox.Checked = configuration.ShufflePalaceEnemies;
            shuffleDripperEnemyCheckbox.Checked = configuration.ShuffleDripperEnemy;
            mixLargeAndSmallCheckbox.Checked = configuration.MixLargeAndSmallEnemies;
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
            shufflePalaceItemsCheckbox.Checked = configuration.ShufflePalaceItems;
            shuffleOverworldItemsCheckbox.Checked = configuration.ShuffleOverworldItems;
            mixOverworldPalaceItemsCheckbox.Checked = configuration.MixOverworldAndPalaceItems;
            includePbagCavesInShuffleCheckbox.Checked = configuration.IncludePBagCavesInItemShuffle;
            shuffleSmallItemsCheckbox.Checked = configuration.ShuffleSmallItems;
            palacesHaveExtraKeysCheckbox.Checked = configuration.PalacesContainExtraKeys;
            randomizeJarRequirementsCheckbox.Checked = configuration.RandomizeNewKasutoJarRequirements;
            removeSpellitemsCheckbox.Checked = configuration.RemoveSpellItems;
            shufflePbagAmountsCheckbox.Checked = configuration.ShufflePBagAmounts;

            //Drops
            shuffleDropFrequencyCheckbox.Checked = configuration.ShuffleItemDropFrequency;
            standardizeDropsCheckbox.Checked = configuration.StandardizeDrops;
            if(configuration.RandomizeDrops)
            {
                randomizeDropsCheckbox.Checked = true;
                manuallySelectDropsCheckbox.Checked = false;
            }
            else if (configuration.IsVanillaDrops())
            {
                randomizeDropsCheckbox.Checked = false;
                manuallySelectDropsCheckbox.Checked = false;
            }
            else
            {
                randomizeDropsCheckbox.Checked = false;
                manuallySelectDropsCheckbox.Checked = true;
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
            }


            //Hints
            enableHelpfulHintsCheckbox.Checked = configuration.EnableHelpfulHints;
            enableSpellItemHintsCheckbox.Checked = configuration.EnableSpellItemHints;
            enableTownNameHintsCheckbox.Checked = configuration.EnableTownNameHints;
            useCommunityHintsCheckbox.Checked = configuration.UseCommunityHints;

            //Misc
            disableLowHealthBeepCheckbox.Checked = configuration.DisableLowHealthBeep;
            disableMusicCheckbox.Checked = configuration.DisableMusic;
            jumpAlwaysOnCheckbox.Checked = configuration.JumpAlwaysOn;
            dashAlwaysOnCheckbox.Checked = configuration.DashAlwaysOn;
            fastSpellCheckbox.Checked = configuration.FastSpellCasting;
            shuffleEnemyPalettesCheckbox.Checked = configuration.ShuffleSpritePalettes;
            alwaysBeamCheckbox.Checked = configuration.PermanmentBeamSword;
            upAOnController1Checkbox.Checked = configuration.UpAOnController1;
            flashingOffCheckbox.Checked = configuration.RemoveFlashing;
            characterSpriteList.SelectedIndex = configuration.Sprite.SelectionIndex;
            tunicColorList.SelectedIndex = tunicColorList.FindStringExact(configuration.Tunic);
            shieldColorList.SelectedIndex = shieldColorList.FindStringExact(configuration.ShieldTunic);
            beamSpriteList.SelectedIndex = beamSpriteList.FindStringExact(configuration.BeamSprite);
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
            MessageBox.Show("Invalid flags entered!");
        }
        dontrunhandler = false;
    }

    private void updateBtn_Click(object sender, EventArgs e)
    {
        //WinSparkle.win_sparkle_check_update_with_ui();
        throw new NotImplementedException();
    }

    private void shuffleOverworldEnemies_CheckedChanged(object sender, EventArgs e)
    {
        if (shufflePalaceEnemiesCheckbox.Checked || shuffleOverworldEnemiesCheckbox.Checked)
        {
            mixLargeAndSmallCheckbox.Enabled = true;
        }
        else
        {
            mixLargeAndSmallCheckbox.Checked = false;
            mixLargeAndSmallCheckbox.Enabled = false;
        }
    }

    private void shufflePalaceEnemies_CheckedChanged(object sender, EventArgs e)
    {
        if (shufflePalaceEnemiesCheckbox.Checked || shuffleOverworldEnemiesCheckbox.Checked)
        {
            mixLargeAndSmallCheckbox.Enabled = true;
        }
        else
        {
            mixLargeAndSmallCheckbox.Checked = false;
            mixLargeAndSmallCheckbox.Enabled = false;
        }
    }

    private void wikiBtn_Click(object sender, EventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://bitbucket.org/digshake/z2randomizer/wiki/Home") { UseShellExecute = true });
    }
    
    private void DiscordButton_Click(object sender, EventArgs e)
    {
        Process.Start(new ProcessStartInfo("http://z2r.gg/discord") { UseShellExecute = true });
    }

    private void palaceItemBox_CheckedChanged(object sender, EventArgs e)
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

    private void overworldItemBox_CheckedChanged(object sender, EventArgs e)
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
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("jhEhMROm7DZ$MHRBTNBhBAh0PSmA");
        flagsTextBox.Text = config.Serialize();
    }

    private void MaxRandoFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("iyAqh$j#g7@$ZqTBT!BhOA!0P@@A");
        flagsTextBox.Text = config.Serialize();
    }

    private void tbirdBox_CheckedChanged(object sender, EventArgs e)
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

    private void removeTbird_CheckedChanged(object sender, EventArgs e)
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

    private void palaceSwapBox_CheckedChanged(object sender, EventArgs e)
    {
        if (allowPalaceContinentSwapCheckbox.Checked)
        {
            includeGPinShuffleCheckbox.Enabled = true;
        }
        else
        {
            includeGPinShuffleCheckbox.Checked = false;
            includeGPinShuffleCheckbox.Enabled = false;
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
                
            
                int seed = r.Next(1000000000);
                if (spawnNextSeed)
                {
                    backgroundWorker1 = new BackgroundWorker();
                    backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
                    backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
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

    private void enemyDropBox_CheckedChanged(object sender, EventArgs e)
    {
        if(manuallySelectDropsCheckbox.Checked)
        {
            for (int i = 0; i < small.Count(); i++)
            {
                small[i].Enabled = true;
                large[i].Enabled = true;
            }
            randomizeDropsCheckbox.Enabled = false;
        }
        else
        {
            for (int i = 0; i < small.Count(); i++)
            {
                small[i].Enabled = false;
                large[i].Enabled = false;
            }
            randomizeDropsCheckbox.Enabled = true;
        }
    }

    private void AtLeastOneChecked(object sender, EventArgs e)
    {
        CheckBox c = (CheckBox)sender;
        CheckBox[] l = large;
        if(small.Contains(sender))
        {
            l = small;
        }
        int count = 0;
        foreach(CheckBox b in l)
        {
            if(b.Checked)
            {
                count++;
            }
        }
        if(count == 0)
        {
            c.Checked = true;
        }
    }

    private void customSave1_Click(object sender, EventArgs e)
    {
        customFlags1TextBox.Text = flagsTextBox.Text;
        Properties.Settings.Default.custom1 = flagsTextBox.Text;
        Properties.Settings.Default.Save();
    }

    private void customLoad1_Click(object sender, EventArgs e)
    {
        flagsTextBox.Text = customFlags1TextBox.Text;
    }

    private void customSave2_Click(object sender, EventArgs e)
    {
        customFlags2TextBox.Text = flagsTextBox.Text;
        Properties.Settings.Default.custom2 = flagsTextBox.Text;
        Properties.Settings.Default.Save();
    }

    private void customSave3_Click(object sender, EventArgs e)
    {
        customFlags3TextBox.Text = flagsTextBox.Text;
        Properties.Settings.Default.custom3 = flagsTextBox.Text;
        Properties.Settings.Default.Save();
    }

    private void customLoad2_Click(object sender, EventArgs e)
    {
        flagsTextBox.Text = customFlags2TextBox.Text;
    }

    private void customLoad3_Click(object sender, EventArgs e)
    {
        flagsTextBox.Text = customFlags3TextBox.Text;
    }

    private void randoDrops_CheckedChanged(object sender, EventArgs e)
    {
        if(randomizeDropsCheckbox.Checked)
        {
            manuallySelectDropsCheckbox.Enabled = false;
        }
        else
        {
            manuallySelectDropsCheckbox.Enabled = true;
        }
    }

    /*private void townSwap_CheckedChanged(object sender, EventArgs e)
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

    private void enableLevelScaling(object sender, EventArgs e)
    {
        if(!atkCapList.GetItemText(atkCapList.SelectedItem).Equals("8") || !magCapList.GetItemText(magCapList.SelectedItem).Equals("8") || !lifeCapList.GetItemText(lifeCapList.SelectedItem).Equals("8"))
        {
            scaleLevelRequirementsToCapCheckbox.Enabled = true;
        }
        else
        {
            scaleLevelRequirementsToCapCheckbox.Enabled = false;
            scaleLevelRequirementsToCapCheckbox.Checked = false;
        }
    }

    private void spellItemBox_CheckedChanged(object sender, EventArgs e)
    {
        if(!removeSpellitemsCheckbox.Checked)
        {
            enableSpellItemHintsCheckbox.Enabled = false;
            enableSpellItemHintsCheckbox.Checked = false;
        }
        else
        {
            enableSpellItemHintsCheckbox.Enabled = true;
        }
    }

    private void mazeBiome_SelectedIndexChanged(object sender, EventArgs e)
    {
        checkVanillaPossible();
    }

    private void eastBiome_SelectedIndexChanged(object sender, EventArgs e)
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
        shuffleHiddenEnable();
        checkVanillaPossible();


    }

    private void shuffleHiddenEnable()
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

    private void checkVanillaPossible()
    {
        if(vanillaPossible(eastBiome) || vanillaPossible(westBiome) || vanillaPossible(dmBiome) || vanillaPossible(mazeBiome))
        {
            shuffledBasnill.Enabled = true;
        }
        else
        {
            shuffledBasnill.Enabled = false;
            shuffledBasnill.Checked = false;
        }
    }

    private Boolean vanillaPossible(ComboBox cb)
    {
        if(cb.SelectedIndex == 0 || cb.SelectedIndex == 1 || cb.GetItemText(cb.SelectedItem).Equals("Random (with Vanilla)"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void westBiome_SelectedIndexChanged(object sender, EventArgs e)
    {
        checkVanillaPossible();
        if(westBiome.SelectedIndex == 0 || westBiome.SelectedIndex == 1)
        {
            generateBaguWoodsCheckbox.Checked = false;
            generateBaguWoodsCheckbox.Enabled = false;
        }
        else
        {
            generateBaguWoodsCheckbox.Enabled = true;
        }
    }

    private void dmBiome_SelectedIndexChanged(object sender, EventArgs e)
    {
        checkVanillaPossible();
    }

    private void hpCmbo_SelectedIndexChanged(object sender, EventArgs e)
    {
        shuffleHiddenEnable();
    }

    private void hideKasutoBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        shuffleHiddenEnable();
    }

    private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;
        
        new Hyrule(config, worker);
        if(worker.CancellationPending)
        {
            e.Cancel = true;
        }
    }

    private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
    {
        if(e.ProgressPercentage == 2)
        {
            f3.setText("Generating Western Hyrule");
        }
        else if(e.ProgressPercentage == 3)
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

    private void palaceBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(palaceStyleList.SelectedIndex != 2)
        {
            includeCommunityRoomsCheckbox.Enabled = false;
            includeCommunityRoomsCheckbox.Checked = false;
            blockingRoomsInAnyPalaceCheckbox.Enabled = false;
            blockingRoomsInAnyPalaceCheckbox.Checked = false;
            bossRoomsExitToPalaceCheckbox.Checked = false;
            bossRoomsExitToPalaceCheckbox.Enabled = false;
        }
        else
        {
            includeCommunityRoomsCheckbox.Enabled = true;
            blockingRoomsInAnyPalaceCheckbox.Enabled = true;
            bossRoomsExitToPalaceCheckbox.Enabled = true;
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

    private void dashBox_CheckedChanged(object sender, EventArgs e)
    {
        if(useDashCheckbox.Checked)
        {
            combineFireCheckbox.Enabled = false;
            combineFireCheckbox.Checked = false;
        }
        else
        {
            combineFireCheckbox.Enabled = true;
        }
    }

    private void combineFireBox_CheckedChanged(object sender, EventArgs e)
    {
        if (combineFireCheckbox.Checked)
        {
            useDashCheckbox.Enabled = false;
            useDashCheckbox.Checked = false;
        }
        else
        {
            useDashCheckbox.Enabled = true;
        }
    }
}

using NLog;
using Z2Randomizer.Core;
using System.ComponentModel;
using Z2Randomizer.Core.Overworld;
using WinFormUI.UI;
using Z2Randomizer.Core.Flags;

namespace Z2Randomizer.WinFormUI;

public partial class MainUI : Form
{
    const string DISCORD_URL = @"http://z2r.gg/discord";
    const string WIKI_URL = @"https://bitbucket.org/digshake/z2randomizer/wiki/Home";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly Random r;
    private bool dontrunhandler;
    private bool spawnNextSeed;
    private CheckBox[] small;
    private CheckBox[] large;
    private GeneratingSeedsForm f3;
    private RandomizerConfiguration config;
    private List<Button> customisableButtons = new List<Button>();
    private SpritePreview? _spritePreview;

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
        beamSpriteList.SelectedIndex = Properties.Settings.Default.beams;
        disableMusicCheckbox.Checked = Properties.Settings.Default.music;
        upAOnController1Checkbox.Checked = Properties.Settings.Default.upac1;
        seedTextBox.Text = Properties.Settings.Default.lastseed;
        flashingOffCheckbox.Checked = Properties.Settings.Default.noflash;
        useCustomRoomsBox.Checked = Properties.Settings.Default.useCustomRooms;
        beepFrequencyDropdown.SelectedIndex = Properties.Settings.Default.beepFrequency;
        beepThresholdDropdown.SelectedIndex = Properties.Settings.Default.beepThreshold;

        small = new CheckBox[] { smallEnemiesBlueJarCheckbox, smallEnemiesRedJarCheckbox, smallEnemiesSmallBagCheckbox, smallEnemiesMediumBagCheckbox,
            smallEnemiesLargeBagCheckbox, smallEnemiesXLBagCheckbox, smallEnemies1UpCheckbox, smallEnemiesKeyCheckbox };
        large = new CheckBox[] { largeEnemiesBlueJarCheckbox, largeEnemiesRedJarCheckbox, largeEnemiesSmallBagCheckbox, largeEnemiesMediumBagCheckbox,
            largeEnemiesLargeBagCheckbox, largeEnemiesXLBagCheckbox, largeEnemies1UpCheckbox, largeEnemiesKeyCheckbox };

        flagsTextBox.TextChanged += FlagBox_TextChanged;
        InitialiseCustomFlagsetButtons();

        dontrunhandler = false;
        mixLargeAndSmallCheckbox.Enabled = false;
        mixOverworldPalaceItemsCheckbox.Enabled = false;
        includePbagCavesInShuffleCheckbox.Enabled = false;
        includeGPinShuffleCheckbox.Enabled = false;
        tbirdRequiredCheckbox.Checked = true;
        hiddenPalaceList.SelectedIndex = 0;
        hideKasutoList.SelectedIndex = 0;
        var selectedSprite = Properties.Settings.Default.sprite;
        characterSpriteList.SelectedIndex = (selectedSprite > (characterSpriteList.Items.Count - 1)) ? 0 : selectedSprite;

        Text = "Zelda 2 Randomizer Version "
            + typeof(MainUI).Assembly.GetName().Version.Major + "."
            + typeof(MainUI).Assembly.GetName().Version.Minor + "."
            + typeof(MainUI).Assembly.GetName().Version.Build;

        flagsTextBox.DoubleClick += new System.EventHandler(flagBox_Clicked);
        shuffleStartingItemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        oldFlagsTextbox.DoubleClick += new System.EventHandler(oldFlagsTextbox_Clicked);

        flagsTextBox.TextChanged += new System.EventHandler(FlagBox_Changed);
        shuffleStartingItemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithCandleCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithGloveCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithRaftCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithBootsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithFluteCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithCrossCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithHammerCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithMagicKeyCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleStartingSpellsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithShieldCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithJumpCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithLifeCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithFairyCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithFireCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithReflectCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWithSpellCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startWIthThunderCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        startHeartsMinList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startHeartsMaxList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        maxHeartsList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startingTechsList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startingGemsMinList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startingGemsMaxList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        randomizeLivesBox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleEnemyHPBox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleAllExpCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleAtkExpNeededCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        lifeExpNeededCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        magicExpNeededCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleXPStealersCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleStealXPAmountCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleSwordImmunityBox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        jumpAlwaysOnCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleLifeRefillCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        tbirdRequiredCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        experienceDropsList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleEncountersCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        allowPathEnemiesCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        includeLavaInShuffleCheckBox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleOverworldEnemiesCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shufflePalaceEnemiesCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        mixLargeAndSmallCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shufflePalaceItemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleOverworldItemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        mixOverworldPalaceItemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleSmallItemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleSpellLocationsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        disableMagicContainerRequirementCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        palacesHaveExtraKeysCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleDropFrequencyCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        palacePaletteCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        allowPalaceContinentSwapCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        attackEffectivenessList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        magicEffectivenessList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        lifeEffectivenessList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        restartAtPalacesCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        randomizeJarRequirementsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        FireSpellBox.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        removeTbirdCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        alwaysBeamCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        includePbagCavesInShuffleCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        includeGPinShuffleCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleDripperEnemyCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleEnemyPalettesCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        hiddenPalaceList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        hideKasutoList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        removeSpellitemsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        useCommunityHintsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        standardizeDropsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        randomizeDropsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shufflePbagAmountsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        atkCapList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        magCapList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        lifeCapList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        scaleLevelRequirementsToCapCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        enableTownNameHintsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        enableHelpfulHintsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        enableSpellItemHintsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        encounterRateBox.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startingAttackLevelList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startingMagicLevelList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        startingLifeLevelList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        continentConnectionBox.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        saneCaveShuffleBox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        hideLessImportantLocationsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        allowBoulderBlockedConnectionsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        climateSelector.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        westBiomeSelector.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        dmBiome.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        eastBiome.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        mazeBiome.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffledVanillaShowsActualTerrain.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        shuffleWhichLocationsAreHiddenCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        randomizeBossItemCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        useGoodBootsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        randomizeSpellSpellEnemyCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        generateBaguWoodsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        palaceStyleList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        gpStyleList.SelectedIndexChanged += new System.EventHandler(UpdateFlagsTextbox);
        includeVanillaRoomsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        blockingRoomsInAnyPalaceCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        bossRoomsExitToPalaceCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        swapUpAndDownstabCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        dashAlwaysOnCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        noDuplicateRoomsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        generatorsMatchCheckBox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        includeVanillaRoomsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        includev4_0RoomsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        includev4_4RoomsCheckbox.CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
        //townSwap.CheckStateChanged += new System.EventHandler(updateFlags);

        TryLoadSpriteImageFromFile(romFileTextBox.Text);

        EnableLevelScaling(null, null);
        EastBiome_SelectedIndexChanged(null, null);
        randomizeDropsCheckbox.CheckedChanged += new System.EventHandler(RandomizeDropsChanged);
        for (int i = 0; i < small.Count(); i++)
        {
            small[i].CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
            small[i].CheckStateChanged += new System.EventHandler(AtLeastOneChecked);
            large[i].CheckStateChanged += new System.EventHandler(UpdateFlagsTextbox);
            large[i].CheckStateChanged += new System.EventHandler(AtLeastOneChecked);
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

    private void InitialiseCustomFlagsetButtons()
    {

        //customisable buttons - config settings are stored using the name of the control as the key, so order does not specifically matter. 
        customisableButtons.Add(customFlagsButton1);
        customisableButtons.Add(customFlagsButton2);
        customisableButtons.Add(customFlagsButton3);
        customisableButtons.Add(customFlagsButton4);
        customisableButtons.Add(customFlagsButton5);
        customisableButtons.Add(customFlagsButton6);


        // could probably implement a custom toolstrip item here so we can figure out whether things should be enabled or not
        // or we rebuild the menu every time it's opened.... how often is it really going to be needed.
        // in the meantime, this'll do.
        customisableButtonContextMenu.Items.Add("Edit", null, CustomFlagsetButtonContextMenuOnClick);
        customisableButtonContextMenu.Items.Add("Copy", null, CustomFlagsetButtonContextMenuOnClick);
        customisableButtonContextMenu.Items.Add(new ToolStripSeparator());
        customisableButtonContextMenu.Items.Add("Clear", null, CustomFlagsetButtonContextMenuOnClick);


        foreach (var button in customisableButtons)
        {
            // only because I couldn't be bothered to do it in the designer
            button.AutoEllipsis = false;
            button.AutoSize = false;

            var setting = (System.Collections.Specialized.StringCollection)Properties.Settings.Default[button.Name];
            var customButtonSettings = new CustomisedButtonSettings(setting);

            if (customButtonSettings.IsEmpty)
            {
                customButtonSettings = new CustomisedButtonSettings(Properties.Settings.Default.customizableButtonBase);
                customButtonSettings.IsCustomised = false;
            }

            SetCustomFlagsetButtonProperties(button, customButtonSettings);

            button.MouseUp += CustomFlagsetButtonOnClick;
        }
    }

    /// <summary>
    /// Handle the left/right clicks on the custom flagset buttons
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CustomFlagsetButtonOnClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        var button = (Button)sender;

        if (e.Button == MouseButtons.Right)
        {
            // Show context menu
            customisableButtonContextMenu.Show(button, new Point(e.X, e.Y));
        }
        else
        {
            var customButtonSettings = (CustomisedButtonSettings)button.Tag;
            if (customButtonSettings.IsCustomised)
            {
                // Update the flags textbox with the custom flagset
                flagsTextBox.Text = customButtonSettings.Flagset;
            }
        }
    }


    private void CustomFlagsetButtonContextMenuOnClick(object sender, System.EventArgs e)
    {
        var menuItem = (ToolStripMenuItem)sender;
        var button = (Button)customisableButtonContextMenu.SourceControl;
        CustomisedButtonSettings customButtonSettings;

        switch (menuItem.Text.ToLowerInvariant())
        {
            case "edit":
                {
                    var customiseButtonFlagsetForm = new CustomiseButtonFlagsetForm(button, flagsTextBox.Text);
                    customiseButtonFlagsetForm.ShowDialog();
                    if (customiseButtonFlagsetForm.DialogResult == DialogResult.OK)
                    {
                        customButtonSettings = (CustomisedButtonSettings)button.Tag;
                        SetCustomFlagsetButtonProperties(button, customButtonSettings);
                        Properties.Settings.Default[button.Name] = customButtonSettings.Export();
                        Properties.Settings.Default.Save();
                    }
                    break;
                }
            case "copy":
                {
                    customButtonSettings = (CustomisedButtonSettings)button.Tag;
                    if (customButtonSettings.IsCustomised)
                        Clipboard.SetText(customButtonSettings.Flagset);
                    break;
                }
            case "paste":
                {
                    // not sure what we will be doing in here yet, if anything
                    // could possibly let someone paste a flagset into a button and immediately popop the custom flagset editor
                    // would help if there was some way to detect if the clipboard contains a valid flagset
                    break;
                }
            case "clear":
                {
                    customButtonSettings = (CustomisedButtonSettings)button.Tag;
                    if (customButtonSettings.IsCustomised)
                    {
                        var result = MessageBox.Show("Are you sure you want to clear this custom Flagset?", "Confirm"
                                , MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Yes)
                        {
                            customButtonSettings = new CustomisedButtonSettings(Properties.Settings.Default.customizableButtonBase);
                            customButtonSettings.IsCustomised = false;
                            SetCustomFlagsetButtonProperties(button, customButtonSettings);
                            Properties.Settings.Default[button.Name] = new System.Collections.Specialized.StringCollection();
                            Properties.Settings.Default.Save();
                        }
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Set the properties of a custom flagset button
    /// </summary>
    /// <param name="button"></param>
    /// <param name="customButtonSettings"></param>
    private void SetCustomFlagsetButtonProperties(Button button, CustomisedButtonSettings customButtonSettings)
    {

        // found that this was being repeated a lot, so made it a function


        button.Text = customButtonSettings.Name;
        button.Tag = customButtonSettings;
        toolTip1.SetToolTip(button, customButtonSettings.Tooltip);

        // couldn't really find a nice way to turn off the wordwrap on a button, and the autoellipsis, would still cause a wordwrap
        // which then shifted the button text up a little so it was out of line with the other buttons.
        // so we'll just do ellipsis ourselves
        // if we are ellipsing we'll shove the full button name as line 1 of the tooltip
        while (button.IsEllipsisShown())
        {
            button.Text = button.Text.Substring(0, button.Text.Length - 4) + "...";
            toolTip1.SetToolTip(button, customButtonSettings.Name +
                    (!string.IsNullOrWhiteSpace(customButtonSettings.Tooltip) ? Environment.NewLine + customButtonSettings.Tooltip : string.Empty));
        }

        if (!customButtonSettings.IsCustomised)
        {
            // set some indicator that this button is not customised
            button.ForeColor = SystemColors.GrayText;
        }
        else
        {
            button.ForeColor = SystemColors.WindowText;
        }
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

    private void TryLoadSpriteImageFromFile(string fileName)
    {
        // Quick file checks to make sure we can load the rom
        if (!File.Exists(fileName))
        {
            return;
        }
        FileInfo? rom;
        try
        {
            rom = new FileInfo(fileName);
        }
        catch (Exception _ex) { return; }

        // basic validation that they selected a "validish" rom before drawing a sprite from it
        if (rom?.Length == 0x10 + 128 * 1024 + 128 * 1024)
            _spritePreview = new SpritePreview(fileName);
        GenerateSpriteImage();
    }

    private void fileBtn_Click(object sender, EventArgs e)
    {
        var FD = new System.Windows.Forms.OpenFileDialog();
        if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            romFileTextBox.Text = FD.FileName;
            TryLoadSpriteImageFromFile(FD.FileName);
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
        Properties.Settings.Default.beepFrequency = beepFrequencyDropdown.SelectedIndex;
        Properties.Settings.Default.beepThreshold = beepThresholdDropdown.SelectedIndex;
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
            MessageBox.Show("An exception occurred generating the rom: \n" + generationException.Message);
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
        includeLavaInShuffleCheckBox.Enabled = shuffleEncountersCheckbox.Checked;
        if (shuffleEncountersCheckbox.CheckState != CheckState.Checked)
        {
            allowPathEnemiesCheckbox.Checked = false;
            includeLavaInShuffleCheckBox.Checked = false;
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
        configuration.IncludeLavaInEncounterShuffle = includeLavaInShuffleCheckBox.Checked;
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
        configuration.Climate = climateSelector.SelectedIndex switch
        {
            0 => Climates.Classic,
            1 => Climates.Chaos,
            _ => throw new Exception("Invalid Climate setting")
        };
        configuration.WestBiome = westBiomeSelector.SelectedIndex switch
        {
            0 => Biome.VANILLA,
            1 => Biome.VANILLA_SHUFFLE,
            2 => Biome.VANILLALIKE,
            3 => Biome.ISLANDS,
            4 => Biome.CANYON,
            5 => Biome.CALDERA,
            6 => Biome.MOUNTAINOUS,
            7 => Biome.RANDOM,
            8 => Biome.RANDOM_NO_VANILLA,
            9 => Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
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
            7 => Biome.RANDOM,
            8 => Biome.RANDOM_NO_VANILLA,
            9 => Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
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
            7 => Biome.RANDOM,
            8 => Biome.RANDOM_NO_VANILLA,
            9 => Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
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
        configuration.NormalPalaceStyle = palaceStyleList.SelectedIndex switch
        {
            0 => PalaceStyle.VANILLA,
            1 => PalaceStyle.SHUFFLED,
            2 => PalaceStyle.RECONSTRUCTED,
            3 => PalaceStyle.RANDOM,
            _ => throw new Exception("Invalid PalaceStyle setting")
        };
        configuration.GPStyle = gpStyleList.SelectedIndex switch
        {
            0 => PalaceStyle.VANILLA,
            1 => PalaceStyle.SHUFFLED,
            2 => PalaceStyle.RECONSTRUCTED,
            3 => PalaceStyle.RECONSTRUCTED_SHORTENED,
            4 => PalaceStyle.RANDOM,
            _ => throw new Exception("Invalid GP Style setting")
        };
        configuration.IncludeVanillaRooms = GetTripleCheckState(includeVanillaRoomsCheckbox);
        configuration.Includev4_0Rooms = GetTripleCheckState(includev4_0RoomsCheckbox);
        configuration.Includev4_4Rooms = GetTripleCheckState(includev4_4RoomsCheckbox);
        configuration.BlockingRoomsInAnyPalace = blockingRoomsInAnyPalaceCheckbox.Checked;
        configuration.BossRoomsExitToPalace = GetTripleCheckState(bossRoomsExitToPalaceCheckbox);
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
        configuration.BeepFrequency = beepFrequencyDropdown.SelectedIndex switch
        {
            //Normal
            0 => 0x30,
            //Half Speed
            1 => 0x60,
            //Quarter Speed
            2 => 0xC0,
            //Off
            3 => 0,
            _ => 0x30
        };

        configuration.BeepThreshold = beepThresholdDropdown.SelectedIndex switch
        {
            //Normal
            0 => 0x20,
            //Half Bar
            1 => 0x10,
            //Quarter Bar
            2 => 0x08,
            //Two Bars
            3 => 0x40,
            _ => 0x20
        };

        configuration.Tunic = tunicColorList.GetItemText(tunicColorList.SelectedItem);
        configuration.ShieldTunic = shieldColorList.GetItemText(shieldColorList.SelectedItem);
        configuration.BeamSprite = beamSpriteList.GetItemText(beamSpriteList.SelectedItem);

        return configuration;
    }

    private void FlagBox_Changed(object sender, EventArgs e)
    {
        if (flagsTextBox.Text != flagsTextBox.Text.Trim())
        {
            flagsTextBox.Text = flagsTextBox.Text.Trim();
        }
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
        if (oldFlags.Length == 0)
        {
            return;
        }
        var BeepFrequency = beepFrequencyDropdown.SelectedIndex;
        var BeepThreshold = beepThresholdDropdown.SelectedIndex;
        var DisableMusic = disableMusicCheckbox.CheckState;
        var FastSpellCasting = fastSpellCheckbox.CheckState;
        var UpAOnController1 = upAOnController1Checkbox.CheckState;
        var RemoveFlashing = flashingOffCheckbox.CheckState;
        var Sprite = characterSpriteList.SelectedIndex;
        var Tunic = tunicColorList.SelectedIndex;
        var ShieldTunic = shieldColorList.SelectedIndex;
        var BeamSprite = beamSpriteList.SelectedIndex;

        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags(oldFlags);

        beepFrequencyDropdown.SelectedIndex = BeepFrequency;
        beepThresholdDropdown.SelectedIndex = BeepThreshold;
        disableMusicCheckbox.CheckState = DisableMusic;
        fastSpellCheckbox.CheckState = FastSpellCasting;
        upAOnController1Checkbox.CheckState = UpAOnController1;
        flashingOffCheckbox.CheckState = RemoveFlashing;
        characterSpriteList.SelectedIndex = Sprite;
        tunicColorList.SelectedIndex = Tunic;
        shieldColorList.SelectedIndex = ShieldTunic;
        beamSpriteList.SelectedIndex = BeamSprite;

        flagsTextBox.Text = config.Serialize();
    }

    private void FlagBox_TextChanged(object sender, EventArgs eventArgs)
    {
        dontrunhandler = true;
        flagsTextBox.Text = flagsTextBox.Text.Trim();
        if (flagsTextBox.Text.Length == 0)
        {
            return;
        }
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
            includeLavaInShuffleCheckBox.CheckState = ToCheckState(configuration.IncludeLavaInEncounterShuffle);
            shuffleEncountersCheckbox.CheckState = ToCheckState(configuration.ShuffleEncounters);
            allowPathEnemiesCheckbox.CheckState = ToCheckState(configuration.AllowUnsafePathEncounters);
            encounterRateBox.SelectedIndex = configuration.EncounterRate switch
            {
                EncounterRate.NORMAL => 0,
                EncounterRate.HALF => 1,
                EncounterRate.NONE => 2,
                EncounterRate.RANDOM => 3,
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
            climateSelector.SelectedIndex = configuration.Climate.Name switch
            {
                "Classic" => 0,
                "Chaos" => 1,
                _ => throw new Exception("Invalid Climate setting")
            };
            westBiomeSelector.SelectedIndex = configuration.WestBiome switch
            {
                Biome.VANILLA => 0,
                Biome.VANILLA_SHUFFLE => 1,
                Biome.VANILLALIKE => 2,
                Biome.ISLANDS => 3,
                Biome.CANYON => 4,
                Biome.CALDERA => 5,
                Biome.MOUNTAINOUS => 6,
                Biome.RANDOM => 7,
                Biome.RANDOM_NO_VANILLA => 8,
                Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 9,
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
                Biome.RANDOM => 7,
                Biome.RANDOM_NO_VANILLA => 8,
                Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 9,
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
                Biome.RANDOM => 7,
                Biome.RANDOM_NO_VANILLA => 8,
                Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 9,
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
            palaceStyleList.SelectedIndex = configuration.NormalPalaceStyle switch
            {
                PalaceStyle.VANILLA => 0,
                PalaceStyle.SHUFFLED => 1,
                PalaceStyle.RECONSTRUCTED => 2,
                PalaceStyle.RANDOM => 3,
                _ => throw new Exception("Invalid PalaceStyle setting")
            };
            gpStyleList.SelectedIndex = configuration.GPStyle switch
            {
                PalaceStyle.VANILLA => 0,
                PalaceStyle.SHUFFLED => 1,
                PalaceStyle.RECONSTRUCTED => 2,
                PalaceStyle.RECONSTRUCTED_SHORTENED => 3,
                PalaceStyle.RANDOM => 4,
                _ => throw new Exception("Invalid PalaceStyle setting")
            };
            includeVanillaRoomsCheckbox.CheckState = ToCheckState(configuration.IncludeVanillaRooms);
            includev4_0RoomsCheckbox.CheckState = ToCheckState(configuration.Includev4_0Rooms);
            includev4_4RoomsCheckbox.CheckState = ToCheckState(configuration.Includev4_4Rooms);
            blockingRoomsInAnyPalaceCheckbox.Checked = configuration.BlockingRoomsInAnyPalace;
            bossRoomsExitToPalaceCheckbox.CheckState = ToCheckState(configuration.BossRoomsExitToPalace);
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
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("jhEhMROm7DZ+MHRBTNBhBAh0PSmA");
        flagsTextBox.Text = config.Serialize();
    }

    private void StandardFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("hAhhD0j9+78+Jp5++gAhOAdEScuA");
        flagsTextBox.Text = config.Serialize();
    }

    private void MaxRandoFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = RandomizerConfiguration.FromLegacyFlags("iyAqh+j#g7@+ZqTBT!BhOA!0P@@A");
        flagsTextBox.Text = config.Serialize();
    }

    private void RandomPercentFlags(object sender, EventArgs e)
    {
        RandomizerConfiguration config = new("hEAK0sALvrpUWVXu20Y+8v9ttf9tb7AAJy");
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
            if (!FlagBuilder.ENCODING_TABLE.Contains(flagString[i]))
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
        if (removeSpellitemsCheckbox.CheckState == CheckState.Checked)
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
        if (VanillaPossible(eastBiome) || VanillaPossible(westBiomeSelector) || VanillaPossible(dmBiome) || VanillaPossible(mazeBiome))
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
        if (westBiomeSelector.SelectedIndex == 0 || westBiomeSelector.SelectedIndex == 1)
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
            includeVanillaRoomsCheckbox.Enabled = false;
            includeVanillaRoomsCheckbox.Checked = true;
            includev4_0RoomsCheckbox.Enabled = false;
            includev4_0RoomsCheckbox.Checked = false;
            includev4_4RoomsCheckbox.Enabled = false;
            includev4_4RoomsCheckbox.Checked = false;
            blockingRoomsInAnyPalaceCheckbox.Enabled = false;
            blockingRoomsInAnyPalaceCheckbox.Checked = false;
            bossRoomsExitToPalaceCheckbox.Checked = false;
            bossRoomsExitToPalaceCheckbox.Enabled = false;
            noDuplicateRoomsCheckbox.Checked = false;
            noDuplicateRoomsCheckbox.Enabled = false;
        }
        else
        {
            includeVanillaRoomsCheckbox.Enabled = true;
            includev4_0RoomsCheckbox.Enabled = true;
            includev4_4RoomsCheckbox.Enabled = true;
            blockingRoomsInAnyPalaceCheckbox.Enabled = true;
            bossRoomsExitToPalaceCheckbox.Enabled = true;
            noDuplicateRoomsCheckbox.Enabled = true;
        }

        if (palaceStyleList.SelectedIndex != 0)
        {
            tbirdRequiredCheckbox.Enabled = true;
        }
        else
        {
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
            if (!FlagBuilder.ENCODING_TABLE.Contains(flagString[i]))
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

    private void GenerateSpriteImage()
    {
        _spritePreview?.ReloadSpriteFromROM(
               CharacterSprite.ByIndex(characterSpriteList.SelectedIndex),
               tunicColorList.GetItemText(tunicColorList.SelectedItem),
               shieldColorList.GetItemText(shieldColorList.SelectedItem),
               beamSpriteList.GetItemText(beamSpriteList.SelectedItem)
           );
        spritePreviewBox.Image = _spritePreview?.Preview;
        spriteCreditLabel.Text = _spritePreview?.Credit;
    }

    private void characterSpriteList_SelectedIndexChanged(object sender, EventArgs e)
    {
        GenerateSpriteImage();
    }

    private void tunicColorList_SelectedIndexChanged(object sender, EventArgs e)
    {
        GenerateSpriteImage();
    }

    private void shieldColorList_SelectedIndexChanged(object sender, EventArgs e)
    {
        GenerateSpriteImage();
    }

    private void beamSpriteList_SelectedIndexChanged(object sender, EventArgs e)
    {
        GenerateSpriteImage();
    }
}

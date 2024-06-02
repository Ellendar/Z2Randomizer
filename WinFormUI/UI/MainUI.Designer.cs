using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Z2Randomizer.WinFormUI;

partial class MainUI
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        mainTabControl = new TabControl();
        tabPage4 = new TabPage();
        StartingLivesLabel = new Label();
        startingLivesBox = new ComboBox();
        label3 = new Label();
        label2 = new Label();
        label1 = new Label();
        maxHeartsList = new ComboBox();
        startHeartsMaxList = new ComboBox();
        startingLevelsLabel = new Label();
        startingLifeLevelList = new ComboBox();
        startingMagicLevelList = new ComboBox();
        startingAttackLevelList = new ComboBox();
        startingLifeLabel = new Label();
        startingMagicLabel = new Label();
        startingAttackLabel = new Label();
        startingTechsLabel = new Label();
        startingTechsList = new ComboBox();
        startingHeartContainersLabel = new Label();
        startHeartsMinList = new ComboBox();
        groupBox1 = new GroupBox();
        startWIthThunderCheckbox = new CheckBox();
        startWithSpellCheckbox = new CheckBox();
        startWithReflectCheckbox = new CheckBox();
        startWithFireCheckbox = new CheckBox();
        startWithFairyCheckbox = new CheckBox();
        startWithLifeCheckbox = new CheckBox();
        startWithJumpCheckbox = new CheckBox();
        startWithShieldCheckbox = new CheckBox();
        shuffleStartingSpellsCheckbox = new CheckBox();
        itemGrp = new GroupBox();
        startWithMagicKeyCheckbox = new CheckBox();
        startWithHammerCheckbox = new CheckBox();
        startWithCrossCheckbox = new CheckBox();
        startWithFluteCheckbox = new CheckBox();
        startWithBootsCheckbox = new CheckBox();
        startWithRaftCheckbox = new CheckBox();
        startWithGloveCheckbox = new CheckBox();
        startWithCandleCheckbox = new CheckBox();
        shuffleStartingItemsCheckbox = new CheckBox();
        tabPage1 = new TabPage();
        westBiomeSelector = new ComboBox();
        westBiomeLabel = new Label();
        includeLavaInShuffleCheckBox = new CheckBox();
        generateBaguWoodsCheckbox = new CheckBox();
        useGoodBootsCheckbox = new CheckBox();
        shuffleWhichLocationsAreHiddenCheckbox = new CheckBox();
        shuffledVanillaShowsActualTerrain = new CheckBox();
        mazeBiome = new ComboBox();
        mazeIslandBiomeLabel = new Label();
        eastBiome = new ComboBox();
        dmBiome = new ComboBox();
        climateSelector = new ComboBox();
        eastContinentBindingLabel = new Label();
        deathMountainBiomeLabel = new Label();
        label39 = new Label();
        climateLabel = new Label();
        allowBoulderBlockedConnectionsCheckbox = new CheckBox();
        saneCaveShuffleBox = new CheckBox();
        hideLessImportantLocationsCheckbox = new CheckBox();
        ContinentConnectionLabel = new Label();
        continentConnectionBox = new ComboBox();
        label36 = new Label();
        encounterRateBox = new ComboBox();
        encounterRateLabel = new Label();
        hideKasutoList = new ComboBox();
        hiddenKasutoLabel = new Label();
        hiddenPalaceList = new ComboBox();
        hiddenPalaceLabel = new Label();
        includeGPinShuffleCheckbox = new CheckBox();
        allowPalaceContinentSwapCheckbox = new CheckBox();
        label4 = new Label();
        allowPathEnemiesCheckbox = new CheckBox();
        shuffleEncountersCheckbox = new CheckBox();
        tabPage2 = new TabPage();
        shortenGPCheckbox = new CheckBox();
        shortenNormalPalaceCheckbox = new CheckBox();
        ShortenPalaceLabel = new Label();
        useCustomRoomsBox = new CheckBox();
        HardBossesCheckbox = new CheckBox();
        noDuplicateRoomsByEnemiesCheckbox = new CheckBox();
        gpStyleLabel = new Label();
        gpStyleList = new ComboBox();
        includev4_4RoomsCheckbox = new CheckBox();
        includev4_0RoomsCheckbox = new CheckBox();
        noDuplicateRoomsByLayoutCheckbox = new CheckBox();
        label7 = new Label();
        label5 = new Label();
        startingGemsMaxList = new ComboBox();
        palaceStyleLabel = new Label();
        palaceStyleList = new ComboBox();
        bossRoomsExitToPalaceCheckbox = new CheckBox();
        blockingRoomsInAnyPalaceCheckbox = new CheckBox();
        includeVanillaRoomsCheckbox = new CheckBox();
        randomizeBossItemCheckbox = new CheckBox();
        removeTbirdCheckbox = new CheckBox();
        restartAtPalacesCheckbox = new CheckBox();
        palacePaletteCheckbox = new CheckBox();
        tbirdRequiredCheckbox = new CheckBox();
        label6 = new Label();
        startingGemsMinList = new ComboBox();
        tabPage5 = new TabPage();
        lifeEffectivenessList = new ComboBox();
        magicEffectivenessList = new ComboBox();
        attackEffectivenessList = new ComboBox();
        scaleLevelRequirementsToCapCheckbox = new CheckBox();
        levelCapLabel = new Label();
        lifeCapList = new ComboBox();
        magCapList = new ComboBox();
        atkCapList = new ComboBox();
        lifeCapLabel = new Label();
        magCapLabel = new Label();
        attackCapLabel = new Label();
        label12 = new Label();
        magicEffectivenessLabel = new Label();
        attackEffectivenessLabel = new Label();
        expBox = new GroupBox();
        lifeExpNeededCheckbox = new CheckBox();
        magicExpNeededCheckbox = new CheckBox();
        shuffleAtkExpNeededCheckbox = new CheckBox();
        shuffleAllExpCheckbox = new CheckBox();
        tabPage9 = new TabPage();
        dashAlwaysOnCheckbox = new CheckBox();
        jumpAlwaysOnCheckbox = new CheckBox();
        FireSpellOptionLabel = new Label();
        FireSpellBox = new ComboBox();
        swapUpAndDownstabCheckbox = new CheckBox();
        randomizeSpellSpellEnemyCheckbox = new CheckBox();
        disableMagicContainerRequirementCheckbox = new CheckBox();
        shuffleSpellLocationsCheckbox = new CheckBox();
        shuffleLifeRefillCheckbox = new CheckBox();
        tabPage6 = new TabPage();
        generatorsMatchCheckBox = new CheckBox();
        enemyExperienceDropsLabel = new Label();
        experienceDropsList = new ComboBox();
        shuffleDripperEnemyCheckbox = new CheckBox();
        mixLargeAndSmallCheckbox = new CheckBox();
        label8 = new Label();
        shufflePalaceEnemiesCheckbox = new CheckBox();
        shuffleOverworldEnemiesCheckbox = new CheckBox();
        shuffleSwordImmunityBox = new CheckBox();
        shuffleStealXPAmountCheckbox = new CheckBox();
        shuffleXPStealersCheckbox = new CheckBox();
        shuffleEnemyHPBox = new CheckBox();
        tabPage7 = new TabPage();
        includeQuestItemsInShuffleCheckbox = new CheckBox();
        includeSwordTechsInShuffleCheckbox = new CheckBox();
        includeSpellsInShuffleCheckbox = new CheckBox();
        shufflePbagAmountsCheckbox = new CheckBox();
        removeSpellitemsCheckbox = new CheckBox();
        includePbagCavesInShuffleCheckbox = new CheckBox();
        randomizeJarRequirementsCheckbox = new CheckBox();
        palacesHaveExtraKeysCheckbox = new CheckBox();
        shuffleSmallItemsCheckbox = new CheckBox();
        mixOverworldPalaceItemsCheckbox = new CheckBox();
        shuffleOverworldItemsCheckbox = new CheckBox();
        shufflePalaceItemsCheckbox = new CheckBox();
        tabPage8 = new TabPage();
        randomizeDropsCheckbox = new CheckBox();
        standardizeDropsCheckbox = new CheckBox();
        largeEnemiesKeyCheckbox = new CheckBox();
        largeEnemies1UpCheckbox = new CheckBox();
        largeEnemiesXLBagCheckbox = new CheckBox();
        largeEnemiesLargeBagCheckbox = new CheckBox();
        largeEnemiesMediumBagCheckbox = new CheckBox();
        largeEnemiesSmallBagCheckbox = new CheckBox();
        largeEnemiesRedJarCheckbox = new CheckBox();
        largeEnemiesBlueJarCheckbox = new CheckBox();
        largeEnemyPoolLabel = new Label();
        smallEnemiesKeyCheckbox = new CheckBox();
        smallEnemies1UpCheckbox = new CheckBox();
        smallEnemiesXLBagCheckbox = new CheckBox();
        smallEnemiesLargeBagCheckbox = new CheckBox();
        smallEnemiesMediumBagCheckbox = new CheckBox();
        smallEnemiesSmallBagCheckbox = new CheckBox();
        smallEnemiesRedJarCheckbox = new CheckBox();
        smallEnemiesBlueJarCheckbox = new CheckBox();
        smallEnemyPoolLabel = new Label();
        label19 = new Label();
        shuffleDropFrequencyCheckbox = new CheckBox();
        miscTabPage = new TabPage();
        randomizeKnockbackCheckbox = new CheckBox();
        shuffleEnemyPalettesCheckbox = new CheckBox();
        alwaysBeamCheckbox = new CheckBox();
        enableTownNameHintsCheckbox = new CheckBox();
        enableSpellItemHintsCheckbox = new CheckBox();
        enableHelpfulHintsCheckbox = new CheckBox();
        CustomTabPage = new TabPage();
        disableHUDLag = new CheckBox();
        useCommunityTextCheckbox = new CheckBox();
        spriteCreditLabel = new Label();
        beepFrequencyDropdown = new ComboBox();
        label10 = new Label();
        beepThresholdDropdown = new ComboBox();
        label9 = new Label();
        spritePreviewLabel = new Label();
        spritePreviewBox = new PictureBox();
        flashingOffCheckbox = new CheckBox();
        upAOnController1Checkbox = new CheckBox();
        beamSpriteList = new ComboBox();
        beamSpriteLabel = new Label();
        shieldColorList = new ComboBox();
        shieldColorLabel = new Label();
        tunicColorList = new ComboBox();
        tunicColorLabel = new Label();
        characterSpriteList = new ComboBox();
        characterSpriteLabel = new Label();
        disableMusicCheckbox = new CheckBox();
        fastSpellCheckbox = new CheckBox();
        romFileTextBox = new TextBox();
        seedTextBox = new TextBox();
        romFileLabel = new Label();
        seedLabel = new Label();
        createSeedButton = new Button();
        romFileBrowseButton = new Button();
        generateRomButton = new Button();
        flagsTextBox = new TextBox();
        updateButton = new Button();
        flagsLabel = new Label();
        toolTip1 = new ToolTip(components);
        wikiButton = new Button();
        customizableButton1 = new Button();
        customizableButton2 = new Button();
        customizableButton3 = new Button();
        customizableButton4 = new Button();
        customizableButton5 = new Button();
        customizableButton6 = new Button();
        discordButton = new Button();
        oldFlagsTextbox = new TextBox();
        convertButton = new Button();
        backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
        oldFlagsLabel = new Label();
        batchButton = new Button();
        customisableButtonContextMenu = new ContextMenuStrip(components);
        mainTabControl.SuspendLayout();
        tabPage4.SuspendLayout();
        groupBox1.SuspendLayout();
        itemGrp.SuspendLayout();
        tabPage1.SuspendLayout();
        tabPage2.SuspendLayout();
        tabPage5.SuspendLayout();
        expBox.SuspendLayout();
        tabPage9.SuspendLayout();
        tabPage6.SuspendLayout();
        tabPage7.SuspendLayout();
        tabPage8.SuspendLayout();
        miscTabPage.SuspendLayout();
        CustomTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)spritePreviewBox).BeginInit();
        SuspendLayout();
        // 
        // mainTabControl
        // 
        mainTabControl.Controls.Add(tabPage4);
        mainTabControl.Controls.Add(tabPage1);
        mainTabControl.Controls.Add(tabPage2);
        mainTabControl.Controls.Add(tabPage5);
        mainTabControl.Controls.Add(tabPage9);
        mainTabControl.Controls.Add(tabPage6);
        mainTabControl.Controls.Add(tabPage7);
        mainTabControl.Controls.Add(tabPage8);
        mainTabControl.Controls.Add(miscTabPage);
        mainTabControl.Controls.Add(CustomTabPage);
        mainTabControl.Location = new Point(15, 139);
        mainTabControl.Margin = new Padding(4, 3, 4, 3);
        mainTabControl.Name = "mainTabControl";
        mainTabControl.SelectedIndex = 0;
        mainTabControl.Size = new Size(603, 359);
        mainTabControl.TabIndex = 0;
        // 
        // tabPage4
        // 
        tabPage4.Controls.Add(StartingLivesLabel);
        tabPage4.Controls.Add(startingLivesBox);
        tabPage4.Controls.Add(label3);
        tabPage4.Controls.Add(label2);
        tabPage4.Controls.Add(label1);
        tabPage4.Controls.Add(maxHeartsList);
        tabPage4.Controls.Add(startHeartsMaxList);
        tabPage4.Controls.Add(startingLevelsLabel);
        tabPage4.Controls.Add(startingLifeLevelList);
        tabPage4.Controls.Add(startingMagicLevelList);
        tabPage4.Controls.Add(startingAttackLevelList);
        tabPage4.Controls.Add(startingLifeLabel);
        tabPage4.Controls.Add(startingMagicLabel);
        tabPage4.Controls.Add(startingAttackLabel);
        tabPage4.Controls.Add(startingTechsLabel);
        tabPage4.Controls.Add(startingTechsList);
        tabPage4.Controls.Add(startingHeartContainersLabel);
        tabPage4.Controls.Add(startHeartsMinList);
        tabPage4.Controls.Add(groupBox1);
        tabPage4.Controls.Add(itemGrp);
        tabPage4.Location = new Point(4, 24);
        tabPage4.Margin = new Padding(4, 3, 4, 3);
        tabPage4.Name = "tabPage4";
        tabPage4.Size = new Size(595, 331);
        tabPage4.TabIndex = 3;
        tabPage4.Text = "Start Configuration";
        tabPage4.UseVisualStyleBackColor = true;
        // 
        // StartingLivesLabel
        // 
        StartingLivesLabel.AutoSize = true;
        StartingLivesLabel.Location = new Point(400, 256);
        StartingLivesLabel.Margin = new Padding(4, 0, 4, 0);
        StartingLivesLabel.Name = "StartingLivesLabel";
        StartingLivesLabel.Size = new Size(77, 15);
        StartingLivesLabel.TabIndex = 35;
        StartingLivesLabel.Text = "Starting Lives";
        // 
        // startingLivesBox
        // 
        startingLivesBox.DropDownStyle = ComboBoxStyle.DropDownList;
        startingLivesBox.FormattingEnabled = true;
        startingLivesBox.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "8", "16", "Random(2-5)" });
        startingLivesBox.Location = new Point(402, 274);
        startingLivesBox.Margin = new Padding(4, 3, 4, 3);
        startingLivesBox.Name = "startingLivesBox";
        startingLivesBox.Size = new Size(140, 23);
        startingLivesBox.TabIndex = 34;
        toolTip1.SetToolTip(startingLivesBox, "Number of lives you start with.");
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(505, 82);
        label3.Margin = new Padding(4, 0, 4, 0);
        label3.Name = "label3";
        label3.Size = new Size(32, 30);
        label3.TabIndex = 33;
        label3.Text = "Seed\r\nMax";
        label3.TextAlign = ContentAlignment.MiddleCenter;
        toolTip1.SetToolTip(label3, "Starting Attack Level");
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(454, 82);
        label2.Margin = new Padding(4, 0, 4, 0);
        label2.Name = "label2";
        label2.Size = new Size(31, 30);
        label2.TabIndex = 32;
        label2.Text = "Start\r\nMax";
        label2.TextAlign = ContentAlignment.MiddleCenter;
        toolTip1.SetToolTip(label2, "Starting Attack Level");
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(406, 82);
        label1.Margin = new Padding(4, 0, 4, 0);
        label1.Name = "label1";
        label1.Size = new Size(31, 30);
        label1.TabIndex = 31;
        label1.Text = "Start\r\nMin";
        label1.TextAlign = ContentAlignment.MiddleCenter;
        toolTip1.SetToolTip(label1, "Starting Attack Level");
        // 
        // maxHeartsList
        // 
        maxHeartsList.DropDownStyle = ComboBoxStyle.DropDownList;
        maxHeartsList.DropDownWidth = 40;
        maxHeartsList.FormattingEnabled = true;
        maxHeartsList.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "?", "+1", "+2", "+3" });
        maxHeartsList.Location = new Point(500, 56);
        maxHeartsList.Margin = new Padding(4, 3, 4, 3);
        maxHeartsList.MaxDropDownItems = 12;
        maxHeartsList.Name = "maxHeartsList";
        maxHeartsList.Size = new Size(40, 23);
        maxHeartsList.TabIndex = 30;
        toolTip1.SetToolTip(maxHeartsList, "The number of heart containers you start with");
        // 
        // startHeartsMaxList
        // 
        startHeartsMaxList.DropDownStyle = ComboBoxStyle.DropDownList;
        startHeartsMaxList.DropDownWidth = 40;
        startHeartsMaxList.FormattingEnabled = true;
        startHeartsMaxList.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "?" });
        startHeartsMaxList.Location = new Point(450, 56);
        startHeartsMaxList.Margin = new Padding(4, 3, 4, 3);
        startHeartsMaxList.Name = "startHeartsMaxList";
        startHeartsMaxList.Size = new Size(40, 23);
        startHeartsMaxList.TabIndex = 29;
        toolTip1.SetToolTip(startHeartsMaxList, "The number of heart containers you start with");
        // 
        // startingLevelsLabel
        // 
        startingLevelsLabel.AutoSize = true;
        startingLevelsLabel.Location = new Point(427, 129);
        startingLevelsLabel.Margin = new Padding(4, 0, 4, 0);
        startingLevelsLabel.Name = "startingLevelsLabel";
        startingLevelsLabel.Size = new Size(78, 15);
        startingLevelsLabel.TabIndex = 28;
        startingLevelsLabel.Text = "Starting Level";
        // 
        // startingLifeLevelList
        // 
        startingLifeLevelList.DropDownStyle = ComboBoxStyle.DropDownList;
        startingLifeLevelList.FormattingEnabled = true;
        startingLifeLevelList.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        startingLifeLevelList.Location = new Point(500, 151);
        startingLifeLevelList.Margin = new Padding(4, 3, 4, 3);
        startingLifeLevelList.Name = "startingLifeLevelList";
        startingLifeLevelList.Size = new Size(40, 23);
        startingLifeLevelList.TabIndex = 27;
        toolTip1.SetToolTip(startingLifeLevelList, "Starting Life Level");
        // 
        // startingMagicLevelList
        // 
        startingMagicLevelList.DropDownStyle = ComboBoxStyle.DropDownList;
        startingMagicLevelList.DropDownWidth = 40;
        startingMagicLevelList.FormattingEnabled = true;
        startingMagicLevelList.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        startingMagicLevelList.Location = new Point(450, 151);
        startingMagicLevelList.Margin = new Padding(4, 3, 4, 3);
        startingMagicLevelList.Name = "startingMagicLevelList";
        startingMagicLevelList.Size = new Size(40, 23);
        startingMagicLevelList.TabIndex = 26;
        toolTip1.SetToolTip(startingMagicLevelList, "Starting Magic Level");
        // 
        // startingAttackLevelList
        // 
        startingAttackLevelList.DropDownStyle = ComboBoxStyle.DropDownList;
        startingAttackLevelList.DropDownWidth = 40;
        startingAttackLevelList.FormattingEnabled = true;
        startingAttackLevelList.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        startingAttackLevelList.Location = new Point(400, 151);
        startingAttackLevelList.Margin = new Padding(4, 3, 4, 3);
        startingAttackLevelList.Name = "startingAttackLevelList";
        startingAttackLevelList.Size = new Size(40, 23);
        startingAttackLevelList.TabIndex = 25;
        toolTip1.SetToolTip(startingAttackLevelList, "Starting Attack Level");
        // 
        // startingLifeLabel
        // 
        startingLifeLabel.AutoSize = true;
        startingLifeLabel.Location = new Point(505, 177);
        startingLifeLabel.Margin = new Padding(4, 0, 4, 0);
        startingLifeLabel.Name = "startingLifeLabel";
        startingLifeLabel.Size = new Size(26, 15);
        startingLifeLabel.TabIndex = 24;
        startingLifeLabel.Text = "Life";
        toolTip1.SetToolTip(startingLifeLabel, "Starting Life Level");
        // 
        // startingMagicLabel
        // 
        startingMagicLabel.AutoSize = true;
        startingMagicLabel.Location = new Point(454, 177);
        startingMagicLabel.Margin = new Padding(4, 0, 4, 0);
        startingMagicLabel.Name = "startingMagicLabel";
        startingMagicLabel.Size = new Size(31, 15);
        startingMagicLabel.TabIndex = 23;
        startingMagicLabel.Text = "Mag";
        toolTip1.SetToolTip(startingMagicLabel, "Starting Magic Level");
        // 
        // startingAttackLabel
        // 
        startingAttackLabel.AutoSize = true;
        startingAttackLabel.Location = new Point(406, 177);
        startingAttackLabel.Margin = new Padding(4, 0, 4, 0);
        startingAttackLabel.Name = "startingAttackLabel";
        startingAttackLabel.Size = new Size(25, 15);
        startingAttackLabel.TabIndex = 22;
        startingAttackLabel.Text = "Atk";
        toolTip1.SetToolTip(startingAttackLabel, "Starting Attack Level");
        // 
        // startingTechsLabel
        // 
        startingTechsLabel.AutoSize = true;
        startingTechsLabel.Location = new Point(400, 210);
        startingTechsLabel.Margin = new Padding(4, 0, 4, 0);
        startingTechsLabel.Name = "startingTechsLabel";
        startingTechsLabel.Size = new Size(80, 15);
        startingTechsLabel.TabIndex = 14;
        startingTechsLabel.Text = "Starting Techs";
        // 
        // startingTechsList
        // 
        startingTechsList.DropDownStyle = ComboBoxStyle.DropDownList;
        startingTechsList.FormattingEnabled = true;
        startingTechsList.Items.AddRange(new object[] { "None", "Downstab", "Upstab", "Both", "Random" });
        startingTechsList.Location = new Point(402, 228);
        startingTechsList.Margin = new Padding(4, 3, 4, 3);
        startingTechsList.Name = "startingTechsList";
        startingTechsList.Size = new Size(140, 23);
        startingTechsList.TabIndex = 13;
        toolTip1.SetToolTip(startingTechsList, "The sword techniques you start with");
        // 
        // startingHeartContainersLabel
        // 
        startingHeartContainersLabel.AutoSize = true;
        startingHeartContainersLabel.Location = new Point(412, 38);
        startingHeartContainersLabel.Margin = new Padding(4, 0, 4, 0);
        startingHeartContainersLabel.Name = "startingHeartContainersLabel";
        startingHeartContainersLabel.Size = new Size(96, 15);
        startingHeartContainersLabel.TabIndex = 10;
        startingHeartContainersLabel.Text = "Heart Containers";
        // 
        // startHeartsMinList
        // 
        startHeartsMinList.DropDownStyle = ComboBoxStyle.DropDownList;
        startHeartsMinList.DropDownWidth = 40;
        startHeartsMinList.FormattingEnabled = true;
        startHeartsMinList.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "?" });
        startHeartsMinList.Location = new Point(400, 56);
        startHeartsMinList.Margin = new Padding(4, 3, 4, 3);
        startHeartsMinList.Name = "startHeartsMinList";
        startHeartsMinList.Size = new Size(40, 23);
        startHeartsMinList.TabIndex = 9;
        toolTip1.SetToolTip(startHeartsMinList, "The number of heart containers you start with");
        // 
        // groupBox1
        // 
        groupBox1.Controls.Add(startWIthThunderCheckbox);
        groupBox1.Controls.Add(startWithSpellCheckbox);
        groupBox1.Controls.Add(startWithReflectCheckbox);
        groupBox1.Controls.Add(startWithFireCheckbox);
        groupBox1.Controls.Add(startWithFairyCheckbox);
        groupBox1.Controls.Add(startWithLifeCheckbox);
        groupBox1.Controls.Add(startWithJumpCheckbox);
        groupBox1.Controls.Add(startWithShieldCheckbox);
        groupBox1.Controls.Add(shuffleStartingSpellsCheckbox);
        groupBox1.Location = new Point(204, 38);
        groupBox1.Margin = new Padding(4, 3, 4, 3);
        groupBox1.Name = "groupBox1";
        groupBox1.Padding = new Padding(4, 3, 4, 3);
        groupBox1.Size = new Size(187, 237);
        groupBox1.TabIndex = 8;
        groupBox1.TabStop = false;
        groupBox1.Text = "                                        ";
        // 
        // startWIthThunderCheckbox
        // 
        startWIthThunderCheckbox.AutoSize = true;
        startWIthThunderCheckbox.Location = new Point(27, 208);
        startWIthThunderCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWIthThunderCheckbox.Name = "startWIthThunderCheckbox";
        startWIthThunderCheckbox.Size = new Size(125, 19);
        startWIthThunderCheckbox.TabIndex = 7;
        startWIthThunderCheckbox.Text = "Start With Thunder";
        toolTip1.SetToolTip(startWIthThunderCheckbox, "Start with thunder spell");
        startWIthThunderCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithSpellCheckbox
        // 
        startWithSpellCheckbox.AutoSize = true;
        startWithSpellCheckbox.Location = new Point(27, 181);
        startWithSpellCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithSpellCheckbox.Name = "startWithSpellCheckbox";
        startWithSpellCheckbox.Size = new Size(106, 19);
        startWithSpellCheckbox.TabIndex = 6;
        startWithSpellCheckbox.Text = "Start With Spell";
        toolTip1.SetToolTip(startWithSpellCheckbox, "Start with spell spell");
        startWithSpellCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithReflectCheckbox
        // 
        startWithReflectCheckbox.AutoSize = true;
        startWithReflectCheckbox.Location = new Point(27, 155);
        startWithReflectCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithReflectCheckbox.Name = "startWithReflectCheckbox";
        startWithReflectCheckbox.Size = new Size(117, 19);
        startWithReflectCheckbox.TabIndex = 5;
        startWithReflectCheckbox.Text = "Start With Reflect";
        toolTip1.SetToolTip(startWithReflectCheckbox, "Start with reflect spell");
        startWithReflectCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithFireCheckbox
        // 
        startWithFireCheckbox.AutoSize = true;
        startWithFireCheckbox.Location = new Point(27, 128);
        startWithFireCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithFireCheckbox.Name = "startWithFireCheckbox";
        startWithFireCheckbox.Size = new Size(100, 19);
        startWithFireCheckbox.TabIndex = 4;
        startWithFireCheckbox.Text = "Start With Fire";
        toolTip1.SetToolTip(startWithFireCheckbox, "Start with fire spell");
        startWithFireCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithFairyCheckbox
        // 
        startWithFairyCheckbox.AutoSize = true;
        startWithFairyCheckbox.Location = new Point(27, 102);
        startWithFairyCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithFairyCheckbox.Name = "startWithFairyCheckbox";
        startWithFairyCheckbox.Size = new Size(106, 19);
        startWithFairyCheckbox.TabIndex = 3;
        startWithFairyCheckbox.Text = "Start With Fairy";
        toolTip1.SetToolTip(startWithFairyCheckbox, "Start with fairy spell");
        startWithFairyCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithLifeCheckbox
        // 
        startWithLifeCheckbox.AutoSize = true;
        startWithLifeCheckbox.Location = new Point(27, 75);
        startWithLifeCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithLifeCheckbox.Name = "startWithLifeCheckbox";
        startWithLifeCheckbox.Size = new Size(100, 19);
        startWithLifeCheckbox.TabIndex = 2;
        startWithLifeCheckbox.Text = "Start With Life";
        toolTip1.SetToolTip(startWithLifeCheckbox, "Start with life spell");
        startWithLifeCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithJumpCheckbox
        // 
        startWithJumpCheckbox.AutoSize = true;
        startWithJumpCheckbox.Location = new Point(27, 48);
        startWithJumpCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithJumpCheckbox.Name = "startWithJumpCheckbox";
        startWithJumpCheckbox.Size = new Size(110, 19);
        startWithJumpCheckbox.TabIndex = 1;
        startWithJumpCheckbox.Text = "Start With Jump";
        toolTip1.SetToolTip(startWithJumpCheckbox, "Start with jump spell");
        startWithJumpCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithShieldCheckbox
        // 
        startWithShieldCheckbox.AutoSize = true;
        startWithShieldCheckbox.Location = new Point(27, 22);
        startWithShieldCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithShieldCheckbox.Name = "startWithShieldCheckbox";
        startWithShieldCheckbox.Size = new Size(113, 19);
        startWithShieldCheckbox.TabIndex = 1;
        startWithShieldCheckbox.Text = "Start With Shield";
        toolTip1.SetToolTip(startWithShieldCheckbox, "Start with shield spell");
        startWithShieldCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleStartingSpellsCheckbox
        // 
        shuffleStartingSpellsCheckbox.AutoSize = true;
        shuffleStartingSpellsCheckbox.Location = new Point(7, 0);
        shuffleStartingSpellsCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleStartingSpellsCheckbox.Name = "shuffleStartingSpellsCheckbox";
        shuffleStartingSpellsCheckbox.Size = new Size(140, 19);
        shuffleStartingSpellsCheckbox.TabIndex = 1;
        shuffleStartingSpellsCheckbox.Text = "Shuffle Starting Spells";
        toolTip1.SetToolTip(shuffleStartingSpellsCheckbox, "Each spell has a 25% chance of being known");
        shuffleStartingSpellsCheckbox.UseVisualStyleBackColor = true;
        shuffleStartingSpellsCheckbox.CheckStateChanged += spellShuffleBox_CheckStateChanged;
        // 
        // itemGrp
        // 
        itemGrp.Controls.Add(startWithMagicKeyCheckbox);
        itemGrp.Controls.Add(startWithHammerCheckbox);
        itemGrp.Controls.Add(startWithCrossCheckbox);
        itemGrp.Controls.Add(startWithFluteCheckbox);
        itemGrp.Controls.Add(startWithBootsCheckbox);
        itemGrp.Controls.Add(startWithRaftCheckbox);
        itemGrp.Controls.Add(startWithGloveCheckbox);
        itemGrp.Controls.Add(startWithCandleCheckbox);
        itemGrp.Controls.Add(shuffleStartingItemsCheckbox);
        itemGrp.Location = new Point(10, 38);
        itemGrp.Margin = new Padding(4, 3, 4, 3);
        itemGrp.Name = "itemGrp";
        itemGrp.Padding = new Padding(4, 3, 4, 3);
        itemGrp.Size = new Size(187, 237);
        itemGrp.TabIndex = 0;
        itemGrp.TabStop = false;
        itemGrp.Text = "                                        ";
        // 
        // startWithMagicKeyCheckbox
        // 
        startWithMagicKeyCheckbox.AutoSize = true;
        startWithMagicKeyCheckbox.Location = new Point(27, 208);
        startWithMagicKeyCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithMagicKeyCheckbox.Name = "startWithMagicKeyCheckbox";
        startWithMagicKeyCheckbox.Size = new Size(136, 19);
        startWithMagicKeyCheckbox.TabIndex = 7;
        startWithMagicKeyCheckbox.Text = "Start With Magic Key";
        toolTip1.SetToolTip(startWithMagicKeyCheckbox, "Start with the magic key");
        startWithMagicKeyCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithHammerCheckbox
        // 
        startWithHammerCheckbox.AutoSize = true;
        startWithHammerCheckbox.Location = new Point(27, 181);
        startWithHammerCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithHammerCheckbox.Name = "startWithHammerCheckbox";
        startWithHammerCheckbox.Size = new Size(128, 19);
        startWithHammerCheckbox.TabIndex = 6;
        startWithHammerCheckbox.Text = "Start With Hammer";
        toolTip1.SetToolTip(startWithHammerCheckbox, "Start with the hammer");
        startWithHammerCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithCrossCheckbox
        // 
        startWithCrossCheckbox.AutoSize = true;
        startWithCrossCheckbox.Location = new Point(27, 155);
        startWithCrossCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithCrossCheckbox.Name = "startWithCrossCheckbox";
        startWithCrossCheckbox.Size = new Size(110, 19);
        startWithCrossCheckbox.TabIndex = 5;
        startWithCrossCheckbox.Text = "Start With Cross";
        toolTip1.SetToolTip(startWithCrossCheckbox, "Start with the cross");
        startWithCrossCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithFluteCheckbox
        // 
        startWithFluteCheckbox.AutoSize = true;
        startWithFluteCheckbox.Location = new Point(27, 128);
        startWithFluteCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithFluteCheckbox.Name = "startWithFluteCheckbox";
        startWithFluteCheckbox.Size = new Size(107, 19);
        startWithFluteCheckbox.TabIndex = 4;
        startWithFluteCheckbox.Text = "Start With Flute";
        toolTip1.SetToolTip(startWithFluteCheckbox, "Start with the flute");
        startWithFluteCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithBootsCheckbox
        // 
        startWithBootsCheckbox.AutoSize = true;
        startWithBootsCheckbox.Location = new Point(27, 102);
        startWithBootsCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithBootsCheckbox.Name = "startWithBootsCheckbox";
        startWithBootsCheckbox.Size = new Size(111, 19);
        startWithBootsCheckbox.TabIndex = 3;
        startWithBootsCheckbox.Text = "Start With Boots";
        toolTip1.SetToolTip(startWithBootsCheckbox, "Start with the boots");
        startWithBootsCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithRaftCheckbox
        // 
        startWithRaftCheckbox.AutoSize = true;
        startWithRaftCheckbox.Location = new Point(27, 75);
        startWithRaftCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithRaftCheckbox.Name = "startWithRaftCheckbox";
        startWithRaftCheckbox.Size = new Size(102, 19);
        startWithRaftCheckbox.TabIndex = 2;
        startWithRaftCheckbox.Text = "Start With Raft";
        toolTip1.SetToolTip(startWithRaftCheckbox, "Start with the raft");
        startWithRaftCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithGloveCheckbox
        // 
        startWithGloveCheckbox.AutoSize = true;
        startWithGloveCheckbox.Location = new Point(27, 48);
        startWithGloveCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithGloveCheckbox.Name = "startWithGloveCheckbox";
        startWithGloveCheckbox.Size = new Size(111, 19);
        startWithGloveCheckbox.TabIndex = 1;
        startWithGloveCheckbox.Text = "Start With Glove";
        toolTip1.SetToolTip(startWithGloveCheckbox, "Start with the glove");
        startWithGloveCheckbox.UseVisualStyleBackColor = true;
        // 
        // startWithCandleCheckbox
        // 
        startWithCandleCheckbox.AutoSize = true;
        startWithCandleCheckbox.Location = new Point(27, 22);
        startWithCandleCheckbox.Margin = new Padding(4, 3, 4, 3);
        startWithCandleCheckbox.Name = "startWithCandleCheckbox";
        startWithCandleCheckbox.Size = new Size(118, 19);
        startWithCandleCheckbox.TabIndex = 1;
        startWithCandleCheckbox.Text = "Start With Candle";
        toolTip1.SetToolTip(startWithCandleCheckbox, "Start with candle");
        startWithCandleCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleStartingItemsCheckbox
        // 
        shuffleStartingItemsCheckbox.AccessibleDescription = "";
        shuffleStartingItemsCheckbox.AutoSize = true;
        shuffleStartingItemsCheckbox.Location = new Point(7, 0);
        shuffleStartingItemsCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleStartingItemsCheckbox.Name = "shuffleStartingItemsCheckbox";
        shuffleStartingItemsCheckbox.Size = new Size(139, 19);
        shuffleStartingItemsCheckbox.TabIndex = 1;
        shuffleStartingItemsCheckbox.Text = "Shuffle Starting Items";
        toolTip1.SetToolTip(shuffleStartingItemsCheckbox, "Each item has a 25% chance of being in your inventory");
        shuffleStartingItemsCheckbox.UseVisualStyleBackColor = true;
        shuffleStartingItemsCheckbox.CheckStateChanged += shuffleItemBox_CheckStateChanged;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(westBiomeSelector);
        tabPage1.Controls.Add(westBiomeLabel);
        tabPage1.Controls.Add(includeLavaInShuffleCheckBox);
        tabPage1.Controls.Add(generateBaguWoodsCheckbox);
        tabPage1.Controls.Add(useGoodBootsCheckbox);
        tabPage1.Controls.Add(shuffleWhichLocationsAreHiddenCheckbox);
        tabPage1.Controls.Add(shuffledVanillaShowsActualTerrain);
        tabPage1.Controls.Add(mazeBiome);
        tabPage1.Controls.Add(mazeIslandBiomeLabel);
        tabPage1.Controls.Add(eastBiome);
        tabPage1.Controls.Add(dmBiome);
        tabPage1.Controls.Add(climateSelector);
        tabPage1.Controls.Add(eastContinentBindingLabel);
        tabPage1.Controls.Add(deathMountainBiomeLabel);
        tabPage1.Controls.Add(label39);
        tabPage1.Controls.Add(climateLabel);
        tabPage1.Controls.Add(allowBoulderBlockedConnectionsCheckbox);
        tabPage1.Controls.Add(saneCaveShuffleBox);
        tabPage1.Controls.Add(hideLessImportantLocationsCheckbox);
        tabPage1.Controls.Add(ContinentConnectionLabel);
        tabPage1.Controls.Add(continentConnectionBox);
        tabPage1.Controls.Add(label36);
        tabPage1.Controls.Add(encounterRateBox);
        tabPage1.Controls.Add(encounterRateLabel);
        tabPage1.Controls.Add(hideKasutoList);
        tabPage1.Controls.Add(hiddenKasutoLabel);
        tabPage1.Controls.Add(hiddenPalaceList);
        tabPage1.Controls.Add(hiddenPalaceLabel);
        tabPage1.Controls.Add(includeGPinShuffleCheckbox);
        tabPage1.Controls.Add(allowPalaceContinentSwapCheckbox);
        tabPage1.Controls.Add(label4);
        tabPage1.Controls.Add(allowPathEnemiesCheckbox);
        tabPage1.Controls.Add(shuffleEncountersCheckbox);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Margin = new Padding(4, 3, 4, 3);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(4, 3, 4, 3);
        tabPage1.Size = new Size(595, 331);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "Overworld";
        tabPage1.ToolTipText = "When selected, will hide Kasuto behind a forest tile";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // westBiomeSelector
        // 
        westBiomeSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        westBiomeSelector.FormattingEnabled = true;
        westBiomeSelector.Items.AddRange(new object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Islands", "Canyon", "Caldera", "Mountainous", "Random", "Random (no Vanilla)", "Random (no Vanilla / Shuffle)" });
        westBiomeSelector.Location = new Point(416, 174);
        westBiomeSelector.Margin = new Padding(4, 3, 4, 3);
        westBiomeSelector.Name = "westBiomeSelector";
        westBiomeSelector.Size = new Size(140, 23);
        westBiomeSelector.TabIndex = 50;
        toolTip1.SetToolTip(westBiomeSelector, "West Hyrule overworld map style.");
        westBiomeSelector.SelectedIndexChanged += WestBiome_SelectedIndexChanged;
        // 
        // westBiomeLabel
        // 
        westBiomeLabel.AutoSize = true;
        westBiomeLabel.Location = new Point(273, 177);
        westBiomeLabel.Margin = new Padding(4, 0, 4, 0);
        westBiomeLabel.Name = "westBiomeLabel";
        westBiomeLabel.Size = new Size(129, 15);
        westBiomeLabel.TabIndex = 49;
        westBiomeLabel.Text = "West Continent Biome:";
        toolTip1.SetToolTip(westBiomeLabel, "West Hyrule overworld map style.");
        // 
        // includeLavaInShuffleCheckBox
        // 
        includeLavaInShuffleCheckBox.AutoSize = true;
        includeLavaInShuffleCheckBox.Location = new Point(7, 129);
        includeLavaInShuffleCheckBox.Margin = new Padding(4, 3, 4, 3);
        includeLavaInShuffleCheckBox.Name = "includeLavaInShuffleCheckBox";
        includeLavaInShuffleCheckBox.Size = new Size(145, 19);
        includeLavaInShuffleCheckBox.TabIndex = 48;
        includeLavaInShuffleCheckBox.Text = "Include Lava in Shuffle";
        toolTip1.SetToolTip(includeLavaInShuffleCheckBox, "If checked, you may have enemies in path encounters");
        includeLavaInShuffleCheckBox.UseVisualStyleBackColor = true;
        // 
        // generateBaguWoodsCheckbox
        // 
        generateBaguWoodsCheckbox.AutoSize = true;
        generateBaguWoodsCheckbox.Location = new Point(282, 114);
        generateBaguWoodsCheckbox.Margin = new Padding(4, 3, 4, 3);
        generateBaguWoodsCheckbox.Name = "generateBaguWoodsCheckbox";
        generateBaguWoodsCheckbox.Size = new Size(151, 19);
        generateBaguWoodsCheckbox.TabIndex = 47;
        generateBaguWoodsCheckbox.Text = "Generate Bagu's Woods";
        generateBaguWoodsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(generateBaguWoodsCheckbox, "When selected, bagu's house will be hidden in a forest surrounded by lost woods tiles.");
        generateBaguWoodsCheckbox.UseVisualStyleBackColor = true;
        // 
        // useGoodBootsCheckbox
        // 
        useGoodBootsCheckbox.AutoSize = true;
        useGoodBootsCheckbox.Location = new Point(282, 88);
        useGoodBootsCheckbox.Margin = new Padding(4, 3, 4, 3);
        useGoodBootsCheckbox.Name = "useGoodBootsCheckbox";
        useGoodBootsCheckbox.Size = new Size(195, 19);
        useGoodBootsCheckbox.TabIndex = 46;
        useGoodBootsCheckbox.Text = "All Water is Walkable with Boots";
        useGoodBootsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(useGoodBootsCheckbox, "When selected, all water within the map boundaries can be traversed with the boots.");
        useGoodBootsCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleWhichLocationsAreHiddenCheckbox
        // 
        shuffleWhichLocationsAreHiddenCheckbox.AutoSize = true;
        shuffleWhichLocationsAreHiddenCheckbox.Location = new Point(7, 267);
        shuffleWhichLocationsAreHiddenCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleWhichLocationsAreHiddenCheckbox.Name = "shuffleWhichLocationsAreHiddenCheckbox";
        shuffleWhichLocationsAreHiddenCheckbox.Size = new Size(221, 19);
        shuffleWhichLocationsAreHiddenCheckbox.TabIndex = 45;
        shuffleWhichLocationsAreHiddenCheckbox.Text = "Shuffle which Location(s) are Hidden";
        shuffleWhichLocationsAreHiddenCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shuffleWhichLocationsAreHiddenCheckbox, "When selected, shuffles which location are in the hidden palace and hidden kasuto spots on the overworld.");
        shuffleWhichLocationsAreHiddenCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffledVanillaShowsActualTerrain
        // 
        shuffledVanillaShowsActualTerrain.AutoSize = true;
        shuffledVanillaShowsActualTerrain.Location = new Point(281, 299);
        shuffledVanillaShowsActualTerrain.Margin = new Padding(4, 3, 4, 3);
        shuffledVanillaShowsActualTerrain.Name = "shuffledVanillaShowsActualTerrain";
        shuffledVanillaShowsActualTerrain.Size = new Size(268, 19);
        shuffledVanillaShowsActualTerrain.TabIndex = 44;
        shuffledVanillaShowsActualTerrain.Text = "Shuffled Vanilla Locations Show Actual Terrain";
        toolTip1.SetToolTip(shuffledVanillaShowsActualTerrain, "When selected, if a shuffled vanilla map is in play, the map will show the correct terrain type of each location.");
        shuffledVanillaShowsActualTerrain.UseVisualStyleBackColor = true;
        // 
        // mazeBiome
        // 
        mazeBiome.DropDownStyle = ComboBoxStyle.DropDownList;
        mazeBiome.FormattingEnabled = true;
        mazeBiome.Items.AddRange(new object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Random" });
        mazeBiome.Location = new Point(416, 238);
        mazeBiome.Margin = new Padding(4, 3, 4, 3);
        mazeBiome.Name = "mazeBiome";
        mazeBiome.Size = new Size(140, 23);
        mazeBiome.TabIndex = 43;
        toolTip1.SetToolTip(mazeBiome, "Maze Island overworld map style.");
        mazeBiome.SelectedIndexChanged += MazeBiome_SelectedIndexChanged;
        // 
        // mazeIslandBiomeLabel
        // 
        mazeIslandBiomeLabel.AutoSize = true;
        mazeIslandBiomeLabel.Location = new Point(273, 241);
        mazeIslandBiomeLabel.Margin = new Padding(4, 0, 4, 0);
        mazeIslandBiomeLabel.Name = "mazeIslandBiomeLabel";
        mazeIslandBiomeLabel.Size = new Size(109, 15);
        mazeIslandBiomeLabel.TabIndex = 42;
        mazeIslandBiomeLabel.Text = "Maze Island Biome:";
        toolTip1.SetToolTip(mazeIslandBiomeLabel, "Maze Island overworld map style.");
        // 
        // eastBiome
        // 
        eastBiome.DropDownStyle = ComboBoxStyle.DropDownList;
        eastBiome.FormattingEnabled = true;
        eastBiome.Items.AddRange(new object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Islands", "Canyon", "Volcano", "Mountainous", "Random", "Random (no Vanilla)", "Random (no Vanilla / Shuffle)" });
        eastBiome.Location = new Point(416, 207);
        eastBiome.Margin = new Padding(4, 3, 4, 3);
        eastBiome.Name = "eastBiome";
        eastBiome.Size = new Size(140, 23);
        eastBiome.TabIndex = 41;
        toolTip1.SetToolTip(eastBiome, "East Hyrule overworld map style.");
        eastBiome.SelectedIndexChanged += EastBiome_SelectedIndexChanged;
        // 
        // dmBiome
        // 
        dmBiome.DropDownStyle = ComboBoxStyle.DropDownList;
        dmBiome.FormattingEnabled = true;
        dmBiome.Items.AddRange(new object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Islands", "Canyon", "Caldera", "Mountainous", "Random", "Random (no Vanilla)", "Random (no Vanilla / Shuffle)" });
        dmBiome.Location = new Point(416, 267);
        dmBiome.Margin = new Padding(4, 3, 4, 3);
        dmBiome.Name = "dmBiome";
        dmBiome.Size = new Size(140, 23);
        dmBiome.TabIndex = 40;
        toolTip1.SetToolTip(dmBiome, "Death Mountain overworld map style.");
        dmBiome.SelectedIndexChanged += DmBiome_SelectedIndexChanged;
        // 
        // climateSelector
        // 
        climateSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        climateSelector.FormattingEnabled = true;
        climateSelector.Items.AddRange(new object[] { "Classic", "Chaos", "Wetlands", "Great Lakes", "Scrubland" });
        climateSelector.Location = new Point(416, 145);
        climateSelector.Margin = new Padding(4, 3, 4, 3);
        climateSelector.Name = "climateSelector";
        climateSelector.Size = new Size(140, 23);
        climateSelector.TabIndex = 39;
        toolTip1.SetToolTip(climateSelector, "Weather affects terrain shape/frequency/complexity");
        // 
        // eastContinentBindingLabel
        // 
        eastContinentBindingLabel.AutoSize = true;
        eastContinentBindingLabel.Location = new Point(273, 210);
        eastContinentBindingLabel.Margin = new Padding(4, 0, 4, 0);
        eastContinentBindingLabel.Name = "eastContinentBindingLabel";
        eastContinentBindingLabel.Size = new Size(124, 15);
        eastContinentBindingLabel.TabIndex = 37;
        eastContinentBindingLabel.Text = "East Continent Biome:";
        toolTip1.SetToolTip(eastContinentBindingLabel, "East Hyrule overworld map style.");
        // 
        // deathMountainBiomeLabel
        // 
        deathMountainBiomeLabel.AutoSize = true;
        deathMountainBiomeLabel.Location = new Point(273, 271);
        deathMountainBiomeLabel.Margin = new Padding(4, 0, 4, 0);
        deathMountainBiomeLabel.Name = "deathMountainBiomeLabel";
        deathMountainBiomeLabel.Size = new Size(133, 15);
        deathMountainBiomeLabel.TabIndex = 36;
        deathMountainBiomeLabel.Text = "Death Mountain Biome:";
        toolTip1.SetToolTip(deathMountainBiomeLabel, "Death Mountain overworld map style.");
        // 
        // label39
        // 
        label39.BorderStyle = BorderStyle.Fixed3D;
        label39.Location = new Point(281, 136);
        label39.Margin = new Padding(4, 0, 4, 0);
        label39.Name = "label39";
        label39.Size = new Size(298, 2);
        label39.TabIndex = 35;
        // 
        // climateLabel
        // 
        climateLabel.AutoSize = true;
        climateLabel.Location = new Point(273, 149);
        climateLabel.Margin = new Padding(4, 0, 4, 0);
        climateLabel.Name = "climateLabel";
        climateLabel.Size = new Size(48, 15);
        climateLabel.TabIndex = 34;
        climateLabel.Text = "Climate";
        toolTip1.SetToolTip(climateLabel, "West Hyrule overworld map style.");
        // 
        // allowBoulderBlockedConnectionsCheckbox
        // 
        allowBoulderBlockedConnectionsCheckbox.AutoSize = true;
        allowBoulderBlockedConnectionsCheckbox.Location = new Point(282, 59);
        allowBoulderBlockedConnectionsCheckbox.Margin = new Padding(4, 3, 4, 3);
        allowBoulderBlockedConnectionsCheckbox.Name = "allowBoulderBlockedConnectionsCheckbox";
        allowBoulderBlockedConnectionsCheckbox.Size = new Size(274, 19);
        allowBoulderBlockedConnectionsCheckbox.TabIndex = 33;
        allowBoulderBlockedConnectionsCheckbox.Text = "Allow Connection Caves to be Boulder Blocked";
        toolTip1.SetToolTip(allowBoulderBlockedConnectionsCheckbox, "When selected, allows boulders to block any cave.");
        allowBoulderBlockedConnectionsCheckbox.UseVisualStyleBackColor = true;
        // 
        // saneCaveShuffleBox
        // 
        saneCaveShuffleBox.AutoSize = true;
        saneCaveShuffleBox.Location = new Point(282, 32);
        saneCaveShuffleBox.Margin = new Padding(4, 3, 4, 3);
        saneCaveShuffleBox.Name = "saneCaveShuffleBox";
        saneCaveShuffleBox.Size = new Size(199, 19);
        saneCaveShuffleBox.TabIndex = 32;
        saneCaveShuffleBox.Text = "Restrict Connection Cave Shuffle";
        saneCaveShuffleBox.ThreeState = true;
        toolTip1.SetToolTip(saneCaveShuffleBox, "When selected, caves will be placed in a more logical manner in which they \"point\" at their destination.");
        saneCaveShuffleBox.UseVisualStyleBackColor = true;
        // 
        // hideLessImportantLocationsCheckbox
        // 
        hideLessImportantLocationsCheckbox.AutoSize = true;
        hideLessImportantLocationsCheckbox.Location = new Point(282, 7);
        hideLessImportantLocationsCheckbox.Margin = new Padding(4, 3, 4, 3);
        hideLessImportantLocationsCheckbox.Name = "hideLessImportantLocationsCheckbox";
        hideLessImportantLocationsCheckbox.Size = new Size(186, 19);
        hideLessImportantLocationsCheckbox.TabIndex = 31;
        hideLessImportantLocationsCheckbox.Text = "Hide Less Important Locations";
        hideLessImportantLocationsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(hideLessImportantLocationsCheckbox, "When selected, blends unimportant locations in with the surrounding terrain.");
        hideLessImportantLocationsCheckbox.UseVisualStyleBackColor = true;
        // 
        // ContinentConnectionLabel
        // 
        ContinentConnectionLabel.AutoSize = true;
        ContinentConnectionLabel.Location = new Point(8, 289);
        ContinentConnectionLabel.Margin = new Padding(4, 0, 4, 0);
        ContinentConnectionLabel.Name = "ContinentConnectionLabel";
        ContinentConnectionLabel.Size = new Size(133, 15);
        ContinentConnectionLabel.TabIndex = 30;
        ContinentConnectionLabel.Text = "Continent Connections:";
        toolTip1.SetToolTip(ContinentConnectionLabel, "Modes for how the different continents can connect to each other.");
        // 
        // continentConnectionBox
        // 
        continentConnectionBox.DropDownStyle = ComboBoxStyle.DropDownList;
        continentConnectionBox.FormattingEnabled = true;
        continentConnectionBox.Items.AddRange(new object[] { "Normal", "R+B Border Shuffle", "Transportation Shuffle", "Anything Goes" });
        continentConnectionBox.Location = new Point(151, 286);
        continentConnectionBox.Margin = new Padding(4, 3, 4, 3);
        continentConnectionBox.Name = "continentConnectionBox";
        continentConnectionBox.Size = new Size(113, 23);
        continentConnectionBox.TabIndex = 29;
        toolTip1.SetToolTip(continentConnectionBox, "Modes for how the different continents can connect to each other.");
        // 
        // label36
        // 
        label36.BorderStyle = BorderStyle.Fixed3D;
        label36.Location = new Point(7, 187);
        label36.Margin = new Padding(4, 0, 4, 0);
        label36.Name = "label36";
        label36.Size = new Size(243, 1);
        label36.TabIndex = 28;
        // 
        // encounterRateBox
        // 
        encounterRateBox.DropDownStyle = ComboBoxStyle.DropDownList;
        encounterRateBox.FormattingEnabled = true;
        encounterRateBox.Items.AddRange(new object[] { "Normal", "50%", "None", "Random" });
        encounterRateBox.Location = new Point(108, 156);
        encounterRateBox.Margin = new Padding(4, 3, 4, 3);
        encounterRateBox.Name = "encounterRateBox";
        encounterRateBox.Size = new Size(100, 23);
        encounterRateBox.TabIndex = 27;
        toolTip1.SetToolTip(encounterRateBox, "Allows you to reduce the encounter rate or turn encounters off entirely.");
        // 
        // encounterRateLabel
        // 
        encounterRateLabel.AutoSize = true;
        encounterRateLabel.Location = new Point(4, 159);
        encounterRateLabel.Margin = new Padding(4, 0, 4, 0);
        encounterRateLabel.Name = "encounterRateLabel";
        encounterRateLabel.Size = new Size(90, 15);
        encounterRateLabel.TabIndex = 26;
        encounterRateLabel.Text = "Encounter Rate:";
        toolTip1.SetToolTip(encounterRateLabel, "Allows you to reduce the encounter rate or turn encounters off entirely.");
        // 
        // hideKasutoList
        // 
        hideKasutoList.DropDownStyle = ComboBoxStyle.DropDownList;
        hideKasutoList.FormattingEnabled = true;
        hideKasutoList.Items.AddRange(new object[] { "Off", "On", "Random" });
        hideKasutoList.Location = new Point(108, 227);
        hideKasutoList.Margin = new Padding(4, 3, 4, 3);
        hideKasutoList.Name = "hideKasutoList";
        hideKasutoList.Size = new Size(100, 23);
        hideKasutoList.TabIndex = 25;
        toolTip1.SetToolTip(hideKasutoList, "When selected, will hide Kasuto behind a forest tile");
        hideKasutoList.SelectedIndexChanged += HideKasutoBox_SelectedIndexChanged;
        // 
        // hiddenKasutoLabel
        // 
        hiddenKasutoLabel.AutoSize = true;
        hiddenKasutoLabel.Location = new Point(5, 229);
        hiddenKasutoLabel.Margin = new Padding(4, 0, 4, 0);
        hiddenKasutoLabel.Name = "hiddenKasutoLabel";
        hiddenKasutoLabel.Size = new Size(88, 15);
        hiddenKasutoLabel.TabIndex = 24;
        hiddenKasutoLabel.Text = "Hidden Kasuto:";
        // 
        // hiddenPalaceList
        // 
        hiddenPalaceList.DropDownStyle = ComboBoxStyle.DropDownList;
        hiddenPalaceList.FormattingEnabled = true;
        hiddenPalaceList.Items.AddRange(new object[] { "Off", "On", "Random" });
        hiddenPalaceList.Location = new Point(108, 196);
        hiddenPalaceList.Margin = new Padding(4, 3, 4, 3);
        hiddenPalaceList.Name = "hiddenPalaceList";
        hiddenPalaceList.Size = new Size(100, 23);
        hiddenPalaceList.TabIndex = 23;
        toolTip1.SetToolTip(hiddenPalaceList, "When selected, will include three eye rock on the overworld");
        hiddenPalaceList.SelectedIndexChanged += HpCmbo_SelectedIndexChanged;
        // 
        // hiddenPalaceLabel
        // 
        hiddenPalaceLabel.AutoSize = true;
        hiddenPalaceLabel.Location = new Point(5, 199);
        hiddenPalaceLabel.Margin = new Padding(4, 0, 4, 0);
        hiddenPalaceLabel.Name = "hiddenPalaceLabel";
        hiddenPalaceLabel.Size = new Size(86, 15);
        hiddenPalaceLabel.TabIndex = 22;
        hiddenPalaceLabel.Text = "Hidden Palace:";
        // 
        // includeGPinShuffleCheckbox
        // 
        includeGPinShuffleCheckbox.AutoSize = true;
        includeGPinShuffleCheckbox.Location = new Point(7, 32);
        includeGPinShuffleCheckbox.Margin = new Padding(4, 3, 4, 3);
        includeGPinShuffleCheckbox.Name = "includeGPinShuffleCheckbox";
        includeGPinShuffleCheckbox.Size = new Size(186, 19);
        includeGPinShuffleCheckbox.TabIndex = 21;
        includeGPinShuffleCheckbox.Text = "Include Great Palace in Shuffle";
        includeGPinShuffleCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includeGPinShuffleCheckbox, "When selected, palace 7 does not have to be in the valley of death.");
        includeGPinShuffleCheckbox.UseVisualStyleBackColor = true;
        // 
        // allowPalaceContinentSwapCheckbox
        // 
        allowPalaceContinentSwapCheckbox.AutoSize = true;
        allowPalaceContinentSwapCheckbox.Location = new Point(7, 7);
        allowPalaceContinentSwapCheckbox.Margin = new Padding(4, 3, 4, 3);
        allowPalaceContinentSwapCheckbox.Name = "allowPalaceContinentSwapCheckbox";
        allowPalaceContinentSwapCheckbox.Size = new Size(204, 19);
        allowPalaceContinentSwapCheckbox.TabIndex = 20;
        allowPalaceContinentSwapCheckbox.Text = "Allow Palaces to Swap Continents";
        allowPalaceContinentSwapCheckbox.ThreeState = true;
        toolTip1.SetToolTip(allowPalaceContinentSwapCheckbox, "When selected, palaces can move from their normal continents. Palace 1 could be found on Maze Island or East Hyrule, for example.");
        allowPalaceContinentSwapCheckbox.UseVisualStyleBackColor = true;
        allowPalaceContinentSwapCheckbox.CheckStateChanged += PalaceSwapBox_CheckStateChanged;
        // 
        // label4
        // 
        label4.BorderStyle = BorderStyle.Fixed3D;
        label4.Location = new Point(7, 63);
        label4.Margin = new Padding(4, 0, 4, 0);
        label4.Name = "label4";
        label4.Size = new Size(243, 1);
        label4.TabIndex = 18;
        // 
        // allowPathEnemiesCheckbox
        // 
        allowPathEnemiesCheckbox.AutoSize = true;
        allowPathEnemiesCheckbox.Location = new Point(7, 103);
        allowPathEnemiesCheckbox.Margin = new Padding(4, 3, 4, 3);
        allowPathEnemiesCheckbox.Name = "allowPathEnemiesCheckbox";
        allowPathEnemiesCheckbox.Size = new Size(184, 19);
        allowPathEnemiesCheckbox.TabIndex = 15;
        allowPathEnemiesCheckbox.Text = "Allow Unsafe Path Encounters";
        toolTip1.SetToolTip(allowPathEnemiesCheckbox, "If checked, you may have enemies in path encounters");
        allowPathEnemiesCheckbox.UseVisualStyleBackColor = true;
        allowPathEnemiesCheckbox.CheckStateChanged += shuffleEncounters_CheckStateChanged;
        // 
        // shuffleEncountersCheckbox
        // 
        shuffleEncountersCheckbox.AutoSize = true;
        shuffleEncountersCheckbox.Location = new Point(7, 78);
        shuffleEncountersCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleEncountersCheckbox.Name = "shuffleEncountersCheckbox";
        shuffleEncountersCheckbox.Size = new Size(125, 19);
        shuffleEncountersCheckbox.TabIndex = 14;
        shuffleEncountersCheckbox.Text = "Shuffle Encounters";
        shuffleEncountersCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shuffleEncountersCheckbox, "Shuffle which overworld encounters occur on different terrain types");
        shuffleEncountersCheckbox.UseVisualStyleBackColor = true;
        shuffleEncountersCheckbox.CheckStateChanged += shuffleEncounters_CheckStateChanged;
        // 
        // tabPage2
        // 
        tabPage2.Controls.Add(shortenGPCheckbox);
        tabPage2.Controls.Add(shortenNormalPalaceCheckbox);
        tabPage2.Controls.Add(ShortenPalaceLabel);
        tabPage2.Controls.Add(useCustomRoomsBox);
        tabPage2.Controls.Add(HardBossesCheckbox);
        tabPage2.Controls.Add(noDuplicateRoomsByEnemiesCheckbox);
        tabPage2.Controls.Add(gpStyleLabel);
        tabPage2.Controls.Add(gpStyleList);
        tabPage2.Controls.Add(includev4_4RoomsCheckbox);
        tabPage2.Controls.Add(includev4_0RoomsCheckbox);
        tabPage2.Controls.Add(noDuplicateRoomsByLayoutCheckbox);
        tabPage2.Controls.Add(label7);
        tabPage2.Controls.Add(label5);
        tabPage2.Controls.Add(startingGemsMaxList);
        tabPage2.Controls.Add(palaceStyleLabel);
        tabPage2.Controls.Add(palaceStyleList);
        tabPage2.Controls.Add(bossRoomsExitToPalaceCheckbox);
        tabPage2.Controls.Add(blockingRoomsInAnyPalaceCheckbox);
        tabPage2.Controls.Add(includeVanillaRoomsCheckbox);
        tabPage2.Controls.Add(randomizeBossItemCheckbox);
        tabPage2.Controls.Add(removeTbirdCheckbox);
        tabPage2.Controls.Add(restartAtPalacesCheckbox);
        tabPage2.Controls.Add(palacePaletteCheckbox);
        tabPage2.Controls.Add(tbirdRequiredCheckbox);
        tabPage2.Controls.Add(label6);
        tabPage2.Controls.Add(startingGemsMinList);
        tabPage2.Location = new Point(4, 24);
        tabPage2.Margin = new Padding(4, 3, 4, 3);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(4, 3, 4, 3);
        tabPage2.Size = new Size(595, 331);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Palaces";
        tabPage2.UseVisualStyleBackColor = true;
        // 
        // shortenGPCheckbox
        // 
        shortenGPCheckbox.AutoSize = true;
        shortenGPCheckbox.Location = new Point(197, 86);
        shortenGPCheckbox.Margin = new Padding(4, 3, 4, 3);
        shortenGPCheckbox.Name = "shortenGPCheckbox";
        shortenGPCheckbox.Size = new Size(15, 14);
        shortenGPCheckbox.TabIndex = 39;
        shortenGPCheckbox.ThreeState = true;
        shortenGPCheckbox.UseVisualStyleBackColor = true;
        // 
        // shortenNormalPalaceCheckbox
        // 
        shortenNormalPalaceCheckbox.AutoSize = true;
        shortenNormalPalaceCheckbox.Location = new Point(197, 34);
        shortenNormalPalaceCheckbox.Margin = new Padding(4, 3, 4, 3);
        shortenNormalPalaceCheckbox.Name = "shortenNormalPalaceCheckbox";
        shortenNormalPalaceCheckbox.Size = new Size(15, 14);
        shortenNormalPalaceCheckbox.TabIndex = 38;
        shortenNormalPalaceCheckbox.ThreeState = true;
        shortenNormalPalaceCheckbox.UseVisualStyleBackColor = true;
        // 
        // ShortenPalaceLabel
        // 
        ShortenPalaceLabel.AutoSize = true;
        ShortenPalaceLabel.Location = new Point(186, 12);
        ShortenPalaceLabel.Margin = new Padding(4, 0, 4, 0);
        ShortenPalaceLabel.Name = "ShortenPalaceLabel";
        ShortenPalaceLabel.Size = new Size(51, 15);
        ShortenPalaceLabel.TabIndex = 37;
        ShortenPalaceLabel.Text = "Shorten:";
        toolTip1.SetToolTip(ShortenPalaceLabel, "Reduces the number of rooms in the palace.");
        // 
        // useCustomRoomsBox
        // 
        useCustomRoomsBox.AutoSize = true;
        useCustomRoomsBox.Location = new Point(328, 211);
        useCustomRoomsBox.Margin = new Padding(4, 3, 4, 3);
        useCustomRoomsBox.Name = "useCustomRoomsBox";
        useCustomRoomsBox.Size = new Size(130, 19);
        useCustomRoomsBox.TabIndex = 36;
        useCustomRoomsBox.Text = "Use Custom Rooms";
        toolTip1.SetToolTip(useCustomRoomsBox, "When checked, Use CustomRooms.json to create you own room set.");
        useCustomRoomsBox.UseVisualStyleBackColor = true;
        // 
        // HardBossesCheckbox
        // 
        HardBossesCheckbox.AutoSize = true;
        HardBossesCheckbox.Location = new Point(328, 186);
        HardBossesCheckbox.Margin = new Padding(4, 3, 4, 3);
        HardBossesCheckbox.Name = "HardBossesCheckbox";
        HardBossesCheckbox.Size = new Size(102, 19);
        HardBossesCheckbox.TabIndex = 31;
        HardBossesCheckbox.Text = "Harder Carock";
        HardBossesCheckbox.ThreeState = true;
        toolTip1.SetToolTip(HardBossesCheckbox, "Carock has learned some new tricks. No more crouching in the corner!\r\n");
        HardBossesCheckbox.UseVisualStyleBackColor = true;
        // 
        // noDuplicateRoomsByEnemiesCheckbox
        // 
        noDuplicateRoomsByEnemiesCheckbox.AutoSize = true;
        noDuplicateRoomsByEnemiesCheckbox.Location = new Point(328, 111);
        noDuplicateRoomsByEnemiesCheckbox.Margin = new Padding(4, 3, 4, 3);
        noDuplicateRoomsByEnemiesCheckbox.Name = "noDuplicateRoomsByEnemiesCheckbox";
        noDuplicateRoomsByEnemiesCheckbox.RightToLeft = RightToLeft.No;
        noDuplicateRoomsByEnemiesCheckbox.Size = new Size(247, 19);
        noDuplicateRoomsByEnemiesCheckbox.TabIndex = 30;
        noDuplicateRoomsByEnemiesCheckbox.Text = "No Duplicate Rooms (By Layout/Enemies)";
        toolTip1.SetToolTip(noDuplicateRoomsByEnemiesCheckbox, "Each room will only show up at most once in a palace. Rooms that have multiple variations can still have one of each variation.");
        noDuplicateRoomsByEnemiesCheckbox.UseVisualStyleBackColor = true;
        noDuplicateRoomsByEnemiesCheckbox.CheckedChanged += DuplicateRoomExclusionHandler;
        // 
        // gpStyleLabel
        // 
        gpStyleLabel.AutoSize = true;
        gpStyleLabel.Location = new Point(7, 65);
        gpStyleLabel.Margin = new Padding(4, 0, 4, 0);
        gpStyleLabel.Name = "gpStyleLabel";
        gpStyleLabel.Size = new Size(103, 15);
        gpStyleLabel.TabIndex = 29;
        gpStyleLabel.Text = "Great Palace Style:";
        toolTip1.SetToolTip(gpStyleLabel, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // gpStyleList
        // 
        gpStyleList.DropDownStyle = ComboBoxStyle.DropDownList;
        gpStyleList.FormattingEnabled = true;
        gpStyleList.Items.AddRange(new object[] { "Vanilla", "Shuffled", "Reconstructed", "Sane", "Chaos", "Random" });
        gpStyleList.Location = new Point(7, 82);
        gpStyleList.Margin = new Padding(4, 3, 4, 3);
        gpStyleList.Name = "gpStyleList";
        gpStyleList.Size = new Size(176, 23);
        gpStyleList.TabIndex = 28;
        toolTip1.SetToolTip(gpStyleList, "Controls what style GP (Palace 7) is.");
        gpStyleList.SelectedIndexChanged += PalaceBox_SelectedIndexChanged;
        // 
        // includev4_4RoomsCheckbox
        // 
        includev4_4RoomsCheckbox.AutoSize = true;
        includev4_4RoomsCheckbox.Location = new Point(7, 162);
        includev4_4RoomsCheckbox.Margin = new Padding(4, 3, 4, 3);
        includev4_4RoomsCheckbox.Name = "includev4_4RoomsCheckbox";
        includev4_4RoomsCheckbox.Size = new Size(129, 19);
        includev4_4RoomsCheckbox.TabIndex = 27;
        includev4_4RoomsCheckbox.Text = "Include v4.4 Rooms";
        includev4_4RoomsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includev4_4RoomsCheckbox, "New Rooms first released in v4.4 are included in the room pool.");
        includev4_4RoomsCheckbox.UseVisualStyleBackColor = true;
        includev4_4RoomsCheckbox.CheckedChanged += AtLeastOneRoomTypeRequiredHandler;
        // 
        // includev4_0RoomsCheckbox
        // 
        includev4_0RoomsCheckbox.AutoSize = true;
        includev4_0RoomsCheckbox.Location = new Point(7, 136);
        includev4_0RoomsCheckbox.Margin = new Padding(4, 3, 4, 3);
        includev4_0RoomsCheckbox.Name = "includev4_0RoomsCheckbox";
        includev4_0RoomsCheckbox.Size = new Size(129, 19);
        includev4_0RoomsCheckbox.TabIndex = 26;
        includev4_0RoomsCheckbox.Text = "Include v4.0 Rooms";
        includev4_0RoomsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includev4_0RoomsCheckbox, "New Rooms first released in v4.0 are included in the room pool.");
        includev4_0RoomsCheckbox.UseVisualStyleBackColor = true;
        includev4_0RoomsCheckbox.CheckedChanged += AtLeastOneRoomTypeRequiredHandler;
        // 
        // noDuplicateRoomsByLayoutCheckbox
        // 
        noDuplicateRoomsByLayoutCheckbox.AutoSize = true;
        noDuplicateRoomsByLayoutCheckbox.Location = new Point(328, 86);
        noDuplicateRoomsByLayoutCheckbox.Margin = new Padding(4, 3, 4, 3);
        noDuplicateRoomsByLayoutCheckbox.Name = "noDuplicateRoomsByLayoutCheckbox";
        noDuplicateRoomsByLayoutCheckbox.RightToLeft = RightToLeft.No;
        noDuplicateRoomsByLayoutCheckbox.Size = new Size(198, 19);
        noDuplicateRoomsByLayoutCheckbox.TabIndex = 25;
        noDuplicateRoomsByLayoutCheckbox.Text = "No Duplicate Rooms (By Layout)";
        toolTip1.SetToolTip(noDuplicateRoomsByLayoutCheckbox, "Each room will only show up at most once in a palace. Rooms that have multiple variations cannot appear more than once per layout.");
        noDuplicateRoomsByLayoutCheckbox.UseVisualStyleBackColor = true;
        noDuplicateRoomsByLayoutCheckbox.CheckedChanged += DuplicateRoomExclusionHandler;
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Location = new Point(119, 270);
        label7.Margin = new Padding(4, 0, 4, 0);
        label7.Name = "label7";
        label7.Size = new Size(30, 15);
        label7.TabIndex = 24;
        label7.Text = "Max";
        toolTip1.SetToolTip(label7, "Starting Attack Level");
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(31, 270);
        label5.Margin = new Padding(4, 0, 4, 0);
        label5.Name = "label5";
        label5.Size = new Size(28, 15);
        label5.TabIndex = 23;
        label5.Text = "Min";
        toolTip1.SetToolTip(label5, "Starting Attack Level");
        // 
        // startingGemsMaxList
        // 
        startingGemsMaxList.DropDownStyle = ComboBoxStyle.DropDownList;
        startingGemsMaxList.FormattingEnabled = true;
        startingGemsMaxList.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6" });
        startingGemsMaxList.Location = new Point(98, 287);
        startingGemsMaxList.Margin = new Padding(4, 3, 4, 3);
        startingGemsMaxList.Name = "startingGemsMaxList";
        startingGemsMaxList.Size = new Size(72, 23);
        startingGemsMaxList.TabIndex = 15;
        toolTip1.SetToolTip(startingGemsMaxList, "How many gems need to be placed before entering palace 7");
        // 
        // palaceStyleLabel
        // 
        palaceStyleLabel.AutoSize = true;
        palaceStyleLabel.Location = new Point(7, 12);
        palaceStyleLabel.Margin = new Padding(4, 0, 4, 0);
        palaceStyleLabel.Name = "palaceStyleLabel";
        palaceStyleLabel.Size = new Size(72, 15);
        palaceStyleLabel.TabIndex = 14;
        palaceStyleLabel.Text = "Palace Style:";
        toolTip1.SetToolTip(palaceStyleLabel, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // palaceStyleList
        // 
        palaceStyleList.DropDownStyle = ComboBoxStyle.DropDownList;
        palaceStyleList.FormattingEnabled = true;
        palaceStyleList.Items.AddRange(new object[] { "Vanilla", "Shuffled", "Reconstructed", "Sane", "Chaos", "Random Per Palace", "Random (All Same)" });
        palaceStyleList.Location = new Point(7, 29);
        palaceStyleList.Margin = new Padding(4, 3, 4, 3);
        palaceStyleList.Name = "palaceStyleList";
        palaceStyleList.Size = new Size(176, 23);
        palaceStyleList.TabIndex = 13;
        toolTip1.SetToolTip(palaceStyleList, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        palaceStyleList.SelectedIndexChanged += PalaceBox_SelectedIndexChanged;
        // 
        // bossRoomsExitToPalaceCheckbox
        // 
        bossRoomsExitToPalaceCheckbox.AutoSize = true;
        bossRoomsExitToPalaceCheckbox.Location = new Point(328, 161);
        bossRoomsExitToPalaceCheckbox.Margin = new Padding(4, 3, 4, 3);
        bossRoomsExitToPalaceCheckbox.Name = "bossRoomsExitToPalaceCheckbox";
        bossRoomsExitToPalaceCheckbox.Size = new Size(163, 19);
        bossRoomsExitToPalaceCheckbox.TabIndex = 12;
        bossRoomsExitToPalaceCheckbox.Text = "Boss Rooms Exit to Palace";
        bossRoomsExitToPalaceCheckbox.ThreeState = true;
        toolTip1.SetToolTip(bossRoomsExitToPalaceCheckbox, "When selected, boss rooms will no longer lead outside, they will lead to more palace");
        bossRoomsExitToPalaceCheckbox.UseVisualStyleBackColor = true;
        // 
        // blockingRoomsInAnyPalaceCheckbox
        // 
        blockingRoomsInAnyPalaceCheckbox.AutoSize = true;
        blockingRoomsInAnyPalaceCheckbox.Location = new Point(328, 136);
        blockingRoomsInAnyPalaceCheckbox.Margin = new Padding(4, 3, 4, 3);
        blockingRoomsInAnyPalaceCheckbox.Name = "blockingRoomsInAnyPalaceCheckbox";
        blockingRoomsInAnyPalaceCheckbox.Size = new Size(251, 19);
        blockingRoomsInAnyPalaceCheckbox.TabIndex = 11;
        blockingRoomsInAnyPalaceCheckbox.Text = "Blocking Rooms Can Appear in Any Palace";
        toolTip1.SetToolTip(blockingRoomsInAnyPalaceCheckbox, "When selected, a palace can be blocked by any of the item/spell blocked rooms");
        blockingRoomsInAnyPalaceCheckbox.UseVisualStyleBackColor = true;
        // 
        // includeVanillaRoomsCheckbox
        // 
        includeVanillaRoomsCheckbox.AutoSize = true;
        includeVanillaRoomsCheckbox.Location = new Point(7, 111);
        includeVanillaRoomsCheckbox.Margin = new Padding(4, 3, 4, 3);
        includeVanillaRoomsCheckbox.Name = "includeVanillaRoomsCheckbox";
        includeVanillaRoomsCheckbox.Size = new Size(142, 19);
        includeVanillaRoomsCheckbox.TabIndex = 10;
        includeVanillaRoomsCheckbox.Text = "Include Vanilla Rooms";
        includeVanillaRoomsCheckbox.ThreeState = true;
        includeVanillaRoomsCheckbox.UseVisualStyleBackColor = true;
        includeVanillaRoomsCheckbox.CheckedChanged += AtLeastOneRoomTypeRequiredHandler;
        // 
        // randomizeBossItemCheckbox
        // 
        randomizeBossItemCheckbox.AutoSize = true;
        randomizeBossItemCheckbox.Location = new Point(328, 59);
        randomizeBossItemCheckbox.Margin = new Padding(4, 3, 4, 3);
        randomizeBossItemCheckbox.Name = "randomizeBossItemCheckbox";
        randomizeBossItemCheckbox.Size = new Size(168, 19);
        randomizeBossItemCheckbox.TabIndex = 8;
        randomizeBossItemCheckbox.Text = "Randomize Boss Item Drop";
        toolTip1.SetToolTip(randomizeBossItemCheckbox, "When selected, the item that drops after a boss has been killed will be randomized");
        randomizeBossItemCheckbox.UseVisualStyleBackColor = true;
        // 
        // removeTbirdCheckbox
        // 
        removeTbirdCheckbox.AutoSize = true;
        removeTbirdCheckbox.Location = new Point(7, 213);
        removeTbirdCheckbox.Margin = new Padding(4, 3, 4, 3);
        removeTbirdCheckbox.Name = "removeTbirdCheckbox";
        removeTbirdCheckbox.Size = new Size(137, 19);
        removeTbirdCheckbox.TabIndex = 7;
        removeTbirdCheckbox.Text = "Remove Thunderbird";
        toolTip1.SetToolTip(removeTbirdCheckbox, "If checked, you must defeat thunderbird");
        removeTbirdCheckbox.UseVisualStyleBackColor = true;
        removeTbirdCheckbox.CheckStateChanged += RemoveTbird_CheckStateChanged;
        // 
        // restartAtPalacesCheckbox
        // 
        restartAtPalacesCheckbox.AutoSize = true;
        restartAtPalacesCheckbox.Location = new Point(328, 6);
        restartAtPalacesCheckbox.Margin = new Padding(4, 3, 4, 3);
        restartAtPalacesCheckbox.Name = "restartAtPalacesCheckbox";
        restartAtPalacesCheckbox.Size = new Size(193, 19);
        restartAtPalacesCheckbox.TabIndex = 5;
        restartAtPalacesCheckbox.Text = "Restart at palaces on game over";
        toolTip1.SetToolTip(restartAtPalacesCheckbox, "When selected, if you game over in a palace, you will restart at that palace instead of the normal starting spot");
        restartAtPalacesCheckbox.UseVisualStyleBackColor = true;
        // 
        // palacePaletteCheckbox
        // 
        palacePaletteCheckbox.AutoSize = true;
        palacePaletteCheckbox.Location = new Point(328, 32);
        palacePaletteCheckbox.Margin = new Padding(4, 3, 4, 3);
        palacePaletteCheckbox.Name = "palacePaletteCheckbox";
        palacePaletteCheckbox.Size = new Size(148, 19);
        palacePaletteCheckbox.TabIndex = 4;
        palacePaletteCheckbox.Text = "Change Palace Palettes";
        toolTip1.SetToolTip(palacePaletteCheckbox, "This option changes the colors and tileset of palaces");
        palacePaletteCheckbox.UseVisualStyleBackColor = true;
        // 
        // tbirdRequiredCheckbox
        // 
        tbirdRequiredCheckbox.AutoSize = true;
        tbirdRequiredCheckbox.Location = new Point(7, 187);
        tbirdRequiredCheckbox.Margin = new Padding(4, 3, 4, 3);
        tbirdRequiredCheckbox.Name = "tbirdRequiredCheckbox";
        tbirdRequiredCheckbox.Size = new Size(141, 19);
        tbirdRequiredCheckbox.TabIndex = 3;
        tbirdRequiredCheckbox.Text = "Thunderbird Required";
        tbirdRequiredCheckbox.ThreeState = true;
        toolTip1.SetToolTip(tbirdRequiredCheckbox, "If checked, you must defeat thunderbird");
        tbirdRequiredCheckbox.UseVisualStyleBackColor = true;
        tbirdRequiredCheckbox.CheckStateChanged += TbirdBox_CheckStateChanged;
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(7, 251);
        label6.Margin = new Padding(4, 0, 4, 0);
        label6.Name = "label6";
        label6.Size = new Size(176, 15);
        label6.TabIndex = 2;
        label6.Text = "Number of Palaces to Complete";
        // 
        // startingGemsMinList
        // 
        startingGemsMinList.DropDownStyle = ComboBoxStyle.DropDownList;
        startingGemsMinList.FormattingEnabled = true;
        startingGemsMinList.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6" });
        startingGemsMinList.Location = new Point(7, 288);
        startingGemsMinList.Margin = new Padding(4, 3, 4, 3);
        startingGemsMinList.Name = "startingGemsMinList";
        startingGemsMinList.Size = new Size(72, 23);
        startingGemsMinList.TabIndex = 1;
        toolTip1.SetToolTip(startingGemsMinList, "How many gems need to be placed before entering palace 7");
        // 
        // tabPage5
        // 
        tabPage5.Controls.Add(lifeEffectivenessList);
        tabPage5.Controls.Add(magicEffectivenessList);
        tabPage5.Controls.Add(attackEffectivenessList);
        tabPage5.Controls.Add(scaleLevelRequirementsToCapCheckbox);
        tabPage5.Controls.Add(levelCapLabel);
        tabPage5.Controls.Add(lifeCapList);
        tabPage5.Controls.Add(magCapList);
        tabPage5.Controls.Add(atkCapList);
        tabPage5.Controls.Add(lifeCapLabel);
        tabPage5.Controls.Add(magCapLabel);
        tabPage5.Controls.Add(attackCapLabel);
        tabPage5.Controls.Add(label12);
        tabPage5.Controls.Add(magicEffectivenessLabel);
        tabPage5.Controls.Add(attackEffectivenessLabel);
        tabPage5.Controls.Add(expBox);
        tabPage5.Location = new Point(4, 24);
        tabPage5.Margin = new Padding(4, 3, 4, 3);
        tabPage5.Name = "tabPage5";
        tabPage5.Size = new Size(595, 331);
        tabPage5.TabIndex = 4;
        tabPage5.Text = "Levels";
        tabPage5.UseVisualStyleBackColor = true;
        // 
        // lifeEffectivenessList
        // 
        lifeEffectivenessList.DropDownStyle = ComboBoxStyle.DropDownList;
        lifeEffectivenessList.FormattingEnabled = true;
        lifeEffectivenessList.Items.AddRange(new object[] { "Random", "OHKO Link", "Vanilla", "High Defense", "Invincible" });
        lifeEffectivenessList.Location = new Point(392, 111);
        lifeEffectivenessList.Margin = new Padding(4, 3, 4, 3);
        lifeEffectivenessList.Name = "lifeEffectivenessList";
        lifeEffectivenessList.Size = new Size(140, 23);
        lifeEffectivenessList.TabIndex = 25;
        toolTip1.SetToolTip(lifeEffectivenessList, "Different modes for the effectiveness of Life levels");
        // 
        // magicEffectivenessList
        // 
        magicEffectivenessList.DropDownStyle = ComboBoxStyle.DropDownList;
        magicEffectivenessList.FormattingEnabled = true;
        magicEffectivenessList.Items.AddRange(new object[] { "Random", "High Spell Cost", "Vanilla", "Low Spell Cost", "Free Spells" });
        magicEffectivenessList.Location = new Point(392, 66);
        magicEffectivenessList.Margin = new Padding(4, 3, 4, 3);
        magicEffectivenessList.Name = "magicEffectivenessList";
        magicEffectivenessList.Size = new Size(140, 23);
        magicEffectivenessList.TabIndex = 24;
        toolTip1.SetToolTip(magicEffectivenessList, "Different modes for the effectiveness of Magic levels");
        // 
        // attackEffectivenessList
        // 
        attackEffectivenessList.DropDownStyle = ComboBoxStyle.DropDownList;
        attackEffectivenessList.FormattingEnabled = true;
        attackEffectivenessList.Items.AddRange(new object[] { "Random", "Low Attack", "Vanilla", "High Attack", "OHKO Enemies" });
        attackEffectivenessList.Location = new Point(392, 23);
        attackEffectivenessList.Margin = new Padding(4, 3, 4, 3);
        attackEffectivenessList.Name = "attackEffectivenessList";
        attackEffectivenessList.Size = new Size(140, 23);
        attackEffectivenessList.TabIndex = 23;
        toolTip1.SetToolTip(attackEffectivenessList, "Different modes for the effectiveness of Attack levels");
        // 
        // scaleLevelRequirementsToCapCheckbox
        // 
        scaleLevelRequirementsToCapCheckbox.AutoSize = true;
        scaleLevelRequirementsToCapCheckbox.Location = new Point(10, 188);
        scaleLevelRequirementsToCapCheckbox.Margin = new Padding(4, 3, 4, 3);
        scaleLevelRequirementsToCapCheckbox.Name = "scaleLevelRequirementsToCapCheckbox";
        scaleLevelRequirementsToCapCheckbox.Size = new Size(197, 19);
        scaleLevelRequirementsToCapCheckbox.TabIndex = 22;
        scaleLevelRequirementsToCapCheckbox.Text = "Scale Level Requirements to Cap";
        toolTip1.SetToolTip(scaleLevelRequirementsToCapCheckbox, "When selected, experience requirements will be scaled up based on the maximum level from the level cap");
        scaleLevelRequirementsToCapCheckbox.UseVisualStyleBackColor = true;
        // 
        // levelCapLabel
        // 
        levelCapLabel.AutoSize = true;
        levelCapLabel.Location = new Point(88, 108);
        levelCapLabel.Margin = new Padding(4, 0, 4, 0);
        levelCapLabel.Name = "levelCapLabel";
        levelCapLabel.Size = new Size(58, 15);
        levelCapLabel.TabIndex = 21;
        levelCapLabel.Text = "Level Cap";
        // 
        // lifeCapList
        // 
        lifeCapList.DropDownStyle = ComboBoxStyle.DropDownList;
        lifeCapList.FormattingEnabled = true;
        lifeCapList.Items.AddRange(new object[] { "8", "7", "6", "5", "4", "3", "2", "1" });
        lifeCapList.Location = new Point(144, 130);
        lifeCapList.Margin = new Padding(4, 3, 4, 3);
        lifeCapList.Name = "lifeCapList";
        lifeCapList.Size = new Size(37, 23);
        lifeCapList.TabIndex = 20;
        toolTip1.SetToolTip(lifeCapList, "Maximum Life Level");
        lifeCapList.SelectedIndexChanged += EnableLevelScaling;
        // 
        // magCapList
        // 
        magCapList.DropDownStyle = ComboBoxStyle.DropDownList;
        magCapList.FormattingEnabled = true;
        magCapList.Items.AddRange(new object[] { "8", "7", "6", "5", "4", "3", "2", "1" });
        magCapList.Location = new Point(99, 130);
        magCapList.Margin = new Padding(4, 3, 4, 3);
        magCapList.Name = "magCapList";
        magCapList.Size = new Size(37, 23);
        magCapList.TabIndex = 19;
        toolTip1.SetToolTip(magCapList, "Maximum Magic Level");
        magCapList.SelectedIndexChanged += EnableLevelScaling;
        // 
        // atkCapList
        // 
        atkCapList.DropDownStyle = ComboBoxStyle.DropDownList;
        atkCapList.FormattingEnabled = true;
        atkCapList.Items.AddRange(new object[] { "8", "7", "6", "5", "4", "3", "2", "1" });
        atkCapList.Location = new Point(55, 130);
        atkCapList.Margin = new Padding(4, 3, 4, 3);
        atkCapList.Name = "atkCapList";
        atkCapList.Size = new Size(37, 23);
        atkCapList.TabIndex = 18;
        toolTip1.SetToolTip(atkCapList, "Maximum Attack Level");
        atkCapList.SelectedIndexChanged += EnableLevelScaling;
        // 
        // lifeCapLabel
        // 
        lifeCapLabel.AutoSize = true;
        lifeCapLabel.Location = new Point(148, 163);
        lifeCapLabel.Margin = new Padding(4, 0, 4, 0);
        lifeCapLabel.Name = "lifeCapLabel";
        lifeCapLabel.Size = new Size(26, 15);
        lifeCapLabel.TabIndex = 17;
        lifeCapLabel.Text = "Life";
        toolTip1.SetToolTip(lifeCapLabel, "Maximum Life Level");
        // 
        // magCapLabel
        // 
        magCapLabel.AutoSize = true;
        magCapLabel.Location = new Point(102, 163);
        magCapLabel.Margin = new Padding(4, 0, 4, 0);
        magCapLabel.Name = "magCapLabel";
        magCapLabel.Size = new Size(31, 15);
        magCapLabel.TabIndex = 16;
        magCapLabel.Text = "Mag";
        toolTip1.SetToolTip(magCapLabel, "Maximum Magic Level");
        // 
        // attackCapLabel
        // 
        attackCapLabel.AutoSize = true;
        attackCapLabel.Location = new Point(61, 163);
        attackCapLabel.Margin = new Padding(4, 0, 4, 0);
        attackCapLabel.Name = "attackCapLabel";
        attackCapLabel.Size = new Size(25, 15);
        attackCapLabel.TabIndex = 15;
        attackCapLabel.Text = "Atk";
        toolTip1.SetToolTip(attackCapLabel, "Maximum Attack Level");
        // 
        // label12
        // 
        label12.AutoSize = true;
        label12.Location = new Point(261, 114);
        label12.Margin = new Padding(4, 0, 4, 0);
        label12.Name = "label12";
        label12.Size = new Size(100, 15);
        label12.TabIndex = 11;
        label12.Text = "Life Effectiveness:";
        toolTip1.SetToolTip(label12, "Different modes for the effectiveness of Life levels");
        // 
        // magicEffectivenessLabel
        // 
        magicEffectivenessLabel.AutoSize = true;
        magicEffectivenessLabel.Location = new Point(261, 69);
        magicEffectivenessLabel.Margin = new Padding(4, 0, 4, 0);
        magicEffectivenessLabel.Name = "magicEffectivenessLabel";
        magicEffectivenessLabel.Size = new Size(114, 15);
        magicEffectivenessLabel.TabIndex = 10;
        magicEffectivenessLabel.Text = "Magic Effectiveness:";
        toolTip1.SetToolTip(magicEffectivenessLabel, "Different modes for the effectiveness of Magic levels");
        // 
        // attackEffectivenessLabel
        // 
        attackEffectivenessLabel.AutoSize = true;
        attackEffectivenessLabel.Location = new Point(261, 27);
        attackEffectivenessLabel.Margin = new Padding(4, 0, 4, 0);
        attackEffectivenessLabel.Name = "attackEffectivenessLabel";
        attackEffectivenessLabel.Size = new Size(115, 15);
        attackEffectivenessLabel.TabIndex = 9;
        attackEffectivenessLabel.Text = "Attack Effectiveness:";
        toolTip1.SetToolTip(attackEffectivenessLabel, "Different modes for the effectiveness of Attack levels");
        // 
        // expBox
        // 
        expBox.Controls.Add(lifeExpNeededCheckbox);
        expBox.Controls.Add(magicExpNeededCheckbox);
        expBox.Controls.Add(shuffleAtkExpNeededCheckbox);
        expBox.Controls.Add(shuffleAllExpCheckbox);
        expBox.Location = new Point(4, 3);
        expBox.Margin = new Padding(4, 3, 4, 3);
        expBox.Name = "expBox";
        expBox.Padding = new Padding(4, 3, 4, 3);
        expBox.Size = new Size(246, 99);
        expBox.TabIndex = 0;
        expBox.TabStop = false;
        expBox.Text = "                                                      ";
        // 
        // lifeExpNeededCheckbox
        // 
        lifeExpNeededCheckbox.AutoSize = true;
        lifeExpNeededCheckbox.Location = new Point(21, 75);
        lifeExpNeededCheckbox.Margin = new Padding(4, 3, 4, 3);
        lifeExpNeededCheckbox.Name = "lifeExpNeededCheckbox";
        lifeExpNeededCheckbox.Size = new Size(189, 19);
        lifeExpNeededCheckbox.TabIndex = 3;
        lifeExpNeededCheckbox.Text = "Shuffle Life Experience Needed";
        toolTip1.SetToolTip(lifeExpNeededCheckbox, "Shuffles experience needed for life levels");
        lifeExpNeededCheckbox.UseVisualStyleBackColor = true;
        // 
        // magicExpNeededCheckbox
        // 
        magicExpNeededCheckbox.AutoSize = true;
        magicExpNeededCheckbox.Location = new Point(21, 48);
        magicExpNeededCheckbox.Margin = new Padding(4, 3, 4, 3);
        magicExpNeededCheckbox.Name = "magicExpNeededCheckbox";
        magicExpNeededCheckbox.Size = new Size(203, 19);
        magicExpNeededCheckbox.TabIndex = 2;
        magicExpNeededCheckbox.Text = "Shuffle Magic Experience Needed";
        toolTip1.SetToolTip(magicExpNeededCheckbox, "Shuffles experience needed for magic levels");
        magicExpNeededCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleAtkExpNeededCheckbox
        // 
        shuffleAtkExpNeededCheckbox.AutoSize = true;
        shuffleAtkExpNeededCheckbox.Location = new Point(21, 22);
        shuffleAtkExpNeededCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleAtkExpNeededCheckbox.Name = "shuffleAtkExpNeededCheckbox";
        shuffleAtkExpNeededCheckbox.Size = new Size(204, 19);
        shuffleAtkExpNeededCheckbox.TabIndex = 1;
        shuffleAtkExpNeededCheckbox.Text = "Shuffle Attack Experience Needed";
        toolTip1.SetToolTip(shuffleAtkExpNeededCheckbox, "Shuffles experience needed for attack levels");
        shuffleAtkExpNeededCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleAllExpCheckbox
        // 
        shuffleAllExpCheckbox.AutoSize = true;
        shuffleAllExpCheckbox.Location = new Point(7, 0);
        shuffleAllExpCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleAllExpCheckbox.Name = "shuffleAllExpCheckbox";
        shuffleAllExpCheckbox.Size = new Size(184, 19);
        shuffleAllExpCheckbox.TabIndex = 1;
        shuffleAllExpCheckbox.Text = "Shuffle All Experience Needed";
        toolTip1.SetToolTip(shuffleAllExpCheckbox, "Shuffles experience needed for all levels");
        shuffleAllExpCheckbox.UseVisualStyleBackColor = true;
        shuffleAllExpCheckbox.CheckStateChanged += shuffleAllExp_CheckStateChanged;
        // 
        // tabPage9
        // 
        tabPage9.Controls.Add(dashAlwaysOnCheckbox);
        tabPage9.Controls.Add(jumpAlwaysOnCheckbox);
        tabPage9.Controls.Add(FireSpellOptionLabel);
        tabPage9.Controls.Add(FireSpellBox);
        tabPage9.Controls.Add(swapUpAndDownstabCheckbox);
        tabPage9.Controls.Add(randomizeSpellSpellEnemyCheckbox);
        tabPage9.Controls.Add(disableMagicContainerRequirementCheckbox);
        tabPage9.Controls.Add(shuffleSpellLocationsCheckbox);
        tabPage9.Controls.Add(shuffleLifeRefillCheckbox);
        tabPage9.Location = new Point(4, 24);
        tabPage9.Margin = new Padding(4, 3, 4, 3);
        tabPage9.Name = "tabPage9";
        tabPage9.Padding = new Padding(4, 3, 4, 3);
        tabPage9.Size = new Size(595, 331);
        tabPage9.TabIndex = 9;
        tabPage9.Text = "Spells";
        tabPage9.UseVisualStyleBackColor = true;
        // 
        // dashAlwaysOnCheckbox
        // 
        dashAlwaysOnCheckbox.AutoSize = true;
        dashAlwaysOnCheckbox.Location = new Point(297, 8);
        dashAlwaysOnCheckbox.Margin = new Padding(4, 3, 4, 3);
        dashAlwaysOnCheckbox.Name = "dashAlwaysOnCheckbox";
        dashAlwaysOnCheckbox.Size = new Size(111, 19);
        dashAlwaysOnCheckbox.TabIndex = 36;
        dashAlwaysOnCheckbox.Text = "Dash Always On";
        toolTip1.SetToolTip(dashAlwaysOnCheckbox, "The player will always move as though Dash spell were on.,");
        dashAlwaysOnCheckbox.UseVisualStyleBackColor = true;
        // 
        // jumpAlwaysOnCheckbox
        // 
        jumpAlwaysOnCheckbox.AutoSize = true;
        jumpAlwaysOnCheckbox.Location = new Point(297, 33);
        jumpAlwaysOnCheckbox.Margin = new Padding(4, 3, 4, 3);
        jumpAlwaysOnCheckbox.Name = "jumpAlwaysOnCheckbox";
        jumpAlwaysOnCheckbox.Size = new Size(114, 19);
        jumpAlwaysOnCheckbox.TabIndex = 35;
        jumpAlwaysOnCheckbox.Text = "Jump Always On";
        toolTip1.SetToolTip(jumpAlwaysOnCheckbox, "The player will jump very high, as if the jump spell is always active");
        jumpAlwaysOnCheckbox.UseVisualStyleBackColor = true;
        // 
        // FireSpellOptionLabel
        // 
        FireSpellOptionLabel.AutoSize = true;
        FireSpellOptionLabel.Location = new Point(7, 137);
        FireSpellOptionLabel.Margin = new Padding(4, 0, 4, 0);
        FireSpellOptionLabel.Name = "FireSpellOptionLabel";
        FireSpellOptionLabel.Size = new Size(57, 15);
        FireSpellOptionLabel.TabIndex = 24;
        FireSpellOptionLabel.Text = "Fire Spell:";
        toolTip1.SetToolTip(FireSpellOptionLabel, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // FireSpellBox
        // 
        FireSpellBox.DropDownStyle = ComboBoxStyle.DropDownList;
        FireSpellBox.FormattingEnabled = true;
        FireSpellBox.Items.AddRange(new object[] { "Normal", "Link with Random Spell", "Replace with Dash Spell", "Random" });
        FireSpellBox.Location = new Point(7, 154);
        FireSpellBox.Margin = new Padding(4, 3, 4, 3);
        FireSpellBox.Name = "FireSpellBox";
        FireSpellBox.Size = new Size(176, 23);
        FireSpellBox.TabIndex = 23;
        toolTip1.SetToolTip(FireSpellBox, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // swapUpAndDownstabCheckbox
        // 
        swapUpAndDownstabCheckbox.AutoSize = true;
        swapUpAndDownstabCheckbox.Location = new Point(7, 111);
        swapUpAndDownstabCheckbox.Margin = new Padding(4, 3, 4, 3);
        swapUpAndDownstabCheckbox.Name = "swapUpAndDownstabCheckbox";
        swapUpAndDownstabCheckbox.Size = new Size(251, 19);
        swapUpAndDownstabCheckbox.TabIndex = 22;
        swapUpAndDownstabCheckbox.Text = "Swap the location of upstab and downstab";
        swapUpAndDownstabCheckbox.ThreeState = true;
        toolTip1.SetToolTip(swapUpAndDownstabCheckbox, "When selected, Upstab and Downstab will swap sources.");
        swapUpAndDownstabCheckbox.UseVisualStyleBackColor = true;
        // 
        // randomizeSpellSpellEnemyCheckbox
        // 
        randomizeSpellSpellEnemyCheckbox.AutoSize = true;
        randomizeSpellSpellEnemyCheckbox.Location = new Point(7, 86);
        randomizeSpellSpellEnemyCheckbox.Margin = new Padding(4, 3, 4, 3);
        randomizeSpellSpellEnemyCheckbox.Name = "randomizeSpellSpellEnemyCheckbox";
        randomizeSpellSpellEnemyCheckbox.Size = new Size(180, 19);
        randomizeSpellSpellEnemyCheckbox.TabIndex = 20;
        randomizeSpellSpellEnemyCheckbox.Text = "Randomize Spell Spell Enemy";
        randomizeSpellSpellEnemyCheckbox.ThreeState = true;
        toolTip1.SetToolTip(randomizeSpellSpellEnemyCheckbox, "When selected, the enemy generated when the Spell spell is cast will be randomized");
        randomizeSpellSpellEnemyCheckbox.UseVisualStyleBackColor = true;
        // 
        // disableMagicContainerRequirementCheckbox
        // 
        disableMagicContainerRequirementCheckbox.AutoSize = true;
        disableMagicContainerRequirementCheckbox.Location = new Point(7, 61);
        disableMagicContainerRequirementCheckbox.Margin = new Padding(4, 3, 4, 3);
        disableMagicContainerRequirementCheckbox.Name = "disableMagicContainerRequirementCheckbox";
        disableMagicContainerRequirementCheckbox.Size = new Size(231, 19);
        disableMagicContainerRequirementCheckbox.TabIndex = 18;
        disableMagicContainerRequirementCheckbox.Text = "Disable Magic Container Requirements";
        disableMagicContainerRequirementCheckbox.ThreeState = true;
        toolTip1.SetToolTip(disableMagicContainerRequirementCheckbox, "When checked, you can get spells without having the necessary magic containers");
        disableMagicContainerRequirementCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleSpellLocationsCheckbox
        // 
        shuffleSpellLocationsCheckbox.AutoSize = true;
        shuffleSpellLocationsCheckbox.Location = new Point(7, 35);
        shuffleSpellLocationsCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleSpellLocationsCheckbox.Name = "shuffleSpellLocationsCheckbox";
        shuffleSpellLocationsCheckbox.Size = new Size(145, 19);
        shuffleSpellLocationsCheckbox.TabIndex = 17;
        shuffleSpellLocationsCheckbox.Text = "Shuffle Spell Locations";
        shuffleSpellLocationsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shuffleSpellLocationsCheckbox, "This option shuffles which towns you find the spells in");
        shuffleSpellLocationsCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleLifeRefillCheckbox
        // 
        shuffleLifeRefillCheckbox.AutoSize = true;
        shuffleLifeRefillCheckbox.Location = new Point(7, 8);
        shuffleLifeRefillCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleLifeRefillCheckbox.Name = "shuffleLifeRefillCheckbox";
        shuffleLifeRefillCheckbox.Size = new Size(161, 19);
        shuffleLifeRefillCheckbox.TabIndex = 16;
        shuffleLifeRefillCheckbox.Text = "Shuffle Life Refill Amount";
        toolTip1.SetToolTip(shuffleLifeRefillCheckbox, "Shuffles how much health is restored when the life spell is used");
        shuffleLifeRefillCheckbox.UseVisualStyleBackColor = true;
        // 
        // tabPage6
        // 
        tabPage6.Controls.Add(generatorsMatchCheckBox);
        tabPage6.Controls.Add(enemyExperienceDropsLabel);
        tabPage6.Controls.Add(experienceDropsList);
        tabPage6.Controls.Add(shuffleDripperEnemyCheckbox);
        tabPage6.Controls.Add(mixLargeAndSmallCheckbox);
        tabPage6.Controls.Add(label8);
        tabPage6.Controls.Add(shufflePalaceEnemiesCheckbox);
        tabPage6.Controls.Add(shuffleOverworldEnemiesCheckbox);
        tabPage6.Controls.Add(shuffleSwordImmunityBox);
        tabPage6.Controls.Add(shuffleStealXPAmountCheckbox);
        tabPage6.Controls.Add(shuffleXPStealersCheckbox);
        tabPage6.Controls.Add(shuffleEnemyHPBox);
        tabPage6.Location = new Point(4, 24);
        tabPage6.Margin = new Padding(4, 3, 4, 3);
        tabPage6.Name = "tabPage6";
        tabPage6.Size = new Size(595, 331);
        tabPage6.TabIndex = 5;
        tabPage6.Text = "Enemies";
        tabPage6.UseVisualStyleBackColor = true;
        // 
        // generatorsMatchCheckBox
        // 
        generatorsMatchCheckBox.AutoSize = true;
        generatorsMatchCheckBox.Location = new Point(4, 110);
        generatorsMatchCheckBox.Margin = new Padding(4, 3, 4, 3);
        generatorsMatchCheckBox.Name = "generatorsMatchCheckBox";
        generatorsMatchCheckBox.Size = new Size(160, 19);
        generatorsMatchCheckBox.TabIndex = 23;
        generatorsMatchCheckBox.Text = "Generators Always Match";
        toolTip1.SetToolTip(generatorsMatchCheckBox, "Shuffle which enemies require fire to kill");
        generatorsMatchCheckBox.UseVisualStyleBackColor = true;
        // 
        // enemyExperienceDropsLabel
        // 
        enemyExperienceDropsLabel.AutoSize = true;
        enemyExperienceDropsLabel.Location = new Point(226, 147);
        enemyExperienceDropsLabel.Margin = new Padding(4, 0, 4, 0);
        enemyExperienceDropsLabel.Name = "enemyExperienceDropsLabel";
        enemyExperienceDropsLabel.Size = new Size(140, 15);
        enemyExperienceDropsLabel.TabIndex = 22;
        enemyExperienceDropsLabel.Text = "Enemy Experience Drops:";
        toolTip1.SetToolTip(enemyExperienceDropsLabel, "Different modes for how much experience the enemies drop");
        // 
        // experienceDropsList
        // 
        experienceDropsList.DropDownStyle = ComboBoxStyle.DropDownList;
        experienceDropsList.FormattingEnabled = true;
        experienceDropsList.Items.AddRange(new object[] { "Vanilla", "None", "Low", "Average", "High" });
        experienceDropsList.Location = new Point(384, 143);
        experienceDropsList.Margin = new Padding(4, 3, 4, 3);
        experienceDropsList.Name = "experienceDropsList";
        experienceDropsList.Size = new Size(140, 23);
        experienceDropsList.TabIndex = 21;
        toolTip1.SetToolTip(experienceDropsList, "Different modes for how much experience the enemies drop");
        // 
        // shuffleDripperEnemyCheckbox
        // 
        shuffleDripperEnemyCheckbox.AutoSize = true;
        shuffleDripperEnemyCheckbox.Location = new Point(4, 59);
        shuffleDripperEnemyCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleDripperEnemyCheckbox.Name = "shuffleDripperEnemyCheckbox";
        shuffleDripperEnemyCheckbox.Size = new Size(144, 19);
        shuffleDripperEnemyCheckbox.TabIndex = 20;
        shuffleDripperEnemyCheckbox.Text = "Shuffle Dripper Enemy";
        toolTip1.SetToolTip(shuffleDripperEnemyCheckbox, "When selected, the enemy spawned by the dripper will be randomized");
        shuffleDripperEnemyCheckbox.UseVisualStyleBackColor = true;
        // 
        // mixLargeAndSmallCheckbox
        // 
        mixLargeAndSmallCheckbox.AutoSize = true;
        mixLargeAndSmallCheckbox.Location = new Point(4, 85);
        mixLargeAndSmallCheckbox.Margin = new Padding(4, 3, 4, 3);
        mixLargeAndSmallCheckbox.Name = "mixLargeAndSmallCheckbox";
        mixLargeAndSmallCheckbox.Size = new Size(180, 19);
        mixLargeAndSmallCheckbox.TabIndex = 18;
        mixLargeAndSmallCheckbox.Text = "Mix Large and Small Enemies";
        mixLargeAndSmallCheckbox.ThreeState = true;
        toolTip1.SetToolTip(mixLargeAndSmallCheckbox, "Allows large enemies to spawn where small enemies normally spawn and vice versa");
        mixLargeAndSmallCheckbox.UseVisualStyleBackColor = true;
        // 
        // label8
        // 
        label8.BorderStyle = BorderStyle.Fixed3D;
        label8.Location = new Point(202, 6);
        label8.Margin = new Padding(4, 0, 4, 0);
        label8.Name = "label8";
        label8.Size = new Size(2, 225);
        label8.TabIndex = 17;
        // 
        // shufflePalaceEnemiesCheckbox
        // 
        shufflePalaceEnemiesCheckbox.AutoSize = true;
        shufflePalaceEnemiesCheckbox.Location = new Point(4, 32);
        shufflePalaceEnemiesCheckbox.Margin = new Padding(4, 3, 4, 3);
        shufflePalaceEnemiesCheckbox.Name = "shufflePalaceEnemiesCheckbox";
        shufflePalaceEnemiesCheckbox.Size = new Size(147, 19);
        shufflePalaceEnemiesCheckbox.TabIndex = 8;
        shufflePalaceEnemiesCheckbox.Text = "Shuffle Palace Enemies";
        shufflePalaceEnemiesCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shufflePalaceEnemiesCheckbox, "Shuffles enemies in the palaces");
        shufflePalaceEnemiesCheckbox.UseVisualStyleBackColor = true;
        shufflePalaceEnemiesCheckbox.CheckStateChanged += ShuffleEnemies_CheckStateChanged;
        // 
        // shuffleOverworldEnemiesCheckbox
        // 
        shuffleOverworldEnemiesCheckbox.AutoSize = true;
        shuffleOverworldEnemiesCheckbox.Location = new Point(4, 6);
        shuffleOverworldEnemiesCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleOverworldEnemiesCheckbox.Name = "shuffleOverworldEnemiesCheckbox";
        shuffleOverworldEnemiesCheckbox.Size = new Size(168, 19);
        shuffleOverworldEnemiesCheckbox.TabIndex = 7;
        shuffleOverworldEnemiesCheckbox.Text = "Shuffle Overworld Enemies";
        shuffleOverworldEnemiesCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shuffleOverworldEnemiesCheckbox, "Shuffles enemies on the overworld");
        shuffleOverworldEnemiesCheckbox.UseVisualStyleBackColor = true;
        shuffleOverworldEnemiesCheckbox.CheckStateChanged += ShuffleEnemies_CheckStateChanged;
        // 
        // shuffleSwordImmunityBox
        // 
        shuffleSwordImmunityBox.AutoSize = true;
        shuffleSwordImmunityBox.Location = new Point(226, 85);
        shuffleSwordImmunityBox.Margin = new Padding(4, 3, 4, 3);
        shuffleSwordImmunityBox.Name = "shuffleSwordImmunityBox";
        shuffleSwordImmunityBox.Size = new Size(154, 19);
        shuffleSwordImmunityBox.TabIndex = 4;
        shuffleSwordImmunityBox.Text = "Shuffle Sword Immunity";
        toolTip1.SetToolTip(shuffleSwordImmunityBox, "Shuffle which enemies require fire to kill");
        shuffleSwordImmunityBox.UseVisualStyleBackColor = true;
        // 
        // shuffleStealXPAmountCheckbox
        // 
        shuffleStealXPAmountCheckbox.AutoSize = true;
        shuffleStealXPAmountCheckbox.Location = new Point(226, 59);
        shuffleStealXPAmountCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleStealXPAmountCheckbox.Name = "shuffleStealXPAmountCheckbox";
        shuffleStealXPAmountCheckbox.Size = new Size(182, 19);
        shuffleStealXPAmountCheckbox.TabIndex = 3;
        shuffleStealXPAmountCheckbox.Text = "Shuffle Amount of Exp Stolen";
        toolTip1.SetToolTip(shuffleStealXPAmountCheckbox, "Shuffle how much experience is stolen from the player when taking damage from certain enemies");
        shuffleStealXPAmountCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleXPStealersCheckbox
        // 
        shuffleXPStealersCheckbox.AutoSize = true;
        shuffleXPStealersCheckbox.Location = new Point(226, 32);
        shuffleXPStealersCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleXPStealersCheckbox.Name = "shuffleXPStealersCheckbox";
        shuffleXPStealersCheckbox.Size = new Size(197, 19);
        shuffleXPStealersCheckbox.TabIndex = 2;
        shuffleXPStealersCheckbox.Text = "Shuffle Which Enemies Steal Exp";
        toolTip1.SetToolTip(shuffleXPStealersCheckbox, "Shuffle which enemies steal experience when doing damage to the player");
        shuffleXPStealersCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleEnemyHPBox
        // 
        shuffleEnemyHPBox.AutoSize = true;
        shuffleEnemyHPBox.Location = new Point(226, 6);
        shuffleEnemyHPBox.Margin = new Padding(4, 3, 4, 3);
        shuffleEnemyHPBox.Name = "shuffleEnemyHPBox";
        shuffleEnemyHPBox.Size = new Size(121, 19);
        shuffleEnemyHPBox.TabIndex = 0;
        shuffleEnemyHPBox.Text = "Shuffle Enemy HP";
        toolTip1.SetToolTip(shuffleEnemyHPBox, "Each enemy will have +/- 50% of its normal HP");
        shuffleEnemyHPBox.UseVisualStyleBackColor = true;
        // 
        // tabPage7
        // 
        tabPage7.Controls.Add(includeQuestItemsInShuffleCheckbox);
        tabPage7.Controls.Add(includeSwordTechsInShuffleCheckbox);
        tabPage7.Controls.Add(includeSpellsInShuffleCheckbox);
        tabPage7.Controls.Add(shufflePbagAmountsCheckbox);
        tabPage7.Controls.Add(removeSpellitemsCheckbox);
        tabPage7.Controls.Add(includePbagCavesInShuffleCheckbox);
        tabPage7.Controls.Add(randomizeJarRequirementsCheckbox);
        tabPage7.Controls.Add(palacesHaveExtraKeysCheckbox);
        tabPage7.Controls.Add(shuffleSmallItemsCheckbox);
        tabPage7.Controls.Add(mixOverworldPalaceItemsCheckbox);
        tabPage7.Controls.Add(shuffleOverworldItemsCheckbox);
        tabPage7.Controls.Add(shufflePalaceItemsCheckbox);
        tabPage7.Location = new Point(4, 24);
        tabPage7.Margin = new Padding(4, 3, 4, 3);
        tabPage7.Name = "tabPage7";
        tabPage7.Size = new Size(595, 331);
        tabPage7.TabIndex = 7;
        tabPage7.Text = "Items";
        tabPage7.UseVisualStyleBackColor = true;
        // 
        // includeQuestItemsInShuffleCheckbox
        // 
        includeQuestItemsInShuffleCheckbox.AutoSize = true;
        includeQuestItemsInShuffleCheckbox.Location = new Point(4, 159);
        includeQuestItemsInShuffleCheckbox.Margin = new Padding(4, 3, 4, 3);
        includeQuestItemsInShuffleCheckbox.Name = "includeQuestItemsInShuffleCheckbox";
        includeQuestItemsInShuffleCheckbox.Size = new Size(184, 19);
        includeQuestItemsInShuffleCheckbox.TabIndex = 22;
        includeQuestItemsInShuffleCheckbox.Text = "Include Quest Items In Shuffle";
        includeQuestItemsInShuffleCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includeQuestItemsInShuffleCheckbox, "If checked, bagu's mirror, the fountain water, and the saria mirror will be shuffled into the item pool.");
        includeQuestItemsInShuffleCheckbox.UseVisualStyleBackColor = true;
        // 
        // includeSwordTechsInShuffleCheckbox
        // 
        includeSwordTechsInShuffleCheckbox.AutoSize = true;
        includeSwordTechsInShuffleCheckbox.Location = new Point(4, 134);
        includeSwordTechsInShuffleCheckbox.Margin = new Padding(4, 3, 4, 3);
        includeSwordTechsInShuffleCheckbox.Name = "includeSwordTechsInShuffleCheckbox";
        includeSwordTechsInShuffleCheckbox.Size = new Size(186, 19);
        includeSwordTechsInShuffleCheckbox.TabIndex = 21;
        includeSwordTechsInShuffleCheckbox.Text = "Include Sword Techs In Shuffle";
        includeSwordTechsInShuffleCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includeSwordTechsInShuffleCheckbox, "If checked, upstab and downstab will be mixed into the item pool.\r\n");
        includeSwordTechsInShuffleCheckbox.UseVisualStyleBackColor = true;
        includeSwordTechsInShuffleCheckbox.CheckedChanged += SwordTechExclusionHandler;
        // 
        // includeSpellsInShuffleCheckbox
        // 
        includeSpellsInShuffleCheckbox.AutoSize = true;
        includeSpellsInShuffleCheckbox.Location = new Point(4, 109);
        includeSpellsInShuffleCheckbox.Margin = new Padding(4, 3, 4, 3);
        includeSpellsInShuffleCheckbox.Name = "includeSpellsInShuffleCheckbox";
        includeSpellsInShuffleCheckbox.Size = new Size(151, 19);
        includeSpellsInShuffleCheckbox.TabIndex = 20;
        includeSpellsInShuffleCheckbox.Text = "Include Spells In Shuffle";
        includeSpellsInShuffleCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includeSpellsInShuffleCheckbox, "If checked, the wizards in each town could have normal items, and spells could be in item locations.\r\n");
        includeSpellsInShuffleCheckbox.UseVisualStyleBackColor = true;
        includeSpellsInShuffleCheckbox.CheckedChanged += SpellShuffleExclusionHandler;
        // 
        // shufflePbagAmountsCheckbox
        // 
        shufflePbagAmountsCheckbox.AutoSize = true;
        shufflePbagAmountsCheckbox.Location = new Point(281, 109);
        shufflePbagAmountsCheckbox.Margin = new Padding(2);
        shufflePbagAmountsCheckbox.Name = "shufflePbagAmountsCheckbox";
        shufflePbagAmountsCheckbox.Size = new Size(167, 19);
        shufflePbagAmountsCheckbox.TabIndex = 19;
        shufflePbagAmountsCheckbox.Text = "Randomize Pbag Amounts";
        shufflePbagAmountsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shufflePbagAmountsCheckbox, "If selected, the pbag amounts will be randomized.");
        shufflePbagAmountsCheckbox.UseVisualStyleBackColor = true;
        // 
        // removeSpellitemsCheckbox
        // 
        removeSpellitemsCheckbox.AutoSize = true;
        removeSpellitemsCheckbox.Location = new Point(281, 82);
        removeSpellitemsCheckbox.Margin = new Padding(2);
        removeSpellitemsCheckbox.Name = "removeSpellitemsCheckbox";
        removeSpellitemsCheckbox.Size = new Size(138, 19);
        removeSpellitemsCheckbox.TabIndex = 18;
        removeSpellitemsCheckbox.Text = "Start With Spell Items";
        removeSpellitemsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(removeSpellitemsCheckbox, "When checked, you start with the trophy, medicine, child, water, and mirror");
        removeSpellitemsCheckbox.UseVisualStyleBackColor = true;
        removeSpellitemsCheckbox.CheckStateChanged += SpellItemBox_CheckStateChanged;
        // 
        // includePbagCavesInShuffleCheckbox
        // 
        includePbagCavesInShuffleCheckbox.AutoSize = true;
        includePbagCavesInShuffleCheckbox.Location = new Point(4, 82);
        includePbagCavesInShuffleCheckbox.Margin = new Padding(4, 3, 4, 3);
        includePbagCavesInShuffleCheckbox.Name = "includePbagCavesInShuffleCheckbox";
        includePbagCavesInShuffleCheckbox.Size = new Size(209, 19);
        includePbagCavesInShuffleCheckbox.TabIndex = 17;
        includePbagCavesInShuffleCheckbox.Text = "Include Pbag Caves in Item Shuffle";
        includePbagCavesInShuffleCheckbox.ThreeState = true;
        toolTip1.SetToolTip(includePbagCavesInShuffleCheckbox, "Will include the 3 pbag caves as item locations");
        includePbagCavesInShuffleCheckbox.UseVisualStyleBackColor = true;
        // 
        // randomizeJarRequirementsCheckbox
        // 
        randomizeJarRequirementsCheckbox.AutoSize = true;
        randomizeJarRequirementsCheckbox.Location = new Point(281, 57);
        randomizeJarRequirementsCheckbox.Margin = new Padding(4, 3, 4, 3);
        randomizeJarRequirementsCheckbox.Name = "randomizeJarRequirementsCheckbox";
        randomizeJarRequirementsCheckbox.Size = new Size(244, 19);
        randomizeJarRequirementsCheckbox.TabIndex = 16;
        randomizeJarRequirementsCheckbox.Text = "Randomize New Kasuto Jar Requirements";
        toolTip1.SetToolTip(randomizeJarRequirementsCheckbox, "When selected, the number of jars required to get the item in New Kasuto will be randomized between 5 and 7.");
        randomizeJarRequirementsCheckbox.UseVisualStyleBackColor = true;
        // 
        // palacesHaveExtraKeysCheckbox
        // 
        palacesHaveExtraKeysCheckbox.AutoSize = true;
        palacesHaveExtraKeysCheckbox.Location = new Point(281, 30);
        palacesHaveExtraKeysCheckbox.Margin = new Padding(4, 3, 4, 3);
        palacesHaveExtraKeysCheckbox.Name = "palacesHaveExtraKeysCheckbox";
        palacesHaveExtraKeysCheckbox.Size = new Size(166, 19);
        palacesHaveExtraKeysCheckbox.TabIndex = 4;
        palacesHaveExtraKeysCheckbox.Text = "Palaces Contain Extra Keys";
        palacesHaveExtraKeysCheckbox.ThreeState = true;
        toolTip1.SetToolTip(palacesHaveExtraKeysCheckbox, "Inserts a lot of extra keys into the palaces");
        palacesHaveExtraKeysCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleSmallItemsCheckbox
        // 
        shuffleSmallItemsCheckbox.AutoSize = true;
        shuffleSmallItemsCheckbox.Location = new Point(281, 5);
        shuffleSmallItemsCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleSmallItemsCheckbox.Name = "shuffleSmallItemsCheckbox";
        shuffleSmallItemsCheckbox.Size = new Size(127, 19);
        shuffleSmallItemsCheckbox.TabIndex = 3;
        shuffleSmallItemsCheckbox.Text = "Shuffle Small Items";
        toolTip1.SetToolTip(shuffleSmallItemsCheckbox, "Shuffles pbags, jars, and 1ups");
        shuffleSmallItemsCheckbox.UseVisualStyleBackColor = true;
        // 
        // mixOverworldPalaceItemsCheckbox
        // 
        mixOverworldPalaceItemsCheckbox.AutoSize = true;
        mixOverworldPalaceItemsCheckbox.Location = new Point(4, 57);
        mixOverworldPalaceItemsCheckbox.Margin = new Padding(4, 3, 4, 3);
        mixOverworldPalaceItemsCheckbox.Name = "mixOverworldPalaceItemsCheckbox";
        mixOverworldPalaceItemsCheckbox.Size = new Size(196, 19);
        mixOverworldPalaceItemsCheckbox.TabIndex = 2;
        mixOverworldPalaceItemsCheckbox.Text = "Mix Overworld and Palace Items";
        mixOverworldPalaceItemsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(mixOverworldPalaceItemsCheckbox, "Allows palace items to be found in the overworld, and vice versa");
        mixOverworldPalaceItemsCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleOverworldItemsCheckbox
        // 
        shuffleOverworldItemsCheckbox.AutoSize = true;
        shuffleOverworldItemsCheckbox.Location = new Point(4, 30);
        shuffleOverworldItemsCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleOverworldItemsCheckbox.Name = "shuffleOverworldItemsCheckbox";
        shuffleOverworldItemsCheckbox.Size = new Size(153, 19);
        shuffleOverworldItemsCheckbox.TabIndex = 1;
        shuffleOverworldItemsCheckbox.Text = "Shuffle Overworld Items";
        shuffleOverworldItemsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shuffleOverworldItemsCheckbox, "Shuffles the items that are found in the overworld");
        shuffleOverworldItemsCheckbox.UseVisualStyleBackColor = true;
        shuffleOverworldItemsCheckbox.CheckStateChanged += OverworldItemBox_CheckStateChanged;
        // 
        // shufflePalaceItemsCheckbox
        // 
        shufflePalaceItemsCheckbox.AutoSize = true;
        shufflePalaceItemsCheckbox.Location = new Point(4, 3);
        shufflePalaceItemsCheckbox.Margin = new Padding(4, 3, 4, 3);
        shufflePalaceItemsCheckbox.Name = "shufflePalaceItemsCheckbox";
        shufflePalaceItemsCheckbox.Size = new Size(132, 19);
        shufflePalaceItemsCheckbox.TabIndex = 0;
        shufflePalaceItemsCheckbox.Text = "Shuffle Palace Items";
        shufflePalaceItemsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(shufflePalaceItemsCheckbox, "Shuffles the items that are found in palaces");
        shufflePalaceItemsCheckbox.UseVisualStyleBackColor = true;
        shufflePalaceItemsCheckbox.CheckStateChanged += PalaceItemBox_CheckStateChanged;
        // 
        // tabPage8
        // 
        tabPage8.Controls.Add(randomizeDropsCheckbox);
        tabPage8.Controls.Add(standardizeDropsCheckbox);
        tabPage8.Controls.Add(largeEnemiesKeyCheckbox);
        tabPage8.Controls.Add(largeEnemies1UpCheckbox);
        tabPage8.Controls.Add(largeEnemiesXLBagCheckbox);
        tabPage8.Controls.Add(largeEnemiesLargeBagCheckbox);
        tabPage8.Controls.Add(largeEnemiesMediumBagCheckbox);
        tabPage8.Controls.Add(largeEnemiesSmallBagCheckbox);
        tabPage8.Controls.Add(largeEnemiesRedJarCheckbox);
        tabPage8.Controls.Add(largeEnemiesBlueJarCheckbox);
        tabPage8.Controls.Add(largeEnemyPoolLabel);
        tabPage8.Controls.Add(smallEnemiesKeyCheckbox);
        tabPage8.Controls.Add(smallEnemies1UpCheckbox);
        tabPage8.Controls.Add(smallEnemiesXLBagCheckbox);
        tabPage8.Controls.Add(smallEnemiesLargeBagCheckbox);
        tabPage8.Controls.Add(smallEnemiesMediumBagCheckbox);
        tabPage8.Controls.Add(smallEnemiesSmallBagCheckbox);
        tabPage8.Controls.Add(smallEnemiesRedJarCheckbox);
        tabPage8.Controls.Add(smallEnemiesBlueJarCheckbox);
        tabPage8.Controls.Add(smallEnemyPoolLabel);
        tabPage8.Controls.Add(label19);
        tabPage8.Controls.Add(shuffleDropFrequencyCheckbox);
        tabPage8.Location = new Point(4, 24);
        tabPage8.Margin = new Padding(2);
        tabPage8.Name = "tabPage8";
        tabPage8.Padding = new Padding(2);
        tabPage8.Size = new Size(595, 331);
        tabPage8.TabIndex = 8;
        tabPage8.Text = "Drops";
        tabPage8.UseVisualStyleBackColor = true;
        // 
        // randomizeDropsCheckbox
        // 
        randomizeDropsCheckbox.AutoSize = true;
        randomizeDropsCheckbox.Location = new Point(9, 36);
        randomizeDropsCheckbox.Margin = new Padding(2);
        randomizeDropsCheckbox.Name = "randomizeDropsCheckbox";
        randomizeDropsCheckbox.Size = new Size(119, 19);
        randomizeDropsCheckbox.TabIndex = 45;
        randomizeDropsCheckbox.Text = "Randomize Drops";
        toolTip1.SetToolTip(randomizeDropsCheckbox, "When selected, the items in the drop pool will be randomized");
        randomizeDropsCheckbox.UseVisualStyleBackColor = true;
        // 
        // standardizeDropsCheckbox
        // 
        standardizeDropsCheckbox.AutoSize = true;
        standardizeDropsCheckbox.Location = new Point(9, 60);
        standardizeDropsCheckbox.Margin = new Padding(2);
        standardizeDropsCheckbox.Name = "standardizeDropsCheckbox";
        standardizeDropsCheckbox.Size = new Size(121, 19);
        standardizeDropsCheckbox.TabIndex = 44;
        standardizeDropsCheckbox.Text = "Standardize Drops";
        toolTip1.SetToolTip(standardizeDropsCheckbox, "When selected, all runners playing the same seed will get the same drops in the same order");
        standardizeDropsCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesKeyCheckbox
        // 
        largeEnemiesKeyCheckbox.AutoSize = true;
        largeEnemiesKeyCheckbox.Location = new Point(404, 204);
        largeEnemiesKeyCheckbox.Margin = new Padding(2);
        largeEnemiesKeyCheckbox.Name = "largeEnemiesKeyCheckbox";
        largeEnemiesKeyCheckbox.Size = new Size(45, 19);
        largeEnemiesKeyCheckbox.TabIndex = 43;
        largeEnemiesKeyCheckbox.Text = "Key";
        toolTip1.SetToolTip(largeEnemiesKeyCheckbox, "Add small keys to the large enemy pool");
        largeEnemiesKeyCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemies1UpCheckbox
        // 
        largeEnemies1UpCheckbox.AutoSize = true;
        largeEnemies1UpCheckbox.Location = new Point(404, 180);
        largeEnemies1UpCheckbox.Margin = new Padding(2);
        largeEnemies1UpCheckbox.Name = "largeEnemies1UpCheckbox";
        largeEnemies1UpCheckbox.Size = new Size(46, 19);
        largeEnemies1UpCheckbox.TabIndex = 42;
        largeEnemies1UpCheckbox.Text = "1up";
        toolTip1.SetToolTip(largeEnemies1UpCheckbox, "Add 1ups to the large enemy pool");
        largeEnemies1UpCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesXLBagCheckbox
        // 
        largeEnemiesXLBagCheckbox.AutoSize = true;
        largeEnemiesXLBagCheckbox.Location = new Point(404, 156);
        largeEnemiesXLBagCheckbox.Margin = new Padding(2);
        largeEnemiesXLBagCheckbox.Name = "largeEnemiesXLBagCheckbox";
        largeEnemiesXLBagCheckbox.Size = new Size(74, 19);
        largeEnemiesXLBagCheckbox.TabIndex = 41;
        largeEnemiesXLBagCheckbox.Text = "500 pbag";
        toolTip1.SetToolTip(largeEnemiesXLBagCheckbox, "Add 500 bags to the large enemy pool");
        largeEnemiesXLBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesLargeBagCheckbox
        // 
        largeEnemiesLargeBagCheckbox.AutoSize = true;
        largeEnemiesLargeBagCheckbox.Checked = true;
        largeEnemiesLargeBagCheckbox.CheckState = CheckState.Checked;
        largeEnemiesLargeBagCheckbox.Location = new Point(404, 132);
        largeEnemiesLargeBagCheckbox.Margin = new Padding(2);
        largeEnemiesLargeBagCheckbox.Name = "largeEnemiesLargeBagCheckbox";
        largeEnemiesLargeBagCheckbox.Size = new Size(74, 19);
        largeEnemiesLargeBagCheckbox.TabIndex = 40;
        largeEnemiesLargeBagCheckbox.Text = "200 pbag";
        toolTip1.SetToolTip(largeEnemiesLargeBagCheckbox, "Add 200 bags to the large enemy pool");
        largeEnemiesLargeBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesMediumBagCheckbox
        // 
        largeEnemiesMediumBagCheckbox.AutoSize = true;
        largeEnemiesMediumBagCheckbox.Location = new Point(404, 108);
        largeEnemiesMediumBagCheckbox.Margin = new Padding(2);
        largeEnemiesMediumBagCheckbox.Name = "largeEnemiesMediumBagCheckbox";
        largeEnemiesMediumBagCheckbox.Size = new Size(74, 19);
        largeEnemiesMediumBagCheckbox.TabIndex = 39;
        largeEnemiesMediumBagCheckbox.Text = "100 pbag";
        toolTip1.SetToolTip(largeEnemiesMediumBagCheckbox, "Add 100 bags to the large enemy pool");
        largeEnemiesMediumBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesSmallBagCheckbox
        // 
        largeEnemiesSmallBagCheckbox.AutoSize = true;
        largeEnemiesSmallBagCheckbox.Location = new Point(404, 84);
        largeEnemiesSmallBagCheckbox.Margin = new Padding(2);
        largeEnemiesSmallBagCheckbox.Name = "largeEnemiesSmallBagCheckbox";
        largeEnemiesSmallBagCheckbox.Size = new Size(68, 19);
        largeEnemiesSmallBagCheckbox.TabIndex = 38;
        largeEnemiesSmallBagCheckbox.Text = "50 pbag";
        toolTip1.SetToolTip(largeEnemiesSmallBagCheckbox, "Add 50 bags to the large enemy pool");
        largeEnemiesSmallBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesRedJarCheckbox
        // 
        largeEnemiesRedJarCheckbox.AutoSize = true;
        largeEnemiesRedJarCheckbox.Checked = true;
        largeEnemiesRedJarCheckbox.CheckState = CheckState.Checked;
        largeEnemiesRedJarCheckbox.Location = new Point(404, 60);
        largeEnemiesRedJarCheckbox.Margin = new Padding(2);
        largeEnemiesRedJarCheckbox.Name = "largeEnemiesRedJarCheckbox";
        largeEnemiesRedJarCheckbox.Size = new Size(63, 19);
        largeEnemiesRedJarCheckbox.TabIndex = 37;
        largeEnemiesRedJarCheckbox.Text = "Red Jar";
        toolTip1.SetToolTip(largeEnemiesRedJarCheckbox, "Add red jars to the large enemy pool");
        largeEnemiesRedJarCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemiesBlueJarCheckbox
        // 
        largeEnemiesBlueJarCheckbox.AutoSize = true;
        largeEnemiesBlueJarCheckbox.Location = new Point(404, 36);
        largeEnemiesBlueJarCheckbox.Margin = new Padding(2);
        largeEnemiesBlueJarCheckbox.Name = "largeEnemiesBlueJarCheckbox";
        largeEnemiesBlueJarCheckbox.Size = new Size(66, 19);
        largeEnemiesBlueJarCheckbox.TabIndex = 36;
        largeEnemiesBlueJarCheckbox.Text = "Blue Jar";
        toolTip1.SetToolTip(largeEnemiesBlueJarCheckbox, "Add blue jars to the large enemy pool");
        largeEnemiesBlueJarCheckbox.UseVisualStyleBackColor = true;
        // 
        // largeEnemyPoolLabel
        // 
        largeEnemyPoolLabel.AutoSize = true;
        largeEnemyPoolLabel.Location = new Point(392, 12);
        largeEnemyPoolLabel.Margin = new Padding(2, 0, 2, 0);
        largeEnemyPoolLabel.Name = "largeEnemyPoolLabel";
        largeEnemyPoolLabel.Size = new Size(102, 15);
        largeEnemyPoolLabel.TabIndex = 35;
        largeEnemyPoolLabel.Text = "Large Enemy Pool";
        // 
        // smallEnemiesKeyCheckbox
        // 
        smallEnemiesKeyCheckbox.AutoSize = true;
        smallEnemiesKeyCheckbox.Location = new Point(254, 204);
        smallEnemiesKeyCheckbox.Margin = new Padding(2);
        smallEnemiesKeyCheckbox.Name = "smallEnemiesKeyCheckbox";
        smallEnemiesKeyCheckbox.Size = new Size(45, 19);
        smallEnemiesKeyCheckbox.TabIndex = 34;
        smallEnemiesKeyCheckbox.Text = "Key";
        toolTip1.SetToolTip(smallEnemiesKeyCheckbox, "Add small keys to the small enemy pool");
        smallEnemiesKeyCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemies1UpCheckbox
        // 
        smallEnemies1UpCheckbox.AutoSize = true;
        smallEnemies1UpCheckbox.Location = new Point(255, 180);
        smallEnemies1UpCheckbox.Margin = new Padding(2);
        smallEnemies1UpCheckbox.Name = "smallEnemies1UpCheckbox";
        smallEnemies1UpCheckbox.Size = new Size(46, 19);
        smallEnemies1UpCheckbox.TabIndex = 33;
        smallEnemies1UpCheckbox.Text = "1up";
        toolTip1.SetToolTip(smallEnemies1UpCheckbox, "Add 1ups to the small enemy pool");
        smallEnemies1UpCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemiesXLBagCheckbox
        // 
        smallEnemiesXLBagCheckbox.AutoSize = true;
        smallEnemiesXLBagCheckbox.Location = new Point(255, 156);
        smallEnemiesXLBagCheckbox.Margin = new Padding(2);
        smallEnemiesXLBagCheckbox.Name = "smallEnemiesXLBagCheckbox";
        smallEnemiesXLBagCheckbox.Size = new Size(74, 19);
        smallEnemiesXLBagCheckbox.TabIndex = 32;
        smallEnemiesXLBagCheckbox.Text = "500 pbag";
        toolTip1.SetToolTip(smallEnemiesXLBagCheckbox, "Add 500 bags to the small enemy pool");
        smallEnemiesXLBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemiesLargeBagCheckbox
        // 
        smallEnemiesLargeBagCheckbox.AutoSize = true;
        smallEnemiesLargeBagCheckbox.Location = new Point(255, 132);
        smallEnemiesLargeBagCheckbox.Margin = new Padding(2);
        smallEnemiesLargeBagCheckbox.Name = "smallEnemiesLargeBagCheckbox";
        smallEnemiesLargeBagCheckbox.Size = new Size(74, 19);
        smallEnemiesLargeBagCheckbox.TabIndex = 31;
        smallEnemiesLargeBagCheckbox.Text = "200 pbag";
        toolTip1.SetToolTip(smallEnemiesLargeBagCheckbox, "Add 200 bags to the small enemy pool");
        smallEnemiesLargeBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemiesMediumBagCheckbox
        // 
        smallEnemiesMediumBagCheckbox.AutoSize = true;
        smallEnemiesMediumBagCheckbox.Location = new Point(255, 108);
        smallEnemiesMediumBagCheckbox.Margin = new Padding(2);
        smallEnemiesMediumBagCheckbox.Name = "smallEnemiesMediumBagCheckbox";
        smallEnemiesMediumBagCheckbox.Size = new Size(74, 19);
        smallEnemiesMediumBagCheckbox.TabIndex = 30;
        smallEnemiesMediumBagCheckbox.Text = "100 pbag";
        toolTip1.SetToolTip(smallEnemiesMediumBagCheckbox, "Add 100 bags to the small enemy pool");
        smallEnemiesMediumBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemiesSmallBagCheckbox
        // 
        smallEnemiesSmallBagCheckbox.AutoSize = true;
        smallEnemiesSmallBagCheckbox.Checked = true;
        smallEnemiesSmallBagCheckbox.CheckState = CheckState.Checked;
        smallEnemiesSmallBagCheckbox.Location = new Point(255, 84);
        smallEnemiesSmallBagCheckbox.Margin = new Padding(2);
        smallEnemiesSmallBagCheckbox.Name = "smallEnemiesSmallBagCheckbox";
        smallEnemiesSmallBagCheckbox.Size = new Size(68, 19);
        smallEnemiesSmallBagCheckbox.TabIndex = 29;
        smallEnemiesSmallBagCheckbox.Text = "50 pbag";
        toolTip1.SetToolTip(smallEnemiesSmallBagCheckbox, "Add 50 bags to the small enemy pool");
        smallEnemiesSmallBagCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemiesRedJarCheckbox
        // 
        smallEnemiesRedJarCheckbox.AutoSize = true;
        smallEnemiesRedJarCheckbox.Location = new Point(255, 60);
        smallEnemiesRedJarCheckbox.Margin = new Padding(2);
        smallEnemiesRedJarCheckbox.Name = "smallEnemiesRedJarCheckbox";
        smallEnemiesRedJarCheckbox.Size = new Size(63, 19);
        smallEnemiesRedJarCheckbox.TabIndex = 28;
        smallEnemiesRedJarCheckbox.Text = "Red Jar";
        toolTip1.SetToolTip(smallEnemiesRedJarCheckbox, "Add red jars to the small enemy pool");
        smallEnemiesRedJarCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemiesBlueJarCheckbox
        // 
        smallEnemiesBlueJarCheckbox.AutoSize = true;
        smallEnemiesBlueJarCheckbox.Checked = true;
        smallEnemiesBlueJarCheckbox.CheckState = CheckState.Checked;
        smallEnemiesBlueJarCheckbox.Location = new Point(255, 36);
        smallEnemiesBlueJarCheckbox.Margin = new Padding(2);
        smallEnemiesBlueJarCheckbox.Name = "smallEnemiesBlueJarCheckbox";
        smallEnemiesBlueJarCheckbox.Size = new Size(66, 19);
        smallEnemiesBlueJarCheckbox.TabIndex = 27;
        smallEnemiesBlueJarCheckbox.Text = "Blue Jar";
        toolTip1.SetToolTip(smallEnemiesBlueJarCheckbox, "Add blue jars to the small enemy pool");
        smallEnemiesBlueJarCheckbox.UseVisualStyleBackColor = true;
        // 
        // smallEnemyPoolLabel
        // 
        smallEnemyPoolLabel.AutoSize = true;
        smallEnemyPoolLabel.Location = new Point(243, 12);
        smallEnemyPoolLabel.Margin = new Padding(2, 0, 2, 0);
        smallEnemyPoolLabel.Name = "smallEnemyPoolLabel";
        smallEnemyPoolLabel.Size = new Size(102, 15);
        smallEnemyPoolLabel.TabIndex = 25;
        smallEnemyPoolLabel.Text = "Small Enemy Pool";
        // 
        // label19
        // 
        label19.BorderStyle = BorderStyle.Fixed3D;
        label19.Location = new Point(209, 12);
        label19.Margin = new Padding(4, 0, 4, 0);
        label19.Name = "label19";
        label19.Size = new Size(2, 225);
        label19.TabIndex = 24;
        // 
        // shuffleDropFrequencyCheckbox
        // 
        shuffleDropFrequencyCheckbox.AutoSize = true;
        shuffleDropFrequencyCheckbox.Location = new Point(9, 12);
        shuffleDropFrequencyCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleDropFrequencyCheckbox.Name = "shuffleDropFrequencyCheckbox";
        shuffleDropFrequencyCheckbox.Size = new Size(177, 19);
        shuffleDropFrequencyCheckbox.TabIndex = 22;
        shuffleDropFrequencyCheckbox.Text = "Shuffle Item Drop Frequency";
        toolTip1.SetToolTip(shuffleDropFrequencyCheckbox, "This option will shuffle how often enemies drop pbags and jars");
        shuffleDropFrequencyCheckbox.UseVisualStyleBackColor = true;
        // 
        // miscTabPage
        // 
        miscTabPage.Controls.Add(randomizeKnockbackCheckbox);
        miscTabPage.Controls.Add(shuffleEnemyPalettesCheckbox);
        miscTabPage.Controls.Add(alwaysBeamCheckbox);
        miscTabPage.Controls.Add(enableTownNameHintsCheckbox);
        miscTabPage.Controls.Add(enableSpellItemHintsCheckbox);
        miscTabPage.Controls.Add(enableHelpfulHintsCheckbox);
        miscTabPage.Location = new Point(4, 24);
        miscTabPage.Margin = new Padding(4, 3, 4, 3);
        miscTabPage.Name = "miscTabPage";
        miscTabPage.Size = new Size(595, 331);
        miscTabPage.TabIndex = 10;
        miscTabPage.Text = "Misc";
        miscTabPage.UseVisualStyleBackColor = true;
        // 
        // randomizeKnockbackCheckbox
        // 
        randomizeKnockbackCheckbox.AutoSize = true;
        randomizeKnockbackCheckbox.Checked = true;
        randomizeKnockbackCheckbox.CheckState = CheckState.Checked;
        randomizeKnockbackCheckbox.Location = new Point(4, 128);
        randomizeKnockbackCheckbox.Margin = new Padding(4, 3, 4, 3);
        randomizeKnockbackCheckbox.Name = "randomizeKnockbackCheckbox";
        randomizeKnockbackCheckbox.Size = new Size(146, 19);
        randomizeKnockbackCheckbox.TabIndex = 26;
        randomizeKnockbackCheckbox.Text = "Randomize Knockback";
        toolTip1.SetToolTip(randomizeKnockbackCheckbox, "Randomizes knockback when link and enemies collide.");
        randomizeKnockbackCheckbox.UseVisualStyleBackColor = true;
        // 
        // shuffleEnemyPalettesCheckbox
        // 
        shuffleEnemyPalettesCheckbox.AutoSize = true;
        shuffleEnemyPalettesCheckbox.Location = new Point(4, 78);
        shuffleEnemyPalettesCheckbox.Margin = new Padding(4, 3, 4, 3);
        shuffleEnemyPalettesCheckbox.Name = "shuffleEnemyPalettesCheckbox";
        shuffleEnemyPalettesCheckbox.Size = new Size(140, 19);
        shuffleEnemyPalettesCheckbox.TabIndex = 25;
        shuffleEnemyPalettesCheckbox.Text = "Shuffle Sprite Palettes";
        toolTip1.SetToolTip(shuffleEnemyPalettesCheckbox, "When selected, sprite colors will be shuffled");
        shuffleEnemyPalettesCheckbox.UseVisualStyleBackColor = true;
        // 
        // alwaysBeamCheckbox
        // 
        alwaysBeamCheckbox.AutoSize = true;
        alwaysBeamCheckbox.Location = new Point(4, 103);
        alwaysBeamCheckbox.Margin = new Padding(4, 3, 4, 3);
        alwaysBeamCheckbox.Name = "alwaysBeamCheckbox";
        alwaysBeamCheckbox.Size = new Size(153, 19);
        alwaysBeamCheckbox.TabIndex = 24;
        alwaysBeamCheckbox.Text = "Permanent Beam Sword";
        toolTip1.SetToolTip(alwaysBeamCheckbox, "Gives Link beam sword regardless of how much health he has");
        alwaysBeamCheckbox.UseVisualStyleBackColor = true;
        // 
        // enableTownNameHintsCheckbox
        // 
        enableTownNameHintsCheckbox.AutoSize = true;
        enableTownNameHintsCheckbox.Location = new Point(4, 53);
        enableTownNameHintsCheckbox.Margin = new Padding(4, 3, 4, 3);
        enableTownNameHintsCheckbox.Name = "enableTownNameHintsCheckbox";
        enableTownNameHintsCheckbox.Size = new Size(158, 19);
        enableTownNameHintsCheckbox.TabIndex = 23;
        enableTownNameHintsCheckbox.Text = "Enable Town Name Hints";
        enableTownNameHintsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(enableTownNameHintsCheckbox, "Signs at the beginning of town will tell you what spell is contained in the town.");
        enableTownNameHintsCheckbox.UseVisualStyleBackColor = true;
        // 
        // enableSpellItemHintsCheckbox
        // 
        enableSpellItemHintsCheckbox.AutoSize = true;
        enableSpellItemHintsCheckbox.Location = new Point(4, 29);
        enableSpellItemHintsCheckbox.Margin = new Padding(4, 3, 4, 3);
        enableSpellItemHintsCheckbox.Name = "enableSpellItemHintsCheckbox";
        enableSpellItemHintsCheckbox.Size = new Size(147, 19);
        enableSpellItemHintsCheckbox.TabIndex = 22;
        enableSpellItemHintsCheckbox.Text = "Enable Spell Item Hints";
        enableSpellItemHintsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(enableSpellItemHintsCheckbox, "The people who require spell items will tell you where the item can be found.");
        enableSpellItemHintsCheckbox.UseVisualStyleBackColor = true;
        // 
        // enableHelpfulHintsCheckbox
        // 
        enableHelpfulHintsCheckbox.AutoSize = true;
        enableHelpfulHintsCheckbox.Location = new Point(4, 3);
        enableHelpfulHintsCheckbox.Margin = new Padding(4, 3, 4, 3);
        enableHelpfulHintsCheckbox.Name = "enableHelpfulHintsCheckbox";
        enableHelpfulHintsCheckbox.Size = new Size(134, 19);
        enableHelpfulHintsCheckbox.TabIndex = 0;
        enableHelpfulHintsCheckbox.Text = "Enable Helpful Hints";
        enableHelpfulHintsCheckbox.ThreeState = true;
        toolTip1.SetToolTip(enableHelpfulHintsCheckbox, "Townspeople will give you helpful hints as to where items are located.");
        enableHelpfulHintsCheckbox.UseVisualStyleBackColor = true;
        // 
        // CustomTabPage
        // 
        CustomTabPage.Controls.Add(disableHUDLag);
        CustomTabPage.Controls.Add(useCommunityTextCheckbox);
        CustomTabPage.Controls.Add(spriteCreditLabel);
        CustomTabPage.Controls.Add(beepFrequencyDropdown);
        CustomTabPage.Controls.Add(label10);
        CustomTabPage.Controls.Add(beepThresholdDropdown);
        CustomTabPage.Controls.Add(label9);
        CustomTabPage.Controls.Add(spritePreviewLabel);
        CustomTabPage.Controls.Add(spritePreviewBox);
        CustomTabPage.Controls.Add(flashingOffCheckbox);
        CustomTabPage.Controls.Add(upAOnController1Checkbox);
        CustomTabPage.Controls.Add(beamSpriteList);
        CustomTabPage.Controls.Add(beamSpriteLabel);
        CustomTabPage.Controls.Add(shieldColorList);
        CustomTabPage.Controls.Add(shieldColorLabel);
        CustomTabPage.Controls.Add(tunicColorList);
        CustomTabPage.Controls.Add(tunicColorLabel);
        CustomTabPage.Controls.Add(characterSpriteList);
        CustomTabPage.Controls.Add(characterSpriteLabel);
        CustomTabPage.Controls.Add(disableMusicCheckbox);
        CustomTabPage.Controls.Add(fastSpellCheckbox);
        CustomTabPage.Location = new Point(4, 24);
        CustomTabPage.Margin = new Padding(4, 3, 4, 3);
        CustomTabPage.Name = "CustomTabPage";
        CustomTabPage.Size = new Size(595, 331);
        CustomTabPage.TabIndex = 6;
        CustomTabPage.Text = "Customize";
        CustomTabPage.UseVisualStyleBackColor = true;
        // 
        // disableHUDLag
        // 
        disableHUDLag.AutoSize = true;
        disableHUDLag.Location = new Point(4, 149);
        disableHUDLag.Margin = new Padding(2);
        disableHUDLag.Name = "disableHUDLag";
        disableHUDLag.Size = new Size(161, 19);
        disableHUDLag.TabIndex = 44;
        disableHUDLag.Text = "Disable HUD Flash on Lag";
        toolTip1.SetToolTip(disableHUDLag, "When enabled, fixes the issue causing the whole HUD to flash when lag happens. A glove icon is added to the HUD as a visual indicator for lag");
        disableHUDLag.UseVisualStyleBackColor = true;
        // 
        // useCommunityTextCheckbox
        // 
        useCommunityTextCheckbox.AutoSize = true;
        useCommunityTextCheckbox.Location = new Point(4, 100);
        useCommunityTextCheckbox.Margin = new Padding(2);
        useCommunityTextCheckbox.Name = "useCommunityTextCheckbox";
        useCommunityTextCheckbox.Size = new Size(114, 19);
        useCommunityTextCheckbox.TabIndex = 43;
        useCommunityTextCheckbox.Text = "Community Text";
        toolTip1.SetToolTip(useCommunityTextCheckbox, "When selected, will replace some text with text submitted by the community");
        useCommunityTextCheckbox.UseVisualStyleBackColor = true;
        // 
        // spriteCreditLabel
        // 
        spriteCreditLabel.Anchor = AnchorStyles.Right;
        spriteCreditLabel.Location = new Point(441, 227);
        spriteCreditLabel.Name = "spriteCreditLabel";
        spriteCreditLabel.Size = new Size(129, 23);
        spriteCreditLabel.TabIndex = 42;
        spriteCreditLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // beepFrequencyDropdown
        // 
        beepFrequencyDropdown.DropDownStyle = ComboBoxStyle.DropDownList;
        beepFrequencyDropdown.FormattingEnabled = true;
        beepFrequencyDropdown.Items.AddRange(new object[] { "Normal", "Half Speed", "Quarter Speed", "Off" });
        beepFrequencyDropdown.Location = new Point(8, 249);
        beepFrequencyDropdown.Margin = new Padding(2);
        beepFrequencyDropdown.Name = "beepFrequencyDropdown";
        beepFrequencyDropdown.Size = new Size(142, 23);
        beepFrequencyDropdown.TabIndex = 41;
        toolTip1.SetToolTip(beepFrequencyDropdown, "Controls how often the beep occurs when on low life.");
        // 
        // label10
        // 
        label10.AutoSize = true;
        label10.Location = new Point(4, 226);
        label10.Margin = new Padding(2, 0, 2, 0);
        label10.Name = "label10";
        label10.Size = new Size(91, 15);
        label10.TabIndex = 40;
        label10.Text = "Beep Frequency";
        // 
        // beepThresholdDropdown
        // 
        beepThresholdDropdown.DropDownStyle = ComboBoxStyle.DropDownList;
        beepThresholdDropdown.FormattingEnabled = true;
        beepThresholdDropdown.Items.AddRange(new object[] { "Normal", "Half Bar", "Quarter Bar", "Two Bars" });
        beepThresholdDropdown.Location = new Point(8, 193);
        beepThresholdDropdown.Margin = new Padding(2);
        beepThresholdDropdown.Name = "beepThresholdDropdown";
        beepThresholdDropdown.Size = new Size(142, 23);
        beepThresholdDropdown.TabIndex = 39;
        toolTip1.SetToolTip(beepThresholdDropdown, "Controls how much health you must be down to before beeping starts.");
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.Location = new Point(4, 170);
        label9.Margin = new Padding(2, 0, 2, 0);
        label9.Name = "label9";
        label9.Size = new Size(88, 15);
        label9.TabIndex = 38;
        label9.Text = "Beep Threshold";
        // 
        // spritePreviewLabel
        // 
        spritePreviewLabel.AutoSize = true;
        spritePreviewLabel.Location = new Point(473, 3);
        spritePreviewLabel.Name = "spritePreviewLabel";
        spritePreviewLabel.Size = new Size(81, 15);
        spritePreviewLabel.TabIndex = 37;
        spritePreviewLabel.Text = "Sprite Preview";
        // 
        // spritePreviewBox
        // 
        spritePreviewBox.Location = new Point(472, 24);
        spritePreviewBox.Margin = new Padding(0);
        spritePreviewBox.Name = "spritePreviewBox";
        spritePreviewBox.Size = new Size(98, 194);
        spritePreviewBox.TabIndex = 36;
        spritePreviewBox.TabStop = false;
        // 
        // flashingOffCheckbox
        // 
        flashingOffCheckbox.AutoSize = true;
        flashingOffCheckbox.Checked = true;
        flashingOffCheckbox.CheckState = CheckState.Checked;
        flashingOffCheckbox.Location = new Point(4, 77);
        flashingOffCheckbox.Margin = new Padding(4, 3, 4, 3);
        flashingOffCheckbox.Name = "flashingOffCheckbox";
        flashingOffCheckbox.Size = new Size(182, 19);
        flashingOffCheckbox.TabIndex = 33;
        flashingOffCheckbox.Text = "Remove Flashing Upon Death";
        toolTip1.SetToolTip(flashingOffCheckbox, "When selected, the flashing animation after Link's death will be removed.");
        flashingOffCheckbox.UseVisualStyleBackColor = true;
        // 
        // upAOnController1Checkbox
        // 
        upAOnController1Checkbox.AutoSize = true;
        upAOnController1Checkbox.Location = new Point(4, 52);
        upAOnController1Checkbox.Margin = new Padding(4, 3, 4, 3);
        upAOnController1Checkbox.Name = "upAOnController1Checkbox";
        upAOnController1Checkbox.Size = new Size(250, 19);
        upAOnController1Checkbox.TabIndex = 32;
        upAOnController1Checkbox.Text = "Remap Up+A to Up+Select on Controller 1";
        toolTip1.SetToolTip(upAOnController1Checkbox, "When selected, Up+A on controller 2 will be remapped to Up+Select on Controller 1");
        upAOnController1Checkbox.UseVisualStyleBackColor = true;
        // 
        // beamSpriteList
        // 
        beamSpriteList.DropDownStyle = ComboBoxStyle.DropDownList;
        beamSpriteList.FormattingEnabled = true;
        beamSpriteList.Items.AddRange(new object[] { "Default", "Fire", "Bubble", "Rock", "Axe", "Hammer", "Wizzrobe Beam", "Random" });
        beamSpriteList.Location = new Point(298, 193);
        beamSpriteList.Margin = new Padding(2);
        beamSpriteList.Name = "beamSpriteList";
        beamSpriteList.Size = new Size(142, 23);
        beamSpriteList.TabIndex = 31;
        toolTip1.SetToolTip(beamSpriteList, "Allows you to select what the beam sprite will be");
        beamSpriteList.SelectedIndexChanged += BeamSpriteList_SelectedIndexChanged;
        // 
        // beamSpriteLabel
        // 
        beamSpriteLabel.AutoSize = true;
        beamSpriteLabel.Location = new Point(294, 170);
        beamSpriteLabel.Margin = new Padding(2, 0, 2, 0);
        beamSpriteLabel.Name = "beamSpriteLabel";
        beamSpriteLabel.Size = new Size(73, 15);
        beamSpriteLabel.TabIndex = 30;
        beamSpriteLabel.Text = "Beam Sprite:";
        // 
        // shieldColorList
        // 
        shieldColorList.DropDownStyle = ComboBoxStyle.DropDownList;
        shieldColorList.FormattingEnabled = true;
        shieldColorList.Items.AddRange(new object[] { "Default", "Green", "Dark Green", "Aqua", "Dark Blue", "Purple", "Pink", "Orange", "Red", "Turd", "Random" });
        shieldColorList.Location = new Point(298, 136);
        shieldColorList.Margin = new Padding(2);
        shieldColorList.Name = "shieldColorList";
        shieldColorList.Size = new Size(142, 23);
        shieldColorList.TabIndex = 29;
        toolTip1.SetToolTip(shieldColorList, "Changes the tunic color for shield");
        shieldColorList.SelectedIndexChanged += ShieldColorList_SelectedIndexChanged;
        // 
        // shieldColorLabel
        // 
        shieldColorLabel.AutoSize = true;
        shieldColorLabel.Location = new Point(294, 112);
        shieldColorLabel.Margin = new Padding(2, 0, 2, 0);
        shieldColorLabel.Name = "shieldColorLabel";
        shieldColorLabel.Size = new Size(106, 15);
        shieldColorLabel.TabIndex = 28;
        shieldColorLabel.Text = "Shield Tunic Color:";
        // 
        // tunicColorList
        // 
        tunicColorList.DropDownStyle = ComboBoxStyle.DropDownList;
        tunicColorList.FormattingEnabled = true;
        tunicColorList.Items.AddRange(new object[] { "Default", "Green", "Dark Green", "Aqua", "Dark Blue", "Purple", "Pink", "Orange", "Red", "Turd", "Random" });
        tunicColorList.Location = new Point(298, 80);
        tunicColorList.Margin = new Padding(2);
        tunicColorList.Name = "tunicColorList";
        tunicColorList.Size = new Size(142, 23);
        tunicColorList.TabIndex = 27;
        toolTip1.SetToolTip(tunicColorList, "Changes the normal tunic color");
        tunicColorList.SelectedIndexChanged += TunicColorList_SelectedIndexChanged;
        // 
        // tunicColorLabel
        // 
        tunicColorLabel.AutoSize = true;
        tunicColorLabel.Location = new Point(294, 54);
        tunicColorLabel.Margin = new Padding(2, 0, 2, 0);
        tunicColorLabel.Name = "tunicColorLabel";
        tunicColorLabel.Size = new Size(114, 15);
        tunicColorLabel.TabIndex = 26;
        tunicColorLabel.Text = "Normal Tunic Color:";
        // 
        // characterSpriteList
        // 
        characterSpriteList.DropDownStyle = ComboBoxStyle.DropDownList;
        characterSpriteList.FormattingEnabled = true;
        characterSpriteList.Items.AddRange(new object[] { "Link", "Zelda", "Iron Knuckle", "Error", "Samus", "Simon", "Stalfos", "Vase Lady", "Ruto", "Yoshi", "Dragonlord", "Miria", "Crystalis", "Taco", "Pyramid", "Lady Link", "Hoodie Link", "GliitchWiitch", "Random" });
        characterSpriteList.Location = new Point(298, 24);
        characterSpriteList.Margin = new Padding(2);
        characterSpriteList.Name = "characterSpriteList";
        characterSpriteList.Size = new Size(142, 23);
        characterSpriteList.TabIndex = 25;
        toolTip1.SetToolTip(characterSpriteList, "Changes the playable character sprite");
        characterSpriteList.SelectedIndexChanged += CharacterSpriteList_SelectedIndexChanged;
        // 
        // characterSpriteLabel
        // 
        characterSpriteLabel.AutoSize = true;
        characterSpriteLabel.Location = new Point(294, 3);
        characterSpriteLabel.Margin = new Padding(2, 0, 2, 0);
        characterSpriteLabel.Name = "characterSpriteLabel";
        characterSpriteLabel.Size = new Size(94, 15);
        characterSpriteLabel.TabIndex = 24;
        characterSpriteLabel.Text = "Character Sprite:";
        // 
        // disableMusicCheckbox
        // 
        disableMusicCheckbox.AutoSize = true;
        disableMusicCheckbox.Location = new Point(4, 3);
        disableMusicCheckbox.Margin = new Padding(2);
        disableMusicCheckbox.Name = "disableMusicCheckbox";
        disableMusicCheckbox.Size = new Size(99, 19);
        disableMusicCheckbox.TabIndex = 23;
        disableMusicCheckbox.Text = "Disable Music";
        toolTip1.SetToolTip(disableMusicCheckbox, "Disables most in game music");
        disableMusicCheckbox.UseVisualStyleBackColor = true;
        // 
        // fastSpellCheckbox
        // 
        fastSpellCheckbox.AutoSize = true;
        fastSpellCheckbox.Location = new Point(4, 27);
        fastSpellCheckbox.Margin = new Padding(4, 3, 4, 3);
        fastSpellCheckbox.Name = "fastSpellCheckbox";
        fastSpellCheckbox.Size = new Size(118, 19);
        fastSpellCheckbox.TabIndex = 4;
        fastSpellCheckbox.Text = "Fast Spell Casting";
        toolTip1.SetToolTip(fastSpellCheckbox, "When checked, you do not have to open the pause menu before casting the selected spell");
        fastSpellCheckbox.UseVisualStyleBackColor = true;
        // 
        // romFileTextBox
        // 
        romFileTextBox.Location = new Point(14, 30);
        romFileTextBox.Margin = new Padding(4, 3, 4, 3);
        romFileTextBox.Name = "romFileTextBox";
        romFileTextBox.Size = new Size(156, 23);
        romFileTextBox.TabIndex = 1;
        toolTip1.SetToolTip(romFileTextBox, "Select a USA version of the Zelda 2 ROM");
        // 
        // seedTextBox
        // 
        seedTextBox.Location = new Point(13, 79);
        seedTextBox.Margin = new Padding(4, 3, 4, 3);
        seedTextBox.Name = "seedTextBox";
        seedTextBox.Size = new Size(156, 23);
        seedTextBox.TabIndex = 2;
        toolTip1.SetToolTip(seedTextBox, "This represents the random values that will be used. A different seed results in a different shuffled ROM.");
        // 
        // romFileLabel
        // 
        romFileLabel.AutoSize = true;
        romFileLabel.Location = new Point(10, 10);
        romFileLabel.Margin = new Padding(4, 0, 4, 0);
        romFileLabel.Name = "romFileLabel";
        romFileLabel.Size = new Size(55, 15);
        romFileLabel.TabIndex = 4;
        romFileLabel.Text = "ROM File";
        // 
        // seedLabel
        // 
        seedLabel.AutoSize = true;
        seedLabel.Location = new Point(10, 59);
        seedLabel.Margin = new Padding(4, 0, 4, 0);
        seedLabel.Name = "seedLabel";
        seedLabel.Size = new Size(32, 15);
        seedLabel.TabIndex = 5;
        seedLabel.Text = "Seed";
        // 
        // createSeedButton
        // 
        createSeedButton.Location = new Point(177, 76);
        createSeedButton.Margin = new Padding(4, 3, 4, 3);
        createSeedButton.Name = "createSeedButton";
        createSeedButton.Size = new Size(88, 27);
        createSeedButton.TabIndex = 6;
        createSeedButton.Text = "Create Seed";
        createSeedButton.UseVisualStyleBackColor = true;
        createSeedButton.Click += createSeedButton_Click;
        // 
        // romFileBrowseButton
        // 
        romFileBrowseButton.Location = new Point(177, 29);
        romFileBrowseButton.Margin = new Padding(4, 3, 4, 3);
        romFileBrowseButton.Name = "romFileBrowseButton";
        romFileBrowseButton.Size = new Size(88, 27);
        romFileBrowseButton.TabIndex = 7;
        romFileBrowseButton.Text = "Browse...";
        romFileBrowseButton.UseVisualStyleBackColor = true;
        romFileBrowseButton.Click += fileBtn_Click;
        // 
        // generateRomButton
        // 
        generateRomButton.Location = new Point(491, 30);
        generateRomButton.Margin = new Padding(4, 3, 4, 3);
        generateRomButton.Name = "generateRomButton";
        generateRomButton.Size = new Size(126, 24);
        generateRomButton.TabIndex = 8;
        generateRomButton.Text = "Generate ROM";
        toolTip1.SetToolTip(generateRomButton, "Create the ROM");
        generateRomButton.UseVisualStyleBackColor = true;
        generateRomButton.Click += generateBtn_Click;
        // 
        // flagsTextBox
        // 
        flagsTextBox.Location = new Point(272, 30);
        flagsTextBox.Margin = new Padding(4, 3, 4, 3);
        flagsTextBox.Name = "flagsTextBox";
        flagsTextBox.Size = new Size(212, 23);
        flagsTextBox.TabIndex = 9;
        toolTip1.SetToolTip(flagsTextBox, "These flags represent the selected options. They can be copy/pasted.");
        // 
        // updateButton
        // 
        updateButton.Location = new Point(492, 105);
        updateButton.Margin = new Padding(4, 3, 4, 3);
        updateButton.Name = "updateButton";
        updateButton.Size = new Size(126, 27);
        updateButton.TabIndex = 10;
        updateButton.Text = "Check for Updates";
        toolTip1.SetToolTip(updateButton, "Check for updates");
        updateButton.UseVisualStyleBackColor = true;
        updateButton.Click += UpdateBtn_Click;
        // 
        // flagsLabel
        // 
        flagsLabel.AutoSize = true;
        flagsLabel.Location = new Point(273, 12);
        flagsLabel.Margin = new Padding(4, 0, 4, 0);
        flagsLabel.Name = "flagsLabel";
        flagsLabel.Size = new Size(34, 15);
        flagsLabel.TabIndex = 11;
        flagsLabel.Text = "Flags";
        // 
        // wikiButton
        // 
        wikiButton.Location = new Point(386, 105);
        wikiButton.Margin = new Padding(4, 3, 4, 3);
        wikiButton.Name = "wikiButton";
        wikiButton.Size = new Size(99, 27);
        wikiButton.TabIndex = 12;
        wikiButton.Text = "Wiki";
        toolTip1.SetToolTip(wikiButton, "Visit the website");
        wikiButton.UseVisualStyleBackColor = true;
        wikiButton.Click += WikiBtn_Click;
        // 
        // customizableButton1
        // 
        customizableButton1.AutoEllipsis = true;
        customizableButton1.Location = new Point(16, 504);
        customizableButton1.Margin = new Padding(4, 3, 4, 3);
        customizableButton1.Name = "customizableButton1";
        customizableButton1.Size = new Size(96, 27);
        customizableButton1.TabIndex = 18;
        customizableButton1.Text = "Beginner";
        toolTip1.SetToolTip(customizableButton1, "This preset is great for people who are looking for a casual experience.");
        customizableButton1.UseVisualStyleBackColor = true;
        // 
        // customizableButton2
        // 
        customizableButton2.AutoEllipsis = true;
        customizableButton2.Location = new Point(116, 504);
        customizableButton2.Margin = new Padding(4, 3, 4, 3);
        customizableButton2.Name = "customizableButton2";
        customizableButton2.Size = new Size(96, 27);
        customizableButton2.TabIndex = 19;
        customizableButton2.Text = "Standard";
        toolTip1.SetToolTip(customizableButton2, "Flags for the 2023 Standard Tournament");
        customizableButton2.UseVisualStyleBackColor = true;
        // 
        // customizableButton3
        // 
        customizableButton3.AutoEllipsis = true;
        customizableButton3.Location = new Point(216, 504);
        customizableButton3.Margin = new Padding(4, 3, 4, 3);
        customizableButton3.Name = "customizableButton3";
        customizableButton3.Size = new Size(96, 27);
        customizableButton3.TabIndex = 20;
        customizableButton3.Text = "Max Rando";
        toolTip1.SetToolTip(customizableButton3, "Flags for the 2023 Max Rando Tournament");
        customizableButton3.UseVisualStyleBackColor = true;
        // 
        // customizableButton4
        // 
        customizableButton4.AutoEllipsis = true;
        customizableButton4.Location = new Point(316, 504);
        customizableButton4.Margin = new Padding(4, 3, 4, 3);
        customizableButton4.Name = "customizableButton4";
        customizableButton4.Size = new Size(96, 27);
        customizableButton4.TabIndex = 21;
        customizableButton4.Text = "Random%";
        toolTip1.SetToolTip(customizableButton4, "Is it randomized? Who knows?");
        customizableButton4.UseVisualStyleBackColor = true;
        // 
        // customizableButton5
        // 
        customizableButton5.AutoEllipsis = true;
        customizableButton5.Location = new Point(416, 504);
        customizableButton5.Margin = new Padding(4, 3, 4, 3);
        customizableButton5.Name = "customizableButton5";
        customizableButton5.Size = new Size(96, 27);
        customizableButton5.TabIndex = 22;
        toolTip1.SetToolTip(customizableButton5, "Unused (for now)");
        customizableButton5.UseVisualStyleBackColor = true;
        // 
        // customizableButton6
        // 
        customizableButton6.AutoEllipsis = true;
        customizableButton6.Location = new Point(516, 504);
        customizableButton6.Margin = new Padding(4, 3, 4, 3);
        customizableButton6.Name = "customizableButton6";
        customizableButton6.Size = new Size(96, 27);
        customizableButton6.TabIndex = 23;
        toolTip1.SetToolTip(customizableButton6, "Unused (for now)");
        customizableButton6.UseVisualStyleBackColor = true;
        // 
        // discordButton
        // 
        discordButton.Location = new Point(273, 105);
        discordButton.Margin = new Padding(4, 3, 4, 3);
        discordButton.Name = "discordButton";
        discordButton.Size = new Size(106, 27);
        discordButton.TabIndex = 24;
        discordButton.Text = "Discord";
        toolTip1.SetToolTip(discordButton, "Join the Z2R Discord");
        discordButton.UseVisualStyleBackColor = true;
        discordButton.Click += DiscordButton_Click;
        // 
        // oldFlagsTextbox
        // 
        oldFlagsTextbox.Location = new Point(273, 76);
        oldFlagsTextbox.Margin = new Padding(4, 3, 4, 3);
        oldFlagsTextbox.Name = "oldFlagsTextbox";
        oldFlagsTextbox.Size = new Size(212, 23);
        oldFlagsTextbox.TabIndex = 37;
        toolTip1.SetToolTip(oldFlagsTextbox, "These flags represent the selected options. They can be copy/pasted.");
        // 
        // convertButton
        // 
        convertButton.Location = new Point(491, 76);
        convertButton.Margin = new Padding(4, 3, 4, 3);
        convertButton.Name = "convertButton";
        convertButton.Size = new Size(126, 24);
        convertButton.TabIndex = 39;
        convertButton.Text = "Convert";
        toolTip1.SetToolTip(convertButton, "Create the ROM");
        convertButton.UseVisualStyleBackColor = true;
        convertButton.Click += convertButton_Click;
        // 
        // backgroundWorker1
        // 
        backgroundWorker1.WorkerReportsProgress = true;
        backgroundWorker1.WorkerSupportsCancellation = true;
        backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
        // 
        // oldFlagsLabel
        // 
        oldFlagsLabel.AutoSize = true;
        oldFlagsLabel.Location = new Point(273, 58);
        oldFlagsLabel.Margin = new Padding(4, 0, 4, 0);
        oldFlagsLabel.Name = "oldFlagsLabel";
        oldFlagsLabel.Size = new Size(56, 15);
        oldFlagsLabel.TabIndex = 38;
        oldFlagsLabel.Text = "Old Flags";
        // 
        // batchButton
        // 
        batchButton.Location = new Point(177, 105);
        batchButton.Margin = new Padding(4, 3, 4, 3);
        batchButton.Name = "batchButton";
        batchButton.Size = new Size(88, 27);
        batchButton.TabIndex = 40;
        batchButton.Text = "Batch";
        batchButton.UseVisualStyleBackColor = true;
        batchButton.Click += Bulk_Generate_Click;
        // 
        // customisableButtonContextMenu
        // 
        customisableButtonContextMenu.Name = "contextMenuStrip1";
        customisableButtonContextMenu.Size = new Size(61, 4);
        // 
        // MainUI
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(626, 551);
        Controls.Add(batchButton);
        Controls.Add(convertButton);
        Controls.Add(oldFlagsLabel);
        Controls.Add(oldFlagsTextbox);
        Controls.Add(discordButton);
        Controls.Add(customizableButton6);
        Controls.Add(customizableButton5);
        Controls.Add(customizableButton4);
        Controls.Add(customizableButton3);
        Controls.Add(customizableButton2);
        Controls.Add(customizableButton1);
        Controls.Add(wikiButton);
        Controls.Add(flagsLabel);
        Controls.Add(updateButton);
        Controls.Add(flagsTextBox);
        Controls.Add(generateRomButton);
        Controls.Add(romFileBrowseButton);
        Controls.Add(createSeedButton);
        Controls.Add(seedLabel);
        Controls.Add(romFileLabel);
        Controls.Add(seedTextBox);
        Controls.Add(romFileTextBox);
        Controls.Add(mainTabControl);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Margin = new Padding(4, 3, 4, 3);
        Name = "MainUI";
        Text = "Zelda 2 Randomizer";
        mainTabControl.ResumeLayout(false);
        tabPage4.ResumeLayout(false);
        tabPage4.PerformLayout();
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        itemGrp.ResumeLayout(false);
        itemGrp.PerformLayout();
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        tabPage2.ResumeLayout(false);
        tabPage2.PerformLayout();
        tabPage5.ResumeLayout(false);
        tabPage5.PerformLayout();
        expBox.ResumeLayout(false);
        expBox.PerformLayout();
        tabPage9.ResumeLayout(false);
        tabPage9.PerformLayout();
        tabPage6.ResumeLayout(false);
        tabPage6.PerformLayout();
        tabPage7.ResumeLayout(false);
        tabPage7.PerformLayout();
        tabPage8.ResumeLayout(false);
        tabPage8.PerformLayout();
        miscTabPage.ResumeLayout(false);
        miscTabPage.PerformLayout();
        CustomTabPage.ResumeLayout(false);
        CustomTabPage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)spritePreviewBox).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TabControl mainTabControl;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TextBox romFileTextBox;
    private TextBox seedTextBox;
    private Label romFileLabel;
    private TabPage tabPage4;
    private GroupBox itemGrp;
    private CheckBox startWithMagicKeyCheckbox;
    private CheckBox startWithHammerCheckbox;
    private CheckBox startWithCrossCheckbox;
    private CheckBox startWithFluteCheckbox;
    private CheckBox startWithBootsCheckbox;
    private CheckBox startWithRaftCheckbox;
    private CheckBox startWithGloveCheckbox;
    private CheckBox startWithCandleCheckbox;
    private CheckBox shuffleStartingItemsCheckbox;
    private TabPage tabPage5;
    private Label seedLabel;
    private GroupBox groupBox1;
    private CheckBox startWIthThunderCheckbox;
    private CheckBox shuffleStartingSpellsCheckbox;
    private CheckBox startWithShieldCheckbox;
    private CheckBox startWithJumpCheckbox;
    private CheckBox startWithLifeCheckbox;
    private CheckBox startWithFairyCheckbox;
    private CheckBox startWithFireCheckbox;
    private CheckBox startWithReflectCheckbox;
    private CheckBox startWithSpellCheckbox;
    private Button createSeedButton;
    private Button romFileBrowseButton;
    private Button generateRomButton;
    private Label startingHeartContainersLabel;
    private ComboBox startHeartsMinList;
    private Label startingTechsLabel;
    private ComboBox startingTechsList;
    private TabPage tabPage6;
    private CheckBox shuffleEnemyHPBox;
    private GroupBox expBox;
    private CheckBox lifeExpNeededCheckbox;
    private CheckBox magicExpNeededCheckbox;
    private CheckBox shuffleAtkExpNeededCheckbox;
    private CheckBox shuffleAllExpCheckbox;
    private Label label6;
    private ComboBox startingGemsMinList;
    private CheckBox shuffleSwordImmunityBox;
    private CheckBox shuffleStealXPAmountCheckbox;
    private CheckBox shuffleXPStealersCheckbox;
    private TabPage CustomTabPage;
    private CheckBox tbirdRequiredCheckbox;
    private CheckBox shuffleEncountersCheckbox;
    private CheckBox shufflePalaceEnemiesCheckbox;
    private CheckBox shuffleOverworldEnemiesCheckbox;
    private Label label8;
    private TextBox flagsTextBox;
    private Button updateButton;
    private Label flagsLabel;
    private CheckBox mixLargeAndSmallCheckbox;
    private ToolTip toolTip1;
    private Button wikiButton;
    private TabPage tabPage7;
    private CheckBox mixOverworldPalaceItemsCheckbox;
    private CheckBox shuffleOverworldItemsCheckbox;
    private CheckBox shufflePalaceItemsCheckbox;
    private CheckBox shuffleSmallItemsCheckbox;
    private CheckBox palacesHaveExtraKeysCheckbox;
    private CheckBox palacePaletteCheckbox;
    private CheckBox allowPalaceContinentSwapCheckbox;
    private Label label4;
    private Label label12;
    private Label magicEffectivenessLabel;
    private Label attackEffectivenessLabel;
    private CheckBox restartAtPalacesCheckbox;
    private CheckBox fastSpellCheckbox;
    private CheckBox randomizeJarRequirementsCheckbox;
    private CheckBox removeTbirdCheckbox;
    private CheckBox includePbagCavesInShuffleCheckbox;
    private CheckBox includeGPinShuffleCheckbox;
    private Button discordButton;
    private CheckBox shuffleDripperEnemyCheckbox;
    private ComboBox hiddenPalaceList;
    private Label hiddenPalaceLabel;
    private CheckBox disableMusicCheckbox;
    private ComboBox hideKasutoList;
    private Label hiddenKasutoLabel;
    private ComboBox characterSpriteList;
    private Label characterSpriteLabel;
    private ComboBox tunicColorList;
    private Label tunicColorLabel;
    private ComboBox shieldColorList;
    private Label shieldColorLabel;
    private CheckBox removeSpellitemsCheckbox;
    private TabPage tabPage8;
    private CheckBox largeEnemiesKeyCheckbox;
    private CheckBox largeEnemies1UpCheckbox;
    private CheckBox largeEnemiesXLBagCheckbox;
    private CheckBox largeEnemiesLargeBagCheckbox;
    private CheckBox largeEnemiesMediumBagCheckbox;
    private CheckBox largeEnemiesSmallBagCheckbox;
    private CheckBox largeEnemiesRedJarCheckbox;
    private CheckBox largeEnemiesBlueJarCheckbox;
    private Label largeEnemyPoolLabel;
    private CheckBox smallEnemiesKeyCheckbox;
    private CheckBox smallEnemies1UpCheckbox;
    private CheckBox smallEnemiesXLBagCheckbox;
    private CheckBox smallEnemiesLargeBagCheckbox;
    private CheckBox smallEnemiesMediumBagCheckbox;
    private CheckBox smallEnemiesSmallBagCheckbox;
    private CheckBox smallEnemiesRedJarCheckbox;
    private CheckBox smallEnemiesBlueJarCheckbox;
    private Label smallEnemyPoolLabel;
    private Label label19;
    private CheckBox shuffleDropFrequencyCheckbox;
    private ComboBox beamSpriteList;
    private Label beamSpriteLabel;
    private CheckBox standardizeDropsCheckbox;
    private CheckBox randomizeDropsCheckbox;
    private CheckBox shufflePbagAmountsCheckbox;
    private TabPage tabPage9;
    private ComboBox lifeCapList;
    private ComboBox magCapList;
    private ComboBox atkCapList;
    private Label lifeCapLabel;
    private Label magCapLabel;
    private Label attackCapLabel;
    private CheckBox disableMagicContainerRequirementCheckbox;
    private CheckBox shuffleSpellLocationsCheckbox;
    private CheckBox shuffleLifeRefillCheckbox;
    private Label levelCapLabel;
    private CheckBox scaleLevelRequirementsToCapCheckbox;
    private TabPage miscTabPage;
    private CheckBox enableTownNameHintsCheckbox;
    private CheckBox enableSpellItemHintsCheckbox;
    private CheckBox enableHelpfulHintsCheckbox;
    private ComboBox encounterRateBox;
    private Label encounterRateLabel;
    private ComboBox attackEffectivenessList;
    private ComboBox magicEffectivenessList;
    private ComboBox lifeEffectivenessList;
    private Label enemyExperienceDropsLabel;
    private ComboBox experienceDropsList;
    private Label startingLevelsLabel;
    private ComboBox startingLifeLevelList;
    private ComboBox startingMagicLevelList;
    private ComboBox startingAttackLevelList;
    private Label startingLifeLabel;
    private Label startingMagicLabel;
    private Label startingAttackLabel;
    private Label label36;
    private Label ContinentConnectionLabel;
    private ComboBox continentConnectionBox;
    private CheckBox upAOnController1Checkbox;
    private CheckBox saneCaveShuffleBox;
    private CheckBox hideLessImportantLocationsCheckbox;
    private CheckBox allowBoulderBlockedConnectionsCheckbox;
    private Label label39;
    private Label climateLabel;
    private ComboBox eastBiome;
    private ComboBox dmBiome;
    private ComboBox climateSelector;
    private Label eastContinentBindingLabel;
    private Label deathMountainBiomeLabel;
    private CheckBox flashingOffCheckbox;
    private ComboBox mazeBiome;
    private Label mazeIslandBiomeLabel;
    private CheckBox shuffledVanillaShowsActualTerrain;
    private CheckBox shuffleWhichLocationsAreHiddenCheckbox;
    private CheckBox randomizeBossItemCheckbox;
    private CheckBox useGoodBootsCheckbox;
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private CheckBox randomizeSpellSpellEnemyCheckbox;
    private CheckBox generateBaguWoodsCheckbox;
    private CheckBox includeVanillaRoomsCheckbox;
    private CheckBox blockingRoomsInAnyPalaceCheckbox;
    private CheckBox bossRoomsExitToPalaceCheckbox;
    private Label palaceStyleLabel;
    private ComboBox palaceStyleList;
    private Label oldFlagsLabel;
    private TextBox oldFlagsTextbox;
    private Button convertButton;
    private Button batchButton;
    private ComboBox maxHeartsList;
    private ComboBox startHeartsMaxList;
    private Label label1;
    private Label label3;
    private Label label2;
    private Label label7;
    private Label label5;
    private ComboBox startingGemsMaxList;
    private CheckBox swapUpAndDownstabCheckbox;
    private CheckBox includeLavaInShuffleCheckBox;
    private Label FireSpellOptionLabel;
    private ComboBox FireSpellBox;
    private CheckBox noDuplicateRoomsByLayoutCheckbox;
    private CheckBox generatorsMatchCheckBox;
    private ContextMenuStrip customisableButtonContextMenu;
    private Label spritePreviewLabel;
    private PictureBox spritePreviewBox;
    private CheckBox dashAlwaysOnCheckbox;
    private CheckBox jumpAlwaysOnCheckbox;
    private ComboBox beepFrequencyDropdown;
    private Label label10;
    private ComboBox beepThresholdDropdown;
    private Label label9;
    private Label spriteCreditLabel;
    private ComboBox westBiomeSelector;
    private Label westBiomeLabel;
    private CheckBox allowPathEnemiesCheckbox;
    private CheckBox includev4_4RoomsCheckbox;
    private CheckBox includev4_0RoomsCheckbox;
    private Label gpStyleLabel;
    private ComboBox gpStyleList;
    private CheckBox shuffleEnemyPalettesCheckbox;
    private CheckBox alwaysBeamCheckbox;
    private CheckBox useCommunityTextCheckbox;
    private CheckBox noDuplicateRoomsByEnemiesCheckbox;
    private CheckBox HardBossesCheckbox;
    private Button customizableButton1;
    private Button customizableButton2;
    private Button customizableButton3;
    private Button customizableButton4;
    private Button customizableButton5;
    private Button customizableButton6;
    private Label StartingLivesLabel;
    private ComboBox startingLivesBox;
    private CheckBox randomizeKnockbackCheckbox;
    private CheckBox disableHUDLag;
    private CheckBox useCustomRoomsBox;
    private CheckBox includeQuestItemsInShuffleCheckbox;
    private CheckBox includeSwordTechsInShuffleCheckbox;
    private CheckBox includeSpellsInShuffleCheckbox;
    private Label ShortenPalaceLabel;
    private CheckBox shortenGPCheckbox;
    private CheckBox shortenNormalPalaceCheckbox;
}


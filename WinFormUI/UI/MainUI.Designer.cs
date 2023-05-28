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
        this.components = (new global::System.ComponentModel.Container());
        this.mainTabControl = (new global::System.Windows.Forms.TabControl());
        this.tabPage4 = (new global::System.Windows.Forms.TabPage());
        this.label3 = (new global::System.Windows.Forms.Label());
        this.label2 = (new global::System.Windows.Forms.Label());
        this.label1 = (new global::System.Windows.Forms.Label());
        this.maxHeartsList = (new global::System.Windows.Forms.ComboBox());
        this.startHeartsMaxList = (new global::System.Windows.Forms.ComboBox());
        this.startingLevelsLabel = (new global::System.Windows.Forms.Label());
        this.startingLifeLevelList = (new global::System.Windows.Forms.ComboBox());
        this.startingMagicLevelList = (new global::System.Windows.Forms.ComboBox());
        this.startingAttackLevelList = (new global::System.Windows.Forms.ComboBox());
        this.startingLifeLabel = (new global::System.Windows.Forms.Label());
        this.startingMagicLabel = (new global::System.Windows.Forms.Label());
        this.startingAttackLabel = (new global::System.Windows.Forms.Label());
        this.randomizeLivesBox = (new global::System.Windows.Forms.CheckBox());
        this.startingTechsLabel = (new global::System.Windows.Forms.Label());
        this.startingTechsList = (new global::System.Windows.Forms.ComboBox());
        this.startingHeartContainersLabel = (new global::System.Windows.Forms.Label());
        this.startHeartsMinList = (new global::System.Windows.Forms.ComboBox());
        this.groupBox1 = (new global::System.Windows.Forms.GroupBox());
        this.startWIthThunderCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithSpellCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithReflectCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithFireCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithFairyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithLifeCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithJumpCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithShieldCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleStartingSpellsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.itemGrp = (new global::System.Windows.Forms.GroupBox());
        this.startWithMagicKeyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithHammerCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithCrossCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithFluteCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithBootsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithRaftCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithGloveCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.startWithCandleCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleStartingItemsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage1 = (new global::System.Windows.Forms.TabPage());
        this.includeLavaInShuffle = (new global::System.Windows.Forms.CheckBox());
        this.generateBaguWoodsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.useGoodBootsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleWhichLocationsAreHiddenCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffledVanillaShowsActualTerrain = (new global::System.Windows.Forms.CheckBox());
        this.mazeBiome = (new global::System.Windows.Forms.ComboBox());
        this.mazeIslandBiomeLabel = (new global::System.Windows.Forms.Label());
        this.eastBiome = (new global::System.Windows.Forms.ComboBox());
        this.dmBiome = (new global::System.Windows.Forms.ComboBox());
        this.westBiome = (new global::System.Windows.Forms.ComboBox());
        this.eastContinentBindingLabel = (new global::System.Windows.Forms.Label());
        this.deathMountainBiomeLabel = (new global::System.Windows.Forms.Label());
        this.label39 = (new global::System.Windows.Forms.Label());
        this.westContinentLabel = (new global::System.Windows.Forms.Label());
        this.allowBoulderBlockedConnectionsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.saneCaveShuffleBox = (new global::System.Windows.Forms.CheckBox());
        this.hideLessImportantLocationsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.ContinentConnectionLabel = (new global::System.Windows.Forms.Label());
        this.continentConnectionBox = (new global::System.Windows.Forms.ComboBox());
        this.label36 = (new global::System.Windows.Forms.Label());
        this.encounterRateBox = (new global::System.Windows.Forms.ComboBox());
        this.encounterRateLabel = (new global::System.Windows.Forms.Label());
        this.hideKasutoList = (new global::System.Windows.Forms.ComboBox());
        this.hiddenKasutoLabel = (new global::System.Windows.Forms.Label());
        this.hiddenPalaceList = (new global::System.Windows.Forms.ComboBox());
        this.hiddenPalaceLabel = (new global::System.Windows.Forms.Label());
        this.includeGPinShuffleCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.allowPalaceContinentSwapCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.label4 = (new global::System.Windows.Forms.Label());
        this.allowPathEnemiesCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleEncountersCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage2 = (new global::System.Windows.Forms.TabPage());
        this.noDuplicateRoomsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.label7 = (new global::System.Windows.Forms.Label());
        this.label5 = (new global::System.Windows.Forms.Label());
        this.startingGemsMaxList = (new global::System.Windows.Forms.ComboBox());
        this.palaceStyleLabel = (new global::System.Windows.Forms.Label());
        this.palaceStyleList = (new global::System.Windows.Forms.ComboBox());
        this.bossRoomsExitToPalaceCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.blockingRoomsInAnyPalaceCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.includeCommunityRoomsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.randomizeBossItemCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.removeTbirdCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shortGPCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.restartAtPalacesCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.palacePaletteCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tbirdRequiredCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.label6 = (new global::System.Windows.Forms.Label());
        this.startingGemsMinList = (new global::System.Windows.Forms.ComboBox());
        this.tabPage5 = (new global::System.Windows.Forms.TabPage());
        this.lifeEffectivenessList = (new global::System.Windows.Forms.ComboBox());
        this.magicEffectivenessList = (new global::System.Windows.Forms.ComboBox());
        this.attackEffectivenessList = (new global::System.Windows.Forms.ComboBox());
        this.scaleLevelRequirementsToCapCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.levelCapLabel = (new global::System.Windows.Forms.Label());
        this.lifeCapList = (new global::System.Windows.Forms.ComboBox());
        this.magCapList = (new global::System.Windows.Forms.ComboBox());
        this.atkCapList = (new global::System.Windows.Forms.ComboBox());
        this.lifeCapLabel = (new global::System.Windows.Forms.Label());
        this.magCapLabel = (new global::System.Windows.Forms.Label());
        this.attackCapLabel = (new global::System.Windows.Forms.Label());
        this.label12 = (new global::System.Windows.Forms.Label());
        this.magicEffectivenessLabel = (new global::System.Windows.Forms.Label());
        this.attackEffectivenessLabel = (new global::System.Windows.Forms.Label());
        this.expBox = (new global::System.Windows.Forms.GroupBox());
        this.lifeExpNeededCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.magicExpNeededCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleAtkExpNeededCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleAllExpCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage9 = (new global::System.Windows.Forms.TabPage());
        this.FireSpellOptionLabel = (new global::System.Windows.Forms.Label());
        this.FireSpellBox = (new global::System.Windows.Forms.ComboBox());
        this.swapUpAndDownstabCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.randomizeSpellSpellEnemyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.disableMagicContainerRequirementCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleSpellLocationsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleLifeRefillCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage6 = (new global::System.Windows.Forms.TabPage());
        this.generatorsMatchCheckBox = (new global::System.Windows.Forms.CheckBox());
        this.enemyExperienceDropsLabel = (new global::System.Windows.Forms.Label());
        this.experienceDropsList = (new global::System.Windows.Forms.ComboBox());
        this.shuffleDripperEnemyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.mixLargeAndSmallCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.label8 = (new global::System.Windows.Forms.Label());
        this.shufflePalaceEnemiesCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleOverworldEnemiesCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleSwordImmunityBox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleStealXPAmountCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleXPStealersCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleEnemyHPBox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage7 = (new global::System.Windows.Forms.TabPage());
        this.shufflePbagAmountsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.removeSpellitemsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.includePbagCavesInShuffleCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.randomizeJarRequirementsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.palacesHaveExtraKeysCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleSmallItemsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.mixOverworldPalaceItemsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleOverworldItemsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shufflePalaceItemsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage8 = (new global::System.Windows.Forms.TabPage());
        this.randomizeDropsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.standardizeDropsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesKeyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemies1UpCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesXLBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesLargeBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesMediumBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesSmallBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesRedJarCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemiesBlueJarCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.largeEnemyPoolLabel = (new global::System.Windows.Forms.Label());
        this.smallEnemiesKeyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemies1UpCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemiesXLBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemiesLargeBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemiesMediumBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemiesSmallBagCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemiesRedJarCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemiesBlueJarCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.smallEnemyPoolLabel = (new global::System.Windows.Forms.Label());
        this.label19 = (new global::System.Windows.Forms.Label());
        this.shuffleDropFrequencyCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage10 = (new global::System.Windows.Forms.TabPage());
        this.enableTownNameHintsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.enableSpellItemHintsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.useCommunityHintsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.enableHelpfulHintsCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.tabPage3 = (new global::System.Windows.Forms.TabPage());
        this.useCustomRoomsBox = (new global::System.Windows.Forms.CheckBox());
        this.dashAlwaysOnCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.flashingOffCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.upAOnController1Checkbox = (new global::System.Windows.Forms.CheckBox());
        this.beamSpriteList = (new global::System.Windows.Forms.ComboBox());
        this.beamSpriteLabel = (new global::System.Windows.Forms.Label());
        this.shieldColorList = (new global::System.Windows.Forms.ComboBox());
        this.shieldColorLabel = (new global::System.Windows.Forms.Label());
        this.tunicColorList = (new global::System.Windows.Forms.ComboBox());
        this.tunicColorLabel = (new global::System.Windows.Forms.Label());
        this.characterSpriteList = (new global::System.Windows.Forms.ComboBox());
        this.characterSpriteLabel = (new global::System.Windows.Forms.Label());
        this.disableMusicCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.shuffleEnemyPalettesCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.alwaysBeamCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.fastSpellCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.jumpAlwaysOnCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.disableLowHealthBeepCheckbox = (new global::System.Windows.Forms.CheckBox());
        this.romFileTextBox = (new global::System.Windows.Forms.TextBox());
        this.seedTextBox = (new global::System.Windows.Forms.TextBox());
        this.romFileLabel = (new global::System.Windows.Forms.Label());
        this.seedLabel = (new global::System.Windows.Forms.Label());
        this.createSeedButton = (new global::System.Windows.Forms.Button());
        this.romFileBrowseButton = (new global::System.Windows.Forms.Button());
        this.generateRomButton = (new global::System.Windows.Forms.Button());
        this.flagsTextBox = (new global::System.Windows.Forms.TextBox());
        this.updateButton = (new global::System.Windows.Forms.Button());
        this.flagsLabel = (new global::System.Windows.Forms.Label());
        this.toolTip1 = (new global::System.Windows.Forms.ToolTip(this.components));
        this.wikiButton = (new global::System.Windows.Forms.Button());
        this.customFlagsButton1 = (new global::System.Windows.Forms.Button());
        this.customFlagsButton2 = (new global::System.Windows.Forms.Button());
        this.customFlagsButton3 = (new global::System.Windows.Forms.Button());
        this.customFlagsButton4 = (new global::System.Windows.Forms.Button());
        this.customFlagsButton5 = (new global::System.Windows.Forms.Button());
        this.customFlagsButton6 = (new global::System.Windows.Forms.Button());
        this.discordButton = (new global::System.Windows.Forms.Button());
        this.oldFlagsTextbox = (new global::System.Windows.Forms.TextBox());
        this.convertButton = (new global::System.Windows.Forms.Button());
        this.backgroundWorker1 = (new global::System.ComponentModel.BackgroundWorker());
        this.oldFlagsLabel = (new global::System.Windows.Forms.Label());
        this.batchButton = (new global::System.Windows.Forms.Button());
        this.customisableButtonContextMenu = (new global::System.Windows.Forms.ContextMenuStrip(this.components));
        this.mainTabControl.SuspendLayout();
        this.tabPage4.SuspendLayout();
        this.groupBox1.SuspendLayout();
        this.itemGrp.SuspendLayout();
        this.tabPage1.SuspendLayout();
        this.tabPage2.SuspendLayout();
        this.tabPage5.SuspendLayout();
        this.expBox.SuspendLayout();
        this.tabPage9.SuspendLayout();
        this.tabPage6.SuspendLayout();
        this.tabPage7.SuspendLayout();
        this.tabPage8.SuspendLayout();
        this.tabPage10.SuspendLayout();
        this.tabPage3.SuspendLayout();
        this.SuspendLayout();
        // 
        // mainTabControl
        // 
        this.mainTabControl.Controls.Add(this.tabPage4);
        this.mainTabControl.Controls.Add(this.tabPage1);
        this.mainTabControl.Controls.Add(this.tabPage2);
        this.mainTabControl.Controls.Add(this.tabPage5);
        this.mainTabControl.Controls.Add(this.tabPage9);
        this.mainTabControl.Controls.Add(this.tabPage6);
        this.mainTabControl.Controls.Add(this.tabPage7);
        this.mainTabControl.Controls.Add(this.tabPage8);
        this.mainTabControl.Controls.Add(this.tabPage10);
        this.mainTabControl.Controls.Add(this.tabPage3);
        this.mainTabControl.Location = (new global::System.Drawing.Point(15, 139));
        this.mainTabControl.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.mainTabControl.Name = ("mainTabControl");
        this.mainTabControl.SelectedIndex = (0);
        this.mainTabControl.Size = (new global::System.Drawing.Size(603, 359));
        this.mainTabControl.TabIndex = (0);
        // 
        // tabPage4
        // 
        this.tabPage4.Controls.Add(this.label3);
        this.tabPage4.Controls.Add(this.label2);
        this.tabPage4.Controls.Add(this.label1);
        this.tabPage4.Controls.Add(this.maxHeartsList);
        this.tabPage4.Controls.Add(this.startHeartsMaxList);
        this.tabPage4.Controls.Add(this.startingLevelsLabel);
        this.tabPage4.Controls.Add(this.startingLifeLevelList);
        this.tabPage4.Controls.Add(this.startingMagicLevelList);
        this.tabPage4.Controls.Add(this.startingAttackLevelList);
        this.tabPage4.Controls.Add(this.startingLifeLabel);
        this.tabPage4.Controls.Add(this.startingMagicLabel);
        this.tabPage4.Controls.Add(this.startingAttackLabel);
        this.tabPage4.Controls.Add(this.randomizeLivesBox);
        this.tabPage4.Controls.Add(this.startingTechsLabel);
        this.tabPage4.Controls.Add(this.startingTechsList);
        this.tabPage4.Controls.Add(this.startingHeartContainersLabel);
        this.tabPage4.Controls.Add(this.startHeartsMinList);
        this.tabPage4.Controls.Add(this.groupBox1);
        this.tabPage4.Controls.Add(this.itemGrp);
        this.tabPage4.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage4.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage4.Name = ("tabPage4");
        this.tabPage4.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage4.TabIndex = (3);
        this.tabPage4.Text = ("Start Configuration");
        this.tabPage4.UseVisualStyleBackColor = (true);
        // 
        // label3
        // 
        this.label3.AutoSize = (true);
        this.label3.Location = (new global::System.Drawing.Point(505, 82));
        this.label3.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label3.Name = ("label3");
        this.label3.Size = (new global::System.Drawing.Size(32, 30));
        this.label3.TabIndex = (33);
        this.label3.Text = ("Seed\r\nMax");
        this.label3.TextAlign = (global::System.Drawing.ContentAlignment.MiddleCenter);
        this.toolTip1.SetToolTip(this.label3, "Starting Attack Level");
        // 
        // label2
        // 
        this.label2.AutoSize = (true);
        this.label2.Location = (new global::System.Drawing.Point(454, 82));
        this.label2.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label2.Name = ("label2");
        this.label2.Size = (new global::System.Drawing.Size(31, 30));
        this.label2.TabIndex = (32);
        this.label2.Text = ("Start\r\nMax");
        this.label2.TextAlign = (global::System.Drawing.ContentAlignment.MiddleCenter);
        this.toolTip1.SetToolTip(this.label2, "Starting Attack Level");
        // 
        // label1
        // 
        this.label1.AutoSize = (true);
        this.label1.Location = (new global::System.Drawing.Point(406, 82));
        this.label1.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label1.Name = ("label1");
        this.label1.Size = (new global::System.Drawing.Size(31, 30));
        this.label1.TabIndex = (31);
        this.label1.Text = ("Start\r\nMin");
        this.label1.TextAlign = (global::System.Drawing.ContentAlignment.MiddleCenter);
        this.toolTip1.SetToolTip(this.label1, "Starting Attack Level");
        // 
        // maxHeartsList
        // 
        this.maxHeartsList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.maxHeartsList.DropDownWidth = (40);
        this.maxHeartsList.FormattingEnabled = (true);
        this.maxHeartsList.Items.AddRange(new global::System.Object[] { "1", "2", "3", "4", "5", "6", "7", "8", "?", "+1", "+2", "+3" });
        this.maxHeartsList.Location = (new global::System.Drawing.Point(500, 56));
        this.maxHeartsList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.maxHeartsList.MaxDropDownItems = (12);
        this.maxHeartsList.Name = ("maxHeartsList");
        this.maxHeartsList.Size = (new global::System.Drawing.Size(40, 23));
        this.maxHeartsList.TabIndex = (30);
        this.toolTip1.SetToolTip(this.maxHeartsList, "The number of heart containers you start with");
        // 
        // startHeartsMaxList
        // 
        this.startHeartsMaxList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startHeartsMaxList.DropDownWidth = (40);
        this.startHeartsMaxList.FormattingEnabled = (true);
        this.startHeartsMaxList.Items.AddRange(new global::System.Object[] { "1", "2", "3", "4", "5", "6", "7", "8", "?" });
        this.startHeartsMaxList.Location = (new global::System.Drawing.Point(450, 56));
        this.startHeartsMaxList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startHeartsMaxList.Name = ("startHeartsMaxList");
        this.startHeartsMaxList.Size = (new global::System.Drawing.Size(40, 23));
        this.startHeartsMaxList.TabIndex = (29);
        this.toolTip1.SetToolTip(this.startHeartsMaxList, "The number of heart containers you start with");
        // 
        // startingLevelsLabel
        // 
        this.startingLevelsLabel.AutoSize = (true);
        this.startingLevelsLabel.Location = (new global::System.Drawing.Point(427, 129));
        this.startingLevelsLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.startingLevelsLabel.Name = ("startingLevelsLabel");
        this.startingLevelsLabel.Size = (new global::System.Drawing.Size(78, 15));
        this.startingLevelsLabel.TabIndex = (28);
        this.startingLevelsLabel.Text = ("Starting Level");
        // 
        // startingLifeLevelList
        // 
        this.startingLifeLevelList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startingLifeLevelList.FormattingEnabled = (true);
        this.startingLifeLevelList.Items.AddRange(new global::System.Object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        this.startingLifeLevelList.Location = (new global::System.Drawing.Point(500, 151));
        this.startingLifeLevelList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startingLifeLevelList.Name = ("startingLifeLevelList");
        this.startingLifeLevelList.Size = (new global::System.Drawing.Size(40, 23));
        this.startingLifeLevelList.TabIndex = (27);
        this.toolTip1.SetToolTip(this.startingLifeLevelList, "Starting Life Level");
        // 
        // startingMagicLevelList
        // 
        this.startingMagicLevelList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startingMagicLevelList.DropDownWidth = (40);
        this.startingMagicLevelList.FormattingEnabled = (true);
        this.startingMagicLevelList.Items.AddRange(new global::System.Object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        this.startingMagicLevelList.Location = (new global::System.Drawing.Point(450, 151));
        this.startingMagicLevelList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startingMagicLevelList.Name = ("startingMagicLevelList");
        this.startingMagicLevelList.Size = (new global::System.Drawing.Size(40, 23));
        this.startingMagicLevelList.TabIndex = (26);
        this.toolTip1.SetToolTip(this.startingMagicLevelList, "Starting Magic Level");
        // 
        // startingAttackLevelList
        // 
        this.startingAttackLevelList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startingAttackLevelList.DropDownWidth = (40);
        this.startingAttackLevelList.FormattingEnabled = (true);
        this.startingAttackLevelList.Items.AddRange(new global::System.Object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
        this.startingAttackLevelList.Location = (new global::System.Drawing.Point(400, 151));
        this.startingAttackLevelList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startingAttackLevelList.Name = ("startingAttackLevelList");
        this.startingAttackLevelList.Size = (new global::System.Drawing.Size(40, 23));
        this.startingAttackLevelList.TabIndex = (25);
        this.toolTip1.SetToolTip(this.startingAttackLevelList, "Starting Attack Level");
        // 
        // startingLifeLabel
        // 
        this.startingLifeLabel.AutoSize = (true);
        this.startingLifeLabel.Location = (new global::System.Drawing.Point(505, 177));
        this.startingLifeLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.startingLifeLabel.Name = ("startingLifeLabel");
        this.startingLifeLabel.Size = (new global::System.Drawing.Size(26, 15));
        this.startingLifeLabel.TabIndex = (24);
        this.startingLifeLabel.Text = ("Life");
        this.toolTip1.SetToolTip(this.startingLifeLabel, "Starting Life Level");
        // 
        // startingMagicLabel
        // 
        this.startingMagicLabel.AutoSize = (true);
        this.startingMagicLabel.Location = (new global::System.Drawing.Point(454, 177));
        this.startingMagicLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.startingMagicLabel.Name = ("startingMagicLabel");
        this.startingMagicLabel.Size = (new global::System.Drawing.Size(31, 15));
        this.startingMagicLabel.TabIndex = (23);
        this.startingMagicLabel.Text = ("Mag");
        this.toolTip1.SetToolTip(this.startingMagicLabel, "Starting Magic Level");
        // 
        // startingAttackLabel
        // 
        this.startingAttackLabel.AutoSize = (true);
        this.startingAttackLabel.Location = (new global::System.Drawing.Point(406, 177));
        this.startingAttackLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.startingAttackLabel.Name = ("startingAttackLabel");
        this.startingAttackLabel.Size = (new global::System.Drawing.Size(25, 15));
        this.startingAttackLabel.TabIndex = (22);
        this.startingAttackLabel.Text = ("Atk");
        this.toolTip1.SetToolTip(this.startingAttackLabel, "Starting Attack Level");
        // 
        // randomizeLivesBox
        // 
        this.randomizeLivesBox.AutoSize = (true);
        this.randomizeLivesBox.Location = (new global::System.Drawing.Point(400, 257));
        this.randomizeLivesBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.randomizeLivesBox.Name = ("randomizeLivesBox");
        this.randomizeLivesBox.Size = (new global::System.Drawing.Size(175, 19));
        this.randomizeLivesBox.TabIndex = (15);
        this.randomizeLivesBox.Text = ("Randomize Number of Lives");
        this.toolTip1.SetToolTip(this.randomizeLivesBox, "Start with anywhere from 2-5 lives");
        this.randomizeLivesBox.UseVisualStyleBackColor = (true);
        // 
        // startingTechsLabel
        // 
        this.startingTechsLabel.AutoSize = (true);
        this.startingTechsLabel.Location = (new global::System.Drawing.Point(398, 211));
        this.startingTechsLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.startingTechsLabel.Name = ("startingTechsLabel");
        this.startingTechsLabel.Size = (new global::System.Drawing.Size(80, 15));
        this.startingTechsLabel.TabIndex = (14);
        this.startingTechsLabel.Text = ("Starting Techs");
        // 
        // startingTechsList
        // 
        this.startingTechsList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startingTechsList.FormattingEnabled = (true);
        this.startingTechsList.Items.AddRange(new global::System.Object[] { "None", "Downstab", "Upstab", "Both", "Random" });
        this.startingTechsList.Location = (new global::System.Drawing.Point(402, 228));
        this.startingTechsList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startingTechsList.Name = ("startingTechsList");
        this.startingTechsList.Size = (new global::System.Drawing.Size(140, 23));
        this.startingTechsList.TabIndex = (13);
        this.toolTip1.SetToolTip(this.startingTechsList, "The sword techniques you start with");
        // 
        // startingHeartContainersLabel
        // 
        this.startingHeartContainersLabel.AutoSize = (true);
        this.startingHeartContainersLabel.Location = (new global::System.Drawing.Point(412, 38));
        this.startingHeartContainersLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.startingHeartContainersLabel.Name = ("startingHeartContainersLabel");
        this.startingHeartContainersLabel.Size = (new global::System.Drawing.Size(96, 15));
        this.startingHeartContainersLabel.TabIndex = (10);
        this.startingHeartContainersLabel.Text = ("Heart Containers");
        // 
        // startHeartsMinList
        // 
        this.startHeartsMinList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startHeartsMinList.DropDownWidth = (40);
        this.startHeartsMinList.FormattingEnabled = (true);
        this.startHeartsMinList.Items.AddRange(new global::System.Object[] { "1", "2", "3", "4", "5", "6", "7", "8", "?" });
        this.startHeartsMinList.Location = (new global::System.Drawing.Point(400, 56));
        this.startHeartsMinList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startHeartsMinList.Name = ("startHeartsMinList");
        this.startHeartsMinList.Size = (new global::System.Drawing.Size(40, 23));
        this.startHeartsMinList.TabIndex = (9);
        this.toolTip1.SetToolTip(this.startHeartsMinList, "The number of heart containers you start with");
        // 
        // groupBox1
        // 
        this.groupBox1.Controls.Add(this.startWIthThunderCheckbox);
        this.groupBox1.Controls.Add(this.startWithSpellCheckbox);
        this.groupBox1.Controls.Add(this.startWithReflectCheckbox);
        this.groupBox1.Controls.Add(this.startWithFireCheckbox);
        this.groupBox1.Controls.Add(this.startWithFairyCheckbox);
        this.groupBox1.Controls.Add(this.startWithLifeCheckbox);
        this.groupBox1.Controls.Add(this.startWithJumpCheckbox);
        this.groupBox1.Controls.Add(this.startWithShieldCheckbox);
        this.groupBox1.Controls.Add(this.shuffleStartingSpellsCheckbox);
        this.groupBox1.Location = (new global::System.Drawing.Point(204, 38));
        this.groupBox1.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.groupBox1.Name = ("groupBox1");
        this.groupBox1.Padding = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.groupBox1.Size = (new global::System.Drawing.Size(187, 237));
        this.groupBox1.TabIndex = (8);
        this.groupBox1.TabStop = (false);
        this.groupBox1.Text = ("                                        ");
        // 
        // startWIthThunderCheckbox
        // 
        this.startWIthThunderCheckbox.AutoSize = (true);
        this.startWIthThunderCheckbox.Location = (new global::System.Drawing.Point(27, 208));
        this.startWIthThunderCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWIthThunderCheckbox.Name = ("startWIthThunderCheckbox");
        this.startWIthThunderCheckbox.Size = (new global::System.Drawing.Size(125, 19));
        this.startWIthThunderCheckbox.TabIndex = (7);
        this.startWIthThunderCheckbox.Text = ("Start With Thunder");
        this.toolTip1.SetToolTip(this.startWIthThunderCheckbox, "Start with thunder spell");
        this.startWIthThunderCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithSpellCheckbox
        // 
        this.startWithSpellCheckbox.AutoSize = (true);
        this.startWithSpellCheckbox.Location = (new global::System.Drawing.Point(27, 181));
        this.startWithSpellCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithSpellCheckbox.Name = ("startWithSpellCheckbox");
        this.startWithSpellCheckbox.Size = (new global::System.Drawing.Size(106, 19));
        this.startWithSpellCheckbox.TabIndex = (6);
        this.startWithSpellCheckbox.Text = ("Start With Spell");
        this.toolTip1.SetToolTip(this.startWithSpellCheckbox, "Start with spell spell");
        this.startWithSpellCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithReflectCheckbox
        // 
        this.startWithReflectCheckbox.AutoSize = (true);
        this.startWithReflectCheckbox.Location = (new global::System.Drawing.Point(27, 155));
        this.startWithReflectCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithReflectCheckbox.Name = ("startWithReflectCheckbox");
        this.startWithReflectCheckbox.Size = (new global::System.Drawing.Size(117, 19));
        this.startWithReflectCheckbox.TabIndex = (5);
        this.startWithReflectCheckbox.Text = ("Start With Reflect");
        this.toolTip1.SetToolTip(this.startWithReflectCheckbox, "Start with reflect spell");
        this.startWithReflectCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithFireCheckbox
        // 
        this.startWithFireCheckbox.AutoSize = (true);
        this.startWithFireCheckbox.Location = (new global::System.Drawing.Point(27, 128));
        this.startWithFireCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithFireCheckbox.Name = ("startWithFireCheckbox");
        this.startWithFireCheckbox.Size = (new global::System.Drawing.Size(100, 19));
        this.startWithFireCheckbox.TabIndex = (4);
        this.startWithFireCheckbox.Text = ("Start With Fire");
        this.toolTip1.SetToolTip(this.startWithFireCheckbox, "Start with fire spell");
        this.startWithFireCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithFairyCheckbox
        // 
        this.startWithFairyCheckbox.AutoSize = (true);
        this.startWithFairyCheckbox.Location = (new global::System.Drawing.Point(27, 102));
        this.startWithFairyCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithFairyCheckbox.Name = ("startWithFairyCheckbox");
        this.startWithFairyCheckbox.Size = (new global::System.Drawing.Size(106, 19));
        this.startWithFairyCheckbox.TabIndex = (3);
        this.startWithFairyCheckbox.Text = ("Start With Fairy");
        this.toolTip1.SetToolTip(this.startWithFairyCheckbox, "Start with fairy spell");
        this.startWithFairyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithLifeCheckbox
        // 
        this.startWithLifeCheckbox.AutoSize = (true);
        this.startWithLifeCheckbox.Location = (new global::System.Drawing.Point(27, 75));
        this.startWithLifeCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithLifeCheckbox.Name = ("startWithLifeCheckbox");
        this.startWithLifeCheckbox.Size = (new global::System.Drawing.Size(100, 19));
        this.startWithLifeCheckbox.TabIndex = (2);
        this.startWithLifeCheckbox.Text = ("Start With Life");
        this.toolTip1.SetToolTip(this.startWithLifeCheckbox, "Start with life spell");
        this.startWithLifeCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithJumpCheckbox
        // 
        this.startWithJumpCheckbox.AutoSize = (true);
        this.startWithJumpCheckbox.Location = (new global::System.Drawing.Point(27, 48));
        this.startWithJumpCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithJumpCheckbox.Name = ("startWithJumpCheckbox");
        this.startWithJumpCheckbox.Size = (new global::System.Drawing.Size(110, 19));
        this.startWithJumpCheckbox.TabIndex = (1);
        this.startWithJumpCheckbox.Text = ("Start With Jump");
        this.toolTip1.SetToolTip(this.startWithJumpCheckbox, "Start with jump spell");
        this.startWithJumpCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithShieldCheckbox
        // 
        this.startWithShieldCheckbox.AutoSize = (true);
        this.startWithShieldCheckbox.Location = (new global::System.Drawing.Point(27, 22));
        this.startWithShieldCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithShieldCheckbox.Name = ("startWithShieldCheckbox");
        this.startWithShieldCheckbox.Size = (new global::System.Drawing.Size(113, 19));
        this.startWithShieldCheckbox.TabIndex = (1);
        this.startWithShieldCheckbox.Text = ("Start With Shield");
        this.toolTip1.SetToolTip(this.startWithShieldCheckbox, "Start with shield spell");
        this.startWithShieldCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleStartingSpellsCheckbox
        // 
        this.shuffleStartingSpellsCheckbox.AutoSize = (true);
        this.shuffleStartingSpellsCheckbox.Location = (new global::System.Drawing.Point(7, 0));
        this.shuffleStartingSpellsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleStartingSpellsCheckbox.Name = ("shuffleStartingSpellsCheckbox");
        this.shuffleStartingSpellsCheckbox.Size = (new global::System.Drawing.Size(140, 19));
        this.shuffleStartingSpellsCheckbox.TabIndex = (1);
        this.shuffleStartingSpellsCheckbox.Text = ("Shuffle Starting Spells");
        this.toolTip1.SetToolTip(this.shuffleStartingSpellsCheckbox, "Each spell has a 25% chance of being known");
        this.shuffleStartingSpellsCheckbox.UseVisualStyleBackColor = (true);
        this.shuffleStartingSpellsCheckbox.CheckStateChanged += (this.spellShuffleBox_CheckStateChanged);
        // 
        // itemGrp
        // 
        this.itemGrp.Controls.Add(this.startWithMagicKeyCheckbox);
        this.itemGrp.Controls.Add(this.startWithHammerCheckbox);
        this.itemGrp.Controls.Add(this.startWithCrossCheckbox);
        this.itemGrp.Controls.Add(this.startWithFluteCheckbox);
        this.itemGrp.Controls.Add(this.startWithBootsCheckbox);
        this.itemGrp.Controls.Add(this.startWithRaftCheckbox);
        this.itemGrp.Controls.Add(this.startWithGloveCheckbox);
        this.itemGrp.Controls.Add(this.startWithCandleCheckbox);
        this.itemGrp.Controls.Add(this.shuffleStartingItemsCheckbox);
        this.itemGrp.Location = (new global::System.Drawing.Point(10, 38));
        this.itemGrp.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.itemGrp.Name = ("itemGrp");
        this.itemGrp.Padding = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.itemGrp.Size = (new global::System.Drawing.Size(187, 237));
        this.itemGrp.TabIndex = (0);
        this.itemGrp.TabStop = (false);
        this.itemGrp.Text = ("                                        ");
        // 
        // startWithMagicKeyCheckbox
        // 
        this.startWithMagicKeyCheckbox.AutoSize = (true);
        this.startWithMagicKeyCheckbox.Location = (new global::System.Drawing.Point(27, 208));
        this.startWithMagicKeyCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithMagicKeyCheckbox.Name = ("startWithMagicKeyCheckbox");
        this.startWithMagicKeyCheckbox.Size = (new global::System.Drawing.Size(136, 19));
        this.startWithMagicKeyCheckbox.TabIndex = (7);
        this.startWithMagicKeyCheckbox.Text = ("Start With Magic Key");
        this.toolTip1.SetToolTip(this.startWithMagicKeyCheckbox, "Start with the magic key");
        this.startWithMagicKeyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithHammerCheckbox
        // 
        this.startWithHammerCheckbox.AutoSize = (true);
        this.startWithHammerCheckbox.Location = (new global::System.Drawing.Point(27, 181));
        this.startWithHammerCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithHammerCheckbox.Name = ("startWithHammerCheckbox");
        this.startWithHammerCheckbox.Size = (new global::System.Drawing.Size(128, 19));
        this.startWithHammerCheckbox.TabIndex = (6);
        this.startWithHammerCheckbox.Text = ("Start With Hammer");
        this.toolTip1.SetToolTip(this.startWithHammerCheckbox, "Start with the hammer");
        this.startWithHammerCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithCrossCheckbox
        // 
        this.startWithCrossCheckbox.AutoSize = (true);
        this.startWithCrossCheckbox.Location = (new global::System.Drawing.Point(27, 155));
        this.startWithCrossCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithCrossCheckbox.Name = ("startWithCrossCheckbox");
        this.startWithCrossCheckbox.Size = (new global::System.Drawing.Size(110, 19));
        this.startWithCrossCheckbox.TabIndex = (5);
        this.startWithCrossCheckbox.Text = ("Start With Cross");
        this.toolTip1.SetToolTip(this.startWithCrossCheckbox, "Start with the cross");
        this.startWithCrossCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithFluteCheckbox
        // 
        this.startWithFluteCheckbox.AutoSize = (true);
        this.startWithFluteCheckbox.Location = (new global::System.Drawing.Point(27, 128));
        this.startWithFluteCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithFluteCheckbox.Name = ("startWithFluteCheckbox");
        this.startWithFluteCheckbox.Size = (new global::System.Drawing.Size(107, 19));
        this.startWithFluteCheckbox.TabIndex = (4);
        this.startWithFluteCheckbox.Text = ("Start With Flute");
        this.toolTip1.SetToolTip(this.startWithFluteCheckbox, "Start with the flute");
        this.startWithFluteCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithBootsCheckbox
        // 
        this.startWithBootsCheckbox.AutoSize = (true);
        this.startWithBootsCheckbox.Location = (new global::System.Drawing.Point(27, 102));
        this.startWithBootsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithBootsCheckbox.Name = ("startWithBootsCheckbox");
        this.startWithBootsCheckbox.Size = (new global::System.Drawing.Size(111, 19));
        this.startWithBootsCheckbox.TabIndex = (3);
        this.startWithBootsCheckbox.Text = ("Start With Boots");
        this.toolTip1.SetToolTip(this.startWithBootsCheckbox, "Start with the boots");
        this.startWithBootsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithRaftCheckbox
        // 
        this.startWithRaftCheckbox.AutoSize = (true);
        this.startWithRaftCheckbox.Location = (new global::System.Drawing.Point(27, 75));
        this.startWithRaftCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithRaftCheckbox.Name = ("startWithRaftCheckbox");
        this.startWithRaftCheckbox.Size = (new global::System.Drawing.Size(102, 19));
        this.startWithRaftCheckbox.TabIndex = (2);
        this.startWithRaftCheckbox.Text = ("Start With Raft");
        this.toolTip1.SetToolTip(this.startWithRaftCheckbox, "Start with the raft");
        this.startWithRaftCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithGloveCheckbox
        // 
        this.startWithGloveCheckbox.AutoSize = (true);
        this.startWithGloveCheckbox.Location = (new global::System.Drawing.Point(27, 48));
        this.startWithGloveCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithGloveCheckbox.Name = ("startWithGloveCheckbox");
        this.startWithGloveCheckbox.Size = (new global::System.Drawing.Size(111, 19));
        this.startWithGloveCheckbox.TabIndex = (1);
        this.startWithGloveCheckbox.Text = ("Start With Glove");
        this.toolTip1.SetToolTip(this.startWithGloveCheckbox, "Start with the glove");
        this.startWithGloveCheckbox.UseVisualStyleBackColor = (true);
        // 
        // startWithCandleCheckbox
        // 
        this.startWithCandleCheckbox.AutoSize = (true);
        this.startWithCandleCheckbox.Location = (new global::System.Drawing.Point(27, 22));
        this.startWithCandleCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startWithCandleCheckbox.Name = ("startWithCandleCheckbox");
        this.startWithCandleCheckbox.Size = (new global::System.Drawing.Size(118, 19));
        this.startWithCandleCheckbox.TabIndex = (1);
        this.startWithCandleCheckbox.Text = ("Start With Candle");
        this.toolTip1.SetToolTip(this.startWithCandleCheckbox, "Start with candle");
        this.startWithCandleCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleStartingItemsCheckbox
        // 
        this.shuffleStartingItemsCheckbox.AccessibleDescription = ("");
        this.shuffleStartingItemsCheckbox.AutoSize = (true);
        this.shuffleStartingItemsCheckbox.Location = (new global::System.Drawing.Point(7, 0));
        this.shuffleStartingItemsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleStartingItemsCheckbox.Name = ("shuffleStartingItemsCheckbox");
        this.shuffleStartingItemsCheckbox.Size = (new global::System.Drawing.Size(139, 19));
        this.shuffleStartingItemsCheckbox.TabIndex = (1);
        this.shuffleStartingItemsCheckbox.Text = ("Shuffle Starting Items");
        this.toolTip1.SetToolTip(this.shuffleStartingItemsCheckbox, "Each item has a 25% chance of being in your inventory");
        this.shuffleStartingItemsCheckbox.UseVisualStyleBackColor = (true);
        this.shuffleStartingItemsCheckbox.CheckStateChanged += (this.shuffleItemBox_CheckStateChanged);
        // 
        // tabPage1
        // 
        this.tabPage1.Controls.Add(this.includeLavaInShuffle);
        this.tabPage1.Controls.Add(this.generateBaguWoodsCheckbox);
        this.tabPage1.Controls.Add(this.useGoodBootsCheckbox);
        this.tabPage1.Controls.Add(this.shuffleWhichLocationsAreHiddenCheckbox);
        this.tabPage1.Controls.Add(this.shuffledVanillaShowsActualTerrain);
        this.tabPage1.Controls.Add(this.mazeBiome);
        this.tabPage1.Controls.Add(this.mazeIslandBiomeLabel);
        this.tabPage1.Controls.Add(this.eastBiome);
        this.tabPage1.Controls.Add(this.dmBiome);
        this.tabPage1.Controls.Add(this.westBiome);
        this.tabPage1.Controls.Add(this.eastContinentBindingLabel);
        this.tabPage1.Controls.Add(this.deathMountainBiomeLabel);
        this.tabPage1.Controls.Add(this.label39);
        this.tabPage1.Controls.Add(this.westContinentLabel);
        this.tabPage1.Controls.Add(this.allowBoulderBlockedConnectionsCheckbox);
        this.tabPage1.Controls.Add(this.saneCaveShuffleBox);
        this.tabPage1.Controls.Add(this.hideLessImportantLocationsCheckbox);
        this.tabPage1.Controls.Add(this.ContinentConnectionLabel);
        this.tabPage1.Controls.Add(this.continentConnectionBox);
        this.tabPage1.Controls.Add(this.label36);
        this.tabPage1.Controls.Add(this.encounterRateBox);
        this.tabPage1.Controls.Add(this.encounterRateLabel);
        this.tabPage1.Controls.Add(this.hideKasutoList);
        this.tabPage1.Controls.Add(this.hiddenKasutoLabel);
        this.tabPage1.Controls.Add(this.hiddenPalaceList);
        this.tabPage1.Controls.Add(this.hiddenPalaceLabel);
        this.tabPage1.Controls.Add(this.includeGPinShuffleCheckbox);
        this.tabPage1.Controls.Add(this.allowPalaceContinentSwapCheckbox);
        this.tabPage1.Controls.Add(this.label4);
        this.tabPage1.Controls.Add(this.allowPathEnemiesCheckbox);
        this.tabPage1.Controls.Add(this.shuffleEncountersCheckbox);
        this.tabPage1.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage1.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage1.Name = ("tabPage1");
        this.tabPage1.Padding = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage1.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage1.TabIndex = (0);
        this.tabPage1.Text = ("Overworld");
        this.tabPage1.ToolTipText = ("When selected, will hide Kasuto behind a forest tile");
        this.tabPage1.UseVisualStyleBackColor = (true);
        // 
        // includeLavaInShuffle
        // 
        this.includeLavaInShuffle.AutoSize = (true);
        this.includeLavaInShuffle.Location = (new global::System.Drawing.Point(7, 129));
        this.includeLavaInShuffle.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.includeLavaInShuffle.Name = ("includeLavaInShuffle");
        this.includeLavaInShuffle.Size = (new global::System.Drawing.Size(145, 19));
        this.includeLavaInShuffle.TabIndex = (48);
        this.includeLavaInShuffle.Text = ("Include Lava in Shuffle");
        this.toolTip1.SetToolTip(this.includeLavaInShuffle, "If checked, you may have enemies in path encounters");
        this.includeLavaInShuffle.UseVisualStyleBackColor = (true);
        // 
        // generateBaguWoodsCheckbox
        // 
        this.generateBaguWoodsCheckbox.AutoSize = (true);
        this.generateBaguWoodsCheckbox.Location = (new global::System.Drawing.Point(282, 114));
        this.generateBaguWoodsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.generateBaguWoodsCheckbox.Name = ("generateBaguWoodsCheckbox");
        this.generateBaguWoodsCheckbox.Size = (new global::System.Drawing.Size(151, 19));
        this.generateBaguWoodsCheckbox.TabIndex = (47);
        this.generateBaguWoodsCheckbox.Text = ("Generate Bagu's Woods");
        this.generateBaguWoodsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.generateBaguWoodsCheckbox, "When selected, bagu's house will be hidden in a forest surrounded by lost woods tiles.");
        this.generateBaguWoodsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // useGoodBootsCheckbox
        // 
        this.useGoodBootsCheckbox.AutoSize = (true);
        this.useGoodBootsCheckbox.Location = (new global::System.Drawing.Point(282, 88));
        this.useGoodBootsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.useGoodBootsCheckbox.Name = ("useGoodBootsCheckbox");
        this.useGoodBootsCheckbox.Size = (new global::System.Drawing.Size(195, 19));
        this.useGoodBootsCheckbox.TabIndex = (46);
        this.useGoodBootsCheckbox.Text = ("All Water is Walkable with Boots");
        this.useGoodBootsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.useGoodBootsCheckbox, "When selected, all water within the map boundaries can be traversed with the boots.");
        this.useGoodBootsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleWhichLocationsAreHiddenCheckbox
        // 
        this.shuffleWhichLocationsAreHiddenCheckbox.AutoSize = (true);
        this.shuffleWhichLocationsAreHiddenCheckbox.Location = (new global::System.Drawing.Point(7, 267));
        this.shuffleWhichLocationsAreHiddenCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleWhichLocationsAreHiddenCheckbox.Name = ("shuffleWhichLocationsAreHiddenCheckbox");
        this.shuffleWhichLocationsAreHiddenCheckbox.Size = (new global::System.Drawing.Size(221, 19));
        this.shuffleWhichLocationsAreHiddenCheckbox.TabIndex = (45);
        this.shuffleWhichLocationsAreHiddenCheckbox.Text = ("Shuffle which Location(s) are Hidden");
        this.shuffleWhichLocationsAreHiddenCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shuffleWhichLocationsAreHiddenCheckbox, "When selected, shuffles which location are in the hidden palace and hidden kasuto spots on the overworld.");
        this.shuffleWhichLocationsAreHiddenCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffledVanillaShowsActualTerrain
        // 
        this.shuffledVanillaShowsActualTerrain.AutoSize = (true);
        this.shuffledVanillaShowsActualTerrain.Location = (new global::System.Drawing.Point(281, 299));
        this.shuffledVanillaShowsActualTerrain.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffledVanillaShowsActualTerrain.Name = ("shuffledVanillaShowsActualTerrain");
        this.shuffledVanillaShowsActualTerrain.Size = (new global::System.Drawing.Size(268, 19));
        this.shuffledVanillaShowsActualTerrain.TabIndex = (44);
        this.shuffledVanillaShowsActualTerrain.Text = ("Shuffled Vanilla Locations Show Actual Terrain");
        this.toolTip1.SetToolTip(this.shuffledVanillaShowsActualTerrain, "When selected, if a shuffled vanilla map is in play, the map will show the correct terrain type of each location.");
        this.shuffledVanillaShowsActualTerrain.UseVisualStyleBackColor = (true);
        // 
        // mazeBiome
        // 
        this.mazeBiome.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.mazeBiome.FormattingEnabled = (true);
        this.mazeBiome.Items.AddRange(new global::System.Object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Random (with Vanilla)" });
        this.mazeBiome.Location = (new global::System.Drawing.Point(422, 268));
        this.mazeBiome.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.mazeBiome.Name = ("mazeBiome");
        this.mazeBiome.Size = (new global::System.Drawing.Size(140, 23));
        this.mazeBiome.TabIndex = (43);
        this.toolTip1.SetToolTip(this.mazeBiome, "Maze Island overworld map style.");
        this.mazeBiome.SelectedIndexChanged += (this.MazeBiome_SelectedIndexChanged);
        // 
        // mazeIslandBiomeLabel
        // 
        this.mazeIslandBiomeLabel.AutoSize = (true);
        this.mazeIslandBiomeLabel.Location = (new global::System.Drawing.Point(279, 271));
        this.mazeIslandBiomeLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.mazeIslandBiomeLabel.Name = ("mazeIslandBiomeLabel");
        this.mazeIslandBiomeLabel.Size = (new global::System.Drawing.Size(109, 15));
        this.mazeIslandBiomeLabel.TabIndex = (42);
        this.mazeIslandBiomeLabel.Text = ("Maze Island Biome:");
        this.toolTip1.SetToolTip(this.mazeIslandBiomeLabel, "Maze Island overworld map style.");
        // 
        // eastBiome
        // 
        this.eastBiome.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.eastBiome.FormattingEnabled = (true);
        this.eastBiome.Items.AddRange(new global::System.Object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Islands", "Canyon", "Volcano", "Mountainous", "Random (no Vanilla)", "Random (with Vanilla)" });
        this.eastBiome.Location = (new global::System.Drawing.Point(422, 237));
        this.eastBiome.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.eastBiome.Name = ("eastBiome");
        this.eastBiome.Size = (new global::System.Drawing.Size(140, 23));
        this.eastBiome.TabIndex = (41);
        this.toolTip1.SetToolTip(this.eastBiome, "East Hyrule overworld map style.");
        this.eastBiome.SelectedIndexChanged += (this.EastBiome_SelectedIndexChanged);
        // 
        // dmBiome
        // 
        this.dmBiome.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.dmBiome.FormattingEnabled = (true);
        this.dmBiome.Items.AddRange(new global::System.Object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Islands", "Canyon", "Caldera", "Mountainous", "Random (no Vanilla)", "Random (with Vanilla)" });
        this.dmBiome.Location = (new global::System.Drawing.Point(422, 205));
        this.dmBiome.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.dmBiome.Name = ("dmBiome");
        this.dmBiome.Size = (new global::System.Drawing.Size(140, 23));
        this.dmBiome.TabIndex = (40);
        this.toolTip1.SetToolTip(this.dmBiome, "Death Mountain overworld map style.");
        this.dmBiome.SelectedIndexChanged += (this.DmBiome_SelectedIndexChanged);
        // 
        // westBiome
        // 
        this.westBiome.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.westBiome.FormattingEnabled = (true);
        this.westBiome.Items.AddRange(new global::System.Object[] { "Vanilla", "Vanilla (shuffled)", "Vanilla-Like", "Islands", "Canyon", "Caldera", "Mountainous", "Random (no Vanilla)", "Random (with Vanilla)" });
        this.westBiome.Location = (new global::System.Drawing.Point(422, 175));
        this.westBiome.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.westBiome.Name = ("westBiome");
        this.westBiome.Size = (new global::System.Drawing.Size(140, 23));
        this.westBiome.TabIndex = (39);
        this.toolTip1.SetToolTip(this.westBiome, "West Hyrule overworld map style.");
        this.westBiome.SelectedIndexChanged += (this.WestBiome_SelectedIndexChanged);
        // 
        // eastContinentBindingLabel
        // 
        this.eastContinentBindingLabel.AutoSize = (true);
        this.eastContinentBindingLabel.Location = (new global::System.Drawing.Point(279, 240));
        this.eastContinentBindingLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.eastContinentBindingLabel.Name = ("eastContinentBindingLabel");
        this.eastContinentBindingLabel.Size = (new global::System.Drawing.Size(124, 15));
        this.eastContinentBindingLabel.TabIndex = (37);
        this.eastContinentBindingLabel.Text = ("East Continent Biome:");
        this.toolTip1.SetToolTip(this.eastContinentBindingLabel, "East Hyrule overworld map style.");
        // 
        // deathMountainBiomeLabel
        // 
        this.deathMountainBiomeLabel.AutoSize = (true);
        this.deathMountainBiomeLabel.Location = (new global::System.Drawing.Point(279, 209));
        this.deathMountainBiomeLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.deathMountainBiomeLabel.Name = ("deathMountainBiomeLabel");
        this.deathMountainBiomeLabel.Size = (new global::System.Drawing.Size(133, 15));
        this.deathMountainBiomeLabel.TabIndex = (36);
        this.deathMountainBiomeLabel.Text = ("Death Mountain Biome:");
        this.toolTip1.SetToolTip(this.deathMountainBiomeLabel, "Death Mountain overworld map style.");
        // 
        // label39
        // 
        this.label39.BorderStyle = (global::System.Windows.Forms.BorderStyle.Fixed3D);
        this.label39.Location = (new global::System.Drawing.Point(281, 170));
        this.label39.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label39.Name = ("label39");
        this.label39.Size = (new global::System.Drawing.Size(298, 2));
        this.label39.TabIndex = (35);
        // 
        // westContinentLabel
        // 
        this.westContinentLabel.AutoSize = (true);
        this.westContinentLabel.Location = (new global::System.Drawing.Point(279, 179));
        this.westContinentLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.westContinentLabel.Name = ("westContinentLabel");
        this.westContinentLabel.Size = (new global::System.Drawing.Size(129, 15));
        this.westContinentLabel.TabIndex = (34);
        this.westContinentLabel.Text = ("West Continent Biome:");
        this.toolTip1.SetToolTip(this.westContinentLabel, "West Hyrule overworld map style.");
        // 
        // allowBoulderBlockedConnectionsCheckbox
        // 
        this.allowBoulderBlockedConnectionsCheckbox.AutoSize = (true);
        this.allowBoulderBlockedConnectionsCheckbox.Location = (new global::System.Drawing.Point(282, 59));
        this.allowBoulderBlockedConnectionsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.allowBoulderBlockedConnectionsCheckbox.Name = ("allowBoulderBlockedConnectionsCheckbox");
        this.allowBoulderBlockedConnectionsCheckbox.Size = (new global::System.Drawing.Size(274, 19));
        this.allowBoulderBlockedConnectionsCheckbox.TabIndex = (33);
        this.allowBoulderBlockedConnectionsCheckbox.Text = ("Allow Connection Caves to be Boulder Blocked");
        this.toolTip1.SetToolTip(this.allowBoulderBlockedConnectionsCheckbox, "When selected, allows boulders to block any cave.");
        this.allowBoulderBlockedConnectionsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // saneCaveShuffleBox
        // 
        this.saneCaveShuffleBox.AutoSize = (true);
        this.saneCaveShuffleBox.Location = (new global::System.Drawing.Point(282, 32));
        this.saneCaveShuffleBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.saneCaveShuffleBox.Name = ("saneCaveShuffleBox");
        this.saneCaveShuffleBox.Size = (new global::System.Drawing.Size(199, 19));
        this.saneCaveShuffleBox.TabIndex = (32);
        this.saneCaveShuffleBox.Text = ("Restrict Connection Cave Shuffle");
        this.saneCaveShuffleBox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.saneCaveShuffleBox, "When selected, caves will be placed in a more logical manner in which they \"point\" at their destination.");
        this.saneCaveShuffleBox.UseVisualStyleBackColor = (true);
        // 
        // hideLessImportantLocationsCheckbox
        // 
        this.hideLessImportantLocationsCheckbox.AutoSize = (true);
        this.hideLessImportantLocationsCheckbox.Location = (new global::System.Drawing.Point(282, 7));
        this.hideLessImportantLocationsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.hideLessImportantLocationsCheckbox.Name = ("hideLessImportantLocationsCheckbox");
        this.hideLessImportantLocationsCheckbox.Size = (new global::System.Drawing.Size(186, 19));
        this.hideLessImportantLocationsCheckbox.TabIndex = (31);
        this.hideLessImportantLocationsCheckbox.Text = ("Hide Less Important Locations");
        this.hideLessImportantLocationsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.hideLessImportantLocationsCheckbox, "When selected, blends unimportant locations in with the surrounding terrain.");
        this.hideLessImportantLocationsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // ContinentConnectionLabel
        // 
        this.ContinentConnectionLabel.AutoSize = (true);
        this.ContinentConnectionLabel.Location = (new global::System.Drawing.Point(279, 144));
        this.ContinentConnectionLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.ContinentConnectionLabel.Name = ("ContinentConnectionLabel");
        this.ContinentConnectionLabel.Size = (new global::System.Drawing.Size(133, 15));
        this.ContinentConnectionLabel.TabIndex = (30);
        this.ContinentConnectionLabel.Text = ("Continent Connections:");
        this.toolTip1.SetToolTip(this.ContinentConnectionLabel, "Modes for how the different continents can connect to each other.");
        // 
        // continentConnectionBox
        // 
        this.continentConnectionBox.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.continentConnectionBox.FormattingEnabled = (true);
        this.continentConnectionBox.Items.AddRange(new global::System.Object[] { "Normal", "R+B Border Shuffle", "Transportation Shuffle", "Anything Goes" });
        this.continentConnectionBox.Location = (new global::System.Drawing.Point(422, 141));
        this.continentConnectionBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.continentConnectionBox.Name = ("continentConnectionBox");
        this.continentConnectionBox.Size = (new global::System.Drawing.Size(140, 23));
        this.continentConnectionBox.TabIndex = (29);
        this.toolTip1.SetToolTip(this.continentConnectionBox, "Modes for how the different continents can connect to each other.");
        // 
        // label36
        // 
        this.label36.BorderStyle = (global::System.Windows.Forms.BorderStyle.Fixed3D);
        this.label36.Location = (new global::System.Drawing.Point(7, 187));
        this.label36.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label36.Name = ("label36");
        this.label36.Size = (new global::System.Drawing.Size(243, 1));
        this.label36.TabIndex = (28);
        // 
        // encounterRateBox
        // 
        this.encounterRateBox.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.encounterRateBox.FormattingEnabled = (true);
        this.encounterRateBox.Items.AddRange(new global::System.Object[] { "Normal", "50%", "None", "Random" });
        this.encounterRateBox.Location = (new global::System.Drawing.Point(108, 156));
        this.encounterRateBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.encounterRateBox.Name = ("encounterRateBox");
        this.encounterRateBox.Size = (new global::System.Drawing.Size(100, 23));
        this.encounterRateBox.TabIndex = (27);
        this.toolTip1.SetToolTip(this.encounterRateBox, "Allows you to reduce the encounter rate or turn encounters off entirely.");
        // 
        // encounterRateLabel
        // 
        this.encounterRateLabel.AutoSize = (true);
        this.encounterRateLabel.Location = (new global::System.Drawing.Point(4, 159));
        this.encounterRateLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.encounterRateLabel.Name = ("encounterRateLabel");
        this.encounterRateLabel.Size = (new global::System.Drawing.Size(90, 15));
        this.encounterRateLabel.TabIndex = (26);
        this.encounterRateLabel.Text = ("Encounter Rate:");
        this.toolTip1.SetToolTip(this.encounterRateLabel, "Allows you to reduce the encounter rate or turn encounters off entirely.");
        // 
        // hideKasutoList
        // 
        this.hideKasutoList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.hideKasutoList.FormattingEnabled = (true);
        this.hideKasutoList.Items.AddRange(new global::System.Object[] { "Off", "On", "Random" });
        this.hideKasutoList.Location = (new global::System.Drawing.Point(108, 227));
        this.hideKasutoList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.hideKasutoList.Name = ("hideKasutoList");
        this.hideKasutoList.Size = (new global::System.Drawing.Size(100, 23));
        this.hideKasutoList.TabIndex = (25);
        this.toolTip1.SetToolTip(this.hideKasutoList, "When selected, will hide Kasuto behind a forest tile");
        this.hideKasutoList.SelectedIndexChanged += (this.HideKasutoBox_SelectedIndexChanged);
        // 
        // hiddenKasutoLabel
        // 
        this.hiddenKasutoLabel.AutoSize = (true);
        this.hiddenKasutoLabel.Location = (new global::System.Drawing.Point(5, 229));
        this.hiddenKasutoLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.hiddenKasutoLabel.Name = ("hiddenKasutoLabel");
        this.hiddenKasutoLabel.Size = (new global::System.Drawing.Size(88, 15));
        this.hiddenKasutoLabel.TabIndex = (24);
        this.hiddenKasutoLabel.Text = ("Hidden Kasuto:");
        // 
        // hiddenPalaceList
        // 
        this.hiddenPalaceList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.hiddenPalaceList.FormattingEnabled = (true);
        this.hiddenPalaceList.Items.AddRange(new global::System.Object[] { "Off", "On", "Random" });
        this.hiddenPalaceList.Location = (new global::System.Drawing.Point(108, 196));
        this.hiddenPalaceList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.hiddenPalaceList.Name = ("hiddenPalaceList");
        this.hiddenPalaceList.Size = (new global::System.Drawing.Size(100, 23));
        this.hiddenPalaceList.TabIndex = (23);
        this.toolTip1.SetToolTip(this.hiddenPalaceList, "When selected, will include three eye rock on the overworld");
        this.hiddenPalaceList.SelectedIndexChanged += (this.HpCmbo_SelectedIndexChanged);
        // 
        // hiddenPalaceLabel
        // 
        this.hiddenPalaceLabel.AutoSize = (true);
        this.hiddenPalaceLabel.Location = (new global::System.Drawing.Point(5, 199));
        this.hiddenPalaceLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.hiddenPalaceLabel.Name = ("hiddenPalaceLabel");
        this.hiddenPalaceLabel.Size = (new global::System.Drawing.Size(86, 15));
        this.hiddenPalaceLabel.TabIndex = (22);
        this.hiddenPalaceLabel.Text = ("Hidden Palace:");
        // 
        // includeGPinShuffleCheckbox
        // 
        this.includeGPinShuffleCheckbox.AutoSize = (true);
        this.includeGPinShuffleCheckbox.Location = (new global::System.Drawing.Point(7, 32));
        this.includeGPinShuffleCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.includeGPinShuffleCheckbox.Name = ("includeGPinShuffleCheckbox");
        this.includeGPinShuffleCheckbox.Size = (new global::System.Drawing.Size(186, 19));
        this.includeGPinShuffleCheckbox.TabIndex = (21);
        this.includeGPinShuffleCheckbox.Text = ("Include Great Palace in Shuffle");
        this.includeGPinShuffleCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.includeGPinShuffleCheckbox, "When selected, palace 7 does not have to be in the valley of death.");
        this.includeGPinShuffleCheckbox.UseVisualStyleBackColor = (true);
        // 
        // allowPalaceContinentSwapCheckbox
        // 
        this.allowPalaceContinentSwapCheckbox.AutoSize = (true);
        this.allowPalaceContinentSwapCheckbox.Location = (new global::System.Drawing.Point(7, 7));
        this.allowPalaceContinentSwapCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.allowPalaceContinentSwapCheckbox.Name = ("allowPalaceContinentSwapCheckbox");
        this.allowPalaceContinentSwapCheckbox.Size = (new global::System.Drawing.Size(204, 19));
        this.allowPalaceContinentSwapCheckbox.TabIndex = (20);
        this.allowPalaceContinentSwapCheckbox.Text = ("Allow Palaces to Swap Continents");
        this.allowPalaceContinentSwapCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.allowPalaceContinentSwapCheckbox, "When selected, palaces can move from their normal continents. Palace 1 could be found on Maze Island or East Hyrule, for example.");
        this.allowPalaceContinentSwapCheckbox.UseVisualStyleBackColor = (true);
        this.allowPalaceContinentSwapCheckbox.CheckStateChanged += (this.PalaceSwapBox_CheckStateChanged);
        // 
        // label4
        // 
        this.label4.BorderStyle = (global::System.Windows.Forms.BorderStyle.Fixed3D);
        this.label4.Location = (new global::System.Drawing.Point(7, 63));
        this.label4.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label4.Name = ("label4");
        this.label4.Size = (new global::System.Drawing.Size(243, 1));
        this.label4.TabIndex = (18);
        // 
        // allowPathEnemiesCheckbox
        // 
        this.allowPathEnemiesCheckbox.AutoSize = (true);
        this.allowPathEnemiesCheckbox.Location = (new global::System.Drawing.Point(7, 104));
        this.allowPathEnemiesCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.allowPathEnemiesCheckbox.Name = ("allowPathEnemiesCheckbox");
        this.allowPathEnemiesCheckbox.Size = (new global::System.Drawing.Size(184, 19));
        this.allowPathEnemiesCheckbox.TabIndex = (15);
        this.allowPathEnemiesCheckbox.Text = ("Allow Unsafe Path Encounters");
        this.toolTip1.SetToolTip(this.allowPathEnemiesCheckbox, "If checked, you may have enemies in path encounters");
        this.allowPathEnemiesCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleEncountersCheckbox
        // 
        this.shuffleEncountersCheckbox.AutoSize = (true);
        this.shuffleEncountersCheckbox.Location = (new global::System.Drawing.Point(7, 78));
        this.shuffleEncountersCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleEncountersCheckbox.Name = ("shuffleEncountersCheckbox");
        this.shuffleEncountersCheckbox.Size = (new global::System.Drawing.Size(125, 19));
        this.shuffleEncountersCheckbox.TabIndex = (14);
        this.shuffleEncountersCheckbox.Text = ("Shuffle Encounters");
        this.shuffleEncountersCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shuffleEncountersCheckbox, "Shuffle which overworld encounters occur on different terrain types");
        this.shuffleEncountersCheckbox.UseVisualStyleBackColor = (true);
        this.shuffleEncountersCheckbox.CheckStateChanged += (this.shuffleEncounters_CheckStateChanged);
        // 
        // tabPage2
        // 
        this.tabPage2.Controls.Add(this.noDuplicateRoomsCheckbox);
        this.tabPage2.Controls.Add(this.label7);
        this.tabPage2.Controls.Add(this.label5);
        this.tabPage2.Controls.Add(this.startingGemsMaxList);
        this.tabPage2.Controls.Add(this.palaceStyleLabel);
        this.tabPage2.Controls.Add(this.palaceStyleList);
        this.tabPage2.Controls.Add(this.bossRoomsExitToPalaceCheckbox);
        this.tabPage2.Controls.Add(this.blockingRoomsInAnyPalaceCheckbox);
        this.tabPage2.Controls.Add(this.includeCommunityRoomsCheckbox);
        this.tabPage2.Controls.Add(this.randomizeBossItemCheckbox);
        this.tabPage2.Controls.Add(this.removeTbirdCheckbox);
        this.tabPage2.Controls.Add(this.shortGPCheckbox);
        this.tabPage2.Controls.Add(this.restartAtPalacesCheckbox);
        this.tabPage2.Controls.Add(this.palacePaletteCheckbox);
        this.tabPage2.Controls.Add(this.tbirdRequiredCheckbox);
        this.tabPage2.Controls.Add(this.label6);
        this.tabPage2.Controls.Add(this.startingGemsMinList);
        this.tabPage2.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage2.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage2.Name = ("tabPage2");
        this.tabPage2.Padding = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage2.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage2.TabIndex = (1);
        this.tabPage2.Text = ("Palaces");
        this.tabPage2.UseVisualStyleBackColor = (true);
        // 
        // noDuplicateRoomsCheckbox
        // 
        this.noDuplicateRoomsCheckbox.AutoSize = (true);
        this.noDuplicateRoomsCheckbox.Location = (new global::System.Drawing.Point(281, 87));
        this.noDuplicateRoomsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.noDuplicateRoomsCheckbox.Name = ("noDuplicateRoomsCheckbox");
        this.noDuplicateRoomsCheckbox.RightToLeft = (global::System.Windows.Forms.RightToLeft.No);
        this.noDuplicateRoomsCheckbox.Size = (new global::System.Drawing.Size(135, 19));
        this.noDuplicateRoomsCheckbox.TabIndex = (25);
        this.noDuplicateRoomsCheckbox.Text = ("No Duplicate Rooms");
        this.noDuplicateRoomsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.noDuplicateRoomsCheckbox, "Each room will only show up at most once in a palace. Rooms that have multiple variations can still have one of each variation.");
        this.noDuplicateRoomsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // label7
        // 
        this.label7.AutoSize = (true);
        this.label7.Location = (new global::System.Drawing.Point(119, 270));
        this.label7.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label7.Name = ("label7");
        this.label7.Size = (new global::System.Drawing.Size(30, 15));
        this.label7.TabIndex = (24);
        this.label7.Text = ("Max");
        this.toolTip1.SetToolTip(this.label7, "Starting Attack Level");
        // 
        // label5
        // 
        this.label5.AutoSize = (true);
        this.label5.Location = (new global::System.Drawing.Point(31, 270));
        this.label5.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label5.Name = ("label5");
        this.label5.Size = (new global::System.Drawing.Size(28, 15));
        this.label5.TabIndex = (23);
        this.label5.Text = ("Min");
        this.toolTip1.SetToolTip(this.label5, "Starting Attack Level");
        // 
        // startingGemsMaxList
        // 
        this.startingGemsMaxList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startingGemsMaxList.FormattingEnabled = (true);
        this.startingGemsMaxList.Items.AddRange(new global::System.Object[] { "0", "1", "2", "3", "4", "5", "6" });
        this.startingGemsMaxList.Location = (new global::System.Drawing.Point(98, 287));
        this.startingGemsMaxList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startingGemsMaxList.Name = ("startingGemsMaxList");
        this.startingGemsMaxList.Size = (new global::System.Drawing.Size(72, 23));
        this.startingGemsMaxList.TabIndex = (15);
        this.toolTip1.SetToolTip(this.startingGemsMaxList, "How many gems need to be placed before entering palace 7");
        // 
        // palaceStyleLabel
        // 
        this.palaceStyleLabel.AutoSize = (true);
        this.palaceStyleLabel.Location = (new global::System.Drawing.Point(7, 12));
        this.palaceStyleLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.palaceStyleLabel.Name = ("palaceStyleLabel");
        this.palaceStyleLabel.Size = (new global::System.Drawing.Size(72, 15));
        this.palaceStyleLabel.TabIndex = (14);
        this.palaceStyleLabel.Text = ("Palace Style:");
        this.toolTip1.SetToolTip(this.palaceStyleLabel, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // palaceStyleList
        // 
        this.palaceStyleList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.palaceStyleList.FormattingEnabled = (true);
        this.palaceStyleList.Items.AddRange(new global::System.Object[] { "Vanilla", "Shuffled", "Reconstructed", "Random" });
        this.palaceStyleList.Location = (new global::System.Drawing.Point(7, 29));
        this.palaceStyleList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.palaceStyleList.Name = ("palaceStyleList");
        this.palaceStyleList.Size = (new global::System.Drawing.Size(176, 23));
        this.palaceStyleList.TabIndex = (13);
        this.toolTip1.SetToolTip(this.palaceStyleList, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        this.palaceStyleList.SelectedIndexChanged += (this.PalaceBox_SelectedIndexChanged);
        // 
        // bossRoomsExitToPalaceCheckbox
        // 
        this.bossRoomsExitToPalaceCheckbox.AutoSize = (true);
        this.bossRoomsExitToPalaceCheckbox.Location = (new global::System.Drawing.Point(7, 113));
        this.bossRoomsExitToPalaceCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.bossRoomsExitToPalaceCheckbox.Name = ("bossRoomsExitToPalaceCheckbox");
        this.bossRoomsExitToPalaceCheckbox.Size = (new global::System.Drawing.Size(163, 19));
        this.bossRoomsExitToPalaceCheckbox.TabIndex = (12);
        this.bossRoomsExitToPalaceCheckbox.Text = ("Boss Rooms Exit to Palace");
        this.bossRoomsExitToPalaceCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.bossRoomsExitToPalaceCheckbox, "When selected, boss rooms will no longer lead outside, they will lead to more palace");
        this.bossRoomsExitToPalaceCheckbox.UseVisualStyleBackColor = (true);
        // 
        // blockingRoomsInAnyPalaceCheckbox
        // 
        this.blockingRoomsInAnyPalaceCheckbox.AutoSize = (true);
        this.blockingRoomsInAnyPalaceCheckbox.Location = (new global::System.Drawing.Point(7, 87));
        this.blockingRoomsInAnyPalaceCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.blockingRoomsInAnyPalaceCheckbox.Name = ("blockingRoomsInAnyPalaceCheckbox");
        this.blockingRoomsInAnyPalaceCheckbox.Size = (new global::System.Drawing.Size(251, 19));
        this.blockingRoomsInAnyPalaceCheckbox.TabIndex = (11);
        this.blockingRoomsInAnyPalaceCheckbox.Text = ("Blocking Rooms Can Appear in Any Palace");
        this.toolTip1.SetToolTip(this.blockingRoomsInAnyPalaceCheckbox, "When selected, a palace can be blocked by any of the item/spell blocked rooms");
        this.blockingRoomsInAnyPalaceCheckbox.UseVisualStyleBackColor = (true);
        // 
        // includeCommunityRoomsCheckbox
        // 
        this.includeCommunityRoomsCheckbox.AutoSize = (true);
        this.includeCommunityRoomsCheckbox.Location = (new global::System.Drawing.Point(7, 60));
        this.includeCommunityRoomsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.includeCommunityRoomsCheckbox.Name = ("includeCommunityRoomsCheckbox");
        this.includeCommunityRoomsCheckbox.Size = (new global::System.Drawing.Size(172, 19));
        this.includeCommunityRoomsCheckbox.TabIndex = (10);
        this.includeCommunityRoomsCheckbox.Text = ("Include Community Rooms");
        this.includeCommunityRoomsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.includeCommunityRoomsCheckbox, "When selected, rooms created by the Zelda 2 community will be included in the room pool");
        this.includeCommunityRoomsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // randomizeBossItemCheckbox
        // 
        this.randomizeBossItemCheckbox.AutoSize = (true);
        this.randomizeBossItemCheckbox.Location = (new global::System.Drawing.Point(281, 60));
        this.randomizeBossItemCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.randomizeBossItemCheckbox.Name = ("randomizeBossItemCheckbox");
        this.randomizeBossItemCheckbox.Size = (new global::System.Drawing.Size(168, 19));
        this.randomizeBossItemCheckbox.TabIndex = (8);
        this.randomizeBossItemCheckbox.Text = ("Randomize Boss Item Drop");
        this.toolTip1.SetToolTip(this.randomizeBossItemCheckbox, "When selected, the item that drops after a boss has been killed will be randomized");
        this.randomizeBossItemCheckbox.UseVisualStyleBackColor = (true);
        // 
        // removeTbirdCheckbox
        // 
        this.removeTbirdCheckbox.AutoSize = (true);
        this.removeTbirdCheckbox.Location = (new global::System.Drawing.Point(7, 213));
        this.removeTbirdCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.removeTbirdCheckbox.Name = ("removeTbirdCheckbox");
        this.removeTbirdCheckbox.Size = (new global::System.Drawing.Size(137, 19));
        this.removeTbirdCheckbox.TabIndex = (7);
        this.removeTbirdCheckbox.Text = ("Remove Thunderbird");
        this.toolTip1.SetToolTip(this.removeTbirdCheckbox, "If checked, you must defeat thunderbird");
        this.removeTbirdCheckbox.UseVisualStyleBackColor = (true);
        this.removeTbirdCheckbox.CheckStateChanged += (this.RemoveTbird_CheckStateChanged);
        // 
        // shortGPCheckbox
        // 
        this.shortGPCheckbox.AutoSize = (true);
        this.shortGPCheckbox.Location = (new global::System.Drawing.Point(7, 160));
        this.shortGPCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shortGPCheckbox.Name = ("shortGPCheckbox");
        this.shortGPCheckbox.Size = (new global::System.Drawing.Size(135, 19));
        this.shortGPCheckbox.TabIndex = (6);
        this.shortGPCheckbox.Text = ("Shorten Great Palace");
        this.shortGPCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shortGPCheckbox, "When selected, the Great Palace will have fewer rooms than normal");
        this.shortGPCheckbox.UseVisualStyleBackColor = (true);
        // 
        // restartAtPalacesCheckbox
        // 
        this.restartAtPalacesCheckbox.AutoSize = (true);
        this.restartAtPalacesCheckbox.Location = (new global::System.Drawing.Point(281, 7));
        this.restartAtPalacesCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.restartAtPalacesCheckbox.Name = ("restartAtPalacesCheckbox");
        this.restartAtPalacesCheckbox.Size = (new global::System.Drawing.Size(193, 19));
        this.restartAtPalacesCheckbox.TabIndex = (5);
        this.restartAtPalacesCheckbox.Text = ("Restart at palaces on game over");
        this.toolTip1.SetToolTip(this.restartAtPalacesCheckbox, "When selected, if you game over in a palace, you will restart at that palace instead of the normal starting spot");
        this.restartAtPalacesCheckbox.UseVisualStyleBackColor = (true);
        // 
        // palacePaletteCheckbox
        // 
        this.palacePaletteCheckbox.AutoSize = (true);
        this.palacePaletteCheckbox.Location = (new global::System.Drawing.Point(281, 33));
        this.palacePaletteCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.palacePaletteCheckbox.Name = ("palacePaletteCheckbox");
        this.palacePaletteCheckbox.Size = (new global::System.Drawing.Size(148, 19));
        this.palacePaletteCheckbox.TabIndex = (4);
        this.palacePaletteCheckbox.Text = ("Change Palace Palettes");
        this.toolTip1.SetToolTip(this.palacePaletteCheckbox, "This option changes the colors and tileset of palaces");
        this.palacePaletteCheckbox.UseVisualStyleBackColor = (true);
        // 
        // tbirdRequiredCheckbox
        // 
        this.tbirdRequiredCheckbox.AutoSize = (true);
        this.tbirdRequiredCheckbox.Location = (new global::System.Drawing.Point(7, 187));
        this.tbirdRequiredCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tbirdRequiredCheckbox.Name = ("tbirdRequiredCheckbox");
        this.tbirdRequiredCheckbox.Size = (new global::System.Drawing.Size(141, 19));
        this.tbirdRequiredCheckbox.TabIndex = (3);
        this.tbirdRequiredCheckbox.Text = ("Thunderbird Required");
        this.tbirdRequiredCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.tbirdRequiredCheckbox, "If checked, you must defeat thunderbird");
        this.tbirdRequiredCheckbox.UseVisualStyleBackColor = (true);
        this.tbirdRequiredCheckbox.CheckStateChanged += (this.TbirdBox_CheckStateChanged);
        // 
        // label6
        // 
        this.label6.AutoSize = (true);
        this.label6.Location = (new global::System.Drawing.Point(7, 251));
        this.label6.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label6.Name = ("label6");
        this.label6.Size = (new global::System.Drawing.Size(176, 15));
        this.label6.TabIndex = (2);
        this.label6.Text = ("Number of Palaces to Complete");
        // 
        // startingGemsMinList
        // 
        this.startingGemsMinList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.startingGemsMinList.FormattingEnabled = (true);
        this.startingGemsMinList.Items.AddRange(new global::System.Object[] { "0", "1", "2", "3", "4", "5", "6" });
        this.startingGemsMinList.Location = (new global::System.Drawing.Point(7, 288));
        this.startingGemsMinList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.startingGemsMinList.Name = ("startingGemsMinList");
        this.startingGemsMinList.Size = (new global::System.Drawing.Size(72, 23));
        this.startingGemsMinList.TabIndex = (1);
        this.toolTip1.SetToolTip(this.startingGemsMinList, "How many gems need to be placed before entering palace 7");
        // 
        // tabPage5
        // 
        this.tabPage5.Controls.Add(this.lifeEffectivenessList);
        this.tabPage5.Controls.Add(this.magicEffectivenessList);
        this.tabPage5.Controls.Add(this.attackEffectivenessList);
        this.tabPage5.Controls.Add(this.scaleLevelRequirementsToCapCheckbox);
        this.tabPage5.Controls.Add(this.levelCapLabel);
        this.tabPage5.Controls.Add(this.lifeCapList);
        this.tabPage5.Controls.Add(this.magCapList);
        this.tabPage5.Controls.Add(this.atkCapList);
        this.tabPage5.Controls.Add(this.lifeCapLabel);
        this.tabPage5.Controls.Add(this.magCapLabel);
        this.tabPage5.Controls.Add(this.attackCapLabel);
        this.tabPage5.Controls.Add(this.label12);
        this.tabPage5.Controls.Add(this.magicEffectivenessLabel);
        this.tabPage5.Controls.Add(this.attackEffectivenessLabel);
        this.tabPage5.Controls.Add(this.expBox);
        this.tabPage5.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage5.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage5.Name = ("tabPage5");
        this.tabPage5.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage5.TabIndex = (4);
        this.tabPage5.Text = ("Levels");
        this.tabPage5.UseVisualStyleBackColor = (true);
        // 
        // lifeEffectivenessList
        // 
        this.lifeEffectivenessList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.lifeEffectivenessList.FormattingEnabled = (true);
        this.lifeEffectivenessList.Items.AddRange(new global::System.Object[] { "Random", "OHKO Link", "Vanilla", "High Defense", "Invincible" });
        this.lifeEffectivenessList.Location = (new global::System.Drawing.Point(392, 111));
        this.lifeEffectivenessList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.lifeEffectivenessList.Name = ("lifeEffectivenessList");
        this.lifeEffectivenessList.Size = (new global::System.Drawing.Size(140, 23));
        this.lifeEffectivenessList.TabIndex = (25);
        this.toolTip1.SetToolTip(this.lifeEffectivenessList, "Different modes for the effectiveness of Life levels");
        // 
        // magicEffectivenessList
        // 
        this.magicEffectivenessList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.magicEffectivenessList.FormattingEnabled = (true);
        this.magicEffectivenessList.Items.AddRange(new global::System.Object[] { "Random", "High Spell Cost", "Vanilla", "Low Spell Cost", "Free Spells" });
        this.magicEffectivenessList.Location = (new global::System.Drawing.Point(392, 66));
        this.magicEffectivenessList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.magicEffectivenessList.Name = ("magicEffectivenessList");
        this.magicEffectivenessList.Size = (new global::System.Drawing.Size(140, 23));
        this.magicEffectivenessList.TabIndex = (24);
        this.toolTip1.SetToolTip(this.magicEffectivenessList, "Different modes for the effectiveness of Magic levels");
        // 
        // attackEffectivenessList
        // 
        this.attackEffectivenessList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.attackEffectivenessList.FormattingEnabled = (true);
        this.attackEffectivenessList.Items.AddRange(new global::System.Object[] { "Random", "Low Attack", "Vanilla", "High Attack", "OHKO Enemies" });
        this.attackEffectivenessList.Location = (new global::System.Drawing.Point(392, 23));
        this.attackEffectivenessList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.attackEffectivenessList.Name = ("attackEffectivenessList");
        this.attackEffectivenessList.Size = (new global::System.Drawing.Size(140, 23));
        this.attackEffectivenessList.TabIndex = (23);
        this.toolTip1.SetToolTip(this.attackEffectivenessList, "Different modes for the effectiveness of Attack levels");
        // 
        // scaleLevelRequirementsToCapCheckbox
        // 
        this.scaleLevelRequirementsToCapCheckbox.AutoSize = (true);
        this.scaleLevelRequirementsToCapCheckbox.Location = (new global::System.Drawing.Point(10, 188));
        this.scaleLevelRequirementsToCapCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.scaleLevelRequirementsToCapCheckbox.Name = ("scaleLevelRequirementsToCapCheckbox");
        this.scaleLevelRequirementsToCapCheckbox.Size = (new global::System.Drawing.Size(197, 19));
        this.scaleLevelRequirementsToCapCheckbox.TabIndex = (22);
        this.scaleLevelRequirementsToCapCheckbox.Text = ("Scale Level Requirements to Cap");
        this.toolTip1.SetToolTip(this.scaleLevelRequirementsToCapCheckbox, "When selected, experience requirements will be scaled up based on the maximum level from the level cap");
        this.scaleLevelRequirementsToCapCheckbox.UseVisualStyleBackColor = (true);
        // 
        // levelCapLabel
        // 
        this.levelCapLabel.AutoSize = (true);
        this.levelCapLabel.Location = (new global::System.Drawing.Point(88, 108));
        this.levelCapLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.levelCapLabel.Name = ("levelCapLabel");
        this.levelCapLabel.Size = (new global::System.Drawing.Size(58, 15));
        this.levelCapLabel.TabIndex = (21);
        this.levelCapLabel.Text = ("Level Cap");
        // 
        // lifeCapList
        // 
        this.lifeCapList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.lifeCapList.FormattingEnabled = (true);
        this.lifeCapList.Items.AddRange(new global::System.Object[] { "8", "7", "6", "5", "4", "3", "2", "1" });
        this.lifeCapList.Location = (new global::System.Drawing.Point(144, 130));
        this.lifeCapList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.lifeCapList.Name = ("lifeCapList");
        this.lifeCapList.Size = (new global::System.Drawing.Size(37, 23));
        this.lifeCapList.TabIndex = (20);
        this.toolTip1.SetToolTip(this.lifeCapList, "Maximum Life Level");
        this.lifeCapList.SelectedIndexChanged += (this.EnableLevelScaling);
        // 
        // magCapList
        // 
        this.magCapList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.magCapList.FormattingEnabled = (true);
        this.magCapList.Items.AddRange(new global::System.Object[] { "8", "7", "6", "5", "4", "3", "2", "1" });
        this.magCapList.Location = (new global::System.Drawing.Point(99, 130));
        this.magCapList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.magCapList.Name = ("magCapList");
        this.magCapList.Size = (new global::System.Drawing.Size(37, 23));
        this.magCapList.TabIndex = (19);
        this.toolTip1.SetToolTip(this.magCapList, "Maximum Magic Level");
        this.magCapList.SelectedIndexChanged += (this.EnableLevelScaling);
        // 
        // atkCapList
        // 
        this.atkCapList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.atkCapList.FormattingEnabled = (true);
        this.atkCapList.Items.AddRange(new global::System.Object[] { "8", "7", "6", "5", "4", "3", "2", "1" });
        this.atkCapList.Location = (new global::System.Drawing.Point(55, 130));
        this.atkCapList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.atkCapList.Name = ("atkCapList");
        this.atkCapList.Size = (new global::System.Drawing.Size(37, 23));
        this.atkCapList.TabIndex = (18);
        this.toolTip1.SetToolTip(this.atkCapList, "Maximum Attack Level");
        this.atkCapList.SelectedIndexChanged += (this.EnableLevelScaling);
        // 
        // lifeCapLabel
        // 
        this.lifeCapLabel.AutoSize = (true);
        this.lifeCapLabel.Location = (new global::System.Drawing.Point(148, 163));
        this.lifeCapLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.lifeCapLabel.Name = ("lifeCapLabel");
        this.lifeCapLabel.Size = (new global::System.Drawing.Size(26, 15));
        this.lifeCapLabel.TabIndex = (17);
        this.lifeCapLabel.Text = ("Life");
        this.toolTip1.SetToolTip(this.lifeCapLabel, "Maximum Life Level");
        // 
        // magCapLabel
        // 
        this.magCapLabel.AutoSize = (true);
        this.magCapLabel.Location = (new global::System.Drawing.Point(102, 163));
        this.magCapLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.magCapLabel.Name = ("magCapLabel");
        this.magCapLabel.Size = (new global::System.Drawing.Size(31, 15));
        this.magCapLabel.TabIndex = (16);
        this.magCapLabel.Text = ("Mag");
        this.toolTip1.SetToolTip(this.magCapLabel, "Maximum Magic Level");
        // 
        // attackCapLabel
        // 
        this.attackCapLabel.AutoSize = (true);
        this.attackCapLabel.Location = (new global::System.Drawing.Point(61, 163));
        this.attackCapLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.attackCapLabel.Name = ("attackCapLabel");
        this.attackCapLabel.Size = (new global::System.Drawing.Size(25, 15));
        this.attackCapLabel.TabIndex = (15);
        this.attackCapLabel.Text = ("Atk");
        this.toolTip1.SetToolTip(this.attackCapLabel, "Maximum Attack Level");
        // 
        // label12
        // 
        this.label12.AutoSize = (true);
        this.label12.Location = (new global::System.Drawing.Point(261, 114));
        this.label12.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label12.Name = ("label12");
        this.label12.Size = (new global::System.Drawing.Size(100, 15));
        this.label12.TabIndex = (11);
        this.label12.Text = ("Life Effectiveness:");
        this.toolTip1.SetToolTip(this.label12, "Different modes for the effectiveness of Life levels");
        // 
        // magicEffectivenessLabel
        // 
        this.magicEffectivenessLabel.AutoSize = (true);
        this.magicEffectivenessLabel.Location = (new global::System.Drawing.Point(261, 69));
        this.magicEffectivenessLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.magicEffectivenessLabel.Name = ("magicEffectivenessLabel");
        this.magicEffectivenessLabel.Size = (new global::System.Drawing.Size(114, 15));
        this.magicEffectivenessLabel.TabIndex = (10);
        this.magicEffectivenessLabel.Text = ("Magic Effectiveness:");
        this.toolTip1.SetToolTip(this.magicEffectivenessLabel, "Different modes for the effectiveness of Magic levels");
        // 
        // attackEffectivenessLabel
        // 
        this.attackEffectivenessLabel.AutoSize = (true);
        this.attackEffectivenessLabel.Location = (new global::System.Drawing.Point(261, 27));
        this.attackEffectivenessLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.attackEffectivenessLabel.Name = ("attackEffectivenessLabel");
        this.attackEffectivenessLabel.Size = (new global::System.Drawing.Size(115, 15));
        this.attackEffectivenessLabel.TabIndex = (9);
        this.attackEffectivenessLabel.Text = ("Attack Effectiveness:");
        this.toolTip1.SetToolTip(this.attackEffectivenessLabel, "Different modes for the effectiveness of Attack levels");
        // 
        // expBox
        // 
        this.expBox.Controls.Add(this.lifeExpNeededCheckbox);
        this.expBox.Controls.Add(this.magicExpNeededCheckbox);
        this.expBox.Controls.Add(this.shuffleAtkExpNeededCheckbox);
        this.expBox.Controls.Add(this.shuffleAllExpCheckbox);
        this.expBox.Location = (new global::System.Drawing.Point(4, 3));
        this.expBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.expBox.Name = ("expBox");
        this.expBox.Padding = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.expBox.Size = (new global::System.Drawing.Size(246, 99));
        this.expBox.TabIndex = (0);
        this.expBox.TabStop = (false);
        this.expBox.Text = ("                                                      ");
        // 
        // lifeExpNeededCheckbox
        // 
        this.lifeExpNeededCheckbox.AutoSize = (true);
        this.lifeExpNeededCheckbox.Location = (new global::System.Drawing.Point(21, 75));
        this.lifeExpNeededCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.lifeExpNeededCheckbox.Name = ("lifeExpNeededCheckbox");
        this.lifeExpNeededCheckbox.Size = (new global::System.Drawing.Size(189, 19));
        this.lifeExpNeededCheckbox.TabIndex = (3);
        this.lifeExpNeededCheckbox.Text = ("Shuffle Life Experience Needed");
        this.toolTip1.SetToolTip(this.lifeExpNeededCheckbox, "Shuffles experience needed for life levels");
        this.lifeExpNeededCheckbox.UseVisualStyleBackColor = (true);
        // 
        // magicExpNeededCheckbox
        // 
        this.magicExpNeededCheckbox.AutoSize = (true);
        this.magicExpNeededCheckbox.Location = (new global::System.Drawing.Point(21, 48));
        this.magicExpNeededCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.magicExpNeededCheckbox.Name = ("magicExpNeededCheckbox");
        this.magicExpNeededCheckbox.Size = (new global::System.Drawing.Size(203, 19));
        this.magicExpNeededCheckbox.TabIndex = (2);
        this.magicExpNeededCheckbox.Text = ("Shuffle Magic Experience Needed");
        this.toolTip1.SetToolTip(this.magicExpNeededCheckbox, "Shuffles experience needed for magic levels");
        this.magicExpNeededCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleAtkExpNeededCheckbox
        // 
        this.shuffleAtkExpNeededCheckbox.AutoSize = (true);
        this.shuffleAtkExpNeededCheckbox.Location = (new global::System.Drawing.Point(21, 22));
        this.shuffleAtkExpNeededCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleAtkExpNeededCheckbox.Name = ("shuffleAtkExpNeededCheckbox");
        this.shuffleAtkExpNeededCheckbox.Size = (new global::System.Drawing.Size(204, 19));
        this.shuffleAtkExpNeededCheckbox.TabIndex = (1);
        this.shuffleAtkExpNeededCheckbox.Text = ("Shuffle Attack Experience Needed");
        this.toolTip1.SetToolTip(this.shuffleAtkExpNeededCheckbox, "Shuffles experience needed for attack levels");
        this.shuffleAtkExpNeededCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleAllExpCheckbox
        // 
        this.shuffleAllExpCheckbox.AutoSize = (true);
        this.shuffleAllExpCheckbox.Location = (new global::System.Drawing.Point(7, 0));
        this.shuffleAllExpCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleAllExpCheckbox.Name = ("shuffleAllExpCheckbox");
        this.shuffleAllExpCheckbox.Size = (new global::System.Drawing.Size(184, 19));
        this.shuffleAllExpCheckbox.TabIndex = (1);
        this.shuffleAllExpCheckbox.Text = ("Shuffle All Experience Needed");
        this.toolTip1.SetToolTip(this.shuffleAllExpCheckbox, "Shuffles experience needed for all levels");
        this.shuffleAllExpCheckbox.UseVisualStyleBackColor = (true);
        this.shuffleAllExpCheckbox.CheckStateChanged += (this.shuffleAllExp_CheckStateChanged);
        // 
        // tabPage9
        // 
        this.tabPage9.Controls.Add(this.FireSpellOptionLabel);
        this.tabPage9.Controls.Add(this.FireSpellBox);
        this.tabPage9.Controls.Add(this.swapUpAndDownstabCheckbox);
        this.tabPage9.Controls.Add(this.randomizeSpellSpellEnemyCheckbox);
        this.tabPage9.Controls.Add(this.disableMagicContainerRequirementCheckbox);
        this.tabPage9.Controls.Add(this.shuffleSpellLocationsCheckbox);
        this.tabPage9.Controls.Add(this.shuffleLifeRefillCheckbox);
        this.tabPage9.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage9.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage9.Name = ("tabPage9");
        this.tabPage9.Padding = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage9.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage9.TabIndex = (9);
        this.tabPage9.Text = ("Spells");
        this.tabPage9.UseVisualStyleBackColor = (true);
        // 
        // FireSpellOptionLabel
        // 
        this.FireSpellOptionLabel.AutoSize = (true);
        this.FireSpellOptionLabel.Location = (new global::System.Drawing.Point(7, 137));
        this.FireSpellOptionLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.FireSpellOptionLabel.Name = ("FireSpellOptionLabel");
        this.FireSpellOptionLabel.Size = (new global::System.Drawing.Size(57, 15));
        this.FireSpellOptionLabel.TabIndex = (24);
        this.FireSpellOptionLabel.Text = ("Fire Spell:");
        this.toolTip1.SetToolTip(this.FireSpellOptionLabel, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // FireSpellBox
        // 
        this.FireSpellBox.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.FireSpellBox.FormattingEnabled = (true);
        this.FireSpellBox.Items.AddRange(new global::System.Object[] { "Normal", "Link with Random Spell", "Replace with Dash Spell", "Random" });
        this.FireSpellBox.Location = (new global::System.Drawing.Point(7, 154));
        this.FireSpellBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.FireSpellBox.Name = ("FireSpellBox");
        this.FireSpellBox.Size = (new global::System.Drawing.Size(176, 23));
        this.FireSpellBox.TabIndex = (23);
        this.toolTip1.SetToolTip(this.FireSpellBox, "Palace modes: Shuffle - same rooms different order; Reconstructed: any rooms from the room pool can appear and palaces can change sizes.");
        // 
        // swapUpAndDownstabCheckbox
        // 
        this.swapUpAndDownstabCheckbox.AutoSize = (true);
        this.swapUpAndDownstabCheckbox.Location = (new global::System.Drawing.Point(7, 111));
        this.swapUpAndDownstabCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.swapUpAndDownstabCheckbox.Name = ("swapUpAndDownstabCheckbox");
        this.swapUpAndDownstabCheckbox.Size = (new global::System.Drawing.Size(251, 19));
        this.swapUpAndDownstabCheckbox.TabIndex = (22);
        this.swapUpAndDownstabCheckbox.Text = ("Swap the location of upstab and downstab");
        this.swapUpAndDownstabCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.swapUpAndDownstabCheckbox, "When selected, Upstab and Downstab will swap sources.");
        this.swapUpAndDownstabCheckbox.UseVisualStyleBackColor = (true);
        // 
        // randomizeSpellSpellEnemyCheckbox
        // 
        this.randomizeSpellSpellEnemyCheckbox.AutoSize = (true);
        this.randomizeSpellSpellEnemyCheckbox.Location = (new global::System.Drawing.Point(7, 86));
        this.randomizeSpellSpellEnemyCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.randomizeSpellSpellEnemyCheckbox.Name = ("randomizeSpellSpellEnemyCheckbox");
        this.randomizeSpellSpellEnemyCheckbox.Size = (new global::System.Drawing.Size(180, 19));
        this.randomizeSpellSpellEnemyCheckbox.TabIndex = (20);
        this.randomizeSpellSpellEnemyCheckbox.Text = ("Randomize Spell Spell Enemy");
        this.randomizeSpellSpellEnemyCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.randomizeSpellSpellEnemyCheckbox, "When selected, the enemy generated when the Spell spell is cast will be randomized");
        this.randomizeSpellSpellEnemyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // disableMagicContainerRequirementCheckbox
        // 
        this.disableMagicContainerRequirementCheckbox.AutoSize = (true);
        this.disableMagicContainerRequirementCheckbox.Location = (new global::System.Drawing.Point(7, 61));
        this.disableMagicContainerRequirementCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.disableMagicContainerRequirementCheckbox.Name = ("disableMagicContainerRequirementCheckbox");
        this.disableMagicContainerRequirementCheckbox.Size = (new global::System.Drawing.Size(231, 19));
        this.disableMagicContainerRequirementCheckbox.TabIndex = (18);
        this.disableMagicContainerRequirementCheckbox.Text = ("Disable Magic Container Requirements");
        this.disableMagicContainerRequirementCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.disableMagicContainerRequirementCheckbox, "When checked, you can get spells without having the necessary magic containers");
        this.disableMagicContainerRequirementCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleSpellLocationsCheckbox
        // 
        this.shuffleSpellLocationsCheckbox.AutoSize = (true);
        this.shuffleSpellLocationsCheckbox.Location = (new global::System.Drawing.Point(7, 35));
        this.shuffleSpellLocationsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleSpellLocationsCheckbox.Name = ("shuffleSpellLocationsCheckbox");
        this.shuffleSpellLocationsCheckbox.Size = (new global::System.Drawing.Size(145, 19));
        this.shuffleSpellLocationsCheckbox.TabIndex = (17);
        this.shuffleSpellLocationsCheckbox.Text = ("Shuffle Spell Locations");
        this.shuffleSpellLocationsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shuffleSpellLocationsCheckbox, "This option shuffles which towns you find the spells in");
        this.shuffleSpellLocationsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleLifeRefillCheckbox
        // 
        this.shuffleLifeRefillCheckbox.AutoSize = (true);
        this.shuffleLifeRefillCheckbox.Location = (new global::System.Drawing.Point(7, 8));
        this.shuffleLifeRefillCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleLifeRefillCheckbox.Name = ("shuffleLifeRefillCheckbox");
        this.shuffleLifeRefillCheckbox.Size = (new global::System.Drawing.Size(161, 19));
        this.shuffleLifeRefillCheckbox.TabIndex = (16);
        this.shuffleLifeRefillCheckbox.Text = ("Shuffle Life Refill Amount");
        this.toolTip1.SetToolTip(this.shuffleLifeRefillCheckbox, "Shuffles how much health is restored when the life spell is used");
        this.shuffleLifeRefillCheckbox.UseVisualStyleBackColor = (true);
        // 
        // tabPage6
        // 
        this.tabPage6.Controls.Add(this.generatorsMatchCheckBox);
        this.tabPage6.Controls.Add(this.enemyExperienceDropsLabel);
        this.tabPage6.Controls.Add(this.experienceDropsList);
        this.tabPage6.Controls.Add(this.shuffleDripperEnemyCheckbox);
        this.tabPage6.Controls.Add(this.mixLargeAndSmallCheckbox);
        this.tabPage6.Controls.Add(this.label8);
        this.tabPage6.Controls.Add(this.shufflePalaceEnemiesCheckbox);
        this.tabPage6.Controls.Add(this.shuffleOverworldEnemiesCheckbox);
        this.tabPage6.Controls.Add(this.shuffleSwordImmunityBox);
        this.tabPage6.Controls.Add(this.shuffleStealXPAmountCheckbox);
        this.tabPage6.Controls.Add(this.shuffleXPStealersCheckbox);
        this.tabPage6.Controls.Add(this.shuffleEnemyHPBox);
        this.tabPage6.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage6.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage6.Name = ("tabPage6");
        this.tabPage6.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage6.TabIndex = (5);
        this.tabPage6.Text = ("Enemies");
        this.tabPage6.UseVisualStyleBackColor = (true);
        // 
        // generatorsMatchCheckBox
        // 
        this.generatorsMatchCheckBox.AutoSize = (true);
        this.generatorsMatchCheckBox.Location = (new global::System.Drawing.Point(4, 110));
        this.generatorsMatchCheckBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.generatorsMatchCheckBox.Name = ("generatorsMatchCheckBox");
        this.generatorsMatchCheckBox.Size = (new global::System.Drawing.Size(160, 19));
        this.generatorsMatchCheckBox.TabIndex = (23);
        this.generatorsMatchCheckBox.Text = ("Generators Always Match");
        this.toolTip1.SetToolTip(this.generatorsMatchCheckBox, "Shuffle which enemies require fire to kill");
        this.generatorsMatchCheckBox.UseVisualStyleBackColor = (true);
        // 
        // enemyExperienceDropsLabel
        // 
        this.enemyExperienceDropsLabel.AutoSize = (true);
        this.enemyExperienceDropsLabel.Location = (new global::System.Drawing.Point(226, 147));
        this.enemyExperienceDropsLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.enemyExperienceDropsLabel.Name = ("enemyExperienceDropsLabel");
        this.enemyExperienceDropsLabel.Size = (new global::System.Drawing.Size(140, 15));
        this.enemyExperienceDropsLabel.TabIndex = (22);
        this.enemyExperienceDropsLabel.Text = ("Enemy Experience Drops:");
        this.toolTip1.SetToolTip(this.enemyExperienceDropsLabel, "Different modes for how much experience the enemies drop");
        // 
        // experienceDropsList
        // 
        this.experienceDropsList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.experienceDropsList.FormattingEnabled = (true);
        this.experienceDropsList.Items.AddRange(new global::System.Object[] { "Vanilla", "None", "Low", "Average", "High" });
        this.experienceDropsList.Location = (new global::System.Drawing.Point(384, 143));
        this.experienceDropsList.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.experienceDropsList.Name = ("experienceDropsList");
        this.experienceDropsList.Size = (new global::System.Drawing.Size(140, 23));
        this.experienceDropsList.TabIndex = (21);
        this.toolTip1.SetToolTip(this.experienceDropsList, "Different modes for how much experience the enemies drop");
        // 
        // shuffleDripperEnemyCheckbox
        // 
        this.shuffleDripperEnemyCheckbox.AutoSize = (true);
        this.shuffleDripperEnemyCheckbox.Location = (new global::System.Drawing.Point(4, 59));
        this.shuffleDripperEnemyCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleDripperEnemyCheckbox.Name = ("shuffleDripperEnemyCheckbox");
        this.shuffleDripperEnemyCheckbox.Size = (new global::System.Drawing.Size(144, 19));
        this.shuffleDripperEnemyCheckbox.TabIndex = (20);
        this.shuffleDripperEnemyCheckbox.Text = ("Shuffle Dripper Enemy");
        this.toolTip1.SetToolTip(this.shuffleDripperEnemyCheckbox, "When selected, the enemy spawned by the dripper will be randomized");
        this.shuffleDripperEnemyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // mixLargeAndSmallCheckbox
        // 
        this.mixLargeAndSmallCheckbox.AutoSize = (true);
        this.mixLargeAndSmallCheckbox.Location = (new global::System.Drawing.Point(4, 85));
        this.mixLargeAndSmallCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.mixLargeAndSmallCheckbox.Name = ("mixLargeAndSmallCheckbox");
        this.mixLargeAndSmallCheckbox.Size = (new global::System.Drawing.Size(180, 19));
        this.mixLargeAndSmallCheckbox.TabIndex = (18);
        this.mixLargeAndSmallCheckbox.Text = ("Mix Large and Small Enemies");
        this.mixLargeAndSmallCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.mixLargeAndSmallCheckbox, "Allows large enemies to spawn where small enemies normally spawn and vice versa");
        this.mixLargeAndSmallCheckbox.UseVisualStyleBackColor = (true);
        // 
        // label8
        // 
        this.label8.BorderStyle = (global::System.Windows.Forms.BorderStyle.Fixed3D);
        this.label8.Location = (new global::System.Drawing.Point(202, 6));
        this.label8.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label8.Name = ("label8");
        this.label8.Size = (new global::System.Drawing.Size(2, 225));
        this.label8.TabIndex = (17);
        // 
        // shufflePalaceEnemiesCheckbox
        // 
        this.shufflePalaceEnemiesCheckbox.AutoSize = (true);
        this.shufflePalaceEnemiesCheckbox.Location = (new global::System.Drawing.Point(4, 32));
        this.shufflePalaceEnemiesCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shufflePalaceEnemiesCheckbox.Name = ("shufflePalaceEnemiesCheckbox");
        this.shufflePalaceEnemiesCheckbox.Size = (new global::System.Drawing.Size(147, 19));
        this.shufflePalaceEnemiesCheckbox.TabIndex = (8);
        this.shufflePalaceEnemiesCheckbox.Text = ("Shuffle Palace Enemies");
        this.shufflePalaceEnemiesCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shufflePalaceEnemiesCheckbox, "Shuffles enemies in the palaces");
        this.shufflePalaceEnemiesCheckbox.UseVisualStyleBackColor = (true);
        this.shufflePalaceEnemiesCheckbox.CheckStateChanged += (this.ShuffleEnemies_CheckStateChanged);
        // 
        // shuffleOverworldEnemiesCheckbox
        // 
        this.shuffleOverworldEnemiesCheckbox.AutoSize = (true);
        this.shuffleOverworldEnemiesCheckbox.Location = (new global::System.Drawing.Point(4, 6));
        this.shuffleOverworldEnemiesCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleOverworldEnemiesCheckbox.Name = ("shuffleOverworldEnemiesCheckbox");
        this.shuffleOverworldEnemiesCheckbox.Size = (new global::System.Drawing.Size(168, 19));
        this.shuffleOverworldEnemiesCheckbox.TabIndex = (7);
        this.shuffleOverworldEnemiesCheckbox.Text = ("Shuffle Overworld Enemies");
        this.shuffleOverworldEnemiesCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shuffleOverworldEnemiesCheckbox, "Shuffles enemies on the overworld");
        this.shuffleOverworldEnemiesCheckbox.UseVisualStyleBackColor = (true);
        this.shuffleOverworldEnemiesCheckbox.CheckStateChanged += (this.ShuffleEnemies_CheckStateChanged);
        // 
        // shuffleSwordImmunityBox
        // 
        this.shuffleSwordImmunityBox.AutoSize = (true);
        this.shuffleSwordImmunityBox.Location = (new global::System.Drawing.Point(226, 85));
        this.shuffleSwordImmunityBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleSwordImmunityBox.Name = ("shuffleSwordImmunityBox");
        this.shuffleSwordImmunityBox.Size = (new global::System.Drawing.Size(154, 19));
        this.shuffleSwordImmunityBox.TabIndex = (4);
        this.shuffleSwordImmunityBox.Text = ("Shuffle Sword Immunity");
        this.toolTip1.SetToolTip(this.shuffleSwordImmunityBox, "Shuffle which enemies require fire to kill");
        this.shuffleSwordImmunityBox.UseVisualStyleBackColor = (true);
        // 
        // shuffleStealXPAmountCheckbox
        // 
        this.shuffleStealXPAmountCheckbox.AutoSize = (true);
        this.shuffleStealXPAmountCheckbox.Location = (new global::System.Drawing.Point(226, 59));
        this.shuffleStealXPAmountCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleStealXPAmountCheckbox.Name = ("shuffleStealXPAmountCheckbox");
        this.shuffleStealXPAmountCheckbox.Size = (new global::System.Drawing.Size(182, 19));
        this.shuffleStealXPAmountCheckbox.TabIndex = (3);
        this.shuffleStealXPAmountCheckbox.Text = ("Shuffle Amount of Exp Stolen");
        this.toolTip1.SetToolTip(this.shuffleStealXPAmountCheckbox, "Shuffle how much experience is stolen from the player when taking damage from certain enemies");
        this.shuffleStealXPAmountCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleXPStealersCheckbox
        // 
        this.shuffleXPStealersCheckbox.AutoSize = (true);
        this.shuffleXPStealersCheckbox.Location = (new global::System.Drawing.Point(226, 32));
        this.shuffleXPStealersCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleXPStealersCheckbox.Name = ("shuffleXPStealersCheckbox");
        this.shuffleXPStealersCheckbox.Size = (new global::System.Drawing.Size(197, 19));
        this.shuffleXPStealersCheckbox.TabIndex = (2);
        this.shuffleXPStealersCheckbox.Text = ("Shuffle Which Enemies Steal Exp");
        this.toolTip1.SetToolTip(this.shuffleXPStealersCheckbox, "Shuffle which enemies steal experience when doing damage to the player");
        this.shuffleXPStealersCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleEnemyHPBox
        // 
        this.shuffleEnemyHPBox.AutoSize = (true);
        this.shuffleEnemyHPBox.Location = (new global::System.Drawing.Point(226, 6));
        this.shuffleEnemyHPBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleEnemyHPBox.Name = ("shuffleEnemyHPBox");
        this.shuffleEnemyHPBox.Size = (new global::System.Drawing.Size(121, 19));
        this.shuffleEnemyHPBox.TabIndex = (0);
        this.shuffleEnemyHPBox.Text = ("Shuffle Enemy HP");
        this.toolTip1.SetToolTip(this.shuffleEnemyHPBox, "Each enemy will have +/- 50% of its normal HP");
        this.shuffleEnemyHPBox.UseVisualStyleBackColor = (true);
        // 
        // tabPage7
        // 
        this.tabPage7.Controls.Add(this.shufflePbagAmountsCheckbox);
        this.tabPage7.Controls.Add(this.removeSpellitemsCheckbox);
        this.tabPage7.Controls.Add(this.includePbagCavesInShuffleCheckbox);
        this.tabPage7.Controls.Add(this.randomizeJarRequirementsCheckbox);
        this.tabPage7.Controls.Add(this.palacesHaveExtraKeysCheckbox);
        this.tabPage7.Controls.Add(this.shuffleSmallItemsCheckbox);
        this.tabPage7.Controls.Add(this.mixOverworldPalaceItemsCheckbox);
        this.tabPage7.Controls.Add(this.shuffleOverworldItemsCheckbox);
        this.tabPage7.Controls.Add(this.shufflePalaceItemsCheckbox);
        this.tabPage7.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage7.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage7.Name = ("tabPage7");
        this.tabPage7.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage7.TabIndex = (7);
        this.tabPage7.Text = ("Items");
        this.tabPage7.UseVisualStyleBackColor = (true);
        // 
        // shufflePbagAmountsCheckbox
        // 
        this.shufflePbagAmountsCheckbox.AutoSize = (true);
        this.shufflePbagAmountsCheckbox.Location = (new global::System.Drawing.Point(4, 215));
        this.shufflePbagAmountsCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.shufflePbagAmountsCheckbox.Name = ("shufflePbagAmountsCheckbox");
        this.shufflePbagAmountsCheckbox.Size = (new global::System.Drawing.Size(145, 19));
        this.shufflePbagAmountsCheckbox.TabIndex = (19);
        this.shufflePbagAmountsCheckbox.Text = ("Shuffle Pbag Amounts");
        this.shufflePbagAmountsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shufflePbagAmountsCheckbox, "If selected, the pbag amounts will be randomized.");
        this.shufflePbagAmountsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // removeSpellitemsCheckbox
        // 
        this.removeSpellitemsCheckbox.AutoSize = (true);
        this.removeSpellitemsCheckbox.Location = (new global::System.Drawing.Point(4, 188));
        this.removeSpellitemsCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.removeSpellitemsCheckbox.Name = ("removeSpellitemsCheckbox");
        this.removeSpellitemsCheckbox.Size = (new global::System.Drawing.Size(129, 19));
        this.removeSpellitemsCheckbox.TabIndex = (18);
        this.removeSpellitemsCheckbox.Text = ("Remove Spell Items");
        this.removeSpellitemsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.removeSpellitemsCheckbox, "When checked, you no longer need the trophy, medicine, or kid to access the respective spells");
        this.removeSpellitemsCheckbox.UseVisualStyleBackColor = (true);
        this.removeSpellitemsCheckbox.CheckStateChanged += (this.SpellItemBox_CheckStateChanged);
        // 
        // includePbagCavesInShuffleCheckbox
        // 
        this.includePbagCavesInShuffleCheckbox.AutoSize = (true);
        this.includePbagCavesInShuffleCheckbox.Location = (new global::System.Drawing.Point(4, 83));
        this.includePbagCavesInShuffleCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.includePbagCavesInShuffleCheckbox.Name = ("includePbagCavesInShuffleCheckbox");
        this.includePbagCavesInShuffleCheckbox.Size = (new global::System.Drawing.Size(209, 19));
        this.includePbagCavesInShuffleCheckbox.TabIndex = (17);
        this.includePbagCavesInShuffleCheckbox.Text = ("Include Pbag Caves in Item Shuffle");
        this.includePbagCavesInShuffleCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.includePbagCavesInShuffleCheckbox, "Will include the 3 pbag caves as item locations");
        this.includePbagCavesInShuffleCheckbox.UseVisualStyleBackColor = (true);
        // 
        // randomizeJarRequirementsCheckbox
        // 
        this.randomizeJarRequirementsCheckbox.AutoSize = (true);
        this.randomizeJarRequirementsCheckbox.Location = (new global::System.Drawing.Point(4, 163));
        this.randomizeJarRequirementsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.randomizeJarRequirementsCheckbox.Name = ("randomizeJarRequirementsCheckbox");
        this.randomizeJarRequirementsCheckbox.Size = (new global::System.Drawing.Size(244, 19));
        this.randomizeJarRequirementsCheckbox.TabIndex = (16);
        this.randomizeJarRequirementsCheckbox.Text = ("Randomize New Kasuto Jar Requirements");
        this.toolTip1.SetToolTip(this.randomizeJarRequirementsCheckbox, "When selected, the number of jars required to get the item in New Kasuto will be randomized between 5 and 7.");
        this.randomizeJarRequirementsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // palacesHaveExtraKeysCheckbox
        // 
        this.palacesHaveExtraKeysCheckbox.AutoSize = (true);
        this.palacesHaveExtraKeysCheckbox.Location = (new global::System.Drawing.Point(4, 136));
        this.palacesHaveExtraKeysCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.palacesHaveExtraKeysCheckbox.Name = ("palacesHaveExtraKeysCheckbox");
        this.palacesHaveExtraKeysCheckbox.Size = (new global::System.Drawing.Size(166, 19));
        this.palacesHaveExtraKeysCheckbox.TabIndex = (4);
        this.palacesHaveExtraKeysCheckbox.Text = ("Palaces Contain Extra Keys");
        this.palacesHaveExtraKeysCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.palacesHaveExtraKeysCheckbox, "Inserts a lot of extra keys into the palaces");
        this.palacesHaveExtraKeysCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleSmallItemsCheckbox
        // 
        this.shuffleSmallItemsCheckbox.AutoSize = (true);
        this.shuffleSmallItemsCheckbox.Location = (new global::System.Drawing.Point(4, 110));
        this.shuffleSmallItemsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleSmallItemsCheckbox.Name = ("shuffleSmallItemsCheckbox");
        this.shuffleSmallItemsCheckbox.Size = (new global::System.Drawing.Size(127, 19));
        this.shuffleSmallItemsCheckbox.TabIndex = (3);
        this.shuffleSmallItemsCheckbox.Text = ("Shuffle Small Items");
        this.toolTip1.SetToolTip(this.shuffleSmallItemsCheckbox, "Shuffles pbags, jars, and 1ups");
        this.shuffleSmallItemsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // mixOverworldPalaceItemsCheckbox
        // 
        this.mixOverworldPalaceItemsCheckbox.AutoSize = (true);
        this.mixOverworldPalaceItemsCheckbox.Location = (new global::System.Drawing.Point(4, 57));
        this.mixOverworldPalaceItemsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.mixOverworldPalaceItemsCheckbox.Name = ("mixOverworldPalaceItemsCheckbox");
        this.mixOverworldPalaceItemsCheckbox.Size = (new global::System.Drawing.Size(196, 19));
        this.mixOverworldPalaceItemsCheckbox.TabIndex = (2);
        this.mixOverworldPalaceItemsCheckbox.Text = ("Mix Overworld and Palace Items");
        this.mixOverworldPalaceItemsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.mixOverworldPalaceItemsCheckbox, "Allows palace items to be found in the overworld, and vice versa");
        this.mixOverworldPalaceItemsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleOverworldItemsCheckbox
        // 
        this.shuffleOverworldItemsCheckbox.AutoSize = (true);
        this.shuffleOverworldItemsCheckbox.Location = (new global::System.Drawing.Point(4, 30));
        this.shuffleOverworldItemsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleOverworldItemsCheckbox.Name = ("shuffleOverworldItemsCheckbox");
        this.shuffleOverworldItemsCheckbox.Size = (new global::System.Drawing.Size(153, 19));
        this.shuffleOverworldItemsCheckbox.TabIndex = (1);
        this.shuffleOverworldItemsCheckbox.Text = ("Shuffle Overworld Items");
        this.shuffleOverworldItemsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shuffleOverworldItemsCheckbox, "Shuffles the items that are found in the overworld");
        this.shuffleOverworldItemsCheckbox.UseVisualStyleBackColor = (true);
        this.shuffleOverworldItemsCheckbox.CheckStateChanged += (this.OverworldItemBox_CheckStateChanged);
        // 
        // shufflePalaceItemsCheckbox
        // 
        this.shufflePalaceItemsCheckbox.AutoSize = (true);
        this.shufflePalaceItemsCheckbox.Location = (new global::System.Drawing.Point(4, 3));
        this.shufflePalaceItemsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shufflePalaceItemsCheckbox.Name = ("shufflePalaceItemsCheckbox");
        this.shufflePalaceItemsCheckbox.Size = (new global::System.Drawing.Size(132, 19));
        this.shufflePalaceItemsCheckbox.TabIndex = (0);
        this.shufflePalaceItemsCheckbox.Text = ("Shuffle Palace Items");
        this.shufflePalaceItemsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.shufflePalaceItemsCheckbox, "Shuffles the items that are found in palaces");
        this.shufflePalaceItemsCheckbox.UseVisualStyleBackColor = (true);
        this.shufflePalaceItemsCheckbox.CheckStateChanged += (this.PalaceItemBox_CheckStateChanged);
        // 
        // tabPage8
        // 
        this.tabPage8.Controls.Add(this.randomizeDropsCheckbox);
        this.tabPage8.Controls.Add(this.standardizeDropsCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesKeyCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemies1UpCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesXLBagCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesLargeBagCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesMediumBagCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesSmallBagCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesRedJarCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemiesBlueJarCheckbox);
        this.tabPage8.Controls.Add(this.largeEnemyPoolLabel);
        this.tabPage8.Controls.Add(this.smallEnemiesKeyCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemies1UpCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemiesXLBagCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemiesLargeBagCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemiesMediumBagCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemiesSmallBagCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemiesRedJarCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemiesBlueJarCheckbox);
        this.tabPage8.Controls.Add(this.smallEnemyPoolLabel);
        this.tabPage8.Controls.Add(this.label19);
        this.tabPage8.Controls.Add(this.shuffleDropFrequencyCheckbox);
        this.tabPage8.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage8.Margin = (new global::System.Windows.Forms.Padding(2));
        this.tabPage8.Name = ("tabPage8");
        this.tabPage8.Padding = (new global::System.Windows.Forms.Padding(2));
        this.tabPage8.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage8.TabIndex = (8);
        this.tabPage8.Text = ("Drops");
        this.tabPage8.UseVisualStyleBackColor = (true);
        // 
        // randomizeDropsCheckbox
        // 
        this.randomizeDropsCheckbox.AutoSize = (true);
        this.randomizeDropsCheckbox.Location = (new global::System.Drawing.Point(9, 36));
        this.randomizeDropsCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.randomizeDropsCheckbox.Name = ("randomizeDropsCheckbox");
        this.randomizeDropsCheckbox.Size = (new global::System.Drawing.Size(119, 19));
        this.randomizeDropsCheckbox.TabIndex = (45);
        this.randomizeDropsCheckbox.Text = ("Randomize Drops");
        this.toolTip1.SetToolTip(this.randomizeDropsCheckbox, "When selected, the items in the drop pool will be randomized");
        this.randomizeDropsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // standardizeDropsCheckbox
        // 
        this.standardizeDropsCheckbox.AutoSize = (true);
        this.standardizeDropsCheckbox.Location = (new global::System.Drawing.Point(9, 60));
        this.standardizeDropsCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.standardizeDropsCheckbox.Name = ("standardizeDropsCheckbox");
        this.standardizeDropsCheckbox.Size = (new global::System.Drawing.Size(121, 19));
        this.standardizeDropsCheckbox.TabIndex = (44);
        this.standardizeDropsCheckbox.Text = ("Standardize Drops");
        this.toolTip1.SetToolTip(this.standardizeDropsCheckbox, "When selected, all runners playing the same seed will get the same drops in the same order");
        this.standardizeDropsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesKeyCheckbox
        // 
        this.largeEnemiesKeyCheckbox.AutoSize = (true);
        this.largeEnemiesKeyCheckbox.Location = (new global::System.Drawing.Point(404, 204));
        this.largeEnemiesKeyCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesKeyCheckbox.Name = ("largeEnemiesKeyCheckbox");
        this.largeEnemiesKeyCheckbox.Size = (new global::System.Drawing.Size(45, 19));
        this.largeEnemiesKeyCheckbox.TabIndex = (43);
        this.largeEnemiesKeyCheckbox.Text = ("Key");
        this.toolTip1.SetToolTip(this.largeEnemiesKeyCheckbox, "Add small keys to the large enemy pool");
        this.largeEnemiesKeyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemies1UpCheckbox
        // 
        this.largeEnemies1UpCheckbox.AutoSize = (true);
        this.largeEnemies1UpCheckbox.Location = (new global::System.Drawing.Point(404, 180));
        this.largeEnemies1UpCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemies1UpCheckbox.Name = ("largeEnemies1UpCheckbox");
        this.largeEnemies1UpCheckbox.Size = (new global::System.Drawing.Size(46, 19));
        this.largeEnemies1UpCheckbox.TabIndex = (42);
        this.largeEnemies1UpCheckbox.Text = ("1up");
        this.toolTip1.SetToolTip(this.largeEnemies1UpCheckbox, "Add 1ups to the large enemy pool");
        this.largeEnemies1UpCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesXLBagCheckbox
        // 
        this.largeEnemiesXLBagCheckbox.AutoSize = (true);
        this.largeEnemiesXLBagCheckbox.Location = (new global::System.Drawing.Point(404, 156));
        this.largeEnemiesXLBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesXLBagCheckbox.Name = ("largeEnemiesXLBagCheckbox");
        this.largeEnemiesXLBagCheckbox.Size = (new global::System.Drawing.Size(74, 19));
        this.largeEnemiesXLBagCheckbox.TabIndex = (41);
        this.largeEnemiesXLBagCheckbox.Text = ("500 pbag");
        this.toolTip1.SetToolTip(this.largeEnemiesXLBagCheckbox, "Add 500 bags to the large enemy pool");
        this.largeEnemiesXLBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesLargeBagCheckbox
        // 
        this.largeEnemiesLargeBagCheckbox.AutoSize = (true);
        this.largeEnemiesLargeBagCheckbox.Checked = (true);
        this.largeEnemiesLargeBagCheckbox.CheckState = (global::System.Windows.Forms.CheckState.Checked);
        this.largeEnemiesLargeBagCheckbox.Location = (new global::System.Drawing.Point(404, 132));
        this.largeEnemiesLargeBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesLargeBagCheckbox.Name = ("largeEnemiesLargeBagCheckbox");
        this.largeEnemiesLargeBagCheckbox.Size = (new global::System.Drawing.Size(74, 19));
        this.largeEnemiesLargeBagCheckbox.TabIndex = (40);
        this.largeEnemiesLargeBagCheckbox.Text = ("200 pbag");
        this.toolTip1.SetToolTip(this.largeEnemiesLargeBagCheckbox, "Add 200 bags to the large enemy pool");
        this.largeEnemiesLargeBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesMediumBagCheckbox
        // 
        this.largeEnemiesMediumBagCheckbox.AutoSize = (true);
        this.largeEnemiesMediumBagCheckbox.Location = (new global::System.Drawing.Point(404, 108));
        this.largeEnemiesMediumBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesMediumBagCheckbox.Name = ("largeEnemiesMediumBagCheckbox");
        this.largeEnemiesMediumBagCheckbox.Size = (new global::System.Drawing.Size(74, 19));
        this.largeEnemiesMediumBagCheckbox.TabIndex = (39);
        this.largeEnemiesMediumBagCheckbox.Text = ("100 pbag");
        this.toolTip1.SetToolTip(this.largeEnemiesMediumBagCheckbox, "Add 100 bags to the large enemy pool");
        this.largeEnemiesMediumBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesSmallBagCheckbox
        // 
        this.largeEnemiesSmallBagCheckbox.AutoSize = (true);
        this.largeEnemiesSmallBagCheckbox.Location = (new global::System.Drawing.Point(404, 84));
        this.largeEnemiesSmallBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesSmallBagCheckbox.Name = ("largeEnemiesSmallBagCheckbox");
        this.largeEnemiesSmallBagCheckbox.Size = (new global::System.Drawing.Size(68, 19));
        this.largeEnemiesSmallBagCheckbox.TabIndex = (38);
        this.largeEnemiesSmallBagCheckbox.Text = ("50 pbag");
        this.toolTip1.SetToolTip(this.largeEnemiesSmallBagCheckbox, "Add 50 bags to the large enemy pool");
        this.largeEnemiesSmallBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesRedJarCheckbox
        // 
        this.largeEnemiesRedJarCheckbox.AutoSize = (true);
        this.largeEnemiesRedJarCheckbox.Checked = (true);
        this.largeEnemiesRedJarCheckbox.CheckState = (global::System.Windows.Forms.CheckState.Checked);
        this.largeEnemiesRedJarCheckbox.Location = (new global::System.Drawing.Point(404, 60));
        this.largeEnemiesRedJarCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesRedJarCheckbox.Name = ("largeEnemiesRedJarCheckbox");
        this.largeEnemiesRedJarCheckbox.Size = (new global::System.Drawing.Size(63, 19));
        this.largeEnemiesRedJarCheckbox.TabIndex = (37);
        this.largeEnemiesRedJarCheckbox.Text = ("Red Jar");
        this.toolTip1.SetToolTip(this.largeEnemiesRedJarCheckbox, "Add red jars to the large enemy pool");
        this.largeEnemiesRedJarCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemiesBlueJarCheckbox
        // 
        this.largeEnemiesBlueJarCheckbox.AutoSize = (true);
        this.largeEnemiesBlueJarCheckbox.Location = (new global::System.Drawing.Point(404, 36));
        this.largeEnemiesBlueJarCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.largeEnemiesBlueJarCheckbox.Name = ("largeEnemiesBlueJarCheckbox");
        this.largeEnemiesBlueJarCheckbox.Size = (new global::System.Drawing.Size(66, 19));
        this.largeEnemiesBlueJarCheckbox.TabIndex = (36);
        this.largeEnemiesBlueJarCheckbox.Text = ("Blue Jar");
        this.toolTip1.SetToolTip(this.largeEnemiesBlueJarCheckbox, "Add blue jars to the large enemy pool");
        this.largeEnemiesBlueJarCheckbox.UseVisualStyleBackColor = (true);
        // 
        // largeEnemyPoolLabel
        // 
        this.largeEnemyPoolLabel.AutoSize = (true);
        this.largeEnemyPoolLabel.Location = (new global::System.Drawing.Point(392, 12));
        this.largeEnemyPoolLabel.Margin = (new global::System.Windows.Forms.Padding(2, 0, 2, 0));
        this.largeEnemyPoolLabel.Name = ("largeEnemyPoolLabel");
        this.largeEnemyPoolLabel.Size = (new global::System.Drawing.Size(102, 15));
        this.largeEnemyPoolLabel.TabIndex = (35);
        this.largeEnemyPoolLabel.Text = ("Large Enemy Pool");
        // 
        // smallEnemiesKeyCheckbox
        // 
        this.smallEnemiesKeyCheckbox.AutoSize = (true);
        this.smallEnemiesKeyCheckbox.Location = (new global::System.Drawing.Point(254, 204));
        this.smallEnemiesKeyCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesKeyCheckbox.Name = ("smallEnemiesKeyCheckbox");
        this.smallEnemiesKeyCheckbox.Size = (new global::System.Drawing.Size(45, 19));
        this.smallEnemiesKeyCheckbox.TabIndex = (34);
        this.smallEnemiesKeyCheckbox.Text = ("Key");
        this.toolTip1.SetToolTip(this.smallEnemiesKeyCheckbox, "Add small keys to the small enemy pool");
        this.smallEnemiesKeyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemies1UpCheckbox
        // 
        this.smallEnemies1UpCheckbox.AutoSize = (true);
        this.smallEnemies1UpCheckbox.Location = (new global::System.Drawing.Point(255, 180));
        this.smallEnemies1UpCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemies1UpCheckbox.Name = ("smallEnemies1UpCheckbox");
        this.smallEnemies1UpCheckbox.Size = (new global::System.Drawing.Size(46, 19));
        this.smallEnemies1UpCheckbox.TabIndex = (33);
        this.smallEnemies1UpCheckbox.Text = ("1up");
        this.toolTip1.SetToolTip(this.smallEnemies1UpCheckbox, "Add 1ups to the small enemy pool");
        this.smallEnemies1UpCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemiesXLBagCheckbox
        // 
        this.smallEnemiesXLBagCheckbox.AutoSize = (true);
        this.smallEnemiesXLBagCheckbox.Location = (new global::System.Drawing.Point(255, 156));
        this.smallEnemiesXLBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesXLBagCheckbox.Name = ("smallEnemiesXLBagCheckbox");
        this.smallEnemiesXLBagCheckbox.Size = (new global::System.Drawing.Size(74, 19));
        this.smallEnemiesXLBagCheckbox.TabIndex = (32);
        this.smallEnemiesXLBagCheckbox.Text = ("500 pbag");
        this.toolTip1.SetToolTip(this.smallEnemiesXLBagCheckbox, "Add 500 bags to the small enemy pool");
        this.smallEnemiesXLBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemiesLargeBagCheckbox
        // 
        this.smallEnemiesLargeBagCheckbox.AutoSize = (true);
        this.smallEnemiesLargeBagCheckbox.Location = (new global::System.Drawing.Point(255, 132));
        this.smallEnemiesLargeBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesLargeBagCheckbox.Name = ("smallEnemiesLargeBagCheckbox");
        this.smallEnemiesLargeBagCheckbox.Size = (new global::System.Drawing.Size(74, 19));
        this.smallEnemiesLargeBagCheckbox.TabIndex = (31);
        this.smallEnemiesLargeBagCheckbox.Text = ("200 pbag");
        this.toolTip1.SetToolTip(this.smallEnemiesLargeBagCheckbox, "Add 200 bags to the small enemy pool");
        this.smallEnemiesLargeBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemiesMediumBagCheckbox
        // 
        this.smallEnemiesMediumBagCheckbox.AutoSize = (true);
        this.smallEnemiesMediumBagCheckbox.Location = (new global::System.Drawing.Point(255, 108));
        this.smallEnemiesMediumBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesMediumBagCheckbox.Name = ("smallEnemiesMediumBagCheckbox");
        this.smallEnemiesMediumBagCheckbox.Size = (new global::System.Drawing.Size(74, 19));
        this.smallEnemiesMediumBagCheckbox.TabIndex = (30);
        this.smallEnemiesMediumBagCheckbox.Text = ("100 pbag");
        this.toolTip1.SetToolTip(this.smallEnemiesMediumBagCheckbox, "Add 100 bags to the small enemy pool");
        this.smallEnemiesMediumBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemiesSmallBagCheckbox
        // 
        this.smallEnemiesSmallBagCheckbox.AutoSize = (true);
        this.smallEnemiesSmallBagCheckbox.Checked = (true);
        this.smallEnemiesSmallBagCheckbox.CheckState = (global::System.Windows.Forms.CheckState.Checked);
        this.smallEnemiesSmallBagCheckbox.Location = (new global::System.Drawing.Point(255, 84));
        this.smallEnemiesSmallBagCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesSmallBagCheckbox.Name = ("smallEnemiesSmallBagCheckbox");
        this.smallEnemiesSmallBagCheckbox.Size = (new global::System.Drawing.Size(68, 19));
        this.smallEnemiesSmallBagCheckbox.TabIndex = (29);
        this.smallEnemiesSmallBagCheckbox.Text = ("50 pbag");
        this.toolTip1.SetToolTip(this.smallEnemiesSmallBagCheckbox, "Add 50 bags to the small enemy pool");
        this.smallEnemiesSmallBagCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemiesRedJarCheckbox
        // 
        this.smallEnemiesRedJarCheckbox.AutoSize = (true);
        this.smallEnemiesRedJarCheckbox.Location = (new global::System.Drawing.Point(255, 60));
        this.smallEnemiesRedJarCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesRedJarCheckbox.Name = ("smallEnemiesRedJarCheckbox");
        this.smallEnemiesRedJarCheckbox.Size = (new global::System.Drawing.Size(63, 19));
        this.smallEnemiesRedJarCheckbox.TabIndex = (28);
        this.smallEnemiesRedJarCheckbox.Text = ("Red Jar");
        this.toolTip1.SetToolTip(this.smallEnemiesRedJarCheckbox, "Add red jars to the small enemy pool");
        this.smallEnemiesRedJarCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemiesBlueJarCheckbox
        // 
        this.smallEnemiesBlueJarCheckbox.AutoSize = (true);
        this.smallEnemiesBlueJarCheckbox.Checked = (true);
        this.smallEnemiesBlueJarCheckbox.CheckState = (global::System.Windows.Forms.CheckState.Checked);
        this.smallEnemiesBlueJarCheckbox.Location = (new global::System.Drawing.Point(255, 36));
        this.smallEnemiesBlueJarCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.smallEnemiesBlueJarCheckbox.Name = ("smallEnemiesBlueJarCheckbox");
        this.smallEnemiesBlueJarCheckbox.Size = (new global::System.Drawing.Size(66, 19));
        this.smallEnemiesBlueJarCheckbox.TabIndex = (27);
        this.smallEnemiesBlueJarCheckbox.Text = ("Blue Jar");
        this.toolTip1.SetToolTip(this.smallEnemiesBlueJarCheckbox, "Add blue jars to the small enemy pool");
        this.smallEnemiesBlueJarCheckbox.UseVisualStyleBackColor = (true);
        // 
        // smallEnemyPoolLabel
        // 
        this.smallEnemyPoolLabel.AutoSize = (true);
        this.smallEnemyPoolLabel.Location = (new global::System.Drawing.Point(243, 12));
        this.smallEnemyPoolLabel.Margin = (new global::System.Windows.Forms.Padding(2, 0, 2, 0));
        this.smallEnemyPoolLabel.Name = ("smallEnemyPoolLabel");
        this.smallEnemyPoolLabel.Size = (new global::System.Drawing.Size(102, 15));
        this.smallEnemyPoolLabel.TabIndex = (25);
        this.smallEnemyPoolLabel.Text = ("Small Enemy Pool");
        // 
        // label19
        // 
        this.label19.BorderStyle = (global::System.Windows.Forms.BorderStyle.Fixed3D);
        this.label19.Location = (new global::System.Drawing.Point(209, 12));
        this.label19.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.label19.Name = ("label19");
        this.label19.Size = (new global::System.Drawing.Size(2, 225));
        this.label19.TabIndex = (24);
        // 
        // shuffleDropFrequencyCheckbox
        // 
        this.shuffleDropFrequencyCheckbox.AutoSize = (true);
        this.shuffleDropFrequencyCheckbox.Location = (new global::System.Drawing.Point(9, 12));
        this.shuffleDropFrequencyCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleDropFrequencyCheckbox.Name = ("shuffleDropFrequencyCheckbox");
        this.shuffleDropFrequencyCheckbox.Size = (new global::System.Drawing.Size(177, 19));
        this.shuffleDropFrequencyCheckbox.TabIndex = (22);
        this.shuffleDropFrequencyCheckbox.Text = ("Shuffle Item Drop Frequency");
        this.toolTip1.SetToolTip(this.shuffleDropFrequencyCheckbox, "This option will shuffle how often enemies drop pbags and jars");
        this.shuffleDropFrequencyCheckbox.UseVisualStyleBackColor = (true);
        // 
        // tabPage10
        // 
        this.tabPage10.Controls.Add(this.enableTownNameHintsCheckbox);
        this.tabPage10.Controls.Add(this.enableSpellItemHintsCheckbox);
        this.tabPage10.Controls.Add(this.useCommunityHintsCheckbox);
        this.tabPage10.Controls.Add(this.enableHelpfulHintsCheckbox);
        this.tabPage10.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage10.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage10.Name = ("tabPage10");
        this.tabPage10.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage10.TabIndex = (10);
        this.tabPage10.Text = ("Hints");
        this.tabPage10.UseVisualStyleBackColor = (true);
        // 
        // enableTownNameHintsCheckbox
        // 
        this.enableTownNameHintsCheckbox.AutoSize = (true);
        this.enableTownNameHintsCheckbox.Location = (new global::System.Drawing.Point(4, 53));
        this.enableTownNameHintsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.enableTownNameHintsCheckbox.Name = ("enableTownNameHintsCheckbox");
        this.enableTownNameHintsCheckbox.Size = (new global::System.Drawing.Size(158, 19));
        this.enableTownNameHintsCheckbox.TabIndex = (23);
        this.enableTownNameHintsCheckbox.Text = ("Enable Town Name Hints");
        this.enableTownNameHintsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.enableTownNameHintsCheckbox, "Signs at the beginning of town will tell you what spell is contained in the town.");
        this.enableTownNameHintsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // enableSpellItemHintsCheckbox
        // 
        this.enableSpellItemHintsCheckbox.AutoSize = (true);
        this.enableSpellItemHintsCheckbox.Location = (new global::System.Drawing.Point(4, 29));
        this.enableSpellItemHintsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.enableSpellItemHintsCheckbox.Name = ("enableSpellItemHintsCheckbox");
        this.enableSpellItemHintsCheckbox.Size = (new global::System.Drawing.Size(147, 19));
        this.enableSpellItemHintsCheckbox.TabIndex = (22);
        this.enableSpellItemHintsCheckbox.Text = ("Enable Spell Item Hints");
        this.enableSpellItemHintsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.enableSpellItemHintsCheckbox, "The people who require spell items will tell you where the item can be found.");
        this.enableSpellItemHintsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // useCommunityHintsCheckbox
        // 
        this.useCommunityHintsCheckbox.AutoSize = (true);
        this.useCommunityHintsCheckbox.Location = (new global::System.Drawing.Point(4, 78));
        this.useCommunityHintsCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.useCommunityHintsCheckbox.Name = ("useCommunityHintsCheckbox");
        this.useCommunityHintsCheckbox.Size = (new global::System.Drawing.Size(121, 19));
        this.useCommunityHintsCheckbox.TabIndex = (21);
        this.useCommunityHintsCheckbox.Text = ("Community Hints");
        this.toolTip1.SetToolTip(this.useCommunityHintsCheckbox, "When selected, will replace some text with hints submitted by the community");
        this.useCommunityHintsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // enableHelpfulHintsCheckbox
        // 
        this.enableHelpfulHintsCheckbox.AutoSize = (true);
        this.enableHelpfulHintsCheckbox.Location = (new global::System.Drawing.Point(4, 3));
        this.enableHelpfulHintsCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.enableHelpfulHintsCheckbox.Name = ("enableHelpfulHintsCheckbox");
        this.enableHelpfulHintsCheckbox.Size = (new global::System.Drawing.Size(134, 19));
        this.enableHelpfulHintsCheckbox.TabIndex = (0);
        this.enableHelpfulHintsCheckbox.Text = ("Enable Helpful Hints");
        this.enableHelpfulHintsCheckbox.ThreeState = (true);
        this.toolTip1.SetToolTip(this.enableHelpfulHintsCheckbox, "Townspeople will give you helpful hints as to where items are located.");
        this.enableHelpfulHintsCheckbox.UseVisualStyleBackColor = (true);
        // 
        // tabPage3
        // 
        this.tabPage3.Controls.Add(this.useCustomRoomsBox);
        this.tabPage3.Controls.Add(this.dashAlwaysOnCheckbox);
        this.tabPage3.Controls.Add(this.flashingOffCheckbox);
        this.tabPage3.Controls.Add(this.upAOnController1Checkbox);
        this.tabPage3.Controls.Add(this.beamSpriteList);
        this.tabPage3.Controls.Add(this.beamSpriteLabel);
        this.tabPage3.Controls.Add(this.shieldColorList);
        this.tabPage3.Controls.Add(this.shieldColorLabel);
        this.tabPage3.Controls.Add(this.tunicColorList);
        this.tabPage3.Controls.Add(this.tunicColorLabel);
        this.tabPage3.Controls.Add(this.characterSpriteList);
        this.tabPage3.Controls.Add(this.characterSpriteLabel);
        this.tabPage3.Controls.Add(this.disableMusicCheckbox);
        this.tabPage3.Controls.Add(this.shuffleEnemyPalettesCheckbox);
        this.tabPage3.Controls.Add(this.alwaysBeamCheckbox);
        this.tabPage3.Controls.Add(this.fastSpellCheckbox);
        this.tabPage3.Controls.Add(this.jumpAlwaysOnCheckbox);
        this.tabPage3.Controls.Add(this.disableLowHealthBeepCheckbox);
        this.tabPage3.Location = (new global::System.Drawing.Point(4, 24));
        this.tabPage3.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.tabPage3.Name = ("tabPage3");
        this.tabPage3.Size = (new global::System.Drawing.Size(595, 331));
        this.tabPage3.TabIndex = (6);
        this.tabPage3.Text = ("Misc.");
        this.tabPage3.UseVisualStyleBackColor = (true);
        // 
        // useCustomRoomsBox
        // 
        this.useCustomRoomsBox.AutoSize = (true);
        this.useCustomRoomsBox.Location = (new global::System.Drawing.Point(4, 126));
        this.useCustomRoomsBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.useCustomRoomsBox.Name = ("useCustomRoomsBox");
        this.useCustomRoomsBox.Size = (new global::System.Drawing.Size(130, 19));
        this.useCustomRoomsBox.TabIndex = (35);
        this.useCustomRoomsBox.Text = ("Use Custom Rooms");
        this.toolTip1.SetToolTip(this.useCustomRoomsBox, "When checked, Use CustomRooms.json to create you own room set.");
        this.useCustomRoomsBox.UseVisualStyleBackColor = (true);
        // 
        // dashAlwaysOnCheckbox
        // 
        this.dashAlwaysOnCheckbox.AutoSize = (true);
        this.dashAlwaysOnCheckbox.Location = (new global::System.Drawing.Point(4, 222));
        this.dashAlwaysOnCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.dashAlwaysOnCheckbox.Name = ("dashAlwaysOnCheckbox");
        this.dashAlwaysOnCheckbox.Size = (new global::System.Drawing.Size(111, 19));
        this.dashAlwaysOnCheckbox.TabIndex = (34);
        this.dashAlwaysOnCheckbox.Text = ("Dash Always On");
        this.toolTip1.SetToolTip(this.dashAlwaysOnCheckbox, "The player will always move as though Dash spell were on.,");
        this.dashAlwaysOnCheckbox.UseVisualStyleBackColor = (true);
        // 
        // flashingOffCheckbox
        // 
        this.flashingOffCheckbox.AutoSize = (true);
        this.flashingOffCheckbox.Checked = (true);
        this.flashingOffCheckbox.CheckState = (global::System.Windows.Forms.CheckState.Checked);
        this.flashingOffCheckbox.Location = (new global::System.Drawing.Point(4, 101));
        this.flashingOffCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.flashingOffCheckbox.Name = ("flashingOffCheckbox");
        this.flashingOffCheckbox.Size = (new global::System.Drawing.Size(182, 19));
        this.flashingOffCheckbox.TabIndex = (33);
        this.flashingOffCheckbox.Text = ("Remove Flashing Upon Death");
        this.toolTip1.SetToolTip(this.flashingOffCheckbox, "When selected, the flashing animation after Link's death will be removed.");
        this.flashingOffCheckbox.UseVisualStyleBackColor = (true);
        // 
        // upAOnController1Checkbox
        // 
        this.upAOnController1Checkbox.AutoSize = (true);
        this.upAOnController1Checkbox.Location = (new global::System.Drawing.Point(4, 76));
        this.upAOnController1Checkbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.upAOnController1Checkbox.Name = ("upAOnController1Checkbox");
        this.upAOnController1Checkbox.Size = (new global::System.Drawing.Size(250, 19));
        this.upAOnController1Checkbox.TabIndex = (32);
        this.upAOnController1Checkbox.Text = ("Remap Up+A to Up+Select on Controller 1");
        this.toolTip1.SetToolTip(this.upAOnController1Checkbox, "When selected, Up+A on controller 2 will be remapped to Up+Select on Controller 1");
        this.upAOnController1Checkbox.UseVisualStyleBackColor = (true);
        // 
        // beamSpriteList
        // 
        this.beamSpriteList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.beamSpriteList.FormattingEnabled = (true);
        this.beamSpriteList.Items.AddRange(new global::System.Object[] { "Default", "Fire", "Bubble", "Rock", "Axe", "Hammer", "Wizzrobe Beam", "Random" });
        this.beamSpriteList.Location = (new global::System.Drawing.Point(298, 193));
        this.beamSpriteList.Margin = (new global::System.Windows.Forms.Padding(2));
        this.beamSpriteList.Name = ("beamSpriteList");
        this.beamSpriteList.Size = (new global::System.Drawing.Size(142, 23));
        this.beamSpriteList.TabIndex = (31);
        this.toolTip1.SetToolTip(this.beamSpriteList, "Allows you to select what the beam sprite will be");
        // 
        // beamSpriteLabel
        // 
        this.beamSpriteLabel.AutoSize = (true);
        this.beamSpriteLabel.Location = (new global::System.Drawing.Point(294, 170));
        this.beamSpriteLabel.Margin = (new global::System.Windows.Forms.Padding(2, 0, 2, 0));
        this.beamSpriteLabel.Name = ("beamSpriteLabel");
        this.beamSpriteLabel.Size = (new global::System.Drawing.Size(73, 15));
        this.beamSpriteLabel.TabIndex = (30);
        this.beamSpriteLabel.Text = ("Beam Sprite:");
        // 
        // shieldColorList
        // 
        this.shieldColorList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.shieldColorList.FormattingEnabled = (true);
        this.shieldColorList.Items.AddRange(new global::System.Object[] { "Default", "Green", "Dark Green", "Aqua", "Dark Blue", "Purple", "Pink", "Orange", "Red", "Turd", "Random" });
        this.shieldColorList.Location = (new global::System.Drawing.Point(298, 136));
        this.shieldColorList.Margin = (new global::System.Windows.Forms.Padding(2));
        this.shieldColorList.Name = ("shieldColorList");
        this.shieldColorList.Size = (new global::System.Drawing.Size(142, 23));
        this.shieldColorList.TabIndex = (29);
        this.toolTip1.SetToolTip(this.shieldColorList, "Changes the tunic color for shield");
        // 
        // shieldColorLabel
        // 
        this.shieldColorLabel.AutoSize = (true);
        this.shieldColorLabel.Location = (new global::System.Drawing.Point(294, 112));
        this.shieldColorLabel.Margin = (new global::System.Windows.Forms.Padding(2, 0, 2, 0));
        this.shieldColorLabel.Name = ("shieldColorLabel");
        this.shieldColorLabel.Size = (new global::System.Drawing.Size(106, 15));
        this.shieldColorLabel.TabIndex = (28);
        this.shieldColorLabel.Text = ("Shield Tunic Color:");
        // 
        // tunicColorList
        // 
        this.tunicColorList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.tunicColorList.FormattingEnabled = (true);
        this.tunicColorList.Items.AddRange(new global::System.Object[] { "Default", "Green", "Dark Green", "Aqua", "Dark Blue", "Purple", "Pink", "Orange", "Red", "Turd", "Random" });
        this.tunicColorList.Location = (new global::System.Drawing.Point(298, 80));
        this.tunicColorList.Margin = (new global::System.Windows.Forms.Padding(2));
        this.tunicColorList.Name = ("tunicColorList");
        this.tunicColorList.Size = (new global::System.Drawing.Size(142, 23));
        this.tunicColorList.TabIndex = (27);
        this.toolTip1.SetToolTip(this.tunicColorList, "Changes the normal tunic color");
        // 
        // tunicColorLabel
        // 
        this.tunicColorLabel.AutoSize = (true);
        this.tunicColorLabel.Location = (new global::System.Drawing.Point(294, 54));
        this.tunicColorLabel.Margin = (new global::System.Windows.Forms.Padding(2, 0, 2, 0));
        this.tunicColorLabel.Name = ("tunicColorLabel");
        this.tunicColorLabel.Size = (new global::System.Drawing.Size(114, 15));
        this.tunicColorLabel.TabIndex = (26);
        this.tunicColorLabel.Text = ("Normal Tunic Color:");
        // 
        // characterSpriteList
        // 
        this.characterSpriteList.DropDownStyle = (global::System.Windows.Forms.ComboBoxStyle.DropDownList);
        this.characterSpriteList.FormattingEnabled = (true);
        this.characterSpriteList.Items.AddRange(new global::System.Object[] { "Link", "Zelda", "Iron Knuckle", "Error", "Samus", "Simon", "Stalfos", "Vase Lady", "Ruto", "Yoshi", "Dragonlord", "Miria", "Crystalis", "Taco", "Pyramid", "Lady Link", "Hoodie Link", "GliitchWiitch", "Random" });
        this.characterSpriteList.Location = (new global::System.Drawing.Point(298, 24));
        this.characterSpriteList.Margin = (new global::System.Windows.Forms.Padding(2));
        this.characterSpriteList.Name = ("characterSpriteList");
        this.characterSpriteList.Size = (new global::System.Drawing.Size(142, 23));
        this.characterSpriteList.TabIndex = (25);
        this.toolTip1.SetToolTip(this.characterSpriteList, "Changes the playable character sprite");
        // 
        // characterSpriteLabel
        // 
        this.characterSpriteLabel.AutoSize = (true);
        this.characterSpriteLabel.Location = (new global::System.Drawing.Point(294, 3));
        this.characterSpriteLabel.Margin = (new global::System.Windows.Forms.Padding(2, 0, 2, 0));
        this.characterSpriteLabel.Name = ("characterSpriteLabel");
        this.characterSpriteLabel.Size = (new global::System.Drawing.Size(94, 15));
        this.characterSpriteLabel.TabIndex = (24);
        this.characterSpriteLabel.Text = ("Character Sprite:");
        // 
        // disableMusicCheckbox
        // 
        this.disableMusicCheckbox.AutoSize = (true);
        this.disableMusicCheckbox.Location = (new global::System.Drawing.Point(4, 27));
        this.disableMusicCheckbox.Margin = (new global::System.Windows.Forms.Padding(2));
        this.disableMusicCheckbox.Name = ("disableMusicCheckbox");
        this.disableMusicCheckbox.Size = (new global::System.Drawing.Size(99, 19));
        this.disableMusicCheckbox.TabIndex = (23);
        this.disableMusicCheckbox.Text = ("Disable Music");
        this.toolTip1.SetToolTip(this.disableMusicCheckbox, "Disables most in game music");
        this.disableMusicCheckbox.UseVisualStyleBackColor = (true);
        // 
        // shuffleEnemyPalettesCheckbox
        // 
        this.shuffleEnemyPalettesCheckbox.AutoSize = (true);
        this.shuffleEnemyPalettesCheckbox.Location = (new global::System.Drawing.Point(4, 172));
        this.shuffleEnemyPalettesCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.shuffleEnemyPalettesCheckbox.Name = ("shuffleEnemyPalettesCheckbox");
        this.shuffleEnemyPalettesCheckbox.Size = (new global::System.Drawing.Size(140, 19));
        this.shuffleEnemyPalettesCheckbox.TabIndex = (22);
        this.shuffleEnemyPalettesCheckbox.Text = ("Shuffle Sprite Palettes");
        this.toolTip1.SetToolTip(this.shuffleEnemyPalettesCheckbox, "When selected, sprite colors will be shuffled");
        this.shuffleEnemyPalettesCheckbox.UseVisualStyleBackColor = (true);
        // 
        // alwaysBeamCheckbox
        // 
        this.alwaysBeamCheckbox.AutoSize = (true);
        this.alwaysBeamCheckbox.Location = (new global::System.Drawing.Point(4, 197));
        this.alwaysBeamCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.alwaysBeamCheckbox.Name = ("alwaysBeamCheckbox");
        this.alwaysBeamCheckbox.Size = (new global::System.Drawing.Size(153, 19));
        this.alwaysBeamCheckbox.TabIndex = (19);
        this.alwaysBeamCheckbox.Text = ("Permanent Beam Sword");
        this.toolTip1.SetToolTip(this.alwaysBeamCheckbox, "Gives Link beam sword regardless of how much health he has");
        this.alwaysBeamCheckbox.UseVisualStyleBackColor = (true);
        // 
        // fastSpellCheckbox
        // 
        this.fastSpellCheckbox.AutoSize = (true);
        this.fastSpellCheckbox.Location = (new global::System.Drawing.Point(4, 51));
        this.fastSpellCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.fastSpellCheckbox.Name = ("fastSpellCheckbox");
        this.fastSpellCheckbox.Size = (new global::System.Drawing.Size(118, 19));
        this.fastSpellCheckbox.TabIndex = (4);
        this.fastSpellCheckbox.Text = ("Fast Spell Casting");
        this.toolTip1.SetToolTip(this.fastSpellCheckbox, "When checked, you do not have to open the pause menu before casting the selected spell");
        this.fastSpellCheckbox.UseVisualStyleBackColor = (true);
        // 
        // jumpAlwaysOnCheckbox
        // 
        this.jumpAlwaysOnCheckbox.AutoSize = (true);
        this.jumpAlwaysOnCheckbox.Location = (new global::System.Drawing.Point(4, 247));
        this.jumpAlwaysOnCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.jumpAlwaysOnCheckbox.Name = ("jumpAlwaysOnCheckbox");
        this.jumpAlwaysOnCheckbox.Size = (new global::System.Drawing.Size(114, 19));
        this.jumpAlwaysOnCheckbox.TabIndex = (3);
        this.jumpAlwaysOnCheckbox.Text = ("Jump Always On");
        this.toolTip1.SetToolTip(this.jumpAlwaysOnCheckbox, "The player will jump very high, as if the jump spell is always active");
        this.jumpAlwaysOnCheckbox.UseVisualStyleBackColor = (true);
        // 
        // disableLowHealthBeepCheckbox
        // 
        this.disableLowHealthBeepCheckbox.AutoSize = (true);
        this.disableLowHealthBeepCheckbox.Location = (new global::System.Drawing.Point(4, 3));
        this.disableLowHealthBeepCheckbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.disableLowHealthBeepCheckbox.Name = ("disableLowHealthBeepCheckbox");
        this.disableLowHealthBeepCheckbox.Size = (new global::System.Drawing.Size(156, 19));
        this.disableLowHealthBeepCheckbox.TabIndex = (0);
        this.disableLowHealthBeepCheckbox.Text = ("Disable Low Health Beep");
        this.toolTip1.SetToolTip(this.disableLowHealthBeepCheckbox, "Disables the beeping that happens when the player is low on health");
        this.disableLowHealthBeepCheckbox.UseVisualStyleBackColor = (true);
        // 
        // romFileTextBox
        // 
        this.romFileTextBox.Location = (new global::System.Drawing.Point(14, 30));
        this.romFileTextBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.romFileTextBox.Name = ("romFileTextBox");
        this.romFileTextBox.Size = (new global::System.Drawing.Size(156, 23));
        this.romFileTextBox.TabIndex = (1);
        this.toolTip1.SetToolTip(this.romFileTextBox, "Select a USA version of the Zelda 2 ROM");
        // 
        // seedTextBox
        // 
        this.seedTextBox.Location = (new global::System.Drawing.Point(13, 79));
        this.seedTextBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.seedTextBox.Name = ("seedTextBox");
        this.seedTextBox.Size = (new global::System.Drawing.Size(156, 23));
        this.seedTextBox.TabIndex = (2);
        this.toolTip1.SetToolTip(this.seedTextBox, "This represents the random values that will be used. A different seed results in a different shuffled ROM.");
        // 
        // romFileLabel
        // 
        this.romFileLabel.AutoSize = (true);
        this.romFileLabel.Location = (new global::System.Drawing.Point(10, 10));
        this.romFileLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.romFileLabel.Name = ("romFileLabel");
        this.romFileLabel.Size = (new global::System.Drawing.Size(55, 15));
        this.romFileLabel.TabIndex = (4);
        this.romFileLabel.Text = ("ROM File");
        // 
        // seedLabel
        // 
        this.seedLabel.AutoSize = (true);
        this.seedLabel.Location = (new global::System.Drawing.Point(10, 59));
        this.seedLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.seedLabel.Name = ("seedLabel");
        this.seedLabel.Size = (new global::System.Drawing.Size(32, 15));
        this.seedLabel.TabIndex = (5);
        this.seedLabel.Text = ("Seed");
        // 
        // createSeedButton
        // 
        this.createSeedButton.Location = (new global::System.Drawing.Point(177, 76));
        this.createSeedButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.createSeedButton.Name = ("createSeedButton");
        this.createSeedButton.Size = (new global::System.Drawing.Size(88, 27));
        this.createSeedButton.TabIndex = (6);
        this.createSeedButton.Text = ("Create Seed");
        this.createSeedButton.UseVisualStyleBackColor = (true);
        this.createSeedButton.Click += (this.createSeedButton_Click);
        // 
        // romFileBrowseButton
        // 
        this.romFileBrowseButton.Location = (new global::System.Drawing.Point(177, 29));
        this.romFileBrowseButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.romFileBrowseButton.Name = ("romFileBrowseButton");
        this.romFileBrowseButton.Size = (new global::System.Drawing.Size(88, 27));
        this.romFileBrowseButton.TabIndex = (7);
        this.romFileBrowseButton.Text = ("Browse...");
        this.romFileBrowseButton.UseVisualStyleBackColor = (true);
        this.romFileBrowseButton.Click += (this.fileBtn_Click);
        // 
        // generateRomButton
        // 
        this.generateRomButton.Location = (new global::System.Drawing.Point(491, 30));
        this.generateRomButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.generateRomButton.Name = ("generateRomButton");
        this.generateRomButton.Size = (new global::System.Drawing.Size(126, 24));
        this.generateRomButton.TabIndex = (8);
        this.generateRomButton.Text = ("Generate ROM");
        this.toolTip1.SetToolTip(this.generateRomButton, "Create the ROM");
        this.generateRomButton.UseVisualStyleBackColor = (true);
        this.generateRomButton.Click += (this.generateBtn_Click);
        // 
        // flagsTextBox
        // 
        this.flagsTextBox.Location = (new global::System.Drawing.Point(272, 30));
        this.flagsTextBox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.flagsTextBox.Name = ("flagsTextBox");
        this.flagsTextBox.Size = (new global::System.Drawing.Size(212, 23));
        this.flagsTextBox.TabIndex = (9);
        this.toolTip1.SetToolTip(this.flagsTextBox, "These flags represent the selected options. They can be copy/pasted.");
        this.flagsTextBox.TextChanged += (this.FlagBox_TextChanged);
        // 
        // updateButton
        // 
        this.updateButton.Location = (new global::System.Drawing.Point(492, 105));
        this.updateButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.updateButton.Name = ("updateButton");
        this.updateButton.Size = (new global::System.Drawing.Size(126, 27));
        this.updateButton.TabIndex = (10);
        this.updateButton.Text = ("Check for Updates");
        this.toolTip1.SetToolTip(this.updateButton, "Check for updates");
        this.updateButton.UseVisualStyleBackColor = (true);
        this.updateButton.Click += (this.UpdateBtn_Click);
        // 
        // flagsLabel
        // 
        this.flagsLabel.AutoSize = (true);
        this.flagsLabel.Location = (new global::System.Drawing.Point(273, 12));
        this.flagsLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.flagsLabel.Name = ("flagsLabel");
        this.flagsLabel.Size = (new global::System.Drawing.Size(34, 15));
        this.flagsLabel.TabIndex = (11);
        this.flagsLabel.Text = ("Flags");
        // 
        // wikiButton
        // 
        this.wikiButton.Location = (new global::System.Drawing.Point(386, 105));
        this.wikiButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.wikiButton.Name = ("wikiButton");
        this.wikiButton.Size = (new global::System.Drawing.Size(99, 27));
        this.wikiButton.TabIndex = (12);
        this.wikiButton.Text = ("Wiki");
        this.toolTip1.SetToolTip(this.wikiButton, "Visit the website");
        this.wikiButton.UseVisualStyleBackColor = (true);
        this.wikiButton.Click += (this.WikiBtn_Click);
        // 
        // customFlagsButton1
        // 
        this.customFlagsButton1.AutoEllipsis = (true);
        this.customFlagsButton1.Location = (new global::System.Drawing.Point(16, 504));
        this.customFlagsButton1.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.customFlagsButton1.Name = ("customFlagsButton1");
        this.customFlagsButton1.Size = (new global::System.Drawing.Size(96, 27));
        this.customFlagsButton1.TabIndex = (18);
        this.customFlagsButton1.Text = ("Beginner");
        this.toolTip1.SetToolTip(this.customFlagsButton1, "This preset is great for people who are looking for a casual experience.");
        this.customFlagsButton1.UseVisualStyleBackColor = (true);
        // 
        // customFlagsButton2
        // 
        this.customFlagsButton2.AutoEllipsis = (true);
        this.customFlagsButton2.Location = (new global::System.Drawing.Point(116, 504));
        this.customFlagsButton2.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.customFlagsButton2.Name = ("customFlagsButton2");
        this.customFlagsButton2.Size = (new global::System.Drawing.Size(96, 27));
        this.customFlagsButton2.TabIndex = (19);
        this.customFlagsButton2.Text = ("Standard");
        this.toolTip1.SetToolTip(this.customFlagsButton2, "Flags for the 2022 Standard Tournament");
        this.customFlagsButton2.UseVisualStyleBackColor = (true);
        // 
        // customFlagsButton3
        // 
        this.customFlagsButton3.AutoEllipsis = (true);
        this.customFlagsButton3.Location = (new global::System.Drawing.Point(216, 504));
        this.customFlagsButton3.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.customFlagsButton3.Name = ("customFlagsButton3");
        this.customFlagsButton3.Size = (new global::System.Drawing.Size(96, 27));
        this.customFlagsButton3.TabIndex = (20);
        this.customFlagsButton3.Text = ("Max Rando");
        this.toolTip1.SetToolTip(this.customFlagsButton3, "Flags for the 2023 Max Rando Tournament");
        this.customFlagsButton3.UseVisualStyleBackColor = (true);
        // 
        // customFlagsButton4
        // 
        this.customFlagsButton4.AutoEllipsis = (true);
        this.customFlagsButton4.Location = (new global::System.Drawing.Point(316, 504));
        this.customFlagsButton4.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.customFlagsButton4.Name = ("customFlagsButton4");
        this.customFlagsButton4.Size = (new global::System.Drawing.Size(96, 27));
        this.customFlagsButton4.TabIndex = (21);
        this.customFlagsButton4.Text = ("Random%");
        this.toolTip1.SetToolTip(this.customFlagsButton4, "Is it randomized? Who knows?");
        this.customFlagsButton4.UseVisualStyleBackColor = (true);
        // 
        // customFlagsButton5
        // 
        this.customFlagsButton5.AutoEllipsis = (true);
        this.customFlagsButton5.Location = (new global::System.Drawing.Point(416, 504));
        this.customFlagsButton5.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.customFlagsButton5.Name = ("customFlagsButton5");
        this.customFlagsButton5.Size = (new global::System.Drawing.Size(96, 27));
        this.customFlagsButton5.TabIndex = (22);
        this.toolTip1.SetToolTip(this.customFlagsButton5, "Unused (for now)");
        this.customFlagsButton5.UseVisualStyleBackColor = (true);
        // 
        // customFlagsButton6
        // 
        this.customFlagsButton6.AutoEllipsis = (true);
        this.customFlagsButton6.Location = (new global::System.Drawing.Point(516, 504));
        this.customFlagsButton6.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.customFlagsButton6.Name = ("customFlagsButton6");
        this.customFlagsButton6.Size = (new global::System.Drawing.Size(96, 27));
        this.customFlagsButton6.TabIndex = (23);
        this.toolTip1.SetToolTip(this.customFlagsButton6, "Unused (for now)");
        this.customFlagsButton6.UseVisualStyleBackColor = (true);
        // 
        // discordButton
        // 
        this.discordButton.Location = (new global::System.Drawing.Point(273, 105));
        this.discordButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.discordButton.Name = ("discordButton");
        this.discordButton.Size = (new global::System.Drawing.Size(106, 27));
        this.discordButton.TabIndex = (24);
        this.discordButton.Text = ("Discord");
        this.toolTip1.SetToolTip(this.discordButton, "Join the Z2R Discord");
        this.discordButton.UseVisualStyleBackColor = (true);
        this.discordButton.Click += (this.DiscordButton_Click);
        // 
        // oldFlagsTextbox
        // 
        this.oldFlagsTextbox.Location = (new global::System.Drawing.Point(273, 76));
        this.oldFlagsTextbox.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.oldFlagsTextbox.Name = ("oldFlagsTextbox");
        this.oldFlagsTextbox.Size = (new global::System.Drawing.Size(212, 23));
        this.oldFlagsTextbox.TabIndex = (37);
        this.toolTip1.SetToolTip(this.oldFlagsTextbox, "These flags represent the selected options. They can be copy/pasted.");
        // 
        // convertButton
        // 
        this.convertButton.Location = (new global::System.Drawing.Point(491, 76));
        this.convertButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.convertButton.Name = ("convertButton");
        this.convertButton.Size = (new global::System.Drawing.Size(126, 24));
        this.convertButton.TabIndex = (39);
        this.convertButton.Text = ("Convert");
        this.toolTip1.SetToolTip(this.convertButton, "Create the ROM");
        this.convertButton.UseVisualStyleBackColor = (true);
        this.convertButton.Click += (this.convertButton_Click);
        // 
        // backgroundWorker1
        // 
        this.backgroundWorker1.WorkerReportsProgress = (true);
        this.backgroundWorker1.WorkerSupportsCancellation = (true);
        this.backgroundWorker1.DoWork += (this.BackgroundWorker1_DoWork);
        this.backgroundWorker1.ProgressChanged += (this.BackgroundWorker1_ProgressChanged);
        // 
        // oldFlagsLabel
        // 
        this.oldFlagsLabel.AutoSize = (true);
        this.oldFlagsLabel.Location = (new global::System.Drawing.Point(273, 58));
        this.oldFlagsLabel.Margin = (new global::System.Windows.Forms.Padding(4, 0, 4, 0));
        this.oldFlagsLabel.Name = ("oldFlagsLabel");
        this.oldFlagsLabel.Size = (new global::System.Drawing.Size(56, 15));
        this.oldFlagsLabel.TabIndex = (38);
        this.oldFlagsLabel.Text = ("Old Flags");
        // 
        // batchButton
        // 
        this.batchButton.Location = (new global::System.Drawing.Point(177, 105));
        this.batchButton.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.batchButton.Name = ("batchButton");
        this.batchButton.Size = (new global::System.Drawing.Size(88, 27));
        this.batchButton.TabIndex = (40);
        this.batchButton.Text = ("Batch");
        this.batchButton.UseVisualStyleBackColor = (true);
        this.batchButton.Click += (this.Bulk_Generate_Click);
        // 
        // customisableButtonContextMenu
        // 
        this.customisableButtonContextMenu.Name = ("contextMenuStrip1");
        this.customisableButtonContextMenu.Size = (new global::System.Drawing.Size(61, 4));
        // 
        // MainUI
        // 
        this.AutoScaleDimensions = (new global::System.Drawing.SizeF(7F, 15F));
        this.AutoScaleMode = (global::System.Windows.Forms.AutoScaleMode.Font);
        this.ClientSize = (new global::System.Drawing.Size(626, 551));
        this.Controls.Add(this.batchButton);
        this.Controls.Add(this.convertButton);
        this.Controls.Add(this.oldFlagsLabel);
        this.Controls.Add(this.oldFlagsTextbox);
        this.Controls.Add(this.discordButton);
        this.Controls.Add(this.customFlagsButton6);
        this.Controls.Add(this.customFlagsButton5);
        this.Controls.Add(this.customFlagsButton4);
        this.Controls.Add(this.customFlagsButton3);
        this.Controls.Add(this.customFlagsButton2);
        this.Controls.Add(this.customFlagsButton1);
        this.Controls.Add(this.wikiButton);
        this.Controls.Add(this.flagsLabel);
        this.Controls.Add(this.updateButton);
        this.Controls.Add(this.flagsTextBox);
        this.Controls.Add(this.generateRomButton);
        this.Controls.Add(this.romFileBrowseButton);
        this.Controls.Add(this.createSeedButton);
        this.Controls.Add(this.seedLabel);
        this.Controls.Add(this.romFileLabel);
        this.Controls.Add(this.seedTextBox);
        this.Controls.Add(this.romFileTextBox);
        this.Controls.Add(this.mainTabControl);
        this.FormBorderStyle = (global::System.Windows.Forms.FormBorderStyle.FixedSingle);
        this.Margin = (new global::System.Windows.Forms.Padding(4, 3, 4, 3));
        this.Name = ("MainUI");
        this.Text = ("Zelda 2 Randomizer");
        this.mainTabControl.ResumeLayout(false);
        this.tabPage4.ResumeLayout(false);
        this.tabPage4.PerformLayout();
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.itemGrp.ResumeLayout(false);
        this.itemGrp.PerformLayout();
        this.tabPage1.ResumeLayout(false);
        this.tabPage1.PerformLayout();
        this.tabPage2.ResumeLayout(false);
        this.tabPage2.PerformLayout();
        this.tabPage5.ResumeLayout(false);
        this.tabPage5.PerformLayout();
        this.expBox.ResumeLayout(false);
        this.expBox.PerformLayout();
        this.tabPage9.ResumeLayout(false);
        this.tabPage9.PerformLayout();
        this.tabPage6.ResumeLayout(false);
        this.tabPage6.PerformLayout();
        this.tabPage7.ResumeLayout(false);
        this.tabPage7.PerformLayout();
        this.tabPage8.ResumeLayout(false);
        this.tabPage8.PerformLayout();
        this.tabPage10.ResumeLayout(false);
        this.tabPage10.PerformLayout();
        this.tabPage3.ResumeLayout(false);
        this.tabPage3.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TabControl mainTabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TextBox romFileTextBox;
    private System.Windows.Forms.TextBox seedTextBox;
    private System.Windows.Forms.Label romFileLabel;
    private System.Windows.Forms.TabPage tabPage4;
    private System.Windows.Forms.GroupBox itemGrp;
    private System.Windows.Forms.CheckBox startWithMagicKeyCheckbox;
    private System.Windows.Forms.CheckBox startWithHammerCheckbox;
    private System.Windows.Forms.CheckBox startWithCrossCheckbox;
    private System.Windows.Forms.CheckBox startWithFluteCheckbox;
    private System.Windows.Forms.CheckBox startWithBootsCheckbox;
    private System.Windows.Forms.CheckBox startWithRaftCheckbox;
    private System.Windows.Forms.CheckBox startWithGloveCheckbox;
    private System.Windows.Forms.CheckBox startWithCandleCheckbox;
    private System.Windows.Forms.CheckBox shuffleStartingItemsCheckbox;
    private System.Windows.Forms.TabPage tabPage5;
    private System.Windows.Forms.Label seedLabel;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox startWIthThunderCheckbox;
    private System.Windows.Forms.CheckBox shuffleStartingSpellsCheckbox;
    private System.Windows.Forms.CheckBox startWithShieldCheckbox;
    private System.Windows.Forms.CheckBox startWithJumpCheckbox;
    private System.Windows.Forms.CheckBox startWithLifeCheckbox;
    private System.Windows.Forms.CheckBox startWithFairyCheckbox;
    private System.Windows.Forms.CheckBox startWithFireCheckbox;
    private System.Windows.Forms.CheckBox startWithReflectCheckbox;
    private System.Windows.Forms.CheckBox startWithSpellCheckbox;
    private System.Windows.Forms.Button createSeedButton;
    private System.Windows.Forms.Button romFileBrowseButton;
    private System.Windows.Forms.Button generateRomButton;
    private System.Windows.Forms.Label startingHeartContainersLabel;
    private System.Windows.Forms.ComboBox startHeartsMinList;
    private System.Windows.Forms.CheckBox randomizeLivesBox;
    private System.Windows.Forms.Label startingTechsLabel;
    private System.Windows.Forms.ComboBox startingTechsList;
    private System.Windows.Forms.TabPage tabPage6;
    private System.Windows.Forms.CheckBox shuffleEnemyHPBox;
    private System.Windows.Forms.GroupBox expBox;
    private System.Windows.Forms.CheckBox lifeExpNeededCheckbox;
    private System.Windows.Forms.CheckBox magicExpNeededCheckbox;
    private System.Windows.Forms.CheckBox shuffleAtkExpNeededCheckbox;
    private System.Windows.Forms.CheckBox shuffleAllExpCheckbox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ComboBox startingGemsMinList;
    private System.Windows.Forms.CheckBox shuffleSwordImmunityBox;
    private System.Windows.Forms.CheckBox shuffleStealXPAmountCheckbox;
    private System.Windows.Forms.CheckBox shuffleXPStealersCheckbox;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.CheckBox disableLowHealthBeepCheckbox;
    private System.Windows.Forms.CheckBox tbirdRequiredCheckbox;
    private System.Windows.Forms.CheckBox allowPathEnemiesCheckbox;
    private System.Windows.Forms.CheckBox shuffleEncountersCheckbox;
    private System.Windows.Forms.CheckBox shufflePalaceEnemiesCheckbox;
    private System.Windows.Forms.CheckBox shuffleOverworldEnemiesCheckbox;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.CheckBox jumpAlwaysOnCheckbox;
    private System.Windows.Forms.TextBox flagsTextBox;
    private System.Windows.Forms.Button updateButton;
    private System.Windows.Forms.Label flagsLabel;
    private System.Windows.Forms.CheckBox mixLargeAndSmallCheckbox;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.Button wikiButton;
    private System.Windows.Forms.TabPage tabPage7;
    private System.Windows.Forms.CheckBox mixOverworldPalaceItemsCheckbox;
    private System.Windows.Forms.CheckBox shuffleOverworldItemsCheckbox;
    private System.Windows.Forms.CheckBox shufflePalaceItemsCheckbox;
    private System.Windows.Forms.CheckBox shuffleSmallItemsCheckbox;
    private System.Windows.Forms.CheckBox palacesHaveExtraKeysCheckbox;
    private System.Windows.Forms.CheckBox palacePaletteCheckbox;
    private System.Windows.Forms.Button customFlagsButton6;
    private System.Windows.Forms.Button customFlagsButton5;
    private System.Windows.Forms.Button customFlagsButton4;
    private System.Windows.Forms.Button customFlagsButton3;
    private System.Windows.Forms.Button customFlagsButton2;
    private System.Windows.Forms.Button customFlagsButton1;
    private System.Windows.Forms.CheckBox allowPalaceContinentSwapCheckbox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label magicEffectivenessLabel;
    private System.Windows.Forms.Label attackEffectivenessLabel;
    private System.Windows.Forms.CheckBox restartAtPalacesCheckbox;
    private System.Windows.Forms.CheckBox shortGPCheckbox;
    private System.Windows.Forms.CheckBox fastSpellCheckbox;
    private System.Windows.Forms.CheckBox randomizeJarRequirementsCheckbox;
    private System.Windows.Forms.CheckBox removeTbirdCheckbox;
    private System.Windows.Forms.CheckBox includePbagCavesInShuffleCheckbox;
    private System.Windows.Forms.CheckBox alwaysBeamCheckbox;
    private System.Windows.Forms.CheckBox includeGPinShuffleCheckbox;
    private System.Windows.Forms.Button discordButton;
    private System.Windows.Forms.CheckBox shuffleDripperEnemyCheckbox;
    private System.Windows.Forms.CheckBox shuffleEnemyPalettesCheckbox;
    private System.Windows.Forms.ComboBox hiddenPalaceList;
    private System.Windows.Forms.Label hiddenPalaceLabel;
    private System.Windows.Forms.CheckBox disableMusicCheckbox;
    private System.Windows.Forms.ComboBox hideKasutoList;
    private System.Windows.Forms.Label hiddenKasutoLabel;
    private System.Windows.Forms.ComboBox characterSpriteList;
    private System.Windows.Forms.Label characterSpriteLabel;
    private System.Windows.Forms.ComboBox tunicColorList;
    private System.Windows.Forms.Label tunicColorLabel;
    private System.Windows.Forms.ComboBox shieldColorList;
    private System.Windows.Forms.Label shieldColorLabel;
    private System.Windows.Forms.CheckBox removeSpellitemsCheckbox;
    private System.Windows.Forms.TabPage tabPage8;
    private System.Windows.Forms.CheckBox largeEnemiesKeyCheckbox;
    private System.Windows.Forms.CheckBox largeEnemies1UpCheckbox;
    private System.Windows.Forms.CheckBox largeEnemiesXLBagCheckbox;
    private System.Windows.Forms.CheckBox largeEnemiesLargeBagCheckbox;
    private System.Windows.Forms.CheckBox largeEnemiesMediumBagCheckbox;
    private System.Windows.Forms.CheckBox largeEnemiesSmallBagCheckbox;
    private System.Windows.Forms.CheckBox largeEnemiesRedJarCheckbox;
    private System.Windows.Forms.CheckBox largeEnemiesBlueJarCheckbox;
    private System.Windows.Forms.Label largeEnemyPoolLabel;
    private System.Windows.Forms.CheckBox smallEnemiesKeyCheckbox;
    private System.Windows.Forms.CheckBox smallEnemies1UpCheckbox;
    private System.Windows.Forms.CheckBox smallEnemiesXLBagCheckbox;
    private System.Windows.Forms.CheckBox smallEnemiesLargeBagCheckbox;
    private System.Windows.Forms.CheckBox smallEnemiesMediumBagCheckbox;
    private System.Windows.Forms.CheckBox smallEnemiesSmallBagCheckbox;
    private System.Windows.Forms.CheckBox smallEnemiesRedJarCheckbox;
    private System.Windows.Forms.CheckBox smallEnemiesBlueJarCheckbox;
    private System.Windows.Forms.Label smallEnemyPoolLabel;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.CheckBox shuffleDropFrequencyCheckbox;
    private System.Windows.Forms.ComboBox beamSpriteList;
    private System.Windows.Forms.Label beamSpriteLabel;
    private System.Windows.Forms.CheckBox standardizeDropsCheckbox;
    private System.Windows.Forms.CheckBox randomizeDropsCheckbox;
    private System.Windows.Forms.CheckBox shufflePbagAmountsCheckbox;
    private System.Windows.Forms.TabPage tabPage9;
    private System.Windows.Forms.ComboBox lifeCapList;
    private System.Windows.Forms.ComboBox magCapList;
    private System.Windows.Forms.ComboBox atkCapList;
    private System.Windows.Forms.Label lifeCapLabel;
    private System.Windows.Forms.Label magCapLabel;
    private System.Windows.Forms.Label attackCapLabel;
    private System.Windows.Forms.CheckBox disableMagicContainerRequirementCheckbox;
    private System.Windows.Forms.CheckBox shuffleSpellLocationsCheckbox;
    private System.Windows.Forms.CheckBox shuffleLifeRefillCheckbox;
    private System.Windows.Forms.Label levelCapLabel;
    private System.Windows.Forms.CheckBox scaleLevelRequirementsToCapCheckbox;
    private System.Windows.Forms.TabPage tabPage10;
    private System.Windows.Forms.CheckBox enableTownNameHintsCheckbox;
    private System.Windows.Forms.CheckBox enableSpellItemHintsCheckbox;
    private System.Windows.Forms.CheckBox useCommunityHintsCheckbox;
    private System.Windows.Forms.CheckBox enableHelpfulHintsCheckbox;
    private System.Windows.Forms.ComboBox encounterRateBox;
    private System.Windows.Forms.Label encounterRateLabel;
    private System.Windows.Forms.ComboBox attackEffectivenessList;
    private System.Windows.Forms.ComboBox magicEffectivenessList;
    private System.Windows.Forms.ComboBox lifeEffectivenessList;
    private System.Windows.Forms.Label enemyExperienceDropsLabel;
    private System.Windows.Forms.ComboBox experienceDropsList;
    private System.Windows.Forms.Label startingLevelsLabel;
    private System.Windows.Forms.ComboBox startingLifeLevelList;
    private System.Windows.Forms.ComboBox startingMagicLevelList;
    private System.Windows.Forms.ComboBox startingAttackLevelList;
    private System.Windows.Forms.Label startingLifeLabel;
    private System.Windows.Forms.Label startingMagicLabel;
    private System.Windows.Forms.Label startingAttackLabel;
    private System.Windows.Forms.Label label36;
    private System.Windows.Forms.Label ContinentConnectionLabel;
    private System.Windows.Forms.ComboBox continentConnectionBox;
    private System.Windows.Forms.CheckBox upAOnController1Checkbox;
    private System.Windows.Forms.CheckBox saneCaveShuffleBox;
    private System.Windows.Forms.CheckBox hideLessImportantLocationsCheckbox;
    private System.Windows.Forms.CheckBox allowBoulderBlockedConnectionsCheckbox;
    private System.Windows.Forms.Label label39;
    private System.Windows.Forms.Label westContinentLabel;
    private System.Windows.Forms.ComboBox eastBiome;
    private System.Windows.Forms.ComboBox dmBiome;
    private System.Windows.Forms.ComboBox westBiome;
    private System.Windows.Forms.Label eastContinentBindingLabel;
    private System.Windows.Forms.Label deathMountainBiomeLabel;
    private System.Windows.Forms.CheckBox flashingOffCheckbox;
    private System.Windows.Forms.ComboBox mazeBiome;
    private System.Windows.Forms.Label mazeIslandBiomeLabel;
    private System.Windows.Forms.CheckBox shuffledVanillaShowsActualTerrain;
    private System.Windows.Forms.CheckBox shuffleWhichLocationsAreHiddenCheckbox;
    private System.Windows.Forms.CheckBox randomizeBossItemCheckbox;
    private System.Windows.Forms.CheckBox useGoodBootsCheckbox;
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private System.Windows.Forms.CheckBox randomizeSpellSpellEnemyCheckbox;
    private System.Windows.Forms.CheckBox generateBaguWoodsCheckbox;
    private System.Windows.Forms.CheckBox includeCommunityRoomsCheckbox;
    private System.Windows.Forms.CheckBox blockingRoomsInAnyPalaceCheckbox;
    private System.Windows.Forms.CheckBox bossRoomsExitToPalaceCheckbox;
    private System.Windows.Forms.Label palaceStyleLabel;
    private System.Windows.Forms.ComboBox palaceStyleList;
    private System.Windows.Forms.CheckBox dashAlwaysOnCheckbox;
    private System.Windows.Forms.Label oldFlagsLabel;
    private System.Windows.Forms.TextBox oldFlagsTextbox;
    private System.Windows.Forms.Button convertButton;
    private System.Windows.Forms.Button batchButton;
    private System.Windows.Forms.ComboBox maxHeartsList;
    private System.Windows.Forms.ComboBox startHeartsMaxList;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox startingGemsMaxList;
    private System.Windows.Forms.CheckBox swapUpAndDownstabCheckbox;
    private System.Windows.Forms.CheckBox includeLavaInShuffle;
    private System.Windows.Forms.CheckBox useCustomRoomsBox;
    private System.Windows.Forms.Label FireSpellOptionLabel;
    private System.Windows.Forms.ComboBox FireSpellBox;
    private System.Windows.Forms.CheckBox noDuplicateRoomsCheckbox;
    private CheckBox generatorsMatchCheckBox;
    private global::System.Windows.Forms.ContextMenuStrip customisableButtonContextMenu;
}


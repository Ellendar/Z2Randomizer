using NLog;
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Z2Randomizer
{
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
        private RandomizerProperties props;

        private const int numFlags = 27;


        public MainUI()
        {
            if (Properties.Settings.Default.update)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.update = false;
                Properties.Settings.Default.Save();
            }
            InitializeComponent();
            r = new Random();
            heartCmbo.SelectedIndex = 3;
            maxHeartsBox.SelectedIndex = 7;
            numGemsCbo.SelectedIndex = 6;
            techCmbo.SelectedIndex = 0;
            allowPathEnemies.Enabled = false;
            fileTextBox.Text = Properties.Settings.Default.filePath;
            tunicColor.SelectedIndex = Properties.Settings.Default.tunic;
            shieldColor.SelectedIndex = Properties.Settings.Default.shield;
            fastSpellBox.Checked = Properties.Settings.Default.spells;
            disableLowHealthBeep.Checked = Properties.Settings.Default.beep;
            beamCmbo.SelectedIndex = Properties.Settings.Default.beams;
            disableMusicBox.Checked = Properties.Settings.Default.music;
            upAC1.Checked = Properties.Settings.Default.upac1;
            customBox1.Text = Properties.Settings.Default.custom1;
            customBox2.Text = Properties.Settings.Default.custom2;
            customBox3.Text = Properties.Settings.Default.custom3;
            seedTextBox.Text = Properties.Settings.Default.lastseed;
            flashingOff.Checked = Properties.Settings.Default.noflash;


            customBox1.TextChanged += new System.EventHandler(this.customSave1_Click);
            customBox2.TextChanged += new System.EventHandler(this.customSave2_Click);

            customBox3.TextChanged += new System.EventHandler(this.customSave3_Click);


            dontrunhandler = false;
            mixEnemies.Enabled = false;
            mixItemBox.Enabled = false;
            gpBox.Enabled = false;
            pbagItemShuffleBox.Enabled = false;
            p7Shuffle.Enabled = false;
            tbirdBox.Checked = true;
            hpCmbo.SelectedIndex = 0;
            hideKasutoBox.SelectedIndex = 0;
            spriteCmbo.SelectedIndex = Properties.Settings.Default.sprite;
            CheckBox[] temp = { smallBlueJar, smallRedJar, small50, small100, small200, small500, small1up, smallKey };
            small = temp;
            CheckBox[] temp2 = { largeBlueJar, largeRedJar, large50, large100, large200, large500, large1up, largeKey };
            large = temp2;


            this.Text = "Zelda 2 Randomizer Version " + typeof(MainUI).Assembly.GetName().Version.Major + "." + typeof(MainUI).Assembly.GetName().Version.Minor + "." + typeof(MainUI).Assembly.GetName().Version.Build;

            flagBox.DoubleClick += new System.EventHandler(this.flagBox_Clicked);

            shuffleItemBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            candleBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            gloveBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            raftBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            bootsBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            fluteBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            crossBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            hammerBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            keyBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            spellShuffleBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            shieldBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            jumpBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            lifeBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            fairyBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            fireBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            reflectBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            spellBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            thunderBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            heartCmbo.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            maxHeartsBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            techCmbo.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            numGemsCbo.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            livesBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleEnemyHPBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleAllExp.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleAtkExp.CheckedChanged += new System.EventHandler(this.updateFlags);
            lifeExpNeeded.CheckedChanged += new System.EventHandler(this.updateFlags);
            magicExpNeeded.CheckedChanged += new System.EventHandler(this.updateFlags);
            stealExpBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            stealExpAmt.CheckedChanged += new System.EventHandler(this.updateFlags);
            swordImmuneBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            jumpNormalbox.CheckedChanged += new System.EventHandler(this.updateFlags);
            lifeRefilBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            disableLowHealthBeep.CheckedChanged += new System.EventHandler(this.updateFlags);
            tbirdBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            expDropBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            shuffleEncounters.CheckedChanged += new System.EventHandler(this.updateFlags);
            allowPathEnemies.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleOverworldEnemies.CheckedChanged += new System.EventHandler(this.updateFlags);
            shufflePalaceEnemies.CheckedChanged += new System.EventHandler(this.updateFlags);
            mixEnemies.CheckedChanged += new System.EventHandler(this.updateFlags);
            palaceItemBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            overworldItemBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            mixItemBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleSmallItemsBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleSpellLocationsBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            disableJarBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            palaceKeys.CheckedChanged += new System.EventHandler(this.updateFlags);
            pbagDrop.CheckedChanged += new System.EventHandler(this.updateFlags);
            palacePalette.CheckedChanged += new System.EventHandler(this.updateFlags);
            palaceSwapBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            atkEffBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            magEffBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            lifeEffBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            upaBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            gpBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            kasutoBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            combineFireBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            removeTbird.CheckedChanged += new System.EventHandler(this.updateFlags);
            beamBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            pbagItemShuffleBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            p7Shuffle.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleDripper.CheckedChanged += new System.EventHandler(this.updateFlags);
            enemyPalette.CheckedChanged += new System.EventHandler(this.updateFlags);
            hpCmbo.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            hideKasutoBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            enemyDropBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            spellItemBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            communityBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            standardDrops.CheckedChanged += new System.EventHandler(this.updateFlags);
            randoDrops.CheckedChanged += new System.EventHandler(this.updateFlags);
            shufflePbagExp.CheckedChanged += new System.EventHandler(this.updateFlags);
            atkCapBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            magCapBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            lifeCapBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            scaleLevels.CheckedChanged += new System.EventHandler(this.updateFlags);
            townSpellHints.CheckedChanged += new System.EventHandler(this.updateFlags);
            helpfulHints.CheckedChanged += new System.EventHandler(this.updateFlags);
            spellItemHints.CheckedChanged += new System.EventHandler(this.updateFlags);
            encounterBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            startAtkBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            startMagBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            startLifeBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            continentConnectionBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            saneCaveShuffleBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            hideLocsBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            boulderConnectionBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            westBiome.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            dmBiome.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            eastBiome.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            mazeBiome.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            vanillaOriginalTerrain.CheckedChanged += new System.EventHandler(this.updateFlags);
            shuffleHidden.CheckedChanged += new System.EventHandler(this.updateFlags);
            bossItem.CheckedChanged += new System.EventHandler(this.updateFlags);
            waterBoots.CheckedChanged += new System.EventHandler(this.updateFlags);
            spellEnemy.CheckedChanged += new System.EventHandler(this.updateFlags);
            baguBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            palaceBox.SelectedIndexChanged += new System.EventHandler(this.updateFlags);
            customRooms.CheckedChanged += new System.EventHandler(this.updateFlags);
            blockerBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            bossRoomBox.CheckedChanged += new System.EventHandler(this.updateFlags);
            dashBox.CheckedChanged += new System.EventHandler(this.updateFlags);



            //townSwap.CheckedChanged += new System.EventHandler(this.updateFlags);

            enableLevelScaling(null, null);
            eastBiome_SelectedIndexChanged(null, null);
            for (int i = 0; i < small.Count(); i++)
            {
                small[i].CheckedChanged += new System.EventHandler(this.updateFlags);
                small[i].CheckedChanged += new System.EventHandler(this.atLeastOneChecked);
                large[i].CheckedChanged += new System.EventHandler(this.updateFlags);
                large[i].CheckedChanged += new System.EventHandler(this.atLeastOneChecked);
            }
            String f = Properties.Settings.Default.lastused;
            if (!f.Equals(""))
            {
                dontrunhandler = true;
                flagBox.Text = f;
                dontrunhandler = false;
            }
            else
            {
                //updateFlags(null, null);
                beginnerFlags(null, null);
            }


            string path = Directory.GetCurrentDirectory();
            logger.Debug(path);
            //WinSparkle.win_sparkle_set_appcast_url("https://www.dropbox.com/s/w4d9qptlg1kyx0o/appcast.xml?dl=1");
            //WinSparkle.win_sparkle_set_app_details("Company","App", "Version"); // THIS CALL NOT IMPLEMENTED YET
            //WinSparkle.win_sparkle_init();
        }

        private void shuffleItemBox_CheckedChanged(object sender, EventArgs e)
        {
            candleBox.Enabled = !shuffleItemBox.Checked;
            gloveBox.Enabled = !shuffleItemBox.Checked;
            raftBox.Enabled = !shuffleItemBox.Checked;
            bootsBox.Enabled = !shuffleItemBox.Checked;
            fluteBox.Enabled = !shuffleItemBox.Checked;
            crossBox.Enabled = !shuffleItemBox.Checked;
            hammerBox.Enabled = !shuffleItemBox.Checked;
            keyBox.Enabled = !shuffleItemBox.Checked;

            if (shuffleItemBox.Checked)
            {
                candleBox.Checked = false;
                gloveBox.Checked = false;
                raftBox.Checked = false;
                bootsBox.Checked = false;
                fluteBox.Checked = false;
                crossBox.Checked = false;
                hammerBox.Checked = false;
                keyBox.Checked = false;
            }
        }

        private void fileBtn_Click(object sender, EventArgs e)
        {
            var FD = new System.Windows.Forms.OpenFileDialog();
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileTextBox.Text = FD.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            seedTextBox.Text = r.Next(1000000000).ToString();
        }

        private void spellShuffleBox_CheckedChanged(object sender, EventArgs e)
        {
            shieldBox.Enabled = !spellShuffleBox.Checked;
            jumpBox.Enabled = !spellShuffleBox.Checked;
            lifeBox.Enabled = !spellShuffleBox.Checked;
            fairyBox.Enabled = !spellShuffleBox.Checked;
            fireBox.Enabled = !spellShuffleBox.Checked;
            reflectBox.Enabled = !spellShuffleBox.Checked;
            spellBox.Enabled = !spellShuffleBox.Checked;
            thunderBox.Enabled = !spellShuffleBox.Checked;

            if (spellShuffleBox.Checked)
            {
                shieldBox.Checked = false;
                jumpBox.Checked = false;
                lifeBox.Checked = false;
                fairyBox.Checked = false;
                fireBox.Checked = false;
                reflectBox.Checked = false;
                spellBox.Checked = false;
                thunderBox.Checked = false;
            }
        }

        private void generateBtn_Click(object sender, EventArgs e)
        {
            String flagString = flagBox.Text;

            if (flagString.Length != numFlags)
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
            if (heartCmbo.SelectedIndex != 8 && heartCmbo.SelectedIndex > maxHeartsBox.SelectedIndex && maxHeartsBox.SelectedIndex != 8)
            {
                MessageBox.Show("Max hearts must be greater than or equal to starting hearts!");
                return;
            }
            Properties.Settings.Default.filePath = fileTextBox.Text;
            Properties.Settings.Default.beep = disableLowHealthBeep.Checked;
            Properties.Settings.Default.beams = beamCmbo.SelectedIndex;
            Properties.Settings.Default.spells = fastSpellBox.Checked;
            Properties.Settings.Default.tunic = tunicColor.SelectedIndex;
            Properties.Settings.Default.shield = shieldColor.SelectedIndex;
            Properties.Settings.Default.music = disableMusicBox.Checked;
            Properties.Settings.Default.sprite = spriteCmbo.SelectedIndex;
            Properties.Settings.Default.upac1 = upAC1.Checked;
            Properties.Settings.Default.noflash = flashingOff.Checked;
            Properties.Settings.Default.lastused = flagBox.Text;
            Properties.Settings.Default.lastseed = seedTextBox.Text;
            Properties.Settings.Default.Save();
            try
            {
                Int32.Parse(seedTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid Seed!");
                return;
            }
            props = generateProps();
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
                MessageBox.Show("File " + "Z2_" + props.seed + "_" + props.flags + ".nes" + " has been created!");
            }
            else
            {
                MessageBox.Show("An exception occurred generating the rom");
                logger.Error(generationException.StackTrace);
            }
        }


        private void shuffleAllExp_CheckedChanged(object sender, EventArgs e)
        {
            shuffleAtkExp.Checked = shuffleAllExp.Checked;
            shuffleAtkExp.Enabled = !shuffleAllExp.Checked;

            magicExpNeeded.Checked = shuffleAllExp.Checked;
            magicExpNeeded.Enabled = !shuffleAllExp.Checked;

            lifeExpNeeded.Checked = shuffleAllExp.Checked;
            lifeExpNeeded.Enabled = !shuffleAllExp.Checked;
        }


        private void shuffleEncounters_CheckedChanged(object sender, EventArgs e)
        {
            allowPathEnemies.Enabled = shuffleEncounters.Checked;
            if (!shuffleEncounters.Checked)
            {
                allowPathEnemies.Checked = false;
            }
        }

        private void updateFlags(object sender, EventArgs e)
        {
            if (!dontrunhandler)
            {
                String flagStr = "";
                BitArray v = new BitArray(6);
                int[] array = new int[1];

                v[0] = shuffleItemBox.Checked;
                v[1] = candleBox.Checked;
                v[2] = gloveBox.Checked;
                v[3] = raftBox.Checked;
                v[4] = bootsBox.Checked;
                v[5] = shuffleOverworldEnemies.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];
                v[0] = fluteBox.Checked;
                v[1] = crossBox.Checked;
                v[2] = hammerBox.Checked;
                v[3] = keyBox.Checked;
                v[4] = spellShuffleBox.Checked;
                v[5] = hideLocsBox.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = shieldBox.Checked;
                v[1] = jumpBox.Checked;
                v[2] = lifeBox.Checked;
                v[3] = fairyBox.Checked;
                v[4] = fireBox.Checked;
                v[5] = combineFireBox.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = reflectBox.Checked;
                v[1] = spellBox.Checked;
                v[2] = thunderBox.Checked;
                v[3] = livesBox.Checked;
                v[4] = removeTbird.Checked;
                v[5] = saneCaveShuffleBox.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];
                BitArray w = new BitArray(new int[] { heartCmbo.SelectedIndex, techCmbo.SelectedIndex });
                v[0] = w[0];
                v[1] = w[1];
                v[2] = w[2];
                v[3] = w[32];
                v[4] = w[33];
                v[5] = w[34];
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = pbagDrop.Checked;
                v[1] = pbagItemShuffleBox.Checked;
                v[2] = w[3];
                v[3] = p7Shuffle.Checked;
                v[4] = palacePalette.Checked;
                v[5] = shuffleEncounters.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = palaceKeys.Checked;
                v[1] = palaceSwapBox.Checked;
                w = new BitArray(new int[] { atkEffBox.SelectedIndex });
                v[2] = w[0];
                v[3] = w[1];
                v[4] = w[2];
                v[5] = allowPathEnemies.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = beamBox.Checked;
                v[1] = shuffleDripper.Checked;
                v[2] = dashBox.Checked;
                v[3] = shuffleEnemyHPBox.Checked;
                v[4] = shuffleAllExp.Checked;
                v[5] = shufflePalaceEnemies.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = shuffleAtkExp.Checked;
                v[1] = lifeExpNeeded.Checked;
                v[2] = magicExpNeeded.Checked;
                v[3] = upaBox.Checked;
                v[4] = gpBox.Checked;
                v[5] = tbirdBox.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                w = new BitArray(new int[] { magEffBox.SelectedIndex });
                v[0] = w[0];
                v[1] = w[1];
                v[2] = w[2];
                v[3] = stealExpBox.Checked;
                v[4] = stealExpAmt.Checked;
                v[5] = lifeRefilBox.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = swordImmuneBox.Checked;
                v[1] = jumpNormalbox.Checked;

                w = new BitArray(new int[] { numGemsCbo.SelectedIndex });
                v[2] = w[0];
                v[3] = w[1];
                v[4] = w[2];
                v[5] = mixEnemies.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = palaceItemBox.Checked;
                v[1] = overworldItemBox.Checked;
                v[2] = mixItemBox.Checked;
                v[3] = shuffleSmallItemsBox.Checked;
                v[4] = shuffleSpellLocationsBox.Checked;
                v[5] = disableJarBox.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];




                w = new BitArray(new int[] { lifeEffBox.SelectedIndex });
                v[0] = w[0];
                v[1] = w[1];
                v[2] = w[2];
                v[3] = kasutoBox.Checked;
                v[4] = communityBox.Checked;
                v[5] = enemyPalette.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                w = new BitArray(new int[] { maxHeartsBox.SelectedIndex });
                v[0] = w[0];
                v[1] = w[1];
                v[2] = w[2];
                v[3] = w[3];
                w = new BitArray(new int[] { hpCmbo.SelectedIndex });
                v[4] = w[0];
                v[5] = w[1];
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                w = new BitArray(new int[] { hideKasutoBox.SelectedIndex });
                v[0] = w[0];
                v[1] = w[1];
                v[2] = enemyDropBox.Checked;
                v[3] = spellItemBox.Checked;
                v[4] = smallBlueJar.Checked;
                v[5] = smallRedJar.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = small50.Checked;
                v[1] = small100.Checked;
                v[2] = small200.Checked;
                v[3] = small500.Checked;
                v[4] = small1up.Checked;
                v[5] = smallKey.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = largeBlueJar.Checked;
                v[1] = largeRedJar.Checked;
                v[2] = large50.Checked;
                v[3] = large100.Checked;
                v[4] = large200.Checked;
                v[5] = large500.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = large1up.Checked;
                v[1] = largeKey.Checked;
                v[2] = helpfulHints.Checked;
                v[3] = spellItemHints.Checked;
                v[4] = standardDrops.Checked;
                v[5] = randoDrops.Checked;
                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];


                v[0] = shufflePbagExp.Checked;
                w = new BitArray(new int[] { atkCapBox.SelectedIndex });
                v[1] = w[0];
                v[2] = w[1];
                v[3] = w[2];
                w = new BitArray(new int[] { magCapBox.SelectedIndex });
                v[4] = w[0];
                v[5] = w[1];

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = w[2];
                w = new BitArray(new int[] { lifeCapBox.SelectedIndex });
                v[1] = w[0];
                v[2] = w[1];
                v[3] = w[2];
                v[4] = scaleLevels.Checked;
                v[5] = townSpellHints.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                w = new BitArray(new int[] { encounterBox.SelectedIndex });
                v[0] = w[0];
                v[1] = w[1];
                w = new BitArray(new int[] { expDropBox.SelectedIndex });
                v[2] = w[0];
                v[3] = w[1];
                v[4] = w[2];
                w = new BitArray(new int[] { startAtkBox.SelectedIndex });
                v[5] = w[0];


                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = w[1];
                v[1] = w[2];
                w = new BitArray(new int[] { startMagBox.SelectedIndex });
                v[2] = w[0];
                v[3] = w[1];
                v[4] = w[2];
                w = new BitArray(new int[] { startLifeBox.SelectedIndex });
                v[5] = w[0];

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = w[1];
                v[1] = w[2];

                w = new BitArray(new int[] { continentConnectionBox.SelectedIndex });
                v[2] = w[0];
                v[3] = w[1];
                v[4] = boulderConnectionBox.Checked;
                w = new BitArray(new int[] { westBiome.SelectedIndex });
                v[5] = w[0];


                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = w[1];
                v[1] = w[2];

                v[2] = w[3];
                w = new BitArray(new int[] { dmBiome.SelectedIndex });
                v[3] = w[0];
                v[4] = w[1];
                v[5] = w[2];

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = w[3];
                w = new BitArray(new int[] { eastBiome.SelectedIndex });
                v[1] = w[0];

                v[2] = w[1];
                v[3] = w[2];
                v[4] = w[3];
                w = new BitArray(new int[] { mazeBiome.SelectedIndex });
                v[5] = w[0];

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                v[0] = w[1];
                v[1] = vanillaOriginalTerrain.Checked;
                v[2] = shuffleHidden.Checked;
                v[3] = bossItem.Checked;
                v[4] = waterBoots.Checked;
                v[5] = spellEnemy.Checked;

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                w = new BitArray(new int[] { palaceBox.SelectedIndex });
                v[0] = baguBox.Checked;
                v[1] = w[0];
                v[2] = customRooms.Checked;
                v[3] = blockerBox.Checked;
                v[4] = bossRoomBox.Checked;
                v[5] = w[1];

                v.CopyTo(array, 0);
                flagStr = flagStr + flags[array[0]];

                flagBox.Text = flagStr;
            }
        }

        private void flagBox_Clicked(object send, EventArgs e)
        {
            flagBox.SelectAll();
        }
        private void flagBox_TextChanged(object sender, EventArgs e)
        {

            dontrunhandler = true;
            try
            {
                String flagText = flagBox.Text;

                while (flagText.Length < numFlags)
                {
                    flagText += "A";
                }

                BitArray v = new BitArray(new int[] { flags.IndexOf(flagText[0]) });
                int[] array = new int[1];

                shuffleItemBox.Checked = v[0];
                candleBox.Checked = v[1];
                gloveBox.Checked = v[2];
                raftBox.Checked = v[3];
                bootsBox.Checked = v[4];
                shuffleOverworldEnemies.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[1]) });
                fluteBox.Checked = v[0];
                crossBox.Checked = v[1];
                hammerBox.Checked = v[2];
                keyBox.Checked = v[3];
                spellShuffleBox.Checked = v[4];
                hideLocsBox.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[2]) });

                shieldBox.Checked = v[0];
                jumpBox.Checked = v[1];
                lifeBox.Checked = v[2];
                fairyBox.Checked = v[3];
                fireBox.Checked = v[4];
                combineFireBox.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[3]) });

                reflectBox.Checked = v[0];
                spellBox.Checked = v[1];
                thunderBox.Checked = v[2];
                livesBox.Checked = v[3];
                removeTbird.Checked = v[4];
                saneCaveShuffleBox.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[4]) });
                BitArray w = new BitArray(3);
                w[0] = v[3];
                w[1] = v[4];
                w[2] = v[5];
                w.CopyTo(array, 0);
                techCmbo.SelectedIndex = array[0];
                w = new BitArray(4);
                w[0] = v[0];
                w[1] = v[1];
                w[2] = v[2];

                v = new BitArray(new int[] { flags.IndexOf(flagText[5]) });
                w[3] = v[2];
                w.CopyTo(array, 0);
                heartCmbo.SelectedIndex = array[0];
                pbagDrop.Checked = v[0];
                pbagItemShuffleBox.Checked = v[1];
                p7Shuffle.Checked = v[3];
                palacePalette.Checked = v[4];
                shuffleEncounters.Checked = v[5];
                v = new BitArray(new int[] { flags.IndexOf(flagText[6]) });

                palaceKeys.Checked = v[0];
                palaceSwapBox.Checked = v[1];
                w = new BitArray(3);
                w[0] = v[2];
                w[1] = v[3];
                w[2] = v[4];
                w.CopyTo(array, 0);
                atkEffBox.SelectedIndex = array[0];
                allowPathEnemies.Checked = v[5];
                v = new BitArray(new int[] { flags.IndexOf(flagText[7]) });

                beamBox.Checked = v[0];
                shuffleDripper.Checked = v[1];
                dashBox.Checked = v[2];
                shuffleEnemyHPBox.Checked = v[3];
                shuffleAllExp.Checked = v[4];
                shufflePalaceEnemies.Checked = v[5];
                v = new BitArray(new int[] { flags.IndexOf(flagText[8]) });

                shuffleAtkExp.Checked = v[0];
                lifeExpNeeded.Checked = v[1];
                magicExpNeeded.Checked = v[2];
                upaBox.Checked = v[3];
                gpBox.Checked = v[4];

                tbirdBox.Checked = v[5];
                v = new BitArray(new int[] { flags.IndexOf(flagText[9]) });

                w[0] = v[0];
                w[1] = v[1];
                w[2] = v[2];
                w.CopyTo(array, 0);
                magEffBox.SelectedIndex = array[0];
                stealExpBox.Checked = v[3];
                stealExpAmt.Checked = v[4];
                lifeRefilBox.Checked = v[5];
                v = new BitArray(new int[] { flags.IndexOf(flagText[10]) });

                v[0] = swordImmuneBox.Checked = v[0];
                v[1] = jumpNormalbox.Checked = v[1];

                w[0] = v[2];
                w[1] = v[3];
                w[2] = v[4];
                w.CopyTo(array, 0);
                numGemsCbo.SelectedIndex = array[0];

                mixEnemies.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[11]) });

                palaceItemBox.Checked = v[0];
                overworldItemBox.Checked = v[1];
                mixItemBox.Checked = v[2];
                shuffleSmallItemsBox.Checked = v[3];
                shuffleSpellLocationsBox.Checked = v[4];
                disableJarBox.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[12]) });
                w[0] = v[0];
                w[1] = v[1];
                w[2] = v[2];
                w.CopyTo(array, 0);
                lifeEffBox.SelectedIndex = array[0];
                kasutoBox.Checked = v[3];
                communityBox.Checked = v[4];
                enemyPalette.Checked = v[5];
                w = new BitArray(4);
                v = new BitArray(new int[] { flags.IndexOf(flagText[13]) });
                w[0] = v[0];
                w[1] = v[1];
                w[2] = v[2];
                w[3] = v[3];
                w.CopyTo(array, 0);
                maxHeartsBox.SelectedIndex = array[0];
                w = new BitArray(2);
                w[0] = v[4];
                w[1] = v[5];
                w.CopyTo(array, 0);
                hpCmbo.SelectedIndex = array[0];

                v = new BitArray(new int[] { flags.IndexOf(flagText[14]) });
                w = new BitArray(2);
                w[0] = v[0];
                w[1] = v[1];
                w.CopyTo(array, 0);

                hideKasutoBox.SelectedIndex = array[0];
                enemyDropBox.Checked = v[2];
                spellItemBox.Checked = v[3];
                smallBlueJar.Checked = v[4];
                smallRedJar.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[15]) });

                small50.Checked = v[0];
                small100.Checked = v[1];
                small200.Checked = v[2];
                small500.Checked = v[3];
                small1up.Checked = v[4];
                smallKey.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[16]) });

                largeBlueJar.Checked = v[0];
                largeRedJar.Checked = v[1];
                large50.Checked = v[2];
                large100.Checked = v[3];
                large200.Checked = v[4];
                large500.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[17]) });

                large1up.Checked = v[0];
                largeKey.Checked = v[1];
                w = new BitArray(2);
                helpfulHints.Checked = v[2];
                spellItemHints.Checked = v[3];
                standardDrops.Checked = v[4];
                randoDrops.Checked = v[5];
                w.CopyTo(array, 0);

                v = new BitArray(new int[] { flags.IndexOf(flagText[18]) });
                shufflePbagExp.Checked = v[0];
                w = new BitArray(3);
                w[0] = v[1];
                w[1] = v[2];
                w[2] = v[3];
                w.CopyTo(array, 0);
                atkCapBox.SelectedIndex = array[0];
                w = new BitArray(3);
                w[0] = v[4];
                w[1] = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[19]) });
                w[2] = v[0];
                w.CopyTo(array, 0);
                magCapBox.SelectedIndex = array[0];
                w = new BitArray(3);
                w[0] = v[1];
                w[1] = v[2];
                w[2] = v[3];
                w.CopyTo(array, 0);
                lifeCapBox.SelectedIndex = array[0];
                scaleLevels.Checked = v[4];
                townSpellHints.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[20]) });
                w = new BitArray(2);
                w[0] = v[0];
                w[1] = v[1];
                w.CopyTo(array, 0);
                encounterBox.SelectedIndex = array[0];
                w = new BitArray(3);
                w[0] = v[2];
                w[1] = v[3];
                w[2] = v[4];
                w.CopyTo(array, 0);
                expDropBox.SelectedIndex = array[0];

                w[0] = v[5];
                v = new BitArray(new int[] { flags.IndexOf(flagText[21]) });
                w[1] = v[0];
                w[2] = v[1];
                w.CopyTo(array, 0);
                startAtkBox.SelectedIndex = array[0];
                w[0] = v[2];
                w[1] = v[3];
                w[2] = v[4];
                w.CopyTo(array, 0);
                startMagBox.SelectedIndex = array[0];
                w[0] = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[22]) });
                w[1] = v[0];
                w[2] = v[1];
                w.CopyTo(array, 0);
                startLifeBox.SelectedIndex = array[0];

                w = new BitArray(2);
                w[0] = v[2];
                w[1] = v[3];
                w.CopyTo(array, 0);
                continentConnectionBox.SelectedIndex = array[0];
                boulderConnectionBox.Checked = v[4];
                w = new BitArray(4);
                w[0] = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[23]) });
                w[1] = v[0];
                w[2] = v[1];
                w[3] = v[2];
                w.CopyTo(array, 0);
                westBiome.SelectedIndex = array[0];
                w[0] = v[3];
                w[1] = v[4];
                w[2] = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[24]) });
                w[3] = v[0];
                w.CopyTo(array, 0);
                dmBiome.SelectedIndex = array[0];
                w[0] = v[1];
                w[1] = v[2];
                w[2] = v[3];
                w[3] = v[4];
                w.CopyTo(array, 0);
                eastBiome.SelectedIndex = array[0];
                w = new BitArray(2);
                w[0] = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[25]) });
                w[1] = v[0];
                w.CopyTo(array, 0);
                mazeBiome.SelectedIndex = array[0];
                vanillaOriginalTerrain.Checked = v[1];
                shuffleHidden.Checked = v[2];
                bossItem.Checked = v[3];
                waterBoots.Checked = v[4];
                spellEnemy.Checked = v[5];

                v = new BitArray(new int[] { flags.IndexOf(flagText[26]) });
                w = new BitArray(2);
                baguBox.Checked = v[0];
                w[0] = v[1];
                customRooms.Checked = v[2];
                blockerBox.Checked = v[3];
                bossRoomBox.Checked = v[4];
                w[1] = v[5];
                w.CopyTo(array, 0);
                palaceBox.SelectedIndex = array[0];


            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid flags entered!");
            }
            dontrunhandler = false;
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            WinSparkle.win_sparkle_check_update_with_ui();
        }

        private void shuffleOverworldEnemies_CheckedChanged(object sender, EventArgs e)
        {
            if (shufflePalaceEnemies.Checked || shuffleOverworldEnemies.Checked)
            {
                mixEnemies.Enabled = true;
            }
            else
            {
                mixEnemies.Checked = false;
                mixEnemies.Enabled = false;
            }
        }

        private void shufflePalaceEnemies_CheckedChanged(object sender, EventArgs e)
        {
            if (shufflePalaceEnemies.Checked || shuffleOverworldEnemies.Checked)
            {
                mixEnemies.Enabled = true;
            }
            else
            {
                mixEnemies.Checked = false;
                mixEnemies.Enabled = false;
            }
        }

        private void wikiBtn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/digshake/z2randomizer/wiki/Home");
        }

        private void palaceItemBox_CheckedChanged(object sender, EventArgs e)
        {
            if (palaceItemBox.Checked && overworldItemBox.Checked)
            {
                mixItemBox.Enabled = true;
            }
            else
            {
                mixItemBox.Enabled = false;
                mixItemBox.Checked = false;
            }
        }

        private void overworldItemBox_CheckedChanged(object sender, EventArgs e)
        {
            if (palaceItemBox.Checked && overworldItemBox.Checked)
            {
                mixItemBox.Enabled = true;
            }
            else
            {
                mixItemBox.Enabled = false;
                mixItemBox.Checked = false;
            }

            if (overworldItemBox.Checked)
            {
                pbagItemShuffleBox.Enabled = true;
            }
            else
            {
                pbagItemShuffleBox.Enabled = false;
                pbagItemShuffleBox.Checked = false;
            }
        }













        private void beginnerFlags(object sender, EventArgs e)
        {
            flagBox.Text = "jhEhMROm7DZ$MHRBTNBhBAh0PSm";
        }

        private void swissFlags(object sender, EventArgs e)
        {
            flagBox.Text = "jAhAD0L9$Za$LpTBT!BANAASESC";
        }

        private void bracketFlags(object sender, EventArgs e)
        {
            flagBox.Text = "hAhAD0j9$Z8$Jp5HgRBANAASESC";
        }

        private void finalsFlags(object sender, EventArgs e)
        {
            flagBox.Text = "hAhAC0j9x78gJqXBTRAANAASESC";
        }

        private void maxRandoFlags(object sender, EventArgs e)
        {
            flagBox.Text = "iyAqh$j#g7@$ZqTBT!BhOAdEzx@";
        }

        private void fourGemFlags(object sender, EventArgs e)
        {
            flagBox.Text = "jhAhDcC#$Zz$JHRBTuAhOAy0PB@";
        }

        private void tbirdBox_CheckedChanged(object sender, EventArgs e)
        {
            if (tbirdBox.Checked)
            {
                removeTbird.Enabled = false;
                removeTbird.Checked = false;
            }
            else
            {
                removeTbird.Enabled = true;
            }
        }

        private void removeTbird_CheckedChanged(object sender, EventArgs e)
        {
            if (removeTbird.Checked)
            {
                tbirdBox.Enabled = false;
                tbirdBox.Checked = false;
            }
            else
            {
                tbirdBox.Enabled = true;
            }
        }

        private void palaceSwapBox_CheckedChanged(object sender, EventArgs e)
        {
            if (palaceSwapBox.Checked)
            {
                p7Shuffle.Enabled = true;
            }
            else
            {
                p7Shuffle.Checked = false;
                p7Shuffle.Enabled = false;
            }

        }

        private void Bulk_Generate_Click(object sender, EventArgs e)
        {
            String flagString = flagBox.Text;

            if (flagString.Length != numFlags)
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
                props = generateProps();
                f3 = new GeneratingSeedsForm();
                f3.Show();
                int i = 0;
                spawnNextSeed = true;
                while (i < numSeeds)
                {

                    f3.Text = "Generating seed " + (i + 1) + " of " + numSeeds + "...";
                    
                
                    props.seed = r.Next(1000000000);
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

        private RandomizerProperties generateProps()
        {
            RandomizerProperties props = new RandomizerProperties();
            props.filename = fileTextBox.Text;

            try
            {
                props.seed = Int32.Parse(seedTextBox.Text);
            }
            catch (Exception ex)
            {
                props.seed = 0;
            }

            props.shuffleItems = shuffleItemBox.Checked;
            props.startCandle = candleBox.Checked;
            props.startGlove = gloveBox.Checked;
            props.startRaft = raftBox.Checked;
            props.startBoots = bootsBox.Checked;
            props.startFlute = fluteBox.Checked;
            props.startCross = crossBox.Checked;
            props.startHammer = hammerBox.Checked;
            props.startKey = keyBox.Checked;
            props.shuffleSpells = spellShuffleBox.Checked;
            props.startShield = shieldBox.Checked;
            props.startJump = jumpBox.Checked;
            props.startLife = lifeBox.Checked;
            props.startFairy = fairyBox.Checked;
            props.startFire = fireBox.Checked;
            props.startReflect = reflectBox.Checked;
            props.startSpell = spellBox.Checked;
            props.startThunder = thunderBox.Checked;
            props.startHearts = heartCmbo.GetItemText(heartCmbo.SelectedItem);
            props.maxHearts = maxHeartsBox.GetItemText(maxHeartsBox.SelectedItem);
            props.startTech = techCmbo.GetItemText(techCmbo.SelectedItem);
            props.startGems = numGemsCbo.GetItemText(numGemsCbo.SelectedItem);
            props.shuffleLives = livesBox.Checked;
            props.shufflePalaceRooms = palaceBox.SelectedIndex == 1;
            props.shuffleEnemyHP = shuffleEnemyHPBox.Checked;
            props.shuffleAllExp = shuffleAllExp.Checked;
            props.shuffleAtkExp = shuffleAtkExp.Checked;
            props.shuffleLifeExp = lifeExpNeeded.Checked;
            props.shuffleMagicExp = magicExpNeeded.Checked;
            props.shuffleEnemyStealExp = stealExpBox.Checked;
            props.shuffleStealExpAmt = stealExpAmt.Checked;
            props.shuffleSwordImmunity = swordImmuneBox.Checked;
            props.jumpAlwaysOn = jumpNormalbox.Checked;
            props.shuffleLifeRefill = lifeRefilBox.Checked;
            props.disableBeep = disableLowHealthBeep.Checked;
            props.requireTbird = tbirdBox.Checked;
            props.shuffleEncounters = shuffleEncounters.Checked;
            props.allowPathEnemies = allowPathEnemies.Checked;
            props.shuffleOverworldEnemies = shuffleOverworldEnemies.Checked;
            props.shufflePalaceEnemies = shufflePalaceEnemies.Checked;
            props.mixEnemies = mixEnemies.Checked;
            props.flags = flagBox.Text;
            props.shuffleOverworldItems = overworldItemBox.Checked;
            props.shufflePalaceItems = palaceItemBox.Checked;
            props.mixOverworldPalaceItems = mixItemBox.Checked;
            props.shuffleSmallItems = shuffleSmallItemsBox.Checked;
            props.shuffleSpellLocations = shuffleSpellLocationsBox.Checked;
            props.disableMagicRecs = disableJarBox.Checked;
            props.extraKeys = palaceKeys.Checked;
            props.palacePalette = palacePalette.Checked;
            props.pbagDrop = pbagDrop.Checked;
            props.swapPalaceCont = palaceSwapBox.Checked;
            props.shuffleAtkEff = atkEffBox.SelectedIndex == 0;
            props.lowAtk = atkEffBox.SelectedIndex == 1;
            props.highAtk = atkEffBox.SelectedIndex == 3;
            props.ohkoEnemies = atkEffBox.SelectedIndex == 4;

            props.shuffleMagEff = magEffBox.SelectedIndex == 0;
            props.highMag = magEffBox.SelectedIndex == 1;
            props.lowMag = magEffBox.SelectedIndex == 3;
            props.wizardMode = magEffBox.SelectedIndex == 4;

            props.shuffleLifeEff = lifeEffBox.SelectedIndex == 0;
            props.ohkoLink = lifeEffBox.SelectedIndex == 1;
            props.highDef = lifeEffBox.SelectedIndex == 3;
            props.tankMode = lifeEffBox.SelectedIndex == 4;
            props.upaBox = upaBox.Checked;
            props.shortenGP = gpBox.Checked;
            props.fastCast = fastSpellBox.Checked;
            props.kasutoJars = kasutoBox.Checked;
            props.useCommunityHints = communityBox.Checked;
            props.combineFire = combineFireBox.Checked;
            props.removeTbird = removeTbird.Checked;
            props.permanentBeam = beamBox.Checked;
            props.beamSprite = beamCmbo.GetItemText(beamCmbo.SelectedItem);
            props.pbagItemShuffle = pbagItemShuffleBox.Checked;
            props.p7shuffle = p7Shuffle.Checked;
            props.shuffleDripper = shuffleDripper.Checked;
            props.shuffleEnemyPalettes = enemyPalette.Checked;
            props.hiddenPalace = hpCmbo.GetItemText(hpCmbo.SelectedItem);
            props.disableMusic = disableMusicBox.Checked;
            props.hiddenKasuto = hideKasutoBox.GetItemText(hideKasutoBox.SelectedItem);
            props.ShuffleEnemyDrops = enemyDropBox.Checked;
            props.charSprite = spriteCmbo.GetItemText(spriteCmbo.SelectedItem);
            props.tunicColor = tunicColor.GetItemText(tunicColor.SelectedItem);
            props.shieldColor = shieldColor.GetItemText(shieldColor.SelectedItem);
            props.removeSpellItems = spellItemBox.Checked;
            props.smallbluejar = smallBlueJar.Checked;
            props.smallredjar = smallRedJar.Checked;
            props.small50 = small50.Checked;
            props.small100 = small100.Checked;
            props.small200 = small200.Checked;
            props.small500 = small500.Checked;
            props.small1up = small1up.Checked;
            props.smallkey = smallKey.Checked;
            props.largebluejar = largeBlueJar.Checked;
            props.largeredjar = largeRedJar.Checked;
            props.large50 = large50.Checked;
            props.large100 = large100.Checked;
            props.large200 = large200.Checked;
            props.large500 = large500.Checked;
            props.large1up = large1up.Checked;
            props.largekey = largeKey.Checked;
            props.standardizeDrops = standardDrops.Checked;
            props.randoDrops = randoDrops.Checked;
            props.shufflePbagXp = shufflePbagExp.Checked;
            props.townSwap = false;
            props.attackCap = Int32.Parse(atkCapBox.GetItemText(atkCapBox.SelectedItem));
            props.magicCap = Int32.Parse(magCapBox.GetItemText(magCapBox.SelectedItem));
            props.lifeCap = Int32.Parse(lifeCapBox.GetItemText(lifeCapBox.SelectedItem));
            props.scaleLevels = scaleLevels.Checked;
            props.spellItemHints = spellItemHints.Checked;
            props.helpfulHints = helpfulHints.Checked;
            props.townNameHints = townSpellHints.Checked;
            props.encounterRate = encounterBox.GetItemText(encounterBox.SelectedItem);
            props.expLevel = expDropBox.GetItemText(expDropBox.SelectedItem);
            props.startAtk = Int32.Parse(startAtkBox.GetItemText(startAtkBox.SelectedItem));
            props.startMag = Int32.Parse(startMagBox.GetItemText(startMagBox.SelectedItem));
            props.startLifeLvl = Int32.Parse(startLifeBox.GetItemText(startLifeBox.SelectedItem));
            props.continentConnections = continentConnectionBox.GetItemText(continentConnectionBox.SelectedItem);
            props.upAC1 = upAC1.Checked;
            props.hideLocs = hideLocsBox.Checked;
            props.saneCaves = saneCaveShuffleBox.Checked;
            props.boulderBlockConnections = boulderConnectionBox.Checked;
            props.westBiome = westBiome.GetItemText(westBiome.SelectedItem);
            props.eastBiome = eastBiome.GetItemText(eastBiome.SelectedItem);
            props.mazeBiome = mazeBiome.GetItemText(mazeBiome.SelectedItem);
            props.dmBiome = dmBiome.GetItemText(dmBiome.SelectedItem);
            props.removeFlashing = flashingOff.Checked;
            props.vanillaOriginal = vanillaOriginalTerrain.Checked;
            props.shuffleHidden = shuffleHidden.Checked;
            props.bossItem = bossItem.Checked;
            props.canWalkOnWaterWithBoots = waterBoots.Checked;
            props.spellEnemy = spellEnemy.Checked;
            props.bagusWoods = baguBox.Checked;
            props.createPalaces = palaceBox.SelectedIndex == 2;
            props.customRooms = customRooms.Checked;
            props.blockersAnywhere = blockerBox.Checked;
            props.bossRoomConnect = bossRoomBox.Checked;
            props.dashSpell = dashBox.Checked;
            return props;
        }

        private void enemyDropBox_CheckedChanged(object sender, EventArgs e)
        {
            if(enemyDropBox.Checked)
            {
                for (int i = 0; i < small.Count(); i++)
                {
                    small[i].Enabled = true;
                    large[i].Enabled = true;
                }
                randoDrops.Enabled = false;
            }
            else
            {
                for (int i = 0; i < small.Count(); i++)
                {
                    small[i].Enabled = false;
                    large[i].Enabled = false;
                }
                randoDrops.Enabled = true;
            }
        }

        private void atLeastOneChecked(object sender, EventArgs e)
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
            customBox1.Text = flagBox.Text;
            Properties.Settings.Default.custom1 = flagBox.Text;
            Properties.Settings.Default.Save();
        }

        private void customLoad1_Click(object sender, EventArgs e)
        {
            flagBox.Text = customBox1.Text;
        }

        private void customSave2_Click(object sender, EventArgs e)
        {
            customBox2.Text = flagBox.Text;
            Properties.Settings.Default.custom2 = flagBox.Text;
            Properties.Settings.Default.Save();
        }

        private void customSave3_Click(object sender, EventArgs e)
        {
            customBox3.Text = flagBox.Text;
            Properties.Settings.Default.custom3 = flagBox.Text;
            Properties.Settings.Default.Save();
        }

        private void customLoad2_Click(object sender, EventArgs e)
        {
            flagBox.Text = customBox2.Text;
        }

        private void customLoad3_Click(object sender, EventArgs e)
        {
            flagBox.Text = customBox3.Text;
        }

        private void randoDrops_CheckedChanged(object sender, EventArgs e)
        {
            if(randoDrops.Checked)
            {
                enemyDropBox.Enabled = false;
            }
            else
            {
                enemyDropBox.Enabled = true;
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
            if(!atkCapBox.GetItemText(atkCapBox.SelectedItem).Equals("8") || !magCapBox.GetItemText(magCapBox.SelectedItem).Equals("8") || !lifeCapBox.GetItemText(lifeCapBox.SelectedItem).Equals("8"))
            {
                scaleLevels.Enabled = true;
            }
            else
            {
                scaleLevels.Enabled = false;
                scaleLevels.Checked = false;
            }
        }

        private void spellItemBox_CheckedChanged(object sender, EventArgs e)
        {
            if(!spellItemBox.Checked)
            {
                spellItemHints.Enabled = false;
                spellItemHints.Checked = false;
            }
            else
            {
                spellItemHints.Enabled = true;
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
            if (eastBiome.SelectedIndex == 0 || (hpCmbo.SelectedIndex == 0 && hideKasutoBox.SelectedIndex == 0))
            {

                shuffleHidden.Enabled = false;
                shuffleHidden.Checked = false;
            }
            else
            {
                shuffleHidden.Enabled = true;
            }
        }

        private void checkVanillaPossible()
        {
            if(vanillaPossible(eastBiome) || vanillaPossible(westBiome) || vanillaPossible(dmBiome) || vanillaPossible(mazeBiome))
            {
                vanillaOriginalTerrain.Enabled = true;
            }
            else
            {
                vanillaOriginalTerrain.Enabled = false;
                vanillaOriginalTerrain.Checked = false;
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
                baguBox.Checked = false;
                baguBox.Enabled = false;
            }
            else
            {
                baguBox.Enabled = true;
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
            
            new Hyrule(props, worker);
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
            if(palaceBox.SelectedIndex != 2)
            {
                customRooms.Enabled = false;
                customRooms.Checked = false;
                blockerBox.Enabled = false;
                blockerBox.Checked = false;
                bossRoomBox.Checked = false;
                bossRoomBox.Enabled = false;
            }
            else
            {
                customRooms.Enabled = true;
                blockerBox.Enabled = true;
                bossRoomBox.Enabled = true;
            }

            if (palaceBox.SelectedIndex != 0)
            {
                gpBox.Enabled = true;
                tbirdBox.Enabled = true;
            }
            else
            {
                gpBox.Checked = false;
                gpBox.Enabled = false;
                tbirdBox.Enabled = false;
                tbirdBox.Checked = true;
            }
        }

        private void dashBox_CheckedChanged(object sender, EventArgs e)
        {
            if(dashBox.Checked)
            {
                combineFireBox.Enabled = false;
                combineFireBox.Checked = false;
            }
            else
            {
                combineFireBox.Enabled = true;
            }
        }

        private void combineFireBox_CheckedChanged(object sender, EventArgs e)
        {
            if (combineFireBox.Checked)
            {
                dashBox.Enabled = false;
                dashBox.Checked = false;
            }
            else
            {
                dashBox.Enabled = true;
            }
        }
    }
}

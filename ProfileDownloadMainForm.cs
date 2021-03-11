using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace RPProfileDownloader
{
    public partial class ProfileDownloadMainForm : Form
    {
        private enum RunModeType {
            Instant = 0,
            ManualOnly = -1,
            Automatic = 10000,
        };
        private readonly int instantTimeout = 10000; // Used when the run mode is set to Instant - gives a grace period
        private readonly KeyValuePair<string, RunModeType>[] intervals = {
                new KeyValuePair<string, RunModeType>("No", RunModeType.Instant),
                new KeyValuePair<string, RunModeType>("Manual Only", RunModeType.ManualOnly),
                new KeyValuePair<string, RunModeType>("Automatic", RunModeType.Automatic),
        };
        private bool ESOrunning = false;
        private RunModeType runMode = RunModeType.Automatic;
        private DateTime dayTimer = DateTime.UtcNow;
        public ProfileDownloadMainForm()
        {
            InitializeComponent();

            foreach (KeyValuePair<string, RunModeType> curPair in intervals)
            {
                ToolStripMenuItem newItem = new ToolStripMenuItem(curPair.Key);
                newItem.Checked = ((int)curPair.Value == Properties.Settings.Default.UpdateInterval);
                mnuUpdateIntervals.Items.Add(newItem);
            }

            runMode = (RunModeType)Properties.Settings.Default.UpdateInterval;

            // Safety valve - Make sure runMode has a valid value.
            switch (runMode)
            {
                case RunModeType.Instant:
                case RunModeType.ManualOnly:
                case RunModeType.Automatic:
                    break;
                default:
                    runMode = RunModeType.Automatic;
                    break;
            }
        }

        /// <summary>
        /// Fires any necessary logic for updating profile data.
        /// </summary>
        public void UpdateProfileData()
        {
            notShowMe.ShowBalloonTip(5000);
            ProfileManager.UpdateProfiles();
        }

        /// <summary>
        /// Ensures automatic update timer is configured correctly.  Make sure this is only used if runMode is set to automatic!!!
        /// </summary>
        public void SetTimer()
        {
            if (!tmrClock.Enabled)
            {
                tmrClock.Interval = Properties.Settings.Default.UpdateInterval;
                tmrClock.Start();
            }
            else if (tmrClock.Interval != Properties.Settings.Default.UpdateInterval)
            {
                tmrClock.Stop();
                tmrClock.Interval = Properties.Settings.Default.UpdateInterval;
                tmrClock.Start();
            }
        }

        /// <summary>
        /// Used by Automatic run mode, updates profile data if ESO has been started.
        /// </summary>
        public void DetectESOAndUpdate()
        {
            if (DateTime.UtcNow.Day != dayTimer.Day)
            {
                UpdateProfileData();
                dayTimer = DateTime.UtcNow;
            }
            else
            {
                // Check to see if ESO is running.
                Process[] instances = Process.GetProcessesByName("eso64");

                // Failsafe - try for 32-bit version.
                if (instances.Length == 0)
                    instances = Process.GetProcessesByName("eso");

                if (instances.Length > 0)
                {
                    if (!ESOrunning)
                        UpdateProfileData();

                    ESOrunning = true;
                }
                else
                    ESOrunning = false;
            }

            SetTimer();
        }

        private void ProfileDownloadMainForm_Load(object sender, EventArgs e)
        {
            // A bit counter-intuitive, but Instant also uses the timer to ensure a grace period for changing settings.
            tmrClock.Interval = Math.Max(Properties.Settings.Default.UpdateInterval, instantTimeout);

            if (!Properties.Settings.Default.hasRunBefore)
            {
                if (MessageBox.Show("Thank you for downloading the RP Profile Viewer addon!  This monitor program will need to be used to keep player profile information up to date.\n\nWould you like to place a shortcut on your desktop?", "First Time Setup", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Adapted from https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop
                    WshShell wsh = new WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\RP Profile Viewer Monitor.lnk") as IWshRuntimeLibrary.IWshShortcut;
                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.WindowStyle = 1;
                    shortcut.Description = "Profile Monitor program for the RP Profile Viewer ESO addon.";
                    shortcut.WorkingDirectory = Application.StartupPath;
                    shortcut.Save();
                }
            }

            if (runMode == RunModeType.Instant)
            {
                // Perform an immediate update, then timeout.
                UpdateProfileData();
                tmrClock.Interval = instantTimeout;
                tmrClock.Start();
            }
            else if (!Properties.Settings.Default.hasRunBefore)
            {
                UpdateProfileData();

                if (runMode == RunModeType.Automatic)
                    SetTimer();
            }
            else if (runMode == RunModeType.Automatic)
                DetectESOAndUpdate();

            Properties.Settings.Default.hasRunBefore = true;
        }

        private void ProfileDownloadMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void mnuTaskbar_Opening(object sender, CancelEventArgs e)
        {
            mniUpdateNow.Enabled = !ProfileManager.working;
        }

        private void mnuTaskbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch(e.ClickedItem.Text)
            {
                case "ESO Rollplay Site":
                    // Stack Overflow sorcery.
                    Process.Start(new ProcessStartInfo("cmd", $"/c start https://eso-rollplay.net") { CreateNoWindow = true });
                    break;
                case "Update Profiles Now":
                    // Only update if the manager isn't already working.
                    if (!ProfileManager.working)
                    {
                        UpdateProfileData();

                        // Reset the clock if running automatically.
                        if (runMode == RunModeType.Automatic)
                        {
                            tmrClock.Stop();
                            tmrClock.Start();
                        }
                    }
                    break;
                case "Update Options":
                    // Do nothing.
                    break;
                default:
                    // If the program is busy, wait to exit.
                    if ((ProfileManager.working) || (ImageConverter.working))
                    {
                        runMode = RunModeType.Instant;
                        tmrClock.Stop();
                        tmrClock.Interval = 1000;
                        tmrClock.Start();
                        mnuTaskbar.Enabled = false;
                    }
                    else
                        Application.Exit();
                    break;
            }
        }

        /// <summary>
        /// Uses the key/values from the intervals array to set the update interval.
        /// </summary>
        private void mnuUpdateIntervals_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            KeyValuePair<string, RunModeType> result = intervals.FirstOrDefault(item => item.Key == e.ClickedItem.Text);

            if ((result.Value == RunModeType.Instant) && (MessageBox.Show("Setting updates to 'No' will make the updater run immediately and then close.  If you select this option, you will need to run the program yourself whenever you want to update profile information.\n\nIf you want to change this setting later, you will have 10 seconds whenever the program runs.\n\nAre you sure you want to proceed?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No))
                    return;

            runMode = result.Value;
            Properties.Settings.Default.UpdateInterval = (int)runMode;

            foreach (ToolStripMenuItem curItem in mnuUpdateIntervals.Items)
                curItem.Checked = (result.Key == curItem.Text);

            switch(runMode)
            {
                case RunModeType.Instant:
                    // Perform an immediate update, then timeout.
                    tmrClock.Stop();
                    UpdateProfileData();
                    tmrClock.Interval = instantTimeout;
                    tmrClock.Start();
                    break;
                case RunModeType.ManualOnly:
                    tmrClock.Stop();
                    break;
                default:
                    DetectESOAndUpdate();
                    break;
            }
        }

        /// <summary>
        /// Handles the timing of updating profile data.
        /// </summary>
        private void tmrClock_Tick(object sender, EventArgs e)
        {
            if (runMode == RunModeType.Automatic)
                DetectESOAndUpdate();
            // Keep the program alive if it's still working...
            else if ((ProfileManager.working) || (ImageConverter.working))
            {
                tmrClock.Stop();
                tmrClock.Interval = 1000;
                tmrClock.Start();
            }
            else // ...otherwise, kill it.
                Application.Exit();
        }
    }
}

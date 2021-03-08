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

namespace RPProfileDownloader
{
    public partial class ProfileDownloadMainForm : Form
    {
        private bool ESOrunning = false;
        private readonly int minTimer = 10000; // The minimum timer value.  If the update interval is lower than this, the program will terminate.
        private readonly KeyValuePair<string, int>[] intervals = {
                new KeyValuePair<string, int>("Never", 0),
                new KeyValuePair<string, int>("1 Minute", 60000),
                new KeyValuePair<string, int>("5 Minutes", 300000),
                new KeyValuePair<string, int>("30 Minutes", 1800000),
                new KeyValuePair<string, int>("1 Hour", 3600000)
        };
        public ProfileDownloadMainForm()
        {
            InitializeComponent();
            // Use minTimer here to force the program to stay open for a time, so the user can change the interval if they want.
            tmrClock.Interval = Math.Max(Properties.Settings.Default.UpdateInterval, minTimer);

            foreach(KeyValuePair<string, int> curPair in intervals)
            {
                ToolStripMenuItem newItem = new ToolStripMenuItem(curPair.Key);
                newItem.Checked = (curPair.Value == Properties.Settings.Default.UpdateInterval);
                mnuUpdateIntervals.Items.Add(newItem);
            }
        }

        /// <summary>
        /// Determines if profile data needs to be updated.
        /// </summary>
        public void TryUpdateProfileData()
        {
            // Force an update if the interval is below min (i.e set to Never)
            if (Properties.Settings.Default.UpdateInterval <= minTimer)
            {
                UpdateProfileData();
                return;
            }

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

        /// <summary>
        /// Fires any necessary logic for updating profile data.
        /// </summary>
        public void UpdateProfileData()
        {
            notShowMe.ShowBalloonTip(5000);
            ProfileManager.UpdateProfiles();
        }

        private void ProfileDownloadMainForm_Load(object sender, EventArgs e)
        {
            TryUpdateProfileData();
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
                        tmrClock.Stop();
                        tmrClock.Start();
                    }
                    break;
                case "Update Interval":
                    // Do nothing.
                    break;
                default:
                    Application.Exit();
                    break;
            }
        }

        /// <summary>
        /// Uses the key/values from the intervals array to set the update interval.
        /// </summary>
        private void mnuUpdateIntervals_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            KeyValuePair<string, int> result = intervals.FirstOrDefault(item => item.Key == e.ClickedItem.Text);

            if ((result.Value <= minTimer) && (MessageBox.Show("Setting the update timer to 'Never' will not allow this program to download profile information automatically!\n\nIf you select this option, you will need to run the program yourself to update profile information.\n\nAre you sure you want to proceed?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No))
                    return;

            Properties.Settings.Default.UpdateInterval = result.Value;

            foreach (ToolStripMenuItem curItem in mnuUpdateIntervals.Items)
            {
                curItem.Checked = (result.Key == curItem.Text);
            }

            if (result.Value <= minTimer)
            {
                TryUpdateProfileData();
                Application.Exit();
            }
        }

        /// <summary>
        /// Handles the timing of updating profile data.  An interval of minTimer or less means the program shouldn't be looking for updates and should close.
        /// </summary>
        private void tmrClock_Tick(object sender, EventArgs e)
        {
            // If the interval is set to a valid value, try pulling the data before continuing.
            if (Properties.Settings.Default.UpdateInterval > minTimer)
            {
                TryUpdateProfileData();

                if (tmrClock.Interval != Properties.Settings.Default.UpdateInterval)
                {
                    tmrClock.Stop();
                    tmrClock.Interval = Properties.Settings.Default.UpdateInterval;
                    tmrClock.Start();
                }
            }
            else // Kill the program.
            {
                Application.Exit();
            }
        }
    }
}

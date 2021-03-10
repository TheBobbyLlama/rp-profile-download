
namespace RPProfileDownloader
{
    partial class ProfileDownloadMainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileDownloadMainForm));
            this.tmrClock = new System.Windows.Forms.Timer(this.components);
            this.notShowMe = new System.Windows.Forms.NotifyIcon(this.components);
            this.mnuTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniVisitRollplay = new System.Windows.Forms.ToolStripMenuItem();
            this.mniUpdateNow = new System.Windows.Forms.ToolStripMenuItem();
            this.mniUpdateOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUpdateIntervals = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTaskbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrClock
            // 
            this.tmrClock.Interval = 60000;
            this.tmrClock.Tick += new System.EventHandler(this.tmrClock_Tick);
            // 
            // notShowMe
            // 
            this.notShowMe.BalloonTipText = "Downloading character profiles...";
            this.notShowMe.BalloonTipTitle = "ESO RP Profiles";
            this.notShowMe.ContextMenuStrip = this.mnuTaskbar;
            this.notShowMe.Icon = ((System.Drawing.Icon)(resources.GetObject("notShowMe.Icon")));
            this.notShowMe.Text = "ESO RP Profile Monitor";
            this.notShowMe.Visible = true;
            // 
            // mnuTaskbar
            // 
            this.mnuTaskbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniVisitRollplay,
            this.mniUpdateNow,
            this.mniUpdateOptions,
            this.mniExit});
            this.mnuTaskbar.Name = "mnuTaskbar";
            this.mnuTaskbar.Size = new System.Drawing.Size(183, 92);
            this.mnuTaskbar.Opening += new System.ComponentModel.CancelEventHandler(this.mnuTaskbar_Opening);
            this.mnuTaskbar.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mnuTaskbar_ItemClicked);
            // 
            // mniVisitRollplay
            // 
            this.mniVisitRollplay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.mniVisitRollplay.Name = "mniVisitRollplay";
            this.mniVisitRollplay.Size = new System.Drawing.Size(182, 22);
            this.mniVisitRollplay.Text = "ESO Rollplay Site";
            // 
            // mniUpdateNow
            // 
            this.mniUpdateNow.Name = "mniUpdateNow";
            this.mniUpdateNow.Size = new System.Drawing.Size(182, 22);
            this.mniUpdateNow.Text = "Update Profiles Now";
            // 
            // mniUpdateOptions
            // 
            this.mniUpdateOptions.DropDown = this.mnuUpdateIntervals;
            this.mniUpdateOptions.Name = "mniUpdateOptions";
            this.mniUpdateOptions.Size = new System.Drawing.Size(182, 22);
            this.mniUpdateOptions.Text = "Update Options";
            // 
            // mnuUpdateIntervals
            // 
            this.mnuUpdateIntervals.Name = "mnuUpdateIntervals";
            this.mnuUpdateIntervals.OwnerItem = this.mniUpdateOptions;
            this.mnuUpdateIntervals.Size = new System.Drawing.Size(61, 4);
            this.mnuUpdateIntervals.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mnuUpdateIntervals_ItemClicked);
            // 
            // mniExit
            // 
            this.mniExit.Name = "mniExit";
            this.mniExit.Size = new System.Drawing.Size(182, 22);
            this.mniExit.Text = "Exit";
            // 
            // ProfileDownloadMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(158, 140);
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProfileDownloadMainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ESO - RP Profile Downloader";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfileDownloadMainForm_FormClosing);
            this.Load += new System.EventHandler(this.ProfileDownloadMainForm_Load);
            this.mnuTaskbar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrClock;
        private System.Windows.Forms.NotifyIcon notShowMe;
        private System.Windows.Forms.ContextMenuStrip mnuTaskbar;
        private System.Windows.Forms.ToolStripMenuItem mniUpdateNow;
        private System.Windows.Forms.ToolStripMenuItem mniExit;
        private System.Windows.Forms.ToolStripMenuItem mniUpdateOptions;
        private System.Windows.Forms.ContextMenuStrip mnuUpdateIntervals;
        private System.Windows.Forms.ToolStripMenuItem mniVisitRollplay;
    }
}


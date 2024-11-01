namespace SportClassAnalyzer
{
    partial class frmMain
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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            openRaceBoxFileToolStripMenuItem = new ToolStripMenuItem();
            raceOptionsToolStripMenuItem = new ToolStripMenuItem();
            selectCourseToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, raceOptionsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1391, 28);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, openRaceBoxFileToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(216, 26);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openRaceBoxFileToolStripMenuItem
            // 
            openRaceBoxFileToolStripMenuItem.Name = "openRaceBoxFileToolStripMenuItem";
            openRaceBoxFileToolStripMenuItem.Size = new Size(216, 26);
            openRaceBoxFileToolStripMenuItem.Text = "Open RaceBox File";
            openRaceBoxFileToolStripMenuItem.Click += openRaceBoxFileToolStripMenuItem_Click;
            // 
            // raceOptionsToolStripMenuItem
            // 
            raceOptionsToolStripMenuItem.CheckOnClick = true;
            raceOptionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectCourseToolStripMenuItem, toolStripSeparator1 });
            raceOptionsToolStripMenuItem.Name = "raceOptionsToolStripMenuItem";
            raceOptionsToolStripMenuItem.Size = new Size(111, 24);
            raceOptionsToolStripMenuItem.Text = "Race Options";
            // 
            // selectCourseToolStripMenuItem
            // 
            selectCourseToolStripMenuItem.Name = "selectCourseToolStripMenuItem";
            selectCourseToolStripMenuItem.Size = new Size(182, 26);
            selectCourseToolStripMenuItem.Text = "Options Form";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(179, 6);
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1391, 450);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "frmMain";
            Text = "Race Analyzer";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem openRaceBoxFileToolStripMenuItem;
        private ToolStripMenuItem raceOptionsToolStripMenuItem;
        private ToolStripMenuItem selectCourseToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
    }
}

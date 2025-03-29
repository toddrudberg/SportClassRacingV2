namespace SportClassAnalyzer
{
    partial class frmLeaderBoard
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
            txtLeaderBoard = new TextBox();
            SuspendLayout();
            // 
            // txtLeaderBoard
            // 
            txtLeaderBoard.Font = new Font("Consolas", 10.125F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLeaderBoard.Location = new Point(47, 40);
            txtLeaderBoard.Multiline = true;
            txtLeaderBoard.Name = "txtLeaderBoard";
            txtLeaderBoard.Size = new Size(880, 764);
            txtLeaderBoard.TabIndex = 0;
            // 
            // frmLeaderBoard
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(953, 883);
            Controls.Add(txtLeaderBoard);
            Name = "frmLeaderBoard";
            Text = "frmLeaderBoard";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLeaderBoard;
    }
}
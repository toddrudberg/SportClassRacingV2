namespace SportClassAnalyzer
{
    partial class frmOptions
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
            gpSelectCourse = new GroupBox();
            checkShowStartLap = new CheckBox();
            radioInnerCourse = new RadioButton();
            radioMiddleCourse = new RadioButton();
            radioOuterCourse = new RadioButton();
            gpImageOffset = new GroupBox();
            btnApplyOffset = new Button();
            numOffsetY = new NumericUpDown();
            numOffsetX = new NumericUpDown();
            lblOffsetY = new Label();
            lblOffsetX = new Label();
            gpImageScale = new GroupBox();
            numScaleY = new NumericUpDown();
            numScaleX = new NumericUpDown();
            lblScaleY = new Label();
            lblScaleX = new Label();
            gpSelectCourse.SuspendLayout();
            gpImageOffset.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOffsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numOffsetX).BeginInit();
            gpImageScale.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numScaleY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numScaleX).BeginInit();
            SuspendLayout();
            // 
            // gpSelectCourse
            // 
            gpSelectCourse.Controls.Add(checkShowStartLap);
            gpSelectCourse.Controls.Add(radioInnerCourse);
            gpSelectCourse.Controls.Add(radioMiddleCourse);
            gpSelectCourse.Controls.Add(radioOuterCourse);
            gpSelectCourse.Location = new Point(44, 45);
            gpSelectCourse.Margin = new Padding(5, 5, 5, 5);
            gpSelectCourse.Name = "gpSelectCourse";
            gpSelectCourse.Padding = new Padding(5, 5, 5, 5);
            gpSelectCourse.Size = new Size(406, 258);
            gpSelectCourse.TabIndex = 0;
            gpSelectCourse.TabStop = false;
            gpSelectCourse.Text = "Select Course";
            // 
            // checkShowStartLap
            // 
            checkShowStartLap.AutoSize = true;
            checkShowStartLap.Location = new Point(32, 200);
            checkShowStartLap.Margin = new Padding(5, 5, 5, 5);
            checkShowStartLap.Name = "checkShowStartLap";
            checkShowStartLap.Size = new Size(203, 36);
            checkShowStartLap.TabIndex = 3;
            checkShowStartLap.Text = "Show Start Lap";
            checkShowStartLap.UseVisualStyleBackColor = true;
            checkShowStartLap.CheckedChanged += checkShowStartLap_CheckedChanged;
            // 
            // radioInnerCourse
            // 
            radioInnerCourse.AutoSize = true;
            radioInnerCourse.Location = new Point(37, 141);
            radioInnerCourse.Margin = new Padding(5, 5, 5, 5);
            radioInnerCourse.Name = "radioInnerCourse";
            radioInnerCourse.Size = new Size(181, 36);
            radioInnerCourse.TabIndex = 2;
            radioInnerCourse.TabStop = true;
            radioInnerCourse.Text = "Inner Course";
            radioInnerCourse.UseVisualStyleBackColor = true;
            radioInnerCourse.CheckedChanged += radioInnerCourse_CheckedChanged;
            // 
            // radioMiddleCourse
            // 
            radioMiddleCourse.AutoSize = true;
            radioMiddleCourse.Location = new Point(37, 93);
            radioMiddleCourse.Margin = new Padding(5, 5, 5, 5);
            radioMiddleCourse.Name = "radioMiddleCourse";
            radioMiddleCourse.Size = new Size(201, 36);
            radioMiddleCourse.TabIndex = 1;
            radioMiddleCourse.TabStop = true;
            radioMiddleCourse.Text = "Middle Course";
            radioMiddleCourse.UseVisualStyleBackColor = true;
            radioMiddleCourse.CheckedChanged += radioMiddleCourse_CheckedChanged;
            // 
            // radioOuterCourse
            // 
            radioOuterCourse.AutoSize = true;
            radioOuterCourse.Location = new Point(37, 45);
            radioOuterCourse.Margin = new Padding(5, 5, 5, 5);
            radioOuterCourse.Name = "radioOuterCourse";
            radioOuterCourse.Size = new Size(187, 36);
            radioOuterCourse.TabIndex = 0;
            radioOuterCourse.TabStop = true;
            radioOuterCourse.Text = "Outer Course";
            radioOuterCourse.UseVisualStyleBackColor = true;
            radioOuterCourse.CheckedChanged += radioOuterCourse_CheckedChanged;
            // 
            // gpImageOffset
            // 
            gpImageOffset.Controls.Add(btnApplyOffset);
            gpImageOffset.Controls.Add(numOffsetY);
            gpImageOffset.Controls.Add(numOffsetX);
            gpImageOffset.Controls.Add(lblOffsetY);
            gpImageOffset.Controls.Add(lblOffsetX);
            gpImageOffset.Location = new Point(44, 328);
            gpImageOffset.Margin = new Padding(5, 5, 5, 5);
            gpImageOffset.Name = "gpImageOffset";
            gpImageOffset.Padding = new Padding(5, 5, 5, 5);
            gpImageOffset.Size = new Size(406, 240);
            gpImageOffset.TabIndex = 1;
            gpImageOffset.TabStop = false;
            gpImageOffset.Text = "Image Offset";
            // 
            // btnApplyOffset
            // 
            btnApplyOffset.Location = new Point(122, 176);
            btnApplyOffset.Margin = new Padding(5, 5, 5, 5);
            btnApplyOffset.Name = "btnApplyOffset";
            btnApplyOffset.Size = new Size(153, 46);
            btnApplyOffset.TabIndex = 4;
            btnApplyOffset.Text = "Apply";
            btnApplyOffset.UseVisualStyleBackColor = true;
            btnApplyOffset.Click += btnApplyOffset_Click;
            // 
            // numOffsetY
            // 
            numOffsetY.DecimalPlaces = 1;
            numOffsetY.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numOffsetY.Location = new Point(122, 112);
            numOffsetY.Margin = new Padding(5, 5, 5, 5);
            numOffsetY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numOffsetY.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numOffsetY.Name = "numOffsetY";
            numOffsetY.Size = new Size(244, 39);
            numOffsetY.TabIndex = 3;
            // 
            // numOffsetX
            // 
            numOffsetX.DecimalPlaces = 1;
            numOffsetX.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numOffsetX.Location = new Point(122, 48);
            numOffsetX.Margin = new Padding(5, 5, 5, 5);
            numOffsetX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numOffsetX.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            numOffsetX.Name = "numOffsetX";
            numOffsetX.Size = new Size(244, 39);
            numOffsetX.TabIndex = 2;
            // 
            // lblOffsetY
            // 
            lblOffsetY.AutoSize = true;
            lblOffsetY.Location = new Point(32, 115);
            lblOffsetY.Margin = new Padding(5, 0, 5, 0);
            lblOffsetY.Name = "lblOffsetY";
            lblOffsetY.Size = new Size(32, 32);
            lblOffsetY.TabIndex = 1;
            lblOffsetY.Text = "Y:";
            // 
            // lblOffsetX
            // 
            lblOffsetX.AutoSize = true;
            lblOffsetX.Location = new Point(32, 51);
            lblOffsetX.Margin = new Padding(5, 0, 5, 0);
            lblOffsetX.Name = "lblOffsetX";
            lblOffsetX.Size = new Size(33, 32);
            lblOffsetX.TabIndex = 0;
            lblOffsetX.Text = "X:";
            // 
            // gpImageScale
            // 
            gpImageScale.Controls.Add(numScaleY);
            gpImageScale.Controls.Add(numScaleX);
            gpImageScale.Controls.Add(lblScaleY);
            gpImageScale.Controls.Add(lblScaleX);
            gpImageScale.Location = new Point(44, 584);
            gpImageScale.Margin = new Padding(5, 5, 5, 5);
            gpImageScale.Name = "gpImageScale";
            gpImageScale.Padding = new Padding(5, 5, 5, 5);
            gpImageScale.Size = new Size(406, 176);
            gpImageScale.TabIndex = 2;
            gpImageScale.TabStop = false;
            gpImageScale.Text = "Image Scale";
            // 
            // numScaleY
            // 
            numScaleY.DecimalPlaces = 4;
            numScaleY.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numScaleY.Location = new Point(122, 112);
            numScaleY.Margin = new Padding(5, 5, 5, 5);
            numScaleY.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numScaleY.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numScaleY.Name = "numScaleY";
            numScaleY.Size = new Size(244, 39);
            numScaleY.TabIndex = 3;
            numScaleY.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numScaleX
            // 
            numScaleX.DecimalPlaces = 4;
            numScaleX.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numScaleX.Location = new Point(122, 48);
            numScaleX.Margin = new Padding(5, 5, 5, 5);
            numScaleX.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numScaleX.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numScaleX.Name = "numScaleX";
            numScaleX.Size = new Size(244, 39);
            numScaleX.TabIndex = 2;
            numScaleX.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblScaleY
            // 
            lblScaleY.AutoSize = true;
            lblScaleY.Location = new Point(32, 115);
            lblScaleY.Margin = new Padding(5, 0, 5, 0);
            lblScaleY.Name = "lblScaleY";
            lblScaleY.Size = new Size(32, 32);
            lblScaleY.TabIndex = 1;
            lblScaleY.Text = "Y:";
            // 
            // lblScaleX
            // 
            lblScaleX.AutoSize = true;
            lblScaleX.Location = new Point(32, 51);
            lblScaleX.Margin = new Padding(5, 0, 5, 0);
            lblScaleX.Name = "lblScaleX";
            lblScaleX.Size = new Size(33, 32);
            lblScaleX.TabIndex = 0;
            lblScaleX.Text = "X:";
            // 
            // frmOptions
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(488, 784);
            Controls.Add(gpImageScale);
            Controls.Add(gpImageOffset);
            Controls.Add(gpSelectCourse);
            Margin = new Padding(5, 5, 5, 5);
            Name = "frmOptions";
            Text = "Options";
            gpSelectCourse.ResumeLayout(false);
            gpSelectCourse.PerformLayout();
            gpImageOffset.ResumeLayout(false);
            gpImageOffset.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOffsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOffsetX).EndInit();
            gpImageScale.ResumeLayout(false);
            gpImageScale.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numScaleY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScaleX).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gpSelectCourse;
        private RadioButton radioInnerCourse;
        private RadioButton radioMiddleCourse;
        private RadioButton radioOuterCourse;
        private CheckBox checkShowStartLap;
        private GroupBox gpImageOffset;
        private NumericUpDown numOffsetY;
        private NumericUpDown numOffsetX;
        private Label lblOffsetY;
        private Label lblOffsetX;
        private Button btnApplyOffset;
        private GroupBox gpImageScale;
        private NumericUpDown numScaleY;
        private NumericUpDown numScaleX;
        private Label lblScaleY;
        private Label lblScaleX;
    }
}
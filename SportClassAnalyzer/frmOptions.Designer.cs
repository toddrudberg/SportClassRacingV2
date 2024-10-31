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
            gpSelectCourse.SuspendLayout();
            SuspendLayout();
            // 
            // gpSelectCourse
            // 
            gpSelectCourse.Controls.Add(checkShowStartLap);
            gpSelectCourse.Controls.Add(radioInnerCourse);
            gpSelectCourse.Controls.Add(radioMiddleCourse);
            gpSelectCourse.Controls.Add(radioOuterCourse);
            gpSelectCourse.Location = new Point(27, 28);
            gpSelectCourse.Name = "gpSelectCourse";
            gpSelectCourse.Size = new Size(250, 161);
            gpSelectCourse.TabIndex = 0;
            gpSelectCourse.TabStop = false;
            gpSelectCourse.Text = "Select Course";
            // 
            // checkShowStartLap
            // 
            checkShowStartLap.AutoSize = true;
            checkShowStartLap.Location = new Point(20, 125);
            checkShowStartLap.Name = "checkShowStartLap";
            checkShowStartLap.Size = new Size(130, 24);
            checkShowStartLap.TabIndex = 3;
            checkShowStartLap.Text = "Show Start Lap";
            checkShowStartLap.UseVisualStyleBackColor = true;
            checkShowStartLap.CheckedChanged += checkShowStartLap_CheckedChanged;
            // 
            // radioInnerCourse
            // 
            radioInnerCourse.AutoSize = true;
            radioInnerCourse.Location = new Point(23, 88);
            radioInnerCourse.Name = "radioInnerCourse";
            radioInnerCourse.Size = new Size(112, 24);
            radioInnerCourse.TabIndex = 2;
            radioInnerCourse.TabStop = true;
            radioInnerCourse.Text = "Inner Course";
            radioInnerCourse.UseVisualStyleBackColor = true;
            radioInnerCourse.CheckedChanged += radioInnerCourse_CheckedChanged;
            // 
            // radioMiddleCourse
            // 
            radioMiddleCourse.AutoSize = true;
            radioMiddleCourse.Location = new Point(23, 58);
            radioMiddleCourse.Name = "radioMiddleCourse";
            radioMiddleCourse.Size = new Size(126, 24);
            radioMiddleCourse.TabIndex = 1;
            radioMiddleCourse.TabStop = true;
            radioMiddleCourse.Text = "Middle Course";
            radioMiddleCourse.UseVisualStyleBackColor = true;
            radioMiddleCourse.CheckedChanged += radioMiddleCourse_CheckedChanged;
            // 
            // radioOuterCourse
            // 
            radioOuterCourse.AutoSize = true;
            radioOuterCourse.Location = new Point(23, 28);
            radioOuterCourse.Name = "radioOuterCourse";
            radioOuterCourse.Size = new Size(116, 24);
            radioOuterCourse.TabIndex = 0;
            radioOuterCourse.TabStop = true;
            radioOuterCourse.Text = "Outer Course";
            radioOuterCourse.UseVisualStyleBackColor = true;
            radioOuterCourse.CheckedChanged += radioOuterCourse_CheckedChanged;
            // 
            // frmOptions
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 216);
            Controls.Add(gpSelectCourse);
            Name = "frmOptions";
            Text = "Form1";
            gpSelectCourse.ResumeLayout(false);
            gpSelectCourse.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gpSelectCourse;
        private RadioButton radioInnerCourse;
        private RadioButton radioMiddleCourse;
        private RadioButton radioOuterCourse;
        private CheckBox checkShowStartLap;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SportClassAnalyzer.frmMain;

namespace SportClassAnalyzer
{
    public partial class frmOptions : Form
    {
        cFormState myFormState = new cFormState();
        frmMain mainForm;
        public frmOptions(frmMain MainForm, cFormState state)
        {
            InitializeComponent();
            this.mainForm = MainForm;
            myFormState = state;

            setValues(myFormState);
        }

        public void setValues(cFormState state)
        {
            switch (state.courseType)
            {
                case cFormState.CourseType.Inner:
                    this.radioInnerCourse.Checked = true;
                    break;
                case cFormState.CourseType.Outer:
                    this.radioOuterCourse.Checked = true;
                    break;

                case cFormState.CourseType.Middle:
                    this.radioMiddleCourse.Checked = true;
                    break;
            }
            this.checkShowStartLap.Checked = state.showStartLap;
            
            // Set the offset values in the numeric up/down controls
            this.numOffsetX.Value = (decimal)state.ImageOffsetX;
            this.numOffsetY.Value = (decimal)state.ImageOffsetY;
            
            // Set the scale values in the numeric up/down controls
            this.numScaleX.Value = (decimal)state.ImageScaleX;
            this.numScaleY.Value = (decimal)state.ImageScaleY;
            this.Text = "Course Options";
            this.lblCourseName.Text = Path.GetFileNameWithoutExtension(state.sCourseFile);
        }

        private void radioOuterCourse_CheckedChanged(object sender, EventArgs e)
        {
            myFormState.courseType = cFormState.CourseType.Outer;
            mainForm.refreshPlot();
            myFormState.Save();
        }

        private void radioMiddleCourse_CheckedChanged(object sender, EventArgs e)
        {
            myFormState.courseType = cFormState.CourseType.Middle;
            mainForm.refreshPlot();
            myFormState.Save();
        }

        private void radioInnerCourse_CheckedChanged(object sender, EventArgs e)
        {
            myFormState.courseType = cFormState.CourseType.Inner;
            mainForm.refreshPlot();
            myFormState.Save();
        }

        private void checkShowStartLap_CheckedChanged(object sender, EventArgs e)
        {
            myFormState.showStartLap = checkShowStartLap.Checked;
            mainForm.refreshPlot();
            myFormState.Save();
        }

        private void btnApplyOffset_Click(object sender, EventArgs e)
        {
            // Update the form state with the new offset values
            myFormState.ImageOffsetX = (double)numOffsetX.Value;
            myFormState.ImageOffsetY = (double)numOffsetY.Value;
            
            // Update the form state with the new scale values
            myFormState.ImageScaleX = (double)numScaleX.Value;
            myFormState.ImageScaleY = (double)numScaleY.Value;
            
            // Save the form state
            myFormState.Save();
            
            // Refresh the plot to apply the new offsets and scales
            mainForm.refreshPlot();
        }
    }
}

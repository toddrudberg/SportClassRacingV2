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
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SportClassAnalyzer
{
    public partial class frmLeaderBoard : Form
    {
        public frmLeaderBoard()
        {
            InitializeComponent();
        }

        public void UpdateDisplay(LeaderBoard board)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateDisplay(board)));
                return;
            }

            txtLeaderBoard.Text = board.GetDisplayText();
        }

    }
}

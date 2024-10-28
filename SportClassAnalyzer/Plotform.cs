using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot;
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
    public partial class plotForm : Form
    {
        //public Plotform()
        //{
        //    InitializeComponent();
        //}

        //private void Plotform_Load(object sender, EventArgs e)
        //{
        //    // Create a new plot model
        //    var plotModel = new PlotModel { Title = "2D Map" };

        //    // Create a scatter series
        //    var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };

        //    // Add some example points to the scatter series (representing map points)
        //    scatterSeries.Points.Add(new ScatterPoint(10, 10));
        //    scatterSeries.Points.Add(new ScatterPoint(20, 15));
        //    scatterSeries.Points.Add(new ScatterPoint(30, 25));
        //    scatterSeries.Points.Add(new ScatterPoint(40, 35));

        //    // Add the scatter series to the plot model
        //    plotModel.Series.Add(scatterSeries);

        //    // Create a PlotView control
        //    var plotView = new PlotView
        //    {
        //        Model = plotModel,
        //        Dock = DockStyle.Fill // Set to fill the form
        //    };

        //    // Add the PlotView control to the form
        //    this.Controls.Add(plotView);
        //}

        public plotForm(PlotModel plotModel)
        {
            InitializeComponent();
            LoadPlot(plotModel);
        }

        private void LoadPlot(PlotModel plotModel)
        {
            // Create a PlotView control and set the received PlotModel
            var plotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill // Set to fill the form
            };

            // Add the PlotView control to the form
            this.Controls.Add(plotView);
        }

        private void plotForm_Load(object sender, EventArgs e)
        {

        }
    }
}

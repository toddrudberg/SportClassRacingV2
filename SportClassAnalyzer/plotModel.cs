using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using OxyPlot.WindowsForms;

namespace SportClassAnalyzer
{
    public class RacePlotModel
    {
        private List<OxyColor> oxyColors = new List<OxyColor>
        {
            OxyColors.Blue,
            OxyColors.Red,
            OxyColors.Orange,
            OxyColors.Purple,
            OxyColors.Brown,
            OxyColors.Cyan,
            OxyColors.Teal
        };

        private Dictionary<OxyColor, string> colorNames = new Dictionary<OxyColor, string>
        {
            { OxyColors.Blue, "Blue" },
            { OxyColors.Red, "Red" },
            { OxyColors.Orange, "Orange" },
            { OxyColors.Purple, "Purple" },
            { OxyColors.Brown, "Brown" },
            { OxyColors.Cyan, "Cyan" },
            { OxyColors.Teal, "Teal" }
        };

        private PlotView currentPlotView;
        public void CreatePlotModel(System.Windows.Forms.Form form, cPylons pylons, List<racePoint> raceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings)
        {
            // Remove the existing PlotView, if there is one
            if (currentPlotView != null)
            {
                form.Controls.Remove(currentPlotView);
                currentPlotView.Dispose();
                currentPlotView = null;
            }

            var plotModel = new PlotModel
            {
                Title = "Race Map",
//                TitlePadding = 10 // Adjust this value as needed
            };
            plotBackGroundImage(plotModel, pylons);
            plotPylons(pylons, plotModel);
            plotRaceData(raceData, lapCrossings, startGateCrossings, plotModel);

            int menuBarHeight = 30; // Adjust this height based on your menu bar size

            currentPlotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.None, // Disable fill to allow for manual positioning
                Location = new Point(0, menuBarHeight), // Start right below the menu bar
                Size = new Size(form.ClientSize.Width, form.ClientSize.Height - menuBarHeight), // Adjust height to fit below the menu bar
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right // Enable resizing with form
            };

            // Add PlotView and force it to display
            form.Controls.Add(currentPlotView);
            currentPlotView.BringToFront();
            currentPlotView.Show();

            // Confirm Layout and Display
            form.PerformLayout();
            form.Invalidate();
            form.Update();
            form.Refresh();
        }

        private void plotBackGroundImage(PlotModel plotModel, cPylons pylons)
        {
            // Load your image
            var imagePath = @"C:\LocalDev\SportClassRacingV2\LasCrucesMap.PNG"; // Path to the Google Earth image
            byte[] imageBytes;

            using (var bitmap = (Bitmap)Image.FromFile(imagePath))
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    imageBytes = stream.ToArray();
                }
            }

            // Create an OxyImage from the byte array
            var oxyImage = new OxyImage(imageBytes);

            // Define coordinates for scaling (latitude and longitude)
            double upperLeftLat = 32.318633;  // Upper-left latitude
            double upperLeftLon = -106.961483; // Upper-left longitude
            double lowerRightLat = 32.278689;  // Lower-right latitude
            double lowerRightLon = -106.874525; // Lower-right longitude

            var homePylon = pylons.homePylon();

            // Calculate Cartesian coordinates for upper-left corner
            double distance = cLatLon.HaversineDistance(homePylon.lat, homePylon.lon, upperLeftLat, upperLeftLon, pylons.elevationInFeet);
            double bearing = cLatLon.CalculateBearing(homePylon.lat, homePylon.lon, upperLeftLat, upperLeftLon);
            double upperLeftX = distance * Math.Sin(bearing * Math.PI / 180); // X-axis as east-west
            double upperLeftY = distance * Math.Cos(bearing * Math.PI / 180); // Y-axis as north-south

            // Calculate Cartesian coordinates for lower-right corner
            distance = cLatLon.HaversineDistance(homePylon.lat, homePylon.lon, lowerRightLat, lowerRightLon, pylons.elevationInFeet);
            bearing = cLatLon.CalculateBearing(homePylon.lat, homePylon.lon, lowerRightLat, lowerRightLon);
            double lowerRightX = distance * Math.Sin(bearing * Math.PI / 180); // X-axis as east-west
            double lowerRightY = distance * Math.Cos(bearing * Math.PI / 180); // Y-axis as north-south


            // Set padding to add a little extra space around the edges
            double padding = 5;

            // Create and set X-axis
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = Math.Min(upperLeftX, lowerRightX) - padding,
                Maximum = Math.Max(upperLeftX, lowerRightX) + padding,
                IsZoomEnabled = true, // Optional: Disable zoom if you want fixed limits
                IsPanEnabled = true   // Optional: Disable panning if you want fixed limits
            };

            // Create and set Y-axis
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = Math.Min(lowerRightY, upperLeftY) - padding,
                Maximum = Math.Max(lowerRightY, upperLeftY) + padding,
                IsZoomEnabled = true, // Optional: Disable zoom if you want fixed limits
                IsPanEnabled = true   // Optional: Disable panning if you want fixed limits
            };

            // Add axes to the plot model
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);


            // Calculate Width and Height based on Cartesian coordinates
            double imageWidth = lowerRightX - upperLeftX;
            double imageHeight = upperLeftY - lowerRightY;

            // Add the image as an annotation to scale by bounding box
            var imageAnnotation = new ImageAnnotation
            {
                ImageSource = oxyImage,
                Opacity = .25,
                // Position image using upper-left corner in Cartesian coordinates
                X = new PlotLength(upperLeftX + (imageWidth + padding) / 2, PlotLengthUnit.Data),
                Y = new PlotLength(upperLeftY - (imageHeight + padding) / 2, PlotLengthUnit.Data),

                // Scale image using width and height in Cartesian coordinates
                Width = new PlotLength(imageWidth, PlotLengthUnit.Data),
                Height = new PlotLength(imageHeight, PlotLengthUnit.Data),

                Layer = AnnotationLayer.BelowSeries // Places the image behind other plot elements
            };

            // Add the image annotation to the plot model
            plotModel.Annotations.Add(imageAnnotation);
        }

        private void plotPylons(cPylons pylons, PlotModel plotModel)
        {
            // Create a scatter series
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Black, // Set your desired color here
                MarkerSize = 10 // Optional: adjust marker size
            };
            var rubberbandSeries = new LineSeries { Color = OxyColors.Black };
            var outerCoursePylons = pylons.outerCoursePylons();
            foreach( var pylon in outerCoursePylons)
            {
                // Add the data points to the scatter series
                scatterSeries.Points.Add(new ScatterPoint((double)pylon.X, (double)pylon.Y, 5, 5));
                rubberbandSeries.Points.Add(new DataPoint(pylon.X, pylon.Y));

                var textAnnotation = new TextAnnotation
                {
                    Text = pylon.name, // Assuming each pylon has a Name property
                    TextPosition = new DataPoint(pylon.X, pylon.Y),
                    TextColor = OxyColors.Black,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12, // Adjust font size as needed
                    Stroke = OxyColors.Transparent // Make sure no border shows
                };

                // Add the annotation to the plot model
                plotModel.Annotations.Add(textAnnotation);
            }
            var homePylon = pylons.homePylonPoint();
            rubberbandSeries.Points.Add(new DataPoint(homePylon.X, homePylon.Y));

            // Add the scatter series to the plot model
            plotModel.Series.Add(scatterSeries);
            plotModel.Series.Add(rubberbandSeries);

            var startPylon = pylons.startFinishPylon();

            //draw a black line between homePylon and startPylon
            var lineSeries = new LineSeries { Color = OxyColors.Black };
            lineSeries.Points.Add(new DataPoint((double)homePylon.X, (double)homePylon.Y));
            lineSeries.Points.Add(new DataPoint((double)startPylon.X, (double)startPylon.Y));
            plotModel.Series.Add(lineSeries);


            var startGates = pylons.gatePylonPoints();
            if( startGates.Count == 2)
            {
                //draw a green line between the start gates
                var lineSeries2 = new LineSeries { Color = OxyColors.Green };
                lineSeries2.Points.Add(new DataPoint((double)startGates[0].X, (double)startGates[0].Y));
                lineSeries2.Points.Add(new DataPoint((double)startGates[1].X, (double)startGates[1].Y));
                plotModel.Series.Add(lineSeries2);
            }
        }

        private void plotRaceData(List<racePoint> myRaceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings, PlotModel plotModel)
        {
            // Create a new instance of PlotForm

            var lineSeries = new LineSeries { Color = OxyColors.Green };
            // Use a different color for each lap
            for (int i = 0; i < lapCrossings[0].dataPoint; i++)
            {
                double X = myRaceData[i].X;
                double Y = myRaceData[i].Y;
                // Add the data points to a line series
                lineSeries.Points.Add(new DataPoint(X, Y));
            }
            plotModel.Series.Add(lineSeries);


            for (int nLap = 0; nLap < lapCrossings.Count - 1; nLap++)
            {
                // Use a different color for each lap
                OxyColor lapColor = oxyColors[nLap % oxyColors.Count]; // Cycle through colors if needed
                Console.WriteLine($"Lap {nLap + 1} is {getColorName(lapColor)}");
                
                var lineSeriesLaps = new LineSeries { Color = lapColor };
                // Call JavaScript to start a new lap (reset points for new polyline)

                for (int i = lapCrossings[nLap].dataPoint; i < lapCrossings[nLap + 1].dataPoint; i++)
                {
                    // Convert latitude and longitude from decimal to double
                    double X = (double)myRaceData[i].X;
                    double Y = (double)myRaceData[i].Y;
                    lineSeriesLaps.Points.Add(new DataPoint(X, Y));
                }
                plotModel.Series.Add(lineSeriesLaps);
            }

            if (lapCrossings.Count == 0)
            {
                var lineSeriesLaps = new LineSeries { Color = OxyColors.Blue };
                for (int i = 0; i < myRaceData.Count; i++)
                {
                    // Convert latitude and longitude from decimal to double
                    double X = (double)myRaceData[i].X;
                    double Y = (double)myRaceData[i].Y;
                    lineSeriesLaps.Points.Add(new DataPoint(X, Y));
                }
                plotModel.Series.Add(lineSeriesLaps);
            }
        }

        private string getColorName(OxyColor color)
        {
            return colorNames.TryGetValue(color, out string name) ? name : "Unknown";
        }
    }
}

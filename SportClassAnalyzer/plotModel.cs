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
        public List<OxyColor> oxyColors = new List<OxyColor>
        {
            OxyColors.Blue,
            OxyColors.Red,
            OxyColors.Orange,
            OxyColors.Purple,
            OxyColors.Brown,
            OxyColors.Cyan,
            OxyColors.Teal
        };

        public Dictionary<OxyColor, string> colorNames = new Dictionary<OxyColor, string>
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
        public void CreatePlotModel(System.Windows.Forms.Form form, cFormState formState, cPylons pylons, cRaceData raceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings)
        {
            List<racePoint> racePoints = raceData.racePoints;
            string title = Path.GetFileNameWithoutExtension(formState.sRaceDataFile);
            // Remove the existing PlotView, if there is one
            if (currentPlotView != null)
            {
                form.Controls.Remove(currentPlotView);
                currentPlotView.Dispose();
                currentPlotView = null;
            }

            var plotModel = new PlotModel
            {
                Title = title,
//                TitlePadding = 10 // Adjust this value as needed
            };
            cPoint upperLeft;
            cPoint lowerRight;
            plotBackGroundImage(plotModel, pylons, out upperLeft, out lowerRight);
            plotPylons(pylons, plotModel, formState);
            plotRaceData(racePoints, lapCrossings, startGateCrossings, plotModel);
            plotLapSummary(raceData.myLaps, plotModel, upperLeft, lowerRight);

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

        private void plotBackGroundImage(PlotModel plotModel, cPylons pylons, out cPoint upperLeft, out cPoint lowerRight)
        {
            upperLeft = new cPoint(0, 0);
            lowerRight = new cPoint(0, 0);
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
                IsAxisVisible = true,
                IsZoomEnabled = true, // Optional: Disable zoom if you want fixed limits
                IsPanEnabled = true,   // Optional: Disable panning if you want fixed limits
                AxislineThickness = 0.1,    // Make the axis line very thin
                MajorTickSize = 0.5,        // Make major ticks very small
                MinorTickSize = 0.3,        // Make minor ticks even smaller
                FontSize = 1,               // Set a very small font size for labels
                TitleFontSize = 1           // Set a very small font size for title
            };

            // Create and set Y-axis
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = Math.Min(lowerRightY, upperLeftY) - padding,
                Maximum = Math.Max(lowerRightY, upperLeftY) + padding,
                IsAxisVisible = true,
                IsZoomEnabled = true, // Optional: Disable zoom if you want fixed limits
                IsPanEnabled = true,   // Optional: Disable panning if you want fixed limits
                AxislineThickness = 0.1,    // Make the axis line very thin
                MajorTickSize = 0.5,        // Make major ticks very small
                MinorTickSize = 0.3,        // Make minor ticks even smaller
                FontSize = 1,               // Set a very small font size for labels
                TitleFontSize = 1           // Set a very small font size for title
            };

            // Add axes to the plot model
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);


            // Calculate Width and Height based on Cartesian coordinates
            double imageWidth = lowerRightX - upperLeftX;
            double imageHeight = upperLeftY - lowerRightY;

            upperLeft = new cPoint(upperLeftX, upperLeftY);
            lowerRight = new cPoint(lowerRightX, lowerRightY);

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

        private void plotPylons(cPylons pylons, PlotModel plotModel, cFormState formState)
        {
            // Create a scatter series
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Black, // Set your desired color here
                MarkerSize = 10 // Optional: adjust marker size
            };
            var rubberbandSeries = new LineSeries { Color = OxyColors.Black };

            var activePylons = pylons.outerCoursePylons();
            List<pylonWpt> startPylons = new List<pylonWpt>();
            startPylons = pylons.startPylons(formState.courseType, true);

            switch (formState.courseType)
            {
                case cFormState.CourseType.Inner:
                    activePylons = pylons.innerCoursePylons();
                    break;
                case cFormState.CourseType.Outer:
                    activePylons = pylons.outerCoursePylons();
                    break;
                case cFormState.CourseType.Middle:
                    activePylons = pylons.middleCoursePylons();
                    break;
            }

            foreach( var pylon in activePylons)
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

            var rubberbandSeriesStart = new LineSeries { Color = OxyColors.Black };
            if (formState.showStartLap && !formState.courseType.Equals(cFormState.CourseType.Outer))
            {
                foreach (var pylon in startPylons)
                {
                    // Add the data points to the scatter series
                    scatterSeries.Points.Add(new ScatterPoint((double)pylon.X, (double)pylon.Y, 5, 5));
                    rubberbandSeriesStart.Points.Add(new DataPoint(pylon.X, pylon.Y));

                    if (pylon.name != "")
                    {
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
                }
                if (formState.courseType == cFormState.CourseType.Inner)
                {
                    rubberbandSeriesStart.Points.Add(new DataPoint(homePylon.X, homePylon.Y));
                }
            }
            

            // Add the scatter series to the plot model
            plotModel.Series.Add(scatterSeries);
            plotModel.Series.Add(rubberbandSeries);
            plotModel.Series.Add(rubberbandSeriesStart);

            var startPylon = pylons.startFinishPylon();

            //draw a black line between homePylon and startPylon
            var lineSeries = new LineSeries { Color = OxyColors.Black };
            lineSeries.Points.Add(new DataPoint((double)homePylon.X, (double)homePylon.Y));
            lineSeries.Points.Add(new DataPoint((double)startPylon.X, (double)startPylon.Y));
            plotModel.Series.Add(lineSeries);


            //var startGates = pylons.gatePylonPoints();
            //if( startGates.Count == 2)
            //{
            //    //draw a green line between the start gates
            //    var lineSeries2 = new LineSeries { Color = OxyColors.Green };
            //    lineSeries2.Points.Add(new DataPoint((double)startGates[0].X, (double)startGates[0].Y));
            //    lineSeries2.Points.Add(new DataPoint((double)startGates[1].X, (double)startGates[1].Y));
            //    plotModel.Series.Add(lineSeries2);
            //}
        }
        private void plotLapSummary(List<cLap> laps, PlotModel plotModel, cPoint upperLeft, cPoint lowerRight)
        {
            double percentOfWidth = 0.25;
            double lineSpacingPercent = 0.0225; // Adjust this value based on your layout needs
            double width = lowerRight.X - upperLeft.X;
            double height = upperLeft.Y - lowerRight.Y;

            // Add a white rectangle as a background for the Lap Summary
            var rectangle = new RectangleAnnotation
            {
                Fill = OxyColors.White,
                MinimumX = lowerRight.X - width * percentOfWidth,
                MaximumX = lowerRight.X,
                MinimumY = upperLeft.Y,
                MaximumY = lowerRight.Y,
                Layer = AnnotationLayer.BelowSeries
            };
            plotModel.Annotations.Add(rectangle);

            // Add "Lap Summary" header text
            double startY = upperLeft.Y - 5; // Adjust startY based on your layout needs
            double lineSpacing = height * lineSpacingPercent; // Vertical space between lines of text

            var headerText = new TextAnnotation
            {
                Text = "Lap Summary",
                TextVerticalAlignment = VerticalAlignment.Top,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                TextPosition = new DataPoint(lowerRight.X - (width * (percentOfWidth - 0.01)), startY),
                TextColor = OxyColors.Black,
                FontSize = 16,
                Stroke = OxyColors.Transparent
            };
            plotModel.Annotations.Add(headerText);
            startY -= 2 * lineSpacing;
            // Add lap details below the header
            //for (int i = 0; i < laps.Count; i++)
            {
                //var lap = laps[i];
                var lapText = new TextAnnotation
                {
                    Text = cLap.ToStringAll(laps, this), // Example lap format
                    TextVerticalAlignment = VerticalAlignment.Top,
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                    TextPosition = new DataPoint(lowerRight.X - (width * (percentOfWidth - 0.01)), startY),
                    TextColor = OxyColors.Black,
                    FontSize = 9, // Slightly smaller than header
                    Stroke = OxyColors.Transparent
                };
                plotModel.Annotations.Add(lapText);
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

            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5, // Adjust the size as needed
                MarkerFill = OxyColors.Red // Customize the color
            };

            for (int nLap = 0; nLap < lapCrossings.Count - 1; nLap++)
            {
                ScatterPoint crossingPoint = new (lapCrossings[nLap].crossingPoint.X, lapCrossings[nLap].crossingPoint.Y);
                scatterSeries.Points.Add(crossingPoint);
            }
            plotModel.Series.Add(scatterSeries);

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

        public string getColorName(OxyColor color)
        {
            return colorNames.TryGetValue(color, out string name) ? name : "Unknown";
        }
    }
}

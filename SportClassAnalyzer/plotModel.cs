using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using OxyPlot.WindowsForms;
using System.Windows.Forms;

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
        private List<LineSeries> _racerTrails = new List<LineSeries>();

        private PlotView currentPlotView;
        public void CreatePlotModel(System.Windows.Forms.Form form, cFormState formState, Course course, cRaceData raceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings)
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
            plotBackGroundImage(plotModel, course, out upperLeft, out lowerRight);
            plotPylons(course, plotModel, formState);
            plotRaceData(racePoints, lapCrossings, startGateCrossings, plotModel, course);
            plotLapSummary(raceData.myLaps, plotModel, upperLeft, lowerRight, course.segments.Sum());

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

        private void plotBackGroundImage(PlotModel plotModel, Course course, out cPoint upperLeft, out cPoint lowerRight)
        {
            upperLeft = new cPoint(0, 0);
            lowerRight = new cPoint(0, 0);
            // Load your image
            var imagePath = course.CourseImage.ImageFile;
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

            var homePylon = course.homePylon();

            // Calculate Cartesian coordinates for upper-left corner
            double distance = cLatLon.HaversineDistance(homePylon.Latitude, homePylon.Longitude, course.CourseImage.UpperLeft.Latitude, course.CourseImage.UpperLeft.Longitude, course.ElevationInFeet);
            double bearing = cLatLon.CalculateBearing(homePylon.Latitude, homePylon.Longitude, course.CourseImage.UpperLeft.Latitude, course.CourseImage.UpperLeft.Longitude);
            double upperLeftX = distance * Math.Sin(bearing * Math.PI / 180) + course.CourseImage.OffsetX; // X-axis as east-west with offset
            double upperLeftY = distance * Math.Cos(bearing * Math.PI / 180) + course.CourseImage.OffsetY; // Y-axis as north-south with offset

            // Calculate Cartesian coordinates for lower-right corner
            distance = cLatLon.HaversineDistance(homePylon.Latitude, homePylon.Longitude, course.CourseImage.LowerRight.Latitude, course.CourseImage.LowerRight.Longitude, course.ElevationInFeet);
            bearing = cLatLon.CalculateBearing(homePylon.Latitude, homePylon.Longitude, course.CourseImage.LowerRight.Latitude, course.CourseImage.LowerRight.Longitude);
            double lowerRightX = distance * Math.Sin(bearing * Math.PI / 180) + course.CourseImage.OffsetX; // X-axis as east-west with offset
            double lowerRightY = distance * Math.Cos(bearing * Math.PI / 180) + course.CourseImage.OffsetY; // Y-axis as north-south with offset

            Console.WriteLine($"Upper Left: {upperLeftX}, {upperLeftY}");
            Console.WriteLine($"Lower Right: {lowerRightX}, {lowerRightY}");

            Console.WriteLine($"Image Offset X: {course.CourseImage.OffsetX}");
            Console.WriteLine($"Image Offset Y: {course.CourseImage.OffsetY}");

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

        private void plotPylons(Course course, PlotModel plotModel, cFormState formState)
        {
            // Create a scatter series
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Black, // Set your desired color here
                MarkerSize = 10 // Optional: adjust marker size
            };
            var rubberbandSeries = new LineSeries { Color = OxyColors.Black };

            var activePylons = course.outerCoursePylons();
            List<Waypoint> startPylons = new List<Waypoint>();
            startPylons = course.startPylons(formState.courseType, true);

            switch (formState.courseType)
            {
                case cFormState.CourseType.Inner:
                    activePylons = course.innerCoursePylons();
                    break;
                case cFormState.CourseType.Outer:
                    activePylons = course.outerCoursePylons();
                    break;
                case cFormState.CourseType.Middle:
                    activePylons = course.middleCoursePylons();
                    break;
            }

            // Apply scale factors to coordinates
            double scaleX = course.CourseImage.ScaleX;
            double scaleY = course.CourseImage.ScaleY;

            foreach (var pylon in activePylons)
            {
                // Scale the coordinates for plotting
                double scaledX = pylon.X * scaleX;
                double scaledY = pylon.Y * scaleY;

                // Add the data points to the scatter series
                scatterSeries.Points.Add(new ScatterPoint(scaledX, scaledY, 5, 5));
                rubberbandSeries.Points.Add(new DataPoint(scaledX, scaledY));

                var textAnnotation = new TextAnnotation
                {
                    Text = pylon.Name, // Assuming each pylon has a Name property
                    TextPosition = new DataPoint(scaledX, scaledY),
                    TextColor = OxyColors.Black,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12, // Adjust font size as needed
                    Stroke = OxyColors.Transparent // Make sure no border shows
                };

                // Add the annotation to the plot model
                plotModel.Annotations.Add(textAnnotation);
            }
            var homePylon = course.homePylonPoint();
            // Scale the home pylon coordinates
            double scaledHomeX = homePylon.X * scaleX;
            double scaledHomeY = homePylon.Y * scaleY;
            rubberbandSeries.Points.Add(new DataPoint(scaledHomeX, scaledHomeY));

            var rubberbandSeriesStart = new LineSeries { Color = OxyColors.Black };
            if (formState.showStartLap && !formState.courseType.Equals(cFormState.CourseType.Outer))
            {
                foreach (var pylon in startPylons)
                {
                    // Scale the coordinates for plotting
                    double scaledX = pylon.X * scaleX;
                    double scaledY = pylon.Y * scaleY;

                    // Add the data points to the scatter series
                    scatterSeries.Points.Add(new ScatterPoint(scaledX, scaledY, 5, 5));
                    rubberbandSeriesStart.Points.Add(new DataPoint(scaledX, scaledY));

                    if (pylon.Name != "")
                    {
                        var textAnnotation = new TextAnnotation
                        {
                            Text = pylon.Name, // Assuming each pylon has a Name property
                            TextPosition = new DataPoint(scaledX, scaledY),
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
                    rubberbandSeriesStart.Points.Add(new DataPoint(scaledHomeX, scaledHomeY));
                }
            }


            // Add the scatter series to the plot model
            plotModel.Series.Add(scatterSeries);
            plotModel.Series.Add(rubberbandSeries);
            plotModel.Series.Add(rubberbandSeriesStart);

            var startPylon = course.startFinishPylon();

            //draw a black line between homePylon and startPylon
            var lineSeries = new LineSeries { Color = OxyColors.Black };
            lineSeries.Points.Add(new DataPoint(scaledHomeX, scaledHomeY));

            // Scale the start/finish pylon coordinates
            double scaledStartX = startPylon.X * scaleX;
            double scaledStartY = startPylon.Y * scaleY;
            lineSeries.Points.Add(new DataPoint(scaledStartX, scaledStartY));
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
        private void plotLapSummary(List<cLap> laps, PlotModel plotModel, cPoint upperLeft, cPoint lowerRight, double ptpDistance)
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
                Text = $"Lap Summary - PTP Distance: {(ptpDistance / 5280):F3} Miles", // Example header text
                TextVerticalAlignment = VerticalAlignment.Top,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                TextPosition = new DataPoint(lowerRight.X - (width * (percentOfWidth - 0.01)), startY),
                TextColor = OxyColors.Black,
                FontSize = 12,
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

        private void plotRaceData(List<racePoint> myRaceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings, PlotModel plotModel, Course course)
        {
            // Get the scale factors from the course
            double scaleX = course.CourseImage.ScaleX;
            double scaleY = course.CourseImage.ScaleY;

            // Create a new instance of PlotForm
            var lineSeries = new LineSeries { Color = OxyColors.Green };
            // Use a different color for each lap
            for (int i = 0; i < lapCrossings[0].dataPoint; i++)
            {
                // Scale the coordinates for plotting
                double X = myRaceData[i].X * scaleX;
                double Y = myRaceData[i].Y * scaleY;
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
                    // Scale the coordinates for plotting
                    double X = (double)myRaceData[i].X * scaleX;
                    double Y = (double)myRaceData[i].Y * scaleY;
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
                // Scale the crossing point coordinates
                double crossingX = lapCrossings[nLap].crossingPoint.X * scaleX;
                double crossingY = lapCrossings[nLap].crossingPoint.Y * scaleY;
                ScatterPoint crossingPoint = new(crossingX, crossingY);
                scatterSeries.Points.Add(crossingPoint);
            }
            plotModel.Series.Add(scatterSeries);

            if (lapCrossings.Count == 0)
            {
                var lineSeriesLaps = new LineSeries { Color = OxyColors.Blue };
                for (int i = 0; i < myRaceData.Count; i++)
                {
                    // Scale the coordinates for plotting
                    double X = (double)myRaceData[i].X * scaleX;
                    double Y = (double)myRaceData[i].Y * scaleY;
                    lineSeriesLaps.Points.Add(new DataPoint(X, Y));
                }
                plotModel.Series.Add(lineSeriesLaps);
            }
        }

        public void CreateMultipleRacePlotModel(Form form, cFormState formState, Course course, List<cRaceData> allRaceData)
        {
            // Remove existing plot view
            if (currentPlotView != null)
            {
                form.Controls.Remove(currentPlotView);
                currentPlotView.Dispose();
                currentPlotView = null;
            }

            // Create new plot model
            var plotModel = new PlotModel
            {
                Title = "Multiple Race Playback"
            };

            // Plot background and pylons
            cPoint upperLeft, lowerRight;
            plotBackGroundImage(plotModel, course, out upperLeft, out lowerRight);
            plotPylons(course, plotModel, formState);

            // Add a legend
            var legend = new Legend
            {
                LegendTitle = "Race Data",
                LegendPosition = LegendPosition.RightTop
            };
            plotModel.Legends.Add(legend);

            _racerTrails = new List<LineSeries>(); // Reset list

            // First: plot static full races
            for (int raceIndex = 0; raceIndex < allRaceData.Count; raceIndex++)
            {
                OxyColor raceColor = oxyColors[raceIndex % oxyColors.Count];
                OxyColor fadedColor = OxyColor.FromAColor((int)(.1 * 256), raceColor);
                string raceName = $"Race {raceIndex + 1}";

                // plotMultipleRaceData likely adds its own LineSeries
                plotMultipleRaceData(allRaceData[raceIndex].racePoints, plotModel, course, fadedColor, raceName);
            }

            // Second: create live trails *after*
            _racerTrails = new List<LineSeries>();

            for (int raceIndex = 0; raceIndex < allRaceData.Count; raceIndex++)
            {
                OxyColor raceColor = oxyColors[raceIndex % oxyColors.Count];

                var trailSeries = new LineSeries
                {
                    Color = raceColor,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid,
                    RenderInLegend = false
                };

                plotModel.Series.Add(trailSeries);
                _racerTrails.Add(trailSeries);
            }


            // Create and add the plot view
            int menuBarHeight = 30;
            currentPlotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.None,
                Location = new Point(0, menuBarHeight),
                Size = new Size(form.ClientSize.Width, form.ClientSize.Height - menuBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            form.Controls.Add(currentPlotView);
            currentPlotView.BringToFront();
            currentPlotView.Show();

            form.PerformLayout();
            form.Invalidate();
            form.Update();
            form.Refresh();
        }


        private void plotMultipleRaceData(List<racePoint> racePoints, PlotModel plotModel, Course course, OxyColor color, string raceName)
        {
            // Get scale factors
            double scaleX = course.CourseImage.ScaleX;
            double scaleY = course.CourseImage.ScaleY;

            // Create a line series for this race
            var lineSeries = new LineSeries
            {
                Color = color,
                Title = raceName,
                StrokeThickness = 2
            };

            // Add all points to the line series
            foreach (var point in racePoints)
            {
                double X = point.X * scaleX;
                double Y = point.Y * scaleY;
                lineSeries.Points.Add(new DataPoint(X, Y));
            }

            // Add the line series to the plot model
            plotModel.Series.Add(lineSeries);
        }

        public void UpdateRacerTrails(Form form, List<List<racePoint>> visiblePointsPerRacer, Course course)
        {
            List<int> numPoints = new List<int>();
            for (int i = 0; i < visiblePointsPerRacer.Count; i++)
            {
                var points = visiblePointsPerRacer[i];
                var trail = _racerTrails[i];

                // Optional: clear first if you're only passing visible points each frame
                trail.Points.Clear();

                
                foreach (var pt in points)
                {
                    var dp = new DataPoint(pt.X * course.CourseImage.ScaleX, pt.Y * course.CourseImage.ScaleY);
                    trail.Points.Add(dp);
                }

                //if (trail.Points.Count > 0)
                //{
                //    var first = trail.Points[0];
                //    Console.WriteLine($"Trail[{i}] Example point: {first.X:F2}, {first.Y:F2}");
                //}

                numPoints.Add(points.Count);
            }



            //Console.WriteLine($"Visible points: {string.Join(", ", numPoints)}");
            currentPlotView.Model.InvalidatePlot(false);

            //form.PerformLayout();
            //form.Invalidate();
            form.Update();
            //form.Refresh();
        }


        public string getColorName(OxyColor color)
        {
            return colorNames.TryGetValue(color, out string name) ? name : "Unknown";
        }
    }
}

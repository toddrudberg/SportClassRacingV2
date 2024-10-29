using Microsoft.Web.WebView2.Core;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using OxyPlot.Annotations;
using System.Runtime.Intrinsics.Arm;
using OxyPlot.Axes;


namespace SportClassAnalyzer
{
    public partial class Form1 : Form
    {

        public class cFormState
        {
            public static string sPylonFile = @"C:\LocalDev\SportClassRacingV2\SportClassOuterCourse - Middle.gpx";
            public static string sRaceDataFile = @"C:\LocalDev\SportClassRacing\Slater Data\20241018_142045.gpx";
        }
        public cPylons myPylons = new cPylons();
        public cRaceData myRaceData = new cRaceData();

        public List<cLapCrossings> myLapCrossings = new List<cLapCrossings>();
        public List<cLapCrossings> myStartGateCrossings = new List<cLapCrossings>();

        #region Console Output
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, COORD size);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool absolute, ref SMALL_RECT consoleWindow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int STD_OUTPUT_HANDLE = -11;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOSIZE = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        #endregion

        #region Constructor and Layout
        public Form1()
        {
            InitializeComponent();
            AllocConsole();
            // Set the console to be tall and narrow
            SetConsoleSize(40, 50); // Adjust width and height here
                                    // Set form size to a percentage of screen resolution
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            this.Size = new Size((int)(screenWidth * 0.8), (int)(screenHeight * 0.9)); // 75% of screen size
                                                                                       // Set the form's position at the top right of the screen
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(screenWidth - this.Width, 0); // Right edge, top of screen

            //buildRace();
            // Start race-building process asynchronously
            //_ = buildRaceAsync();
        }
        #endregion

        private async void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void buildRace(bool buildFromRaceBox = false)
        {
            Console.WriteLine("Loading Pylon Data");
            // Load the pylons
            pylons pylons = null;
            XmlSerializer serializer = new XmlSerializer(typeof(pylons));
            using (FileStream fs = new FileStream(cFormState.sPylonFile, FileMode.Open))
            {
                pylons = (pylons)serializer.Deserialize(fs);
            }
            //write the pylons to a listbox
            myPylons.pylonWpts = pylons.wpt.ToList();
            myPylons.assignCartisianCoordinates(4450);//pylons.elevationInFeet);
            myPylons.assignSegments();


            foreach (pylonWpt wpt in pylons.wpt)
            {
                listBox1.Items.Add(wpt);
            }

            gpx raceData = null;
            if (buildFromRaceBox)
            {
                Console.WriteLine("Loading RaceBox Data");
                List<cRaceBoxRecord> raceBoxRecords = cRaceBoxParser.ParseCsv(cFormState.sRaceDataFile);
                Console.WriteLine("Converting RaceBox Data to GPX");
                raceData = cRaceBoxParser.ConvertRaceBoxToGpx(raceBoxRecords);

            }
            else
            {
                Console.WriteLine("Loading Race Data");
                // Load the race data

                XmlSerializer raceSerializer = new XmlSerializer(typeof(gpx));
                using (FileStream fs = new FileStream(cFormState.sRaceDataFile, FileMode.Open))
                {
                    raceData = (gpx)raceSerializer.Deserialize(fs);
                }
            }
            // Process the race data as needed
            // For example, add race data to a listbox
            foreach (var gpxTrkTrkpt in raceData.trk.trkseg)
            {
                listBox2.Items.Add(gpxTrkTrkpt);
            }

            myRaceData.myRaceData = raceData.trk.trkseg.ToList();
            myRaceData.assignCartisianCoordinates(myPylons.homePylon());
            myRaceData.calculateSpeedsAndTruncate(100);
            myRaceData.detectLaps(myPylons, out myLapCrossings, out myStartGateCrossings);

            var plotModel = new PlotModel { Title = "Race Map" };
            // Create a PlotView and set it to fill the form
            //var plotView = new PlotView
            //{
            //    Model = plotModel,
            //    Dock = DockStyle.Fill // This makes the PlotView fill the entire form
            //};

            // Add the PlotView to the form's controls
            //this.Controls.Add(plotView);

            plotBackGroundImage(plotModel);
            plotPylonsPlotModel(myPylons, plotModel);
            plotRaceDataPlotModel(myRaceData.myRaceData, myLapCrossings, myStartGateCrossings, plotModel);

            // Pass the generated PlotModel to the new form
            //var plotForm = new SportClassAnalyzer.plotForm(plotModel, this);
            //plotForm.Show(); // or plotForm.Show() if you want it non-modal

            // Create a PlotView control and set the received PlotModel
            var plotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill // Set to fill the form
            };

            // Add the PlotView control to the form
            this.Controls.Add(plotView);

            this.listBox1.Hide();
            this.listBox2.Hide();
        }

        public void plotBackGroundImage(PlotModel plotModel)
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

            var homePylon = myPylons.homePylon();

            // Calculate Cartesian coordinates for upper-left corner
            double distance = cLatLon.HaversineDistance(homePylon.lat, homePylon.lon, upperLeftLat, upperLeftLon, 4450);
            double bearing = cLatLon.CalculateBearing(homePylon.lat, homePylon.lon, upperLeftLat, upperLeftLon);
            double upperLeftX = distance * Math.Sin(bearing * Math.PI / 180); // X-axis as east-west
            double upperLeftY = distance * Math.Cos(bearing * Math.PI / 180); // Y-axis as north-south

            // Calculate Cartesian coordinates for lower-right corner
            distance = cLatLon.HaversineDistance(homePylon.lat, homePylon.lon, lowerRightLat, lowerRightLon, 4450);
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


        private async Task DisplayMap()
        {
            Console.WriteLine("Building the map");

            webView2Control.Dock = DockStyle.Fill;
            await webView2Control.EnsureCoreWebView2Async();

            string htmlPath = System.IO.Path.Combine(Application.StartupPath, "map.html");
            webView2Control.Source = new Uri(htmlPath);

            // Attach NavigationCompleted event handler only once
            if (webView2Control.CoreWebView2 != null)
            {
                webView2Control.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
                webView2Control.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            }
        }

        // Event handler for WebView2 navigation completion
        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                //try
                //{
                //    if (webView2Control.CoreWebView2 != null)
                //    {
                //        var result = webView2Control.ExecuteScriptAsync("clearMap();");
                //        Console.WriteLine($"Script executed with result: {result}");
                //    }
                //    else
                //    {
                //        Console.WriteLine("CoreWebView2 is not initialized.");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Error executing script: {ex.Message}");
                //}
                Console.WriteLine("Drawing Pylons");
                PlotPylons(myPylons);
                Console.WriteLine("Drawing Race Data");
                PlotRaceData(myRaceData.myRaceData, myLapCrossings, myStartGateCrossings);
                Console.WriteLine("Map Updated");
            }
        }

        private void SetConsoleSize(int width, int height)
        {
            IntPtr consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            // Set the buffer size
            COORD bufferSize = new COORD((short)width, (short)height);
            SetConsoleScreenBufferSize(consoleHandle, bufferSize);

            // Set the window size
            SMALL_RECT windowSize = new SMALL_RECT();
            windowSize.Left = 0;
            windowSize.Top = 0;
            windowSize.Right = (short)(width - 1);
            windowSize.Bottom = (short)(height - 1);
            SetConsoleWindowInfo(consoleHandle, true, ref windowSize);
            // Find the console window handle
            IntPtr consoleWindowHandle = FindWindow(null, Console.Title);
            // Position the console in the upper-left corner
            SetWindowPos(consoleWindowHandle, HWND_TOP, 0, 0, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
        }

        public void plotPylonsPlotModel(cPylons pylons, PlotModel plotModel)
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


        public async void PlotPylons(cPylons pylons)
        {
            foreach (var pylon in pylons.pylonWpts)
            {
                // Convert latitude and longitude from decimal to double
                double lat = (double)pylon.lat;
                double lon = (double)pylon.lon;
                string name = pylon.name;
                // Inject JavaScript to add marker to the map
                string script = $"addPylonMarker({lat}, {lon}, '{name}');";
                await webView2Control.ExecuteScriptAsync(script);                
            }

            double lat1 = (double)pylons.homePylon().lat;
            double lon1 = (double)pylons.homePylon().lon;
            double lat2 = (double)pylons.startFinishPylon().lat;
            double lon2 = (double)pylons.startFinishPylon().lon;
            string script2 = $"drawLineBetweenPoints({lat1}, {lon1}, {lat2}, {lon2}, 'black');";
            await webView2Control.ExecuteScriptAsync(script2);


            List<pylonWpt> gatePylons = pylons.gatePylons();
            lat1 = (double)gatePylons[0].lat;
            lon1 = (double)gatePylons[0].lon;
            lat2 = (double)gatePylons[1].lat;
            lon2 = (double)gatePylons[1].lon;
            script2 = $"drawLineBetweenPoints({lat1}, {lon1}, {lat2}, {lon2}, 'green');";
            await webView2Control.ExecuteScriptAsync(script2);

            // make a list of pylons not including the start finish pylon
            var pylonsList = pylons.outerCoursePylons();
            // draw lines between the pylonsList
            for (int i = 0; i < pylonsList.Count; i++)
            {
                pylonWpt pylon = pylonsList[i];
                pylonWpt pylon1 = new pylonWpt();
                if (i == pylonsList.Count - 1)
                {
                    pylon1 = pylons.homePylon();
                }
                else
                {
                    pylon1 = pylonsList[i + 1];
                }
                // Convert latitude and longitude from decimal to double
                lat1 = (double)pylon.lat;
                lon1 = (double)pylon.lon;
                lat2 = (double)pylon1.lat;
                lon2 = (double)pylon1.lon;
                string script = $"drawLineBetweenPoints({lat1}, {lon1}, {lat2}, {lon2}, 'black');";
                await webView2Control.ExecuteScriptAsync(script);
            }
        }



        List<OxyColor> oxyColors = new List<OxyColor>
        {
            OxyColors.Blue,
            OxyColors.Red,
            OxyColors.Orange,
            OxyColors.Purple,
            OxyColors.Brown,
            OxyColors.Pink,
            OxyColors.Teal
        };
        Dictionary<OxyColor, string> colorNames = new Dictionary<OxyColor, string>
        {
            { OxyColors.Blue, "Blue" },
            { OxyColors.Red, "Red" },
            { OxyColors.Orange, "Orange" },
            { OxyColors.Purple, "Purple" },
            { OxyColors.Brown, "Brown" },
            { OxyColors.Pink, "Pink" },
            { OxyColors.Teal, "Teal" }
        };

        // Function to get the color name or "Unknown" if it's not in the dictionary
        string getColorName(OxyColor color)
        {
            return colorNames.TryGetValue(color, out string name) ? name : "Unknown";
        }
        public void plotRaceDataPlotModel(List<racePoint> myRaceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings, PlotModel plotModel)
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
                OxyColor lapColor = oxyColors[nLap % lapColors.Length]; // Cycle through colors if needed
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



        // Define different colors for each lap
        string[] lapColors = { "red", "blue", "black", "purple", "orange", "yellow", "magenta", "cyan" };

        int lapIndex = 0; // To track lap number
        public async void PlotRaceData(List<racePoint> myRaceData, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings)
        {
            lapIndex = 1;

            // Use a different color for each lap
            string lapColor = lapColors[lapIndex % lapColors.Length]; // Cycle through colors if needed

            // Call JavaScript to start a new lap (reset points for new polyline)
            await webView2Control.ExecuteScriptAsync("startNewLap();");

            // plot startGateCrossings in green until first lap crossing
            for (int i = 0; i < lapCrossings[0].dataPoint; i++)
            {
                // Convert latitude and longitude from decimal to double
                double lat = (double)myRaceData[i].lat;
                double lon = (double)myRaceData[i].lon;
                //Thread.Sleep(50);

                // Inject JavaScript to add the marker to the map with lap-specific color
                string script = $"addRaceDataPoint({lat}, {lon}, 'green');";
                await webView2Control.ExecuteScriptAsync(script);
            }


            for (int nLap = 0; nLap < lapCrossings.Count - 1; nLap++)
            {
                // Use a different color for each lap
                lapColor = lapColors[lapIndex % lapColors.Length]; // Cycle through colors if needed
                Console.WriteLine($"Lap {nLap + 1} is {lapColor}");

                // Call JavaScript to start a new lap (reset points for new polyline)
                await webView2Control.ExecuteScriptAsync("startNewLap();");

                for (int i = lapCrossings[nLap].dataPoint; i < lapCrossings[nLap + 1].dataPoint; i++)
                {
                    // Convert latitude and longitude from decimal to double
                    double lat = (double)myRaceData[i].lat;
                    double lon = (double)myRaceData[i].lon;
                    //Thread.Sleep(50);

                    // Inject JavaScript to add the marker to the map with lap-specific color
                    string script = $"addRaceDataPoint({lat}, {lon}, '{lapColor}');";
                    await webView2Control.ExecuteScriptAsync(script);
                }
                lapIndex++;
            }

            if (lapCrossings.Count == 0)
            {
                for (int i = 0; i < myRaceData.Count; i++)
                {
                    // Convert latitude and longitude from decimal to double
                    double lat = (double)myRaceData[i].lat;
                    double lon = (double)myRaceData[i].lon;
                    //Thread.Sleep(50);

                    // Inject JavaScript to add the marker to the map with lap-specific color
                    string script = $"addRaceDataPoint({lat}, {lon}, '{lapColor}');";
                    await webView2Control.ExecuteScriptAsync(script);
                }
            }
        }

        public void clearAllData()
        {
            myPylons = new cPylons();
            myRaceData.myRaceData.Clear();
            myLapCrossings.Clear();
            myStartGateCrossings.Clear();
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "GPX files|*.gpx";
            openFileDialog1.Title = "Select a GPX file";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                clearAllData();
                cFormState.sRaceDataFile = openFileDialog1.FileName;
                buildRace();
                //await DisplayMap();
            }
        }

        private async void openRaceBoxFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "RaceBox csv|*.csv";
            openFileDialog1.Title = "Select a RaceBox file";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                clearAllData();
                cFormState.sRaceDataFile = openFileDialog1.FileName;
                buildRace(true);
                //await DisplayMap();
            }
        }
    }
}

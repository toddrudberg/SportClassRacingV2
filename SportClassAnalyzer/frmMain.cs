using Microsoft.Web.WebView2.Core;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot;
using System.Runtime.InteropServices;
using System.Xml.Serialization;


namespace SportClassAnalyzer
{
    public partial class Form1 : Form
    {

        public class cFormState
        {
            public static string sPylonFile = @""C:\LocalDev\SportClassRacingV2\SportClassOuterCourse.gpx"";
            //public static string sRaceDataFile = @"C:\LocalDev\SportClassRacing\TestData.gpx";
            //public static string sRaceDataFile = @"C:\LocalDev\SportClassRacing\Slater Data\20241018_104841.gpx";
            public static string sRaceDataFile = @"C:\LocalDev\SportClassRacing\Slater Data\20241018_142045.gpx";
            //public static string sRaceDataFile = @"C:\LocalDev\SportClassRacing\Slater Data\20241018_142045.gpx";
        }

        //public List<pylonWpt> myPylons = new List<pylonWpt>();


        public cPylons myPylons = new cPylons();
        public cRaceData myRaceData = new cRaceData();

        public List<cLapCrossings> myLapCrossings = new List<cLapCrossings>();

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
            // Wait until buildRace is complete before displaying the map
            //await raceBuiltCompletionSource.Task;
            //await DisplayMap();

            // Create a new instance of PlotForm
            var plotModel = new PlotModel { Title = "2D Map" };

            // Create a scatter series
            var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };

            // Add some example points to the scatter series
            scatterSeries.Points.Add(new ScatterPoint(10, 10));
            scatterSeries.Points.Add(new ScatterPoint(20, 15));
            scatterSeries.Points.Add(new ScatterPoint(30, 25));
            scatterSeries.Points.Add(new ScatterPoint(40, 35));

            // Add the scatter series to the plot model
            plotModel.Series.Add(scatterSeries);

            // Pass the generated PlotModel to the new form
            var plotForm = new SportClassAnalyzer.plotForm(plotModel);
            //var plotForm = new SportClassAnalyzer.plotForm();
            // Show the new form as a modal dialog
            plotForm.Show(); // or plotForm.Show() if you want it non-modal
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
            if(buildFromRaceBox)
            {
                Console.WriteLine("Loading RaceBox Data");
                List<cRaceBoxRecord> raceBoxRecords =  cRaceBoxParser.ParseCsv(cFormState.sRaceDataFile);
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
            myRaceData.detectLaps(myPylons, out myLapCrossings);
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
                Console.WriteLine("Drawing Pylons");
                PlotPylons(myPylons);
                Console.WriteLine("Drawing Race Data");
                PlotRaceData(myRaceData.myRaceData, myLapCrossings);
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

            // make a list of pylons not including the start finish pylon
            var pylonsList = pylons.pylonWpts.Where(p => p.name != "StartFinish").ToList();
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

        // Define different colors for each lap
        //string[] lapColors = { "red", "blue", "green", "purple", "orange" };

        //int lapIndex = 0; // To track lap number

        //public async void PlotRaceData(List<racePoint> myRaceData)
        //{
        //    foreach (var dataPoint in myRaceData)
        //    {
        //        // Convert latitude and longitude from decimal to double
        //        double lat = (double)dataPoint.lat;
        //        double lon = (double)dataPoint.lon;

        //        // Inject JavaScript to add marker to the map without time
        //        string script = $"addRaceDataPoint({lat}, {lon});";
        //        await webView2Control.ExecuteScriptAsync(script);
        //    }
        //}
        // Define different colors for each lap
        string[] lapColors = { "red", "blue", "green", "purple", "orange", "yellow", "magenta", "cyan" };

        int lapIndex = 0; // To track lap number

        public async void PlotRaceData(List<racePoint> myRaceData, List<cLapCrossings> lapCrossings)
        {
            lapIndex = 1;

            // Use a different color for each lap
            string lapColor = lapColors[lapIndex % lapColors.Length]; // Cycle through colors if needed

            // Call JavaScript to start a new lap (reset points for new polyline)
            await webView2Control.ExecuteScriptAsync("startNewLap();");

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


        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "GPX files|*.gpx";
            openFileDialog1.Title = "Select a GPX file";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                myLapCrossings.Clear();
                myPylons.pylonWpts.Clear();
                myRaceData.myRaceData.Clear();
                if (webView2Control.CoreWebView2 != null)
                {
                    await webView2Control.ExecuteScriptAsync("clearMap();");
                }


                cFormState.sRaceDataFile = openFileDialog1.FileName;
                buildRace();
                await DisplayMap();
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
                myLapCrossings.Clear();
                myPylons.pylonWpts.Clear();
                myRaceData.myRaceData.Clear();
                if (webView2Control.CoreWebView2 != null)
                {
                    webView2Control.CoreWebView2.ExecuteScriptAsync("clearMap();");
                }

                cFormState.sRaceDataFile = openFileDialog1.FileName;
                buildRace(true);
                await DisplayMap();
            }
        }
    }
}

using Microsoft.Web.WebView2.Core;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using OxyPlot.Annotations;
using System.Runtime.Intrinsics.Arm;
using OxyPlot.Axes;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace SportClassAnalyzer
{

    public partial class frmMain : Form
    {
        
        public cFormState myFormState = new cFormState();
        private frmOptions optionsForm;

        public cPylons myPylons = new cPylons();
        public cRaceData myRaceData = new cRaceData();

        public List<cLapCrossings> myLapCrossings = new List<cLapCrossings>();
        public List<cLapCrossings> myStartGateCrossings = new List<cLapCrossings>();

        private bool raceBuilt = false;

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
        public frmMain()
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

            myFormState = cFormState.Load();
            myFormState.Save();

            optionsFormBringForward();
        }

        public void optionsFormBringForward()
        {
            if (optionsForm == null || optionsForm.IsDisposed)
            {
                optionsForm = new frmOptions(this, myFormState);
                optionsForm.Show();
            }
            else
            {
                optionsForm.BringToFront();
            }
        }
#endregion

private void buildRace(bool buildFromRaceBox = false)
        {
            Console.WriteLine("Loading Pylon Data");
            // Load the pylons
            pylons pylons = null;
            XmlSerializer serializer = new XmlSerializer(typeof(pylons));
            using (FileStream fs = new FileStream(myFormState.sPylonFile, FileMode.Open))
            {
                pylons = (pylons)serializer.Deserialize(fs);
            }
            //write the pylons to a listbox
            myPylons.pylonWpts = pylons.wpt.ToList();
            myPylons.elevationInFeet = pylons.elevationInFeet;
            myPylons.assignCartisianCoordinates(myPylons.elevationInFeet);
            myPylons.assignSegments(myFormState);


            foreach (pylonWpt wpt in pylons.wpt)
            {
                listBox1.Items.Add(wpt);
            }

            gpx raceData = null;
            if (buildFromRaceBox)
            {
                Console.WriteLine("Loading RaceBox Data");
                List<cRaceBoxRecord> raceBoxRecords = cRaceBoxParser.ParseCsv(myFormState.sRaceDataFile);
                Console.WriteLine("Converting RaceBox Data to GPX");
                raceData = cRaceBoxParser.ConvertRaceBoxToGpx(raceBoxRecords);

            }
            else
            {
                Console.WriteLine("Loading Race Data");
                // Load the race data

                XmlSerializer raceSerializer = new XmlSerializer(typeof(gpx));
                using (FileStream fs = new FileStream(myFormState.sRaceDataFile, FileMode.Open))
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

            myRaceData.racePoints = raceData.trk.trkseg.ToList();
            myRaceData.assignCartisianCoordinates(myPylons.homePylon());
            myRaceData.calculateSpeedsAndTruncate(100);
            myRaceData.detectLaps(myPylons, out myLapCrossings, out myStartGateCrossings);
            myRaceData.checkForCourseCuts(myFormState, myPylons, myLapCrossings, myStartGateCrossings, myRaceData.myLaps);
            raceBuilt = true;
            refreshPlot();

        }

        public void refreshPlot()
        {
            if (raceBuilt)
            {
                RacePlotModel racePlotModel = new RacePlotModel();

                racePlotModel.CreatePlotModel(this, myFormState, myPylons, myRaceData, myLapCrossings, myStartGateCrossings);

                this.listBox1.Hide();
                this.listBox2.Hide();
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

        public void clearAllData()
        {
            myPylons = new cPylons();
            myRaceData = new cRaceData();
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
                myFormState.sRaceDataFile = openFileDialog1.FileName;
                buildRace();

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
                myFormState.sRaceDataFile = openFileDialog1.FileName;
                buildRace(true);

            }
        }

    }
}

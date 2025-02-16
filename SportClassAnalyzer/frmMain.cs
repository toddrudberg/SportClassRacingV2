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
using System.Diagnostics;
using Pastel;


namespace SportClassAnalyzer
{

    public partial class frmMain : Form
    {

        public cFormState myFormState = new cFormState();
        private frmOptions optionsForm;

        public Course myCourse = new Course();
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Loading Pylon Data");
            // Load the pylons
            Course theCourse = Course.LoadCourseFile(myFormState.sCourseFile);

            theCourse.ElevationInFeet = 0;//pylons.elevationInFeet; using zero makes the course length match the survey

            myCourse = theCourse;

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

            stopwatch.Stop();
            Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms".Pastel(Color.Green));
            myRaceData.racePoints = raceData.trk.trkseg.ToList();
            raceBuilt = true;
            refreshPlot();
        }

        public void refreshPlot()
        {
            if (raceBuilt)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                myCourse.assignCartisianCoordinates(myCourse.ElevationInFeet);
                myCourse.assignTheta();
                myCourse.assignSegments(myFormState);

                myRaceData.assignCartisianCoordinates(myCourse.homePylon());

                myRaceData.calculateSpeedsAndTruncate(100);
                myRaceData.detectLaps(myCourse, out myLapCrossings, out myStartGateCrossings);
                myRaceData.checkForCourseCuts(myFormState, myCourse, myLapCrossings, myStartGateCrossings, myRaceData.myLaps);

                RacePlotModel racePlotModel = new RacePlotModel();
                racePlotModel.CreatePlotModel(this, myFormState, myCourse, myRaceData, myLapCrossings, myStartGateCrossings);
                stopwatch.Stop();
                Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms".Pastel(Color.Chartreuse));

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
            myCourse = new Course();
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
                myFormState.Save();
                buildRace(true);
            }
        }

        private void openRaceCourseFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "JSON files|*.json";
            openFileDialog1.Title = "Select a JSON file";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                clearAllData();
                myFormState.sCourseFile = openFileDialog1.FileName;
                myFormState.Save();
            }
        }

        private void selectRaceCourseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "JSON files|*.json";
            openFileDialog1.Title = "Select a JSON file";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                Course dog = Course.LoadCourseFile(openFileDialog1.FileName);
                dog.assignCartisianCoordinates();
            }
        }
    }
}

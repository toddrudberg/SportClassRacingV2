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
            this.DoubleBuffered = true;
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

            // Initialize the form state's image offset and scale values from the course file
            // myFormState.ImageOffsetX = myCourse.CourseImage.OffsetX;
            // myFormState.ImageOffsetY = myCourse.CourseImage.OffsetY;
            // myFormState.ImageScaleX = myCourse.CourseImage.ScaleX;
            // myFormState.ImageScaleY = myCourse.CourseImage.ScaleY;
            // myFormState.Save();

            // Update the options form with the new values from the course
            // if (optionsForm != null && !optionsForm.IsDisposed)
            // {
            //     optionsForm.setValues(myFormState);
            //     optionsForm.Refresh();
            // }

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

                // Apply the image offset and scale values from the form state
                myCourse.CourseImage.OffsetX = myFormState.ImageOffsetX;
                myCourse.CourseImage.OffsetY = myFormState.ImageOffsetY;
                myCourse.CourseImage.ScaleX = myFormState.ImageScaleX;
                myCourse.CourseImage.ScaleY = myFormState.ImageScaleY;

                // Ensure the options form is updated with the current values
                //if (optionsForm != null && !optionsForm.IsDisposed)
                //{
                //    optionsForm.setValues(myFormState);
                //}

                myRaceData.assignCartisianCoordinates(myCourse.homePylon());

                myRaceData.calculateSpeedsAndTruncate(100);
                myRaceData.detectLaps(myCourse, out myLapCrossings, out myStartGateCrossings);
                myRaceData.checkForCourseCuts(myFormState, myCourse, myLapCrossings, myStartGateCrossings, myRaceData.myLaps);

                RacePlotModel racePlotModel = new RacePlotModel();
                racePlotModel.CreatePlotModel(this, myFormState, myCourse, myRaceData, myLapCrossings, myStartGateCrossings);
                stopwatch.Stop();
                Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms".Pastel(Color.Chartreuse));

                // Log the current offset values
                Console.WriteLine($"Image Offset X: {myCourse.CourseImage.OffsetX}");
                Console.WriteLine($"Image Offset Y: {myCourse.CourseImage.OffsetY}");
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

        private void selectRaceCourseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "JSON files|*.json";
            openFileDialog1.Title = "Select a JSON file";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                Course dog = Course.LoadCourseFile(openFileDialog1.FileName);
                myFormState.sCourseFile = openFileDialog1.FileName;
                myFormState.ImageOffsetX = dog.CourseImage.OffsetX;
                myFormState.ImageOffsetY = dog.CourseImage.OffsetY;
                myFormState.ImageScaleX = dog.CourseImage.ScaleX;
                myFormState.ImageScaleY = dog.CourseImage.ScaleY;
                //we need to set the formOptions values
                myFormState.Save();
                //update the frmOptions values using the newly saved myFormState
                if (optionsForm == null || optionsForm.IsDisposed)
                {
                    optionsForm = new frmOptions(this, myFormState);
                    optionsForm.Show();
                }
                else
                {
                    optionsForm.setValues(myFormState);
                    optionsForm.Refresh();
                    optionsForm.BringToFront();
                }
                dog.assignCartisianCoordinates();
            }
        }

        private void playbackAllRacesInFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select a folder containing race data files";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                LoadAndPlaybackAllRacesInFolder(folderPath);
            }
        }

        private void LoadAndPlaybackAllRacesInFolder(string folderPath)
        {
            // Clear existing data
            clearAllData();

            // Get all GPX and CSV files in the folder
            string[] gpxFiles = Directory.GetFiles(folderPath, "*.gpx");
            string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");

            if (gpxFiles.Length == 0 && csvFiles.Length == 0)
            {
                MessageBox.Show("No race data files found in the selected folder.", "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create a list to hold all race data
            List<cRaceData> allRaceData = new List<cRaceData>();

            // Load GPX files
            foreach (string gpxFile in gpxFiles)
            {
                try
                {
                    cRaceData raceDataObj = LoadGpxFile(gpxFile);
                    if (raceDataObj != null)
                    {
                        allRaceData.Add(raceDataObj);
                        Console.WriteLine($"Loaded race data from {Path.GetFileName(gpxFile)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading {Path.GetFileName(gpxFile)}: {ex.Message}");
                }
            }

            // Load CSV files (RaceBox format)
            foreach (string csvFile in csvFiles)
            {
                try
                {
                    cRaceData raceDataObj = LoadRaceBoxFile(csvFile);
                    if (raceDataObj != null)
                    {
                        allRaceData.Add(raceDataObj);
                        Console.WriteLine($"Loaded race data from {Path.GetFileName(csvFile)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading {Path.GetFileName(csvFile)}: {ex.Message}");
                }
            }

            if (allRaceData.Count == 0)
            {
                MessageBox.Show("Failed to load any race data files.", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Load the course if not already loaded
            if (myCourse == null || myCourse.Pylons == null)
            {
                if (string.IsNullOrEmpty(myFormState.sCourseFile) || !File.Exists(myFormState.sCourseFile))
                {
                    MessageBox.Show("Please select a race course file first.", "Course Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Course theCourse = Course.LoadCourseFile(myFormState.sCourseFile);
                myCourse = theCourse;
            }

            // Process and visualize all race data
            ProcessAndVisualizeMultipleRaces(allRaceData);
        }

        private cRaceData LoadGpxFile(string filePath)
        {
            XmlSerializer raceSerializer = new XmlSerializer(typeof(gpx));
            gpx raceData;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                raceData = (gpx)raceSerializer.Deserialize(fs);
            }

            cRaceData raceDataObj = new cRaceData();
            raceDataObj.racePoints = raceData.trk.trkseg.ToList();

            return raceDataObj;
        }

        private cRaceData LoadRaceBoxFile(string filePath)
        {
            List<cRaceBoxRecord> raceBoxRecords = cRaceBoxParser.ParseCsv(filePath);
            gpx raceData = cRaceBoxParser.ConvertRaceBoxToGpx(raceBoxRecords);

            cRaceData raceDataObj = new cRaceData();
            raceDataObj.racePoints = raceData.trk.trkseg.ToList();

            return raceDataObj;
        }

        private void ProcessAndVisualizeMultipleRaces(List<cRaceData> allRaceData)
        {
            // Process course data
            myCourse.assignCartisianCoordinates(myCourse.ElevationInFeet);
            myCourse.assignTheta();
            myCourse.assignSegments(myFormState);

            // Process each race data set and collect filtered race data
            List<cRaceData> filteredRaceData = new List<cRaceData>();

            foreach (cRaceData raceData in allRaceData)
            {
                // Assign Cartesian coordinates
                raceData.assignCartisianCoordinates(myCourse.homePylon());

                // Calculate speeds
                raceData.calculateSpeedsAndTruncate(100);

                // Create separate lap crossing lists for each race
                List<cLapCrossings> raceLapCrossings = new List<cLapCrossings>();
                List<cLapCrossings> raceStartGateCrossings = new List<cLapCrossings>();

                // Detect laps to filter data
                raceData.detectLaps(myCourse, out raceLapCrossings, out raceStartGateCrossings);

                // If laps were detected, create a filtered race data object
                if (raceLapCrossings.Count > 0)
                {
                    // Create a new race data object with only the points from start to last lap
                    cRaceData filteredData = new cRaceData();

                    // Determine start and end indices
                    int startIndex = 0;
                    int endIndex = raceLapCrossings[raceLapCrossings.Count - 1].dataPoint;

                    // If start gate crossings were detected, use the first one as the start
                    //if (raceStartGateCrossings.Count > 0)
                    //{
                    //    startIndex = Math.Max(0, raceStartGateCrossings[0].dataPoint - 5);
                    //}

                    // Extract only the points between start and end
                    filteredData.racePoints = new List<racePoint>(
                        raceData.racePoints.GetRange(startIndex, endIndex - startIndex + 1));

                    // Add to the filtered list
                    filteredRaceData.Add(filteredData);

                    Console.WriteLine($"Race filtered: {startIndex} to {endIndex} ({filteredData.racePoints.Count} points)");
                }
                else
                {
                    // If no laps were detected, use the original data
                    filteredRaceData.Add(raceData);
                    Console.WriteLine("No laps detected for this race, using all data points");
                }
            }

            // Set flag to indicate race is built
            raceBuilt = true;

            // Create a plot model for multiple races using the filtered data
            RacePlotModel racePlotModel = new RacePlotModel();
            racePlotModel.CreateMultipleRacePlotModel(this, myFormState, myCourse, filteredRaceData);
            PlayBackWithTrailingWindow(racePlotModel, myCourse, filteredRaceData, 5.0);
            //PlayBackLoopInBackground(racePlotModel, myCourse, filteredRaceData, 5.0);
        }

        public void PlayBackWithTrailingWindow(RacePlotModel racePlotModel, Course course, List<cRaceData> allRaceData, double playbackSpeed = 1.0)
        {
            System.DateTime earliestTime = System.DateTime.MaxValue;
            System.DateTime longestTime = System.DateTime.MinValue;
            if (true)
            {
                for (int i = 0; i < allRaceData.Count; i++)
                {
                    cRaceData raceData = allRaceData[i];
                    if (raceData.racePoints.Count == 0)
                        continue;

                    earliestTime = raceData.racePoints[0].time < earliestTime ? raceData.racePoints[0].time : earliestTime;
                    longestTime = raceData.racePoints[raceData.racePoints.Count - 1].time > longestTime ? raceData.racePoints[raceData.racePoints.Count - 1].time : longestTime;
                }
            }

            DateTime startTime = earliestTime;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            TimeSpan trailingWindow = TimeSpan.FromSeconds(1);
            TimeSpan maxDuration = longestTime - startTime;
            
            Stopwatch cycleTime = new Stopwatch();
            cycleTime.Start();
            while (true)
            {
                TimeSpan scaledElapsed = TimeSpan.FromSeconds(stopwatch.Elapsed.TotalSeconds * playbackSpeed) ;
                DateTime playbackTime = startTime + scaledElapsed;

                // End playback when past the last data point
                if (scaledElapsed > maxDuration)
                    break;

                List<int> numPoints = new List<int>();
                List<List<racePoint>> visiblePerRacer = new List<List<racePoint>>();
                for (int i = 0; i < allRaceData.Count; i++)
                {
                    // Get all points within the trailing 10-second window
                    cRaceData raceData = allRaceData[i];
                    List<racePoint> points = raceData.racePoints;

                    var visiblePoints = points
                        .Where(p => p.time <= playbackTime && p.time >= playbackTime - trailingWindow)
                        .ToList();
                    numPoints.Add(visiblePoints.Count);
                    visiblePerRacer.Add(visiblePoints);
                }
                racePlotModel.UpdateRacerTrails(this, visiblePerRacer, course);
                //racePlotModel.UpdateAircraftPositions(this, visiblePerRacer, course);
                Console.WriteLine($"Cycle time: {cycleTime.ElapsedMilliseconds} ms");
                cycleTime.Restart();
                Thread.Sleep(16); 
            }

            stopwatch.Stop();
        }

    }
}

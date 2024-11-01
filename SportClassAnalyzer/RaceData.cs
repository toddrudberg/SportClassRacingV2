using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pastel;
using System.Drawing;

namespace SportClassAnalyzer
{
    public class cLap
    {
        public double maxSpeed;
        public double ptpSpeed;
        public double elapsedTime;
        public double averageSpeed;
        public double distanceFlown;
        public DateTime startTime;
        public DateTime endTime;
        public bool isStartLap = false;
        public int nCuts = 0;

        public static string ToStringAll(List<cLap> laps, RacePlotModel racePlotModel)
        {
            string result = string.Empty;

            for (int i = 0; i < laps.Count; i++)
            {
                cLap lap = laps[i];
                if (lap.isStartLap)
                {
                    result += $"Start Lap: (Green) ";
                    result += lap.nCuts == 0 ? "No Cuts\n" : $"Cuts = {lap.nCuts}\n";

                    TimeZoneInfo mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                    DateTime localStartTime = TimeZoneInfo.ConvertTimeFromUtc(lap.startTime, mountainTimeZone);
                    result += $"Start Time: {localStartTime:yyyy-MM-dd HH:mm:ss.fff}\n";
                }
                else
                {
                    OxyColor lapColor = racePlotModel.oxyColors[i - 1 % racePlotModel.oxyColors.Count]; // Cycle through colors if needed
                    result += $"Lap {i}: ({racePlotModel.getColorName(lapColor)}) ";
                    result += lap.nCuts == 0 ? "No Cuts\n" : $"Cuts = {lap.nCuts}\n";

                    result += $"Elapsed Time: {lap.elapsedTime.ToString("F3")} sec\n";
                    result += $"PTP Speed: {lap.ptpSpeed.ToString("F3")} mph\n";
                }
                result += $"Max Speed: {lap.maxSpeed.ToString("F0")} mph\n";
                result += $"Average Speed: {lap.averageSpeed.ToString("F3")} mph\n";
                result += $"Distance Flown: {(lap.distanceFlown / 5280).ToString("F3")} miles\n";

                if (i == laps.Count - 1)
                {
                    TimeZoneInfo mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                    DateTime localEndTime = TimeZoneInfo.ConvertTimeFromUtc(lap.endTime, mountainTimeZone);
                    result += $"End Time: {localEndTime:yyyy-MM-dd HH:mm:ss.fff}\n";
                }
                result += "\n";
            }
            return result;
        }  
    }

    public class cLapCrossings
    {
        public int dataPoint;
        public DateTime crossingTime;
        public cPoint crossingPoint;

        public cLapCrossings(int dataPoint, DateTime crossingTime, cPoint crossingPoint)
        {
            this.dataPoint = dataPoint;
            this.crossingTime = crossingTime;
            this.crossingPoint = crossingPoint;
        }
    }

    public class cRaceData
    {
        public List<racePoint> racePoints = new List<racePoint>();
        public List<cLap> myLaps = new List<cLap>();

        public void assignCartisianCoordinates(pylonWpt homePylon)
        {
            foreach (racePoint dp in racePoints)
            {
                // Calculate distance and bearing from home pylon to this pylon
                double distance = cLatLon.HaversineDistance(homePylon.lat, homePylon.lon, dp.lat, dp.lon, dp.altitudeInFeet);
                double bearing = cLatLon.CalculateBearing(homePylon.lat, homePylon.lon, dp.lat, dp.lon);

                // Convert polar coordinates (distance, bearing) to Cartesian coordinates
                dp.X = distance * Math.Sin(bearing * Math.PI / 180); // X-axis as east-west
                dp.Y = distance * Math.Cos(bearing * Math.PI / 180); // Y-axis as north-south
                //Console.WriteLine($"{Math.Round(dp.X)}, {Math.Round(dp.Y)}");
            }
        }

        public void checkForCourseCuts(cFormState formState, cPylons pylons, List<cLapCrossings> lapCrossings, List<cLapCrossings> startGateCrossings, List<cLap> laps)
        {
            var coursePylons = pylons.outerCoursePylons();
            var startPylons = pylons.startPylons(formState.courseType, false);
            switch(formState.courseType)
            {
                case cFormState.CourseType.Inner:
                    coursePylons = pylons.innerCoursePylons();
                    break;
                case cFormState.CourseType.Middle:
                    coursePylons = pylons.middleCoursePylons();
                    break;
                case cFormState.CourseType.Outer:
                    coursePylons = pylons.outerCoursePylons();
                    break;
            }
            
            List<cLapCrossings> cuts;

            if (lapCrossings.Count > 1)
            {
                for (int i = 0; i < lapCrossings.Count; i++)
                {
                    int startOfLap = 0;
                    int endOfLap = 0;
                    int nPylonCuts = 0;
                    bool evaluateStartLap = false;

                    if (i == 0)
                    {
                        if (startGateCrossings.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            evaluateStartLap = true;
                        }
                    }
                    else
                    {
                        startOfLap = lapCrossings[i - 1].dataPoint;
                    }

                    endOfLap = lapCrossings[i].dataPoint;

                    List<racePoint> lapData = racePoints.GetRange(startOfLap, endOfLap - startOfLap);

                    bool insideCourse = false;
                    var activePylons = coursePylons;
                    if (evaluateStartLap)
                    {
                        activePylons = startPylons;
                    }
                    for (int j = 0; j < activePylons.Count - 1; j++)
                    {
                        cPoint p1 = new cPoint(activePylons[j].X, activePylons[j].Y);
                        cPoint p2 = new cPoint(activePylons[j + 1].X, activePylons[j + 1].Y);
                        int cut = LineCrossingDetector.DetectCrossings(lapData, p1, p2, out cuts);
                        if (insideCourse)
                        {
                            if (cut == 0)
                            {
                                nPylonCuts++;
                            }
                            else if (cut == 1)
                            {
                                insideCourse = false;
                            }
                        }
                        else
                        {
                            if (cut == 1 )
                            {
                                nPylonCuts++;
                                insideCourse = true;
                            }
                            if (cut == 2 )
                            {
                                nPylonCuts++;
                            }
                        }
                    }                 
                    myLaps[i].nCuts = nPylonCuts;
                }
            }
        }

        public void detectLaps(cPylons pylons, out List<cLapCrossings> lapCrossings, out List<cLapCrossings> startGateCrossings)
        {
            myLaps.Clear();
            cPoint homePylon = pylons.homePylonPoint();
            cPoint startFinishPylon = pylons.startFinishPylonPoint();

            //List<cLapCrossings> lapCrossings = new List<cLapCrossings>();

            List<cPoint> startGates = pylons.gatePylonPoints();
            List<cLapCrossings> test = new List<cLapCrossings>();
            int numStartCrossings = LineCrossingDetector.DetectCrossings(racePoints, startGates[0], startGates[1], out startGateCrossings);
            if( numStartCrossings > 0 && startGateCrossings[0].dataPoint > 5)
            {
                //remove all data points before the first start gate crossing
                racePoints = racePoints.GetRange(startGateCrossings[0].dataPoint - 5, racePoints.Count - startGateCrossings[0].dataPoint);            
            }
            else
            {
                racePoints = racePoints.GetRange(0, racePoints.Count);
                Console.WriteLine("No start gate crossings detected - assuming start at beginning of data");
            }

            int crossings = LineCrossingDetector.DetectCrossings(racePoints, homePylon, startFinishPylon, out lapCrossings);

            if( crossings == 0)
            {
                Console.WriteLine("No laps detected");
                return;
            }
            Console.WriteLine("Lap detection complete");
            Console.WriteLine($"Number of laps detected: {crossings}");
            int startOfRace = lapCrossings[0].dataPoint;
            int endOfRace = lapCrossings[lapCrossings.Count - 1].dataPoint;


            if (lapCrossings.Count > 1)
            {

                for (int i = 0; i < lapCrossings.Count; i++)
                {
                    int startOfLap = 0;
                    int endOfLap = 0;

                    if(i == 0)
                    {
                        if( numStartCrossings == 0)
                        {
                            Console.WriteLine("alert! assuming start crossing is beginning of data.".Pastel(Color.Red));
                            //continue;
                        }
                    }
                    else
                    {
                        startOfLap = lapCrossings[i - 1].dataPoint;
                    }
                    
                    endOfLap = lapCrossings[i].dataPoint;

                    List<racePoint> lapData = racePoints.GetRange(startOfLap, endOfLap - startOfLap);
                    //calculate lap data
                    //public double maxSpeed;-
                    //public double ptpSpeed;-
                    //public double elapsedTime;-
                    //public double averageSpeed;-
                    //public double distanceFlown;-
                    //public DateTime startTime;-
                    //public DateTime endTime;-
                    //public bool isStartLap;-
                    cLap lap = new cLap();
                    lap.startTime = lapData[0].time;
                    lap.endTime = lapCrossings[i].crossingTime;
                    lap.maxSpeed = lapData.Max(p => p.speedMPH);                    
                    lap.averageSpeed = lapData.Average(p => p.speedMPH);
                    lap.distanceFlown = lapData
                        .Zip(lapData.Skip(1), (previous, current) =>
                            Math.Sqrt(Math.Pow(current.X - previous.X, 2) + Math.Pow(current.Y - previous.Y, 2)))
                        .Sum();                  
                    //output lap data to Console:
                    Console.WriteLine();
                    if(i == 0)
                    {
                        lap.isStartLap = true;
                        Console.WriteLine("Start Lap");
                        Console.WriteLine($"Average Speed: {Math.Round(lap.averageSpeed,4)} mph");
                        Console.WriteLine($"Max Speed: {Math.Round(lap.maxSpeed)} mph");
                        Console.WriteLine($"Distance Flown: {Math.Round(lap.distanceFlown / 5280, 2).ToString("F2")} miles");
                        myLaps.Add(lap);
                    }
                    else
                    {
                        // Calculate the elapsed time as a TimeSpan
                        TimeSpan elapsedTimeSpan = lapCrossings[i].crossingTime - lapCrossings[i - 1].crossingTime;

                        // Convert the TimeSpan to milliseconds
                        double elapsedTime = elapsedTimeSpan.TotalMilliseconds / 1000.0;

                        lap.elapsedTime = elapsedTime;
                        lap.ptpSpeed = pylons.segments.Sum() / lap.elapsedTime * 3600 / 5280;
                        myLaps.Add(lap);
                        Console.WriteLine($"Lap {i}");
                        Console.WriteLine($"Elapsed Time: {Math.Round(lap.elapsedTime, 2).ToString("F2")} seconds");
                        Console.WriteLine($"PTP Speed: {lap.ptpSpeed.ToString("F4")} mph");
                        Console.WriteLine($"Average Speed: {Math.Round(lap.averageSpeed,4)} mph");
                        Console.WriteLine($"Max Speed: {Math.Round(lap.maxSpeed)} mph");
                        Console.WriteLine($"Distance Flown: {Math.Round(lap.distanceFlown / 5280, 2).ToString("F2")} miles");                    }                    
                }
                Console.WriteLine();
            }
        }

        public void calculateSpeedsAndTruncate(double limitSpeed)
        {
            int lastIndex = 0;
            double lastSpeed = 0;
            racePoints[0].speedMPH = lastSpeed;

            for (int i = 1; i < racePoints.Count - 1; i++)
            {
                double distance = Math.Sqrt(Math.Pow(racePoints[lastIndex].X - racePoints[i].X, 2) + Math.Pow(racePoints[lastIndex].Y - racePoints[i].Y, 2));
                if( distance < 10)
                {
                    racePoints[i].speedMPH = lastSpeed;
                    continue;
                }

                // Calculate time between two points
                double time = (racePoints[i].time - racePoints[lastIndex].time).TotalSeconds;

                // Calculate speed
                double speed = distance / time;
                // convert ft/s to mph
                speed = speed * 3600 / 5280;
                racePoints[i].speedMPH = speed;
                lastSpeed = speed;
                lastIndex = i;
                //Console.WriteLine($"{Math.Round(speed)} mph");
            }

            //truncate myRaceData to only include points where speed is greater than 100 mph
            racePoints = racePoints.Where(p => p.speedMPH > limitSpeed).ToList();
        }

    }

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class gpx
    {

        private gpxTrk trkField;

        private decimal versionField;

        /// <remarks/>
        public gpxTrk trk
        {
            get
            {
                return this.trkField;
            }
            set
            {
                this.trkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class gpxTrk
    {

        private string nameField;

        private racePoint[] trksegField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("trkpt", IsNullable = false)]
        public racePoint[] trkseg
        {
            get
            {
                return this.trksegField;
            }
            set
            {
                this.trksegField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class racePoint
    {

        private decimal eleField;

        private System.DateTime timeField;

        private decimal latField;

        private decimal lonField;

        public double X;
        public double Y;
        public double speedMPH;

        /// <remarks/>
        public decimal ele
        {
            get
            {
                return this.eleField;
            }
            set
            {
                this.eleField = value;
            }
        }

        public double altitudeInFeet
        {
            get
            {
                return (double)ele * 3.28084;
            }
        }

        /// <remarks/>
        public System.DateTime time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double lat
        {
            get
            {
                return (double)this.latField;
            }
            set
            {
                this.latField = (decimal)value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double lon
        {
            get
            {
                return (double)this.lonField;
            }
            set
            {
                this.lonField = (decimal)value;
            }
        }

        public override string ToString()
        {
            
                // Calculate total seconds to the nearest 0.1s
                double totalSeconds = time.TimeOfDay.TotalSeconds;
                string formattedSeconds = totalSeconds.ToString("F1"); // Format to 1 decimal place
                // I need the time converted to YY:MM:DD
                string formattedDate = time.ToString("yy:MM:dd");
                double elevationInFeet = (double)ele * 3.28084;
                string formattedElevation = elevationInFeet.ToString("F0");

        
                string output = string.Format("date: {4} time: {0} lat: {1}, lon: {2}, ele: {3}", formattedSeconds, lat, lon, formattedElevation, formattedDate);
                return output;
            
        }
    }


}

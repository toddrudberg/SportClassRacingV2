using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{
    public class cLap
    {
        public double maxSpeed;
        public double elapsedTime;
        public double averageSpeed;
        public double distanceFlown;
    }

    public class cLapCrossings
    {
        public int dataPoint;
        public double crossingTime;
        public cPoint crossingPoint;

        public cLapCrossings(int dataPoint, double crossingTime, cPoint crossingPoint)
        {
            this.dataPoint = dataPoint;
            this.crossingTime = crossingTime;
            this.crossingPoint = crossingPoint;
        }
    }

    public class cRaceData
    {
        public List<racePoint> myRaceData = new List<racePoint>();
        public List<cLap> myLaps = new List<cLap>();

        public void assignCartisianCoordinates(pylonWpt homePylon)
        {
            foreach (racePoint dp in myRaceData)
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


        public void detectLaps(cPylons pylons, out List<cLapCrossings> lapCrossings)
        {
            cPoint homePylon = pylons.homePylonPoint();
            cPoint startFinishPylon = pylons.startFinishPylonPoint();

            //List<cLapCrossings> lapCrossings = new List<cLapCrossings>();
            int crossings = LineCrossingDetector.DetectCrossings(myRaceData, homePylon, startFinishPylon, out lapCrossings);

            if( crossings == 0)
            {
                Console.WriteLine("No laps detected");
                return;
            }
            Console.WriteLine("Lap detection complete");
            Console.WriteLine($"Number of laps detected: {crossings}");
            //truncate myRaceData to only include points between startOfRace and endOfRace
            int startOfRace = lapCrossings[0].dataPoint;
            int endOfRace = lapCrossings[lapCrossings.Count - 1].dataPoint;


            if (lapCrossings.Count > 1)
            {

                for (int i = 1; i < lapCrossings.Count; i++)
                {
                    int startOfLap = lapCrossings[i - 1].dataPoint;
                    int endOfLap = lapCrossings[i].dataPoint;
                    List<racePoint> lapData = myRaceData.GetRange(startOfLap, endOfLap - startOfLap);
                    //calculate lap data
                    cLap lap = new cLap();
                    lap.maxSpeed = lapData.Max(p => p.speedMPH);
                    lap.elapsedTime = lapCrossings[i].crossingTime - lapCrossings[i - 1].crossingTime;
                    lap.averageSpeed = lapData.Average(p => p.speedMPH);
                    lap.distanceFlown = lapData
                        .Zip(lapData.Skip(1), (previous, current) =>
                            Math.Sqrt(Math.Pow(current.X - previous.X, 2) + Math.Pow(current.Y - previous.Y, 2)))
                        .Sum();

                    myLaps.Add(lap);

                    //output lap data to Console:
                    Console.WriteLine();
                    Console.WriteLine($"Lap {i}");
                    Console.WriteLine($"Elapsed Time: {Math.Round(lap.elapsedTime, 2).ToString("F2")} seconds");
                    Console.WriteLine($"PTP Speed: {(pylons.segments.Sum() / lap.elapsedTime * 3600 / 5280).ToString("F4")} mph");
                    Console.WriteLine($"Average Speed: {Math.Round(lap.averageSpeed,4)} mph");
                    Console.WriteLine($"Max Speed: {Math.Round(lap.maxSpeed)} mph");
                    Console.WriteLine($"Distance Flown: {Math.Round(lap.distanceFlown / 5280, 2).ToString("F2")} miles");
                }
                Console.WriteLine();
            }

            //myRaceData = myRaceData.GetRange(startOfRace, endOfRace - startOfRace);
        }

        public void calculateSpeedsAndTruncate(double limitSpeed)
        {
            int lastIndex = 0;
            double lastSpeed = 0;
            myRaceData[0].speedMPH = lastSpeed;

            for (int i = 1; i < myRaceData.Count - 1; i++)
            {
                double distance = Math.Sqrt(Math.Pow(myRaceData[lastIndex].X - myRaceData[i].X, 2) + Math.Pow(myRaceData[lastIndex].Y - myRaceData[i].Y, 2));
                if( distance < 10)
                {
                    myRaceData[i].speedMPH = lastSpeed;
                    continue;
                }

                // Calculate time between two points
                double time = (myRaceData[i].time - myRaceData[lastIndex].time).TotalSeconds;

                // Calculate speed
                double speed = distance / time;
                // convert ft/s to mph
                speed = speed * 3600 / 5280;
                myRaceData[i].speedMPH = speed;
                lastSpeed = speed;
                lastIndex = i;
                //Console.WriteLine($"{Math.Round(speed)} mph");
            }

            //truncate myRaceData to only include points where speed is greater than 100 mph
            myRaceData = myRaceData.Where(p => p.speedMPH > limitSpeed).ToList();
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

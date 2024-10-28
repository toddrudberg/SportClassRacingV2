using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{
    public class cPylons
    {
        public List<pylonWpt> pylonWpts = new List<pylonWpt>();
        public List<double> segments = new List<double>();
        public cPylons() { }

        public pylonWpt homePylon()
        {
            pylonWpt homePylon = pylonWpts.Where(p => p.name == "Home").FirstOrDefault();

            if (homePylon == null)
            {
                Console.WriteLine("didn't find home pylon");
                homePylon = new pylonWpt();
            }

            return homePylon;
        }

        public cPoint homePylonPoint()
        {
            pylonWpt homePylon = this.homePylon(); 
            if (homePylon == null)
            {
                throw new InvalidOperationException("homePylon() returned null");
            }
        
            cPoint homePylonPoint = new cPoint(homePylon.X, homePylon.Y); // Ensure pylonWpt has X and Y properties and cPoint has a matching constructor
            return homePylonPoint;
        }

        public pylonWpt startFinishPylon()
        {
            pylonWpt startFinishPylon = pylonWpts.Where(p => p.name == "StartFinish").FirstOrDefault();
            if (startFinishPylon == null)
            {
                Console.WriteLine("didn't find start finish pylon");
                startFinishPylon = new pylonWpt();
            }
            return startFinishPylon;
        }

        public List<pylonWpt> gatePylons()
        {
            List<pylonWpt> gatePylons = pylonWpts.Where(p => p.name == "Gate1" || p.name == "Gate2").ToList();
            return gatePylons;
        }

        public List<cPoint> gatePylonPoints()
        {
            List<pylonWpt> gatePylons = this.gatePylons();
            List<cPoint> gatePylonPoints = new List<cPoint>();
            foreach (var pylon in gatePylons)
            {
                gatePylonPoints.Add(new cPoint(pylon.X, pylon.Y));
            }
            return gatePylonPoints;
        }

        public List<pylonWpt> outerCoursePylons()
        {
            List<pylonWpt> coursePylons = pylonWpts.Where(p => p.name != "Gate1" && p.name != "Gate2" && p.name != "StartFinish").ToList();
            // remove all points that start with MP and IP
            coursePylons = coursePylons.Where(p => !p.name.StartsWith("MP") && !p.name.StartsWith("IP")).ToList();
            return coursePylons;
        }

        public List<pylonWpt> middleCoursePylons()
        {
            List<pylonWpt> coursePylons = pylonWpts.Where(p => p.name != "Gate1" && p.name != "Gate2" && p.name != "StartFinish").ToList();
            // remove all points that start with MP and IP
            coursePylons = coursePylons.Where(p => !p.name.StartsWith("MP") && !p.name.StartsWith("IP")).ToList();
            return coursePylons;
            
        }
        public cPoint startFinishPylonPoint()
        {
            pylonWpt startFinishPylon = this.startFinishPylon();
            cPoint startFinishPylonPoint = new cPoint(startFinishPylon.X, startFinishPylon.Y);
            return startFinishPylonPoint;
        }

        public void assignCartisianCoordinates(double elevationInFeet = 0)
        {
            var homePylon = this.homePylon();
            
            if (homePylon != null)
            {
                homePylon.X = 0.0;
                homePylon.Y = 0.0;
                Console.WriteLine($"Pylon, X [ft], Y [ft]");
                Console.WriteLine($"{homePylon.name}, {Math.Round(homePylon.X)}, {Math.Round(homePylon.Y)}");
                // let's get a list of all the pylons that are not the home pylon
                var pylons = pylonWpts.Where(p => p.name != "Home").ToList();

                // Convert other pylons relative to the home pylon
                foreach (var pylon in pylons)
                {
                    // Calculate distance and bearing from home pylon to this pylon
                    double distance = cLatLon.HaversineDistance(homePylon.lat, homePylon.lon, pylon.lat, pylon.lon, elevationInFeet);
                    double bearing = cLatLon.CalculateBearing(homePylon.lat, homePylon.lon, pylon.lat, pylon.lon);

                    // Convert polar coordinates (distance, bearing) to Cartesian coordinates
                    pylon.X = distance * Math.Sin(bearing * Math.PI / 180); // X-axis as east-west
                    pylon.Y = distance * Math.Cos(bearing * Math.PI / 180); // Y-axis as north-south


                    Console.WriteLine($"{pylon.name}, {Math.Round(pylon.X)}, {Math.Round(pylon.Y)}");
                }
            }
            else
            {
                Console.WriteLine("Home Pylon not found");
            }
        }

        public int assignSegments()
        {
            int output = 0;
            //let's use the where statement to get all pylons not named "StartFinish"
            var pylons = this.outerCoursePylons();
            //remove Gate1 and Gate2 from the list
            // calculate the distance between each pylon and the next pylon using the X, Y data
            for (int i = 0; i < pylons.Count - 1; i++)
            {
                double distance = Math.Sqrt(Math.Pow(pylons[i].X - pylons[i + 1].X, 2) + Math.Pow(pylons[i].Y - pylons[i + 1].Y, 2));
                segments.Add(distance);
                output++;
            }
            // calculate the distance between the last pylon and the first pylon
            double distance2 = Math.Sqrt(Math.Pow(pylons[pylons.Count - 1].X - pylons[0].X, 2) + Math.Pow(pylons[pylons.Count - 1].Y - pylons[0].Y, 2));
            segments.Add(distance2);
            output++;

            // sum the distances to get the total distance
            double totalDistance = segments.Sum();
            Console.WriteLine($"Total Distance: {totalDistance} ft");
            //convert to miles
            double totalDistanceMiles = totalDistance / 5280;
            Console.WriteLine($"Total Distance: {totalDistanceMiles} miles");

            return output;
        }
    }



    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class pylons
    {
        private ushort eleField;
        private pylonWpt[] wptField;

        private decimal versionField;

        private string creatorField;

        public ushort ele
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

        public double elevationInFeet
        {
            get
            {
                return ele * 3.28084;
            }
        }
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("wpt")]
        public pylonWpt[] wpt
        {
            get
            {
                return this.wptField;
            }
            set
            {
                this.wptField = value;
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string creator
        {
            get
            {
                return this.creatorField;
            }
            set
            {
                this.creatorField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class pylonWpt
    {

        private string nameField;
        private decimal latField;
        private decimal lonField;

        public double X;
        public double Y;

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
            string output = string.Format("{0} ({1}, {2})", name, lat, lon);
            return output;
        }
    }
}

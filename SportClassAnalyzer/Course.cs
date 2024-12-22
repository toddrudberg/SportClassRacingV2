using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{
    public class Course
    {
        public double ElevationInFeet { get; set; }
        public CourseImage CourseImage { get; set; }
        public Pylons Pylons { get; set; }

        public List<double> segments = new List<double>();

        public Waypoint homePylon()
        {
            Waypoint homePylon = Pylons.Waypoints.Where(p => p.Name == "Home").FirstOrDefault();

            if (homePylon == null)
            {
                Console.WriteLine("didn't find home pylon");
                homePylon = new Waypoint();
            }

            return homePylon;
        }
        public cPoint homePylonPoint()
        {
            Waypoint homePylon = this.homePylon();
            if (homePylon == null)
            {
                throw new InvalidOperationException("homePylon() returned null");
            }

            cPoint homePylonPoint = new cPoint(homePylon.X, homePylon.Y); // Ensure pylonWpt has X and Y properties and cPoint has a matching constructor
            return homePylonPoint;
        }

        public Waypoint startFinishPylon()
        {
            Waypoint startFinishPylon = Pylons.Waypoints.Where(p => p.Name == "StartFinish").FirstOrDefault();
            if (startFinishPylon == null)
            {
                Console.WriteLine("didn't find start finish pylon");
                startFinishPylon = new Waypoint();
            }
            return startFinishPylon;
        }

        public List<Waypoint> gatePylons()
        {
            List<Waypoint> gatePylons = Pylons.Waypoints.Where(p => p.Name == "Gate1" || p.Name == "Gate2").ToList();
            return gatePylons;
        }

        public List<cPoint> gatePylonPoints()
        {
            List<Waypoint> gatePylons = this.gatePylons();
            List<cPoint> gatePylonPoints = new List<cPoint>();
            foreach (var pylon in gatePylons)
            {
                gatePylonPoints.Add(new cPoint(pylon.X, pylon.Y));
            }
            return gatePylonPoints;
        }

        public List<Waypoint> outerCoursePylons()
        {
            List<Waypoint> coursePylons = Pylons.Waypoints.Where(p => p.Name != "Gate1" && p.Name != "Gate2" && p.Name != "StartFinish").ToList();
            // remove all points that start with MP and IP
            coursePylons = coursePylons.Where(p => !p.Name.StartsWith("mP") && !p.Name.StartsWith("iP")).ToList();
            return coursePylons;
        }

        public List<Waypoint> middleCoursePylons()
        {
            List<Waypoint> coursePylons = Pylons.Waypoints.Where(p => p.Name != "Gate1" && p.Name != "Gate2" && p.Name != "StartFinish").ToList();
            // remove all points that start with MP and IP
            coursePylons = coursePylons.Where(p => !p.Name.StartsWith("oP") && !p.Name.StartsWith("iP")).ToList();
            return coursePylons;

        }
        public List<Waypoint> innerCoursePylons()
        {
            List<Waypoint> coursePylons = Pylons.Waypoints.Where(p => p.Name != "Gate1" && p.Name != "Gate2" && p.Name != "StartFinish").ToList();
            // remove all points that start with MP and IP
            coursePylons = coursePylons.Where(p => !p.Name.StartsWith("oP") && !p.Name.StartsWith("mP")).ToList();
            return coursePylons;

        }

        public List<Waypoint> startPylons(cFormState.CourseType courseType, bool forDisplay = false)
        {
            List<Waypoint> coursePylons = outerCoursePylons().Select(p => p.Clone()).ToList();

            coursePylons = coursePylons.Where(p => !p.Name.StartsWith("oP1")).ToList();

            if (forDisplay)
            {
                //remove the Home pylon
                coursePylons = coursePylons.Where(p => p.Name != "Home").ToList();

                // rename replace the o with an s in all the names
                foreach (var pylon in coursePylons)
                {
                    pylon.Name = pylon.Name.Replace("o", "s");
                }

                if (courseType == cFormState.CourseType.Middle)
                {
                    // replace the name for s2, s3, s4, Home with ""
                    foreach (var pylon in coursePylons)
                    {
                        if (pylon.Name == "sP2" || pylon.Name == "sP3" || pylon.Name == "sP7" || pylon.Name == "sP4" || pylon.Name == "Home")
                        {
                            pylon.Name = "";
                        }
                    }

                }
            }
            return coursePylons;
        }

        public cPoint startFinishPylonPoint()
        {
            Waypoint startFinishPylon = this.startFinishPylon();
            cPoint startFinishPylonPoint = new cPoint(startFinishPylon.X, startFinishPylon.Y);
            return startFinishPylonPoint;
        }

        public void assignCartisianCoordinates(double elevationInFeet = 0)
        {
            Waypoint homePylon = this.homePylon();

            if (homePylon != null)
            {
                homePylon.X = 0.0;
                homePylon.Y = 0.0;
                Console.WriteLine($"Pylon, X [ft], Y [ft]");
                Console.WriteLine($"{homePylon.Name}, {Math.Round(homePylon.X)}, {Math.Round(homePylon.Y)}");
                // let's get a list of all the pylons that are not the home pylon
                var pylons = Pylons.Waypoints.Where(p => p.Name != "Home").ToList();

                // Convert other pylons relative to the home pylon
                foreach (var pylon in pylons)
                {
                    // Calculate distance and bearing from home pylon to this pylon
                    double distance = cLatLon.HaversineDistance(homePylon.Latitude, homePylon.Longitude, pylon.Latitude, pylon.Longitude, elevationInFeet);
                    double bearing = cLatLon.CalculateBearing(homePylon.Latitude, homePylon.Longitude, pylon.Latitude, pylon.Longitude);

                    // Convert polar coordinates (distance, bearing) to Cartesian coordinates
                    pylon.X = distance * Math.Sin(bearing * Math.PI / 180); // X-axis as east-west
                    pylon.Y = distance * Math.Cos(bearing * Math.PI / 180); // Y-axis as north-south


                    Console.WriteLine($"{pylon.Name}, {Math.Round(pylon.X)}, {Math.Round(pylon.Y)}");
                }
            }
            else
            {
                Console.WriteLine("Home Pylon not found");
            }
        }

        public int assignSegments(cFormState formState)
        {
            int output = 0;
            var pylons = this.outerCoursePylons();
            segments.Clear();
            switch (formState.courseType)
            {
                case cFormState.CourseType.Inner:
                    pylons = innerCoursePylons();
                    break;
                case cFormState.CourseType.Middle:
                    pylons = middleCoursePylons();
                    break;
                case cFormState.CourseType.Outer:
                    pylons = outerCoursePylons();
                    break;
            }
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

        public static Course LoadCourseFile(string courseFile)
        {
            try
            {
                // Ensure the file exists
                if (!File.Exists(courseFile))
                {
                    throw new FileNotFoundException($"The file '{courseFile}' was not found.");
                }

                // Read the JSON content from the file
                string jsonContent = File.ReadAllText(courseFile);

                // Deserialize the JSON content into a Course object
                CourseWrapper cw = JsonSerializer.Deserialize<CourseWrapper>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // To handle case differences in property names
                });

                // Ensure the deserialization was successful
                if (cw == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the course file.");
                }

                return cw.Course;
            }
            catch (Exception ex)
            {
                // Log the error and rethrow or handle as needed
            Console.WriteLine($"Error loading course file: {ex.Message}");
                throw;
            }
        }
    }

    public class CourseWrapper
    {
        public Course Course { get; set; }
    }

    public class CourseImage
    {
        public string ImageFile { get; set; }
        public Coordinates UpperLeft { get; set; }
        public Coordinates LowerRight { get; set; }
    }

    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Pylons
    {
        public List<Waypoint> Waypoints { get; set; }
    }

    public class Waypoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Waypoint Clone()
        {
            return new Waypoint
            {
                Name = this.Name,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                X = this.X,
                Y = this.Y
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{

    public class _2dVector
    {
    }

    public class cPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public cPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class LineCrossingDetector
    {
        public static int DetectCrossings(List<racePoint> dataPoints, cPoint lineStart, cPoint lineEnd, out List<cLapCrossings> lapCrossings)
        {
            var crossings = new List<cPoint>();
            lapCrossings = new List<cLapCrossings>();

            for (int i = 0; i < dataPoints.Count - 1; i++)
            {
                cPoint p1 = new cPoint(dataPoints[i].X, dataPoints[i].Y);
                cPoint p2 = new cPoint(dataPoints[i + 1].X, dataPoints[i + 1].Y);

                if (DoLinesIntersect(p1, p2, lineStart, lineEnd, out cPoint intersection))
                {
                    
                    racePoint rp1 = dataPoints[i];
                    racePoint rp2 = dataPoints[i + 1];
                    //let's calculate the distance between p1 and intersection
                    double distance1 = Math.Sqrt(Math.Pow(intersection.X - rp1.X, 2) + Math.Pow(intersection.Y - rp1.Y, 2));
                    //let's calculate the distance between p2 and intersection
                    double distance2 = Math.Sqrt(Math.Pow(intersection.X - rp2.X, 2) + Math.Pow(intersection.Y - rp2.Y, 2));
                    //let's interpolate the time of the intersection
                    TimeSpan time1 = rp1.time.TimeOfDay;
                    TimeSpan time2 = rp2.time.TimeOfDay;
                    double dtime1 = time1.TotalSeconds;
                    double dtime2 = time2.TotalSeconds;
                    double dtime = dtime1 + (dtime2 - dtime1) * distance1 / (distance1 + distance2);
                    lapCrossings.Add(new cLapCrossings(i, dtime, intersection));
                    crossings.Add(intersection);
                    Console.WriteLine($"Crossing detected at: X={intersection.X}, Y={intersection.Y}");
                }
            }

            return crossings.Count;
        }

        // Check if two line segments (p1-p2 and q1-q2) intersect
        private static bool DoLinesIntersect(cPoint p1, cPoint p2, cPoint q1, cPoint q2, out cPoint intersection)
        {
            intersection = null;

            // Calculate the orientation of three points
            double d1 = Direction(q1, q2, p1);
            double d2 = Direction(q1, q2, p2);
            double d3 = Direction(p1, p2, q1);
            double d4 = Direction(p1, p2, q2);

            // General case: if d1 and d2 have opposite signs, and d3 and d4 have opposite signs
            if (d1 * d2 < 0 && d3 * d4 < 0)
            {
                intersection = CalculateIntersection(p1, p2, q1, q2);
                return true;
            }

            return false;
        }

        // Calculate the direction for orientation
        private static double Direction(cPoint a, cPoint b, cPoint c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        // Calculate the exact intersection point (assumes lines intersect)
        private static cPoint CalculateIntersection(cPoint p1, cPoint p2, cPoint q1, cPoint q2)
        {
            double a1 = p2.Y - p1.Y;
            double b1 = p1.X - p2.X;
            double c1 = a1 * p1.X + b1 * p1.Y;

            double a2 = q2.Y - q1.Y;
            double b2 = q1.X - q2.X;
            double c2 = a2 * q1.X + b2 * q1.Y;

            double determinant = a1 * b2 - a2 * b1;

            if (Math.Abs(determinant) < 1e-6)
            {
                // Lines are parallel, no intersection
                return null;
            }

            double x = (b2 * c1 - b1 * c2) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;

            return new cPoint(x, y);
        }
    }

    public class cLatLon
    {
        private const double EarthRadiusFeet = 20925524.9; // Earth's radius in feet

        // Haversine formula to calculate the great-circle distance
        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2, double altitude = 0)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            lat1 = lat1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (EarthRadiusFeet + altitude) * c;
        }

        // Calculate the bearing from point A to point B
        public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
        {
            lat1 = lat1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            double bearing = Math.Atan2(y, x) * 180 / Math.PI; // Convert radians to degrees

            return (bearing + 360) % 360; // Normalize to 0-360 degrees
        }
    }
}

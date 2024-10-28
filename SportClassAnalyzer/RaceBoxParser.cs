using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    class cRaceBoxRecord
    {
        public int RecordNumber { get; set; }
        public DateTime Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Speed { get; set; }
        public double GForceX { get; set; }
        public double GForceY { get; set; }
        public double GForceZ { get; set; }
        public int Lap { get; set; }
        public double GyroX { get; set; }
        public double GyroY { get; set; }
        public double GyroZ { get; set; }
    }

    class cRaceBoxParser
    {
    //     static void Main()
    //     {
    //         string csvData = @"Record,Time,Latitude,Longitude,Altitude,Speed,GForceX,GForceY,GForceZ,Lap,GyroX,GyroY,GyroZ
    // 1,2024-10-19T15:24:58.400,32.2848992,-106.9305228,4441.0,3.17,0.023,-0.019,1.007,0,0.41,-0.44,-0.18
    // 2,2024-10-19T15:24:58.440,32.2848996,-106.9305226,4441.0,3.12,0.039,-0.017,1.009,0,0.44,-0.32,-0.45
    // 3,2024-10-19T15:24:58.480,32.2849001,-106.9305224,4441.0,3.27,0.052,-0.015,1.007,0,0.48,-0.22,-0.69
    // 4,2024-10-19T15:24:58.520,32.2849005,-106.9305222,4441.0,3.17,0.064,-0.015,1.007,0,0.45,-0.04,-0.89";

    //         // Parse CSV data
    //         List<Record> records = ParseCsv(csvData);

    //         // Output the parsed data
    //         foreach (var record in records)
    //         {
    //             Console.WriteLine($"Record {record.RecordNumber}: Time={record.Time}, Latitude={record.Latitude}, Longitude={record.Longitude}, Speed={record.Speed}");
    //         }
    //     }

        public static List<cRaceBoxRecord> ParseCsv(string csvData)
        {
            List<cRaceBoxRecord> records = new List<cRaceBoxRecord>();
            
            // open csvData file and read all lines
            string[] lines = System.IO.File.ReadAllLines(csvData);


            // Skip the header line (first line)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                // Parse each field into the Record object
                var record = new cRaceBoxRecord
                {
                    RecordNumber = int.Parse(values[0]),
                    Time = DateTime.Parse(values[1], CultureInfo.InvariantCulture),
                    Latitude = double.Parse(values[2], CultureInfo.InvariantCulture),
                    Longitude = double.Parse(values[3], CultureInfo.InvariantCulture),
                    Altitude = double.Parse(values[4], CultureInfo.InvariantCulture),
                    Speed = double.Parse(values[5], CultureInfo.InvariantCulture),
                    GForceX = double.Parse(values[6], CultureInfo.InvariantCulture),
                    GForceY = double.Parse(values[7], CultureInfo.InvariantCulture),
                    GForceZ = double.Parse(values[8], CultureInfo.InvariantCulture),
                    Lap = int.Parse(values[9]),
                    GyroX = double.Parse(values[10], CultureInfo.InvariantCulture),
                    GyroY = double.Parse(values[11], CultureInfo.InvariantCulture),
                    GyroZ = double.Parse(values[12], CultureInfo.InvariantCulture)
                };

                // Add the record to the list
                records.Add(record);
            }

            return records;
        }
        public static gpx ConvertRaceBoxToGpx(List<cRaceBoxRecord> raceBoxRecords)
        {
            gpx raceData = new gpx();

            raceData.trk = new gpxTrk();

            List<racePoint> trackPoints = raceBoxRecords.Select(record => new racePoint
            {
                time = record.Time,
                lat = record.Latitude,
                lon = record.Longitude,
                ele = (decimal)record.Altitude
            }).ToList();

            raceData.trk.trkseg = trackPoints.ToArray();

            return raceData;
        }

    }

}

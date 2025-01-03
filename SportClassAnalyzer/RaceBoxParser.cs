﻿using System;
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
                ele = (decimal)(record.Altitude / 3.28084)
            }).ToList();

            raceData.trk.trkseg = trackPoints.ToArray();

            return raceData;
        }

    }

}

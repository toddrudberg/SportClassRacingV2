using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{
    public class cFormState
    {
        //"C:\LocalDev\SportClassRacingV2\RaceColville.gpx"
        public string sCourseFile { get; set; } = @"C:\LocalDev\SportClassRacingV2\SportClassOuterCourse - Middle.gpx";
        public string sRaceCourseImageFile { get; set; } = @"C:\LocalDev\SportClassRacingV2\SportClassOuterCourse.png";
        //public string sPylonFile { get; set; } = @"C:\LocalDev\SportClassRacingV2\RaceColville.gpx";
        
        public string sRaceDataFile { get; set; } = @"C:\LocalDev\SportClassRacing\Slater Data\20241018_142045.gpx";
        public bool showStartLap { get; set; } = true;
        public CourseType courseType { get; set; } = CourseType.Outer;


        public enum CourseType
        {
            Inner,
            Middle,
            Outer
        }

        // Method to get the file path in the application's AppData folder
        private static string GetFilePath()
        {
            // Base folder: AppData\Roaming
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Application-specific subfolder
            string appFolder = Path.Combine(baseFolder, "SportClassAnalyzer");

            // Ensure the directory exists
            Directory.CreateDirectory(appFolder);

            // Generate a unique filename based on date and time
            string fileName = $"formState.json";

            // Combine the folder and filename
            return Path.Combine(appFolder, fileName);
        }

        // Save method to serialize the object state to a JSON file in the generated path
        public void Save()
        {
            string filePath = GetFilePath();
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new JsonStringEnumConverter());  // Convert enums to strings

                string jsonString = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving state: {ex.Message}");
            }
        }

        // Load method to deserialize the object state from a specific file path
        public static cFormState Load()
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new JsonStringEnumConverter());  // Convert enums from strings

                    string jsonString = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<cFormState>(jsonString, options);
                }
                else
                {
                    Console.WriteLine("File not found. Loading default state.");
                    return new cFormState();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading state: {ex.Message}");
                return new cFormState();
            }
        }
    }


}

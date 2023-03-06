using Newtonsoft.Json;

namespace TempratureWidget
{
    public class Calculate
    {
        public static string EvaluateLogFile(string logContentsStr)
        {
            // Read the log file
            string[] lines = File.ReadAllLines(logContentsStr);

            // Parse the reference values for temperature, humidity, and carbon monoxide
            string[] reference = lines[0].Split(' ');
            double referenceTemprature = double.Parse(reference[1]);
            double referenceHumidity = double.Parse(reference[2]);
            int referenceCarbonMonoxide = int.Parse(reference[3]);

            // Create dictionaries to store sensor data
            Dictionary<string, List<double>> temperatureReadings = new Dictionary<string, List<double>>();
            Dictionary<string, List<double>> humidityReadings = new Dictionary<string, List<double>>();
            Dictionary<string, List<int>> monoxideReadings = new Dictionary<string, List<int>>();

            string lastType = string.Empty;
            string lastReferenceType = string.Empty;

            // Process the log file line by line
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(' ');

                if (parts[0] == "thermometer")
                {
                    lastReferenceType = parts[0];
                    // Add temperature reading to the appropriate thermometer
                    if (!temperatureReadings.ContainsKey(parts[1]))
                    {
                        lastType = parts[1];
                        temperatureReadings[parts[1]] = new List<double>();
                    }
                }
                else if (parts[0] == "humidity")
                {
                    lastReferenceType = parts[0];
                    // Add humidity reading to the appropriate humidity sensor
                    if (!humidityReadings.ContainsKey(parts[1]))
                    {
                        lastType = parts[1];
                        humidityReadings[parts[1]] = new List<double>();
                    }
                }
                else if (parts[0] == "monoxide")
                {
                    lastReferenceType = parts[0];
                    // Add carbon monoxide reading to the appropriate detector;
                    if (!monoxideReadings.ContainsKey(parts[1]))
                    {
                        lastType = parts[1];
                        monoxideReadings[parts[1]] = new List<int>();
                    }
                }
                else if (lastReferenceType == "thermometer")
                {
                    double value = double.Parse(parts[1]);
                    temperatureReadings[lastType].Add(value);
                }
                else if (lastReferenceType == "humidity")
                {
                    double value = double.Parse(parts[1]);
                    humidityReadings[lastType].Add(value);
                }
                else if (lastReferenceType == "monoxide")
                {
                    int value = int.Parse(parts[1]);
                    monoxideReadings[lastType].Add(value);
                }
            }

            // Evaluate the sensors based on the given criteria
            Dictionary<string, string> sensorEvaluation = new Dictionary<string, string>();

            foreach (KeyValuePair<string, List<double>> pair in temperatureReadings)
            {
                double mean = CalculateMean(pair.Value);
                double stdDev = CalculateStdDev(pair.Value);

                if (Math.Abs(mean - referenceTemprature) <= 0.5 && stdDev < 3)
                {
                    sensorEvaluation[pair.Key] = "ultra precise";
                }
                else if (Math.Abs(mean - referenceTemprature) <= 0.5 && stdDev < 5)
                {
                    sensorEvaluation[pair.Key] = "very precise";
                }
                else
                {
                    sensorEvaluation[pair.Key] = "precise";
                }
            }

            foreach (KeyValuePair<string, List<double>> pair in humidityReadings)
            {
                bool isWithinRange = true;

                foreach (double reading in pair.Value)
                {
                    if (Math.Abs(referenceHumidity - reading) > 1)
                    {
                        isWithinRange = false;
                        break;
                    }
                }

                if (isWithinRange)
                {
                    sensorEvaluation[pair.Key] = "keep";
                }
                else
                {
                    sensorEvaluation[pair.Key] = "discard";
                }
            }

            foreach (KeyValuePair<string, List<int>> pair in monoxideReadings)
            {
                bool isWithinRange = true;

                foreach (int reading in pair.Value)
                {
                    if (Math.Abs(referenceCarbonMonoxide - reading) > 3)
                    {
                        isWithinRange = false;
                        break;
                    }
                }

                if (isWithinRange)
                {
                    sensorEvaluation[pair.Key] = "keep";
                }
                else
                {
                    sensorEvaluation[pair.Key] = "discard";
                }
            }

            return JsonConvert.SerializeObject(sensorEvaluation, Formatting.Indented);
        }

        private static double CalculateMean(List<double> values)
        {
            double sum = 0;
            foreach (double value in values)
            {
                sum += value;
            }
            return sum / values.Count;
        }

        private static double CalculateStdDev(List<double> values)
        {
            double mean = CalculateMean(values);
            double variance = 0;
            foreach (double value in values)
            {
                variance += Math.Pow(value - mean, 2);
            }
            variance /= values.Count;
            return Math.Sqrt(variance);
        }
    }
}
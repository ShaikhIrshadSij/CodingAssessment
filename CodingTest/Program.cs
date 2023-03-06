using TempratureWidget;

static void ProcessLog()
{
    string logFilePath = $"{AppContext.BaseDirectory.Replace("\\bin\\Debug\\net6.0\\", "")}/LogFile/CodingTest.txt";
    string response = Calculate.EvaluateLogFile(logFilePath);
    Console.WriteLine(response);
}



ProcessLog();
Console.ReadKey();
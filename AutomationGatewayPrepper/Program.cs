using CsvHelper;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AutomationGatewayPrepper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AutomationGatewayPrepper Starting...");

            using var reader = new StreamReader(@"C:\Users\Javier\Desktop\test.csv");
            using var csv = new CsvReader(reader);
            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => Regex.Replace(header, @"\s", string.Empty);
            var records = csv.GetRecords<Configuration>();

            foreach (var item in records)
            {
                Console.Write("hurray");
            }
        }
    }
}

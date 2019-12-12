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

            using var reader = new StreamReader(@"C:\Users\Javier\Desktop\Export.csv");
            using var csv = new CsvReader(reader);
            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => Regex.Replace(header, @"\s", string.Empty);
            csv.Configuration.Delimiter = ",";
            csv.Configuration.BadDataFound = x =>
            {
                Console.WriteLine($"Bad Datarow: {x.RawRecord}");
            };

            var records = csv.GetRecords<Configuration>();            
            foreach (var item in records)
            {
                Console.Write("hurray");
            }
        }
    }
}

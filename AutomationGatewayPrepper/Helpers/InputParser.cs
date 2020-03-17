using System;
using System.IO;

namespace AutomationGatewayPrepper.Helpers
{
    public static class InputParser
    {
        public static bool Read(string[] args, out string inputFile, out string outputFile)
        {
            // TODO: This could be greatly improved.
            inputFile = outputFile = null;
            string outputFileName, outputDir;
            if (args == null || args.Length < 1)
                return Error("Input file must be specified.");

            inputFile = args[0];
            if (!File.Exists(inputFile))
                return Error("Input file '{0}' does not exist.", inputFile);


            ReadOutputFileComponents(args, out outputFileName, out outputDir);
            outputFile = Path.Combine(outputDir, outputFileName);

            return true;
        }

        static void ReadOutputFileComponents(string[] args, out string fileName, out string dir)
        {
            dir = ".";
            fileName = "AutoGate_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".csv";

            if (args.Length < 2)
                return;

            var outArg = args[1];

            var outFile = new FileInfo(outArg);
            if (!outFile.Directory.Exists)
                return;

            dir = outFile.Directory.FullName;
            if (outArg.ToLower().EndsWith(".csv"))
                fileName = outFile.Name;
        }

        static bool Error(string message, params object[] args)
        {
            string error;
            try { error = string.Format(message, args); }
            catch { error = message; }
            Console.WriteLine("Error: {0}", error);
            return false;
        }
    }
}

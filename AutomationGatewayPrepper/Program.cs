using AutomationGatewayPrepper.Entities.Input;
using AutomationGatewayPrepper.Entities.Output;
using AutomationGatewayPrepper.Helpers;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutomationGatewayPrepper
{
    class Program
    {
        private const string _fic = "FIC_";
        private const string _fis = "FIS_";
        private const string _global = "GLOBAL.";
        private const string _user = "USER_";
        static void Main(string[] args)
        {
            Console.WriteLine("AutomationGatewayPrepper Starting...");

            
            var automationNodeTypeParameters = new List<AutomationNodeTypeParameter>();
            var automationNodeInstances = new List<AutomationNodeInstance>();
            var automationNodeInstanceParameters = new List<AutomationNodeInstanceParameter>();

            using var reader = new StreamReader(@"C:\Users\Javier\Desktop\Export.csv");
            using var csv = new CsvReader(reader);
            ConfigureCsvReader(csv);

            //Yields one record at a time. Entire file is not loaded into memory.
            var records = csv.GetRecords<Configuration>();

            var automationChannels = PopulateAutomationChannels();
            var automationNodeTypes = GetAutomationNodeTypes(records);

            foreach (var row in records)
            {
                if (!String.IsNullOrEmpty(row.Address))
                {
                    //Populate automationNodeTypeParameters
                    AutomationNodeTypeParameter antp = GetAutomationNodeTypes(automationNodeTypes, row);
                    automationNodeTypeParameters.Add(antp);
                }
                else
                {
                    //TODO log bad records
                }
            }

            //Generate csv file to import into UADM
            var randomFileName = "AutoGate_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".csv";
            using (var writer = new StreamWriter(@"C:\Source\Work\AutomationGatewayPrepper\AutomationGatewayPrepper\Output\" + randomFileName))
            using (var csvWriter = new CsvWriter(writer))
            {
                csvWriter.Configuration.ShouldQuote = (field, context) => true;
                csvWriter.WriteField("#AutomationChannels");
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationChannels);
                csvWriter.WriteField("#AutomationNodeTypes");
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeTypes);
                csvWriter.WriteField("#AutomationNodeTypeParameter");
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeTypeParameters);
                csvWriter.WriteField("#AutomationNodeInstances");
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeInstances);
                csvWriter.WriteField("#AutomationNodeInstanceParameters");
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeInstanceParameters);
            }
        }

        private static List<AutomationNodeType> GetAutomationNodeTypes(IEnumerable<Configuration> records)
        {
            HashSet<string> unqiueNodeTypes = new HashSet<string>();
            List<AutomationNodeType> result = new List<AutomationNodeType>();
            foreach (var item in records)
            {
                if (!String.IsNullOrWhiteSpace(item.Namespace))
                {
                    unqiueNodeTypes.Add(item.Namespace);
                }
            }

            foreach (var nodeTypes in unqiueNodeTypes)
            {
                AutomationNodeType ant = new AutomationNodeType();
                ant.Id = nodeTypes;
                ant.Name = nodeTypes;
                ant.Description = nodeTypes;
                result.Add(ant);
            }

            return result;
        }

        /// <summary>
        /// FIX - sTART HERE
        /// </summary>
        /// <param name="automationNodeTypes"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static AutomationNodeTypeParameter GetAutomationNodeTypes(List<AutomationNodeType> automationNodeTypes, Configuration row)
        {
            
            var antp = new AutomationNodeTypeParameter();
            if (row.AStagname.ToLower().Contains(_fis.ToLower()))
            {
                antp.AutomationNodeTypeRef = automationNodeTypes.Where(x => x.Name == _fis).First().Id;
            }
            else
            {
                antp.AutomationNodeTypeRef = automationNodeTypes.Where(x => x.Name == _fic).First().Id;
            }
            //else if (row.AStagname.ToLower().Contains(_global.ToLower()))
            //{
            //    antp.AutomationNodeTypeRef = automationNodeTypes.Where(x => x.Name == _global).First().Id;
            //}
            //else if (row.AStagname.ToLower().Contains(_user.ToLower()))
            //{
            //    antp.AutomationNodeTypeRef = automationNodeTypes.Where(x => x.Name == _user).First().Id;
            //}
            //else
            //{
            //    throw new ArgumentException($"AutomationNodeType not matched for {row.AStagname}.");
            //}
            antp.Id = row.AStagname.Replace('.', '_');
            antp.DataType = DataTypeHelper.GetDataType(row.Datatype);
            antp.MinValue = row.Lowlimit;
            antp.MaxValue = row.Highlimit;
            antp.ParameterValue = row.Startvalue;
            antp.IsInternal = bool.FalseString;
            antp.IsStatic = bool.FalseString;
            antp.IsPersistent = bool.FalseString;
            return antp;
        }

        private static void ConfigureCsvReader(CsvReader csv)
        {
            //Remove spaces in header row
            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => Regex.Replace(header, @"\s", string.Empty);
            csv.Configuration.Delimiter = ",";

            //If any bad data is found (bad encoding/ character(s), etc...) write it out to console window
            csv.Configuration.BadDataFound = x =>
            {
                Console.WriteLine($"Bad Datarow: {x.RawRecord}");
            };
        }

        private static List<AutomationChannel> PopulateAutomationChannels()
        {
            var automationChannels = new List<AutomationChannel>();
            var ac = new AutomationChannel();
            ac.Id = "UaWrapper";
            ac.Name = "UA DCOM Wrapper";
            ac.ConnectionFamilyType = "Undefined";
            ac.ConnectionType = "OPCUA";
            ac.IdentityTokenType = "Anonymous";
            ac.Address = "opc.tcp://MOMDEVHISTSRV:48050";
            ac.SecurityPolicy = "None";
            ac.MessageSecurityMode = "None";
            automationChannels.Add(ac);

            ac = new AutomationChannel();
            ac.Id = "WinCC";
            ac.Name = "WinCC OPC";
            ac.ConnectionFamilyType = "Undefined";
            ac.ConnectionType = "OPCUA";
            ac.IdentityTokenType = "Anonymous";
            ac.Address = "opc.tcp://AMFBAY2CONTROL:4862";
            ac.SecurityPolicy = "None";
            ac.MessageSecurityMode = "None";
            automationChannels.Add(ac);
            return automationChannels;
        }
    }
}

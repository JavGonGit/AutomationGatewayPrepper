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
            var automationNodeTypes = new List<AutomationNodeType>();
            var unqiueNodeTypes = new HashSet<string>();
            var records = csv.GetRecords<Configuration>();
            foreach (var row in records)
            {
                if (!String.IsNullOrEmpty(row.Address))
                {
                    //Get unique NodeTypes                    
                    if (!String.IsNullOrWhiteSpace(row.Namespace))
                    {
                        unqiueNodeTypes.Add(row.Namespace);
                    }

                    //Populate automationNodeTypeParameters
                    AutomationNodeTypeParameter antp = AutomationNodeTypeParameters(automationNodeTypes, row);
                    automationNodeTypeParameters.Add(antp);

                    //Populate AutomationNodeInstances

                }
                else
                {
                    //TODO log bad records
                }
            }

            // Automation Channels
            var automationChannels = PopulateAutomationChannels();

            //Add Unique list of Node Types
            automationNodeTypes = GetAutomationNodeTypes(unqiueNodeTypes);

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
                csvWriter.WriteField("#AutomationNodeTypeParameters");
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

        private static List<AutomationNodeType> GetAutomationNodeTypes(HashSet<string> unqiueNodeTypes)
        {
            List<AutomationNodeType> automationNodeTypes = new List<AutomationNodeType>();
            foreach (var item in unqiueNodeTypes)
            {
                AutomationNodeType ant = new AutomationNodeType();
                ant.Id = item;
                ant.Name = item;
                ant.Description = item;
                automationNodeTypes.Add(ant);
            }
            return automationNodeTypes;
        }

        private static List<AutomationNodeType> GetAutomationNodeTypes(IEnumerable<Configuration> records)
        {
            HashSet<string> unqiueNodeTypes = new HashSet<string>();
            List<AutomationNodeType> result = new List<AutomationNodeType>();
            foreach (var item in records)
            {
                //Can improve by using HashSet check here and adding to automationNodeType if successful instead of looping twice
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
        private static AutomationNodeTypeParameter AutomationNodeTypeParameters(List<AutomationNodeType> automationNodeTypes, Configuration row)
        {
            
            var antp = new AutomationNodeTypeParameter();
            antp.AutomationNodeTypeRef = row.Namespace;
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

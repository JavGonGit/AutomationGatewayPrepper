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
        private const string _WINCC = "WinCC";
        static void Main(string[] args)
        {
            Console.WriteLine("AutomationGatewayPrepper Starting...");

            using var reader = new StreamReader(@"C:\Users\Javier\Desktop\Export.csv");
            using var csv = new CsvReader(reader);
            ConfigureCsvReader(csv);

            var automationChannels = PopulateAutomationChannels();
            var automationNodeTypeParameters = new List<AutomationNodeTypeParameter>();
            var automationNodeInstances = new List<AutomationNodeInstance>();
            var automationNodeInstanceParameters = new List<AutomationNodeInstanceParameter>();
            var automationNodeTypes = new List<AutomationNodeType>();
            var unqiueNodeTypes = new HashSet<string>();

            //Yields one record at a time. Entire file is not loaded into memory.
            var records = csv.GetRecords<Configuration>();
            foreach (var row in records)
            {
                if (!String.IsNullOrEmpty(row.Address) && !String.IsNullOrWhiteSpace(row.Namespace))
                {
                    unqiueNodeTypes.Add(row.Namespace);
                    automationNodeTypeParameters.Add(GetAutomationNodeTypeParameters(row));
                    automationNodeInstanceParameters.Add(GetAutomationNodeInstanceParameters(automationChannels, row));
                }
                else
                {
                    //TODO log bad records
                }
            }

            //Add Unique list of Node Types
            automationNodeTypes = GetAutomationNodeTypes(unqiueNodeTypes);

            //Add Unique list of AutomationNodeInstances
            automationNodeInstances = GetAutomationNodeInstance(unqiueNodeTypes);

            //Generate csv file to import into UADM
            GenerateAutomationGatewayCsv(automationChannels, automationNodeTypeParameters, automationNodeInstances, automationNodeInstanceParameters, automationNodeTypes);
        }


        private static void GenerateAutomationGatewayCsv(List<AutomationChannel> automationChannels, List<AutomationNodeTypeParameter> automationNodeTypeParameters, List<AutomationNodeInstance> automationNodeInstances, List<AutomationNodeInstanceParameter> automationNodeInstanceParameters, List<AutomationNodeType> automationNodeTypes)
        {
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
                csvWriter.WriteHeader(typeof(AutomationNodeType));
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeTypes);
                csvWriter.WriteField("#AutomationNodeTypeParameters");
                csvWriter.NextRecord();
                csvWriter.WriteHeader(typeof(AutomationNodeTypeParameter));
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeTypeParameters);
                csvWriter.WriteField("#AutomationNodeInstances");
                csvWriter.NextRecord();
                csvWriter.WriteHeader(typeof(AutomationNodeInstance));
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeInstances);
                csvWriter.WriteField("#AutomationNodeInstanceParameters");
                csvWriter.NextRecord();
                csvWriter.WriteHeader(typeof(AutomationNodeInstanceParameter));
                csvWriter.NextRecord();
                csvWriter.WriteRecords(automationNodeInstanceParameters);
            }
        }

        private static AutomationNodeInstanceParameter GetAutomationNodeInstanceParameters(List<AutomationChannel> automationChannels, Configuration row)
        {
            const string ZERO_STRING = "0";
            const string ADDRESS_TYPE = "NodeId";
            const string ADDRESS_PREFIX = "ns=5;s=";
            const string AQUISITION_MODE = "OnChange";
            const string SOOTHING_MODE = "None";

            AutomationNodeInstanceParameter anip = new AutomationNodeInstanceParameter
            {
                AutomationNodeInstanceRef = row.Namespace,
                Id = row.Name,
                ParameterValue = ZERO_STRING,
                ChannelNId = automationChannels.Where(x => x.Name.Contains(_WINCC)).First().Id,
                AddressType = ADDRESS_TYPE,
                Address = ADDRESS_PREFIX + row.Namespace + '_' + row.AStagname,
                AcquisitionMode = AQUISITION_MODE,
                //AcquisitionCycleNId = ,
                SmoothingMode = SOOTHING_MODE,
                //DeltaValue = "",
                //DeltaTime = "",

            };
            return anip;
        }

        private static List<AutomationNodeInstance> GetAutomationNodeInstance(HashSet<string> unqiueNodeTypes)
        {
            var instanceList = new List<AutomationNodeInstance>();
            foreach (var nodeType in unqiueNodeTypes)
            {
                instanceList.Add(new AutomationNodeInstance
                {
                    AutomationNodeTypeRef = nodeType,
                    Id = nodeType,
                    Name = nodeType,
                    Description = nodeType
                });
            }
            return instanceList;
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

        private static AutomationNodeTypeParameter GetAutomationNodeTypeParameters(Configuration row)
        {
            var antp = new AutomationNodeTypeParameter();
            antp.AutomationNodeTypeRef = row.Namespace;
            antp.Id = row.Name;
            antp.DataType = DataTypeHelper.GetDataType(row.Datatype);
            antp.MinValue = row.Lowlimit;
            antp.MaxValue = row.Highlimit;
            antp.ParameterType = "Scalar";
            antp.ParameterValue = DataTypeHelper.GetParameterValue(row.Datatype);
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
            ac.Id = _WINCC;
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

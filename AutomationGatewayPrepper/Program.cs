using AutomationGatewayPrepper.Entities.Input;
using AutomationGatewayPrepper.Entities.Output;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AutomationGatewayPrepper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AutomationGatewayPrepper Starting...");            

            var automationChannels = PopulateAutomationChannels();
            var automationNodeTypes = PopulateAutomationNodeTypes();
            var automationNodeTypeParameters = new List<AutomationNodeTypeParameter>();
            var automationNodeInstances = new List<AutomationNodeInstance>();
            var automationNodeInstanceParameters = new List<AutomationNodeInstanceParameter>();

            using var reader = new StreamReader(@"C:\Users\Javier\Desktop\Export.csv");
            using var csv = new CsvReader(reader);
            ConfigureCsvReader(csv);
            //Careful when using link on records variable as not all excel rows are loaded into memory unless .ToList() or similar methods are called.
            var records = csv.GetRecords<Configuration>();
            foreach (var row in records)
            {

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

            #region DemoOutput
            /*
             * E.g.
             * DHW_CNC_Machine_FIC_Command_Header_NodeId	Set the Node Id the equipment is in	Signed 16-bit value	2	ShortToSignedWord	AMFMasterPLCMES	DHW_FIC_Command	0001:TS:7:8A0E0007.2495091F.A.4C	0									0	0	0	0	0	0	0	FIC_Command.Header.NodeId	DHW_CNC_Machine	0	0	0	0	1 min
             * 
             * "#AutomationChannels"
             * "Id","Name","ConnectionFamilyType","ConnectionType","IdentityTokenType","Address","SecurityPolicy","MessageSecurityMode","ReconnectTime","TimeStampMode","UserName","AccessPoint","Rack","Slot","ChangeDrivenRead"
             * "AutomationChannelId_991f3bd2787f43d2b0154f5cf75e1234","s7","Undefined","S7","Anonymous","MyS7Id_8e0fc398931147c2b6e2076a430e4291","None","None","300","","","MyS7AccPoint","1","2","True"
             * "ChannelId_085cd9f34f1544998d931bc16d2ccbc2","Name ofChannelId_085cd9f34f1544998d931bc16d2ccbc2tyty","Undefined","OPCUA","Anonymous","opc.tcp://MyServerId_5cc640d68bd24a178003998a0b223ea2","None","None","","","","","","",""
             * 
             * "#AutomationNodeTypes"
             * "Id","Name","Description"
             * "ANTId_d2542f10cdf040a18be9301c618c9079","Name ofANTId_d2542f10cdf040a18be9301c618c9079","Description ofANTId_d2542f10cdf040a18be9301c618c9079"
             * 
             * "AutomationNodeTypeParameters"
             * "AutomationNodeTypeRef","Id","DataType","ParameterValue","MinValue","MaxValue","MaxLength","IsStatic","IsInternal","IsPersistent"
             * "ANTId_d2542f10cdf040a18be9301c618c9079","AA","Bool","False","","","","False","False","False"
             * 
             * "#AutomationNodeInstances"
             * "AutomationNodeTypeRef","Id","Name","Description"
             * "ANTId_d2542f10cdf040a18be9301c618c9079","asas","as","as"
             * "ANTId_d2542f10cdf040a18be9301c618c9079","OPCInst","",""
             * 
             * "#AutomationNodeInstanceParameters"
             * "AutomationNodeInstanceRef","Id","ParameterValue","ChannelNId","AddressType","Address","AcquisitionMode","AcquisitionCycleNId","SmoothingMode","DeltaValue","DeltaTime"
             * "asas","AA","False","AutomationChannelId_991f3bd2787f43d2b0154f5cf75e1234","NodeId","S7","OnChange","","None","",""
             * "OPCInst","AA","False","ChannelId_085cd9f34f1544998d931bc16d2ccbc2","NodeId","s=OPC","OnChange","","None","",""
             * 
             * */
            #endregion
        }

        private static List<AutomationNodeType> PopulateAutomationNodeTypes()
        {
            var automationNodeTypes = new List<AutomationNodeType>();
            var ant = new AutomationNodeType();
            ant.Id = "FIC_Datablock";
            ant.Name = "FIC_Datablock";
            ant.Description = "FIC_Datablock";
            automationNodeTypes.Add(ant);
            ant = new AutomationNodeType();
            ant.Id = "FIS_Datablock";
            ant.Name = "FIS_Datablock";
            ant.Description = "FIS_Datablock";
            automationNodeTypes.Add(ant);
            return automationNodeTypes;
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

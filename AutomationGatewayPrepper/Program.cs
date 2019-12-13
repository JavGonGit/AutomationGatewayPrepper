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

            using var reader = new StreamReader(@"C:\Users\Javier\Desktop\Export.csv");
            using var csv = new CsvReader(reader);

            //Remove spaces in header row
            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => Regex.Replace(header, @"\s", string.Empty);
            csv.Configuration.Delimiter = ",";

            //If any bad data is found (bad encoding/ character(s), etc...) write it out to console window
            csv.Configuration.BadDataFound = x =>
            {
                Console.WriteLine($"Bad Datarow: {x.RawRecord}");
            };

            //Careful when using link on records variable as not all excel rows are loaded into memory unless .ToList() or similar methods are called.
            var records = csv.GetRecords<Configuration>();

            foreach (var item in records)
            {
                Console.Write("hurray");
            }
            
            PopulateAutomationChannel();

            //Generate csv file to import into UADM
            var randomFileName = "AutoGate_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".csv";
            using (var writer = new StreamWriter(@"C:\Source\Work\AutomationGatewayPrepper\AutomationGatewayPrepper\Output\" + randomFileName))
            using (var csvWriter = new CsvWriter(writer))
            {
                csvWriter.WriteField("\"#AutomationChannels\"");
                csvWriter.NextRecord();
            }

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
        }

        private static List<AutomationChannel> PopulateAutomationChannel()
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

            return automationChannels;
        }
    }
}

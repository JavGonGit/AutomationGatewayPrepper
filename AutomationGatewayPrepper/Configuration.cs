using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationGatewayPrepper
{
    public class Configuration
    {
        public string AutomationNodeTypeRef { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string DataType { get; set; }
        public string ParameterValue { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string MaxLength { get; set; }
        public string IsStatic { get; set; }
        public string IsInternal { get; set; }
        public string IsPersistent { get; set; }
        //"AutomationNodeTypeRef","Id","DataType","","","","","","",""
        //Name	Data type	Connection	Group	Address	"As Tag Name" , Namespace

    }
}

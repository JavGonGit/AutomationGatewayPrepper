using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationGatewayPrepper.Entities.Output
{
    class AutomationNodeInstanceParameter
    {
        public string AutomationNodeInstanceRef { get; set; }
        public string Id { get; set; }
        public string ParameterValue { get; set; }
        public string ParameterValues { get; set; }
        public string ChannelNId { get; set; }
        public string AddressType { get; set; }
        public string Address { get; set; }
        public string AcquisitionMode { get; set; }
        public string AcquisitionCycleNId { get; set; }
        public string SmoothingMode { get; set; }
        public string DeltaValue { get; set; }
        public string DeltaTime { get; set; }

    }
}

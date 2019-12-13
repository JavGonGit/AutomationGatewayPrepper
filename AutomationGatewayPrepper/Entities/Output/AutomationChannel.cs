using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AutomationGatewayPrepper.Entities.Output
{
    public class AutomationChannel
    {
        //public AutomationChannel()
        //{
        //    PropertyInfo[] properties = this.GetType().GetProperties();

        //    foreach (var property in properties)
        //    {
        //        if (property.PropertyType == typeof(string) && property.GetValue(this) == null)
        //        {
        //            property.SetValue(this, String.Empty);
        //        }
        //    }
        //}

        public string Id { get; set; }
        public string Name { get; set; }
        public string ConnectionFamilyType { get; set; }
        public string ConnectionType { get; set; }
        public string IdentityTokenType { get; set; }
        public string Address { get; set; }
        public string SecurityPolicy { get; set; }
        public string MessageSecurityMode { get; set; }
        public string ReconnectTime { get; set; }
        public string TimeStampMode { get; set; }
        public string UserName { get; set; }
        public string AccessPoint { get; set; }
        public string Rack { get; set; }
        public string Slot { get; set; }
        public string ChangeDrivenRead { get; set; }

    }
}

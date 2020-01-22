using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomationGatewayPrepper.Helpers
{
    public static class DataTypeHelper
    {
        #region ClassInfo
        /* Automation Gateway Server Datatypes
         * URL: https://support.industrysoftware.automation.siemens.com/docs/mes/uaf/manuals/2.5/SIMATICITUA_UserManual.pdf
         * Page: 72
         * 
         * AutomationType   OPCType     C#Equivalent
         * Bool             Bool        Bool
         * Int              Int16       short (int16)
         * DInt             Int32       int (int32)
         * LInt             Int64       int (int64)
         * DateTime         DateTime    DateTimeOffSet
         * String           String      String
         * Byte             Byte        byte
         * SInt             Sbyte       sbyte
         * UInt             UInt16      Ushort (uint16)
         * UDInt            UInt32      Uint (uint32)
         * ULInt            UInt64      Ulong (uint64)
         * Real             Float       float
         * LReal            Double      double
         * 
         * (1) All DateTime values are written on the Automation Gateway Server in UTC format. There are twotypes of DateTime values, which either represent the Timestamp of the Automation Parameter value orthe DateTime of the Automation Parameter value
         *      itself (if defined as a DateTime data type) The Timestamp is always expressed in UTC format and cannot be changed by the user as itrepresents the time when the value was written on the Automation Gateway Server. 
         *      The DateTime ofthe Automation Parameter value is also written in UTC format, even if the method that was used towrite it (AutomationWrite) expressed the DateTime in a local time format.
         * (2) If the parameter value is set to a value of decimal type, you must perform an explicit cast to avoid a “type mismatch exception” when theAutomationWrite() method is called at runtime.
         */
        #endregion

        private const string SIGNED16 = "Signed 16-bit value";
        private const string SIGNED32 = "Signed 32-bit value";
        private const string BINARY_TAG = "Binary Tag";
        private const string FLOAT = "Floating-point number 32-bit IEEE 754";
        private const string TEXT8BIT = "Text tag 8-bit character set";
        private const string UNSIGNED8BIT = "Unsigned 8-bit value";
        private const string UNSIGNED16BIT = "Unsigned 16-bit value";
        private const string UNSIGNED32BIT = "Unsigned 32-bit value";

        private const string _tagParseExpression = @"^ *(?<PlcId>[A-Z]{3})_(?<Scrap>.*)_(?<DatablockType>FIC|FIS|UR|GLOBAL)_(?<TagName>[^ ]+)";
        private static Regex TagParse = new Regex(_tagParseExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static string GetDataType(String datatype)
        {
            string result;
            switch (datatype)
            {
                case SIGNED16:
                    result = "Int";
                    break;
                case SIGNED32:
                    result = "DInt";
                    break;
                case BINARY_TAG:
                    result = "Bool";
                    break;
                case FLOAT:
                    result = "Real";
                    break;
                case TEXT8BIT:
                    result = "String";
                    break;
                case UNSIGNED8BIT:
                    result = "Datetime";
                    break;
                case UNSIGNED16BIT:
                    result = "UInt";
                    break;
                case UNSIGNED32BIT:
                    result = "UDInt";
                    break;
                default:
                    throw new ArgumentException($"Unexpected datatype encountered: {datatype}. Please add this value to DataTypeHelper to resolve.");
            }
            return result;
        }

        public static string GetParameterValue(String datatype)
        {
            string result;
            const string zero = "0";
            switch (datatype)
            {
                case SIGNED16:
                    result = zero;
                    break;
                case SIGNED32:
                    result = zero;
                    break;
                case BINARY_TAG:
                    result = bool.FalseString;
                    break;
                case FLOAT:
                    result = zero;
                    break;
                case TEXT8BIT:
                    result = String.Empty;
                    break;
                case UNSIGNED8BIT:
                    result = String.Empty;
                    break;
                case UNSIGNED16BIT:
                    result = zero;
                    break;
                case UNSIGNED32BIT:
                    result = zero;
                    break;
                default:
                    throw new ArgumentException($"Unexpected datatype encountered: {datatype}. Please add this value to DataTypeHelper to resolve.");
            }
            return result;
        }

        public static string GetParameterName(string id)
        {
            var parse = TagParse.Match(TranslateParameterRecords(id));
            if (!parse.Success)
                throw new Exception(string.Format("Failed to parse tag '{0}'.", id));

            var plcId = parse.Groups["PlcId"].Value;
            var dbType = parse.Groups["DatablockType"].Value;
            var tagName = parse.Groups["TagName"].Value;

            return string.Format("{0}_{1}_{2}", plcId, dbType, tagName);
        }

        public static string TranslateParameterRecords(string id)
        {
            var builder = new StringBuilder(id);

            builder.Replace("FIC_Command", "FIC");
            builder.Replace("FIS_Status", "FIS");
            builder.Replace("Global_Permissives", "GLOBAL");
            builder.Replace("User_Read", "UR");

            builder.Replace("ID", "Id");

            return builder.ToString();
        }
    }
}

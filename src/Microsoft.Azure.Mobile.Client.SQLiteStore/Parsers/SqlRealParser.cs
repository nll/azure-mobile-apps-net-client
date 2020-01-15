using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Parsers
{
    internal class SqlRealParser : SqlParser
    {
        private readonly JTokenType columnType;

        internal SqlRealParser(JTokenType columnType)
        {
            this.columnType = columnType;
        }

        public override JToken DeserializeValue(object value)
        {
            return SqlHelpers.ParseReal(columnType, value);
        }
    }
}
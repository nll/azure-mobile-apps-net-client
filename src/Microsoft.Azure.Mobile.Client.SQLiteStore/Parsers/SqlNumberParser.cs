using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Parsers
{
    internal class SqlNumberParser : SqlParser
    {
        private readonly JTokenType columnType;

        internal SqlNumberParser(JTokenType columnType)
        {
            this.columnType = columnType;
        }

        public override JToken DeserializeValue(object value)
        {
            return SqlHelpers.ParseNumber(columnType, value);
        }
    }
}
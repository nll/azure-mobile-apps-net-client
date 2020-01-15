using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Parsers
{
    internal class SqlTextParser : SqlParser
    {
        private readonly JTokenType columnType;

        internal SqlTextParser(JTokenType columnType)
        {
            this.columnType = columnType;
        }

        public override JToken DeserializeValue(object value)
        {
            return SqlHelpers.ParseText(columnType, value);
        }
    }
}
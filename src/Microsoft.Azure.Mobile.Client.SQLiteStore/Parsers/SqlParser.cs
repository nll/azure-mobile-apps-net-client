using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Parsers
{
    internal class SqlParser
    {
        public static SqlParser Create(string storeType, JTokenType columnType)
        {
            if (SqlHelpers.IsTextType(storeType))
            {
                return new SqlTextParser(columnType);
            }
            
            if (SqlHelpers.IsRealType(storeType))
            {
                return new SqlRealParser(columnType);
            }
            
            if (SqlHelpers.IsNumberType(storeType))
            {
                return new SqlNumberParser(columnType);
            }

            return new SqlParser();
        }

        public virtual JToken DeserializeValue(object value)
        {
            return null;
        }
    }
}
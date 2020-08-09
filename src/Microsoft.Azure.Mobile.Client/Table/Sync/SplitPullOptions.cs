using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Table.Sync
{
    public class SplitPullOptions
    {
        private static readonly SplitPullOptions Empty = new SplitPullOptions(string.Empty, string.Empty, Enumerable.Empty<int>());

        public SplitPullOptions(string fieldName, string queryFieldPrefix, IEnumerable<int> fieldValues)
        {
            FieldName = fieldName;
            QueryFieldPrefix = queryFieldPrefix;
            FieldValues = new List<int>(fieldValues);
        }

        private const string FieldNameKey = "X-SPLIT-FIELD-NAME";
        private const string QueryFieldPrefixKey = "X-SPLIT-QUERY-FIELD-PREFIX";
        private const string FieldValuesKey = "X-SPLIT-FIELD-VALUES";

        public string FieldName { get; }

        public string QueryFieldPrefix { get; }

        public IList<int> FieldValues { get; }

        public bool HasValues => !string.IsNullOrEmpty(FieldName) && 
                                 !string.IsNullOrEmpty(QueryFieldPrefix) &&
                                 FieldValues.Any();

        public IDictionary<string, string> AddToParameters(IDictionary<string, string> parameters)
        {

            if (!HasValues)
            {
                throw new InvalidOperationException();
            }
            
            if (parameters != null)
            {
                parameters = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            parameters[FieldNameKey] = FieldName;
            parameters[QueryFieldPrefixKey] = QueryFieldPrefix;
            parameters[FieldValuesKey] = string.Join(",", FieldValues);

            return parameters;
        }

        internal static SplitPullOptions FromParameters(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return Empty;
            }

            var hasFieldName = parameters.TryGetValue(FieldNameKey, out var fieldName);
            var hasQueryFieldPrefix = parameters.TryGetValue(QueryFieldPrefixKey, out var queryFieldPrefix);
            var hasFieldValues  = parameters.TryGetValue(FieldValuesKey, out var fieldValuesString);
            if (hasFieldName && hasQueryFieldPrefix && hasFieldValues)
            {
                var fieldValues = fieldValuesString.Split(',')
                    .Select(it=>it.Trim())
                    .Where(it => !string.IsNullOrEmpty(it))
                    .Select(int.Parse); 
                return new SplitPullOptions(fieldName, queryFieldPrefix, fieldValues);
            }

            return Empty;
        }

        internal static void ClearParameters(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            parameters.Remove(FieldNameKey);
            parameters.Remove(QueryFieldPrefixKey);
            parameters.Remove(FieldValuesKey);
        }
    }
}
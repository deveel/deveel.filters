using System.Text.Json;

namespace Deveel.Filters {
    public static class JsonElementUtil {
        public static JsonElement ToElement(object? value) {
            string valueString;
            if (value == null) {
                valueString = "null";
            } else if (value is string s) { 
                valueString = $"\"{s}\"";
            } else if (value is bool b) {
                valueString = b ? "true" : "false";
            } else {
                valueString = Convert.ToString(value);
            }

            return JsonDocument.Parse(valueString).RootElement;
        }

        public static BinaryFilter BuildFilter(IDictionary<string, JsonElement>? jsonData, FilterType filterType, FilterType defaultLogical = FilterType.And) {
            if (defaultLogical != FilterType.And &&
                defaultLogical != FilterType.Or)
                throw new ArgumentException($"The type '{defaultLogical}' is not a logical filter type", nameof(defaultLogical));

            if (filterType != FilterType.Equal &&
                filterType != FilterType.NotEqual &&
                filterType != FilterType.GreaterThan &&
                filterType != FilterType.GreaterThanOrEqual &&
                filterType != FilterType.LessThan &&
                filterType != FilterType.LessThanOrEqual)
                throw new ArgumentException($"The filter type '{filterType}' is not a binary filter type", nameof(filterType));

            if (jsonData == null)
                throw new ArgumentNullException(nameof(jsonData));

            BinaryFilter? result = null;

            foreach (var item in jsonData) {
                var value = InferValue(item.Value);
                var filter = Filter.Binary(Filter.Variable(item.Key), Filter.Constant(value), filterType);

                if (result == null) {
                    result = filter;
                } else {
                    result = Filter.Binary(result, filter, defaultLogical);
                }
            }

            if (result == null)
                throw new FilterException("It was not possible to build a binary filter from the given data");

            return result;
        }

        public static object? InferValue(JsonElement? json) {
            if (json == null)
                return null;

            switch (json.Value.ValueKind) {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.String: {
                        if (json.Value.TryGetDateTimeOffset(out var dateTimeOffset))
                            return dateTimeOffset;
                        if (json.Value.TryGetDateTime(out var dateTime))
                            return dateTime;

                        return json.Value.GetString();
                    }
                case JsonValueKind.Number:
                    if (json.Value.TryGetInt32(out var i))
                        return i;
                    if (json.Value.TryGetInt64(out var l))
                        return l;
                    if (json.Value.TryGetDouble(out var d))
                        return d;
                    if (json.Value.TryGetSingle(out var f))
                        return f;

                    throw new FilterException("The number is not supported");
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    throw new FilterException("Complex data are not supported for this filter type");
            }
        }
    }
}

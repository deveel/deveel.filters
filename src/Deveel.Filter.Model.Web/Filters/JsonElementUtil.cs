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

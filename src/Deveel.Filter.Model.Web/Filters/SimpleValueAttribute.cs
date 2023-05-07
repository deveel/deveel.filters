using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Deveel.Filters {
	public sealed class SimpleValueAttribute : ValidationAttribute {
		public override bool IsValid(object? value) {
			if (value == null)
				return true;
			
			if (value is string)
				return true;
			if (value is DateTime ||
				value is DateTimeOffset ||
				value is TimeSpan)
				return true;
			if (value is bool)
				return true;

			if (value is byte ||
				value is int ||
				value is short ||
				value is long ||
				value is sbyte ||
				value is uint ||
				value is ushort ||
				value is ulong ||
				value is float ||
				value is double ||
				value is decimal)
				return true;

			if (value is JsonElement json) {
				if (json.ValueKind == JsonValueKind.Array ||
					json.ValueKind == JsonValueKind.Object)
					return false;
			}

			return false;
		}
	}
}

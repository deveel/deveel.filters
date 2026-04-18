// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Deveel.Filters {
	/// <summary>
	/// A validation attribute that ensures the annotated value is a simple (scalar) type,
	/// such as a string, number, boolean, date, or a dictionary of simple values.
	/// Complex objects and arrays are not considered valid.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class SimpleValueAttribute : ValidationAttribute {
		/// <inheritdoc/>
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

				return true;
			}

			if (value is IDictionary<string, object> dictionary) {
				foreach (var kvp in dictionary) {
                    if (!IsValid(kvp.Value))
                        return false;
                }
                return true;
			}

			if (value is IDictionary<string, JsonElement> jsonDictionary) {
                foreach (var kvp in jsonDictionary) {
                    if (!IsValid(kvp.Value))
                        return false;
                }
                return true;
            }

			return false;
		}
	}
}

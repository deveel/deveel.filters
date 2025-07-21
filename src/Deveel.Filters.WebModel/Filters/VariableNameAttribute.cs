using System.ComponentModel.DataAnnotations;

namespace Deveel.Filters {
    /// <summary>
    /// An attribute that can be used to validate a string
    /// is a valid variable name for a filter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class VariableNameAttribute : ValidationAttribute {
		/// <inheritdoc/>
		public override bool IsValid(object? value) {
			if (value == null || !(value is string s))
				return false;

			return Filter.IsValidReference(s);
		}
	}
}

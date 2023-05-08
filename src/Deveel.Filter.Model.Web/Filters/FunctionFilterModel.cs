using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
	/// <summary>
	/// Describes a function that is returning a boolean value
	/// and is used to filter a set of data.
	/// </summary>
	public sealed class FunctionFilterModel {
		/// <summary>
		/// The reference to the variable that is the
		/// instance object defining the function.
		/// </summary>
		[Required, VariableName, JsonPropertyName("varRef")]
		public string Instance { get; set; }

		/// <summary>
		/// The name of the function to invoke.
		/// </summary>
		[Required, JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// An optional array of arguments to pass to the function.
		/// </summary>
		[JsonPropertyName("args")]
		public FilterModel[]? Arguments { get; set; }

		internal Filter BuildFilter() {
			var variable = Filter.Variable(Instance);
			var args = Arguments?.Select(x => x.BuildFilter()).ToArray() ?? Array.Empty<Filter>();
			return Filter.Function(variable, Name, args);
		}
	}
}

namespace Deveel.Filters {
	/// <summary>
	/// Enumerates the types of filters that are known
	/// </summary>
	public enum FilterType {
		/// <summary>
		/// A filter that compares two parties for equality
		/// </summary>
		Equals = 1,

		/// <summary>
		/// A filter that compares two parties for inequality
		/// </summary>
		NotEquals,

		/// <summary>
		/// Compares two parties to determine if the first part is greater 
		/// than the second one.
		/// </summary>
		GreaterThan,

		/// <summary>
		/// Compares two parties to determine if the first part is greater
		/// or equal to the second one.
		/// </summary>
		GreaterThanOrEqual,

		/// <summary>
		/// Compares two parties to determine if the first part is less than
		/// the second one.
		/// </summary>
		LessThan,

		/// <summary>
		/// Compares two parties to determine if the first part is less than
		/// or equal to the second one.
		/// </summary>
		LessThanOrEqual,

		/// <summary>
		/// A logical AND between two filters
		/// </summary>
		And,

		/// <summary>
		/// A logical OR between two filters
		/// </summary>
		Or,

		/// <summary>
		/// An unary filter that negates the result of another filter
		/// </summary>
		Not,

		/// <summary>
		/// The invocation of a function that returns a boolean value
		/// </summary>
		Function,

		/// <summary>
		/// A constant and immutable value.
		/// </summary>
		Constant,

		/// <summary>
		/// The reference to a variable in the filter context.
		/// </summary>
		Variable
	}
}

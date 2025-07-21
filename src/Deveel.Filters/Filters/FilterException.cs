using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	/// <summary>
	/// An exception thrown when a filter is not valid.
	/// </summary>
	public class FilterException : Exception {
		/// <summary>
		/// Constructs the exception.
		/// </summary>
		public FilterException() {
		}

		/// <summary>
		/// Constructs the exception with the given message.
		/// </summary>
		/// <param name="message">The message of the exception.</param>
		public FilterException(string message) : base(message) {
		}

		/// <summary>
		/// Constructs the exception with the given message and inner exception.
		/// </summary>
		/// <param name="message">The message of the exception.</param>
		/// <param name="innerException">The inner exception of this exception.</param>
		public FilterException(string message, Exception innerException) : base(message, innerException) {
		}
	}
}
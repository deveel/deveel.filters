using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	/// <summary>
	/// Defines the contract for a filter that can be applied to a
	/// set to select a subset of.
	/// </summary>
	public interface IFilter {
		/// <summary>
		/// Gets the type of the filter.
		/// </summary>
		FilterType FilterType { get; }
	}
}

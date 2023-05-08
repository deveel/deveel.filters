using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public interface IConstantFilter : IFilter {
		object? Value { get; }
	}
}

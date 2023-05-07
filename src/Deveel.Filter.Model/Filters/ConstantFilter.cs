using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public sealed class ConstantFilter : Filter {
		public ConstantFilter(object? value) {
			Value = value;
		}

		public object? Value { get; }

		public override FilterType FilterType => FilterType.Constant;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public static class FilterExtensions {
		public static FilterModel ToFilterModel(this IFilter filter) {
			var converter = new FilterModelConverter();
			return (FilterModel) converter.Visit(filter);
		}
	}
}

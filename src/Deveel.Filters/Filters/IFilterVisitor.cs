using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public interface IFilterVisitor {
		IFilter Visit(IFilter filter);

		IFilter VisitFunction(IFunctionFilter filter);

		IFilter VisitConstant(IConstantFilter constant);

		IFilter VisitVariable(IVariableFilter variable);

		IFilter VisitUnary(IUnaryFilter filter);

		IFilter VisitBinary(IBinaryFilter filter);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public interface IFilterVisitor {
		Filter Visit(Filter filter);

		Filter VisitFunction(FunctionFilter filter);

		Filter VisitConstant(ConstantFilter constant);

		Filter VisitVariable(VariableFilter variable);

		Filter VisitUnary(UnaryFilter filter);

		Filter VisitBinary(BinaryFilter filter);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	class FilterModelConverter : FilterVisitor {
		public override IFilter VisitBinary(IBinaryFilter filter) {
			var left = (FilterModel) Visit(filter.Left);
			var right = (FilterModel) Visit(filter.Right);

			switch (filter.FilterType) {
				case FilterType.Equals:
					return new FilterModel {
						Equals = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.NotEquals:
					return new FilterModel {
						NotEquals = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.GreaterThan:
					return new FilterModel {
						GreaterThan = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.GreaterThanOrEqual:
					return new FilterModel {
						GreaterThanOrEqual = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.LessThan:
					return new FilterModel {
						LessThan = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.LessThanOrEqual:
					return new FilterModel {
						LessThanOrEqual = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.And:
					return new FilterModel {
						And = new BinaryFilterModel { Left = left, Right = right },
					};
				case FilterType.Or:
					return new FilterModel {
						Or = new BinaryFilterModel { Left = left, Right = right },
					};
				default:
					throw new FilterException($"The filter type {filter.FilterType} is not binary");
			}
		}

		public override IFilter VisitConstant(IConstantFilter filter) {
			return new FilterModel {
				Value = filter.Value
			};
		}

		public override IFilter VisitVariable(IVariableFilter filter) {
			return new FilterModel {
				Ref = filter.VariableName
			};
		}

		public override IFilter VisitUnary(IUnaryFilter filter) {
			if (filter.FilterType != FilterType.Not)
				throw new FilterException($"The filter type {filter.FilterType} is not unary");

			var operand = (FilterModel)Visit(filter.Operand);
			return new FilterModel {
				Not = operand
			};
		}

		public override IList<IFilter> VisitFunctionArguments(IList<IFilter>? arguments) {
			if (arguments == null)
				return new FilterModel[0];

			var list = new List<IFilter>(arguments.Count);
			foreach (var argument in arguments) {
				list.Add(Visit(argument));
			}
			return list;
		}

		public override IFilter VisitFunction(IFunctionFilter filter) {
			var arguments = VisitFunctionArguments(filter.Arguments);
			var args = new FilterModel[arguments.Count];
			for (var i = 0; i < arguments.Count; i++) {
				args[i] = (FilterModel)arguments[i];
			}
			return new FilterModel {
				Function = new FunctionFilterModel {
					Instance = filter.Variable.VariableName,
					Name = filter.FunctionName,
					Arguments = args
				}
			};
		}
	}
}

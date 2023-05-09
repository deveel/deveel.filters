using System.Text.Json;

namespace Deveel.Filters {
    class FilterModelConverter : FilterVisitor {
		private readonly FilterBuilderOptions builderOptions;

        public FilterModelConverter(FilterBuilderOptions binaryOptions) {
            this.builderOptions = binaryOptions;
        }

		private BinaryFilterModel MakeBinary(FilterModel left, FilterModel right, bool logicalAnd = false) {
			if (builderOptions.PreferBinaryData &&
				((IFilter)left).FilterType == FilterType.Variable &&
				!String.IsNullOrWhiteSpace(left.Ref) &&
				((IFilter)right).FilterType == FilterType.Constant) {
				var variable = left.Ref;
				var constant = JsonElementUtil.ToElement(right.Value);

				return new BinaryFilterModel {
					BinaryData = new Dictionary<string, JsonElement> {
						{ variable, constant }
					}
				};
			}

			return new BinaryFilterModel {
                Left = left,
                Right = right
            };
		}

        public override IFilter VisitBinary(IBinaryFilter filter) {
			var left = (FilterModel) Visit(filter.Left);
			var right = (FilterModel) Visit(filter.Right);

			switch (filter.FilterType) {
				case FilterType.Equal:
					return new FilterModel {
						Equal = MakeBinary(left, right)
					};
				case FilterType.NotEqual:
					return new FilterModel {
						NotEqual = MakeBinary(left, right),
					};
				case FilterType.GreaterThan:
					return new FilterModel {
						GreaterThan = MakeBinary(left, right)
					};
				case FilterType.GreaterThanOrEqual:
					return new FilterModel {
						GreaterThanOrEqual = MakeBinary(left, right),
					};
				case FilterType.LessThan:
					return new FilterModel {
						LessThan = MakeBinary(left, right),
					};
				case FilterType.LessThanOrEqual:
					return new FilterModel {
						LessThanOrEqual = MakeBinary(left, right),
					};
				case FilterType.And:
					return new FilterModel {
						And = MakeBinary(left, right, true),
					};
				case FilterType.Or:
					return new FilterModel {
						Or = MakeBinary(left, right),
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

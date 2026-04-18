using System.Text.Json;

namespace Deveel.Filters {
    class FilterModelConverter : FilterVisitor {
		private readonly FilterBuilderOptions builderOptions;

        public FilterModelConverter(FilterBuilderOptions binaryOptions) {
            this.builderOptions = binaryOptions;
        }
        
        public FilterModel? WebModel { get; private set; }

        private FilterModel? VisitModel(Filter filter)
        {
	        var visitor = new FilterModelConverter(builderOptions);
	        visitor.Visit(filter);
	        return visitor.WebModel;
        }

		private BinaryFilterModel MakeBinary(FilterModel left, FilterModel right, bool logicalAnd = false) {
			if (builderOptions.PreferBinaryData &&
				left.GetFilterType() == FilterType.Variable &&
				!String.IsNullOrWhiteSpace(left.Ref) &&
				right.GetFilterType() == FilterType.Constant) {
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

        public override Filter VisitBinary(BinaryFilter filter) {
			var left = VisitModel(filter.Left);
			var right = VisitModel(filter.Right);

			switch (filter.FilterType) {
				case FilterType.Equal:
					WebModel = new FilterModel {
						Equal = MakeBinary(left, right)
					};
					break;
				case FilterType.NotEqual:
					WebModel = new FilterModel {
						NotEqual = MakeBinary(left, right),
					};
					break;
				case FilterType.GreaterThan:
					WebModel = new FilterModel {
						GreaterThan = MakeBinary(left, right)
					};
					break;
				case FilterType.GreaterThanOrEqual:
					WebModel = new FilterModel {
						GreaterThanOrEqual = MakeBinary(left, right),
					};
					break;
				case FilterType.LessThan:
					WebModel = new FilterModel {
						LessThan = MakeBinary(left, right),
					};
					break;
				case FilterType.LessThanOrEqual:
					WebModel = new FilterModel {
						LessThanOrEqual = MakeBinary(left, right),
					};
					break;
				case FilterType.And:
					WebModel = new FilterModel {
						And = MakeBinary(left, right, true),
					};
					break;
				case FilterType.Or:
					WebModel = new FilterModel {
						Or = MakeBinary(left, right),
					};
					break;
				default:
					throw new FilterException($"The filter type {filter.FilterType} is not binary");
			}
			
			return filter;
		}

		public override Filter VisitConstant(ConstantFilter filter) {
			WebModel = new FilterModel {
				Value = filter.Value
			};

			return base.VisitConstant(filter);
		}

		public override Filter VisitVariable(VariableFilter filter) {
			WebModel = new FilterModel {
				Ref = filter.VariableName
			};
			
			return filter;
		}

		public override Filter VisitUnary(UnaryFilter filter) {
			if (filter.FilterType != FilterType.Not)
				throw new FilterException($"The filter type {filter.FilterType} is not unary");

			var operand = VisitModel(filter.Operand);
			WebModel = new FilterModel {
				Not = operand
			};
			
			return filter;
		}
		
		public override Filter VisitFunction(FunctionFilter filter) {
			var arguments = VisitFunctionArguments(filter.Arguments);
			var args = new FilterModel[arguments.Count];
			for (var i = 0; i < arguments.Count; i++) {
				args[i] = VisitModel(arguments[i]);
			}
			WebModel = new FilterModel {
				Function = new FunctionFilterModel {
					Instance = filter.Variable.VariableName,
					Name = filter.FunctionName,
					Arguments = args
				}
			};
			
			return filter;
		}
	}
}

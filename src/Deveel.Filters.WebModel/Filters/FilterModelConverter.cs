// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Deveel.Filters {
    class FilterModelConverter : FilterExpressionVisitor {
		private readonly FilterBuilderOptions builderOptions;

        public FilterModelConverter(FilterBuilderOptions binaryOptions) {
            this.builderOptions = binaryOptions;
        }
        
        public FilterModel? WebModel { get; private set; }

        private FilterModel? VisitModel(FilterExpression filter)
        {
	        var visitor = new FilterModelConverter(builderOptions);
	        visitor.Visit(filter);
	        return visitor.WebModel;
        }

		private BinaryFilterModel MakeBinary(FilterModel left, FilterModel right, bool logicalAnd = false) {
			if (builderOptions.PreferBinaryData &&
				left.GetFilterType() == FilterExpressionType.Variable &&
				!String.IsNullOrWhiteSpace(left.Ref) &&
				right.GetFilterType() == FilterExpressionType.Constant) {
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

        public override FilterExpression VisitBinary(BinaryFilterExpression filterExpression) {
			var left = VisitModel(filterExpression.Left);
			var right = VisitModel(filterExpression.Right);

			switch (filterExpression.ExpressionType) {
				case FilterExpressionType.Equal:
					WebModel = new FilterModel {
						Equal = MakeBinary(left, right)
					};
					break;
				case FilterExpressionType.NotEqual:
					WebModel = new FilterModel {
						NotEqual = MakeBinary(left, right),
					};
					break;
				case FilterExpressionType.GreaterThan:
					WebModel = new FilterModel {
						GreaterThan = MakeBinary(left, right)
					};
					break;
				case FilterExpressionType.GreaterThanOrEqual:
					WebModel = new FilterModel {
						GreaterThanOrEqual = MakeBinary(left, right),
					};
					break;
				case FilterExpressionType.LessThan:
					WebModel = new FilterModel {
						LessThan = MakeBinary(left, right),
					};
					break;
				case FilterExpressionType.LessThanOrEqual:
					WebModel = new FilterModel {
						LessThanOrEqual = MakeBinary(left, right),
					};
					break;
				case FilterExpressionType.And:
					WebModel = new FilterModel {
						And = MakeBinary(left, right, true),
					};
					break;
				case FilterExpressionType.Or:
					WebModel = new FilterModel {
						Or = MakeBinary(left, right),
					};
					break;
				default:
					throw new FilterException($"The filter type {filterExpression.ExpressionType} is not binary");
			}
			
			return filterExpression;
		}

		public override FilterExpression VisitConstant(ConstantFilterExpression filterExpression) {
			WebModel = new FilterModel {
				Value = filterExpression.Value
			};

			return base.VisitConstant(filterExpression);
		}

		public override FilterExpression VisitVariable(VariableFilterExpression filterExpression) {
			WebModel = new FilterModel {
				Ref = filterExpression.VariableName
			};
			
			return filterExpression;
		}

		public override FilterExpression VisitUnary(UnaryFilterExpression filterExpression) {
			if (filterExpression.ExpressionType != FilterExpressionType.Not)
				throw new FilterException($"The filter type {filterExpression.ExpressionType} is not unary");

			var operand = VisitModel(filterExpression.Operand);
			WebModel = new FilterModel {
				Not = operand
			};
			
			return filterExpression;
		}
		
		public override FilterExpression VisitFunction(FunctionFilterExpression filterExpression) {
			var arguments = VisitFunctionArguments(filterExpression.Arguments);
			var args = new FilterModel[arguments.Count];
			for (var i = 0; i < arguments.Count; i++) {
				args[i] = VisitModel(arguments[i]);
			}
			WebModel = new FilterModel {
				Function = new FunctionFilterModel {
					Instance = filterExpression.Variable.VariableName,
					Name = filterExpression.FunctionName,
					Arguments = args
				}
			};
			
			return filterExpression;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public static class FilterConverterTests {
		[Fact]
		public static void ConvertVariableToFilter() {
			var variable = new VariableFilterImpl("foo");

			var filter = Filter.Convert(variable);

			Assert.NotNull(filter);
			Assert.IsType<VariableFilter>(filter);
			Assert.Equal("foo", ((VariableFilter) filter).VariableName);
		}

		[Theory]
		[InlineData(123)]
		[InlineData("test")]
		[InlineData(true)]
		[InlineData(false)]
		[InlineData(123.456)]
		[InlineData(123.456f)]
		[InlineData(null)]
		public static void ConvertConstantToFilter(object value) {
			var constant = new ConstantFilterImpl(value);
			var filter = Filter.Convert(constant);
			Assert.NotNull(filter);
			Assert.IsType<ConstantFilter>(filter);
			Assert.Equal(value, ((ConstantFilter) filter).Value);
		}

		[Theory]
		[InlineData("x", 123, FilterType.Equals)]
		[InlineData("x", "test", FilterType.Equals)]
		[InlineData("x", 123, FilterType.NotEquals)]
		[InlineData("x", "test", FilterType.NotEquals)]
		[InlineData("x", 123, FilterType.GreaterThan)]
		[InlineData("x", 123, FilterType.GreaterThanOrEqual)]
		[InlineData("x", 123, FilterType.LessThan)]
		[InlineData("x", 123, FilterType.LessThanOrEqual)]
		public static void ConvertBinaryToFilter(string varName, object value, FilterType filterType) {
			var variable = new VariableFilterImpl(varName);
			var constant = new ConstantFilterImpl(value);
			var binary = new BinaryFilterImpl(variable, constant, filterType);

			var filter = Filter.Convert(binary);
			Assert.NotNull(filter);
			Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(filterType, filter.FilterType);
			Assert.Equal(varName, ((VariableFilter) ((BinaryFilter) filter).Left).VariableName);
			Assert.Equal(value, ((ConstantFilter) ((BinaryFilter) filter).Right).Value);
		}

		[Theory]
		[InlineData("x", 123, FilterType.Equals)]
		[InlineData("x", "test", FilterType.Equals)]
		[InlineData("x", 123, FilterType.NotEquals)]
		[InlineData("x", "test", FilterType.NotEquals)]
		[InlineData("x", 123, FilterType.GreaterThan)]
		[InlineData("x", 123, FilterType.GreaterThanOrEqual)]
		[InlineData("x", 123, FilterType.LessThan)]
		[InlineData("x", 123, FilterType.LessThanOrEqual)]
		public static void ConvertUnaryToFilter(string varName, object value, FilterType filterType) {
			var variable = new VariableFilterImpl(varName);
			var constant = new ConstantFilterImpl(value);
			var binary = new BinaryFilterImpl(variable, constant, filterType);
			var unary = new UnaryFilterImpl(binary, FilterType.Not);
			var filter = Filter.Convert(unary);
			Assert.NotNull(filter);
			Assert.IsType<UnaryFilter>(filter);
			Assert.Equal(FilterType.Not, filter.FilterType);

			var operand = Assert.IsType<BinaryFilter>(((UnaryFilter) filter).Operand);
			Assert.Equal(filterType, operand.FilterType);
			Assert.Equal(varName, ((VariableFilter) operand.Left).VariableName);
			Assert.Equal(value, ((ConstantFilter) operand.Right).Value);
		}

		[Theory]
		[InlineData("x", "Contains", 123)]
		[InlineData("x", "Contains", "test")]
		public static void ConvertFunctionToFilter(string varName, string functionName, object value) {
			var variable = new VariableFilterImpl(varName);
			var constant = new ConstantFilterImpl(value);
			var function = new FunctionFilterImpl(variable, functionName, new[] { constant });
			var filter = Filter.Convert(function);
			Assert.NotNull(filter);

			Assert.IsType<FunctionFilter>(filter);
			Assert.Equal(functionName, ((FunctionFilter) filter).FunctionName);
			Assert.Equal(varName, ((VariableFilter) ((FunctionFilter) filter).Variable).VariableName);
			Assert.Equal(value, ((ConstantFilter) ((FunctionFilter)filter).Arguments[0]).Value);
		}

		class VariableFilterImpl : IVariableFilter {
			public VariableFilterImpl(string variableName) {
				VariableName = variableName;
			}

			public string VariableName { get; }

			public FilterType FilterType => FilterType.Variable;
		}

		class ConstantFilterImpl : IConstantFilter {
			public ConstantFilterImpl(object value) {
				Value = value;
			}

			public object Value { get; }

			public FilterType FilterType => FilterType.Constant;
		}

		class FunctionFilterImpl : IFunctionFilter {
			public FunctionFilterImpl(IVariableFilter variable, string functionName, IList<IFilter> arguments) {
				Variable = variable;
				FunctionName = functionName;
				Arguments = arguments;
			}

			public IVariableFilter Variable { get; }

			public string FunctionName { get; }

			public IList<IFilter> Arguments { get; }

			public FilterType FilterType => FilterType.Function;
		}

		class UnaryFilterImpl : IUnaryFilter {
			public UnaryFilterImpl(IFilter operand, FilterType filterType) {
				Operand = operand;
				FilterType = filterType;
			}

			public IFilter Operand { get; }

			public FilterType FilterType { get; }
		}

		class BinaryFilterImpl : IBinaryFilter {
			public BinaryFilterImpl(IFilter left, IFilter right, FilterType filterType) {
				Left = left;
				Right = right;
				FilterType = filterType;
			}

			public IFilter Left { get; }

			public IFilter Right { get; }

			public FilterType FilterType { get; }
		}
	}
}

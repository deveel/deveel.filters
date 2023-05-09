using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	[Trait("Feature", "Visitor")]
	public static class VisitorTests {
		[Fact]
		public static void VisitVariable() {
			var variable = Filter.Variable("test");
			var visitor = new FilterVisitor();
			var result = visitor.Visit(variable);

			var resultVariable = Assert.IsType<VariableFilter>(result);
			Assert.Equal(variable.VariableName, resultVariable.VariableName);
		}

		[Fact]
		public static void VisitConstant() {
			var constant = Filter.Constant(123);
			var visitor = new FilterVisitor();
			var result = visitor.Visit(constant);
			var resultConstant = Assert.IsType<ConstantFilter>(result);
			Assert.Equal(constant.Value, resultConstant.Value);
		}

		[Fact]
		public static void VisitBinary() {
			var left = Filter.Variable("test");
			var right = Filter.Constant(123);
			var binary = Filter.Equal(left, right);
			var visitor = new FilterVisitor();
			var result = visitor.Visit(binary);
			var resultBinary = Assert.IsType<BinaryFilter>(result);
			Assert.Equal(binary.FilterType, resultBinary.FilterType);
			Assert.Equal(binary.Left.FilterType, resultBinary.Left.FilterType);
			Assert.Equal(binary.Right.FilterType, resultBinary.Right.FilterType);
		}

		[Fact]
		public static void VisitUnary() {
			var operand = Filter.Variable("test");
			var unary = Filter.Not(operand);
			var visitor = new FilterVisitor();
			var result = visitor.Visit(unary);
			var resultUnary = Assert.IsType<UnaryFilter>(result);
			Assert.Equal(unary.FilterType, resultUnary.FilterType);
			Assert.Equal(unary.Operand.FilterType, resultUnary.Operand.FilterType);
		}

		[Fact]
		public static void VisitFunction() {
			var function = Filter.Function(Filter.Variable("x"), "test", Filter.Constant(123));

			var visitor = new FilterVisitor();
			var result = visitor.Visit(function);
			var resultFunction = Assert.IsType<FunctionFilter>(result);
			Assert.Equal(function.FilterType, resultFunction.FilterType);
			Assert.NotNull(resultFunction.Arguments);
			Assert.Equal(function.Arguments.Length, resultFunction.Arguments.Length);
		}
	}
}

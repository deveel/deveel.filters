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
			var variable = FilterExpression.Variable("test");
			var visitor = new FilterExpressionVisitor();
			var result = visitor.Visit(variable);

			var resultVariable = Assert.IsType<VariableFilterExpression>(result);
			Assert.Equal(variable.VariableName, resultVariable.VariableName);
		}

		[Fact]
		public static void VisitConstant() {
			var constant = FilterExpression.Constant(123);
			var visitor = new FilterExpressionVisitor();
			var result = visitor.Visit(constant);
			var resultConstant = Assert.IsType<ConstantFilterExpression>(result);
			Assert.Equal(constant.Value, resultConstant.Value);
		}

		[Fact]
		public static void VisitBinary() {
			var left = FilterExpression.Variable("test");
			var right = FilterExpression.Constant(123);
			var binary = FilterExpression.Equal(left, right);
			var visitor = new FilterExpressionVisitor();
			var result = visitor.Visit(binary);
			var resultBinary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(binary.ExpressionType, resultBinary.ExpressionType);
			Assert.Equal(binary.Left.ExpressionType, resultBinary.Left.ExpressionType);
			Assert.Equal(binary.Right.ExpressionType, resultBinary.Right.ExpressionType);
		}

		[Fact]
		public static void VisitUnary() {
			var operand = FilterExpression.Variable("test");
			var unary = FilterExpression.Not(operand);
			var visitor = new FilterExpressionVisitor();
			var result = visitor.Visit(unary);
			var resultUnary = Assert.IsType<UnaryFilterExpression>(result);
			Assert.Equal(unary.ExpressionType, resultUnary.ExpressionType);
			Assert.Equal(unary.Operand.ExpressionType, resultUnary.Operand.ExpressionType);
		}

		[Fact]
		public static void VisitFunction() {
			var function = FilterExpression.Function(FilterExpression.Variable("x"), "test", new[] { FilterExpression.Constant(123) });

			var visitor = new FilterExpressionVisitor();
			var result = visitor.Visit(function);
			var resultFunction = Assert.IsType<FunctionFilterExpression>(result);
			Assert.Equal(function.ExpressionType, resultFunction.ExpressionType);
			Assert.NotNull(resultFunction.Arguments);
			Assert.Equal(function.Arguments.Length, resultFunction.Arguments.Length);
		}
	}
}

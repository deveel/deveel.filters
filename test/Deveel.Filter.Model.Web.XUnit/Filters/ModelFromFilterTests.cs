namespace Deveel.Filters {
	public static class ModelFromFilterTests {
		[Fact]
		public static void BuildEqualsModel() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equals);
			var model = binary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.Equals);
			Assert.NotNull(model.Equals.Left);
			Assert.NotNull(model.Equals.Right);
			Assert.NotNull(model.Equals.Left.Ref);
			Assert.Equal("x", model.Equals.Left.Ref);
			Assert.NotNull(model.Equals.Right.Value);
			Assert.Equal(22, model.Equals.Right.Value);
		}

		[Fact]
		public static void BuildNotEqualsModel() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.NotEquals);
			var model = binary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.NotEquals);
			Assert.NotNull(model.NotEquals.Left);
			Assert.NotNull(model.NotEquals.Right);
			Assert.NotNull(model.NotEquals.Left.Ref);
			Assert.Equal("x", model.NotEquals.Left.Ref);
			Assert.NotNull(model.NotEquals.Right.Value);
			Assert.Equal(22, model.NotEquals.Right.Value);
		}

		[Fact]
		public static void BuildGreaterThanModel() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.GreaterThan);
			var model = binary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.GreaterThan);
			Assert.NotNull(model.GreaterThan.Left);
			Assert.NotNull(model.GreaterThan.Right);
			Assert.NotNull(model.GreaterThan.Left.Ref);
			Assert.Equal("x", model.GreaterThan.Left.Ref);
			Assert.NotNull(model.GreaterThan.Right.Value);
			Assert.Equal(22, model.GreaterThan.Right.Value);
		}

		[Fact]
		public static void BuildGreaterThanOrEqualModel() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.GreaterThanOrEqual);
			var model = binary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.GreaterThanOrEqual);
			Assert.NotNull(model.GreaterThanOrEqual.Left);
			Assert.NotNull(model.GreaterThanOrEqual.Right);
			Assert.NotNull(model.GreaterThanOrEqual.Left.Ref);
			Assert.Equal("x", model.GreaterThanOrEqual.Left.Ref);
			Assert.NotNull(model.GreaterThanOrEqual.Right.Value);
			Assert.Equal(22, model.GreaterThanOrEqual.Right.Value);
		}

		[Fact]
		public static void BuildLessThanModel() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.LessThan);
			var model = binary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.LessThan);
			Assert.NotNull(model.LessThan.Left);
			Assert.NotNull(model.LessThan.Right);
			Assert.NotNull(model.LessThan.Left.Ref);
			Assert.Equal("x", model.LessThan.Left.Ref);
			Assert.NotNull(model.LessThan.Right.Value);
			Assert.Equal(22, model.LessThan.Right.Value);
		}

		[Fact]
		public static void BuildLessThanOrEqualModel() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.LessThanOrEqual);
			var model = binary.ToFilterModel();
			Assert.NotNull(model);

			Assert.NotNull(model.LessThanOrEqual);
			Assert.NotNull(model.LessThanOrEqual.Left);
			Assert.NotNull(model.LessThanOrEqual.Right);

			Assert.NotNull(model.LessThanOrEqual.Left.Ref);
			Assert.Equal("x", model.LessThanOrEqual.Left.Ref);

			Assert.NotNull(model.LessThanOrEqual.Right.Value);
			Assert.Equal(22, model.LessThanOrEqual.Right.Value);
		}

		[Fact]
		public static void BuildAndModel() {
			var binary = Filter.And(Filter.Equals(Filter.Variable("x"), Filter.Constant(22)), Filter.NotEquals(Filter.Variable("y"), Filter.Constant(45)));

			var model = binary.ToFilterModel();

			Assert.NotNull(model);
			Assert.NotNull(model.And);
			Assert.NotNull(model.And.Left);
			Assert.NotNull(model.And.Right);

			Assert.NotNull(model.And.Left.Equals);
			Assert.NotNull(model.And.Left.Equals.Left);
			Assert.NotNull(model.And.Left.Equals.Right);

			Assert.NotNull(model.And.Left.Equals.Left.Ref);
			Assert.Equal("x", model.And.Left.Equals.Left.Ref);

			Assert.NotNull(model.And.Left.Equals.Right.Value);
			Assert.Equal(22, model.And.Left.Equals.Right.Value);

			Assert.NotNull(model.And.Right.NotEquals);
			Assert.NotNull(model.And.Right.NotEquals.Left);
			Assert.NotNull(model.And.Right.NotEquals.Right);

			Assert.NotNull(model.And.Right.NotEquals.Left.Ref);
			Assert.Equal("y", model.And.Right.NotEquals.Left.Ref);

			Assert.NotNull(model.And.Right.NotEquals.Right.Value);
			Assert.Equal(45, model.And.Right.NotEquals.Right.Value);
		}

		[Fact]
		public static void BuildOrModel() {
			var binary = Filter.Or(Filter.Equals(Filter.Variable("x"), Filter.Constant(22)), Filter.NotEquals(Filter.Variable("y"), Filter.Constant(45)));
			var model = binary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.Or);
			Assert.NotNull(model.Or.Left);
			Assert.NotNull(model.Or.Right);
			Assert.NotNull(model.Or.Left.Equals);
			Assert.NotNull(model.Or.Left.Equals.Left);
			Assert.NotNull(model.Or.Left.Equals.Right);
			Assert.NotNull(model.Or.Left.Equals.Left.Ref);
			Assert.Equal("x", model.Or.Left.Equals.Left.Ref);
			Assert.NotNull(model.Or.Left.Equals.Right.Value);
			Assert.Equal(22, model.Or.Left.Equals.Right.Value);
			Assert.NotNull(model.Or.Right.NotEquals);
			Assert.NotNull(model.Or.Right.NotEquals.Left);
			Assert.NotNull(model.Or.Right.NotEquals.Right);
			Assert.NotNull(model.Or.Right.NotEquals.Left.Ref);
			Assert.Equal("y", model.Or.Right.NotEquals.Left.Ref);
			Assert.NotNull(model.Or.Right.NotEquals.Right.Value);
			Assert.Equal(45, model.Or.Right.NotEquals.Right.Value);
		}

		[Fact]
		public static void BuildNotModel() {
			var unary = Filter.Not(Filter.Equals(Filter.Variable("x"), Filter.Constant(22)));
			var model = unary.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.Not);
			Assert.NotNull(model.Not.Equals);
			Assert.NotNull(model.Not.Equals.Left);
			Assert.NotNull(model.Not.Equals.Right);
			Assert.NotNull(model.Not.Equals.Left.Ref);
			Assert.Equal("x", model.Not.Equals.Left.Ref);
			Assert.NotNull(model.Not.Equals.Right.Value);
			Assert.Equal(22, model.Not.Equals.Right.Value);
		}

		[Fact]
		public static void BuildConstantModel() {
			var constant = Filter.Constant(22);
			var model = constant.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.Value);
			Assert.Equal(22, model.Value);
		}

		[Fact]
		public static void BuildVariableModel() {
			var variable = Filter.Variable("x");
			var model = variable.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.Ref);
			Assert.Equal("x", model.Ref);
		}

		[Fact]
		public static void BuildFunctionModel() {
			var function = Filter.Function(Filter.Variable("x"), "f", Filter.Constant(22), Filter.Constant(45));
			var model = function.ToFilterModel();
			Assert.NotNull(model);
			Assert.NotNull(model.Function);
			Assert.NotNull(model.Function.Instance);
			Assert.Equal("x", model.Function.Instance);
			Assert.NotNull(model.Function.Name);
			Assert.Equal("f", model.Function.Name);
			Assert.NotNull(model.Function.Arguments);
			Assert.Equal(2, model.Function.Arguments.Length);
			Assert.NotNull(model.Function.Arguments[0].Value);
			Assert.Equal(22, model.Function.Arguments[0].Value);
			Assert.NotNull(model.Function.Arguments[1].Value);
			Assert.Equal(45, model.Function.Arguments[1].Value);
		}
	}
}

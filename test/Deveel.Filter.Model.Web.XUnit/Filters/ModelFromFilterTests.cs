using System.Text.Json;

namespace Deveel.Filters {
	public static class ModelFromFilterTests {
		[Fact]
		public static void BuildEqualModel() {
            var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equal);
            var model = binary.ToFilterModel();

            Assert.NotNull(model);
            Assert.NotNull(model.Equal);
            Assert.Null(model.Equal.Left);
            Assert.Null(model.Equal.Right);
			Assert.NotNull(model.Equal.BinaryData);

			Assert.True(model.Equal.BinaryData.ContainsKey("x"));
			var value = model.Equal.BinaryData["x"];

			Assert.Equal(JsonValueKind.Number, value.ValueKind);
			Assert.Equal(22, value.GetInt32());
        }

        [Fact]
		public static void BuildEqualModelExtended() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equal);
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false});
			Assert.NotNull(model);
			Assert.NotNull(model.Equal);
			Assert.NotNull(model.Equal.Left);
			Assert.NotNull(model.Equal.Right);
			Assert.NotNull(model.Equal.Left.Ref);
			Assert.Equal("x", model.Equal.Left.Ref);
			Assert.NotNull(model.Equal.Right.Value);
			Assert.Equal(22, model.Equal.Right.Value);
		}

		[Fact]
		public static void BuildNotEqualModel() {
            var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.NotEqual);
            var model = binary.ToFilterModel();
            Assert.NotNull(model);
            Assert.NotNull(model.NotEqual);
            Assert.Null(model.NotEqual.Left);
            Assert.Null(model.NotEqual.Right);
            Assert.NotNull(model.NotEqual.BinaryData);
            Assert.True(model.NotEqual.BinaryData.ContainsKey("x"));
            var value = model.NotEqual.BinaryData["x"];
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.Equal(22, value.GetInt32());
        }

		[Fact]
		public static void BuildNotEqualModelExtended() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.NotEqual);
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false });
			Assert.NotNull(model);
			Assert.NotNull(model.NotEqual);
			Assert.NotNull(model.NotEqual.Left);
			Assert.NotNull(model.NotEqual.Right);
			Assert.NotNull(model.NotEqual.Left.Ref);
			Assert.Equal("x", model.NotEqual.Left.Ref);
			Assert.NotNull(model.NotEqual.Right.Value);
			Assert.Equal(22, model.NotEqual.Right.Value);
		}

		[Fact]
		public static void BuildGreaterThanModel() {
            var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.GreaterThan);
            var model = binary.ToFilterModel();
            Assert.NotNull(model);
            Assert.NotNull(model.GreaterThan);
            Assert.Null(model.GreaterThan.Left);
            Assert.Null(model.GreaterThan.Right);
            Assert.NotNull(model.GreaterThan.BinaryData);
            Assert.True(model.GreaterThan.BinaryData.ContainsKey("x"));
            var value = model.GreaterThan.BinaryData["x"];
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.Equal(22, value.GetInt32());
        }

		[Fact]
		public static void BuildGreaterThanModelExtended() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.GreaterThan);
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false});
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
            Assert.Null(model.GreaterThanOrEqual.Left);
            Assert.Null(model.GreaterThanOrEqual.Right);
            Assert.NotNull(model.GreaterThanOrEqual.BinaryData);
            Assert.True(model.GreaterThanOrEqual.BinaryData.ContainsKey("x"));
            var value = model.GreaterThanOrEqual.BinaryData["x"];
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.Equal(22, value.GetInt32());
        }

		[Fact]
		public static void BuildGreaterThanOrEqualModelExtended() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.GreaterThanOrEqual);
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false });
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
            Assert.Null(model.LessThan.Left);
            Assert.Null(model.LessThan.Right);
            Assert.NotNull(model.LessThan.BinaryData);
            Assert.True(model.LessThan.BinaryData.ContainsKey("x"));
            var value = model.LessThan.BinaryData["x"];
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.Equal(22, value.GetInt32());
        }

		[Fact]
		public static void BuildLessThanModelExtended() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.LessThan);
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false});
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
            Assert.Null(model.LessThanOrEqual.Left);
            Assert.Null(model.LessThanOrEqual.Right);
            Assert.NotNull(model.LessThanOrEqual.BinaryData);
            Assert.True(model.LessThanOrEqual.BinaryData.ContainsKey("x"));
            var value = model.LessThanOrEqual.BinaryData["x"];
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.Equal(22, value.GetInt32());
        }

		[Fact]
		public static void BuildLessThanOrEqualModelExtended() {
			var binary = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.LessThanOrEqual);
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false});
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
			var binary = Filter.And(Filter.Equal(Filter.Variable("x"), Filter.Constant(22)), Filter.NotEquals(Filter.Variable("y"), Filter.Constant(45)));
            var model = binary.ToFilterModel();
            Assert.NotNull(model);
            Assert.NotNull(model.And);
            Assert.NotNull(model.And.Left);
            Assert.NotNull(model.And.Right);
			Assert.Null(model.And.BinaryData);

			Assert.NotNull(model.And.Left.Equal);
			Assert.NotNull(model.And.Left.Equal.BinaryData);
			Assert.True(model.And.Left.Equal.BinaryData.ContainsKey("x"));
			var value1 = model.And.Left.Equal.BinaryData["x"];
			Assert.Equal(JsonValueKind.Number, value1.ValueKind);
			Assert.Equal(22, value1.GetInt32());

			Assert.NotNull(model.And.Right.NotEqual);
			Assert.NotNull(model.And.Right.NotEqual.BinaryData);
			Assert.True(model.And.Right.NotEqual.BinaryData.ContainsKey("y"));
			var value2 = model.And.Right.NotEqual.BinaryData["y"];
			Assert.Equal(JsonValueKind.Number, value2.ValueKind);
			Assert.Equal(45, value2.GetInt32());
		}

		[Fact]
		public static void BuildAndModelExtended() {
			var binary = Filter.And(Filter.Equal(Filter.Variable("x"), Filter.Constant(22)), Filter.NotEquals(Filter.Variable("y"), Filter.Constant(45)));

			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false });

			Assert.NotNull(model);
			Assert.NotNull(model.And);
			Assert.NotNull(model.And.Left);
			Assert.NotNull(model.And.Right);

			Assert.NotNull(model.And.Left.Equal);
			Assert.NotNull(model.And.Left.Equal.Left);
			Assert.NotNull(model.And.Left.Equal.Right);

			Assert.NotNull(model.And.Left.Equal.Left.Ref);
			Assert.Equal("x", model.And.Left.Equal.Left.Ref);

			Assert.NotNull(model.And.Left.Equal.Right.Value);
			Assert.Equal(22, model.And.Left.Equal.Right.Value);

			Assert.NotNull(model.And.Right.NotEqual);
			Assert.NotNull(model.And.Right.NotEqual.Left);
			Assert.NotNull(model.And.Right.NotEqual.Right);

			Assert.NotNull(model.And.Right.NotEqual.Left.Ref);
			Assert.Equal("y", model.And.Right.NotEqual.Left.Ref);

			Assert.NotNull(model.And.Right.NotEqual.Right.Value);
			Assert.Equal(45, model.And.Right.NotEqual.Right.Value);
		}

		[Fact]
		public static void BuildOrModelExtended() {
			var binary = Filter.Or(Filter.Equal(Filter.Variable("x"), Filter.Constant(22)), Filter.NotEquals(Filter.Variable("y"), Filter.Constant(45)));
			var model = binary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false });
			Assert.NotNull(model);
			Assert.NotNull(model.Or);
			Assert.NotNull(model.Or.Left);
			Assert.NotNull(model.Or.Right);
			Assert.NotNull(model.Or.Left.Equal);
			Assert.NotNull(model.Or.Left.Equal.Left);
			Assert.NotNull(model.Or.Left.Equal.Right);
			Assert.NotNull(model.Or.Left.Equal.Left.Ref);
			Assert.Equal("x", model.Or.Left.Equal.Left.Ref);
			Assert.NotNull(model.Or.Left.Equal.Right.Value);
			Assert.Equal(22, model.Or.Left.Equal.Right.Value);
			Assert.NotNull(model.Or.Right.NotEqual);
			Assert.NotNull(model.Or.Right.NotEqual.Left);
			Assert.NotNull(model.Or.Right.NotEqual.Right);
			Assert.NotNull(model.Or.Right.NotEqual.Left.Ref);
			Assert.Equal("y", model.Or.Right.NotEqual.Left.Ref);
			Assert.NotNull(model.Or.Right.NotEqual.Right.Value);
			Assert.Equal(45, model.Or.Right.NotEqual.Right.Value);
		}

		[Fact]
		public static void BuildNotModelWithExtendedOperand() {
			var unary = Filter.Not(Filter.Equal(Filter.Variable("x"), Filter.Constant(22)));
			var model = unary.ToFilterModel(new FilterBuilderOptions { PreferBinaryData = false });
			Assert.NotNull(model);
			Assert.NotNull(model.Not);
			Assert.NotNull(model.Not.Equal);
			Assert.NotNull(model.Not.Equal.Left);
			Assert.NotNull(model.Not.Equal.Right);
			Assert.NotNull(model.Not.Equal.Left.Ref);
			Assert.Equal("x", model.Not.Equal.Left.Ref);
			Assert.NotNull(model.Not.Equal.Right.Value);
			Assert.Equal(22, model.Not.Equal.Right.Value);
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
			var function = Filter.Function(Filter.Variable("x"), "f", new[] { Filter.Constant(22), Filter.Constant(45) });
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

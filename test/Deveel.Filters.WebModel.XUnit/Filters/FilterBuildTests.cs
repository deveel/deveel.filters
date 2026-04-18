using System.Text.Json;

namespace Deveel.Filters {
	public static class FilterBuildTests {
		[Fact]
		public static void BuildSimpleBinaryFilter() {
			var filter = new FilterModel {
				Equal = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = "foo"
					},
					Right = new FilterModel {
						Value = 42
					}
				}
			};

			var result = filter.BuildFilter();
			Assert.NotNull(result);
			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Equal, result.ExpressionType);

			var left = Assert.IsType<VariableFilterExpression>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilterExpression>(binary.Right);
			Assert.Equal(42, right.Value);
		}

		[Fact]
		public static void BuildEqualsWithDynamicData() {
			var filter = new FilterModel {
				BinaryData = new Dictionary<string, JsonElement> {
					{ "foo", JsonDocument.Parse("42").RootElement }
				}
			};

			var result = filter.BuildFilter();

			Assert.NotNull(result);
			Assert.IsType<BinaryFilterExpression>(result);
			var binary = (BinaryFilterExpression) result;
			Assert.Equal(FilterExpressionType.Equal, binary.ExpressionType);

			var left = Assert.IsType<VariableFilterExpression>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilterExpression>(binary.Right);
			Assert.Equal(42, right.Value);
		}

		[Fact]
		public static void BuildSimpleBinaryFilterWithNot() {
			var filter = new FilterModel {
				Not = new FilterModel {
					Equal = new BinaryFilterModel {
						Left = new FilterModel {
							Ref = "foo"
						},
						Right = new FilterModel {
							Value = 42
						}
					}
				}
			};
			var result = filter.BuildFilter();
			Assert.NotNull(result);
			Assert.IsType<UnaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Not, result.ExpressionType);

			var binary = Assert.IsType<BinaryFilterExpression>(((UnaryFilterExpression) result).Operand);
			Assert.Equal(FilterExpressionType.Equal, binary.ExpressionType);

			var left = Assert.IsType<VariableFilterExpression>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilterExpression>(binary.Right);
			Assert.Equal(42, right.Value);
		}

		[Fact]
		public static void BuildVariable() {
			var filter = new FilterModel {
				Ref = "foo"
			};

			var result = filter.BuildFilter();
			Assert.NotNull(result);
			Assert.IsType<VariableFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Variable, result.ExpressionType);
			var variable = (VariableFilterExpression) result;
			Assert.Equal("foo", variable.VariableName);
		}

		[Fact]
		public static void BuildConstant() {
			var filter = new FilterModel {
				Value = 42
			};
			var result = filter.BuildFilter();
			Assert.NotNull(result);
			Assert.IsType<ConstantFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Constant, result.ExpressionType);
			var constant = (ConstantFilterExpression) result;
			Assert.Equal(42, constant.Value);
		}

		[Fact]
		public static void BuildConstantWithJson() {
			var filter = new FilterModel {
				Value = JsonDocument.Parse("42").RootElement
			};

			var result = filter.BuildFilter();
			Assert.NotNull(result);
			Assert.IsType<ConstantFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Constant, result.ExpressionType);
			var constant = (ConstantFilterExpression) result;
			Assert.Equal(42, constant.Value);
		}

		[Fact]
		public static void BuildFunction() {
			var function = new FilterModel {
				Function = new FunctionFilterModel {
					Name = "test",
					Instance = "x",
					Arguments = new[] {
						new FilterModel {
							Ref = "foo"
						},
						new FilterModel {
							Value = 42
						}
					}
				}
			};

			var result = function.BuildFilter();

			Assert.NotNull(result);
			Assert.IsType<FunctionFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Function, result.ExpressionType);

			var func = (FunctionFilterExpression) result;
			Assert.Equal("test", func.FunctionName);
			Assert.NotNull(func.Variable);
			Assert.Equal("x", func.Variable.VariableName);
			Assert.NotNull(func.Arguments);
			Assert.Equal(2, func.Arguments.Length);

			var arg1 = Assert.IsType<VariableFilterExpression>(func.Arguments[0]);
			Assert.Equal("foo", arg1.VariableName);

			var arg2 = Assert.IsType<ConstantFilterExpression>(func.Arguments[1]);
			Assert.Equal(42, arg2.Value);
		}

		[Fact]
		public static void BuildFunctionWithNonConstantArguments() {
			var error = Assert.Throws<ArgumentException>(() => {
                var function = new FilterModel {
                    Function = new FunctionFilterModel {
                        Name = "test",
                        Instance = "x",
                        Arguments = new[] {
                            new FilterModel {
								Not = new FilterModel {
									Ref = "foo"
								}
                            },
                            new FilterModel {
                                Ref = "bar"
                            }
                        }
                    }
                };
                function.BuildFilter();
            });
		}
	}
}

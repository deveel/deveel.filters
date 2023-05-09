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
			var binary = Assert.IsType<BinaryFilter>(result);
			Assert.Equal(FilterType.Equal, result.FilterType);

			var left = Assert.IsType<VariableFilter>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilter>(binary.Right);
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
			Assert.IsType<BinaryFilter>(result);
			var binary = (BinaryFilter) result;
			Assert.Equal(FilterType.Equal, binary.FilterType);

			var left = Assert.IsType<VariableFilter>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilter>(binary.Right);
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
			Assert.IsType<UnaryFilter>(result);
			Assert.Equal(FilterType.Not, result.FilterType);

			var binary = Assert.IsType<BinaryFilter>(((UnaryFilter) result).Operand);
			Assert.Equal(FilterType.Equal, binary.FilterType);

			var left = Assert.IsType<VariableFilter>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilter>(binary.Right);
			Assert.Equal(42, right.Value);
		}

		[Fact]
		public static void BuildVariable() {
			var filter = new FilterModel {
				Ref = "foo"
			};

			var result = filter.BuildFilter();
			Assert.NotNull(result);
			Assert.IsType<VariableFilter>(result);
			Assert.Equal(FilterType.Variable, result.FilterType);
			var variable = (VariableFilter) result;
			Assert.Equal("foo", variable.VariableName);
		}

		[Fact]
		public static void BuildConstant() {
			var filter = new FilterModel {
				Value = 42
			};
			var result = filter.BuildFilter();
			Assert.NotNull(result);
			Assert.IsType<ConstantFilter>(result);
			Assert.Equal(FilterType.Constant, result.FilterType);
			var constant = (ConstantFilter) result;
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
			Assert.IsType<FunctionFilter>(result);
			Assert.Equal(FilterType.Function, result.FilterType);

			var func = (FunctionFilter) result;
			Assert.Equal("test", func.FunctionName);
			Assert.NotNull(func.Variable);
			Assert.Equal("x", func.Variable.VariableName);
			Assert.NotNull(func.Arguments);
			Assert.Equal(2, func.Arguments.Length);

			var arg1 = Assert.IsType<VariableFilter>(func.Arguments[0]);
			Assert.Equal("foo", arg1.VariableName);

			var arg2 = Assert.IsType<ConstantFilter>(func.Arguments[1]);
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

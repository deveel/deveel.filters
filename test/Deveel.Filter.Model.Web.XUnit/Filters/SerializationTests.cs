using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
    public static class FilterModelSerializationTests {
		private static string Serialize(FilterModel model) {
			return JsonSerializer.Serialize(model, new JsonSerializerOptions {
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
			});
		}

		private static FilterModel? Deserialize(string json) {
            return JsonSerializer.Deserialize<FilterModel>(json, new JsonSerializerOptions {
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
			});
        }

		[Fact]
		public static void SerializeVariable() {
			var model = new FilterModel {
				Ref = "x",
			};

			var json = Serialize(model);

			Assert.Equal("{\"ref\":\"x\"}", json);
		}

		[Theory]
		[InlineData(123, "{\"value\":123}")]
		[InlineData("test", "{\"value\":\"test\"}")]
		[InlineData(true, "{\"value\":true}")]
		[InlineData(false, "{\"value\":false}")]
		[InlineData(123.456, "{\"value\":123.456}")]
		[InlineData(123.456f, "{\"value\":123.456}")]
		[InlineData(null, "{}")]
		public static void SerializeConstant(object? value, string expectedJson) {
			var model = new FilterModel {
				Value = value
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "{\"eq\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}}")]
		[InlineData("x", "test", "{\"eq\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}}")]
		public static void SerializeEquals(string varName, object value, string expectedJson) {
			var model = new FilterModel {
				Equals = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = varName
					},
					Right = new FilterModel {
						Value = value
					}
				}
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "{\"x\":123}")]
		[InlineData("x", "test", "{\"x\":\"test\"}")]
		[InlineData("x", 123.456, "{\"x\":123.456}")]
		[InlineData("x", 123.456f, "{\"x\":123.456}")]
		[InlineData("x", null, "{\"x\":null}")]
		public static void SerializeEqualsWithDynamicData(string key, object? value, string expectedJson) {
			var valueString = value == null ? "null" : (value is string s ? $"\"{s}\"" : Convert.ToString(value));
            var model = new FilterModel {
                ValueEquals = new Dictionary<string, JsonElement> {
					{ key, JsonDocument.Parse(valueString).RootElement }
                }
            };
            var json = Serialize(model);
            Assert.Equal(expectedJson, json);
        }

		[Theory]
		[InlineData("x", 123, "{\"neq\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}}")]
		[InlineData("x", "test", "{\"neq\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}}")]
		public static void SerializeNotEquals(string varName, object value, string expectedJson) {
			var model = new FilterModel {
				NotEquals = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = varName
					},
					Right = new FilterModel {
						Value = value
					}
				}
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "{\"gt\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}}")]
		[InlineData("x", "test", "{\"gt\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}}")]
		public static void SerializeGreaterThan(string varName, object value, string expectedJson) {
			var model = new FilterModel {
				GreaterThan = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = varName
					},
					Right = new FilterModel {
						Value = value
					}
				}
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "{\"gte\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}}")]
		[InlineData("x", "test", "{\"gte\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}}")]
		public static void SerializeGreaterThanOrEqual(string varName, object value, string expectedJson) {
			var model = new FilterModel {
				GreaterThanOrEqual = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = varName
					},
					Right = new FilterModel {
						Value = value
					}
				}
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "{\"lt\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}}")]
		[InlineData("x", "test", "{\"lt\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}}")]
		public static void SerializeLessThan(string varName, object value, string expectedJson) {
			var model = new FilterModel {
				LessThan = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = varName
					},
					Right = new FilterModel {
						Value = value
					}
				}
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "{\"lte\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}}")]
		[InlineData("x", "test", "{\"lte\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}}")]
		public static void SerializeLessThanOrEqual(string varName, object value, string expectedJson) {
			var model = new FilterModel {
				LessThanOrEqual = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = varName
					},
					Right = new FilterModel {
						Value = value
					}
				}
			};
			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "y", 456, "{\"and\":{\"left\":{\"eq\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":123}}},\"right\":{\"neq\":{\"left\":{\"ref\":\"y\"},\"right\":{\"value\":456}}}}}")]
		[InlineData("x", "test", "y", "test2", "{\"and\":{\"left\":{\"eq\":{\"left\":{\"ref\":\"x\"},\"right\":{\"value\":\"test\"}}},\"right\":{\"neq\":{\"left\":{\"ref\":\"y\"},\"right\":{\"value\":\"test2\"}}}}}")]
		public static void SerializeAnd(string varName1, object value1, string varName2, object value2, string expectedJson) {
			var model = new FilterModel {
				And = new BinaryFilterModel {
					Left = new FilterModel {
						Equals = new BinaryFilterModel {
							Left = new FilterModel {
								Ref = varName1
							},
							Right = new FilterModel {
								Value = value1
							}
						}
					},
					Right = new FilterModel {
						NotEquals = new BinaryFilterModel {
							Left = new FilterModel {
								Ref = varName2
							},
							Right = new FilterModel {
								Value = value2
							}
						}
					}
				}
			};

			var json = Serialize(model);

			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("x", 123, "y", 456, "{\"and\":{\"left\":{\"x\":123},\"right\":{\"y\":456}}}")]
		[InlineData("x", "test", "y", "test2", "{\"and\":{\"left\":{\"x\":\"test\"},\"right\":{\"y\":\"test2\"}}}")]
		public static void SerializeAndWithDyamicEquals(string varName1, object value1, string varName2, object value2, string expectedJson) {
			var jsonValue1 = JsonElementUtil.ToElement(value1);
			var jsonValue2 = JsonElementUtil.ToElement(value2);

			var model = new FilterModel {
				And = new BinaryFilterModel {
					Left = new FilterModel {
						ValueEquals = new Dictionary<string, JsonElement> {
							{ varName1, jsonValue1 }
                        }
					},
					Right = new FilterModel {
                        ValueEquals = new Dictionary<string, JsonElement> {
							{ varName2, jsonValue2 }
                        }
                    }
				}
			};

			var json = Serialize(model);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("{\"and\":{\"left\":{\"x\":123},\"right\":{\"y\":456}}}", "x", 123, "y", 456)]
		[InlineData("{\"and\":{\"left\":{\"x\":\"test\"},\"right\":{\"y\":\"test2\"}}}", "x", "test", "y", "test2")]
		public static void DeserializeAndWithDynamicEquals(string json, string varName1, object value1, string varName2, object value2) {
			var model = Deserialize(json);

			Assert.NotNull(model);
			Assert.NotNull(model.And);
			Assert.NotNull(model.And.Left);
			Assert.NotNull(model.And.Left.ValueEquals);
			Assert.NotNull(model.And.Right);
			Assert.NotNull(model.And.Right.ValueEquals);

			Assert.Equal(1, model.And.Left.ValueEquals.Count);
			Assert.Equal(1, model.And.Right.ValueEquals.Count);

			Assert.True(model.And.Left.ValueEquals.ContainsKey(varName1));
			Assert.True(model.And.Right.ValueEquals.ContainsKey(varName2));

			var jsonValue1 = JsonElementUtil.InferValue(model.And.Left.ValueEquals[varName1]);
			var jsonValue2 = JsonElementUtil.InferValue(model.And.Right.ValueEquals[varName2]);

			Assert.Equal(value1, jsonValue1);
			Assert.Equal(value2, jsonValue2);
		}
	}
}

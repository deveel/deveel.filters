using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Deveel.Filters {
    public static class ModelValidationTests {
        [Fact]
        public static void ValidteBinaryModel() {
            var model = new FilterModel {
                And = new BinaryFilterModel {
                    Left = new FilterModel {
                        Value = 20
                    },
                    Right = new FilterModel {
                        Ref = "x"
                    }
                }
            };

            var resuls = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Assert.True(Validator.TryValidateObject(model, context, resuls, true));

            Assert.Empty(resuls);
        }

        [Fact]
        public static void ValidateInvalidBinaryModel() {
            var model = new FilterModel {
                And = new BinaryFilterModel {
                    Left = new FilterModel {
                        Value = 20
                    }
                }
            };

            var context = new ValidationContext(model);
            var resuls = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(model, context, resuls, true));
            
            Assert.Single(resuls);
            Assert.Equal("Right", resuls[0].MemberNames.First());
        }

        [Fact]
        public static void ValidateVariableModel() {
            var model = new FilterModel {
                Ref = "x"
            };
            var context = new ValidationContext(model);
            var resuls = new List<ValidationResult>();
            Assert.True(Validator.TryValidateObject(model, context, resuls, true));
            Assert.Empty(resuls);
        }

        [Fact]
        public static void ValidateInvalidVariableModel() {
            var model = new FilterModel {
                Ref = ""
            };
            var context = new ValidationContext(model);
            var resuls = new List<ValidationResult>();
            Assert.False(Validator.TryValidateObject(model, context, resuls, true));
            Assert.Single(resuls);
            Assert.Equal("Ref", resuls[0].MemberNames.First());
        }

        [Fact]
        public static void ValidateConstantModel() {
            var model = new FilterModel {
                Value = 123
            };
            var context = new ValidationContext(model);
            var resuls = new List<ValidationResult>();
            Assert.True(Validator.TryValidateObject(model, context, resuls, true));
            Assert.Empty(resuls);
        }

        [Fact]
        public static void ValidateDynamicEqual() {
            var model = new FilterModel {
                ValueEquals = new Dictionary<string, JsonElement> {
                    { "x.name", JsonDocument.Parse("\"foo\"").RootElement }
                }
            };

            var context = new ValidationContext(model);
            var resuls = new List<ValidationResult>();
            Assert.True(Validator.TryValidateObject(model, context, resuls, true));
            Assert.Empty(resuls);
         }
    }
}

using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Deveel.Filters {
	public static class Program {
        public static void Main(string[] args) {
            BenchmarkRunner.Run<FilterTests>();
        }
    }

    [MemoryDiagnoser]
    [RankColumn, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RuntimeMoniker.Net60)]
	[SimpleJob(RuntimeMoniker.Net70)]
    public class FilterTests {
        [Benchmark]
        public void BuildSimpleEqual() {
            var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equal);

            filter.AsLambda(typeof(int), "x");
        }

        [Benchmark]
        public void BuildSimpleEqualOfT() {
            var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equal);

            filter.AsLambda<int>("x");
        }


        [Benchmark]
        public void BuildAsyncSimpleEqual() {
            var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equal);

            filter.AsAsyncLambda(typeof(int), "x");
        }

        [Benchmark]
        public void BuildAsyncSimpleEqualOfT() {
            var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(22), FilterType.Equal);

            filter.AsAsyncLambda<int>("x");
        }

        [Benchmark]
        public void BuildLogicalAndOfComplexObject() {
            var obj = new {value = 25};

            var filter = Filter.And(Filter.GreaterThan(Filter.Variable("x.value"), Filter.Constant(22)),
                               Filter.LessThanOrEqual(Filter.Variable("x.value"), Filter.Constant(33)));

            filter.AsLambda(obj.GetType(), "x");
        }


        #region Dynamic Linq

        [Benchmark]
        public void BuildSimpleEqualDynamicLinq() {
            const string linq = "x == 22";

            var param = Expression.Parameter(typeof(int), "x");
            DynamicExpressionParser.ParseLambda(new[] { param }, typeof(bool), linq, 33);
        }

        [Benchmark]
        public void BuildSimpleEqualDynamicLinqOfT() {
            const string linq = "it == 22";


            DynamicExpressionParser.ParseLambda<int, bool>(ParsingConfig.Default, true, linq, 33);
        }


        // Not supportet by Dynamic Linq
        //[Benchmark]
        //public void BuildSimpleAsyncEqualDynamicLinq() {
        //    const string linq = "it == 22";

        //    DynamicExpressionParser.ParseLambda(typeof(int), typeof(Task<bool>), linq, 33);
        //}

        [Benchmark]
        public void BuildLogicalAndOfComplexObjectDynamicLinq() {
            var obj = new { value = 25 };

            const string linq = "x.value > 22 && x.value <= 33";
            var param = Expression.Parameter(obj.GetType(), "x");
            DynamicExpressionParser.ParseLambda(new[] { param }, typeof(bool), linq, 33);
        }

        #endregion
    }

    public class AntiVirusFriendlyConfig : ManualConfig {
        public AntiVirusFriendlyConfig() {
            AddJob(Job
                .Default
                .WithToolchain(InProcessNoEmitToolchain.Instance));
        }
    }
}
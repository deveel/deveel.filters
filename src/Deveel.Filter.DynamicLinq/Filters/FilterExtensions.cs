using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Deveel.Filters {
	public static class FilterExtensions {
		public static LambdaExpression AsDynamicLamda(this Filter filter, Type parameterType, string parameterName = "x", ParsingConfig? config = null) {
			var filterString = filter.ToString();

			try {
				return DynamicExpressionParser.ParseLambda(config, parameterType, typeof(bool), filterString, parameterName);
			} catch (Exception ex) {

				throw new FilterException("Unable to construct the dynamic lamda", ex);
			}
		}

		public static Expression<Func<T, bool>> AsDynamicLambda<T>(this Filter filter, string parameterName = "x", ParsingConfig? config = null) 
			=>	(Expression<Func<T, bool>>)AsDynamicLamda(filter, typeof(T), parameterName, config);
	}
}
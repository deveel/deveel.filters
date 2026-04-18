// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Provides extension methods for converting <see cref="FilterExpression"/> instances
	/// to <see cref="FilterModel"/> web model representations.
	/// </summary>
    public static class FilterExtensions {
		/// <summary>
		/// Converts the specified <see cref="FilterExpression"/> to a <see cref="FilterModel"/>
		/// suitable for web serialization and exchange.
		/// </summary>
		/// <param name="filter">The filter expression to convert.</param>
		/// <param name="binaryOptions">
		/// Optional options controlling how binary filters are represented in the model.
		/// If <c>null</c>, default options are used.
		/// </param>
		/// <returns>A <see cref="FilterModel"/> representing the filter.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="filter"/> is <c>null</c>.
		/// </exception>
		public static FilterModel ToFilterModel(this FilterExpression filter, FilterBuilderOptions? binaryOptions = null) {
			if (filter == null)
                throw new ArgumentNullException(nameof(filter));

			if (binaryOptions == null)
				binaryOptions = new FilterBuilderOptions();

			var converter = new FilterModelConverter(binaryOptions);
			converter.Visit(filter);
			return converter.WebModel;
		}
	}
}

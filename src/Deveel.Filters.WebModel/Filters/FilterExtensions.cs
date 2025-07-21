namespace Deveel.Filters {
    public static class FilterExtensions {
		public static FilterModel ToFilterModel(this IFilter filter, FilterBuilderOptions? binaryOptions = null) {
			if (filter == null)
                throw new ArgumentNullException(nameof(filter));

			if (binaryOptions == null)
				binaryOptions = new FilterBuilderOptions();

			var converter = new FilterModelConverter(binaryOptions);
			return (FilterModel) converter.Visit(filter);
		}
	}
}

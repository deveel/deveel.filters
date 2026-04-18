namespace Deveel.Filters {
    public static class FilterExtensions {
		public static FilterModel ToFilterModel(this Filter filter, FilterBuilderOptions? binaryOptions = null) {
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

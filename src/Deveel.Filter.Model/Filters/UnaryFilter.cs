namespace Deveel.Filters {
	public sealed class UnaryFilter : Filter, IUnaryFilter {
		private readonly FilterType _filterType;

		internal UnaryFilter(Filter operand, FilterType filterType) {
			Operand = operand ?? throw new ArgumentNullException(nameof(operand));
			_filterType = filterType;
		}

		public override FilterType FilterType => _filterType;

		public Filter Operand { get; }

		IFilter IUnaryFilter.Operand => Operand;
	}
}

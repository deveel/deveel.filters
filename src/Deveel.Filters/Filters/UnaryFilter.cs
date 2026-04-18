namespace Deveel.Filters {
	public sealed class UnaryFilter : Filter {
		private readonly FilterType _filterType;

		internal UnaryFilter(Filter operand, FilterType filterType) {
			Operand = operand ?? throw new ArgumentNullException(nameof(operand));
			_filterType = filterType;
		}

		public override FilterType FilterType => _filterType;

		public Filter Operand { get; }
	}
}

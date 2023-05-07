namespace Deveel.Filters {
	public sealed class BinaryFilter : Filter {
		private readonly FilterType _filterType;

		internal BinaryFilter(Filter left, Filter right, FilterType filterType) {
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Right = right ?? throw new ArgumentNullException(nameof(right));
			_filterType = filterType;
		}

		public override FilterType FilterType => _filterType;

		public Filter Left { get; }

		public Filter Right { get; }
	}
}

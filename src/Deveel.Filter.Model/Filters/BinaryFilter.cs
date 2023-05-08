namespace Deveel.Filters {
	public sealed class BinaryFilter : Filter, IBinaryFilter {
		private readonly FilterType _filterType;

		internal BinaryFilter(Filter left, Filter right, FilterType filterType) {
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Right = right ?? throw new ArgumentNullException(nameof(right));
			_filterType = filterType;
		}

		public override FilterType FilterType => _filterType;

		public Filter Left { get; }

		IFilter IBinaryFilter.Left => Left;

		public Filter Right { get; }

		IFilter IBinaryFilter.Right => Right;
	}
}

namespace Deveel.Filters {
	public interface IBinaryFilter : IFilter {
		IFilter Left { get; }

		IFilter Right { get; }
	}
}

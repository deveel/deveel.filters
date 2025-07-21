namespace Deveel.Filters {
	public interface IUnaryFilter : IFilter {
		IFilter Operand { get; }
	}
}

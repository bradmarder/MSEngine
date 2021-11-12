namespace MSEngine.Core;

public readonly ref struct BufferKeeper
{
	public Span<Turn> Turns { get; init; }
	public Span<int> EdgeIndexes { get; init; }
	public Span<int> Mines { get; init; }
	public Span<int> VisitedIndexes { get; init; }
	public Span<int> RevealedMineCountNodeIndexes { get; init; }
	public Span<int> AdjacentHiddenNodeIndexes { get; init; }
	public Span<float> Grid { get; init; }
}

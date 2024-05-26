namespace MSEngine.Core;

public readonly ref struct BufferKeeper
{
	public required Span<Turn> Turns { get; init; }
	public required Span<int> RevealedMineCountNodeIndexes { get; init; }
	public required Span<int> AdjacentHiddenNodeIndexes { get; init; }
	public required Span<float> Grid { get; init; }
}

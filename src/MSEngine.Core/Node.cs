namespace MSEngine.Core;

public readonly record struct Node
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Node(int index, bool hasMine, byte mineCount, NodeState state)
	{
		Debug.Assert(index >= 0);
		Debug.Assert(mineCount >= 0);
		Debug.Assert(mineCount <= Engine.MaxNodeEdges);
		Debug.Assert(Enum.IsDefined(state));

		Index = index;
		HasMine = hasMine;
		MineCount = mineCount;
		State = state;
	}

	public int Index { get; init; }
	public bool HasMine { get; init; }
	public byte MineCount { get; init; }
	public NodeState State { get; init; }

	public string NewNodeCtor() => $"new {nameof(Node)}({Index}, {HasMine.ToString().ToLower()}, {MineCount}, {nameof(NodeState)}.{State}),";
}

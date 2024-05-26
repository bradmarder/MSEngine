using System.Diagnostics.CodeAnalysis;

namespace MSEngine.Core;

public readonly record struct Node
{
	[SetsRequiredMembers]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Node(int index, bool hasMine, byte mineCount, NodeState state)
	{
		Debug.Assert(index >= 0);
		Debug.Assert(mineCount >= 0);
		Debug.Assert(mineCount <= 8);
		Debug.Assert(Enum.IsDefined(state));

		Index = index;
		HasMine = hasMine;
		MineCount = mineCount;
		State = state;
	}

	public required int Index { get; init; }
	public required bool HasMine { get; init; }
	public required byte MineCount { get; init; }
	public required NodeState State { get; init; }

	public string NewNodeCtor() => $"new {nameof(Node)}({Index}, {HasMine.ToString().ToLower()}, {MineCount}, {nameof(NodeState)}.{State}),";
}

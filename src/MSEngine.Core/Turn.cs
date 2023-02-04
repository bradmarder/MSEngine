using System.Diagnostics.CodeAnalysis;

namespace MSEngine.Core;

public readonly record struct Turn
{
	[SetsRequiredMembers]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Turn(int nodeIndex, NodeOperation operation)
	{
		Debug.Assert(nodeIndex >= 0);
		Debug.Assert(Enum.IsDefined(operation));

		NodeIndex = nodeIndex;
		Operation = operation;
	}

	public required int NodeIndex { get; init; }
	public required NodeOperation Operation { get; init; }

	public string NewTurnCtor() => $"new {nameof(Turn)}({NodeIndex}, {nameof(NodeOperation)}.{Operation}),";
}

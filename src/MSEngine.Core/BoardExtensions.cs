namespace MSEngine.Core;

public static class BoardExtensions
{
	public static BoardStatus Status(this ReadOnlySpan<Node> nodes)
	{
		var complete = true;
		foreach (var node in nodes)
		{
			if (node is { HasMine: true, State: NodeState.Revealed })
			{
				return BoardStatus.Failed;
			}
			if (node is { HasMine: false, State: not NodeState.Revealed })
			{
				complete = false;
			}
		}
		return complete ? BoardStatus.Completed : BoardStatus.Pending;
	}
	public static bool IsComplete(this Span<Node> nodes)
	{
		foreach (var node in nodes)
		{
			if (node is { HasMine: false, State: not NodeState.Revealed })
			{
				return false;
			}
		}
		return true;
	}
	public static int FlagsAvailable(this ReadOnlySpan<Node> nodes) => nodes.MineCount() - nodes.FlaggedNodesCount();
	public static int MineCount(this ReadOnlySpan<Node> nodes)
	{
		var n = 0;
		foreach (var node in nodes)
		{
			if (node.HasMine)
			{
				n++;
			}
		}
		return n;
	}
	public static int FlaggedNodesCount(this ReadOnlySpan<Node> nodes)
	{
		var n = 0;
		foreach (var node in nodes)
		{
			if (node.State == NodeState.Flagged)
			{
				n++;
			}
		}
		return n;
	}
	public static bool AllMinesFlagged(this ReadOnlySpan<Node> nodes)
	{
		foreach (var node in nodes)
		{
			if (node is { HasMine: true, State: not NodeState.Flagged })
			{
				return false;
			}
		}
		return true;
	}
}

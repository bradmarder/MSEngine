﻿namespace MSEngine.Core;

public static class Engine
{
	public const byte MaxNodeEdges = 8;

	public static void FillCustomBoard(in Matrix<Node> matrix, Span<int> mines, int? safeNodeIndex = null)
	{
		if (safeNodeIndex is null)
		{
			Utilities.ScatterMines(mines, matrix.Nodes.Length);
		}
		else
		{
			Utilities.ScatterMines(mines, matrix.Nodes.Length, (int)safeNodeIndex, matrix.ColumnCount);
		}

		Span<int> buffer = stackalloc int[MaxNodeEdges];

		for (var i = 0; i < matrix.Nodes.Length; i++)
		{
			var hasMine = mines.Contains(i);
			var mineCount = hasMine ? byte.MinValue : Utilities.GetAdjacentMineCount(mines, i, matrix, buffer);

			matrix[i] = new(i, hasMine, mineCount, NodeState.Hidden);
		}
	}

	/// <summary>
	/// Validates a board-turn configuration-operation.
	/// Calling this method is optional - It's intended usage is only when building-testing a client
	/// </summary>
	/// <exception cref="InvalidGameStateException">
	/// -Multiple nodes have matching coordinates
	/// -Turns are not allowed if board status is completed/failed
	/// -Turn has coordinates that are outside the board
	/// -No more flags available
	/// -Only chord operations are allowed on revealed nodes
	/// -May not flag a node that is already flagged
	/// -Impossible to remove flag from un-flagged node
	/// -May only chord a revealed node
	/// -May only chord a node that has adjacent mines
	/// -May only chord a node when adjacent mine count equals adjacent node flag count
	/// -May only chord a node that has hidden adjacent nodes
	/// </exception>
	public static void EnsureValidBoardConfiguration(in Matrix<Node> matrix, Turn turn)
	{
		var nodes = matrix.Nodes;
		if (nodes.Status() == BoardStatus.Completed || nodes.Status() == BoardStatus.Failed)
		{
			throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
		}
		if (turn.NodeIndex >= nodes.Length)
		{
			throw new InvalidGameStateException("Turn has index outside the matrix");
		}
		if (turn.Operation == NodeOperation.Flag && nodes.FlagsAvailable() == 0)
		{
			throw new InvalidGameStateException("No more flags available");
		}

		var node = nodes[turn.NodeIndex];
		if (node.State == NodeState.Revealed && turn.Operation != NodeOperation.Chord && turn.Operation != NodeOperation.Reveal)
		{
			throw new InvalidGameStateException("Only chord/reveal operations are allowed on revealed nodes");
		}
		if (turn.Operation == NodeOperation.RemoveFlag && node.State != NodeState.Flagged)
		{
			throw new InvalidGameStateException("Impossible to remove flag from un-flagged node");
		}
		if (turn.Operation == NodeOperation.Chord)
		{
			if (node.State != NodeState.Revealed)
			{
				throw new InvalidGameStateException("May only chord a revealed node");
			}
			if (node.MineCount == 0)
			{
				throw new InvalidGameStateException("May only chord a node that has adjacent mines");
			}

			var nodeAdjacentFlagCount = 0;
			var nodeAdjacentHiddenCount = 0;
			Span<int> adjacentIndexes = stackalloc int[MaxNodeEdges];
			adjacentIndexes.FillAdjacentNodeIndexes(matrix, turn.NodeIndex);

			foreach (var i in adjacentIndexes)
			{
				if (i == -1) { continue; }

				var adjacentNode = nodes[i];
				if (adjacentNode.State == NodeState.Flagged) { nodeAdjacentFlagCount++; }
				if (adjacentNode.State == NodeState.Hidden) { nodeAdjacentHiddenCount++; }
			}

			if (node.MineCount != nodeAdjacentFlagCount)
			{
				throw new InvalidGameStateException("May only chord a node when adjacent mine count equals adjacent node flag count");
			}
			if (nodeAdjacentHiddenCount == 0)
			{
				throw new InvalidGameStateException("May only chord a node that has hidden adjacent nodes");
			}
		}
	}

	public static void ComputeBoard(in Matrix<Node> matrix, Turn turn)
	{
		ref var node = ref matrix[turn.NodeIndex];

		switch (turn.Operation)
		{
			case NodeOperation.Reveal:
				if (node.HasMine)
				{
					RevealHiddenMines(matrix.Nodes);
				}
				else
				{
					node = node with { State = NodeState.Revealed };
					if (node.MineCount == 0)
					{
						TriggerChainReaction(matrix, turn.NodeIndex);
					}
				}
				break;
			case NodeOperation.Flag:
				node = node with { State = NodeState.Flagged };
				break;
			case NodeOperation.RemoveFlag:
				node = node with { State = NodeState.Hidden };
				break;
			case NodeOperation.Chord:
				Chord(matrix, turn.NodeIndex);
				break;
			default:
				Debug.Fail(turn.Operation.ToString());
				break;
		}
	}

	internal static void Chord(in Matrix<Node> matrix, int nodeIndex)
	{
		Debug.Assert(nodeIndex >= 0);
		Debug.Assert(nodeIndex < matrix.Nodes.Length);

		Span<int> buffer = stackalloc int[MaxNodeEdges];
		buffer.FillAdjacentNodeIndexes(matrix, nodeIndex);

		foreach (var i in buffer)
		{
			if (i == -1) { continue; }
			if (matrix[i].State != NodeState.Hidden) { continue; }

			var turn = new Turn(i, NodeOperation.Reveal);
			ComputeBoard(matrix, turn);
		}
	}

	internal static void RevealHiddenMines(Span<Node> nodes)
	{
		foreach (ref var node in nodes)
		{
			if (node.HasMine && node.State == NodeState.Hidden)
			{
				node = node with { State = NodeState.Revealed };
			}
		}
	}

	internal static void TriggerChainReaction(in Matrix<Node> matrix, int nodeIndex)
	{
		var nodeCount = matrix.Nodes.Length;
		Debug.Assert(nodeIndex >= 0);
		Debug.Assert(nodeIndex < nodeCount);

		Span<int> visitors = nodeCount < 4_096 ? stackalloc int[nodeCount] : new int[nodeCount];
		ReadOnlySpan<int> readOnlyVisitors = visitors;
		visitors.Fill(-1);
		var queueVisitorEnumerator = visitors.GetEnumerator();
		var currentVisitorEnumerator = readOnlyVisitors.GetEnumerator();
		queueVisitorEnumerator.MoveNext();
		currentVisitorEnumerator.MoveNext();

		// set the first node to visit
		queueVisitorEnumerator.Current = nodeIndex;

		Span<int> buffer = stackalloc int[MaxNodeEdges];
		var queueVisitIndexCount = 1;

		while (true)
		{
			var index = currentVisitorEnumerator.Current;
			var pass = currentVisitorEnumerator.MoveNext();

			if (index == -1 || !pass) { break; }

			// we technically do not need to slice since the span is filled with -1
			// is the perf/complexity tradeoff worthwhile?
			var queueVisitIndexes = readOnlyVisitors.Slice(0, queueVisitIndexCount);

			buffer.FillAdjacentNodeIndexes(matrix, index);

			foreach (var i in buffer)
			{
				if (i == -1) { continue; }

				ref var node = ref matrix[i];

				if (node.State == NodeState.Flagged) { continue; }

				if (node.State == NodeState.Hidden)
				{
					node = node with { State = NodeState.Revealed };
				}

				if (node.MineCount == 0 && !queueVisitIndexes.Contains(i))
				{
					var more = queueVisitorEnumerator.MoveNext();
					Debug.Assert(more);
					queueVisitorEnumerator.Current = i;
					queueVisitIndexCount++;
				}
			}
		}
	}
}

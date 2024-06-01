namespace MSEngine.Core;

public static class Engine
{
	public static void FillCustomBoard(Span<Node> nodes)
	{
		Span<int> mines = stackalloc int[Minefield.Length];
		Utilities.ScatterMines(mines);

		for (var i = 0; i < NodeMatrix.Length; i++)
		{
			var hasMine = mines.Contains(i);
			var mineCount = hasMine ? byte.MinValue : Utilities.GetAdjacentMineCount(mines, i);
			nodes[i] = new(i, hasMine, mineCount, NodeState.Hidden);
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
	public static void EnsureValidBoardConfiguration(ReadOnlySpan<Node> matrix, Turn turn)
	{
		if (matrix.Status() == BoardStatus.Completed || matrix.Status() == BoardStatus.Failed)
		{
			throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
		}
		if (turn.NodeIndex >= NodeMatrix.Length)
		{
			throw new InvalidGameStateException("Turn has index outside the matrix");
		}
		if (turn.Operation == NodeOperation.Flag && matrix.FlagsAvailable() == 0)
		{
			throw new InvalidGameStateException("No more flags available");
		}

		var node = matrix[turn.NodeIndex];
		if (node.State is NodeState.Revealed && turn.Operation != NodeOperation.Chord && turn.Operation != NodeOperation.Reveal)
		{
			throw new InvalidGameStateException("Only chord/reveal operations are allowed on revealed nodes");
		}
		if (turn.Operation is NodeOperation.RemoveFlag && node.State != NodeState.Flagged)
		{
			throw new InvalidGameStateException("Impossible to remove flag from un-flagged node");
		}
		if (turn.Operation is NodeOperation.Chord)
		{
			if (node.State is not NodeState.Revealed)
			{
				throw new InvalidGameStateException("May only chord a revealed node");
			}
			if (node.MineCount == 0)
			{
				throw new InvalidGameStateException("May only chord a node that has adjacent mines");
			}

			var nodeAdjacentFlagCount = 0;
			var nodeAdjacentHiddenCount = 0;

			foreach (var i in Utilities.GetAdjacentNodeIndexes(turn.NodeIndex))
			{
				var adjacentNode = matrix[i];
				if (adjacentNode.State is NodeState.Flagged) { nodeAdjacentFlagCount++; }
				if (adjacentNode.State is NodeState.Hidden) { nodeAdjacentHiddenCount++; }
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

	public static void ComputeBoard(Span<Node> nodes, Turn turn)
	{
		ref var node = ref nodes[turn.NodeIndex];

		switch (turn.Operation)
		{
			case NodeOperation.Reveal:
				if (node.HasMine)
				{
					RevealHiddenMines(nodes);
				}
				else
				{
					node = node with { State = NodeState.Revealed };
					if (node.MineCount == 0)
					{
						TriggerChainReaction(nodes, turn.NodeIndex);
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
				Chord(nodes, turn.NodeIndex);
				break;
			default:
				Debug.Fail(turn.Operation.ToString());
				break;
		}
	}

	internal static void Chord(Span<Node> nodes, int nodeIndex)
	{
		Debug.Assert(nodeIndex >= 0);
		Debug.Assert(nodeIndex < NodeMatrix.Length);

		foreach (var i in Utilities.GetAdjacentNodeIndexes(nodeIndex))
		{
			if (nodes[i].State is not NodeState.Hidden) { continue; }

			var turn = new Turn(i, NodeOperation.Reveal);
			ComputeBoard(nodes, turn);
		}
	}

	public static void RevealHiddenMines(Span<Node> nodes)
	{
		foreach (ref var node in nodes)
		{
			if (node is { HasMine: true, State: NodeState.Hidden })
			{
				node = node with { State = NodeState.Revealed };
			}
		}
	}

	internal static void TriggerChainReaction(Span<Node> nodes, int nodeIndex)
	{
		Debug.Assert(nodeIndex >= 0);
		Debug.Assert(nodeIndex < NodeMatrix.Length);

		Span<int> visitors = stackalloc int[NodeMatrix.Length];
		ReadOnlySpan<int> readOnlyVisitors = visitors;
		visitors.Fill(-1);
		var queueVisitorEnumerator = visitors.GetEnumerator();
		var currentVisitorEnumerator = readOnlyVisitors.GetEnumerator();
		queueVisitorEnumerator.MoveNext();
		currentVisitorEnumerator.MoveNext();

		// set the first node to visit
		queueVisitorEnumerator.Current = nodeIndex;

		var queueVisitIndexCount = 1;

		while (true)
		{
			var index = currentVisitorEnumerator.Current;
			var pass = currentVisitorEnumerator.MoveNext();

			if (index < 0 || !pass) { break; }

			// we technically do not need to slice since the span is filled with -1
			// is the perf/complexity tradeoff worthwhile?
			var queueVisitIndexes = readOnlyVisitors.Slice(0, queueVisitIndexCount);

			foreach (var i in Utilities.GetAdjacentNodeIndexes(index))
			{
				ref var node = ref nodes[i];

				if (node.State is NodeState.Flagged) { continue; }

				if (node.State is NodeState.Hidden)
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

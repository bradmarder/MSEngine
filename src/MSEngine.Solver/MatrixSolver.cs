using CommunityToolkit.HighPerformance;
using MSEngine.Core;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver;

public static class MatrixSolver
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool TryAddTurn(Span<Turn> turns, Turn turn, ref int turnCount)
	{
		Debug.Assert(turnCount >= 0);
		Debug.Assert(turns.Length > turnCount);

		if (turns.Slice(0, turnCount).Contains(turn))
		{
			return false;
		}

		turns[turnCount] = turn;
		turnCount++;

		return true;
	}

	internal static void ReduceMatrix(
		Span2D<float> matrix,
		ReadOnlySpan<int> adjacentHiddenNodeIndexes,
		ReadOnlySpan<int> revealedAMCNodes,
		Span<Turn> turns,
		ref int turnCount,
		bool useAllHiddenNodes)
	{
		var hasReduced = false;
		var maxColumnIndex = matrix.Width - 1;

		for (var rowIndex = 0; rowIndex < matrix.Height; rowIndex++)
		{
			var row = matrix.GetRowSpan(rowIndex);
			var val = row[maxColumnIndex];

			// if the augment column is zero, then all the 1's in the row are not mines
			if (val == 0)
			{
				for (var c = 0; c < maxColumnIndex; c++)
				{
					if (row[c] == 1)
					{
						var index = adjacentHiddenNodeIndexes[c];
						var turn = new Turn(index, NodeOperation.Reveal);

						TryAddTurn(turns, turn, ref turnCount);
						matrix.GetColumn(c).Clear();
						hasReduced = true;
					}
				}
			}

			// if the sum of the row equals the augmented column, then all the 1's in the row are mines
			if (val > 0)
			{
				float sum = 0;
				for (var y = 0; y < maxColumnIndex; y++)
				{
					sum += row[y];
				}
				if (sum == val)
				{
					for (var c = 0; c < maxColumnIndex; c++)
					{
						if (row[c] == 1)
						{
							var index = adjacentHiddenNodeIndexes[c];
							var turn = new Turn(index, NodeOperation.Flag);

							TryAddTurn(turns, turn, ref turnCount);
							matrix.GetColumn(c).Clear();
							hasReduced = true;

							foreach (var i in Utilities.GetAdjacentNodeIndexes(index))
							{
								var adjRevealIndex = revealedAMCNodes.IndexOf(i);
								if (adjRevealIndex < 0) { continue; }

								matrix[adjRevealIndex, maxColumnIndex]--;
							}

							if (useAllHiddenNodes)
							{
								matrix[matrix.Height - 1, maxColumnIndex]--;
							}
						}
					}
				}
			}
		}

		if (hasReduced)
		{
			ReduceMatrix(matrix, adjacentHiddenNodeIndexes, revealedAMCNodes, turns, ref turnCount, useAllHiddenNodes);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateTurns(ReadOnlySpan<Node> nodes, Span<int> buffer, Span<Turn> turns, bool useAllHiddenNodes)
	{
		#region Revealed Nodes with AMC > 0

		var revealedAMCNodeCount = 0;
		foreach (var node in nodes)
		{
			if (node is { State: NodeState.Revealed, MineCount :> 0 }

				// optional, but major perf improvement
				&& Utilities.HasHiddenAdjacentNodes(nodes, node.Index))
			{
				buffer[revealedAMCNodeCount] = node.Index;
				revealedAMCNodeCount++;
			}
		}

		if (revealedAMCNodeCount == 0)
		{
			return 0;
		}

		var revealedAMCNodes = buffer.Slice(0, revealedAMCNodeCount);

		#endregion

		#region Adjacent Hidden Node Indexes

		var ahcCount = 0;

		foreach (var node in nodes)
		{
			if (node.State is not NodeState.Hidden) { continue; }

			var hasAHC = false;
			if (!useAllHiddenNodes)
			{
				foreach (var x in Utilities.GetAdjacentNodeIndexes(node.Index))
				{
					if (nodes[x] is { State: NodeState.Revealed, MineCount :> 0 })
					{
						hasAHC = true;
						break;
					}
				}
			}

			if (useAllHiddenNodes || hasAHC)
			{
				buffer[revealedAMCNodeCount + ahcCount] = node.Index;
				ahcCount++;
			}
		}

		var adjacentHiddenNodeIndexes = buffer.Slice(revealedAMCNodeCount, ahcCount);

		#endregion

		#region Raw Matrix Generation

		var rows = revealedAMCNodeCount + (useAllHiddenNodes ? 1 : 0);
		var columns = ahcCount + 1;
		var rented = ArrayPool<float>.Shared.Rent(rows * columns);
		var matrix = new Span2D<float>(rented, rows, columns);

		for (var row = 0; row < rows; row++)
		{
			// loop over all the columns in the last row and set them to 1
			// set the augment column equal to # of mines remaining
			var isLastRow = row == rows - 1;
			if (useAllHiddenNodes && isLastRow)
			{
				for (var i = 0; i < columns; i++)
				{
					var isAugmentedColumn = i == columns - 1;
					matrix[rows - 1, i] = isAugmentedColumn ? nodes.FlagsAvailable() : 1;
				}
				break;
			}

			var nodeIndex = revealedAMCNodes[row];
			for (var column = 0; column < columns; column++)
			{
				var isAugmentedColumn = column == columns - 1;

				matrix[row, column] = isAugmentedColumn
					? nodes[nodeIndex].MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodes, nodeIndex)
					: Utilities.AreNodesAdjacent(nodeIndex, adjacentHiddenNodeIndexes[column]) ? 1 : 0;
			}
		}

		#endregion

		var turnCount = 0;
		ReduceMatrix(matrix, adjacentHiddenNodeIndexes, revealedAMCNodes, turns, ref turnCount, useAllHiddenNodes);

		matrix.GaussEliminate();

		#region Guass Matrix Processing

		for (var rowIndex = 0; rowIndex < matrix.Height; rowIndex++)
		{
			var row = matrix.GetRowSpan(rowIndex);
			var vector = row[..^1];
			var augment = row[^1];
			float min = 0;
			float max = 0;
			foreach (var x in vector)
			{
				max += x > 0 ? x : 0;
				min += x < 0 ? x : 0;
			}

			if (augment != min && augment != max)
			{
				continue;
			}

			for (var column = 0; column < vector.Length; column++)
			{
				var val = vector[column];
				if (val == 0) { continue; }

				var index = adjacentHiddenNodeIndexes[column];
				var turn = augment == min
					? new Turn(index, val > 0 ? NodeOperation.Reveal : NodeOperation.Flag)
					: new Turn(index, val > 0 ? NodeOperation.Flag : NodeOperation.Reveal);

				TryAddTurn(turns, turn, ref turnCount);
			}
		}

		#endregion

		ArrayPool<float>.Shared.Return(rented);

		// we must return the turncount so the caller knows how much to slice from turns
		return turnCount;
	}
}

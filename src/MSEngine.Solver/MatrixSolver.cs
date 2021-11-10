using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver;

public static class MatrixSolver
{
	// zero'ify this column from all rows in the matrix
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void ZeroifyColumn(in Matrix<float> matrix, int column)
	{
		Debug.Assert(column >= 0);
		Debug.Assert(column < matrix.ColumnCount);

		foreach (var row in matrix)
		{
			row[column] = 0;
		}
	}

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
		in Matrix<Node> nodeMatrix,
		in Matrix<float> matrix,
		ReadOnlySpan<int> adjacentHiddenNodeIndexes,
		ReadOnlySpan<int> revealedAMCNodes,
		Span<Turn> turns,
		ref int turnCount,
		bool useAllHiddenNodes)
	{
		var hasReduced = false;
		var maxColumnIndex = matrix.ColumnCount - 1;
		Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];

		foreach (var row in matrix)
		{
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
						ZeroifyColumn(matrix, c);
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
							ZeroifyColumn(matrix, c);
							hasReduced = true;

							buffer.FillAdjacentNodeIndexes(nodeMatrix, index);

							foreach (var i in buffer)
							{
								if (i == -1) { continue; }
								var rowIndex = revealedAMCNodes.IndexOf(i);
								if (rowIndex == -1) { continue; }

								matrix[rowIndex, maxColumnIndex]--;
							}

							if (useAllHiddenNodes)
							{
								matrix[matrix.RowCount - 1, maxColumnIndex]--;
							}
						}
					}
				}
			}
		}

		if (hasReduced)
		{
			ReduceMatrix(nodeMatrix, matrix, adjacentHiddenNodeIndexes, revealedAMCNodes, turns, ref turnCount, useAllHiddenNodes);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateTurns(in Matrix<Node> nodeMatrix, in BufferKeeper buffs, bool useAllHiddenNodes)
	{
		var nodes = nodeMatrix.Nodes;

		#region Revealed Nodes with AMC > 0

		var revealedAMCNodeCount = 0;
		foreach (var node in nodes)
		{
			if (node.State == NodeState.Revealed && node.MineCount > 0

				// optional, but major perf improvement
				&& Utilities.HasHiddenAdjacentNodes(nodeMatrix, buffs.EdgeIndexes, node.Index))
			{
				buffs.RevealedMineCountNodeIndexes[revealedAMCNodeCount] = node.Index;
				revealedAMCNodeCount++;
			}
		}

		if (revealedAMCNodeCount == 0)
		{
			return 0;
		}

		var revealedAMCNodes = buffs.RevealedMineCountNodeIndexes.Slice(0, revealedAMCNodeCount);

		#endregion

		#region Adjacent Hidden Node Indexes

		var ahcCount = 0;

		foreach (var node in nodes)
		{
			if (node.State != NodeState.Hidden) { continue; }

			var hasAHC = false;
			if (!useAllHiddenNodes)
			{
				buffs.EdgeIndexes.FillAdjacentNodeIndexes(nodeMatrix, node.Index);

				foreach (var x in buffs.EdgeIndexes)
				{
					if (x == -1) { continue; }

					var adjNode = nodes[x];
					if (adjNode.State == NodeState.Revealed && adjNode.MineCount > 0)
					{
						hasAHC = true;
						break;
					}
				}
			}

			if (useAllHiddenNodes || hasAHC)
			{
				buffs.AdjacentHiddenNodeIndexes[ahcCount] = node.Index;
				ahcCount++;
			}
		}

		var adjacentHiddenNodeIndexes = buffs.AdjacentHiddenNodeIndexes.Slice(0, ahcCount);

		#endregion

		#region Raw Matrix Generation

		var rows = revealedAMCNodeCount + (useAllHiddenNodes ? 1 : 0);
		var columns = ahcCount + 1;
		var grids = buffs.Grid.Slice(0, rows * columns);
		var matrix = new Matrix<float>(grids, columns);

		for (var row = 0; row < rows; row++)
		{
			// loop over all the columns in the last row and set them to 1
			// set the augment column equal to # of mines remaining
			var isLastRow = row == rows - 1;
			if (useAllHiddenNodes && isLastRow)
			{
				for (var i = 0; i < matrix.ColumnCount; i++)
				{
					var isAugmentedColumn = i == matrix.ColumnCount - 1;
					matrix[matrix.RowCount - 1, i] = isAugmentedColumn ? nodes.FlagsAvailable() : 1;
				}
				break;
			}

			var nodeIndex = revealedAMCNodes[row];
			for (var column = 0; column < columns; column++)
			{
				var isAugmentedColumn = column == columns - 1;

				matrix[row, column] = isAugmentedColumn
					? nodes[nodeIndex].MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodeMatrix, buffs.EdgeIndexes, nodeIndex)
					: Utilities.AreNodesAdjacent(buffs.EdgeIndexes, nodes.Length, nodeMatrix.ColumnCount, nodeIndex, adjacentHiddenNodeIndexes[column]) ? 1 : 0;
			}
		}

		#endregion

		var turnCount = 0;
		ReduceMatrix(nodeMatrix, matrix, adjacentHiddenNodeIndexes, revealedAMCNodes, buffs.Turns, ref turnCount, useAllHiddenNodes);

		matrix.GaussEliminate();

		#region Guass Matrix Processing

		for (var row = 0; row < matrix.RowCount; row++)
		{
			var vector = matrix.Vector(row);
			var augmentColumn = matrix.Augment(row);
			float min = 0;
			float max = 0;
			foreach (var x in vector)
			{
				max += x > 0 ? x : 0;
				min += x < 0 ? x : 0;
			}

			if (augmentColumn != min && augmentColumn != max)
			{
				continue;
			}

			for (var column = 0; column < vector.Length; column++)
			{
				var val = vector[column];
				if (val == 0)
				{
					continue;
				}

				var index = adjacentHiddenNodeIndexes[column];
				var turn = augmentColumn == min
					? new Turn(index, val > 0 ? NodeOperation.Reveal : NodeOperation.Flag)
					: new Turn(index, val > 0 ? NodeOperation.Flag : NodeOperation.Reveal);

				TryAddTurn(buffs.Turns, turn, ref turnCount);
			}
		}

		#endregion

		// we must return the turncount so the caller knows how much to slice from turns
		return turnCount;
	}
}

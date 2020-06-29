﻿using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        // zero'ify this column from all rows in the matrix
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ZeroifyColumn(Matrix<float> matrix, int column)
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

            if (Utilities.Contains(turns.Slice(0, turnCount), turn))
            {
                return false;
            }

            turns[turnCount] = turn;
            turnCount++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReduceMatrix(
            Matrix<Node> nodeMatrix,
            Matrix<float> matrix,
            ReadOnlySpan<int> adjacentHiddenNodeIndexes,
            ReadOnlySpan<int> revealedAMCNodes,
            Span<Turn> turns,
            ref int turnCount,
            bool useAllHiddenNodes)
        {
            var hasReduced = false;
            var maxColumnIndex = matrix.ColumnCount - 1;

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

                                var buffer = Utilities.GetAdjacentNodeIndexes(index, nodeMatrix);

                                foreach (var i in buffer)
                                {
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
        public static int CalculateTurns(Matrix<Node> nodeMatrix, Span<Turn> turns, bool useAllHiddenNodes)
        {
            var nodes = nodeMatrix.Nodes;

            Span<int> revealedAMCNodes = stackalloc int[nodes.Length];

            #region Revealed Nodes with AMC > 0

            var revealedAMCNodeCount = 0;
            foreach (var node in nodes)
            {
                if (node.State == NodeState.Revealed && node.MineCount > 0 
                    
                    // optional, but major perf improvement
                    && Utilities.HasHiddenAdjacentNodes(nodeMatrix, node.Index))
                {
                    revealedAMCNodes[revealedAMCNodeCount] = node.Index;
                    revealedAMCNodeCount++;
                }
            }

            if (revealedAMCNodeCount == 0)
            {
                return 0;
            }

            revealedAMCNodes = revealedAMCNodes.Slice(0, revealedAMCNodeCount);

            #endregion

            #region Adjacent Hidden Node Indexes

            var ahcCount = 0;
            Span<int> adjacentHiddenNodeIndexes = stackalloc int[nodes.Length];

            foreach (var node in nodes)
            {
                if (node.State != NodeState.Hidden) { continue; }

                var hasAHC = false;
                if (!useAllHiddenNodes)
                {
                    var buffer = Utilities.GetAdjacentNodeIndexes(node.Index, nodeMatrix);

                    foreach (var x in buffer)
                    {
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
                    adjacentHiddenNodeIndexes[ahcCount] = node.Index;
                    ahcCount++;
                }
            }

            adjacentHiddenNodeIndexes = adjacentHiddenNodeIndexes.Slice(0, ahcCount);

            #endregion

            #region Raw Matrix Generation

            var rows = revealedAMCNodeCount + (useAllHiddenNodes ? 1 : 0);
            var columns = ahcCount + 1;

            var matrix = new Matrix<float>(
                stackalloc float[rows * columns],
                columns);

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
                        ? nodes[nodeIndex].MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodeMatrix, nodeIndex)
                        : Utilities.AreNodesAdjacent(nodes.Length, nodeMatrix.ColumnCount, nodeIndex, adjacentHiddenNodeIndexes[column]) ? 1 : 0;
                }
            }

            #endregion

            var turnCount = 0;
            ReduceMatrix(nodeMatrix, matrix, adjacentHiddenNodeIndexes, revealedAMCNodes, turns, ref turnCount, useAllHiddenNodes);

            matrix.GaussEliminate();

            #region Guass Matrix Processing

            var augmentIndex = matrix.ColumnCount - 1;
            Span<float> vector = stackalloc float[augmentIndex];

            foreach (var row in matrix)
            {
                for (var column = 0; column < augmentIndex; column++)
                {
                    vector[column] = row[column];
                }

                var augmentColumn = row[augmentIndex];
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

                for (var column = 0; column < augmentIndex; column++)
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

                    TryAddTurn(turns, turn, ref turnCount);
                }
            }

            #endregion

            // we must return the turncount so the caller knows how much to slice from turns
            return turnCount;
        }
    }
}
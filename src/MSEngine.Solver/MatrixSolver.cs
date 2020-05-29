using MSEngine.Core;
using System;
using System.Diagnostics;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        public static int CalculateTurns(ReadOnlySpan<Node> nodes, ref Span<Turn> turns, int columnCount, bool useAllHiddenNodes)
        {
            Debug.Assert(nodes.Length > 0);
            Debug.Assert(nodes.Length == turns.Length);

            Span<int> buffer = stackalloc int[8];
            Span<int> revealedAMCNodes = stackalloc int[nodes.Length];

            #region Revealed Nodes with AMC > 0

            var revealedAMCNodeCount = 0;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.State == NodeState.Revealed && node.MineCount > 0 && Utilities.HasHiddenAdjacentNodes(nodes, buffer, i, columnCount))
                {
                    revealedAMCNodes[revealedAMCNodeCount] = i;
                    revealedAMCNodeCount++;
                }
            }

            if (revealedAMCNodeCount == 0)
            {
                return 0;
            }

            #endregion

            #region Adjacent Hidden Node Indexes

            var ahcCount = 0;
            Span<int> adjacentHiddenNodeIndex = stackalloc int[nodes.Length];

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.State != NodeState.Hidden) { continue; }

                var hasAHC = false;
                if (!useAllHiddenNodes)
                {
                    buffer.FillAdjacentNodeIndexes(nodes.Length, i, columnCount);

                    foreach (var x in buffer)
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
                    adjacentHiddenNodeIndex[ahcCount] = i;
                    ahcCount++;
                }
            }

            #endregion

            #region Raw Matrix Generation

            var rows = revealedAMCNodeCount + (useAllHiddenNodes ? 1 : 0);
            var columns = ahcCount + 1;

            Span<float> nodeBuffer = stackalloc float[rows * columns];
            var matrix = new FlatMatrix<float>(nodeBuffer, columns);

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

                for (var column = 0; column < columns; column++)
                {
                    var nodeIndex = revealedAMCNodes[row];
                    var node = nodes[nodeIndex];
                    var isAugmentedColumn = column == columns - 1;
                    var val = isAugmentedColumn
                        ? node.MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodes, buffer, nodeIndex, columnCount)
                        : Utilities.IsAdjacentTo(buffer, nodes.Length, columnCount, nodeIndex, adjacentHiddenNodeIndex[column]) ? 1 : 0;

                    matrix[row, column] = val;

                    // if the augment column is zero, then all the 1's in the row are not mines
                    if (isAugmentedColumn && val == 0)
                    {
                        var tc = 0;
                        for (var c = 0; c < columns - 1; c++)
                        {
                            if (matrix[row, c] == 1)
                            {
                                var ni = adjacentHiddenNodeIndex[c];
                                turns[tc] = new Turn(ni, NodeOperation.Reveal);
                                tc++;
                            }
                        }
                        return tc;
                    }

                    // if the sum of the row equals the augmented column, then all the 1's in the row are mines
                    if (isAugmentedColumn)
                    {
                        float sum = 0;
                        for (var y = 0; y < columns - 1; y++)
                        {
                            sum += matrix[row, y];
                        }
                        if (sum == val)
                        {
                            var ta = 0;
                            for (var c = 0; c < columns - 1; c++)
                            {
                                if (matrix[row, c] == 1)
                                {
                                    var ni = adjacentHiddenNodeIndex[c];
                                    turns[ta] = new Turn(ni, NodeOperation.Flag);
                                    ta++;
                                }
                            }
                            return ta;
                        }
                    }
                }
            }

            #endregion

            matrix.GaussEliminate();

            #region Guass Matrix Processing

            var augmentIndex = matrix.ColumnCount - 1;
            Span<float> vector = stackalloc float[augmentIndex];
            var turnCount = 0;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < augmentIndex; column++)
                {
                    vector[column] = matrix[row, column];
                }
                
                var augmentColumn = matrix[row, augmentIndex];
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
                    var index = adjacentHiddenNodeIndex[column];

                    var turn = augmentColumn == min
                        ? new Turn(index, val > 0 ? NodeOperation.Reveal : NodeOperation.Flag)
                        : new Turn(index, val > 0 ? NodeOperation.Flag : NodeOperation.Reveal);

                    // prevent adding duplicate turns
                    if (turns.Slice(0, turnCount).IndexOf(turn) == -1)
                    {
                        turns[turnCount] = turn;
                        turnCount++;
                    }
                }
            }

            #endregion

            // we must return the turncount so the caller knows how much to slice from turns
            return turnCount;
        }
    }
}
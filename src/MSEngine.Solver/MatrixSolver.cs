using MSEngine.Core;
using System;
using System.Diagnostics;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        private static void ReduceMatrix(Matrix<float> matrix, ReadOnlySpan<int> adjacentHiddenNodeIndexes, Span<Turn> turns, ref int turnCount)
        {
            var removeRowCount = 0;
            Span<int> removeRowIndexes = stackalloc int[matrix.RowCount];

            for (var row = 0; row < matrix.RowCount; row++)
            {
                var val = matrix[row, matrix.ColumnCount - 1];

                // if the augment column is zero, then all the 1's in the row are not mines
                if (val == 0)
                {
                    removeRowIndexes[removeRowCount] = row;
                    removeRowCount++;

                    for (var c = 0; c < matrix.ColumnCount - 1; c++)
                    {
                        if (matrix[row, c] == 1)
                        {
                            var i = adjacentHiddenNodeIndexes[c];
                            turns[turnCount] = new Turn(i, NodeOperation.Reveal);
                            turnCount++;
                        }
                    }
                }

                // if the sum of the row equals the augmented column, then all the 1's in the row are mines
                if (val > 0)
                {
                    float sum = 0;
                    for (var y = 0; y < matrix.ColumnCount - 1; y++)
                    {
                        sum += matrix[row, y];
                    }
                    if (sum == val)
                    {
                        removeRowIndexes[removeRowCount] = row;
                        removeRowCount++;

                        for (var c = 0; c < matrix.ColumnCount - 1; c++)
                        {
                            if (matrix[row, c] == 1)
                            {
                                var i = adjacentHiddenNodeIndexes[c];
                                turns[turnCount] = new Turn(i, NodeOperation.Flag);
                                turnCount++;
                            }
                        }
                    }
                }
            }
            
            if (removeRowCount > 0)
            {
				// var foo = matrix.Nodes.Slice()
                // removeRowCount the rows/columns, prevent dupes
				// tons of slicing....
				//matrix.no
                //ReduceMatrix(matrix, adjacentHiddenNodeIndexes, turns, ref turnCount);
            }

            // requires multiple passes
            // remove rows
            // remove columns (unless the row removed was the useAllHiddenNodes row)
            // since doing multiple passes, must prevent duplicates?
        }

        public static int CalculateTurns(Matrix<Node> nodeMatrix, Span<Turn> turns, bool useAllHiddenNodes)
        {
            var nodes = nodeMatrix.Nodes;
            Debug.Assert(nodes.Length > 0);
            Debug.Assert(nodes.Length == turns.Length);
            
            Span<int> buffer = stackalloc int[8];
            Span<int> revealedAMCNodes = stackalloc int[nodes.Length];

            #region Revealed Nodes with AMC > 0

            var revealedAMCNodeCount = 0;
            foreach(var node in nodes)
            {
                if (node.State == NodeState.Revealed && node.MineCount > 0 && Utilities.HasHiddenAdjacentNodes(nodeMatrix, buffer, node.Index))
                {
                    revealedAMCNodes[revealedAMCNodeCount] = node.Index;
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

            foreach (var node in nodes)
            {
                if (node.State != NodeState.Hidden) { continue; }

                var hasAHC = false;
                if (!useAllHiddenNodes)
                {
                    buffer.FillAdjacentNodeIndexes(nodes.Length, node.Index, nodeMatrix.ColumnCount);

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
                    adjacentHiddenNodeIndex[ahcCount] = node.Index;
                    ahcCount++;
                }
            }

            #endregion

            #region Raw Matrix Generation

            var rows = revealedAMCNodeCount + (useAllHiddenNodes ? 1 : 0);
            var columns = ahcCount + 1;

            Span<float> nodeBuffer = stackalloc float[rows * columns];
            var matrix = new Matrix<float>(nodeBuffer, columns);

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
                        ? node.MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodeMatrix, buffer, nodeIndex)
                        : Utilities.IsAdjacentTo(buffer, nodes.Length, nodeMatrix.ColumnCount, nodeIndex, adjacentHiddenNodeIndex[column]) ? 1 : 0;

                    matrix[row, column] = val;
                }
            }

            #endregion

            #region Pre-Gaussian-Elimination Turn Calculation

            var turnCount = 0;
            ReduceMatrix(matrix, adjacentHiddenNodeIndex, turns, ref turnCount);

            #endregion

            matrix.GaussEliminate();

            #region Guass Matrix Processing

            var augmentIndex = matrix.ColumnCount - 1;
            Span<float> vector = stackalloc float[augmentIndex];

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
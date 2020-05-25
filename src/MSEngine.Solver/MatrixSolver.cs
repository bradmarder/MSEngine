using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAdjacentFlaggedNodeCount(ReadOnlySpan<Node> nodes, Span<int> adjacentIndexes, int nodeIndex)
        {
            Debug.Assert(adjacentIndexes.Length == 8);
            Debug.Assert(nodeIndex >= 0);

            adjacentIndexes.FillAdjacentNodeIndexes(nodes.Length, nodeIndex, 8);

            var n = 0;
            foreach (var i in adjacentIndexes)
            {
                if (i == -1) { continue; }

                if (nodes[i].State == NodeState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasHiddenAdjacentNodes(ReadOnlySpan<Node> nodes, Span<int> buffer, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(buffer.Length == 8);

            buffer.FillAdjacentNodeIndexes(nodes.Length, nodeIndex, 8);

            foreach (var x in buffer)
            {
                if (x == -1) { continue; }
                if (nodes[x].State == NodeState.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        public static void CalculateTurns(ReadOnlySpan<Node> nodes, ref Span<Turn> turns)
        {
            Debug.Assert(nodes.Length > 0);
            Debug.Assert(nodes.Length == turns.Length);

            Span<int> buffer = stackalloc int[8];
            Span<int> revealedAMCNodes = stackalloc int[nodes.Length];

            #region Revealed Nodes with AMC > 0

            var revealedAMCNodeCount = 0;
            for (int i = 0, l = nodes.Length; i < l; i++)
            {
                var node = nodes[i];
                if (node.State == NodeState.Revealed && node.MineCount > 0 && HasHiddenAdjacentNodes(nodes, buffer, i))
                {
                    revealedAMCNodes[revealedAMCNodeCount] = i;
                    revealedAMCNodeCount++;
                }
            }

            if (revealedAMCNodeCount == 0)
            {
                turns = Span<Turn>.Empty;
                return;
            }

            #endregion

            #region Adjacent Hidden Node Indexes

            var ahcCount = 0;

            // we allocate an inner buffer on the stack here because we must avoid mutating the original buffer while iterating over it
            Span<int> innerBuffer = stackalloc int[8];

            Span<int> adjacentHiddenNodeIndex = stackalloc int[nodes.Length];

            for (int i = 0, l = nodes.Length; i < l; i++)
            {
                var node = nodes[i];
                if (node.State != NodeState.Hidden) { continue; }

                var hasAHC = false;
                buffer.FillAdjacentNodeIndexes(nodes.Length, i, 8);

                foreach (var x in buffer)
                {
                    if (x == -1) { continue; }

                    var adjNode = nodes[x];
                    if (adjNode.State == NodeState.Revealed && adjNode.MineCount > 0 && HasHiddenAdjacentNodes(nodes, innerBuffer, x))
                    {
                        hasAHC = true;
                        break;
                    }
                }
                if (hasAHC)
                {
                    adjacentHiddenNodeIndex[ahcCount] = i;
                    ahcCount++;
                }
            }

            #endregion

            #region Raw Matrix Generation

            var rowCount = revealedAMCNodeCount;
            var columnCount = ahcCount + 1;

            Span<int> nodeBuffer = stackalloc int[rowCount * columnCount];
            var matrix = new FlatMatrix<int>(nodeBuffer, columnCount);

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var nodeIndex = revealedAMCNodes[row];
                    var node = nodes[nodeIndex];

                    matrix[row, column] = column == columnCount - 1

                        // augmented column has special logic
                        ? node.MineCount - GetAdjacentFlaggedNodeCount(nodes, buffer, nodeIndex)

                        : Utilities.IsAdjacentTo(buffer, nodes.Length, 8, nodeIndex, adjacentHiddenNodeIndex[column]) ? 1 : 0;
                }
            }

            #endregion

            matrix.GaussEliminate();

            #region Guass Matrix Processing

            // exclude the augmented column
            var finalIndex = columnCount - 1;
            Span<int> vector = stackalloc int[finalIndex];
            var turnCount = 0;

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < finalIndex; column++)
                {
                    vector[column] = matrix[row, column];
                }
                
                var final = matrix[row, finalIndex];
                var min = 0;
                var max = 0;
                foreach (var x in vector)
                {
                    max += x > 0 ? x : 0;
                    min += x < 0 ? x : 0;
                }

                if (final != min && final != max)
                {
                    continue;
                }

                for (var column = 0; column < finalIndex; column++)
                {
                    var val = vector[column];
                    if (val == 0)
                    {
                        continue;
                    }
                    var index = adjacentHiddenNodeIndex[column];

                    var turn = final == min

                        // All of the negative numbers in that row are mines and all of the positive values in that row are not mines
                        ? new Turn(index, val > 0 ? NodeOperation.Reveal : NodeOperation.Flag)

                        // All of the negative numbers in that row are not mines and all of the positive values in that row are mines.
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

            // we must slice due to overallocating the buffer
            turns = turns.Slice(0, turnCount);
        }
    }
}
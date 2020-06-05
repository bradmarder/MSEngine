using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAdjacentTo(Span<int> buffer, int nodeCount, int columnCount, int i1, int i2)
        {
            Debug.Assert(buffer.Length == 8);
            Debug.Assert(nodeCount > 0);
            Debug.Assert(columnCount > 0);
            Debug.Assert(i1 >= 0);
            Debug.Assert(i2 >= 0);
            Debug.Assert(i1 != i2);

            buffer.FillAdjacentNodeIndexes(nodeCount, i1, columnCount);

#if NETCOREAPP3_1
            return buffer.Contains(i2);
#else
            return buffer.IndexOf(i2) != -1;
#endif

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillAdjacentNodeIndexes(this Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(index >= 0);
            Debug.Assert(index < nodeCount);
            Debug.Assert(columnCount > 0);
            Debug.Assert(indexes.Length == 8);

            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = (index + 1) % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            indexes.Fill(-1);

            // ignore indexes 0/1/2 if isTop is true
            // ignore indexes 0/3/5 isLeftSide is true
            // ignore indexes 2/4/7 isRightSide is true
            // ignore indexes 5/6/7 if isBottom is true

            if (!isTop)
            {
                var val = index - columnCount;
                if (!isLeftSide)
                {
                    indexes[0] = val - 1;
                }
                indexes[1] = val;
                if (!isRightSide)
                {
                    indexes[2] = val + 1;
                }
            }
            if (!isLeftSide)
            {
                indexes[3] = index - 1;
            }
            if (!isRightSide)
            {
                indexes[4] = index + 1;
            }
            if (!isBottom)
            {
                var val = index + columnCount;
                if (!isLeftSide)
                {
                    indexes[5] = val - 1;
                }
                indexes[6] = val;
                if (!isRightSide)
                {
                    indexes[7] = val + 1;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scatter(this Span<int> mines, int nodeCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount >= mines.Length);

            // we must fill the buffer with -1 because the default (0) is a valid index
            mines.Fill(-1);

            int m;
            foreach (ref var x in mines)
            {
                // we use a loop to prevent duplicate indexes
                do
                {
                    m = RandomNumberGenerator.GetInt32(nodeCount);
                }
#if NETCOREAPP3_1
                while (mines.Contains(m));
#else
                while (mines.IndexOf(m) != -1);
#endif

                x = m;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, Span<int> buffer, int nodeIndex, int nodeCount, int columns)
        {
            Debug.Assert(buffer.Length == 8);
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeCount > 0);
            Debug.Assert(columns > 0);
            Debug.Assert(nodeIndex < nodeCount);

            buffer.FillAdjacentNodeIndexes(nodeCount, nodeIndex, columns);

            byte n = 0;
            foreach (var i in buffer)
            {
#if NETCOREAPP3_1
                if (mineIndexes.Contains(i))
#else
                if (mineIndexes.IndexOf(i) != -1)
#endif
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAdjacentFlaggedNodeCount(Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
            Debug.Assert(matrix.Nodes.Length > nodeIndex);
            Debug.Assert(buffer.Length == 8);
            Debug.Assert(nodeIndex >= 0);

            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            var n = 0;
            foreach (var i in buffer)
            {
                if (i == -1) { continue; }

                if (matrix.Nodes[i].State == NodeState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasHiddenAdjacentNodes(Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
            Debug.Assert(matrix.Nodes.Length > nodeIndex);
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(buffer.Length == 8);

            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            foreach (var x in buffer)
            {
                if (x == -1) { continue; }
                if (matrix.Nodes[x].State == NodeState.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        public static string Log(Matrix<Node> matrix)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var node in matrix.Nodes)
            {
                var state = string.Concat(nameof(NodeState), '.', node.State);
                sb.AppendLine($"new {nameof(Node)}({node.Index}, {node.HasMine.ToString().ToLower()}, {node.MineCount}, {state}),");
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        private static readonly IReadOnlyDictionary<int, int[]> _keyToAdjacentNodeIndexesMap;

        static Utilities()
        {
            var map = new Dictionary<int, int[]>(81);
            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];

            //beginner
            for (var i = 0; i < 81; i++)
            {
                buffer.FillBuffer(81, i, 9);

                var values = buffer.ToArray().Where(x => x >= 0).ToArray();
                map.Add(i, values);
            }

            //intermediate

            //expert

            _keyToAdjacentNodeIndexesMap = map;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreNodesAdjacent(int nodeCount, int columnCount, int nodeIndexOne, int nodeIndexTwo)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(columnCount > 0);
            Debug.Assert(nodeIndexOne >= 0);
            Debug.Assert(nodeIndexTwo >= 0);
            Debug.Assert(nodeIndexOne != nodeIndexTwo);
            Debug.Assert(nodeCount > nodeIndexOne);
            Debug.Assert(nodeCount > nodeIndexTwo);
            Debug.Assert(nodeCount % columnCount == 0);

            return GetAdjacentNodeIndexes(nodeIndexOne, nodeCount, columnCount)
                .Contains(nodeIndexTwo);
        }

        private static void FillBuffer(this Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(index >= 0);
            Debug.Assert(index < nodeCount);
            Debug.Assert(columnCount > 0);
            Debug.Assert(nodeCount % columnCount == 0);

            // the "Engine.MaxNodeEdges + 1" case if for scattering mines and including the safe node
            Debug.Assert(indexes.Length == Engine.MaxNodeEdges || indexes.Length == Engine.MaxNodeEdges + 1);

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
        public static int[] GetAdjacentNodeIndexes(int nodeIndex, Matrix<Node> matrix)
            => GetAdjacentNodeIndexes(nodeIndex, matrix.Nodes.Length, matrix.ColumnCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] GetAdjacentNodeIndexes(int nodeIndex, int nodeCount, int columnCount)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeIndex < nodeCount);
            Debug.Assert(nodeCount % columnCount == 0);

            if (nodeCount == 81 && columnCount == 9)
            {
                return _keyToAdjacentNodeIndexesMap[nodeIndex];
            }

            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];
            buffer.FillBuffer(nodeCount, nodeIndex, columnCount);

            return buffer
                .ToArray()
                .Where(x => x >= 0)
                .ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScatterMines(Span<int> mines, int nodeCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount >= mines.Length);

            ScatterMines(mines, nodeCount, ReadOnlySpan<int>.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScatterMines(Span<int> mines, int nodeCount, int safeNodeIndex, int columnCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount >= mines.Length);
            Debug.Assert(safeNodeIndex >= 0);
            Debug.Assert(columnCount > 0);

            var items = GetAdjacentNodeIndexes(safeNodeIndex, nodeCount, columnCount);
            Span<int> buffer = stackalloc int[items.Length + 1];
            items.CopyTo(buffer);
            buffer[items.Length] = safeNodeIndex;

            ScatterMines(mines, nodeCount, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScatterMines(Span<int> mines, int nodeCount, ReadOnlySpan<int> ignore)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount >= mines.Length);
            // Debug.Assert(nodeCount >= mines.Length + ignore.Length);

            // we must fill the buffer with -1 because the default (0) is a valid index
            mines.Fill(-1);

            int m;
            foreach (ref var x in mines)
            {
                // we use a loop to prevent duplicate indexes
                do
                {
                    m = RandomNumberGenerator.GetInt32(nodeCount);
                } while (mines.Contains(m) || ignore.Contains(m));

                x = m;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, int nodeIndex, int nodeCount, int columns)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeCount > 0);
            Debug.Assert(columns > 0);
            Debug.Assert(nodeIndex < nodeCount);

            var buffer = GetAdjacentNodeIndexes(nodeIndex, nodeCount, columns);

            byte n = 0;
            foreach (var i in buffer)
            {
                if (mineIndexes.Contains(i))
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetAdjacentFlaggedNodeCount(Matrix<Node> matrix, int nodeIndex)
        {
            Debug.Assert(matrix.Nodes.Length > nodeIndex);
            Debug.Assert(nodeIndex >= 0);

            var buffer = GetAdjacentNodeIndexes(nodeIndex, matrix);

            byte n = 0;
            foreach (var i in buffer)
            {
                if (matrix.Nodes[i].State == NodeState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasHiddenAdjacentNodes(Matrix<Node> matrix, int nodeIndex)
        {
            Debug.Assert(matrix.Nodes.Length > nodeIndex);
            Debug.Assert(nodeIndex >= 0);

            var buffer = GetAdjacentNodeIndexes(nodeIndex, matrix);

            foreach (var x in buffer)
            {
                if (matrix.Nodes[x].State == NodeState.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
#if NETCOREAPP3_1
            return MemoryExtensions.Contains(span, value);
#else
            return span.IndexOf(value) != -1;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this Span<T> span, T value) where T : IEquatable<T>
        {
#if NETCOREAPP3_1
            return MemoryExtensions.Contains(span, value);
#else
            return span.IndexOf(value) != -1;
#endif
        }

        public static string Log(Matrix<Node> matrix)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var node in matrix.Nodes)
            {
                sb.AppendLine(node.NewNodeCtor());
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        public static bool AreNodesAdjacent(Span<int> buffer, int nodeCount, int columnCount, int nodeIndexOne, int nodeIndexTwo)
        {
            Debug.Assert(buffer.Length == Engine.MaxNodeEdges);
            Debug.Assert(nodeCount > 0);
            Debug.Assert(columnCount > 0);
            Debug.Assert(nodeIndexOne >= 0);
            Debug.Assert(nodeIndexTwo >= 0);
            Debug.Assert(nodeIndexOne != nodeIndexTwo);
            Debug.Assert(nodeCount > nodeIndexOne);
            Debug.Assert(nodeCount > nodeIndexTwo);
            Debug.Assert(nodeCount % columnCount == 0);

            buffer.FillAdjacentNodeIndexes(nodeCount, nodeIndexOne, columnCount);

            return buffer.Contains(nodeIndexTwo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillAdjacentNodeIndexes(this Span<int> indexes, in Matrix<Node> matrix, int index)
            => indexes.FillAdjacentNodeIndexes(matrix.Nodes.Length, index, matrix.ColumnCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillAdjacentNodeIndexes(this Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            // ~10% optimization: help compiler assume it doesn't need to to check out-of-bounds on accessing indexes
            _ = indexes[Engine.MaxNodeEdges - 1];

            Debug.Assert(nodeCount > 0);
            Debug.Assert(index >= 0);
            Debug.Assert(index < nodeCount);
            Debug.Assert(columnCount > 0);
            Debug.Assert(nodeCount % columnCount == 0);

            // the "Engine.MaxNodeEdges + 1" case if for scattering mines and including the safe node
            Debug.Assert(indexes.Length == Engine.MaxNodeEdges || indexes.Length == Engine.MaxNodeEdges + 1);

            var indexPlusOne = index + 1;
            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = indexPlusOne % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            if (isTop)
            {
                indexes[0] = indexes[1] = indexes[2] = -1;
            }
            else
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

            if (isLeftSide)
            {
                indexes[0] = indexes[3] = indexes[5] = -1;
            }
            else
            {
                indexes[3] = index - 1;
            }

            if (isRightSide)
            {
                indexes[2] = indexes[4] = indexes[7] = -1;
            }
            else
            {
                indexes[4] = indexPlusOne;
            }

            if (isBottom)
            {
                indexes[5] = indexes[6] = indexes[7] = -1;
            }
            else
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
            Debug.Assert(nodeCount % columnCount == 0);

            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges + 1];
            buffer.FillAdjacentNodeIndexes(nodeCount, safeNodeIndex, columnCount);
            buffer[Engine.MaxNodeEdges] = safeNodeIndex;

            ScatterMines(mines, nodeCount, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScatterMines(Span<int> mines, int nodeCount, ReadOnlySpan<int> ignore)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount >= mines.Length);

            // we must fill the buffer with -1 because the default (0) is a valid index
            mines.Fill(-1);

            var n = 0;
            Span<byte> pool = stackalloc byte[2056];
            RandomNumberGenerator.Fill(pool);
            
            int m;
            foreach (ref var x in mines)
            {
                // we use a loop to prevent duplicate indexes
                do
                {
                    var slice = pool.Slice(n, sizeof(int));
                    n += sizeof(int);

                    // Warning -> do not calculate the Math.Abs of the immediate BitConverter.ToInt32(slice) result
                    // In the rare case where it equals int.MinValue, this would throw an overflow exception.
                    // By first modding by the nodeCount, we can guarantee safety of the Math.Abs method
                    m = Math.Abs(BitConverter.ToInt32(slice) % nodeCount);

                    Debug.Assert(m >= 0);
                    Debug.Assert(m < nodeCount);
                } while (mines.Contains(m) || ignore.Contains(m));

                x = m;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VectorScatterMines(Span<int> mines, int nodeCount, ReadOnlySpan<int> ignore)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount >= mines.Length);

            // we must fill the buffer with -1 because the default (0) is a valid index
            mines.Fill(-1);

            var n = 0;
            var poolN = 0;
            Span<byte> pool = stackalloc byte[2056];
            RandomNumberGenerator.Fill(pool);
            System.Numerics.Vector<byte> bar;
            System.Numerics.Vector<uint> foo = default;
            int m;
            foreach (ref var x in mines)
            {
                // we use a loop to prevent duplicate indexes
                do
                {
                    if (n == 0) // vector.count / sizeof(int)
                    {
                        bar = new System.Numerics.Vector<byte>(pool.Slice(poolN, 32)); // vector.count instead of 32
                        foo = System.Numerics.Vector.AsVectorUInt32(bar);
                        poolN++;
                    }

                    // Warning -> do not calculate the Math.Abs of the immediate BitConverter.ToInt32(slice) result
                    // In the rare case where it equals int.MinValue, this would throw an overflow exception.
                    // By first modding by the nodeCount, we can guarantee safety of the Math.Abs method
                    m = (int)(foo[n] % nodeCount);

                    n++;
                    if (n == 8) { n = 0; }

                    Debug.Assert(m >= 0);
                    Debug.Assert(m < nodeCount);
                } while (mines.Contains(m) || ignore.Contains(m));

                x = m;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte GetAdjacentMineCount(ReadOnlySpan<int> mines, int nodeIndex, in Matrix<Node> matrix, Span<int> buffer)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeIndex < matrix.Nodes.Length);

            buffer.FillAdjacentNodeIndexes(matrix, nodeIndex);

            byte n = 0;
            foreach (var i in buffer)
            {
                if (i != -1 && mines.Contains(i))
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetAdjacentFlaggedNodeCount(in Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
            Debug.Assert(matrix.Nodes.Length > nodeIndex);
            Debug.Assert(buffer.Length == Engine.MaxNodeEdges);
            Debug.Assert(nodeIndex >= 0);

            buffer.FillAdjacentNodeIndexes(matrix, nodeIndex);

            byte n = 0;
            foreach (var i in buffer)
            {
                if (i != -1 && matrix[i].State == NodeState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasHiddenAdjacentNodes(in Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
            Debug.Assert(matrix.Nodes.Length > nodeIndex);
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(buffer.Length == Engine.MaxNodeEdges);

            buffer.FillAdjacentNodeIndexes(matrix, nodeIndex);

            foreach (var i in buffer)
            {
                if (i != -1 && matrix[i].State == NodeState.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        public static string Log(in Matrix<Node> matrix)
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
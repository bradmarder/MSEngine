using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine();

        public virtual void FillBeginnerBoard(Span<Node> nodes)
        {
            Span<int> mines = stackalloc int[10];
            mines.Scatter(nodes.Length);

            FillCustomBoard(nodes, mines, 8, 8);
        }
        public virtual void FillIntermediateBoard(Span<Node> nodes)
        {
            Span<int> mines = stackalloc int[40];
            mines.Scatter(nodes.Length);

            FillCustomBoard(nodes, mines, 16, 16);
        }
        public virtual void FillExpertBoard(Span<Node> nodes)
        {
            Span<int> mines = stackalloc int[99];
            mines.Scatter(nodes.Length);

            FillCustomBoard(nodes, mines, 30, 16);
        }
        public virtual void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns, byte rows)
        {
            if (columns == 0) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            if (mines.Length >= nodes.Length) { throw new ArgumentOutOfRangeException(nameof(mines)); }
            if (nodes.Length != (columns * rows)) { throw new ArgumentOutOfRangeException(nameof(nodes)); }

            Span<int> buffer = stackalloc int[8];

            for (var i = 0; i < nodes.Length; i++)
            {
                var hasMine = mines.IndexOf(i) != -1;
                var amc = GetAdjacentMineCount(mines, buffer, i, nodes.Length, columns);
                
                nodes[i] = new Node(hasMine, amc);
            }
        }
        internal static int GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, Span<int> adjacentIndexes, int nodeIndex, int nodeCount, int columns)
        {
            Debug.Assert(adjacentIndexes.Length == 8);

            adjacentIndexes.FillAdjacentNodeIndexes(nodeCount, nodeIndex, columns);

            var n = 0;
            for (var i = 0; i < 8; i++)
            {
                if (mineIndexes.IndexOf(adjacentIndexes[i]) != -1)
                {
                    n++;
                }
            }
            return n;
        }
    }
}

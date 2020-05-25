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
            var nodeCount = columns * rows;
            if (mines.Length >= nodeCount) { throw new ArgumentOutOfRangeException(nameof(mines)); }
            if (nodes.Length != nodeCount) { throw new ArgumentOutOfRangeException(nameof(nodes)); }

            Span<int> buffer = stackalloc int[8];

            for (var i = 0; i < nodeCount; i++)
            {
                buffer.FillAdjacentNodeIndexes(nodeCount, i, columns);
                var amc = GetAdjacentMineCount(mines, buffer);
                var hasMine = mines.IndexOf(i) != -1;

                nodes[i] = new Node(hasMine, amc);
            }
        }
        private static int GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, ReadOnlySpan<int> adjacentIndexes)
        {
            Debug.Assert(adjacentIndexes.Length == 8);

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

using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    internal enum Shuffler
    {
        None,
        Random,
        Hacked
    }
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine(Shuffler.Random);
        public static IEngine PureInstance { get; } = new Engine(Shuffler.None);
        public static IEngine HackedInstance { get; } = new Engine(Shuffler.Hacked);

        private readonly Shuffler _shuffler;

        private Engine(Shuffler shuffler)
        {
            _shuffler = shuffler;
        }

        public virtual void FillBeginnerBoard(Span<Node> nodes) => FillCustomBoard(nodes, 8, 8, 10);
        public virtual void FillIntermediateBoard(Span<Node> nodes) => FillCustomBoard(nodes, 16, 16, 40);
        public virtual void FillExpertBoard(Span<Node> nodes) => FillCustomBoard(nodes, 30, 16, 99);
        public virtual void FillCustomBoard(Span<Node> nodes, byte columns, byte rows, byte mineCount)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            var nodeCount = columns * rows;
            if (mineCount >= nodeCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }
            if (nodes.Length != nodeCount) { throw new ArgumentOutOfRangeException(nameof(nodes)); }

            Span<int> mineIndexes = stackalloc int[mineCount];
            Span<int> adjacentIndexes = stackalloc int[8];

            mineIndexes.Scatter(nodeCount);

            for (var i = 0; i < nodeCount; i++)
            {
                adjacentIndexes.FillAdjacentNodeIndexes(nodeCount, i, columns);
                var amc = GetAdjacentMineCount(mineIndexes, adjacentIndexes);
                var hasMine = mineIndexes.IndexOf(i) != -1;

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

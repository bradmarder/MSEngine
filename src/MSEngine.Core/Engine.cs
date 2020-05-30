using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine();

        public virtual void FillBeginnerBoard(Span<Node> nodes) => FillCustomBoard(nodes, 10, 8);
        public virtual void FillIntermediateBoard(Span<Node> nodes) => FillCustomBoard(nodes, 40, 16);
        public virtual void FillExpertBoard(Span<Node> nodes) => FillCustomBoard(nodes, 99, 30);
        public virtual void FillCustomBoard(Span<Node> nodes, int mineCount, byte columns)
        {
            Span<int> mines = stackalloc int[mineCount];
            mines.Scatter(nodes.Length);

            FillCustomBoard(nodes, mines, columns);
        }
        public virtual void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns)
        {
            Debug.Assert(columns > 0);
            Debug.Assert(nodes.Length > mines.Length);
            Debug.Assert(nodes.Length % columns == 0);

            Span<int> buffer = stackalloc int[8];

            for (var i = 0; i < nodes.Length; i++)
            {
                var hasMine = mines.IndexOf(i) != -1;
                var amc = GetAdjacentMineCount(mines, buffer, i, nodes.Length, columns);
                
                nodes[i] = new Node(hasMine, amc);
            }
        }
        internal static int GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, Span<int> buffer, int nodeIndex, int nodeCount, int columns)
        {
            Debug.Assert(buffer.Length == 8);
            Debug.Assert(nodeIndex >= 0);

            buffer.FillAdjacentNodeIndexes(nodeCount, nodeIndex, columns);

            var n = 0;
            foreach (var i in buffer)
            {
                if (mineIndexes.IndexOf(i) != -1)
                {
                    n++;
                }
            }
            return n;
        }
    }
}

using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// |  Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    /// |-------- |---------:|---------:|---------:|------:|------:|------:|----------:|
    /// |     For | 19.01 us | 0.091 us | 0.085 us |     - |     - |     - |         - |
    /// | Foreach | 19.18 us | 0.427 us | 0.439 us |     - |     - |     - |         - |
    /// </summary>
    [MemoryDiagnoser]
    public class FillBoardForeachVsFor
    {
        [Benchmark]
        public void For()
        {
            Span<Node> nodes = stackalloc Node[480];
            ForFillCustomBoard(nodes, Span<int>.Empty, 30);
        }

        [Benchmark]
        public void Foreach()
        {
            Span<Node> nodes = stackalloc Node[480];
            RefForeachFillCustomBoard(nodes, Span<int>.Empty, 30);
        }

        public virtual void ForFillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns)
        {
            Span<int> buffer = stackalloc int[8];

            for (var i = 0; i < nodes.Length; i++)
            {
                var hasMine = mines.IndexOf(i) != -1;
                var amc = GetAdjacentMineCount(mines, buffer, i, nodes.Length, columns);

                nodes[i] = new Node(i, hasMine, amc);
            }
        }

        public virtual void RefForeachFillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns)
        {
            Span<int> buffer = stackalloc int[8];

            int i = 0;
            foreach (ref var node in nodes)
            {
                var hasMine = mines.IndexOf(i) != -1;
                var amc = GetAdjacentMineCount(mines, buffer, i, nodes.Length, columns);

                node = new Node(i, hasMine, amc);
                i++;
            }
        }

        internal static byte GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, Span<int> buffer, int nodeIndex, int nodeCount, int columns)
        {
            buffer.FillAdjacentNodeIndexes(nodeCount, nodeIndex, columns);

            byte n = 0;
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

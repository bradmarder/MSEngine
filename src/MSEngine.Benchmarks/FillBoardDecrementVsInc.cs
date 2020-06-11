using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// |             Method |     Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    /// |------------------- |---------:|----------:|----------:|------:|------:|------:|----------:|
    /// | OldFillCustomBoard | 2.820 us | 0.0493 us | 0.0461 us |     - |     - |     - |         - |
    /// | NewFillCustomBoard | 2.763 us | 0.0276 us | 0.0245 us |     - |     - |     - |         - |
    /// </summary>
    [MemoryDiagnoser]
    public class FillBoardDecrementVsInc
    {
        [Benchmark]
        public virtual void OldFillCustomBoard()
        {
            Span<Node> nodes = stackalloc Node[64];
            Span<int> mines = stackalloc int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];

            for (var i = 0; i < nodes.Length; i++)
            {
                var hasMine = Utilities.Contains(mines, i);
                var amc = Utilities.GetAdjacentMineCount(mines, buffer, i, nodes.Length, 8);

                nodes[i] = new Node(i, hasMine, amc);
            }
        }

        [Benchmark]
        public virtual void NewFillCustomBoard()
        {
            Span<Node> nodes = stackalloc Node[64];
            Span<int> mines = stackalloc int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];

            for (var i = nodes.Length - 1; i >= 0; i--)
            {
                var hasMine = Utilities.Contains(mines, i);
                var amc = Utilities.GetAdjacentMineCount(mines, buffer, i, nodes.Length, 8);

                nodes[i] = new Node(i, hasMine, amc);
            }
        }
    }
}

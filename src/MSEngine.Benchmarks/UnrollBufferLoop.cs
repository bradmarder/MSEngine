using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// | Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    /// |------- |---------:|---------:|---------:|------:|------:|------:|----------:|
    /// |    Old | 49.70 ns | 0.226 ns | 0.211 ns |     - |     - |     - |         - |
    /// |    New | 71.09 ns | 0.364 ns | 0.340 ns |     - |     - |     - |         - |
    /// </summary>
    [MemoryDiagnoser]
    public class UnrollBufferLoop
    {
        [Benchmark]
        public void Old()
        {
            Span<int> buffer = stackalloc int[8];
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, true, 0, NodeState.Revealed),
                new(1, true, 0, NodeState.Revealed),
                new(2, true, 0, NodeState.Revealed),
                new(3, true, 0, NodeState.Revealed),
                new(4, true, 0, NodeState.Revealed),
                new(5, true, 0, NodeState.Revealed),
                new(6, true, 0, NodeState.Revealed),
                new(7, true, 0, NodeState.Revealed),
                new(8, true, 0, NodeState.Revealed),
            };
            var matrix = new Matrix<Node>(nodes, 3);
            OldHasHiddenAdjacentNodes(matrix, buffer, 4);
        }

        [Benchmark]
        public void New()
        {
            Span<int> buffer = stackalloc int[8];
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, true, 0, NodeState.Revealed),
                new(1, true, 0, NodeState.Revealed),
                new(2, true, 0, NodeState.Revealed),
                new(3, true, 0, NodeState.Revealed),
                new(4, true, 0, NodeState.Revealed),
                new(5, true, 0, NodeState.Revealed),
                new(6, true, 0, NodeState.Revealed),
                new(7, true, 0, NodeState.Revealed),
                new(8, true, 0, NodeState.Revealed),
            };
            var matrix = new Matrix<Node>(nodes, 3);
            NewHasHiddenAdjacentNodes(matrix, buffer, 4);
        }

        [Benchmark]
        public void LastChance()
        {
            Span<int> buffer = stackalloc int[8];
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, true, 0, NodeState.Revealed),
                new(1, true, 0, NodeState.Revealed),
                new(2, true, 0, NodeState.Revealed),
                new(3, true, 0, NodeState.Revealed),
                new(4, true, 0, NodeState.Revealed),
                new(5, true, 0, NodeState.Revealed),
                new(6, true, 0, NodeState.Revealed),
                new(7, true, 0, NodeState.Revealed),
                new(8, true, 0, NodeState.Revealed),
            };
            var matrix = new Matrix<Node>(nodes, 3);
            LastChance(matrix, buffer, 4);
        }

        public static bool OldHasHiddenAdjacentNodes(Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
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
        public static bool NewHasHiddenAdjacentNodes(Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            var enumerator = buffer.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            }

            return false;
        }
        public static bool LastChance(Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {
            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            var enumerator = buffer.GetEnumerator();
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }
            enumerator.MoveNext();

            if (enumerator.Current != -1 && matrix.Nodes[enumerator.Current].State == NodeState.Hidden) { return true; }

            return false;
        }
    }
}

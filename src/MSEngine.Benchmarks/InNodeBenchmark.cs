using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;
using System.Collections.Generic;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// |    Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    /// |---------- |---------:|---------:|---------:|------:|------:|------:|----------:|
    /// | WithoutIn | 82.69 ns | 0.419 ns | 0.327 ns |     - |     - |     - |         - |
    /// |    WithIn | 79.21 ns | 0.285 ns | 0.266 ns |     - |     - |     - |         - |
    /// </summary>
    [MemoryDiagnoser]
    public class InNodeBenchmark
    {
        [Benchmark]
        public void WithoutIn()
        {
            var node = new BenchmarkNodeWithoutIn();
            Span<BenchmarkNodeWithoutIn> nodes = stackalloc BenchmarkNodeWithoutIn[]
            {
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithoutIn(node, NodeOperation.Flag)
            };
        }

        [Benchmark]
        public void WithIn()
        {
            var node = new BenchmarkNodeWithIn();
            Span<BenchmarkNodeWithIn> nodes = stackalloc BenchmarkNodeWithIn[]
            {
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag),
                new BenchmarkNodeWithIn(node, NodeOperation.Flag)
            };
        }
    }

    public readonly struct BenchmarkNodeWithoutIn
    {
        private static readonly IReadOnlyDictionary<NodeOperation, NodeState> _operationToStateMap = new Dictionary<NodeOperation, NodeState>
        {
            [NodeOperation.Flag] = NodeState.Flagged,
            [NodeOperation.RemoveFlag] = NodeState.Hidden,
            [NodeOperation.Reveal] = NodeState.Revealed
        };
        internal BenchmarkNodeWithoutIn(BenchmarkNodeWithoutIn node, NodeOperation op)
        {
            Index = node.Index;
            HasMine = node.HasMine;
            MineCount = node.MineCount;
            State = _operationToStateMap[op];
        }

        public int Index { get; }
        public bool HasMine { get; }
        public byte MineCount { get; }
        public NodeState State { get; }
    }

    public readonly struct BenchmarkNodeWithIn
    {
        private static readonly IReadOnlyDictionary<NodeOperation, NodeState> _operationToStateMap = new Dictionary<NodeOperation, NodeState>
        {
            [NodeOperation.Flag] = NodeState.Flagged,
            [NodeOperation.RemoveFlag] = NodeState.Hidden,
            [NodeOperation.Reveal] = NodeState.Revealed
        };
        internal BenchmarkNodeWithIn(in BenchmarkNodeWithIn node, NodeOperation op)
        {
            Index = node.Index;
            HasMine = node.HasMine;
            MineCount = node.MineCount;
            State = _operationToStateMap[op];
        }

        public int Index { get; }
        public bool HasMine { get; }
        public byte MineCount { get; }
        public NodeState State { get; }
    }
}

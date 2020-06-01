using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;
using System.Collections.Generic;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// |     Method |     Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    /// |----------- |---------:|----------:|----------:|------:|------:|------:|----------:|
    /// | Dictionary | 6.670 ns | 0.0133 ns | 0.0118 ns |     - |     - |     - |         - |
    /// |     Switch | 1.564 ns | 0.0135 ns | 0.0126 ns |     - |     - |     - |         - |
    /// </summary>
    [MemoryDiagnoser]
    public class NodeSwitchDictionaryBenchmark
    {
        [Benchmark]
        public void Dictionary()
        {
            new NodeDictionary(0, true, 0, NodeOperation.Reveal);
        }

        [Benchmark]
        public void Switch()
        {
            new NodeSwitch(0, true, 0, NodeOperation.Reveal);
        }
    }

    public readonly struct NodeDictionary
    {
        private static readonly IReadOnlyDictionary<NodeOperation, NodeState> _operationToStateMap = new Dictionary<NodeOperation, NodeState>
        {
            [NodeOperation.Flag] = NodeState.Flagged,
            [NodeOperation.RemoveFlag] = NodeState.Hidden,
            [NodeOperation.Reveal] = NodeState.Revealed
        };
        internal NodeDictionary(int index, bool hasMine, int mineCount, NodeOperation op)
        {
            Index = index;
            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = _operationToStateMap[op];
        }

        public int Index { get; }
        public bool HasMine { get; }
        public byte MineCount { get; }
        public NodeState State { get; }
    }

    public readonly struct NodeSwitch
    {
        internal NodeSwitch(int index, bool hasMine, int mineCount, NodeOperation op)
        {
            Index = index;
            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = op switch
            {
                NodeOperation.Reveal => NodeState.Revealed,
                NodeOperation.Flag => NodeState.Flagged,
                _ => NodeState.Hidden,
            };
        }

        public int Index { get; }
        public bool HasMine { get; }
        public byte MineCount { get; }
        public NodeState State { get; }
    }
}

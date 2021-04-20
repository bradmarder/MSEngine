using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;
using System.Diagnostics;

namespace MSEngine.Benchmarks
{
    [MemoryDiagnoser]
    public class StaticBeginnerGame
    {
        [Benchmark]
        public void Play()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, false, 0, NodeState.Hidden),
                new(1, false, 0, NodeState.Hidden),
                new(2, false, 1, NodeState.Hidden),
                new(3, true, 0, NodeState.Hidden),
                new(4, false, 1, NodeState.Hidden),
                new(5, false, 0, NodeState.Hidden),
                new(6, false, 0, NodeState.Hidden),
                new(7, false, 0, NodeState.Hidden),
                new(8, false, 0, NodeState.Hidden),
                new(9, false, 0, NodeState.Hidden),
                new(10, false, 1, NodeState.Hidden),
                new(11, false, 1, NodeState.Hidden),
                new(12, false, 1, NodeState.Hidden),
                new(13, false, 1, NodeState.Hidden),
                new(14, false, 1, NodeState.Hidden),
                new(15, false, 1, NodeState.Hidden),
                new(16, false, 1, NodeState.Hidden),
                new(17, false, 1, NodeState.Hidden),
                new(18, false, 0, NodeState.Hidden),
                new(19, false, 0, NodeState.Hidden),
                new(20, false, 0, NodeState.Hidden),
                new(21, false, 1, NodeState.Hidden),
                new(22, true, 0, NodeState.Hidden),
                new(23, false, 1, NodeState.Hidden),
                new(24, true, 1, NodeState.Hidden),
                new(25, false, 2, NodeState.Hidden),
                new(26, false, 1, NodeState.Hidden),
                new(27, false, 1, NodeState.Hidden),
                new(28, false, 1, NodeState.Hidden),
                new(29, false, 1, NodeState.Hidden),
                new(30, false, 2, NodeState.Hidden),
                new(31, false, 2, NodeState.Hidden),
                new(32, true, 1, NodeState.Hidden),
                new(33, false, 2, NodeState.Hidden),
                new(34, false, 1, NodeState.Hidden),
                new(35, true, 0, NodeState.Hidden),
                new(36, false, 1, NodeState.Hidden),
                new(37, false, 1, NodeState.Hidden),
                new(38, false, 2, NodeState.Hidden),
                new(39, true, 1, NodeState.Hidden),
                new(40, false, 2, NodeState.Hidden),
                new(41, false, 2, NodeState.Hidden),
                new(42, false, 1, NodeState.Hidden),
                new(43, false, 1, NodeState.Hidden),
                new(44, false, 1, NodeState.Hidden),
                new(45, false, 1, NodeState.Hidden),
                new(46, true, 1, NodeState.Hidden),
                new(47, false, 2, NodeState.Hidden),
                new(48, true, 1, NodeState.Hidden),
                new(49, false, 2, NodeState.Hidden),
                new(50, false, 1, NodeState.Hidden),
                new(51, false, 0, NodeState.Hidden),
                new(52, false, 0, NodeState.Hidden),
                new(53, false, 2, NodeState.Hidden),
                new(54, false, 2, NodeState.Hidden),
                new(55, false, 2, NodeState.Hidden),
                new(56, false, 2, NodeState.Hidden),
                new(57, true, 1, NodeState.Hidden),
                new(58, false, 1, NodeState.Hidden),
                new(59, false, 0, NodeState.Hidden),
                new(60, false, 0, NodeState.Hidden),
                new(61, false, 1, NodeState.Hidden),
                new(62, true, 0, NodeState.Hidden),
                new(63, false, 1, NodeState.Hidden)
            };
            ReadOnlySpan<Turn> turns = stackalloc Turn[]
            {
                new Turn(57, NodeOperation.Flag),
                new Turn(62, NodeOperation.Flag),
                new Turn(63, NodeOperation.Reveal),
                new Turn(56, NodeOperation.Reveal),
                new Turn(53, NodeOperation.Reveal),
                new Turn(54, NodeOperation.Reveal),
                new Turn(55, NodeOperation.Reveal),
                new Turn(48, NodeOperation.Flag),
                new Turn(46, NodeOperation.Flag),
                new Turn(47, NodeOperation.Reveal)
            };
            var matrix = new Matrix<Node>(nodes, 8);
            Span<int> indexes = stackalloc int[nodes.Length];

            foreach (var x in turns)
            {
                Engine.ComputeBoard(matrix, x, indexes);
            }

            Debug.Assert(matrix.Nodes.Status() == BoardStatus.Completed);
        }
    }
}

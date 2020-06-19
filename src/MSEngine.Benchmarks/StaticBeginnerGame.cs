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
                new Node(0, false, 0, NodeState.Hidden),
                new Node(1, false, 0, NodeState.Hidden),
                new Node(2, false, 1, NodeState.Hidden),
                new Node(3, true, 0, NodeState.Hidden),
                new Node(4, false, 1, NodeState.Hidden),
                new Node(5, false, 0, NodeState.Hidden),
                new Node(6, false, 0, NodeState.Hidden),
                new Node(7, false, 0, NodeState.Hidden),
                new Node(8, false, 0, NodeState.Hidden),
                new Node(9, false, 0, NodeState.Hidden),
                new Node(10, false, 1, NodeState.Hidden),
                new Node(11, false, 1, NodeState.Hidden),
                new Node(12, false, 1, NodeState.Hidden),
                new Node(13, false, 1, NodeState.Hidden),
                new Node(14, false, 1, NodeState.Hidden),
                new Node(15, false, 1, NodeState.Hidden),
                new Node(16, false, 1, NodeState.Hidden),
                new Node(17, false, 1, NodeState.Hidden),
                new Node(18, false, 0, NodeState.Hidden),
                new Node(19, false, 0, NodeState.Hidden),
                new Node(20, false, 0, NodeState.Hidden),
                new Node(21, false, 1, NodeState.Hidden),
                new Node(22, true, 0, NodeState.Hidden),
                new Node(23, false, 1, NodeState.Hidden),
                new Node(24, true, 1, NodeState.Hidden),
                new Node(25, false, 2, NodeState.Hidden),
                new Node(26, false, 1, NodeState.Hidden),
                new Node(27, false, 1, NodeState.Hidden),
                new Node(28, false, 1, NodeState.Hidden),
                new Node(29, false, 1, NodeState.Hidden),
                new Node(30, false, 2, NodeState.Hidden),
                new Node(31, false, 2, NodeState.Hidden),
                new Node(32, true, 1, NodeState.Hidden),
                new Node(33, false, 2, NodeState.Hidden),
                new Node(34, false, 1, NodeState.Hidden),
                new Node(35, true, 0, NodeState.Hidden),
                new Node(36, false, 1, NodeState.Hidden),
                new Node(37, false, 1, NodeState.Hidden),
                new Node(38, false, 2, NodeState.Hidden),
                new Node(39, true, 1, NodeState.Hidden),
                new Node(40, false, 2, NodeState.Hidden),
                new Node(41, false, 2, NodeState.Hidden),
                new Node(42, false, 1, NodeState.Hidden),
                new Node(43, false, 1, NodeState.Hidden),
                new Node(44, false, 1, NodeState.Hidden),
                new Node(45, false, 1, NodeState.Hidden),
                new Node(46, true, 1, NodeState.Hidden),
                new Node(47, false, 2, NodeState.Hidden),
                new Node(48, true, 1, NodeState.Hidden),
                new Node(49, false, 2, NodeState.Hidden),
                new Node(50, false, 1, NodeState.Hidden),
                new Node(51, false, 0, NodeState.Hidden),
                new Node(52, false, 0, NodeState.Hidden),
                new Node(53, false, 2, NodeState.Hidden),
                new Node(54, false, 2, NodeState.Hidden),
                new Node(55, false, 2, NodeState.Hidden),
                new Node(56, false, 2, NodeState.Hidden),
                new Node(57, true, 1, NodeState.Hidden),
                new Node(58, false, 1, NodeState.Hidden),
                new Node(59, false, 0, NodeState.Hidden),
                new Node(60, false, 0, NodeState.Hidden),
                new Node(61, false, 1, NodeState.Hidden),
                new Node(62, true, 0, NodeState.Hidden),
                new Node(63, false, 1, NodeState.Hidden)
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

            foreach (var x in turns)
            {
                Engine.Instance.ComputeBoard(matrix, x);
            }

            Debug.Assert(matrix.Nodes.Status() == BoardStatus.Completed);
        }
    }
}

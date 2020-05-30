using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MSEngine.Core;
using MSEngine.Solver;

namespace MSEngine.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BoardGenTests>();
        }
    }

    [MemoryDiagnoser]
    public class BoardGenTests
    {
        //[Benchmark]
        public void LenTestSlow()
        {
            var n = 0;
            Span<int> mobs = stackalloc int[480];
            for (var i = 0; i < mobs.Length; i++)
            {
                n++;
            }
            if (n > 480) { throw new Exception(); }
        }

        //[Benchmark]
        public void LenTestFast()
        {
            var n = 0;
            Span<int> mobs = stackalloc int[480];
            for (int i = 0, l = mobs.Length; i < l; i++)
            {
                n++;
            }
            if (n > 480) { throw new Exception(); }
        }

        // 18.17 ns
        //[Benchmark]
        public void Bar()
        {
            Span<int> foo = stackalloc int[8];
            foo.FillAdjacentNodeIndexes(64, 9, 3);
        }

        [Benchmark]
        public void Exec()
        {
            Play();
        }

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => { });
        }

        private static void Play()
        {
            const int nodeCount = 8 * 8;
            const int columnCount = 8;
            //const int nodeCount = 30 * 16;
            //const int columnCount = 30;
            const int firstTurnNodeIndex = nodeCount / 2;

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];
            var matrix = new Matrix<Node>(nodes, columnCount);

            var iteration = 0;

            while (true)
            {
                if (iteration == 0)
                {
                    Engine.Instance.FillBeginnerBoard(nodes);
                    var turn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

                    // Technically, computing the board *before* the check is redundant here, since we can just
                    // inspect the node directly. We do this to maintain strict separation of clients
                    // We could place this ComputeBoard method after the node inspection for perf
                    BoardStateMachine.Instance.ComputeBoard(matrix, turn);

                    var node = nodes[turn.NodeIndex];
                    if (node.HasMine || node.MineCount > 0)
                    {
                        continue;
                    }
                }
                else
                {
                    var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false);
                    if (turnCount == 0)
                    {
                        turnCount = MatrixSolver.CalculateTurns(matrix, turns, true);
                    }
                    foreach (var turn in turns.Slice(0, turnCount))
                    {
                        BoardStateMachine.Instance.ComputeBoard(matrix, turn);
                    }
                    if (turnCount == 0)
                    {
                        var turn = NodeStrategies.RevealFirstHiddenNode(nodes);
                        BoardStateMachine.Instance.ComputeBoard(matrix, turn);
                    }
                }
                iteration++;

                var status = nodes.Status();
                if (status == BoardStatus.Pending)
                {
                    continue;
                }
                break;
            }
        }
    }
}
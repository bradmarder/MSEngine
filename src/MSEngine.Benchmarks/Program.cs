using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            BenchmarkRunner.Run<RunSimulation>();
        }
    }

    [MemoryDiagnoser]
    public class RunSimulation
    {
        [Benchmark]
        public void Exec() => Play();

        private static void Play()
        {
            const int nodeCount = 8 * 8;const int columnCount = 8; const int firstTurnNodeIndex = 18; // 2:2 for beginner/int
            //const int nodeCount = 30 * 16;const int columnCount = 30; const int firstTurnNodeIndex = 93; // 3:3 for expert

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
                    Engine.Instance.ComputeBoard(matrix, turn);

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
                        if (turnCount == 0) { break; }
                    }
                    foreach (var turn in turns.Slice(0, turnCount))
                    {
                        Engine.Instance.ComputeBoard(matrix, turn);
                    }
                }
                iteration++;
            }
        }
    }
}
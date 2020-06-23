using System;
using System.Diagnostics;
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
            BenchmarkRunner.Run<StaticBeginnerGame>();
        }
    }

    [MemoryDiagnoser]
    public class RandomSimulation
    {
        [Benchmark]
        public void Master()
        {
            const Difficulty difficulty = Difficulty.Beginner; const int nodeCount = 9 * 9; const int columnCount = 9; const int firstTurnNodeIndex = 20; // 2:2 for beginner/int
            //const int nodeCount = 30 * 16;const int columnCount = 30; const int firstTurnNodeIndex = 93; // 3:3 for expert

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];
            var matrix = new Matrix<Node>(nodes, columnCount);

            Beginner(matrix, turns, firstTurnNodeIndex, difficulty);
        }

        public void Beginner(Matrix<Node> matrix, Span<Turn> turns, int firstTurnNodeIndex, Difficulty difficulty)
        {
            bool isFirstIteration = true;

            while (true)
            {
                if (isFirstIteration)
                {
                    switch (difficulty)
                    {
                        case Difficulty.Beginner:
                            Engine.FillBeginnerBoard(matrix.Nodes);
                            break;
                        case Difficulty.Intermediate:
                            Engine.FillIntermediateBoard(matrix.Nodes);
                            break;
                        case Difficulty.Expert:
                            Engine.FillExpertBoard(matrix.Nodes);
                            break;
                        default: throw new NotImplementedException();
                    }

                    var turn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

                    // Technically, computing the board *before* the check is redundant here, since we can just
                    // inspect the node directly. We do this to maintain strict separation of clients
                    // We could place this ComputeBoard method after the node inspection for perf
                    Engine.ComputeBoard(matrix, turn);

                    var node = matrix.Nodes[turn.NodeIndex];
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
                        Engine.ComputeBoard(matrix, turn);
                    }
                }
                isFirstIteration = false;
            }
        }
    }
}
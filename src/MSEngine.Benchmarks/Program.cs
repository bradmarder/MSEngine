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
            BenchmarkRunner.Run<RandomSimulation>();
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

            Simulation(matrix, turns, firstTurnNodeIndex, difficulty);
        }

        public static void Simulation(Matrix<Node> matrix, Span<Turn> turns, int firstTurnNodeIndex, Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Beginner:
                    Engine.FillBeginnerBoard(matrix.Nodes, firstTurnNodeIndex);
                    break;
                case Difficulty.Intermediate:
                    Engine.FillIntermediateBoard(matrix.Nodes, firstTurnNodeIndex);
                    break;
                case Difficulty.Expert:
                    Engine.FillExpertBoard(matrix.Nodes, firstTurnNodeIndex);
                    break;
                default: throw new NotImplementedException();
            }

            var firstTurn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);
            Engine.ComputeBoard(matrix, firstTurn);

            while (true)
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
        }
    }
}
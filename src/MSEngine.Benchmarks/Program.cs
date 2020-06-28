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
        public void Beginner() => PrepareMatrix(Difficulty.Beginner);

        [Benchmark]
        public void Intermediate() => PrepareMatrix(Difficulty.Intermediate);

        [Benchmark]
        public void Expert() => PrepareMatrix(Difficulty.Expert);

        public static void PrepareMatrix(Difficulty difficulty)
        {
            var nodeCount = difficulty switch
            {
                Difficulty.Beginner => 81,
                Difficulty.Intermediate => 16 * 16,
                Difficulty.Expert => 30 * 16,
                _ => throw new NotImplementedException()
            };
            var columnCount = difficulty switch
            {
                Difficulty.Beginner => 9,
                Difficulty.Intermediate => 16,
                Difficulty.Expert => 30,
                _ => throw new NotImplementedException()
            };
            var firstTurnNodeIndex = difficulty switch
            {
                Difficulty.Beginner => 20,
                Difficulty.Intermediate => 49,
                Difficulty.Expert => 93,
                _ => throw new NotImplementedException()
            };

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
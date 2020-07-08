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
            var mineCount = difficulty switch
            {
                Difficulty.Beginner => 10,
                Difficulty.Intermediate => 40,
                Difficulty.Expert => 99,
                _ => throw new NotImplementedException()
            };

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];
            var matrix = new Matrix<Node>(nodes, columnCount);

            Simulation(matrix, turns, firstTurnNodeIndex, mineCount);
        }

        public static void Simulation(Matrix<Node> matrix, Span<Turn> turns, int firstTurnNodeIndex, int mineCount)
        {
            Span<int> mines = stackalloc int[mineCount];
            Span<int> visitedIndexes = stackalloc int[matrix.Nodes.Length];
            Span<int> revealedMineCountNodeIndexes = stackalloc int[matrix.Nodes.Length - mineCount];
            Span<int> adjacentHiddenNodeIndexes = stackalloc int[matrix.Nodes.Length];
            Span<float> grid = stackalloc float[revealedMineCountNodeIndexes.Length * adjacentHiddenNodeIndexes.Length];

            Engine.FillCustomBoard(matrix, mines, firstTurnNodeIndex);

            var firstTurn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);
            Engine.ComputeBoard(matrix, firstTurn, visitedIndexes);

            while (true)
            {
                var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false, revealedMineCountNodeIndexes, adjacentHiddenNodeIndexes, grid);
                if (turnCount == 0)
                {
                    turnCount = MatrixSolver.CalculateTurns(matrix, turns, true, revealedMineCountNodeIndexes, adjacentHiddenNodeIndexes, grid);
                    if (turnCount == 0) { break; }
                }
                foreach (var turn in turns.Slice(0, turnCount))
                {
                    Engine.ComputeBoard(matrix, turn, visitedIndexes);
                }
            }
        }
    }
}
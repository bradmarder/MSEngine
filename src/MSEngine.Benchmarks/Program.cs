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
        // 18.17 ns
        //[Benchmark]
        //public void Bar()
        //{
        //    Span<int> foo = stackalloc int[8];
        //    foo.FillAdjacentNodeIndexes(64, 9, 3);
        //}

        [Benchmark]
        public void Exec() => ExecuteGame();

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => ExecuteGame());
        }

        private static void ExecuteGame()
        {
            const int nodeCount = 64;
            const int columnCount = 8;

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[0];
            Engine.Instance.FillBeginnerBoard(nodes);

            var turnCount = 0;

            while (true)
            {
                if (turnCount == 0)
                {
                    turns = stackalloc Turn[1]
                    {
                        new Turn(27, NodeOperation.Reveal)
                    };
                }
                if (turns.Length == 0)
                {
                    turns = stackalloc Turn[nodes.Length];
                    MatrixSolver.CalculateTurns(nodes, ref turns);
                }

                // if the matrix solver couldn't calculate any turns, we just select a "random" hidden node
                if (turns.Length == 0)
                {
                    turns = stackalloc Turn[1]
                    {
                        EducatedGuessStrategy.UseStrategy(nodes)
                    };
                }

                var turn = turns[0];
                turns = turns.Slice(1, turns.Length - 1);
                //if (turnCount > 0)
                //{
                //    BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
                //}
                BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, turn);

                // Get new board unless node has no mine and zero AMC
                var status = nodes.Status();
                if (turnCount == 0 && (nodes[turn.NodeIndex].MineCount > 0 || status == BoardStatus.Failed))
                {
                    // nodes.Clear(); not required since every node is always reset
                    Engine.Instance.FillBeginnerBoard(nodes);
                    turns = Span<Turn>.Empty;
                    continue;
                }
                turnCount++;

                if (status == BoardStatus.Pending)
                {
                    continue;
                }

                break;
            }
        }
    }
}
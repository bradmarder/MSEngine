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
            BenchmarkRunner.Run<Test>();
        }
    }

    [MemoryDiagnoser]
    public class Test
    {
        private static readonly int _defaultInt = default;
        private static readonly Board PseudoRandomExpertBoard = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();
        private static readonly Board PseudoRandomExpertBoard2 = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();

        [Benchmark]
        public void Foo()
        {
            Enumerable.Range(0, 1000).Select(x => _defaultInt).ToList();
        }

        [Benchmark]
        public void Bar()
        {
            Enumerable.Range(0, 1000).Select(x => default(int)).ToList();
        }

        //[Benchmark]
        //public void SolveExpertBoard()
        //{
        //    var board = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();

        //    while (board.Status == BoardStatus.Pending)
        //    {
        //        var (turn, strategy) = EliteSolver.Instance.ComputeTurn(board);
        //        board = BoardStateMachine.Instance.ComputeBoard(board, turn);
        //    }
        //}

        //[Benchmark]
        //public void SolveTestExpertBoard()
        //{
        //    var board = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();

        //    while (board.Status == BoardStatus.Pending)
        //    {
        //        var (turn, strategy) = EliteSolver.Instance.ComputeTurn(board);
        //        board = BoardStateMachine.Instance.ComputeBoard(board, turn);
        //    }
        //}
    }
}

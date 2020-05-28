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
        [Benchmark]
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

        [Benchmark]
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
        //public void Bar()
        //{
        //    Span<int> foo = stackalloc int[8];
        //    foo.FillAdjacentNodeIndexes(64, 9, 3);
        //}

        //[Benchmark]
        //public void Exec() => ExecuteGame();

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => { });
        }
    }
}
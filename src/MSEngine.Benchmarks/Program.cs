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
        // 57ns
        //[Benchmark]
        public void Foo()
        {
            Span<Coordinates> foo = stackalloc Coordinates[64];
            const int columns = 8;
            const int rows = 8;
            for (byte x = 0; x < columns; x++)
            {
                for (byte y = 0; y < rows; y++)
                {
                    foo[y * columns + x] = new Coordinates(x, y);
                }
            }
        }

        // 18.17 ns
        [Benchmark]
        public void Bar()
        {
            Span<int> foo = stackalloc int[8];
            Utilities.FillAdjacentTileIndexes(64, 9, 3, foo);
        }

        //[Benchmark]
        //public void Fill()
        //{
        //    Span<Tile> tiles = stackalloc Tile[64];
        //    Engine.Instance.FillBeginnerBoard(tiles);
        //}

        //[Benchmark]
        //public void Intermediate()
        //{
        //    Engine.Instance.GenerateIntermediateBoard();
        //}

        ////[Benchmark]
        //public void Expert()
        //{
        //    Engine.Instance.GenerateExpertBoard();
        //}

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => { });
        }

        //private static void RunRandomDistributionTest(Func<Board> boardGenerator, int maxIterationCount = int.MaxValue)
        //{
        //    var iteration = 0;
        //    var board = boardGenerator();
        //    var expectedAverage = board.MineCount / (decimal)(board.Width * board.Height);
        //    var map = board.Tiles.ToDictionary(x => x.Coordinates, _ => 0);

        //    while (iteration < maxIterationCount)
        //    {
        //        iteration++;

        //        boardGenerator()
        //            .Tiles
        //            .Where(x => x.HasMine)
        //            .ToList()
        //            .ForEach(x => map[x.Coordinates]++);

        //        var means = map
        //            .Select(y => y.Value / (decimal)iteration)
        //            .ToArray();
        //        var min = means.Min();
        //        var max = means.Max();
        //        var minDiff = Math.Abs(expectedAverage - min); //.00369639666
        //        var maxDiff = Math.Abs(expectedAverage - max); //.00333032896

        //        Console.SetCursorPosition(0, Console.CursorTop);
        //        Console.Write($"MinDiff = {minDiff} and MaxDiff = {maxDiff}");

        //        // beginner
        //        // MinDiff = 0.0008879570668942427624236854 and MaxDiff = 0.0007066073655878684435107989
        //        // MinDiff = 0.0003602253545151916915500224 and MaxDiff = 0.0004654803596709192191884712
        //    }

        //    Console.ReadLine();
        //}
    }
}
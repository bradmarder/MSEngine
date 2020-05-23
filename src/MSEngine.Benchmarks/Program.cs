using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            Utilities.FillAdjacentTileIndexes(64, 9, foo);
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

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ =>
                {
                    
                });
        }
    }

    [MemoryDiagnoser]
    public class HashTest
    {
        [Benchmark]
        public void Control()
        {
            const string input = "lol test ";
            HashTest.CreateMD5(input);
        }

        [Benchmark]
        public void Span()
        {
            const string input = "lol test ";
            HashTest.SpanCreateMD5(input);
        }

        public static string CreateMD5(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string SpanCreateMD5(ReadOnlySpan<char> input)
        {
            var encoding = System.Text.Encoding.UTF8;
            var inputByteCount = encoding.GetByteCount(input);

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                Span<byte> bytes = stackalloc byte[inputByteCount];
                Span<byte> destination = stackalloc byte[md5.HashSize / 8];

                encoding.GetBytes(input, bytes);

                // checking the result is not required because this only returns false if "(destination.Length < HashSizeValue/8)", which is never true in this case
                md5.TryComputeHash(bytes, destination, out int _bytesWritten);
                
                // cleanup???
                return BitConverter.ToString(destination.ToArray());
            }
        }
    }

    [MemoryDiagnoser]
    public class Test
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        [Benchmark]
        public void Heap()
        {
            var bytes = new byte[16];
            _rng.GetBytes(bytes);
        }

        [Benchmark]
        public void Fill()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);
        }
    }
}
//private static readonly Board PseudoRandomExpertBoard = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();
//private static readonly Board PseudoRandomExpertBoard2 = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();

//[Benchmark]
//public void Control()
//{
//    var board = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();

//    while (board.Status == BoardStatus.Pending)
//    {
//        var (turn, strategy) = EliteSolver.Instance.ComputeTurn(board);
//        board = BoardStateMachine.Instance.ComputeBoard(board, turn);
//    }
//}

//[Benchmark]
//public void Exp()
//{
//    var board = Core.Engine.PseudoRandomInstance.GenerateExpertBoard();

//    while (board.Status == BoardStatus.Pending)
//    {
//        var (turn, strategy) = EliteSolver.Instance.ComputeTurn(board);
//        board = BoardStateMachine.Instance.ComputeBoard(board, turn);
//    }
//}

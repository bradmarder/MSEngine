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
        [Benchmark]
        public void Beginner()
        {
            Span<Tile> tiles = stackalloc Tile[64];
            Engine.Instance.GenerateBeginnerBoard(tiles);
        }

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

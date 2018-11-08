using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MSEngine.Core;

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
        [Benchmark]
        public Board GeneratePureBoard() => Engine.GeneratePureBoard(30, 16, 99);

        [Benchmark]
        public Board GenerateRandomBoard() => Engine.GenerateRandomExpertBoard();
    }
}

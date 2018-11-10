using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
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
        public void GenerateRandomExpertBoard() => MSEngine.Core.Engine.GetCoordinates(30, 16).Consume(new Consumer());
    }
}

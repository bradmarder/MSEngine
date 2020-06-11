using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using MSEngine.Core;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// |                  Method |     Mean |    Error |   StdDev |
    /// |------------------------ |---------:|---------:|---------:|
    /// | OldGetAdjacentMineCount | 63.27 ns | 0.925 ns | 0.865 ns |
    /// | NewGetAdjacentMineCount | 60.82 ns | 1.459 ns | 1.293 ns |
    /// </summary>
    public class GetMineCount
    {
        [Benchmark]
        public byte OldGetAdjacentMineCount()
        {
            Span<int> mineIndexes = stackalloc int[10];
            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];
            buffer.FillAdjacentNodeIndexes(64, 30, 8);

            byte n = 0;
            foreach (var i in buffer)
            {
                if (Utilities.Contains(mineIndexes, i))
                {
                    n++;
                }
            }
            return n;
        }

        [Benchmark]
        public byte NewGetAdjacentMineCount()
        {
            Span<int> mineIndexes = stackalloc int[10];
            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];
            buffer.FillAdjacentNodeIndexes(64, 30, 8);

            byte n = 0;
            foreach (var i in buffer)
            {
                if (i != -1 && Utilities.Contains(mineIndexes, i))
                {
                    n++;
                }
            }
            return n;
        }
    }
}

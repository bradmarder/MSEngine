using BenchmarkDotNet.Attributes;
using System;

namespace MSEngine.Benchmarks
{
    [MemoryDiagnoser]
    public class IndexOfVsContainsVSForeachManual
    {
        [Benchmark]
        public bool ForeachManual()
        {
            Span<int> buffer = stackalloc int[8];

            foreach (var i in buffer)
            {
                if (i == 5) { return true; }
            }

            return false;
        }

        [Benchmark]
        public bool IndexOf()
        {
            Span<int> buffer = stackalloc int[8];

            return buffer.IndexOf(5) != -1;
        }

        [Benchmark]
        public bool Contains()
        {
            Span<int> buffer = stackalloc int[8];

            return buffer.Contains(5);
        }
    }
}

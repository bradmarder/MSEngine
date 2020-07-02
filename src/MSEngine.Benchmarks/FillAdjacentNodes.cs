using BenchmarkDotNet.Attributes;
using System;

namespace MSEngine.Benchmarks
{
    /// <summary>
    /// | Method |     Mean |    Error |   StdDev |
    /// |------- |---------:|---------:|---------:|
    /// |    Old | 23.09 ns | 0.263 ns | 0.246 ns |
    /// |    New | 43.34 ns | 0.310 ns | 0.290 ns |
    /// </summary>
    public class FillAdjacentNodes
    {
        [Benchmark]
        public void Old()
        {
            Span<int> foo = stackalloc int[8];
            OldFillAdjacentNodeIndexes(foo, 9, 4, 3);
        }

        [Benchmark]
        public void New()
        {
            Span<int> foo = stackalloc int[8];
            NewFillAdjacentNodeIndexes(foo, 9, 4, 3);
        }

        [Benchmark]
        public void NoFill()
        {
            Span<int> foo = stackalloc int[8];
            NoFill(foo, 9, 4, 3);
        }

        public static void NewFillAdjacentNodeIndexes(Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = (index + 1) % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            indexes.Fill(-1);

            var enumerator = indexes.GetEnumerator();
            enumerator.MoveNext();

            if (!isTop)
            {
                var val = index - columnCount;
                if (!isLeftSide)
                {
                    enumerator.Current = val - 1;
                    enumerator.MoveNext();
                }
                enumerator.Current = val;
                enumerator.MoveNext();
                if (!isRightSide)
                {
                    enumerator.Current = val + 1;
                    enumerator.MoveNext();
                }
            }
            if (!isLeftSide)
            {
                enumerator.Current = index - 1;
                enumerator.MoveNext();
            }
            if (!isRightSide)
            {
                enumerator.Current = index + 1;
                enumerator.MoveNext();
            }
            if (!isBottom)
            {
                var val = index + columnCount;
                if (!isLeftSide)
                {
                    enumerator.Current = val - 1;
                    enumerator.MoveNext();
                }
                enumerator.Current = val;
                enumerator.MoveNext();
                if (!isRightSide)
                {
                    enumerator.Current = val + 1;
                }
            }
        }

        public static void OldFillAdjacentNodeIndexes(Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = (index + 1) % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            indexes.Fill(-1);

            if (!isTop)
            {
                var val = index - columnCount;
                if (!isLeftSide)
                {
                    indexes[0] = val - 1;
                }
                indexes[1] = val;
                if (!isRightSide)
                {
                    indexes[2] = val + 1;
                }
            }
            if (!isLeftSide)
            {
                indexes[3] = index - 1;
            }
            if (!isRightSide)
            {
                indexes[4] = index + 1;
            }
            if (!isBottom)
            {
                var val = index + columnCount;
                if (!isLeftSide)
                {
                    indexes[5] = val - 1;
                }
                indexes[6] = val;
                if (!isRightSide)
                {
                    indexes[7] = val + 1;
                }
            }
        }

        public static void NoFill(Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = (index + 1) % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            if (isTop)
            {
                indexes[0] = indexes[1] = indexes[2] = -1;
            }
            else
            {
                var val = index - columnCount;
                if (!isLeftSide)
                {
                    indexes[0] = val - 1;
                }
                indexes[1] = val;
                if (!isRightSide)
                {
                    indexes[2] = val + 1;
                }
            }

            if (isLeftSide)
            {
                indexes[0] = indexes[3] = indexes[5] = -1;
            }
            else
            {
                indexes[3] = index - 1;
            }

            if (isRightSide)
            {
                indexes[2] = indexes[4] = indexes[7] = -1;
            }
            else
            {
                indexes[4] = index + 1;
            }

            if (isBottom)
            {
                indexes[5] = indexes[6] = indexes[7] = -1;
            }
            else
            {
                var val = index + columnCount;
                if (!isLeftSide)
                {
                    indexes[5] = val - 1;
                }
                indexes[6] = val;
                if (!isRightSide)
                {
                    indexes[7] = val + 1;
                }
            }
        }
    }
}

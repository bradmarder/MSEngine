using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;

namespace MSEngine.Benchmarks
{
    public class IndexFillvsSpanFill
    {
        [Benchmark]
        public void Old()
        {
            Span<int> foo = stackalloc int[8];
            OLDFillAdjacentNodeIndexes(foo, 9, 4, 3);
        }

        [Benchmark]
        public void New()
        {
            Span<int> foo = stackalloc int[8];
            NEWFillAdjacentNodeIndexes(foo, 9, 4, 3);
        }

        public static void OLDFillAdjacentNodeIndexes(Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            // ~10% optimization: help compiler assume it doesn't need to to check out-of-bounds on accessing indexes
            _ = indexes[Engine.MaxNodeEdges - 1];

            var indexPlusOne = index + 1;
            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = indexPlusOne % columnCount == 0;
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
                indexes[4] = indexPlusOne;
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

        public static void NEWFillAdjacentNodeIndexes(Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            // ~10% optimization: help compiler assume it doesn't need to to check out-of-bounds on accessing indexes
            _ = indexes[Engine.MaxNodeEdges - 1];

            var indexPlusOne = index + 1;
            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = indexPlusOne % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            if (isTop)
            {
                //indexes[0] = indexes[1] = indexes[2] = -1;
                indexes.Slice(0, 3).Fill(-1);
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
                indexes[4] = indexPlusOne;
            }

            if (isBottom)
            {
                //indexes[5] = indexes[6] = indexes[7] = -1;
                indexes.Slice(5, 3).Fill(-1);
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

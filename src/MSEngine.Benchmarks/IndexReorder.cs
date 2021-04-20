using BenchmarkDotNet.Attributes;
using MSEngine.Core;
using System;
using System.Linq;

namespace MSEngine.Benchmarks
{
    public class IndexReorder
    {
        [Benchmark]
        public void Old()
        {
            Span<int> foo = stackalloc int[8];
            foreach (var x in Enumerable.Range(0, 64))
            {
                OLDFillAdjacentNodeIndexes(foo, 9, x, 3);
            }
        }

        [Benchmark]
        public void New()
        {
            Span<int> foo = stackalloc int[8];
            foreach(var x in Enumerable.Range(0, 64))
            {
                NEWFillAdjacentNodeIndexes(foo, 9, x, 3);
            }
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
            var notTop = index >= columnCount;
            var notLeftSide = index % columnCount > 0;
            var notRightSide = indexPlusOne % columnCount > 0;
            var notBottom = index < nodeCount - columnCount;

            if (notTop)
            {
                var val = index - columnCount;
                if (notLeftSide)
                {
                    indexes[0] = val - 1;
                }
                indexes[1] = val;
                if (notRightSide)
                {
                    indexes[2] = val + 1;
                }
            }
            else
            {
                indexes[0] = indexes[1] = indexes[2] = -1;
            }

            if (notLeftSide)
            {
                indexes[3] = index - 1;
                
            }
            else
            {
                indexes[0] = indexes[3] = indexes[5] = -1;
            }

            if (notRightSide)
            {
                indexes[4] = indexPlusOne;
                
            }
            else
            {
                indexes[2] = indexes[4] = indexes[7] = -1;
            }

            if (notBottom)
            {
                
                var val = index + columnCount;
                if (notLeftSide)
                {
                    indexes[5] = val - 1;
                }
                indexes[6] = val;
                if (notRightSide)
                {
                    indexes[7] = val + 1;
                }
            }
            else
            {
                indexes[5] = indexes[6] = indexes[7] = -1;
            }
        }
    }
}

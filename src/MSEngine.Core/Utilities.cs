using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        public static bool IsAdjacentTo(Span<int> buffer, int nodeCount, int columnCount, int i1, int i2)
        {
            buffer.FillAdjacentNodeIndexes(nodeCount, i1, columnCount);

            foreach (var i in buffer)
            {
                if (i == i2) { return true; }
            }

            return false;
        }

        // cache? 18ns or whatever is already fast...
        public static void FillAdjacentNodeIndexes(this Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(index >= 0);
            Debug.Assert(index < nodeCount);
            Debug.Assert(columnCount > 0);
            Debug.Assert(indexes.Length == 8);

            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = (index + 1) % columnCount == 0;
            var isBottom = index >= nodeCount - columnCount;

            indexes.Fill(-1);

            // ignore indexes 0/1/2 if isTop is true
            // ignore indexes 0/3/5 isLeftSide is true
            // ignore indexes 2/4/7 isRightSide is true
            // ignore indexes 5/6/7 if isBottom is true

            if (!isTop)
            {
                if (!isLeftSide)
                {
                    indexes[0] = index - columnCount - 1;
                }
                indexes[1] = index - columnCount;
                if (!isRightSide)
                {
                    indexes[2] = index - columnCount + 1;
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
                if (!isLeftSide)
                {
                    indexes[5] = index + columnCount - 1;
                }
                indexes[6] = index + columnCount;
                if (!isRightSide)
                {
                    indexes[7] = index + columnCount + 1;
                }
            }
        }

        internal static void Scatter(this Span<int> mines, int nodeCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount > mines.Length);

            // we must fill the span with -1 because the default (0) is a valid index
            mines.Fill(-1);

            // experiment? just fill with bytes, then um, convert to INT indexes?
            // the "do while" loop is just too slow...
            //RandomNumberGenerator.Fill(mines);

            for (int i = 0, l = mines.Length; i < l; i++)
            {
                int m;

                // we use a loop to prevent duplicate indexes
                do
                {
                    m = RandomNumberGenerator.GetInt32(nodeCount);
                } while (mines.IndexOf(m) != -1);

                mines[i] = m;
            }
        }

        internal static void PseudoScatter(this Span<int> mines, int nodeCount)
        {
            Debug.Assert(nodeCount > 0);
            Debug.Assert(nodeCount > mines.Length);

            // magic seed that produces a solvable beginner board
            var random = new Random(653635);

            mines.Fill(-1);

            for (int i = 0, l = mines.Length; i < l; i++)
            {
                int m;

                // we use a loop to prevent duplicate indexes
                do
                {
                    m = random.Next(nodeCount);
                } while (mines.IndexOf(m) != -1);

                mines[i] = m;
            }
        }
    }
}
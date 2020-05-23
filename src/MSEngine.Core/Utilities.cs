using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAdjacentTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x1 = coordinateOne.X;
            var y1 = coordinateOne.Y;
            var x2 = coordinateTwo.X;
            var y2 = coordinateTwo.Y;

            // PERF: Prioritize filtering by x coordinate because expert boards have more columns than rows
            return x2 > (x1 - 2)
                && x2 < (x1 + 2)
                && y2 > (y1 - 2)
                && y2 < (y1 + 2)
                && coordinateOne != coordinateTwo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNextTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x1 = coordinateOne.X;
            var y1 = coordinateOne.Y;
            var x2 = coordinateTwo.X;
            var y2 = coordinateTwo.Y;

            // PERF: Prioritize filtering by x coordinate because expert boards have more columns than rows
            return (x1 == x2 && (y2 == (y1 + 1) || y2 == (y1 - 1)))
                || (y1 == y2 && (x2 == (x1 + 1) || x2 == (x1 - 1)));
        }

        // cache? 18ns or whatever is already fast...
        public static void FillAdjacentTileIndexes(int tileCount, int index, int columnCount, Span<int> indexes)
        {
            Debug.Assert(tileCount > 0);
            Debug.Assert(index >= 0);
            Debug.Assert(index < tileCount);
            Debug.Assert(columnCount > 0);
            Debug.Assert(indexes.Length == 8);

            var isTop = index < columnCount;
            var isLeftSide = index % columnCount == 0;
            var isRightSide = (index + 1) % columnCount == 0;
            var isBottom = index >= tileCount - columnCount;

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

        internal static void ShuffleItems<T>(this Span<T> items)
        {
            var n = items.Length;

            while (n > 1)
            {
                n--;
                var k = RandomNumberGenerator.GetInt32(n + 1);
                var value = items[k];
                items[k] = items[n];
                items[n] = value;
            }
        }

        /// <summary>
        /// Intended for testing/benchmarking a deterministic board that requires zero guesses to solve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static void PseudoShuffleItems<T>(this Span<T> items)
        {
            var n = items.Length;

            // magic seed that produces a solvable beginner board
            var random = new Random(653635);

            while (n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                var value = items[k];
                items[k] = items[n];
                items[n] = value;
            }
        }

        /// <summary>
        /// scatter approach with mine indexes is 5x slower than scattering tiles
        /// </summary>
        internal static void Scatter(this Span<int> mines, int tileCount, int mineCount)
        {
            Debug.Assert(mines.Length == mineCount);
            Debug.Assert(tileCount > 0);
            Debug.Assert(mineCount >= 0);
            Debug.Assert(tileCount > mineCount);

            // we must fill the span with -1 because the default (0) is a valid index
            mines.Fill(-1);

            // experiment? just fill with bytes, then um, convert to INT indexes?
            // the "do while" loop is just too slow...
            //RandomNumberGenerator.Fill(mines);
            

            for (var i = 0; i < mineCount; i++)
            {
                int m;

                // we use a loop to prevent duplicate indexes
                do
                {
                    m = RandomNumberGenerator.GetInt32(tileCount);
                } while (mines.IndexOf(m) != -1);

                mines[i] = m;
            }
        }

        internal static void PseudoScatter(this Span<int> mines, int tileCount, int mineCount)
        {
            Debug.Assert(mines.Length == mineCount);
            Debug.Assert(tileCount > 0);
            Debug.Assert(mineCount >= 0);
            Debug.Assert(tileCount > mineCount);

            // magic seed that produces a solvable beginner board
            var random = new Random(653635);

            mines.Fill(-1);

            for (var i = 0; i < mineCount; i++)
            {
                int m;

                // we use a loop to prevent duplicate indexes
                do
                {
                    m = random.Next(tileCount);
                } while (mines.IndexOf(m) != -1);

                mines[i] = m;
            }
        }
    }
}
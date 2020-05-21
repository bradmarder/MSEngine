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

            // magic seed that produces a solvable board
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
            // Span<int> mineIndexes = stackalloc int[mineCount];

            // we must fill the span with -1 because the default (0) is a valid index
            mines.Fill(-1);

            // experiment? just fill with bytes, then um, convert to INT indexes?
            // the "do while" loop is just too slow...
            //RandomNumberGenerator.Fill(mines);

            Debug.Assert(mines.Length == mineCount);
            Debug.Assert(tileCount > 0);
            Debug.Assert(mineCount > 0);
            Debug.Assert(tileCount > mineCount);

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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNextTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            // filters ordered to prioritize short-circuiting
            return (x == coordinateTwo.X && new[] { y + 1, y - 1 }.Contains(coordinateTwo.Y))
                || (y == coordinateTwo.Y && new[] { x + 1, x - 1 }.Contains(coordinateTwo.X));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAdjacentTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            // filters ordered to prioritize short-circuiting
            return new[] { x, x + 1, x - 1 }.Contains(coordinateTwo.X)
                && new[] { y, y + 1, y - 1 }.Contains(coordinateTwo.Y)
                && coordinateOne != coordinateTwo;
        }

        internal static T[] GetShuffledItems<T>(this IEnumerable<T> list)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            var items = list.ToArray();
            var n = items.Length;
            var data = new byte[4];

            using (var provider = new RNGCryptoServiceProvider())
            {
                while (n > 1)
                {
                    n--;
                    provider.GetBytes(data);
                    var seed = BitConverter.ToInt32(data, 0);

                    // we use Random because it has a simple API for getting a random number in a specified range
                    var k = new Random(seed).Next(n + 1);

                    var value = items[k];
                    items[k] = items[n];
                    items[n] = value;
                }
            }

            return items;
        }

        /// <summary>
        /// Intended for testing/benchmarking a deterministic board that requires zero guesses to solve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static T[] GetPseudoShuffledItems<T>(this IEnumerable<T> list)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            var items = list.ToArray();
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

            return items;
        }
    }
}

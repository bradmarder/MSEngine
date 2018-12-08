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

        internal static List<T> GetShuffledItems<T>(this IEnumerable<T> list)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            var items = list.ToList();
            var n = items.Count;

            using (var provider = new RNGCryptoServiceProvider())
            {
                var data = new byte[4];

                while (n > 1)
                {
                    n--;
                    provider.GetBytes(data);
                    var seed = BitConverter.ToInt32(data, 0);
                    var k = new Random(seed).Next(n + 1);
                    var value = items[k];
                    items[k] = items[n];
                    items[n] = value;
                }
            }

            return items;
        }
    }
}

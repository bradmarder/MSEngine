using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    public static class Utilities
    {
        private static readonly ConcurrentDictionary<uint, bool> _adjacentTileMap = new ConcurrentDictionary<uint, bool>();
        private static readonly ConcurrentDictionary<uint, bool> _nextTileMap = new ConcurrentDictionary<uint, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNextTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var key = coordinateOne.X
                | (uint)coordinateOne.Y << 8
                | (uint)coordinateTwo.X << 16
                | (uint)coordinateTwo.Y << 24;

            if (_nextTileMap.TryGetValue(key, out var value))
            {
                return value;
            }

            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            var val = (x == coordinateTwo.X && new[] { y + 1, y - 1 }.Contains(coordinateTwo.Y))
                || (y == coordinateTwo.Y && new[] { x + 1, x - 1 }.Contains(coordinateTwo.X));

            _nextTileMap.TryAdd(key, val);

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAdjacentTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var key = coordinateOne.X
                | (uint)coordinateOne.Y << 8
                | (uint)coordinateTwo.X << 16
                | (uint)coordinateTwo.Y << 24;

            if (_adjacentTileMap.TryGetValue(key, out var value))
            {
                return value;
            }

            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            var val = new[] { x, x + 1, x - 1 }.Contains(coordinateTwo.X)
                && new[] { y, y + 1, y - 1 }.Contains(coordinateTwo.Y)
                && coordinateOne != coordinateTwo;

            _adjacentTileMap.TryAdd(key, val);

            return val;
        }

        internal static T[] GetShuffledItems<T>(this IEnumerable<T> list)
        {
            var items = list.ToArray();
            var n = items.Length;

            while (n > 1)
            {
                n--;
                var k = RandomNumberGenerator.GetInt32(n + 1);
                var value = items[k];
                items[k] = items[n];
                items[n] = value;
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

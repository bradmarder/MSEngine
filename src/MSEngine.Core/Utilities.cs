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
        private static readonly ConcurrentDictionary<uint, Tile> _keyToTileMap = new ConcurrentDictionary<uint, Tile>();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Tile GetTile(Coordinates coordinates, bool hasMine, int adjacentMineCount)
        {
            return GetTile(coordinates, hasMine, adjacentMineCount, TileOperation.RemoveFlag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Tile GetTile(in Tile tile, TileOperation operation)
        {
            return GetTile(tile.Coordinates, tile.HasMine, tile.AdjacentMineCount, operation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Tile GetTile(Coordinates coordinates, bool hasMine, int adjacentMineCount, TileOperation operation)
        {
            var key = (hasMine ? 1 : uint.MinValue) // 1 bit
                | (uint)operation << 1              // 7 bits (only needs 2)
                | (uint)adjacentMineCount << 8      // 8 bits
                | (uint)coordinates.X << 16         // 8 bits
                | (uint)coordinates.Y << 24;        // 8 bits

            if (_keyToTileMap.TryGetValue(key, out var value))
            {
                return value;
            }

            var tile = new Tile(coordinates, hasMine, adjacentMineCount, operation);

            _keyToTileMap.TryAdd(key, tile);

            return tile;
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

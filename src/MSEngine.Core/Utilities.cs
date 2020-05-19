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
        private static readonly ConcurrentDictionary<uint, Tile> _keyToTileMap = new ConcurrentDictionary<uint, Tile>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAdjacentTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x1 = coordinateOne.X;
            var y1 = coordinateOne.Y;
            var x2 = coordinateTwo.X;
            var y2 = coordinateTwo.Y;

            return x2 > (x1 - 2)
                && x2 < (x1 + 2)
                && y2 > (y1 - 2)
                && y2 < (y1 + 2)
                && coordinateOne != coordinateTwo;
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

        internal static void ShuffleItems<T>(ref this Span<T> items)
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
        internal static void PseudoShuffleItems<T>(ref this Span<T> items)
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
    }
}

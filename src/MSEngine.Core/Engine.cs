﻿using System;
using System.Collections.Generic;
using System.Linq;
using static MSEngine.Core.Utilities;

namespace MSEngine.Core
{
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine();

        public Board GenerateRandomBeginnerBoard() => GenerateRandomBoard(8, 8, 10);
        public Board GenerateRandomIntermediateBoard() => GenerateRandomBoard(16, 16, 40);
        public Board GenerateRandomExpertBoard() => GenerateRandomBoard(30, 16, 99);
        public Board GenerateRandomBoard(byte columns, byte rows, byte mineCount) =>
            GenerateBoard(columns, rows, mineCount, Utilities.GetShuffledItems);

        internal static Board GenerateBoard(byte columns, byte rows, byte mineCount, Func<IEnumerable<Coordinates>, IEnumerable<Coordinates>> shuffler)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            if (shuffler == null) { throw new ArgumentNullException(nameof(shuffler)); }

            // if we allowed tileCount == mineCount, then we would have an infinite loop attempting to generate a board
            // because logic dictates the first tile revealed must not be a mine
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }

            var coordinates = GetCoordinates(columns, rows);
            var coordinatesToMineMap = shuffler(coordinates)

                // we use a select expression to get the index, since ToDictionary() does not give access to it
                .Select((x, i) => (Coordinates: x, Index: i))

                .ToDictionary(x => x.Coordinates, x => x.Index < mineCount);
            var coordinatesToAdjacentMineCountMap = coordinates.ToDictionary(
                x => x,
                x => coordinatesToMineMap.Count(y => y.Value && IsAdjacentTo(y.Key, x)));
            var tiles = coordinatesToMineMap.Select(x => new Tile(x.Key, x.Value, coordinatesToAdjacentMineCountMap[x.Key]));

            return new Board(tiles);
        }

        /// <summary>
        /// A pure method which does not randomize mine location. Intended for testing purposes.
        /// </summary>
        internal static Board GeneratePureBoard(byte columns, byte rows, byte mineCount) =>
            GenerateBoard(columns, rows, mineCount, Enumerable.AsEnumerable);

        internal static List<Coordinates> GetCoordinates(byte rows, byte columns)
        {
            if (columns == 0) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0) { throw new ArgumentOutOfRangeException(nameof(rows)); }

            return Enumerable
                .Range(0, rows)
                .SelectMany(x => Enumerable
                    .Range(0, columns)
                    .Select(y => new Coordinates((byte)x, (byte)y)))
                .ToList();
        }
    }
}

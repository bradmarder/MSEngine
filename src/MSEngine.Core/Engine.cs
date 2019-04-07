using System;
using System.Collections.Generic;
using System.Linq;
using static MSEngine.Core.Utilities;

namespace MSEngine.Core
{
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine(Utilities.GetShuffledItems);
        public static IEngine PureInstance { get; } = new Engine(Enumerable.AsEnumerable);
        public static IEngine PseudoRandomInstance { get; } = new Engine(Utilities.GetPseudoShuffledItems);

        private readonly Func<IEnumerable<Coordinates>, IEnumerable<Coordinates>> _shuffler;

        public Engine(Func<IEnumerable<Coordinates>, IEnumerable<Coordinates>> shuffler)
        {
            _shuffler = shuffler ?? throw new ArgumentNullException(nameof(shuffler));
        }

        public virtual Board GenerateBeginnerBoard() => GenerateBoard(8, 8, 10);
        public virtual Board GenerateIntermediateBoard() => GenerateBoard(16, 16, 40);
        public virtual Board GenerateExpertBoard() => GenerateBoard(30, 16, 99);
        public virtual Board GenerateBoard(byte columns, byte rows, byte mineCount)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }

            var coordinates = GetCoordinates(columns, rows);
            var coordinatesToMineMap = _shuffler(coordinates)
                .Select((x, i) => (Coordinates: x, Index: i))
                .ToDictionary(x => x.Coordinates, x => x.Index < mineCount);
            var coordinatesToAdjacentMineCountMap = coordinates.ToDictionary(
                x => x,
                x => coordinatesToMineMap.Count(y => y.Value && IsAdjacentTo(y.Key, x)));
            var tiles = coordinatesToMineMap.Select(x => new Tile(x.Key, x.Value, coordinatesToAdjacentMineCountMap[x.Key]));

            return new Board(tiles);
        }

        internal static Coordinates[] GetCoordinates(byte rows, byte columns)
        {
            if (columns == 0) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0) { throw new ArgumentOutOfRangeException(nameof(rows)); }

            return Enumerable
                .Range(0, rows)
                .SelectMany(x => Enumerable
                    .Range(0, columns)
                    .Select(y => new Coordinates((byte)x, (byte)y)))
                .ToArray();
        }
    }
}

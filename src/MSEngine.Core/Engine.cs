using System;
using System.Collections.Generic;
using System.Linq;
using static MSEngine.Core.Utilities;

namespace MSEngine.Core
{
    internal enum Shuffler
    {
        None,
        Random,
        Hacked
    }
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine(Shuffler.Random);
        public static IEngine PureInstance { get; } = new Engine(Shuffler.None);
        public static IEngine HackedInstance { get; } = new Engine(Shuffler.Hacked);

        private static readonly Coordinates[] _beginnerCoordinates = GetCoordinates(8, 8);
        private static readonly Coordinates[] _intermediateCoordinates = GetCoordinates(16, 16);
        private static readonly Coordinates[] _expertCoordinates = GetCoordinates(30, 16);

        private readonly Shuffler _shuffler;

        private Engine(Shuffler shuffler)
        {
            _shuffler = shuffler;
        }

        public virtual Board GenerateBeginnerBoard() => GenerateCustomBoard(8, 8, 10);
        public virtual Board GenerateIntermediateBoard() => GenerateCustomBoard(16, 16, 40);
        public virtual Board GenerateExpertBoard() => GenerateCustomBoard(30, 16, 99);
        public virtual Board GenerateCustomBoard(byte columns, byte rows, byte mineCount)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }

            Span<Coordinates> coordinates = stackalloc Coordinates[rows * columns];
            var temp = columns switch
            {
                8 => _beginnerCoordinates,
                16 => _intermediateCoordinates,
                30 => _expertCoordinates,
                _ => GetCoordinates(rows, columns)
            };
            temp.CopyTo(coordinates);

            switch (_shuffler)
            {
                case Shuffler.Random:
                    Utilities.ShuffleItems(ref coordinates);
                    break;
                case Shuffler.Hacked:
                    Utilities.PseudoShuffleItems(ref coordinates);
                    break;
            };

            var i = 0;
            var coordinatesToMineMap = new Dictionary<Coordinates, bool>(coordinates.Length);
            foreach (var x in coordinates)
            {
                coordinatesToMineMap.Add(x, i < mineCount);
                i++;
            }

            var n = 0;
            var coordinatesToAdjacentMineCountMap = new Dictionary<Coordinates, int>(coordinates.Length);
            foreach (var x in coordinates)
            {
                var amc = coordinatesToMineMap.Count(y => y.Value && IsAdjacentTo(y.Key, x));
                coordinatesToAdjacentMineCountMap.Add(x, amc);
                n++;
            }

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

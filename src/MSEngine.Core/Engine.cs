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
    public enum Difficulty
    {
        Beginner,
        Intermediate,
        Expert
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

        public virtual Board GenerateBeginnerBoard() => GenerateCustomBoard(8, 8, 10, Difficulty.Beginner);
        public virtual Board GenerateIntermediateBoard() => GenerateCustomBoard(16, 16, 40, Difficulty.Intermediate);
        public virtual Board GenerateExpertBoard() => GenerateCustomBoard(30, 16, 99, Difficulty.Expert);
        public virtual Board GenerateCustomBoard(byte columns, byte rows, byte mineCount, Difficulty? difficulty = null)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }

            Span<Coordinates> coordinates = stackalloc Coordinates[rows * columns];
            (difficulty switch
            {
                Difficulty.Beginner => _beginnerCoordinates,
                Difficulty.Intermediate => _intermediateCoordinates,
                Difficulty.Expert => _expertCoordinates,
                _ => GetCoordinates(columns, rows)
            })
            .CopyTo(coordinates);

            switch (_shuffler)
            {
                case Shuffler.Random:
                    coordinates.ShuffleItems();
                    break;
                case Shuffler.Hacked:
                    coordinates.PseudoShuffleItems();
                    break;
            };

            Span<bool> mineMap = stackalloc bool[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                mineMap[i] = i < mineCount;
            }

            Span<int> amcMap = stackalloc int[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                amcMap[i] = GetAdjacentMineCount(coordinates[i], mineMap, coordinates);
            }

            Span<Tile> tiles = stackalloc Tile[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                tiles[i] = new Tile(coordinates[i], mineMap[i], amcMap[i]);
            }
            return new Board(tiles);
        }

        private static int GetAdjacentMineCount(Coordinates coor, Span<bool> mineMap, Span<Coordinates> coordinates)
        {
            var n = 0;
            for (var i = 0; i < coordinates.Length; i++)
            {
                if (mineMap[i] && IsAdjacentTo(coor, coordinates[i]))
                {
                    n++;
                }
            }
            return n;
        }

        internal static Coordinates[] GetCoordinates(byte columns, byte rows)
        {
            if (columns == 0) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0) { throw new ArgumentOutOfRangeException(nameof(rows)); }

            return Enumerable
                .Range(0, columns)
                .SelectMany(x => Enumerable
                    .Range(0, rows)
                    .Select(y => new Coordinates((byte)x, (byte)y)))
                .ToArray();
        }
    }
}

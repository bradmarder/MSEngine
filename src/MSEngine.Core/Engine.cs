using System;
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
        Expert,
        Custom
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

        public virtual void GenerateBeginnerBoard(Span<Tile> tiles) => GenerateCustomBoard(tiles, 8, 8, 10);
        public virtual void GenerateIntermediateBoard(Span<Tile> tiles) => GenerateCustomBoard(tiles, 16, 16, 40);
        public virtual void GenerateExpertBoard(Span<Tile> tiles) => GenerateCustomBoard(tiles, 30, 16, 99);
        public virtual void GenerateCustomBoard(Span<Tile> tiles, byte columns, byte rows, byte mineCount)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }
            if (tiles.Length != tileCount) { throw new ArgumentOutOfRangeException(nameof(tiles)); }

            Span<Coordinates> coordinates = stackalloc Coordinates[tileCount];
            Span<int> amcMap = stackalloc int[tileCount];

            var difficulty = columns == 8 && rows == 8 ? Difficulty.Beginner
                : columns == 16 && rows == 16 ? Difficulty.Intermediate
                : columns == 30 && rows == 16 ? Difficulty.Expert
                : Difficulty.Custom;

            (difficulty switch
            {
                Difficulty.Beginner => _beginnerCoordinates,
                Difficulty.Intermediate => _intermediateCoordinates,
                Difficulty.Expert => _expertCoordinates,
                Difficulty.Custom => GetCoordinates(columns, rows),
                _ => throw new NotImplementedException(nameof(difficulty))
            })
            .CopyTo(coordinates);

            // faster to COPY or to just create?
            //for (byte x = 0; x < columns; x++)
            //{
            //    for (byte y = 0; y < rows; y++)
            //    {
            //        coordinates[y * columns + x] = new Coordinates(x, y);
            //    }
            //}

            switch (_shuffler)
            {
                case Shuffler.Random:
                    coordinates.ShuffleItems();
                    break;
                case Shuffler.Hacked:
                    coordinates.PseudoShuffleItems();
                    break;
            };

            // if the index of a tile/coordinate is less than the mineCount, it will have a mine

            for (var i = 0; i < tileCount; i++)
            {
                amcMap[i] = GetAdjacentMineCount(coordinates[i], coordinates, tileCount, mineCount);
            }
            for (var i = 0; i < tileCount; i++)
            {
                tiles[i] = new Tile(coordinates[i], i < mineCount, amcMap[i]);
            }
        }

        private static int GetAdjacentMineCount(Coordinates coor, Span<Coordinates> coordinates, int tileCount, int mineCount)
        {
            var n = 0;
            for (var i = 0; i < tileCount; i++)
            {
                if (i < mineCount && IsAdjacentTo(coor, coordinates[i]))
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

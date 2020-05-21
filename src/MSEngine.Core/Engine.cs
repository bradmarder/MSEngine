﻿using System;
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

            for (byte x = 0; x < columns; x++)
            {
                for (byte y = 0; y < rows; y++)
                {
                    coordinates[y * columns + x] = new Coordinates(x, y);
                }
            }

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
                var amc = GetAdjacentMineCount(coordinates, coordinates[i], tileCount, mineCount);
                tiles[i] = new Tile(coordinates[i], i < mineCount, amc);
            }
        }

        private static int GetAdjacentMineCount(ReadOnlySpan<Coordinates> coordinates, Coordinates coor, int tileCount, int mineCount)
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
    }
}

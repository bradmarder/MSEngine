using System;
using System.Diagnostics;

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

        public virtual void FillBeginnerBoard(Span<Tile> tiles) => FillCustomBoard(tiles, 8, 8, 10);
        public virtual void FillIntermediateBoard(Span<Tile> tiles) => FillCustomBoard(tiles, 16, 16, 40);
        public virtual void FillExpertBoard(Span<Tile> tiles) => FillCustomBoard(tiles, 30, 16, 99);
        public virtual void FillCustomBoard(Span<Tile> tiles, byte columns, byte rows, byte mineCount)
        {
            if (columns == 0 || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == 0 || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }
            if (tiles.Length != tileCount) { throw new ArgumentOutOfRangeException(nameof(tiles)); }

            Span<int> mineIndexes = stackalloc int[mineCount];
            Span<int> adjacentIndexes = stackalloc int[8];

            switch (_shuffler)
            {
                case Shuffler.Random:
                    mineIndexes.Scatter(tileCount);
                    break;
                case Shuffler.Hacked:
                    mineIndexes.PseudoScatter(tileCount);
                    break;
            };

            for (var i = 0; i < tileCount; i++)
            {
                adjacentIndexes.FillAdjacentTileIndexes(tileCount, i, columns);
                var amc = GetAdjacentMineCount(mineIndexes, adjacentIndexes);
                var hasMine = mineIndexes.IndexOf(i) != -1;

                tiles[i] = new Tile(hasMine, amc);
            }
        }
        private static int GetAdjacentMineCount(ReadOnlySpan<int> mineIndexes, ReadOnlySpan<int> adjacentIndexes)
        {
            Debug.Assert(adjacentIndexes.Length == 8);

            var n = 0;
            for (var i = 0; i < 8; i++)
            {
                if (mineIndexes.IndexOf(adjacentIndexes[0]) != -1)
                {
                    n++;
                }
            }
            return n;
        }
    }
}

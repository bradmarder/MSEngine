using System;

namespace MSEngine.Core
{
    public static class BoardExtensions
    {
        public static BoardStatus Status(this Span<Tile> tiles) => Status((ReadOnlySpan<Tile>)tiles);
        public static int Width(this Span<Tile> tiles) => Width((ReadOnlySpan<Tile>)tiles);
        public static int Height(this Span<Tile> tiles) => Height((ReadOnlySpan<Tile>)tiles);
        public static int FlagsAvailable(this Span<Tile> tiles) => FlagsAvailable((ReadOnlySpan<Tile>)tiles);
        public static int MineCount(this Span<Tile> tiles) => MineCount((ReadOnlySpan<Tile>)tiles);
        public static int FlaggedTilesCount(this Span<Tile> tiles) => FlaggedTilesCount((ReadOnlySpan<Tile>)tiles);
        public static bool AllMinesFlagged(this Span<Tile> tiles) => AllMinesFlagged((ReadOnlySpan<Tile>)tiles);

        public static BoardStatus Status(this ReadOnlySpan<Tile> tiles)
        {
            var complete = true;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.HasMineExploded)
                {
                    return BoardStatus.Failed;
                }
                if (!tile.SatisfiesWinningCriteria)
                {
                    complete = false;
                }
            }
            return complete ? BoardStatus.Completed : BoardStatus.Pending;
        }
        public static int Width(this ReadOnlySpan<Tile> tiles)
        {
            byte max = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var x = tiles[i].Coordinates.X;
                max = max > x ? max : x;
            }
            return max + 1;
        }
        public static int Height(this ReadOnlySpan<Tile> tiles)
        {
            byte max = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var y = tiles[i].Coordinates.Y;
                max = max > y ? max : y;
            }
            return max + 1;
        }
        public static int FlagsAvailable(this ReadOnlySpan<Tile> tiles) => tiles.MineCount() - tiles.FlaggedTilesCount();
        public static int MineCount(this ReadOnlySpan<Tile> tiles)
        {
            var n = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                if (tiles[i].HasMine)
                {
                    n++;
                }
            }
            return n;
        }
        public static int FlaggedTilesCount(this ReadOnlySpan<Tile> tiles)
        {
            var n = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                if (tiles[i].State == TileState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }
        public static bool AllMinesFlagged(this ReadOnlySpan<Tile> tiles)
        {
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.HasMine && tile.State != TileState.Flagged)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

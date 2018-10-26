using System;
using System.Collections.Generic;
using System.Linq;

namespace MSEngine.Core
{
    public class Board
    {
        internal Board(IEnumerable<Tile> tiles)
        {
            Tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
        }

        public IEnumerable<Tile> Tiles { get; }

        public byte Width => (byte)(1 + Tiles.Max(x => x.Coordinates.X));
        public byte Height => (byte)(1 + Tiles.Max(x => x.Coordinates.Y));
        public int FlagsAvailable => MineCount - FlaggedTilesCount;
        public int MineCount => Tiles.Count(x => x.HasMine);
        public int FlaggedTilesCount => Tiles.Count(x => x.State == TileState.Flagged);
        public BoardStatus Status =>
            Tiles.Any(x => x.HasMineExploded) ? BoardStatus.Failed
            : Tiles.All(x => x.SatisfiesWinningCriteria) ? BoardStatus.Completed
            : BoardStatus.Pending;
        public bool HasFailedOnFirstReveal =>
            Tiles.Count(x => x.State == TileState.Revealed) == 1
            && Status == BoardStatus.Failed;
    }
}

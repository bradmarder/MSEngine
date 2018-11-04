using System;
using System.Collections.Generic;
using System.Linq;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class ChordingStrategy
    {
        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var tile = board.Tiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > 0)
                .Where(x =>
                {
                    var adjacentTiles = board.Tiles.Where(y => Engine.IsAdjacentTo(x.Coordinates, y.Coordinates));
                    var adjacentHiddenTileCount = adjacentTiles.Count(y => y.State == TileState.Hidden);
                    var adjacentFlaggedTileCount = adjacentTiles.Count(y => y.State == TileState.Flagged);

                    return adjacentHiddenTileCount > 0 && x.AdjacentMineCount == adjacentFlaggedTileCount;
                })
                .Cast<Tile?>()
                .FirstOrDefault();

            if (tile != null)
            {
                turn = new Turn(tile.Value.Coordinates, TileOperation.Chord);
                return true;
            }

            turn = default;
            return false;
        }
    }
}

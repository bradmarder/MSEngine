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

            var chordTiles = board.Tiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > byte.MinValue)
                .Where(x =>
                {
                    var adjacentTiles = board.Tiles.Where(y => Engine.IsAdjacentTo(x.Coordinates, y.Coordinates));
                    var adjacentHiddenTileCount = adjacentTiles.Count(y => y.State == TileState.Hidden);
                    var adjacentFlaggedTileCount = adjacentTiles.Count(y => y.State == TileState.Flagged);

                    return adjacentHiddenTileCount > byte.MinValue && x.AdjacentMineCount == adjacentFlaggedTileCount;
                })
                .ToList();

            if (chordTiles.Any())
            {
                turn = new Turn(chordTiles.First().Coordinates, TileOperation.Chord);
                return true;
            }

            turn = default;
            return false;
        }
    }
}

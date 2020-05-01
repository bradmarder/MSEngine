using System;
using System.Linq;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class EducatedGuessStrategy
    {
        public static Turn UseStrategy(Board board)
        {
            //var tileToMineProbabilityMap = board.Tiles
            //    .Where(x => x.State == TileState.Revealed)
            //    .Where(x => x.AdjacentMineCount > 0)
            //    .ToDictionary(x => x, tile =>
            //    {
            //        var adjacentTiles = board.Tiles
            //            .Where(x => Utilities.IsAdjacentTo(x.Coordinates, tile.Coordinates))
            //            .ToList();
            //        var adjacentHiddenTileCount = adjacentTiles.Count(x => x.State == TileState.Hidden);
            //        var adjacentFlaggedTileCount = adjacentTiles.Count(x => x.State == TileState.Flagged);

            //        return adjacentHiddenTileCount == 0
            //            ? 0
            //            : (tile.AdjacentMineCount - adjacentFlaggedTileCount) / adjacentHiddenTileCount;
            //    });

            //var hiddenTileCount = board.Tiles.Count(x => x.State == TileState.Hidden);
            //var defaultMineProbability = board.FlagsAvailable / hiddenTileCount;

            var hiddenTile = board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .OrderBy(x => x.Coordinates.X)
                .ThenBy(x => x.Coordinates.Y)
                //.OrderBy(tile =>
                //{
                //    var adjacentTiles = board.Tiles
                //        .Where(x => tileToMineProbabilityMap.Keys.Contains(x))
                //        .Where(x => Utilities.IsAdjacentTo(tile.Coordinates, x.Coordinates))
                //        .ToList();

                //    return adjacentTiles.Any()
                //        ? adjacentTiles.Max(x => tileToMineProbabilityMap[x])
                //        : defaultMineProbability;
                //})
                .First();

            return new Turn(hiddenTile.Coordinates, TileOperation.Reveal);
        }
    }
}

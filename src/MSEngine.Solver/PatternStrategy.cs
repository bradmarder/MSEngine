using System;
using System.Linq;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class PatternStrategy
    {
        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            var revealedTilesWithAMC = board.Tiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > 0)
                .ToList();
            var hiddenTiles = board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .ToList();
            var flaggedTiles = board.Tiles
                .Where(x => x.State == TileState.Flagged)
                .ToList();
            var primaryTileToNextTilesMap = revealedTilesWithAMC.ToDictionary(
                x => x,
                x => revealedTilesWithAMC
                    .Where(y => Utilities.IsNextTo(y.Coordinates, x.Coordinates))
                    .ToList());

            foreach (var primary in primaryTileToNextTilesMap)
            {
                if (!primary.Value.Any())
                {
                    continue;
                }

                var primaryFlaggedAjacentTileCount = flaggedTiles.Count(x => Utilities.IsAdjacentTo(x.Coordinates, primary.Key.Coordinates));
                var primaryHiddenAdjacentTiles = hiddenTiles
                    .Where(x => Utilities.IsAdjacentTo(x.Coordinates, primary.Key.Coordinates))
                    .ToList();

                foreach (var secondary in primary.Value)
                {
                    var secondaryHiddenAdjacentTiles = hiddenTiles
                        .Where(x => Utilities.IsAdjacentTo(x.Coordinates, secondary.Coordinates))
                        .ToList();
                    var secondaryExtraTiles = Enumerable
                        .Except(secondaryHiddenAdjacentTiles, primaryHiddenAdjacentTiles)
                        .ToList();

                    if (!secondaryExtraTiles.Any())
                    {
                        continue;
                    }

                    var secondaryFlaggedAjacentTileCount = flaggedTiles.Count(x => Utilities.IsAdjacentTo(x.Coordinates, secondary.Coordinates));
                    var sharedHiddenTileCount = Enumerable
                        .Intersect(primaryHiddenAdjacentTiles, secondaryHiddenAdjacentTiles)
                        .Count();

                    // we know there are n mines in the sharedHiddenTiles
                    var sharedHiddenTileMineCount = primary.Key.AdjacentMineCount - primaryFlaggedAjacentTileCount;
                    var extraMineCount = secondary.AdjacentMineCount - secondaryFlaggedAjacentTileCount;

                    if (primaryHiddenAdjacentTiles.Count == sharedHiddenTileCount && extraMineCount == sharedHiddenTileMineCount)
                    {
                        turn = new Turn(secondaryExtraTiles.First().Coordinates, TileOperation.Reveal);
                        return true;
                    }

                    if (extraMineCount > sharedHiddenTileMineCount && (extraMineCount - sharedHiddenTileMineCount) == secondaryExtraTiles.Count)
                    {
                        turn = new Turn(secondaryExtraTiles.First().Coordinates, TileOperation.Flag);
                        return true;
                    }
                }
            }

            turn = default;
            return false;
        }
    }
}
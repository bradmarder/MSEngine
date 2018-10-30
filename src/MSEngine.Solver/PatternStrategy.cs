﻿using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class PatternStrategy
    {
        public static bool IsNextTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            // filters ordered to prioritize short-circuiting
            return (x == coordinateTwo.X && new[] { y + 1, y - 1 }.Contains(coordinateTwo.Y))
                || (y == coordinateTwo.Y && new[] { x + 1, x - 1 }.Contains(coordinateTwo.X));
        }

        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var revealedTilesWithAMC = board.Tiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > 0)
                .ToList();

            var hiddenTiles = board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .ToList();

            var primaryTileToNextTilesMap = revealedTilesWithAMC.ToDictionary(x => x, x => revealedTilesWithAMC.Where(y => IsNextTo(y.Coordinates, x.Coordinates)));

            foreach (var primary in primaryTileToNextTilesMap)
            {
                var primaryHiddenAdjacentTiles = hiddenTiles
                    .Where(x => Engine.IsAdjacentTo(x.Coordinates, primary.Key.Coordinates))
                    .ToList();

                foreach (var secondary in primary.Value)
                {
                    var secondaryHiddenAdjacentTiles = hiddenTiles
                        .Where(x => Engine.IsAdjacentTo(x.Coordinates, secondary.Coordinates))
                        .ToList();

                    var sharedHiddenTiles = primaryHiddenAdjacentTiles
                        .Intersect(secondaryHiddenAdjacentTiles)
                        .ToList();

                    // the secondary tile must have more hidden adjacent tiles
                    if (primaryHiddenAdjacentTiles.Count >= secondaryHiddenAdjacentTiles.Count)
                    {
                        continue;
                    }

                    // both AMC must be less than the shared hidden count
                    if (primary.Key.AdjacentMineCount >= sharedHiddenTiles.Count || secondary.AdjacentMineCount >= sharedHiddenTiles.Count)
                    {
                        continue;
                    }

                    // the primaryHiddenAdjacentTiles must be a subset of the secondaryHiddenAdjacentTiles
                    var extraTiles = secondaryHiddenAdjacentTiles
                        .Except(primaryHiddenAdjacentTiles)
                        .ToList();
                    if (!extraTiles.Any())
                    {
                        continue;
                    }

                    var reveal = extraTiles.First();
                    turn = new Turn(reveal.Coordinates, TileOperation.Reveal);
                    return true;
                }
            }

            turn = default;
            return false;
        }
    }
}

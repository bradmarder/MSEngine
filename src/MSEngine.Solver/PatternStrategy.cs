using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class PatternStrategy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            var flaggedTiles = board.Tiles
                .Where(x => x.State == TileState.Flagged)
                .ToList();
            var primaryTileToNextTilesMap = revealedTilesWithAMC.ToDictionary(x => x, x => revealedTilesWithAMC.Where(y => IsNextTo(y.Coordinates, x.Coordinates)));

            foreach (var primary in primaryTileToNextTilesMap)
            {
                var primaryHiddenAdjacentTiles = hiddenTiles
                    .Where(x => Engine.IsAdjacentTo(x.Coordinates, primary.Key.Coordinates))
                    .ToList();
                var primaryFlaggedAjacentTileCount = flaggedTiles.Count(x => Engine.IsAdjacentTo(x.Coordinates, primary.Key.Coordinates));

                foreach (var secondary in primary.Value)
                {
                    var secondaryFlaggedAjacentTileCount = flaggedTiles.Count(x => Engine.IsAdjacentTo(x.Coordinates, secondary.Coordinates));
                    var secondaryHiddenAdjacentTiles = hiddenTiles
                        .Where(x => Engine.IsAdjacentTo(x.Coordinates, secondary.Coordinates))
                        .ToList();

                    var sharedHiddenTiles = primaryHiddenAdjacentTiles
                        .Intersect(secondaryHiddenAdjacentTiles)
                        .ToList();

                    // the secondary tile must have more hidden adjacent tiles
                    if ((primaryHiddenAdjacentTiles.Count - primaryFlaggedAjacentTileCount) >= secondaryHiddenAdjacentTiles.Count - secondaryFlaggedAjacentTileCount)
                    {
                        continue;
                    }

                    // both AMC must be less than the shared hidden count
                    if (((primary.Key.AdjacentMineCount - primaryFlaggedAjacentTileCount) >= sharedHiddenTiles.Count)
                        || ((secondary.AdjacentMineCount - secondaryFlaggedAjacentTileCount) >= sharedHiddenTiles.Count))
                    {
                        continue;
                    }
                    
                    // the primaryHiddenAdjacentTiles must be a subset of the secondaryHiddenAdjacentTiles
                    var secondaryExtraTiles = secondaryHiddenAdjacentTiles
                        .Except(primaryHiddenAdjacentTiles)
                        .ToList();
                    if (!secondaryExtraTiles.Any())
                    {
                        continue;
                    }

                    // the secondaryHiddenAdjacentTilesCount must be equal to it's AMC + FlagCount
                    if (secondaryExtraTiles.Count + secondaryFlaggedAjacentTileCount > secondary.AdjacentMineCount)
                    {
                        continue;
                    }

                    // testing branch
                    if (primary.Key.AdjacentMineCount == 1)
                    {
                        var op = secondary.AdjacentMineCount == 1 ? TileOperation.Reveal : TileOperation.Flag;
                        turn = new Turn(secondaryExtraTiles.First().Coordinates, op);
                        return true;
                    }

                    continue;
                    //var extraMineCount = Math.Abs(secondary.AdjacentMineCount - primary.Key.AdjacentMineCount);
                    // MUST look at primary/secondary AMC
                    //var operation = primary.Key.AdjacentMineCount == secondaryExtraTiles.Count && secondary.AdjacentMineCount > primary.Key.AdjacentMineCount
                    //    ? TileOperation.Flag
                    //    : TileOperation.Reveal;

                    //Console.WriteLine("extraMineCount = " + extraMineCount);
                    //Console.WriteLine("primary = " + primary.Key.Coordinates.X + "," + primary.Key.Coordinates.Y);
                    //Console.WriteLine("secondary = " + secondary.Coordinates.X + "," + secondary.Coordinates.Y);
                    //Console.WriteLine("primaryFlaggedAjacentTileCount = " + primaryFlaggedAjacentTileCount); //   0
                    //Console.WriteLine("secondaryFlaggedAjacentTileCount = " + secondaryFlaggedAjacentTileCount);// 0
                    //Console.WriteLine("secondaryExtraTiles = " + secondaryExtraTiles.Count); // 1
                    //var reveal = secondaryExtraTiles.First();
                    //turn = new Turn(reveal.Coordinates, operation);
                    //return true;
                }
            }

            turn = default;
            return false;
        }
    }
}

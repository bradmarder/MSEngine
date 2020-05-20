using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class PatternStrategy
    {
        private static readonly ConcurrentDictionary<uint, bool> _nextTileMap = new ConcurrentDictionary<uint, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNextTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var key = coordinateOne.X
                | (uint)coordinateOne.Y << 8
                | (uint)coordinateTwo.X << 16
                | (uint)coordinateTwo.Y << 24;

            if (_nextTileMap.TryGetValue(key, out var value))
            {
                return value;
            }

            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            var val = (x == coordinateTwo.X && new[] { y + 1, y - 1 }.Contains(coordinateTwo.Y))
                || (y == coordinateTwo.Y && new[] { x + 1, x - 1 }.Contains(coordinateTwo.X));

            _nextTileMap.TryAdd(key, val);

            return val;
        }

        public static bool TryUseStrategy(Span<Tile> tiles, out Turn turn)
        {
            var linqTiles = tiles.ToArray();

            var revealedTilesWithAMC = linqTiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > 0)
                .ToList();
            var hiddenTiles = linqTiles
                .Where(x => x.State == TileState.Hidden)
                .ToList();
            var flaggedTiles = linqTiles
                .Where(x => x.State == TileState.Flagged)
                .ToList();
            var primaryTileToNextTilesMap = revealedTilesWithAMC.ToDictionary(
                x => x,
                x => revealedTilesWithAMC
                    .Where(y => IsNextTo(y.Coordinates, x.Coordinates))
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
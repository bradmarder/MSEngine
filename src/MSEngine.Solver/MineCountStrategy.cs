﻿using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    /// <summary>
    /// adjacent mine count equals (adjacent tile hidden count + flagged count)
    /// </summary>
    public static class MineCountStrategy
    {
        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            foreach (var i in Enumerable.Range(1, 8).Reverse())
            {
                var foo = board.Tiles
                    .Where(x => x.AdjacentMineCount == i)
                    .Where(x => x.State == TileState.Revealed)
                    .Where(x =>
                    {
                        var adjacentTiles = board.Tiles.Where(y => Engine.IsAdjacentTo(x.Coordinates, y.Coordinates));
                        var flagCount = adjacentTiles.Count(y => y.State == TileState.Flagged);
                        var hiddenCount = adjacentTiles.Count(y => y.State == TileState.Hidden);

                        return i == (hiddenCount + flagCount);
                    })
                    .Select(x => x.Coordinates)
                    .SelectMany(x => board.Tiles.Where(y => y.State == TileState.Hidden && Engine.IsAdjacentTo(x, y.Coordinates)))
                    .ToList();

                if (foo.Any())
                {
                    turn = new Turn(foo.First().Coordinates, TileOperation.Flag);
                    return true;
                }
            }

            turn = default;
            return false;
        }
    }
}

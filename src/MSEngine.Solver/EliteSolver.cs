using System;
using System.Linq;
using System.Collections.Generic;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public class EliteSolver : ISolver
    {
        public Turn ComputeTurn(Board board)
        {
            // first turn - pick a tile in the middle of the board
            if (board.Tiles.All(x => x.State == TileState.Hidden))
            {
                var xCoordinate = board.Tiles.Average(x => x.Coordinates.X);
                var yCoordinate = board.Tiles.Average(x => x.Coordinates.Y);
                return new Turn(
                    Convert.ToByte(xCoordinate),
                    Convert.ToByte(yCoordinate),
                    TileOperation.Reveal);
            }

            // chording strategy
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
                return new Turn(chordTiles.First().Coordinates, TileOperation.Chord);
            }

            // adjacent mine count equals (adjacent tile hidden count + flagged count)
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
                    return new Turn(foo.First().Coordinates, TileOperation.Flag);
                }
            }

            // reveal the first hidden tile
            // TODO: select tile with lower probabilty of having a mine
            // we need to order by x/y because the tiles are currently ordered by HasMine
            var firstHiddenTile = board.Tiles
                .OrderBy(x => x.Coordinates.Y)
                .ThenBy(x => x.Coordinates.X)
                .First(x => x.State == TileState.Hidden);
            return new Turn(firstHiddenTile.Coordinates, TileOperation.Reveal);
        }
    }
}

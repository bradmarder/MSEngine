using System;
using System.Linq;
using System.Collections.Generic;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public class RandomSolver
    {
        public Turn ComputeTurn(Span<Tile> tiles)
        {
            var randomTile = tiles
                .ToArray()
                .Where(x => x.State == TileState.Hidden)
                .OrderBy(x => Guid.NewGuid().GetHashCode())
                .First();

            var operation = tiles.FlagsAvailable() > 0 ? TileOperation.Flag : TileOperation.Reveal;

            return new Turn(randomTile.Coordinates, operation);
        }
    }
}

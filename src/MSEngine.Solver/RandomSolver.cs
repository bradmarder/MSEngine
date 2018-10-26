using System;
using System.Linq;
using System.Collections.Generic;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public class RandomSolver : ISolver
    {
        public Turn ComputeTurn(Board board)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var randomTile = board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .OrderBy(x => Guid.NewGuid().GetHashCode())
                .First();

            var operation = board.FlagsAvailable > 0 ? TileOperation.Flag : TileOperation.Reveal;

            return new Turn(randomTile.Coordinates, operation);
        }
    }
}

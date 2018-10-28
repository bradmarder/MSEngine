using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    /// <summary>
    /// // reveal the first hidden tile\
    /// TODO: select tile with lower probabilty of having a mine
    /// we need to order by x/y because the tiles are currently ordered by HasMine
    /// </summary>
    public static class EducatedGuessStrategy
    {
        public static Turn UseStrategy(Board board)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var firstHiddenTile = board.Tiles
                .OrderBy(x => x.Coordinates.Y)
                .ThenBy(x => x.Coordinates.X)
                .First(x => x.State == TileState.Hidden);

            return new Turn(firstHiddenTile.Coordinates, TileOperation.Reveal);
        }
    }
}

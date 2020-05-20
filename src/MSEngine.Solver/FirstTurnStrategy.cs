using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class FirstTurnStrategy
    {
        public static bool TryUseStrategy(Span<Tile> tiles, out Turn turn)
        {
            var linqTiles = tiles.ToArray();
            var isFirstTurn = linqTiles.All(x => x.State == TileState.Hidden);
            if (isFirstTurn)
            {
                var xCoordinate = linqTiles.Average(x => x.Coordinates.X);
                var yCoordinate = linqTiles.Average(x => x.Coordinates.Y);

                turn = new Turn(
                    Convert.ToByte(xCoordinate),
                    Convert.ToByte(yCoordinate),
                    TileOperation.Reveal);
                return true;
            }

            turn = default;
            return false;
        }
    }
}

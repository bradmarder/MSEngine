using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class FirstTurnStrategy
    {
        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            var isFirstTurn = board.Tiles.All(x => x.State == TileState.Hidden);
            if (isFirstTurn)
            {
                var xCoordinate = board.Tiles.Average(x => x.Coordinates.X);
                var yCoordinate = board.Tiles.Average(x => x.Coordinates.Y);

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

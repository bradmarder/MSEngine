using System;
using System.Linq;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class FinishStrategy
    {
        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            if (board.AllMinesFlagged)
            {
                var tile = board.Tiles.First(x => x.State == TileState.Hidden);
                turn = new Turn(tile.Coordinates, TileOperation.Reveal);
                return true;
            }

            turn = default;
            return false;
        }
    }
}

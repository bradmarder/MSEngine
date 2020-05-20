using System;
using System.Linq;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class FinishStrategy
    {
        public static bool TryUseStrategy(Span<Tile> tiles, out Turn turn)
        {
            if (tiles.AllMinesFlagged())
            {
                var tile = tiles
                    .ToArray()
                    .First(x => x.State == TileState.Hidden);
                turn = new Turn(tile.Coordinates, TileOperation.Reveal);
                return true;
            }

            turn = default;
            return false;
        }
    }
}

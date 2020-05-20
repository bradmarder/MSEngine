using System;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class EducatedGuessStrategy
    {
        public static Turn UseStrategy(Span<Tile> tiles)
        {
            var maxIndex = -1;
            var maxHash = int.MinValue;

            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                var hash = tile.GetHashCode();
                if (tile.State == TileState.Hidden && hash > maxHash)
                {
                    maxIndex = i;
                    maxHash = hash;
                }
            }

            return new Turn(tiles[maxIndex].Coordinates, TileOperation.Reveal);
        }
    }
}

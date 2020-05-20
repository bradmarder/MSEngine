using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAdjacentFlaggedTileCount(Span<Tile> tiles, Coordinates coordinates)
        {
            var n = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State == TileState.Flagged && Utilities.IsAdjacentTo(tile.Coordinates, coordinates))
                {
                    n++;
                }

                // technically, you may short-circuit the for-loop here if n = 8 (the max AMC)
                if (n == 8)
                {
                    break;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasHiddenAdjacentTiles(Span<Coordinates> hiddenCoordinates, Coordinates coordinates)
        {
            for (int i = 0, l = hiddenCoordinates.Length; i < l; i++)
            {
                if (Utilities.IsAdjacentTo(hiddenCoordinates[i], coordinates))
                {
                    return true;
                }
            }
            return false;
        }

        public static void CalculateTurns(Span<Tile> tiles, ref Span<Turn> turns)
        {
            if (tiles.Length != turns.Length)
            {
                throw new InvalidOperationException("Turns must be overallocated to the ~maximum possible");
            }

            // overallocated
            Span<Coordinates> hiddenCoordinates = stackalloc Coordinates[tiles.Length];
            Span<Tile> revealedAMCTiles = stackalloc Tile[tiles.Length];

            var hiddenTileCount = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State == TileState.Hidden)
                {
                    hiddenCoordinates[hiddenTileCount] = tile.Coordinates;
                    hiddenTileCount++;
                }
            }
            hiddenCoordinates = hiddenCoordinates.Slice(0, hiddenTileCount);

            var revealedAMCTileCount = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State == TileState.Revealed && tile.AdjacentMineCount > 0 && HasHiddenAdjacentTiles(hiddenCoordinates, tile.Coordinates))
                {
                    revealedAMCTiles[revealedAMCTileCount] = tile;
                    revealedAMCTileCount++;
                }
            }
            revealedAMCTiles = revealedAMCTiles.Slice(0, revealedAMCTileCount);

            if (revealedAMCTileCount == 0)
            {
                turns.Clear();
                return;
            }

            Span<Coordinates> revealedCoordinates = stackalloc Coordinates[revealedAMCTiles.Length];
            for (int i = 0, l = revealedCoordinates.Length; i < l; i++)
            {
                revealedCoordinates[i] = revealedAMCTiles[i].Coordinates;
            }

            // overallocated
            Span<Coordinates> adjacentHiddenCoordinates = stackalloc Coordinates[hiddenCoordinates.Length];
            var ahcCount = 0;
            for (int i = 0, l = adjacentHiddenCoordinates.Length; i < l; i++)
            {
                bool hasAHC = false;
                var hiddenCoor = hiddenCoordinates[i];
                for (int n = 0, m = revealedCoordinates.Length; n < m; n++)
                {
                    // if we find *Any* adjacent, we break out of the inner for loop

                    if (Utilities.IsAdjacentTo(hiddenCoor, revealedCoordinates[n]))
                    {
                        hasAHC = true;
                        break;
                    }
                }
                if (hasAHC)
                {
                    adjacentHiddenCoordinates[ahcCount] = hiddenCoor;
                    ahcCount++;
                }
            }
            adjacentHiddenCoordinates = adjacentHiddenCoordinates.Slice(0, ahcCount);

            var rowCount = revealedAMCTiles.Length;
            var columnCount = adjacentHiddenCoordinates.Length + 1;

            Span<sbyte> foo = stackalloc sbyte[rowCount * columnCount];
            var matrix = new FlatMatrix<sbyte>(foo, columnCount);

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var tile = revealedAMCTiles[row];
                    matrix[row, column] = (sbyte)(column == columnCount - 1

                        // augmented column has special logic
                        ? tile.AdjacentMineCount - GetAdjacentFlaggedTileCount(tiles, tile.Coordinates)

                        : Utilities.IsAdjacentTo(revealedCoordinates[row], adjacentHiddenCoordinates[column]) ? 1 : 0);
                }
            }

            matrix.GaussEliminate();

            // exclude the augmented column
            var finalIndex = columnCount - 1;
            Span<sbyte> vector = stackalloc sbyte[finalIndex];
            var turnCount = 0;

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < finalIndex; column++)
                {
                    vector[column] = matrix[row, column];
                }
                
                var final = matrix[row, finalIndex];
                var min = 0;
                var max = 0;
                foreach (var x in vector)
                {
                    max += x > 0 ? x : 0;
                    min += x < 0 ? x : 0;
                }

                if (final != min && final != max)
                {
                    continue;
                }

                for (var column = 0; column < finalIndex; column++)
                {
                    var val = vector[column];
                    if (val == 0)
                    {
                        continue;
                    }
                    var coor = adjacentHiddenCoordinates[column];

                    var turn = final == min

                        // All of the negative numbers in that row are mines and all of the positive values in that row are not mines
                        ? new Turn(coor.X, coor.Y, val > 0 ? TileOperation.Reveal : TileOperation.Flag)

                        // All of the negative numbers in that row are not mines and all of the positive values in that row are mines.
                        : new Turn(coor.X, coor.Y, val > 0 ? TileOperation.Flag : TileOperation.Reveal);

                    // prevent adding duplicate turns
                    if (turns.Slice(0, turnCount).IndexOf(turn) == -1)
                    {
                        turns[turnCount] = turn;
                        turnCount++;
                    }
                }
            }

            // we must slice due to overallocation
            turns = turns.Slice(0, turnCount);
        }
    }
}
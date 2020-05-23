using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAdjacentFlaggedTileCount(ReadOnlySpan<Tile> tiles, Span<int> adjacentIndexes, int tileIndex)
        {
            Debug.Assert(adjacentIndexes.Length == 8);
            Debug.Assert(tileIndex >= 0);

            adjacentIndexes.FillAdjacentTileIndexes(tiles.Length, tileIndex, 8);

            var n = 0;
            foreach (var i in adjacentIndexes)
            {
                if (i == -1) { continue; }

                if (tiles[i].State == TileState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasHiddenAdjacentTiles(ReadOnlySpan<Tile> tiles, int tileIndex)
        {
            Debug.Assert(tileIndex >= 0);

            Span<int> adjacentTileIndexes = stackalloc int[8];
            adjacentTileIndexes.FillAdjacentTileIndexes(tiles.Length, tileIndex, 8);

            foreach (var x in adjacentTileIndexes)
            {
                if (x == -1) { continue; }
                if (tiles[x].State == TileState.Hidden)
                {
                    return true;
                }
            }

            return false;
        }

        public static void CalculateTurns(ReadOnlySpan<Tile> tiles, ref Span<Turn> turns)
        {
            Debug.Assert(tiles.Length > 0);
            Debug.Assert(tiles.Length == turns.Length);

            // overallocated
            Span<int> hiddenTileIndexes = stackalloc int[tiles.Length];
            Span<int> revealedAMCTiles = stackalloc int[tiles.Length];

            var hiddenTileCount = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                if (tiles[i].State == TileState.Hidden)
                {
                    hiddenTileIndexes[hiddenTileCount] = i;
                    hiddenTileCount++;
                }
            }
            hiddenTileIndexes = hiddenTileIndexes.Slice(0, hiddenTileCount);

            var revealedAMCTileCount = 0;
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State == TileState.Revealed && tile.AdjacentMineCount > 0 && HasHiddenAdjacentTiles(tiles, i))
                {
                    revealedAMCTiles[revealedAMCTileCount] = i;
                    revealedAMCTileCount++;
                }
            }
            revealedAMCTiles = revealedAMCTiles.Slice(0, revealedAMCTileCount);

            if (revealedAMCTileCount == 0)
            {
                turns = Span<Turn>.Empty;
                return;
            }

            // overallocated
            Span<int> adjacentHiddenCoordinates = stackalloc int[hiddenTileIndexes.Length];
            var ahcCount = 0;
            for (int i = 0, l = adjacentHiddenCoordinates.Length; i < l; i++)
            {
                bool hasAHC = false;
                var hiddenCoor = hiddenTileIndexes[i];
                for (int n = 0, m = revealedAMCTiles.Length; n < m; n++)
                {
                    // if we find *Any* adjacent, we break out of the inner for loop
                    if (Utilities.IsAdjacentTo(hiddenCoor, revealedAMCTiles[n]))
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

            Span<int> adjacentIndexes = stackalloc int[8];
            Span<int> buffer = stackalloc int[rowCount * columnCount];
            var matrix = new FlatMatrix<int>(buffer, columnCount);

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var tileIndex = revealedAMCTiles[row];
                    var tile = tiles[tileIndex];

                    matrix[row, column] = (column == columnCount - 1

                        // augmented column has special logic
                        ? tile.AdjacentMineCount - GetAdjacentFlaggedTileCount(tiles, adjacentIndexes, tileIndex)

                        : Utilities.IsAdjacentTo(revealedCoordinates[row], adjacentHiddenCoordinates[column]) ? 1 : 0);
                }
            }

            // output view of the matrix
            // output view of gauss matrix (ensure gaussed properly??)

            matrix.GaussEliminate();

            // exclude the augmented column
            var finalIndex = columnCount - 1;
            Span<int> vector = stackalloc int[finalIndex];
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
                    var index = adjacentHiddenCoordinates[column];

                    var turn = final == min

                        // All of the negative numbers in that row are mines and all of the positive values in that row are not mines
                        ? new Turn(index, val > 0 ? TileOperation.Reveal : TileOperation.Flag)

                        // All of the negative numbers in that row are not mines and all of the positive values in that row are mines.
                        : new Turn(index, val > 0 ? TileOperation.Flag : TileOperation.Reveal);

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
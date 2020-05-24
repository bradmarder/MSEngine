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

            //Span<int> hiddenTileIndexes = stackalloc int[tiles.Length];
            Span<int> revealedAMCTiles = stackalloc int[tiles.Length];

            #region Hidden Nodes

            //var hiddenTileCount = 0;
            //for (int i = 0, l = tiles.Length; i < l; i++)
            //{
            //    if (tiles[i].State == TileState.Hidden)
            //    {
            //        hiddenTileIndexes[hiddenTileCount] = i;
            //        hiddenTileCount++;
            //    }
            //}

            #endregion

            #region Revealed Nodes with AMC > 0

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

            #endregion

            if (revealedAMCTileCount == 0)
            {
                turns = Span<Turn>.Empty;
                return;
            }

            #region Adjacent Hidden Node Indexes

            var ahcCount = 0;
            Span<int> foo = stackalloc int[8];
            Span<int> adjacentHiddenNodeIndex = stackalloc int[tiles.Length];

            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State != TileState.Hidden) { continue; }

                var hasAHC = false;
                foo.FillAdjacentTileIndexes(tiles.Length, i, 8);

                foreach (var x in foo)
                {
                    if (x == -1) { continue; }

                    var adjNode = tiles[x];
                    if (adjNode.State == TileState.Revealed && adjNode.AdjacentMineCount > 0 && HasHiddenAdjacentTiles(tiles, x))
                    {
                        hasAHC = true;
                        break;
                    }
                }
                if (hasAHC)
                {
                    adjacentHiddenNodeIndex[ahcCount] = i;
                    ahcCount++;
                }
            }

            #endregion

            #region Raw Matrix Generation

            var rowCount = revealedAMCTileCount;
            var columnCount = ahcCount + 1;

            Span<int> buffer = stackalloc int[rowCount * columnCount];
            var matrix = new FlatMatrix<int>(buffer, columnCount);

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var tileIndex = revealedAMCTiles[row];
                    var tile = tiles[tileIndex];

                    matrix[row, column] = column == columnCount - 1

                        // augmented column has special logic
                        ? tile.AdjacentMineCount - GetAdjacentFlaggedTileCount(tiles, foo, tileIndex)

                        : Utilities.IsAdjacentTo(foo, tiles.Length, 8, tileIndex, adjacentHiddenNodeIndex[column]) ? 1 : 0;
                        //: Utilities.IsAdjacentTo(revealedCoordinates[row], adjacentHiddenNodeIndex[column]) ? 1 : 0;
                }
            }

            #endregion

            // output view of the matrix
            // output view of gauss matrix (ensure gaussed properly??)

            matrix.GaussEliminate();

            #region Guass Matrix Processing

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
                    var index = adjacentHiddenNodeIndex[column];

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

            #endregion

            // we must slice due to overallocation
            turns = turns.Slice(0, turnCount);
        }
    }
}
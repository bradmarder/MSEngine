using MSEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        private static readonly List<Turn> _emptyTurns = new List<Turn>(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetAdjacentFlaggedTileCount(Board board, Coordinates coordinates)
        {
            return board.Tiles.Count(x =>
                x.State == TileState.Flagged
                && Utilities.IsAdjacentTo(x.Coordinates, coordinates));
        }

        public static List<Turn> CalculateTurns(Board board)
        {
            var hiddenCoordinates = board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .Select(x => x.Coordinates)
                .ToList();
            var revealedAMCTiles = board.Tiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > 0)
                .Where(x => hiddenCoordinates.Any(y => Utilities.IsAdjacentTo(y, x.Coordinates)))
                //.Where(x => x.AdjacentMineCount > GetAdjacentFlaggedTileCount(board, x.Coordinates)) insignificant optimzation
                .ToList();

            if (!revealedAMCTiles.Any())
            {
                return _emptyTurns;
            }

            var revealedCoordinates = revealedAMCTiles
                .Select(x => x.Coordinates)
                .ToList();
            var adjacentHiddenCoordinates = hiddenCoordinates
                .Where(x => revealedCoordinates.Any(y => Utilities.IsAdjacentTo(y, x)))
                .ToList();

            var rowCount = revealedAMCTiles.Count;
            var columnCount = adjacentHiddenCoordinates.Count + 1;

            Span<sbyte> foo = stackalloc sbyte[rowCount * columnCount];
            var matrix = new FlatMatrix<sbyte>(foo, columnCount);

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var tile = revealedAMCTiles[row];
                    matrix[row, column] = (sbyte)(column == columnCount - 1

                        // augmented column has special logic
                        ? tile.AdjacentMineCount - GetAdjacentFlaggedTileCount(board, tile.Coordinates)

                        : Utilities.IsAdjacentTo(revealedCoordinates[row], adjacentHiddenCoordinates[column]) ? 1 : 0);
                }
            }

            matrix.GaussEliminate();

            var turns = new List<Turn>();

            // exclude the augmented column
            var finalIndex = columnCount - 1;
            Span<sbyte> vector = stackalloc sbyte[finalIndex];

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

                    turns.Add(turn);
                }
            }

            return turns
                 .Distinct()
                 //.OrderByDescending(x => x.Operation) // questionable optimization: Flag before Reveal
                 .ToList();
        }
    }
}

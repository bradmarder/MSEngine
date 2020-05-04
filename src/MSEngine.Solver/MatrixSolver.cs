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

        public static IEnumerable<IEnumerable<T>> Rows<T>(this T[,] matrix)
        {
            var min = matrix.GetLowerBound(0);
            var max = matrix.GetUpperBound(0);

            for (var x = min; x <= max; x++)
            {
                yield return Row(matrix, x);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<T> Row<T>(T[,] matrix, int r)
        {
            var min = matrix.GetLowerBound(1);
            var max = matrix.GetUpperBound(1);

            for (var x = min; x <= max; x++)
            {
                yield return matrix[r, x];
            }
        }

        public static List<Turn> CalculateTurns(Board board)
        {
            var hiddenCoordinates = board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .Select(x => x.Coordinates)
                .ToList();
            var list = board.Tiles
                .Where(x => x.State == TileState.Revealed)
                .Where(x => x.AdjacentMineCount > 0)
                .Where(x => hiddenCoordinates.Any(y => Utilities.IsAdjacentTo(y, x.Coordinates)))
                //.Where(x => x.AdjacentMineCount > GetAdjacentFlaggedTileCount(board, x.Coordinates)) insignificant optimzation
                .ToList();

            if (!list.Any())
            {
                return _emptyTurns;
            }

            var revealedCoordinates = list
                .Select(x => x.Coordinates)
                .ToList();
            var adjacentHiddenCoordinates = hiddenCoordinates
                .Where(x => revealedCoordinates.Any(y => Utilities.IsAdjacentTo(y, x)))
                .ToList();

            var rowCount = list.Count;
            var columnCount = adjacentHiddenCoordinates.Count + 1;
            var matrix = new sbyte[rowCount, columnCount];

            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var tile = list[row];
                    matrix[row, column] = (sbyte)(column == columnCount - 1

                        // augmented column has special logic
                        ? tile.AdjacentMineCount - GetAdjacentFlaggedTileCount(board, tile.Coordinates)

                        : Utilities.IsAdjacentTo(revealedCoordinates[row], adjacentHiddenCoordinates[column]) ? 1 : 0);
                }
            }

            return matrix
                .GaussEliminate()
                .Rows()
                .Select(Enumerable.ToList)
                .Select(vector =>
                {
                    // exclude the augmented column
                    var vals = vector
                        .Take(vector.Count - 1)
                        .ToList();

                    var final = vector.Last();
                    var max = vals.Sum(x => x > 0 ? x : 0);
                    var min = vals.Sum(x => x < 0 ? x : 0);
                    var nonZeroValues = vals
                        .Select((x, i) => (Val: x, Coordinates: adjacentHiddenCoordinates[i]))
                        .Where(x => x.Val != 0);

                    // All of the negative numbers in that row are mines and all of the positive values in that row are not mines
                    return final == min ? nonZeroValues.Select(x => new Turn(x.Coordinates.X, x.Coordinates.Y, x.Val > 0 ? TileOperation.Reveal : TileOperation.Flag))

                        // All of the negative numbers in that row are not mines and all of the positive values in that row are mines.
                        : final == max ? nonZeroValues.Select(x => new Turn(x.Coordinates.X, x.Coordinates.Y, x.Val > 0 ? TileOperation.Flag : TileOperation.Reveal))

                        : Enumerable.Empty<Turn>();
                })
                .SelectMany(x => x)
                .Distinct()
                //.OrderByDescending(x => x.Operation) // questionable optimization: Flag before Reveal
                .ToList();
        }
    }
}

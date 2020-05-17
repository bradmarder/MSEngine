using MSEngine.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class MatrixSolver
    {
        private static readonly List<Turn> _emptyTurns = new List<Turn>(0);
        private static readonly ConcurrentDictionary<ulong, sbyte[,]> _matrices = new ConcurrentDictionary<ulong, sbyte[,]>();

        private static ulong GetMatrixKey(int rowCount, int columnCount)
        {
            var row = (ulong)rowCount << 16;
            var col = (ulong)columnCount;
            return row | col;
        }

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

            //var key = GetMatrixKey(rowCount, columnCount);
            //var matrix = _matrices.TryRemove(key, out var val)
            //    ? val
            //    : new sbyte[rowCount, columnCount];
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

            matrix.GaussEliminate();

            // impossible to have more turns than tiles, so we set a max capacity
            var turns = new List<Turn>(rowCount * columnCount);

            for (var row = 0; row < rowCount; row++)
            {
                Span<sbyte> vector = stackalloc sbyte[columnCount];
                for (var column = 0; column < columnCount; column++)
                {
                    vector[column] = matrix[row, column];

                    // exclude the augmented column
                    var finalIndex = columnCount - 1;
                    var vals = vector.Slice(0, finalIndex);
                    var final = vector[finalIndex];
                    var min = 0;
                    var max = 0;
                    foreach (var x in vector)
                    {
                        max += x > 0 ? x : 0;
                        min += x < 0 ? x : 0;
                    }

                    var nonZeroValues = vals
                        .ToArray()
                        .Select((x, i) => (Val: x, Coordinates: adjacentHiddenCoordinates[i]))
                        .Where(x => x.Val != 0);

                    // All of the negative numbers in that row are mines and all of the positive values in that row are not mines
                    if (final == min)
                    {
                        turns.AddRange(
                            nonZeroValues.Select(x => new Turn(x.Coordinates.X, x.Coordinates.Y, x.Val > 0 ? TileOperation.Reveal : TileOperation.Flag)));
                    }

                    // All of the negative numbers in that row are not mines and all of the positive values in that row are mines.
                    if (final == max)
                    {
                        turns.AddRange(
                            nonZeroValues.Select(x => new Turn(x.Coordinates.X, x.Coordinates.Y, x.Val > 0 ? TileOperation.Flag : TileOperation.Reveal)));
                    }
                }
            }

            return turns
                .Distinct()
                //.OrderByDescending(x => x.Operation) // questionable optimization: Flag before Reveal
                .ToList();

            // in the time gap between renting and returning this matrix, it's possible another
            // matrix with the same row/column count was added
            //_matrices.TryAdd(key, matrix);
        }
    }
}

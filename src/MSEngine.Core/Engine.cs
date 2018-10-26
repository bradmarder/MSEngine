﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MSEngine.Core
{
    public static class Engine
    {
        /// <summary>
        /// Generates an 8x8 board with 10 mines
        /// </summary>
        /// <returns></returns>
        public static Board GenerateRandomBeginnerBoard() => GenerateRandomBoard(8, 8, 10);

        /// <summary>
        /// Generates a 16x16 board with 40 mines
        /// </summary>
        /// <returns></returns>
        public static Board GenerateRandomIntermediateBoard() => GenerateRandomBoard(16, 16, 40);

        /// <summary>
        /// Generates a 30x16 board with 99 mines
        /// </summary>
        /// <returns></returns>
        public static Board GenerateRandomExpertBoard() => GenerateRandomBoard(30, 16, 99);

        /// <summary>
        /// Generates a random minesweeper board.
        /// </summary>
        /// <param name="columns">Max value of 30</param>
        /// <param name="rows">Max value of 16</param>
        /// <param name="mineCount">Must be less than tile count (columns * height)</param>
        /// <returns></returns>
        public static Board GenerateRandomBoard(byte columns, byte rows, byte mineCount) =>
            GenerateBoard(columns, rows, mineCount, Utilities.GetShuffledItems);

        /// <summary>
        /// Generates a minesweeper board.
        /// This is a "Potentially Pure" function.
        /// This implies that if the shuffler function provided is pure, then this function is pure as well.
        /// </summary>
        /// <param name="columns">Max value of 30</param>
        /// <param name="rows">Max value of 16</param>
        /// <param name="mineCount">Must be less than tile count (columns * height)</param>
        /// <param name="shuffler"></param>
        /// <returns></returns>
        public static Board GenerateBoard(byte columns, byte rows, byte mineCount, Func<IEnumerable<Coordinates>, IEnumerable<Coordinates>> shuffler)
        {
            if (columns == byte.MinValue || columns > 30) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == byte.MinValue || rows > 16) { throw new ArgumentOutOfRangeException(nameof(rows)); }
            if (shuffler == null) { throw new ArgumentNullException(nameof(shuffler)); }

            // if we allowed tileCount == mineCount, then we would have an infinite loop attempting to generate a board
            // because logic dictates the first tile revealed must not be a mine
            var tileCount = columns * rows;
            if (mineCount >= tileCount) { throw new ArgumentOutOfRangeException(nameof(mineCount)); }

            var coordinates = GetCoordinates(columns, rows);
            var coordinatesToMineMap = shuffler(coordinates)

                // we use a select expression to get the index, since ToDictionary() does not give access to it
                .Select((x, i) => (Coordinates: x, Index: i))

                .ToDictionary(x => x.Coordinates, x => x.Index < mineCount);
            var coordinatesToAdjacentMineCountMap = coordinates.ToDictionary(
                x => x,
                x => (byte)coordinatesToMineMap.Count(y => y.Value && IsAdjacentTo(y.Key, x)));
            var tiles = coordinatesToMineMap.Select(x => new Tile(x.Key, x.Value, coordinatesToAdjacentMineCountMap[x.Key]));

            return new Board(tiles);
        }

        public static Board CalculateBoard(Board board, Queue<Turn> turns)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }
            if (turns == null) { throw new ArgumentNullException(nameof(turns)); }

            return turns.Aggregate(board, CalculateBoard);
        }

        public static Board CalculateBoard(Board board, Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }
            if (board.Status == BoardStatus.Completed || board.Status == BoardStatus.Failed)
            {
                throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
            }
            if (!board.Tiles.Any(x => x.Coordinates == turn.Coordinates))
            {
                throw new InvalidGameStateException("Turn has coordinates that are outside the board");
            }
            if (turn.Operation == TileOperation.Flag && board.FlagsAvailable == 0)
            {
                throw new InvalidGameStateException("No more flags available");
            }

            var targetTile = board.Tiles.Single(x => x.Coordinates == turn.Coordinates);
            if (targetTile.State == TileState.Revealed)
            {
                throw new InvalidGameStateException("Operations not allowed on revealed tiles");
            }

            if (turn.Operation == TileOperation.Flag || turn.Operation == TileOperation.RemoveFlag || (turn.Operation == TileOperation.Reveal && !targetTile.HasMine && targetTile.AdjacentMineCount > 0))
            {
                var tiles = board.Tiles.Select(x => x.Coordinates == targetTile.Coordinates ? new Tile(x, turn.Operation) : x);
                return new Board(tiles);
            }

            if (turn.Operation == TileOperation.Reveal)
            {
                return targetTile.HasMine
                    ? GetFailedBoard(board)
                    : GetChainReactionBoard(board, targetTile);
            }

            throw new NotImplementedException(turn.Operation.ToString());
        }

        /// <summary>
        /// A pure method which does not randomize mine location. Intended for testing purposes.
        /// </summary>
        internal static Board GeneratePureBoard(byte columns, byte rows, byte mineCount) =>
            GenerateBoard(columns, rows, mineCount, Enumerable.AsEnumerable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsAdjacentTo(Coordinates coordinateOne, Coordinates coordinateTwo)
        {
            var x = coordinateOne.X;
            var y = coordinateOne.Y;

            // filters ordered to prioritize short-circuiting
            return new[] { x, x + 1, x - 1 }.Contains(coordinateTwo.X)
                && new[] { y, y + 1, y - 1 }.Contains(coordinateTwo.Y)
                && coordinateOne != coordinateTwo;
        }

        internal static IEnumerable<Coordinates> GetCoordinates(byte rows, byte columns)
        {
            if (columns == byte.MinValue) { throw new ArgumentOutOfRangeException(nameof(columns)); }
            if (rows == byte.MinValue) { throw new ArgumentOutOfRangeException(nameof(rows)); }

            return Enumerable
                .Range(0, rows)
                .SelectMany(x => Enumerable
                    .Range(0, columns)
                    .Select(y => new Coordinates((byte)x, (byte)y)));
        }

        /// <summary>
        /// Reveals all unflagged tiles with mines
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        internal static Board GetFailedBoard(Board board)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var tiles = board.Tiles.Select(x =>
                x.State == TileState.Revealed || (x.State == TileState.Flagged && x.HasMine)
                ? x
                : new Tile(x, TileOperation.Reveal));

            return new Board(tiles);
        }

        internal static Board GetChainReactionBoard(Board board, Tile targetTile)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var unrevealedAdjacentTiles = board.Tiles

                // if an adjacent tile has a "false flag", it does not expand revealing
                .Where(x => x.State == TileState.Hidden)

                .Where(x => IsAdjacentTo(x.Coordinates, targetTile.Coordinates))
                .Where(x => x.AdjacentMineCount == 0);
            var expanding = new Queue<Tile>(unrevealedAdjacentTiles);
            var expandedCoordinates = new HashSet<Coordinates>();

            while (expanding.Any())
            {
                var tile = expanding.Dequeue();
                expandedCoordinates.Add(tile.Coordinates);

                if (tile.AdjacentMineCount > 0)
                {
                    continue;
                }

                // concat adjacent unflagged tiles to the queue
                board.Tiles
                    .Where(x => IsAdjacentTo(x.Coordinates, tile.Coordinates))
                    .Where(x => !expandedCoordinates.Contains(x.Coordinates))
                    .Where(x => x.State != TileState.Flagged)
                    .ToList()
                    .ForEach(expanding.Enqueue);
            }

            var tiles = board.Tiles.Select(x =>
                x.State != TileState.Revealed && expandedCoordinates.Contains(x.Coordinates)
                    ? new Tile(x, TileOperation.Reveal)
                    : x);

            return new Board(tiles);
        }
    }
}

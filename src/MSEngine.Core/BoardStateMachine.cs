using System;
using System.Linq;
using System.Collections.Generic;
using static MSEngine.Core.Utilities;

namespace MSEngine.Core
{
    public class BoardStateMachine : IBoardStateMachine
    {
        public static IBoardStateMachine Instance { get; } = new BoardStateMachine();

        public virtual void EnsureValidBoardConfiguration(Board board, Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var distinctCoordinateCount = board.Tiles
                .Select(x => x.Coordinates)
                .Distinct()
                .Count();
            if (board.Tiles.Length > distinctCoordinateCount)
            {
                throw new InvalidGameStateException("Multiple tiles have matching coordinates");
            }
            if (board.Status == BoardStatus.Completed || board.Status == BoardStatus.Failed)
            {
                throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
            }
            if (board.Tiles.All(x => x.Coordinates != turn.Coordinates))
            {
                throw new InvalidGameStateException("Turn has coordinates that are outside the board");
            }
            if (turn.Operation == TileOperation.Flag && board.FlagsAvailable == 0)
            {
                throw new InvalidGameStateException("No more flags available");
            }

            var targetTile = board.Tiles.First(x => x.Coordinates == turn.Coordinates);

            if (targetTile.State == TileState.Revealed && turn.Operation != TileOperation.Chord)
            {
                throw new InvalidGameStateException("Only chord operations are allowed on revealed tiles");
            }
            if (targetTile.State == TileState.Flagged && turn.Operation == TileOperation.Flag)
            {
                throw new InvalidGameStateException("May not flag a tile that is already flagged");
            }
            if (turn.Operation == TileOperation.RemoveFlag && targetTile.State != TileState.Flagged)
            {
                throw new InvalidGameStateException("Impossible to remove flag from un-flagged tile");
            }
            if (turn.Operation == TileOperation.Chord)
            {
                if (targetTile.State != TileState.Revealed)
                {
                    throw new InvalidGameStateException("May only chord a revealed tile");
                }
                if (targetTile.AdjacentMineCount == 0)
                {
                    throw new InvalidGameStateException("May only chord a tile that has adjacent mines");
                }
                var adjacentTiles = board.Tiles.Where(x => IsAdjacentTo(x.Coordinates, targetTile.Coordinates));
                var targetTileAdjacentFlagCount = adjacentTiles.Count(x => x.State == TileState.Flagged);
                var targetTileAdjacentHiddenCount = adjacentTiles.Count(x => x.State == TileState.Hidden);

                if (targetTile.AdjacentMineCount != targetTileAdjacentFlagCount)
                {
                    throw new InvalidGameStateException("May only chord a tile when adjacent mine count equals adjacent tile flag count");
                }
                if (targetTileAdjacentHiddenCount == 0)
                {
                    throw new InvalidGameStateException("May only chord a tile that has hidden adjacent tiles");
                }
            }
        }
        public virtual Board ComputeBoard(Board board, IEnumerable<Turn> turns)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }
            if (turns == null) { throw new ArgumentNullException(nameof(turns)); }

            return turns.Aggregate(board, ComputeBoard);
        }
        public virtual Board ComputeBoard(Board board, Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var targetTile = board.Tiles.First(x => x.Coordinates == turn.Coordinates);

            // these cases will only affect a single tile
            if (turn.Operation == TileOperation.Flag || turn.Operation == TileOperation.RemoveFlag || (turn.Operation == TileOperation.Reveal && !targetTile.HasMine && targetTile.AdjacentMineCount > 0))
            {
                var tiles = board.Tiles.Select(x => x.Coordinates == targetTile.Coordinates ? new Tile(x, turn.Operation) : x);
                return new Board(tiles);
            }

            if (turn.Operation == TileOperation.Reveal)
            {
                return targetTile.HasMine
                    ? GetFailedBoard(board)
                    : GetChainReactionBoard(board, targetTile.Coordinates);
            }

            if (turn.Operation == TileOperation.Chord)
            {
                return GetChordBoard(board, targetTile.Coordinates);
            }

            throw new NotImplementedException(turn.Operation.ToString());
        }

        internal static Board GetFailedBoard(Board board)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            // should we show false flags?
            var tiles = board.Tiles.Select(x =>
                !x.HasMine || x.State == TileState.Revealed || x.State == TileState.Flagged
                    ? x
                    : new Tile(x, TileOperation.Reveal));

            return new Board(tiles);
        }
        internal static Board GetChainReactionBoard(Board board, Coordinates coordinates)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var unrevealedAdjacentTiles = board.Tiles

                // if an adjacent tile has a "false flag", it does not expand revealing
                .Where(x => x.State == TileState.Hidden)

                .Where(x => x.AdjacentMineCount == 0)
                .Where(x => x.Coordinates == coordinates || IsAdjacentTo(x.Coordinates, coordinates));
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

                board.Tiles
                    .Where(x => x.State != TileState.Flagged)
                    .Where(x => !expandedCoordinates.Contains(x.Coordinates))
                    .Where(x => IsAdjacentTo(x.Coordinates, tile.Coordinates))
                    .ToList()
                    .ForEach(expanding.Enqueue);
            }

            var tiles = board.Tiles.Select(x =>
                x.State != TileState.Revealed && expandedCoordinates.Contains(x.Coordinates)
                    ? new Tile(x, TileOperation.Reveal)
                    : x);

            return new Board(tiles);
        }

        internal Board GetChordBoard(Board board, Coordinates coordinates)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            return board.Tiles
                .Where(x => x.State == TileState.Hidden)
                .Where(x => IsAdjacentTo(x.Coordinates, coordinates))
                .Select(x => new Turn(x.Coordinates, TileOperation.Reveal))
                .Aggregate(board, ComputeBoard);
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using static MSEngine.Core.Utilities;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class BoardStateMachine : IBoardStateMachine
    {
        public static IBoardStateMachine Instance { get; } = new BoardStateMachine();

        public static Tile GetTargetTile(ReadOnlySpan<Tile> tiles, Coordinates coordinates)
        {
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.Coordinates == coordinates)
                {
                    return tile;
                }
            }
            throw new InvalidOperationException("A turn should always match a tile");
        }

        public virtual void EnsureValidBoardConfiguration(ReadOnlySpan<Tile> tiles, Turn turn)
        {
            var linqTiles = tiles.ToArray();

            var distinctCoordinateCount = linqTiles
                .Select(x => x.Coordinates)
                .Distinct()
                .Count();
            if (tiles.Length > distinctCoordinateCount)
            {
                throw new InvalidGameStateException("Multiple tiles have matching coordinates");
            }
            if (tiles.Status() == BoardStatus.Completed || tiles.Status() == BoardStatus.Failed)
            {
                throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
            }
            if (linqTiles.All(x => x.Coordinates != turn.Coordinates))
            {
                throw new InvalidGameStateException("Turn has coordinates that are outside the board");
            }
            if (turn.Operation == TileOperation.Flag && tiles.FlagsAvailable() == 0)
            {
                throw new InvalidGameStateException("No more flags available");
            }

            var targetTile = linqTiles.Single(x => x.Coordinates == turn.Coordinates);
            if (targetTile.State == TileState.Revealed && turn.Operation != TileOperation.Chord && turn.Operation != TileOperation.Reveal)
            {
                throw new InvalidGameStateException("Only chord/reveal operations are allowed on revealed tiles");
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
                var adjacentTiles = linqTiles.Where(x => IsAdjacentTo(x.Coordinates, targetTile.Coordinates));
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
        public virtual void ComputeBoard(Span<Tile> tiles, ReadOnlySpan<Turn> turns)
        {
            foreach (var x in turns)
            {
                ComputeBoard(tiles, x);
            }
        }
        public virtual void ComputeBoard(Span<Tile> tiles, Turn turn)
        {
            if (!Enum.IsDefined(typeof(TileOperation), turn.Operation))
            {
                throw new NotImplementedException(turn.Operation.ToString());
            }

            var targetTile = GetTargetTile(tiles, turn.Coordinates);

            // If a tile is already revealed, we return instead of throwing an exception
            // This is because the solver generates batches of turns at a time, and any turn
            // may trigger a chain reaction and auto-reveal other tiles
            if (targetTile.State == TileState.Revealed && turn.Operation == TileOperation.Reveal)
            {
                return;
            }

            // these cases will only affect a single tile
            if (turn.Operation == TileOperation.Flag || turn.Operation == TileOperation.RemoveFlag || (turn.Operation == TileOperation.Reveal && !targetTile.HasMine && targetTile.AdjacentMineCount > 0))
            {

                for (int i = 0, l = tiles.Length; i < l; i++)
                {
                    var tile = tiles[i];
                    if (tile.Coordinates == targetTile.Coordinates)
                    {
                        tiles[i] = new Tile(tile, turn.Operation);
                    }
                }
                return;
            }

            if (turn.Operation == TileOperation.Reveal)
            {
                if (targetTile.HasMine)
                {
                    GetFailedBoard(tiles);
                }
                else
                {
                    GetChainReactionBoard(tiles, targetTile.Coordinates);
                }
                return;
            }

            if (turn.Operation == TileOperation.Chord)
            {
                GetChordBoard(tiles, targetTile.Coordinates);
                return;
            }
        }

        internal static void GetFailedBoard(Span<Tile> tiles)
        {
            // should we show false flags?
            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.HasMine && tile.State == TileState.Hidden)
                {
                    tiles[i] = new Tile(tile, TileOperation.Reveal);
                }
            }
        }

        internal static void GetChainReactionBoard(Span<Tile> tiles, Coordinates coordinates)
        {
            var linqTiles = tiles.ToArray();
            var unrevealedAdjacentTiles = linqTiles

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

                linqTiles
                    .Where(x => x.State != TileState.Flagged)
                    .Where(x => !expandedCoordinates.Contains(x.Coordinates))
                    .Where(x => IsAdjacentTo(x.Coordinates, tile.Coordinates))
                    .ToList()
                    .ForEach(expanding.Enqueue);
            }

            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State != TileState.Revealed && expandedCoordinates.Contains(tile.Coordinates))
                {
                    tiles[i] = new Tile(tile, TileOperation.Reveal);
                }
            }
        }

        internal void GetChordBoard(Span<Tile> tiles, Coordinates coordinates)
        {
            var turns = tiles
                .ToArray()
                .Where(x => x.State == TileState.Hidden)
                .Where(x => IsAdjacentTo(x.Coordinates, coordinates))
                .Select(x => new Turn(x.Coordinates, TileOperation.Reveal))
                .ToArray()
                .AsSpan();

            ComputeBoard(tiles, turns);
        }
    }
}

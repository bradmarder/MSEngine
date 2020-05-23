using System;
using System.Linq;
using static MSEngine.Core.Utilities;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class BoardStateMachine : IBoardStateMachine
    {
        public static IBoardStateMachine Instance { get; } = new BoardStateMachine();

        public virtual void EnsureValidBoardConfiguration(ReadOnlySpan<Tile> tiles, Turn turn)
        {
            if (tiles.Status() == BoardStatus.Completed || tiles.Status() == BoardStatus.Failed)
            {
                throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
            }
            if (turn.TileIndex > tiles.Length)
            {
                throw new InvalidGameStateException("Turn has index outside the matrix");
            }
            if (turn.Operation == TileOperation.Flag && tiles.FlagsAvailable() == 0)
            {
                throw new InvalidGameStateException("No more flags available");
            }

            var targetTile = tiles[turn.TileIndex];
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

                var targetTileAdjacentFlagCount = 0;
                var targetTileAdjacentHiddenCount = 0;
                Span<int> adjacentIndexes = stackalloc int[8];
                adjacentIndexes.FillAdjacentTileIndexes(tiles.Length, turn.TileIndex, 8);
                foreach(var i in adjacentIndexes)
                {
                    var tile = tiles[i];
                    if (tile.State == TileState.Flagged) { targetTileAdjacentFlagCount++; }
                    if (tile.State == TileState.Hidden) { targetTileAdjacentHiddenCount++; }
                }

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
            var tile = tiles[turn.TileIndex];

            // If a tile is already revealed, we return instead of throwing an exception
            // This is because the solver generates batches of turns at a time, and any turn
            // may trigger a chain reaction and auto-reveal other tiles
            if (tile.State == TileState.Revealed && turn.Operation == TileOperation.Reveal)
            {
                return;
            }

            // these cases will only affect a single tile
            if (turn.Operation == TileOperation.Flag || turn.Operation == TileOperation.RemoveFlag || (turn.Operation == TileOperation.Reveal && !tile.HasMine && tile.AdjacentMineCount > 0))
            {
                tiles[turn.TileIndex] = new Tile(tile, turn.Operation);
                return;
            }

            if (turn.Operation == TileOperation.Reveal)
            {
                if (tile.HasMine)
                {
                    GetFailedBoard(tiles);
                }
                else
                {
                    GetChainReactionBoard(tiles, turn.TileIndex);
                }
                return;
            }

            if (turn.Operation == TileOperation.Chord)
            {
                GetChordBoard(tiles, turn.TileIndex);
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

        internal static void GetChainReactionBoard(Span<Tile> tiles, int tileIndex)
        {
            Span<Tile> chainTiles = stackalloc Tile[byte.MaxValue];
            Span<Coordinates> chainCoordinates = stackalloc Coordinates[byte.MaxValue];
            var chainCoorCount = 0;
            var chainTileCount = 0;

            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];

                // if an adjacent tile has a "false flag", it does not expand revealing
                if (tile.State == TileState.Hidden

                    && tile.AdjacentMineCount == 0
                    && (tile.Coordinates == coordinates || IsAdjacentTo(tile.Coordinates, coordinates)))
                {
                    chainTiles[chainTileCount] = tile;
                    chainTileCount++;
                }
            }

            while (chainTileCount > 0)
            {
                var tile = chainTiles[chainTileCount - 1];
                chainTileCount--;

                chainCoordinates[chainCoorCount] = tile.Coordinates;
                chainCoorCount++;

                if (tile.AdjacentMineCount > 0)
                {
                    continue;
                }

                for (int i = 0, l = tiles.Length; i < l; i++)
                {
                    var x = tiles[i];
                    if (x.State != TileState.Flagged
                        && chainCoordinates.Slice(0, chainCoorCount).IndexOf(x.Coordinates) == -1
                        && IsAdjacentTo(x.Coordinates, tile.Coordinates))
                    {
                        chainTiles[chainTileCount] = x;
                        chainTileCount++;
                    }
                }
            }

            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                var tile = tiles[i];
                if (tile.State != TileState.Revealed && chainCoordinates.Slice(0, chainCoorCount).IndexOf(tile.Coordinates) != -1)
                {
                    tiles[i] = new Tile(tile, TileOperation.Reveal);
                }
            }
        }

        internal void GetChordBoard(Span<Tile> tiles, int tileIndex)
        {
            Debug.Assert(tileIndex >= 0);
            Debug.Assert(tileIndex < tiles.Length);

            var turnCount = 0;
            Span<Turn> turns = stackalloc Turn[8];
            Span<int> adjacentIndexes = stackalloc int[8];

            adjacentIndexes.FillAdjacentTileIndexes(tiles.Length, tileIndex, 8);

            foreach (var i in adjacentIndexes)
            {
                if (i == -1) { continue; }

                if (tiles[i].State == TileState.Hidden)
                {
                    turns[turnCount] = new Turn(i, TileOperation.Reveal);
                    turnCount++;
                }
            }

            ComputeBoard(tiles, turns);
        }
    }
}

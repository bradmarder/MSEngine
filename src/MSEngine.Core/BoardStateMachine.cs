using System;
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
                tiles[turn.TileIndex] = new Tile(tile.HasMine, tile.AdjacentMineCount, turn.Operation);
                return;
            }

            if (turn.Operation == TileOperation.Reveal)
            {
                if (tile.HasMine)
                {
                    FailBoard(tiles);
                }
                else
                {
                    ChainReaction(tiles, turn.TileIndex);
                }
                return;
            }

            if (turn.Operation == TileOperation.Chord)
            {
                Chord(tiles, turn.TileIndex);
                return;
            }
        }

        internal static void FailBoard(Span<Tile> tiles)
        {
            // should we show false flags?
            foreach (ref var tile in tiles)
            {
                if (tile.HasMine && tile.State == TileState.Hidden)
                {
                    tile = new Tile(tile.HasMine, tile.AdjacentMineCount, TileOperation.Reveal);
                }
            }
        }

        internal static void ChainReaction(Span<Tile> tiles, int tileIndex)
        {
            Debug.Assert(tileIndex >= 0);

            Span<int> visitedIndexes = stackalloc int[tiles.Length];
            Span<int> revealIndexes = stackalloc int[tiles.Length];
            Span<int> adjacentIndexes = stackalloc int[8];

            visitedIndexes.Fill(-1);
            revealIndexes.Fill(-1);

            var visitedIndexCount = 0;
            var revealIndexCount = 0;

            // TESTING
            revealIndexes[revealIndexCount] = tileIndex;
            revealIndexCount++;

            VisitTile(tiles, adjacentIndexes, visitedIndexes, revealIndexes, tileIndex, ref visitedIndexCount, ref revealIndexCount);

            for (int i = 0, l = tiles.Length; i < l; i++)
            {
                if (revealIndexes.IndexOf(i) != -1)
                {
                    var tile = tiles[i];
                    tiles[i] = new Tile(tile.HasMine, tile.AdjacentMineCount, TileOperation.Reveal);
                }
            }
        }

        // we recursively visit tiles
        internal static void VisitTile(
            ReadOnlySpan<Tile> tiles,
            Span<int> adjacentIndexes,
            Span<int> visitedIndexes,
            Span<int> revealIndexes,
            int tileIndex,
            ref int visitedIndexCount,
            ref int revealIndexCount)
        {
            // Console.WriteLine("VISIT NODE = " + tileIndex);
            const int columnCount = 8;
            adjacentIndexes.FillAdjacentTileIndexes(tiles.Length, tileIndex, columnCount);

            visitedIndexes[visitedIndexCount] = tileIndex;
            visitedIndexCount++;

            foreach (var index in adjacentIndexes)
            {
                if (index == -1) { continue; }
                if (revealIndexes.IndexOf(index) != -1) { continue; }

                var tile = tiles[index];

                // if an adjacent tile has a "false flag", it does not expand revealing
                if (tile.State == TileState.Hidden)
                {
                    revealIndexes[revealIndexCount] = index;
                    revealIndexCount++;

                    if (tile.AdjacentMineCount == 0 && visitedIndexes.IndexOf(index) == -1)
                    {
                        VisitTile(tiles, adjacentIndexes, visitedIndexes, revealIndexes, index, ref visitedIndexCount, ref revealIndexCount);
                    }
                }
            }
        }

        internal void Chord(Span<Tile> tiles, int tileIndex)
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
using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardComputationTest
    {
        [Fact]
        public void Zero_mine_count_with_one_reveal_completes_board()
        {
            Span<Tile> tiles = stackalloc Tile[8 * 8];
            Engine.PureInstance.FillCustomBoard(tiles, 8, 8, 0);
            var turn = new Turn(0, 0, TileOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(tiles, turn);

            Assert.Equal(BoardStatus.Completed, tiles.Status());
        }

        [Fact]
        public void All_mines_revealed_if_game_fails()
        {
            Span<Tile> tiles = stackalloc Tile[2 * 2];
            Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
            BoardStateMachine.GetFailedBoard(tiles);
            var allTilesRevealed = tiles.ToArray().All(x => !x.HasMine || x.State == TileState.Flagged || x.State == TileState.Revealed);

            Assert.True(allTilesRevealed);
        }

        [Fact]
        public void Flagging_tile_only_flags_single_tile()
        {
            Span<Tile> tiles = stackalloc Tile[2 * 2];
            Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
            var board = tiles.ToArray();
            var origin = new Coordinates(0, 0);
            var turn = new Turn(origin, TileOperation.Flag);
            BoardStateMachine.Instance.ComputeBoard(tiles, turn);
            var tile = tiles.ToArray().Single(x => x.Coordinates == origin);
            var everyOtherTileHasNotChanged = tiles.ToArray().All(x => x.Equals(tile) || board.Contains(x));

            Assert.Equal(TileState.Flagged, tile.State);
            Assert.True(everyOtherTileHasNotChanged);
        }

        [Fact]
        public void Revealing_tile_with_no_mine_and_has_adjacent_mines_only_reveals_single_tile()
        {
            Span<Tile> tiles = stackalloc Tile[2 * 2];
            Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
            var board = tiles.ToArray();
            var origin = new Coordinates(1, 0);
            var turn = new Turn(origin, TileOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(tiles, turn);
            var tile = tiles.ToArray().Single(x => x.Coordinates == origin);
            var everyOtherTileHasNotChanged = tiles.ToArray().All(x => x.Equals(tile) || board.Contains(x));

            Assert.Equal(TileState.Revealed, tile.State);
            Assert.True(everyOtherTileHasNotChanged);
        }

        [Fact]
        public void Chording_tile_reveals_surrounding_tiles()
        {
            Span<Tile> tiles = stackalloc Tile[2 * 2];
            Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
            Span<Turn> turns = stackalloc Turn[3]
            {
                new Turn(0, 0, TileOperation.Flag),
                new Turn(1, 1, TileOperation.Reveal),
                new Turn(1, 1, TileOperation.Chord)
            };
            BoardStateMachine.Instance.ComputeBoard(tiles, turns);

            // 0,0 has the only mine, so we flag it
            // revealing 1,1 shows a 1
            // chording 1,1 should reveal 0,1 and 1,0 - thus completing the board
            Assert.Equal(BoardStatus.Completed, tiles.Status());
        }

        [Fact]
        public void Revealing_tile_without_mine_and_zero_adjacent_mines_triggers_chain_reaction()
        {
            Span<Tile> tiles = stackalloc Tile[3 * 3];
            Engine.PureInstance.FillCustomBoard(tiles, 3, 3, 1);
            var targetTile = tiles.ToArray().Single(x => x.Coordinates.X == 2 && x.Coordinates.Y == 2);
            var firstTurn = new Turn(2, 2, TileOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);
            var mineTile = tiles.ToArray().Single(x => x.Coordinates.X == 0 && x.Coordinates.Y == 0);
            var everyTileOtherThanMineTile = tiles.ToArray().Where(x => x.Coordinates != mineTile.Coordinates);

            Assert.False(targetTile.HasMine);
            Assert.Equal(0, targetTile.AdjacentMineCount);
            Assert.True(mineTile.HasMine);
            Assert.True(everyTileOtherThanMineTile.All(x => x.State == TileState.Revealed));
        }

        [Fact]
        public void Chain_reaction_is_blocked_by_false_flag()
        {
            Span<Tile> tiles = stackalloc Tile[5 * 1];
            Engine.PureInstance.FillCustomBoard(tiles, 5, 1, 0);
            Span<Turn> turns = stackalloc Turn[2]
            {
                new Turn(2, 0, TileOperation.Flag),
                new Turn(4, 0, TileOperation.Reveal)
            };
            BoardStateMachine.Instance.ComputeBoard(tiles, turns);
            var firstTwoTilesAreHidden = tiles
                .ToArray()
                .Where(x => x.Coordinates.X < 2)
                .All(x => x.State == TileState.Hidden);
            var lastTwoTilesAreRevealed = tiles
                .ToArray()
                .Where(x => x.Coordinates.X > 2)
                .All(x => x.State == TileState.Revealed);
            var middleTile = tiles.ToArray().Single(x => x.Coordinates.X == 2);

            Assert.Equal(TileState.Flagged, middleTile.State);
            Assert.Equal(BoardStatus.Pending, tiles.Status());
            Assert.True(firstTwoTilesAreHidden);
            Assert.True(lastTwoTilesAreRevealed);
        }
    }
}

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
            var turn = new Turn(0, TileOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(tiles, turn);

            Assert.Equal(BoardStatus.Completed, tiles.Status());
        }

        [Fact]
        public void All_mines_revealed_if_game_fails()
        {
            Span<Tile> tiles = stackalloc Tile[2 * 2];
            Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
            BoardStateMachine.FailBoard(tiles);
            var allTilesRevealed = tiles.ToArray().All(x => !x.HasMine || x.State == TileState.Flagged || x.State == TileState.Revealed);

            Assert.True(allTilesRevealed);
        }

        [Fact]
        public void Flagging_tile_only_flags_single_tile()
        {
            Span<Tile> tiles = stackalloc Tile[2 * 2];
            Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
            var board = tiles.ToArray();
            var turn = new Turn(0, TileOperation.Flag);
            BoardStateMachine.Instance.ComputeBoard(tiles, turn);
            var tile = tiles[0];
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
            var turn = new Turn(0, TileOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(tiles, turn);
            var tile = tiles[0];
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
                new Turn(0, TileOperation.Flag),
                new Turn(3, TileOperation.Reveal),
                new Turn(3, TileOperation.Chord)
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
            var targetTile = tiles[8];
            var firstTurn = new Turn(8, TileOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);

            Assert.False(targetTile.HasMine);
            Assert.Equal(0, targetTile.AdjacentMineCount);
            Assert.True(tiles[0].HasMine);

            for (var i = 0; i < tiles.Length; i++)
            {
                if (i == 0) { continue; }
                Assert.Equal(TileState.Revealed, tiles[i].State);
            }
        }

        [Fact]
        public void Chain_reaction_is_blocked_by_false_flag()
        {
            Span<Tile> tiles = stackalloc Tile[5 * 1];
            Engine.PureInstance.FillCustomBoard(tiles, 5, 1, 0);
            Span<Turn> turns = stackalloc Turn[2]
            {
                new Turn(2, TileOperation.Flag),
                new Turn(4, TileOperation.Reveal)
            };
            BoardStateMachine.Instance.ComputeBoard(tiles, turns);

            for (var i = 0; i < tiles.Length; i++)
            {
                var nodeState = tiles[i].State;
                var expectedState = i == 2 ? TileState.Flagged
                    : i < 2 ? TileState.Hidden
                    : TileState.Revealed;

                Assert.Equal(expectedState, nodeState);
            }
            Assert.Equal(BoardStatus.Pending, tiles.Status());
        }
    }
}

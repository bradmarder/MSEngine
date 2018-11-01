using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardComputationTest
    {
        [Fact]
        public void Zero_mine_count_with_one_reveal_completes_board()
        {
            var board = Engine.GeneratePureBoard(8, 8, 0);
            var turn = new Turn(0, 0, TileOperation.Reveal);
            var completedBoard = Engine.ComputeBoard(board, turn);

            Assert.Equal(BoardStatus.Completed, completedBoard.Status);
        }

        [Fact]
        public void All_mines_revealed_if_game_fails()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var fail = Engine.GetFailedBoard(board);
            var allTilesRevealed = fail.Tiles.All(x => !x.HasMine || x.State == TileState.Flagged || x.State == TileState.Revealed);

            Assert.True(allTilesRevealed);
        }

        [Fact]
        public void Flagging_tile_only_flags_single_tile()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var origin = new Coordinates(0, 0);
            var turn = new Turn(origin, TileOperation.Flag);
            var fin = Engine.ComputeBoard(board, turn);
            var tile = fin.Tiles.Single(x => x.Coordinates == origin);
            var everyOtherTileHasNotChanged = fin.Tiles.All(x => x.Equals(tile) || board.Tiles.Contains(x));

            Assert.Equal(TileState.Flagged, tile.State);
            Assert.True(everyOtherTileHasNotChanged);
        }

        [Fact]
        public void Revealing_tile_with_no_mine_and_has_adjacent_mines_only_reveals_single_tile()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var origin = new Coordinates(1, 0);
            var turn = new Turn(origin, TileOperation.Reveal);
            var fin = Engine.ComputeBoard(board, turn);
            var tile = fin.Tiles.Single(x => x.Coordinates == origin);
            var everyOtherTileHasNotChanged = fin.Tiles.All(x => x.Equals(tile) || board.Tiles.Contains(x));

            Assert.Equal(TileState.Revealed, tile.State);
            Assert.True(everyOtherTileHasNotChanged);
        }

        [Fact]
        public void Chording_tile_reveals_surrounding_tiles()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var firstTurn = new Turn(0, 0, TileOperation.Flag);
            var secondTurn = new Turn(1, 1, TileOperation.Reveal);
            var thirdTurn = new Turn(1, 1, TileOperation.Chord);
            var turns = new Queue<Turn>(new[] { firstTurn, secondTurn, thirdTurn });
            var finalBoard = Engine.ComputeBoard(board, turns);

            // 0,0 has the only mine, so we flag it
            // revealing 1,1 shows a 1
            // chording 1,1 should reveal 0,1 and 1,0 - thus completing the board
            Assert.Equal(BoardStatus.Completed, finalBoard.Status);
        }

        [Fact]
        public void Revealing_tile_without_mine_and_zero_adjacent_mines_triggers_chain_reaction()
        {
            var board = Engine.GeneratePureBoard(3, 3, 1);
            var targetTile = board.Tiles.Single(x => x.Coordinates.X == 2 && x.Coordinates.Y == 2);
            var firstTurn = new Turn(2, 2, TileOperation.Reveal);
            var finalBoard = Engine.ComputeBoard(board, firstTurn);
            var mineTile = finalBoard.Tiles.Single(x => x.Coordinates.X == 0 && x.Coordinates.Y == 0);
            var everyTileOtherThanMineTile = finalBoard.Tiles.Where(x => x.Coordinates != mineTile.Coordinates);

            Assert.False(targetTile.HasMine);
            Assert.Equal(0, targetTile.AdjacentMineCount);
            Assert.True(mineTile.HasMine);
            Assert.True(everyTileOtherThanMineTile.All(x => x.State == TileState.Revealed));
        }

        [Fact]
        public void Chain_reaction_is_blocked_by_false_flag()
        {
            var board = Engine.GeneratePureBoard(5, 1, 0);
            var firstTurn = new Turn(2, 0, TileOperation.Flag);
            var secondTurn = new Turn(4, 0, TileOperation.Reveal);
            var turns = new Queue<Turn>(new[] { firstTurn, secondTurn });
            var finalBoard = Engine.ComputeBoard(board, turns);
            var firstTwoTilesAreHidden = finalBoard.Tiles
                .Where(x => x.Coordinates.X < 2)
                .All(x => x.State == TileState.Hidden);
            var lastTwoTilesAreRevealed = finalBoard.Tiles
                .Where(x => x.Coordinates.X > 2)
                .All(x => x.State == TileState.Revealed);
            var middleTile = finalBoard.Tiles.Single(x => x.Coordinates.X == 2);

            Assert.Equal(TileState.Flagged, middleTile.State);
            Assert.Equal(BoardStatus.Pending, finalBoard.Status);
            Assert.True(firstTwoTilesAreHidden);
            Assert.True(lastTwoTilesAreRevealed);
        }
    }
}

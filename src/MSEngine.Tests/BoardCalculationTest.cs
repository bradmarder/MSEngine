using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardCalculationTest
    {
        [Fact]
        public void Zero_mine_count_with_one_reveal_completes_board()
        {
            var board = Engine.GeneratePureBoard(8, 8, 0);
            var turn = new Turn(0, 0, TileOperation.Reveal);
            var completedBoard = Engine.CalculateBoard(board, turn);

            Assert.Equal(BoardStatus.Completed, completedBoard.Status);
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_any_operation_applied_after_game_ends(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(8, 8, 0);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var boardAfterFirstTurn = Engine.CalculateBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Completed, boardAfterFirstTurn.Status);

            var secondTurn = new Turn(0, 1, operation);

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_any_operation_applied_on_revealed_tile(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var turns = new Queue<Turn>();
            turns.Enqueue(new Turn(1, 1, TileOperation.Reveal));
            turns.Enqueue(new Turn(1, 1, operation));

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(board, turns));
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            var board = Engine.GeneratePureBoard(1, 1, 0);
            var turn = new Turn(1, 0, TileOperation.Reveal);

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_no_flags_available()
        {
            var board = Engine.GeneratePureBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Flag);

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(board, turn));
        }

        [Fact]
        public void All_tiles_revealed_if_game_fails()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var fail = Engine.GetFailedBoard(board);
            var allTilesRevealed = fail.Tiles.All(x => (x.HasMine && x.State == TileState.Flagged) || x.State == TileState.Revealed);

            Assert.True(allTilesRevealed);
        }

        [Theory]
        [InlineData(TileOperation.Flag, TileState.Flagged)]
        [InlineData(TileOperation.RemoveFlag, TileState.Hidden)]
        public void Flagging_tile_only_flags_single_tile(TileOperation operation, TileState state)
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var origin = new Coordinates(0, 0);
            var turn = new Turn(origin, operation);
            var fin = Engine.CalculateBoard(board, turn);
            var tile = fin.Tiles.Single(x => x.Coordinates == origin);
            var everyOtherTileHasNotChanged = fin.Tiles.All(x => x.Equals(tile) || board.Tiles.Contains(x));

            Assert.Equal(state, tile.State);
            Assert.True(everyOtherTileHasNotChanged);
        }

        [Fact]
        public void Revealing_tile_with_no_mine_and_has_adjacent_mines_only_reveals_single_tile()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var origin = new Coordinates(1, 0);
            var turn = new Turn(origin, TileOperation.Reveal);
            var fin = Engine.CalculateBoard(board, turn);
            var tile = fin.Tiles.Single(x => x.Coordinates == origin);
            var everyOtherTileHasNotChanged = fin.Tiles.All(x => x.Equals(tile) || board.Tiles.Contains(x));

            Assert.Equal(TileState.Revealed, tile.State);
            Assert.True(everyOtherTileHasNotChanged);
        }
    }
}

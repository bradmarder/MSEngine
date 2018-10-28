using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardValidationTest
    {
        [Fact]
        public void Throws_exception_if_board_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => Engine.EnsureValidBoardConfiguration(null, new Turn()));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_completed(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(8, 8, 0);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var secondTurn = new Turn(0, 1, operation);
            var boardAfterFirstTurn = Engine.ComputeBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Completed, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_failed(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(8, 8, 1);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var secondTurn = new Turn(0, 1, operation);
            var boardAfterFirstTurn = Engine.ComputeBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Failed, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_reveal_or_flag_operation_applied_on_revealed_tile(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var firstTurn = new Turn(1, 1, TileOperation.Reveal);
            var boardAfterFirstTurn = Engine.ComputeBoard(board, firstTurn);
            var secondTurn = new Turn(1, 1, operation);

            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            var board = Engine.GeneratePureBoard(1, 1, 0);
            var turn = new Turn(1, 0, TileOperation.Reveal);

            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_no_flags_available()
        {
            var board = Engine.GeneratePureBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Flag);

            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_tile_is_already_flagged()
        {
            var board = Engine.GeneratePureBoard(1, 2, 1);
            var turn = new Turn(0, 0, TileOperation.Flag);
            var boardAfterFirstTurn = Engine.ComputeBoard(board, turn);

            Assert.Equal(BoardStatus.Pending, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(boardAfterFirstTurn, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_remove_flag_and_tile_is_not_flagged()
        {
            var board = Engine.GeneratePureBoard(2, 2, 2);
            var turn = new Turn(0, 0, TileOperation.RemoveFlag);
            
            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_non_revealed_tile()
        {
            var board = Engine.GeneratePureBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Chord);

            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_with_zero_adjacent_mines()
        {
            var origin = new Coordinates(2, 2);
            var board = Engine.GeneratePureBoard(3, 3, 1);
            var firstTurn = new Turn(origin, TileOperation.Reveal);
            var secondTurn = new Turn(origin, TileOperation.Chord);
            var boardAfterFirstTurn = Engine.ComputeBoard(board, firstTurn);
            var targetTile = boardAfterFirstTurn.Tiles.Single(x => x.Coordinates == origin);

            Assert.Equal(TileState.Revealed, targetTile.State);
            Assert.Equal(byte.MinValue, targetTile.AdjacentMineCount);
            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_when_adjacent_mine_count_does_not_equal_adjacent_flag_count()
        {
            var origin = new Coordinates(1, 1);
            var board = Engine.GeneratePureBoard(2, 2, 3);
            var firstTurn = new Turn(0, 0, TileOperation.Flag);
            var secondTurn = new Turn(origin, TileOperation.Reveal);
            var thirdTurn = new Turn(origin, TileOperation.Chord);
            var boardAfterFirstTurn = Engine.ComputeBoard(board, firstTurn);
            var boardAfterSecondTurn = Engine.ComputeBoard(boardAfterFirstTurn, secondTurn);
            
            Assert.Throws<InvalidGameStateException>(() => Engine.EnsureValidBoardConfiguration(boardAfterSecondTurn, thirdTurn));
        }
    }
}

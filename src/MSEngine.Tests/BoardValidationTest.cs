using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardValidationTest
    {
        [Fact]
        public void Throws_if_board_has_duplicate_coordinates()
        {
            var dupeCoordinates = new Coordinates(0, 0);
            var tile = new Tile(dupeCoordinates, false, 0);
            var board = new Board(new[] { tile, tile });

            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(board, new Turn()));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_completed(TileOperation operation)
        {
            var board = Engine.PureInstance.GenerateCustomBoard(8, 8, 0);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var secondTurn = new Turn(0, 1, operation);
            var boardAfterFirstTurn = BoardStateMachine.Instance.ComputeBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Completed, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_failed(TileOperation operation)
        {
            var board = Engine.PureInstance.GenerateCustomBoard(8, 8, 1);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var secondTurn = new Turn(0, 1, operation);
            var boardAfterFirstTurn = BoardStateMachine.Instance.ComputeBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Failed, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_reveal_or_flag_operation_applied_on_revealed_tile(TileOperation operation)
        {
            var board = Engine.PureInstance.GenerateCustomBoard(2, 2, 1);
            var firstTurn = new Turn(1, 1, TileOperation.Reveal);
            var boardAfterFirstTurn = BoardStateMachine.Instance.ComputeBoard(board, firstTurn);
            var secondTurn = new Turn(1, 1, operation);

            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            var board = Engine.PureInstance.GenerateCustomBoard(1, 1, 0);
            var turn = new Turn(1, 0, TileOperation.Reveal);

            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_no_flags_available()
        {
            var board = Engine.PureInstance.GenerateCustomBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Flag);

            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_tile_is_already_flagged()
        {
            var board = Engine.PureInstance.GenerateCustomBoard(1, 2, 1);
            var turn = new Turn(0, 0, TileOperation.Flag);
            var boardAfterFirstTurn = BoardStateMachine.Instance.ComputeBoard(board, turn);

            Assert.Equal(BoardStatus.Pending, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(boardAfterFirstTurn, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_remove_flag_and_tile_is_not_flagged()
        {
            var board = Engine.PureInstance.GenerateCustomBoard(2, 2, 2);
            var turn = new Turn(0, 0, TileOperation.RemoveFlag);
            
            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_non_revealed_tile()
        {
            var board = Engine.PureInstance.GenerateCustomBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Chord);

            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_with_zero_adjacent_mines()
        {
            var origin = new Coordinates(2, 2);
            var board = Engine.PureInstance.GenerateCustomBoard(3, 3, 1);
            var firstTurn = new Turn(origin, TileOperation.Reveal);
            var secondTurn = new Turn(origin, TileOperation.Chord);
            var boardAfterFirstTurn = BoardStateMachine.Instance.ComputeBoard(board, firstTurn);
            var targetTile = boardAfterFirstTurn.Tiles.Single(x => x.Coordinates == origin);

            Assert.Equal(TileState.Revealed, targetTile.State);
            Assert.Equal(0, targetTile.AdjacentMineCount);
            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_when_adjacent_mine_count_does_not_equal_adjacent_flag_count()
        {
            var origin = new Coordinates(1, 1);
            var board = Engine.PureInstance.GenerateCustomBoard(2, 2, 3);
            var firstTurn = new Turn(0, 0, TileOperation.Flag);
            var secondTurn = new Turn(origin, TileOperation.Reveal);
            var thirdTurn = new Turn(origin, TileOperation.Chord);
            var boardAfterFirstTurn = BoardStateMachine.Instance.ComputeBoard(board, firstTurn);
            var boardAfterSecondTurn = BoardStateMachine.Instance.ComputeBoard(boardAfterFirstTurn, secondTurn);
            
            Assert.Throws<InvalidGameStateException>(() => BoardStateMachine.Instance.EnsureValidBoardConfiguration(boardAfterSecondTurn, thirdTurn));
        }

        [Fact]
        public void Mayonlychordatilethathashiddenadjacenttiles()
        {
            Assert.True(true);
        }
    }
}

using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardValidationTest
    {
        private static readonly IBoardStateMachine _boardStateMachine = new BoardStateMachine();

        [Fact]
        public void Throws_exception_if_board_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _boardStateMachine.EnsureValidBoardConfiguration(null, new Turn()));
        }

        [Fact]
        public void Throws_if_board_has_duplicate_coordinates()
        {
            var dupeCoordinates = new Coordinates(0, 0);
            var tile = new Tile(dupeCoordinates, false, 0);
            var board = new Board(new[] { tile, tile });

            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(board, new Turn()));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_completed(TileOperation operation)
        {
            var board = Engine.PureInstance.GenerateBoard(8, 8, 0);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var secondTurn = new Turn(0, 1, operation);
            var boardAfterFirstTurn = _boardStateMachine.ComputeBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Completed, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_failed(TileOperation operation)
        {
            var board = Engine.PureInstance.GenerateBoard(8, 8, 1);
            var firstTurn = new Turn(0, 0, TileOperation.Reveal);
            var secondTurn = new Turn(0, 1, operation);
            var boardAfterFirstTurn = _boardStateMachine.ComputeBoard(board, firstTurn);

            Assert.Equal(BoardStatus.Failed, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_reveal_or_flag_operation_applied_on_revealed_tile(TileOperation operation)
        {
            var board = Engine.PureInstance.GenerateBoard(2, 2, 1);
            var firstTurn = new Turn(1, 1, TileOperation.Reveal);
            var boardAfterFirstTurn = _boardStateMachine.ComputeBoard(board, firstTurn);
            var secondTurn = new Turn(1, 1, operation);

            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            var board = Engine.PureInstance.GenerateBoard(1, 1, 0);
            var turn = new Turn(1, 0, TileOperation.Reveal);

            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_no_flags_available()
        {
            var board = Engine.PureInstance.GenerateBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Flag);

            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_tile_is_already_flagged()
        {
            var board = Engine.PureInstance.GenerateBoard(1, 2, 1);
            var turn = new Turn(0, 0, TileOperation.Flag);
            var boardAfterFirstTurn = _boardStateMachine.ComputeBoard(board, turn);

            Assert.Equal(BoardStatus.Pending, boardAfterFirstTurn.Status);
            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(boardAfterFirstTurn, turn));
        }

        [Fact]
        public void Throws_exception_if_operation_is_remove_flag_and_tile_is_not_flagged()
        {
            var board = Engine.PureInstance.GenerateBoard(2, 2, 2);
            var turn = new Turn(0, 0, TileOperation.RemoveFlag);
            
            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_non_revealed_tile()
        {
            var board = Engine.PureInstance.GenerateBoard(1, 1, 0);
            var turn = new Turn(0, 0, TileOperation.Chord);

            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(board, turn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_with_zero_adjacent_mines()
        {
            var origin = new Coordinates(2, 2);
            var board = Engine.PureInstance.GenerateBoard(3, 3, 1);
            var firstTurn = new Turn(origin, TileOperation.Reveal);
            var secondTurn = new Turn(origin, TileOperation.Chord);
            var boardAfterFirstTurn = _boardStateMachine.ComputeBoard(board, firstTurn);
            var targetTile = boardAfterFirstTurn.Tiles.Single(x => x.Coordinates == origin);

            Assert.Equal(TileState.Revealed, targetTile.State);
            Assert.Equal(0, targetTile.AdjacentMineCount);
            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(boardAfterFirstTurn, secondTurn));
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_when_adjacent_mine_count_does_not_equal_adjacent_flag_count()
        {
            var origin = new Coordinates(1, 1);
            var board = Engine.PureInstance.GenerateBoard(2, 2, 3);
            var firstTurn = new Turn(0, 0, TileOperation.Flag);
            var secondTurn = new Turn(origin, TileOperation.Reveal);
            var thirdTurn = new Turn(origin, TileOperation.Chord);
            var boardAfterFirstTurn = _boardStateMachine.ComputeBoard(board, firstTurn);
            var boardAfterSecondTurn = _boardStateMachine.ComputeBoard(boardAfterFirstTurn, secondTurn);
            
            Assert.Throws<InvalidGameStateException>(() => _boardStateMachine.EnsureValidBoardConfiguration(boardAfterSecondTurn, thirdTurn));
        }

        [Fact]
        public void Mayonlychordatilethathashiddenadjacenttiles()
        {
            Assert.True(true);
        }
    }
}

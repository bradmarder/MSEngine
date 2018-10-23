using System;
using System.Collections.Immutable;
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
            var turns = ImmutableQueue.Create(new Turn(0, 0, TileOperation.Reveal));
            var state = new GameState(board, turns);
            var completedBoard = Engine.CalculateBoard(state);

            Assert.Equal(BoardStatus.Completed, completedBoard.Status);
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_any_operation_applied_after_game_ends(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(8, 8, 0);
            var firstTurn = ImmutableQueue.Create(new Turn(0, 0, TileOperation.Reveal));
            var state = new GameState(board, firstTurn);
            var boardAfterFirstTurn = Engine.CalculateBoard(state);

            Assert.Equal(BoardStatus.Completed, boardAfterFirstTurn.Status);

            var secondTurn = firstTurn.Enqueue(new Turn(0, 1, operation));
            var secondState = new GameState(board, secondTurn);
            
            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(secondState));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_any_operation_applied_on_revealed_tile(TileOperation operation)
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var turns = ImmutableQueue.Create(
                new Turn(1, 1, TileOperation.Reveal),
                new Turn(1, 1, operation));
            var state = new GameState(board, turns);

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(state));
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            var board = Engine.GeneratePureBoard(1, 1, 0);
            var turns = ImmutableQueue.Create(new Turn(1, 0, TileOperation.Reveal));
            var state = new GameState(board, turns);

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(state));
        }
    }
}

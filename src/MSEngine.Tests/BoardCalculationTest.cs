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
            var board = Engine.GenerateRandomBoard(8, 8, 0);
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
            var board = Engine.GenerateRandomBoard(8, 8, 0);
            var turns = ImmutableQueue.Create(
                new Turn(0, 0, TileOperation.Reveal),
                new Turn(0, 1, operation));
            var state = new GameState(board, turns);
            
            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(state));
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_any_operation_applied_on_revealed_tile(TileOperation operation)
        {
            var board = Engine.GenerateRandomBeginnerBoard();
            var turns = ImmutableQueue.Create(
                new Turn(0, 0, TileOperation.Reveal),
                new Turn(0, 0, operation));
            var state = new GameState(board, turns);

            Assert.Throws<InvalidGameStateException>(() => Engine.CalculateBoard(state));
        }
    }
}

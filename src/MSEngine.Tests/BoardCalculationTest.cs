using System;
using System.Linq;
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

        [Fact]
        public void All_tiles_revealed_if_game_fails()
        {
            var board = Engine.GeneratePureBoard(2, 2, 1);
            var turn = new Turn(0, 0, TileOperation.Reveal);
            var fail = Engine.CalculateBoard(board, turn);

            Assert.Equal(BoardStatus.Failed, fail.Status);

            var allTilesRevealed = fail.Tiles.All(x => (x.HasMine && x.State == TileState.Flagged) || x.State == TileState.Revealed);
            Assert.True(allTilesRevealed);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(2, 1)]
        [InlineData(2, 3)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        public void Coordinates_are_adjacent(byte x, byte y)
        {
            var origin = new Coordinates(2, 2);
            var coordindates = new Coordinates(x, y);
            var isAdjacent = Engine.IsAdjacentTo(origin, coordindates);

            Assert.True(Engine.IsAdjacentTo(origin, coordindates));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(0, 3)]
        [InlineData(0, 4)]
        [InlineData(1, 0)]
        [InlineData(1, 4)]
        [InlineData(2, 0)]
        [InlineData(2, 2)]
        [InlineData(2, 4)]
        [InlineData(3, 0)]
        [InlineData(3, 4)]
        [InlineData(4, 0)]
        [InlineData(4, 1)]
        [InlineData(4, 2)]
        [InlineData(4, 3)]
        [InlineData(4, 4)]
        public void Coordinates_are_not_adjacent(byte x, byte y)
        {
            var origin = new Coordinates(2, 2);
            var coordindates = new Coordinates(x, y);
            var isAdjacent = Engine.IsAdjacentTo(origin, coordindates);

            Assert.False(Engine.IsAdjacentTo(origin, coordindates));
        }
    }
}

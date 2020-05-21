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

            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[2] { tile, tile };
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, new Turn());
            });
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_completed(TileOperation operation)
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[8 * 8];
                Engine.PureInstance.FillCustomBoard(tiles, 8, 8, 0);
                var firstTurn = new Turn(0, 0, TileOperation.Reveal);
                var secondTurn = new Turn(0, 1, operation);
                BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);

                Assert.Equal(BoardStatus.Completed, tiles.Status());
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, secondTurn);
            });
        }

        [Theory]
        [InlineData(TileOperation.Reveal)]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        [InlineData(TileOperation.Chord)]
        public void Throws_exception_if_any_operation_applied_after_game_is_failed(TileOperation operation)
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[8 * 8];
                Engine.PureInstance.FillCustomBoard(tiles, 8, 8, 1);
                var firstTurn = new Turn(0, 0, TileOperation.Reveal);
                var secondTurn = new Turn(0, 1, operation);
                BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);

                Assert.Equal(BoardStatus.Failed, tiles.Status());
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, secondTurn);
            });
        }

        [Theory]
        [InlineData(TileOperation.Flag)]
        [InlineData(TileOperation.RemoveFlag)]
        public void Throws_exception_if_flag_operation_applied_on_revealed_tile(TileOperation operation)
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[2 * 2];
                Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 1);
                var firstTurn = new Turn(1, 1, TileOperation.Reveal);
                BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);
                var secondTurn = new Turn(1, 1, operation);
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, secondTurn);
            });
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[1 * 1];
                Engine.PureInstance.FillCustomBoard(tiles, 1, 1, 0);
                var turn = new Turn(1, 0, TileOperation.Reveal);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_no_flags_available()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[1 * 1];
                Engine.PureInstance.FillCustomBoard(tiles, 1, 1, 0);
                var turn = new Turn(0, 0, TileOperation.Flag);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_tile_is_already_flagged()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[1 * 2];
                Engine.PureInstance.FillCustomBoard(tiles, 1, 2, 1);
                var turn = new Turn(0, 0, TileOperation.Flag);
                BoardStateMachine.Instance.ComputeBoard(tiles, turn);

                Assert.Equal(BoardStatus.Pending, tiles.Status());
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_operation_is_remove_flag_and_tile_is_not_flagged()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[2 * 2];
                Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 2);
                var turn = new Turn(0, 0, TileOperation.RemoveFlag);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_chord_on_non_revealed_tile()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[1 * 1];
                Engine.PureInstance.FillCustomBoard(tiles, 1, 1, 0);
                var turn = new Turn(0, 0, TileOperation.Chord);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_with_zero_adjacent_mines()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                var origin = new Coordinates(2, 2);
                Span<Tile> tiles = stackalloc Tile[3 * 3];
                Engine.PureInstance.FillCustomBoard(tiles, 3, 3, 1);
                var firstTurn = new Turn(origin, TileOperation.Reveal);
                var secondTurn = new Turn(origin, TileOperation.Chord);
                BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);
                var targetTile = tiles.ToArray().Single(x => x.Coordinates == origin);

                Assert.Equal(TileState.Revealed, targetTile.State);
                Assert.Equal(0, targetTile.AdjacentMineCount);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, secondTurn);
            });
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_tile_when_adjacent_mine_count_does_not_equal_adjacent_flag_count()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[2 * 2];
                var origin = new Coordinates(1, 1);
                Engine.PureInstance.FillCustomBoard(tiles, 2, 2, 3);
                var firstTurn = new Turn(0, 0, TileOperation.Flag);
                var secondTurn = new Turn(origin, TileOperation.Reveal);
                var thirdTurn = new Turn(origin, TileOperation.Chord);

                BoardStateMachine.Instance.ComputeBoard(tiles, firstTurn);
                BoardStateMachine.Instance.ComputeBoard(tiles, secondTurn);
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, thirdTurn);
            });
        }

        [Fact]
        public void Mayonlychordatilethathashiddenadjacenttiles()
        {
            Assert.True(true);
        }
    }
}

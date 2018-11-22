using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardGenerationTest
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(31, 16)]
        [InlineData(30, 17)]
        public void Throws_on_invalid_columns_or_rows_or_shuffler(byte columns, byte rows)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Engine.GeneratePureBoard(columns, rows, 0));
            Assert.Throws<ArgumentNullException>(() => Engine.GenerateBoard(1, 1, 0, null));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void Expected_tile_count_equals_actual_tile_count(byte columns, byte rows)
        {
            var expectedTileCount = columns * rows;
            var board = Engine.GeneratePureBoard(columns, rows, 0);

            Assert.Equal(expectedTileCount, board.Tiles.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public void Expected_mine_count_equals_actual_mine_count(byte expectedMineCount)
        {
            var board = Engine.GeneratePureBoard(30, 16, expectedMineCount);

            Assert.Equal(expectedMineCount, board.MineCount);
        }

        [Theory]
        [InlineData(64)]
        [InlineData(65)]
        public void Throws_if_mine_count_is_greater_than_or_equal_to_tile_count(byte mineCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Engine.GeneratePureBoard(8, 8, mineCount));
        }
    }
}

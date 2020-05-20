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
        public void Throws_on_invalid_columns(byte columns, byte rows)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[columns * rows];
                Engine.PureInstance.GenerateCustomBoard(tiles, columns, rows, 0);
            });
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void ThrowsOnInsufficientBuffer(byte columns, byte rows)
        {
            var tileCount = columns * rows;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[tileCount - 1];
                Engine.PureInstance.GenerateCustomBoard(tiles, columns, rows, 0);
            });
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void ThrowsOnOverallocatedBuffer(byte columns, byte rows)
        {
            var tileCount = columns * rows;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[tileCount + 1];
                Engine.PureInstance.GenerateCustomBoard(tiles, columns, rows, 0);
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public void Expected_mine_count_equals_actual_mine_count(byte expectedMineCount)
        {
            byte columns = 30;
            byte rows = 16;
            Span<Tile> tiles = stackalloc Tile[columns * rows];
            Engine.PureInstance.GenerateCustomBoard(tiles, columns, rows, expectedMineCount);

            Assert.Equal(expectedMineCount, tiles.MineCount());
        }

        [Theory]
        [InlineData(64)]
        [InlineData(65)]
        public void Throws_if_mine_count_is_greater_than_or_equal_to_tile_count(byte mineCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Tile> tiles = stackalloc Tile[64];
                Engine.PureInstance.GenerateCustomBoard(tiles, 8, 8, mineCount);
            });
        }
    }
}

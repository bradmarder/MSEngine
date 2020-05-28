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
        public void Throws_on_invalid_columns(byte columns, byte rows)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Node> nodes = stackalloc Node[columns * rows];
                Engine.Instance.FillCustomBoard(nodes, Span<int>.Empty, columns, rows);
            });
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void ThrowsOnInsufficientBuffer(byte columns, byte rows)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Node> nodes = stackalloc Node[columns * rows - 1];
                Engine.Instance.FillCustomBoard(nodes, Span<int>.Empty, columns, rows);
            });
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void ThrowsOnOverallocatedBuffer(byte columns, byte rows)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Node> nodes = stackalloc Node[columns * rows + 1];
                Engine.Instance.FillCustomBoard(nodes, Span<int>.Empty, columns, rows);
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(63)]
        public void Expected_mine_count_equals_actual_mine_count(byte expectedMineCount)
        {
            Span<Node> nodes = stackalloc Node[64];
            Span<int> mines = stackalloc int[expectedMineCount];
            mines.Scatter(nodes.Length);

            Engine.Instance.FillCustomBoard(nodes, mines, 8, 8);

            Assert.Equal(expectedMineCount, nodes.MineCount());
        }

        [Fact]
        public void Throws_if_mine_count_is_greater_than_or_equal_to_node_count()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Span<Node> nodes = stackalloc Node[64];
                Span<int> mines = stackalloc int[nodes.Length + 1];

                Engine.Instance.FillCustomBoard(nodes, mines, 8, 8);
            });
        }

        [Fact]
        public void ScatteringMinesProducesZeroDuplicates()
        {
            Span<int> mines = stackalloc int[10];
            mines.Scatter(11);
            var distinctCount = mines.ToArray().Distinct().Count();

            Assert.Equal(10, distinctCount);
        }

        [Fact]
        public void ScatteringMinesProducesValidIndexes()
        {
            Span<int> mines = stackalloc int[10];
            mines.Scatter(10);

            foreach (var x in mines)
            {
                Assert.True(x > -1);
                Assert.True(x < 10);
            }
        }
    }
}

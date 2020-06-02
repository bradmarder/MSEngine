using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardGenerationTest
    {
        [Theory(Skip = "Figure out why AppVeyor runs tests in release instead of debug")]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public void Throws_on_invalid_columns(byte columns, byte rows)
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                Span<Node> nodes = stackalloc Node[columns * rows];
                Engine.Instance.FillCustomBoard(nodes, Span<int>.Empty, columns);
            });
        }

        [Theory(Skip = "Figure out why AppVeyor runs tests in release instead of debug")]
        [InlineData(1, 1)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void ThrowsOnInsufficientBuffer(byte columns, byte rows)
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                Span<Node> nodes = stackalloc Node[columns * rows - 1];
                Engine.Instance.FillCustomBoard(nodes, Span<int>.Empty, columns);
            });
        }

        /// <summary>
        /// verifies that nodes.Length % columns == 0
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        [Theory(Skip = "Figure out why AppVeyor runs tests in release instead of debug")]
        [InlineData(2, 2)]
        [InlineData(8, 8)]
        [InlineData(16, 16)]
        [InlineData(30, 16)]
        public void ThrowsOnInvalidBuffer(byte columns, byte rows)
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                Span<Node> nodes = stackalloc Node[(columns * rows) + 1];
                Engine.Instance.FillCustomBoard(nodes, Span<int>.Empty, columns);
            });
        }

        [Fact(Skip = "Figure out why AppVeyor runs tests in release instead of debug")]
        public void Throws_if_mine_count_is_greater_than_or_equal_to_node_count()
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                Span<Node> nodes = stackalloc Node[64];
                Span<int> mines = stackalloc int[nodes.Length + 1];

                Engine.Instance.FillCustomBoard(nodes, mines, 8);
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

            Engine.Instance.FillCustomBoard(nodes, mines, 8);

            Assert.Equal(expectedMineCount, nodes.MineCount());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ScatteringMinesProducesZeroDuplicates(int mineCount)
        {
            Span<int> mines = stackalloc int[mineCount];

            mines.Scatter(mineCount);

            var distinctCount = mines.ToArray().Distinct().Count();
            Assert.Equal(mineCount, distinctCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ScatteringMinesProducesInRangeIndexes(int nodeCount)
        {
            Span<int> mines = stackalloc int[nodeCount];

            mines.Scatter(nodeCount);

            foreach (var i in mines)
            {
                Assert.InRange(i, 0, nodeCount - 1);
            }
        }

        /// <summary>
        /// Silly test, but makes sure the mine randomizer is actually doing it's job
        /// Will fail once every 100c10 iterations!
        /// </summary>
        [Fact]
        public void ScatteringMinesAreRandomized()
        {
            Span<int> setOne = stackalloc int[10];
            Span<int> setTwo = stackalloc int[10];

            setOne.Scatter(100);
            setTwo.Scatter(100);

            Assert.NotEqual(
                setOne.ToArray().OrderBy(x => x),
                setTwo.ToArray().OrderBy(x => x));
        }
    }
}

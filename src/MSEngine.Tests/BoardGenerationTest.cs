using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardGenerationTest
    {
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

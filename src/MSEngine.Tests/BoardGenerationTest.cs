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
        public void ExpectedMineCountEqualsActualMineCount(byte expectedMineCount)
        {
            Span<Node> nodes = stackalloc Node[64];
            Span<int> mines = stackalloc int[expectedMineCount];
            Utilities.ScatterMines(mines, nodes.Length);

            Engine.FillCustomBoard(nodes, mines, 8);

            Assert.Equal(expectedMineCount, nodes.MineCount());
        }

        // 5x5 grid, always use max amount of mines
        // we test the 4 corners and the center
        [Theory]
        [InlineData(0, 21, new int[] { 0, 1, 5, 6 })]
        [InlineData(4, 21, new int[] { 3, 4, 8, 9 })]
        [InlineData(20, 21, new int[] { 15, 16, 20, 21 })]
        [InlineData(24, 21, new int[] { 18, 19, 23, 24 })]
        [InlineData(12, 16, new int[] { 6, 7, 8, 11, 12, 13, 16, 17, 18 })]
        public void ScatteringMinesConsidersSafeNodeIndex(int safeNodeIndex, int mineCount, int[] safeNodeIndexes)
        {
            Span<int> mines = stackalloc int[mineCount];

            Utilities.ScatterMines(mines, 25, safeNodeIndex, 5);

            Assert.True(mines.IndexOfAny(safeNodeIndexes) == -1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ScatteringMinesProducesZeroDuplicates(int mineCount)
        {
            Span<int> mines = stackalloc int[mineCount];

            Utilities.ScatterMines(mines, mineCount);

            var distinctCount = mines.ToArray().Distinct().Count();
            Assert.Equal(mineCount, distinctCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ScatteringMinesProducesInRangeIndexes(int nodeCount)
        {
            Span<int> mines = stackalloc int[nodeCount];

            Utilities.ScatterMines(mines, nodeCount);

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

            Utilities.ScatterMines(setOne, 100);
            Utilities.ScatterMines(setTwo, 100);

            Assert.NotEqual(
                setOne.ToArray().OrderBy(x => x),
                setTwo.ToArray().OrderBy(x => x));
        }
    }
}

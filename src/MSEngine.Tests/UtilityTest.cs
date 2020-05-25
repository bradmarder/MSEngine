using System;
using System.Linq;
using MSEngine.Core;
using Xunit;

namespace MSEngine.Tests
{
    public class UtilityTest
    {
        // node indexes of a 3x3 matrix
        // 0 1 2
        // 3 4 5
        // 6 7 8
        [Theory]
        [InlineData(0, new int[] { -1, -1, -1, -1, 1, -1, 3, 4 })]
        [InlineData(1, new int[] { -1, -1, -1, 0, 2, 3, 4, 5 })]
        [InlineData(2, new int[] { -1, -1, -1, 1, -1, 4, 5, -1 })]
        [InlineData(3, new int[] { -1, 0, 1, -1, 4, -1, 6, 7 })]
        [InlineData(4, new int[] { 0, 1, 2, 3, 5, 6, 7, 8 })]
        [InlineData(5, new int[] { 1, 2, -1, 4, -1, 7, 8, -1 })]
        [InlineData(6, new int[] { -1, 3, 4, -1, 7, -1, -1, -1 })]
        [InlineData(7, new int[] { 3, 4, 5, 6, 8, -1, -1, -1 })]
        [InlineData(8, new int[] { 4, 5, -1, 7, -1, -1, -1, -1 })]
        public void AdjacentNodeIndexesFilledCorrectly(int index, int[] expectedIndexes)
        {
            const int columnCount = 3;
            const int nodeCount = 9;
            Span<int> actualIndexes = stackalloc int[nodeCount -1];

            actualIndexes.FillAdjacentNodeIndexes(nodeCount, index, columnCount);

            Assert.Equal(expectedIndexes, actualIndexes.ToArray());
        }
    }
}

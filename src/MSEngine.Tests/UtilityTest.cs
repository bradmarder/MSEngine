using System;
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

        // 3x3 grid, all corners have a mine
        [Theory]
        [InlineData(1, 2)]
        [InlineData(3, 2)]
        [InlineData(4, 4)]
        [InlineData(5, 2)]
        [InlineData(7, 2)]
        public void GetAdjacentMineCounts(int nodeIndex, int expectedMineCount)
        {
            Span<int> mines = stackalloc int[] { 0, 2, 6, 8 };
            Span<int> buffer = stackalloc int[8];

            var actualMineCount = Utilities.GetAdjacentMineCount(mines, buffer, nodeIndex, 9, 3);

            Assert.Equal(expectedMineCount, actualMineCount);
        }

        // 3x3 grid, all corners have a flag
        [Theory]
        [InlineData(1, 2)]
        [InlineData(3, 2)]
        [InlineData(4, 4)]
        [InlineData(5, 2)]
        [InlineData(7, 2)]
        public void GetAdjacentFlaggedNodes(int nodeIndex, int expectedFlagCount)
        {
            Span<int> buffer = stackalloc int[8];
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(0, false, 0, NodeState.Flagged),
                new Node(1, false, 0, NodeState.Hidden),
                new Node(2, false, 0, NodeState.Flagged),
                new Node(3, false, 0, NodeState.Hidden),
                new Node(4, false, 0, NodeState.Hidden),
                new Node(5, false, 0, NodeState.Hidden),
                new Node(6, false, 0, NodeState.Flagged),
                new Node(7, false, 0, NodeState.Hidden),
                new Node(8, false, 0, NodeState.Flagged)
            };
            var matrix = new Matrix<Node>(nodes, 3);

            var actualFlagCount = Utilities.GetAdjacentFlaggedNodeCount(matrix, buffer, nodeIndex);

            Assert.Equal(expectedFlagCount, actualFlagCount);
        }

        // 3x3 grid, top row is hidden
        [Theory]
        [InlineData(3, true)]
        [InlineData(4, true)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        [InlineData(7, false)]
        [InlineData(8, false)]
        public void HasHiddenAdjacentNodes(int nodeIndex, bool expectedHasNodes)
        {
            Span<int> buffer = stackalloc int[8];
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(0, false, 0, NodeState.Hidden),
                new Node(1, false, 0, NodeState.Hidden),
                new Node(2, false, 0, NodeState.Hidden),
                new Node(3, false, 0, NodeState.Revealed),
                new Node(4, false, 0, NodeState.Revealed),
                new Node(5, false, 0, NodeState.Revealed),
                new Node(6, false, 0, NodeState.Revealed),
                new Node(7, false, 0, NodeState.Revealed),
                new Node(8, false, 0, NodeState.Revealed)
            };
            var matrix = new Matrix<Node>(nodes, 3);

            var actualHasNodes = Utilities.HasHiddenAdjacentNodes(matrix, buffer, nodeIndex);

            Assert.Equal(expectedHasNodes, actualHasNodes);
        }

        // 3x3 grid, middle node adjacent to all, corners never adjacent to each other
        [Theory]
        [InlineData(4, 0, true)]
        [InlineData(4, 1, true)]
        [InlineData(4, 2, true)]
        [InlineData(4, 3, true)]
        [InlineData(4, 5, true)]
        [InlineData(4, 6, true)]
        [InlineData(4, 7, true)]
        [InlineData(4, 8, true)]
        [InlineData(0, 2, false)]
        [InlineData(0, 6, false)]
        [InlineData(0, 8, false)]
        [InlineData(2, 6, false)]
        [InlineData(2, 8, false)]
        [InlineData(6, 8, false)]
        public void AreNodesAdjacent(int nodeIndexOne, int nodeIndexTwo, bool expected)
        {
            Span<int> buffer = stackalloc int[8];
            Span<Node> nodes = stackalloc Node[9];
            Engine.Instance.FillCustomBoard(nodes, 0, 3);

            var actual = Utilities.AreNodesAdjacent(buffer, 9, 3, nodeIndexOne, nodeIndexTwo);

            Assert.Equal(expected, actual);
        }
    }
}

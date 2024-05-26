using MSEngine.Solver;

namespace MSEngine.Tests;

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
		var actualIndexes = Utilities.GetAdjacentNodeIndexes(index).ToArray();

		Assert.Equal(expectedIndexes, actualIndexes);
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
		var matrix = new Matrix<Node>(stackalloc Node[9], 3);

		var actualMineCount = Utilities.GetAdjacentMineCount(mines, nodeIndex);

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
		Span<Node> nodes = stackalloc Node[]
		{
			new(0, false, 0, NodeState.Flagged),
			new(1, false, 0, NodeState.Hidden),
			new(2, false, 0, NodeState.Flagged),
			new(3, false, 0, NodeState.Hidden),
			new(4, false, 0, NodeState.Hidden),
			new(5, false, 0, NodeState.Hidden),
			new(6, false, 0, NodeState.Flagged),
			new(7, false, 0, NodeState.Hidden),
			new(8, false, 0, NodeState.Flagged),
		};
		var matrix = new Matrix<Node>(nodes, 3);

		var actualFlagCount = Utilities.GetAdjacentFlaggedNodeCount(matrix, nodeIndex);

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
		Span<Node> nodes = stackalloc Node[]
		{
			new(0, false, 0, NodeState.Hidden),
			new(1, false, 0, NodeState.Hidden),
			new(2, false, 0, NodeState.Hidden),
			new(3, false, 0, NodeState.Revealed),
			new(4, false, 0, NodeState.Revealed),
			new(5, false, 0, NodeState.Revealed),
			new(6, false, 0, NodeState.Revealed),
			new(7, false, 0, NodeState.Revealed),
			new(8, false, 0, NodeState.Revealed),
		};
		var matrix = new Matrix<Node>(nodes, 3);

		var actualHasNodes = Utilities.HasHiddenAdjacentNodes(matrix, nodeIndex);

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
		Span<Node> nodes = stackalloc Node[9];
		var matrix = new Matrix<Node>(stackalloc Node[9], 3);
		Engine.FillCustomBoard(matrix, Span<int>.Empty);

		//var actual = Utilities.AreNodesAdjacent(9, 3, nodeIndexOne, nodeIndexTwo);
		var actual = Utilities.AreNodesAdjacent(nodeIndexOne, nodeIndexTwo);

		Assert.Equal(expected, actual);
	}

	// 3x3 matrix, all 1's, then we zero'ify the middle column
	[Theory]
	[InlineData(0, 1)]
	[InlineData(1, 0)]
	[InlineData(2, 1)]
	[InlineData(3, 1)]
	[InlineData(4, 0)]
	[InlineData(5, 1)]
	[InlineData(6, 1)]
	[InlineData(7, 0)]
	[InlineData(8, 1)]
	public void MatrixColumnZeroification(int index, int expected)
	{
		Span<float> data = stackalloc float[9];
		data.Fill(1);
		var matrix = new Matrix<float>(data, 3);

		MatrixSolver.ZeroifyColumn(matrix, 1);

		Assert.Equal(expected, data[index]);
	}
}

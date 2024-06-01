using MSEngine.Solver;

namespace MSEngine.Tests;

public class SolverTest
{
	/// <summary>
	/// 01>_____
	/// 011_____
	/// 111_____
	/// __3_____
	/// ________
	/// ________
	/// ________
	/// ________
	/// </summary>
	[Fact]
	public void CalculatesTurnsForZeroAugmentColumnPriorToGaussianElimination()
	{
		const int nodeCount = 64;
		const int mineCount = 1;

		Span<Node> nodes = stackalloc Node[]
		{
			new(0, false, 0, NodeState.Revealed),
			new(1, false, 1, NodeState.Revealed),
			new(2, true, 0, NodeState.Flagged),
			new(3, false, 0, NodeState.Hidden),
			new(4, false, 0, NodeState.Hidden),
			new(5, false, 0, NodeState.Hidden),
			new(6, false, 0, NodeState.Hidden),
			new(7, false, 0, NodeState.Hidden),

			new(8, false, 0, NodeState.Revealed),
			new(9, false, 1, NodeState.Revealed),
			new(10, false, 1, NodeState.Revealed),
			new(11, false, 0, NodeState.Hidden),
			new(12, false, 0, NodeState.Hidden),
			new(13, false, 0, NodeState.Hidden),
			new(14, false, 0, NodeState.Hidden),
			new(15, false, 0, NodeState.Hidden),

			new(16, false, 1, NodeState.Revealed),
			new(17, false, 1, NodeState.Revealed),
			new(18, false, 1, NodeState.Revealed),
			new(19, false, 0, NodeState.Hidden),
			new(20, false, 0, NodeState.Hidden),
			new(21, false, 0, NodeState.Hidden),
			new(22, false, 0, NodeState.Hidden),
			new(23, false, 0, NodeState.Hidden),

			new(24, false, 0, NodeState.Hidden),
			new(25, false, 0, NodeState.Hidden),
			new(26, false, 3, NodeState.Revealed),
			new(27, false, 0, NodeState.Hidden),
			new(28, false, 0, NodeState.Hidden),
			new(29, false, 0, NodeState.Hidden),
			new(30, false, 0, NodeState.Hidden),
			new(31, false, 0, NodeState.Hidden),

			new(32, false, 0, NodeState.Hidden),
			new(33, false, 0, NodeState.Hidden),
			new(34, false, 0, NodeState.Hidden),
			new(35, false, 0, NodeState.Hidden),
			new(36, false, 0, NodeState.Hidden),
			new(37, false, 0, NodeState.Hidden),
			new(38, false, 0, NodeState.Hidden),
			new(39, false, 0, NodeState.Hidden),

			new(40, false, 0, NodeState.Hidden),
			new(41, false, 0, NodeState.Hidden),
			new(42, false, 0, NodeState.Hidden),
			new(43, false, 0, NodeState.Hidden),
			new(44, false, 0, NodeState.Hidden),
			new(45, false, 0, NodeState.Hidden),
			new(46, false, 0, NodeState.Hidden),
			new(47, false, 0, NodeState.Hidden),

			new(48, false, 0, NodeState.Hidden),
			new(49, false, 0, NodeState.Hidden),
			new(50, false, 0, NodeState.Hidden),
			new(51, false, 0, NodeState.Hidden),
			new(52, false, 0, NodeState.Hidden),
			new(53, false, 0, NodeState.Hidden),
			new(54, false, 0, NodeState.Hidden),
			new(55, false, 0, NodeState.Hidden),

			new(56, false, 0, NodeState.Hidden),
			new(57, false, 0, NodeState.Hidden),
			new(58, false, 0, NodeState.Hidden),
			new(59, false, 0, NodeState.Hidden),
			new(60, false, 0, NodeState.Hidden),
			new(61, false, 0, NodeState.Hidden),
			new(62, false, 0, NodeState.Hidden),
			new(63, false, 0, NodeState.Hidden),
		};

		var buffs = new BufferKeeper
		{
			Turns = stackalloc Turn[nodeCount],
			RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
			AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
		};

		var turnCount = MatrixSolver.CalculateTurns(nodes, buffs, false);

		Assert.Equal(3, turnCount);
		Assert.Equal(new Turn(3, NodeOperation.Reveal), buffs.Turns[0]);
		Assert.Equal(new Turn(11, NodeOperation.Reveal), buffs.Turns[1]);
		Assert.Equal(new Turn(19, NodeOperation.Reveal), buffs.Turns[2]);
	}

	/// <summary>
	/// 00001_10
	/// 11212_10
	/// 1>2>2110
	/// 22312_10
	/// 2>201_32
	/// 3>3112__  <-- these 2 hidden nodes should be flagged
	/// ________
	/// ________
	/// </summary>
	[Fact]
	public void CalculatesTurnsForVectorSumEqualsAugmentColumnPriorToGaussianElimination()
	{
		const int nodeCount = 64;
		const int mineCount = 4;

		Span<Node> nodes = stackalloc Node[]
		{
			new(0, false, 0, NodeState.Revealed),
			new(1, false, 0, NodeState.Revealed),
			new(2, false, 0, NodeState.Revealed),
			new(3, false, 0, NodeState.Revealed),
			new(4, false, 1, NodeState.Revealed),
			new(5, false, 0, NodeState.Hidden),
			new(6, false, 1, NodeState.Revealed),
			new(7, false, 0, NodeState.Revealed),

			new(8, false, 1, NodeState.Revealed),
			new(9, false, 1, NodeState.Revealed),
			new(10, false, 2, NodeState.Revealed),
			new(11, false, 1, NodeState.Revealed),
			new(12, false, 2, NodeState.Revealed),
			new(13, false, 0, NodeState.Hidden),
			new(14, false, 1, NodeState.Revealed),
			new(15, false, 0, NodeState.Revealed),

			new(16, false, 1, NodeState.Revealed),
			new(17, true, 0, NodeState.Flagged),
			new(18, false, 2, NodeState.Revealed),
			new(19, true, 0, NodeState.Flagged),
			new(20, false, 2, NodeState.Revealed),
			new(21, false, 1, NodeState.Revealed),
			new(22, false, 1, NodeState.Revealed),
			new(23, false, 0, NodeState.Revealed),

			new(24, false, 2, NodeState.Revealed),
			new(25, false, 2, NodeState.Revealed),
			new(26, false, 3, NodeState.Revealed),
			new(27, false, 1, NodeState.Revealed),
			new(28, false, 2, NodeState.Revealed),
			new(29, false, 0, NodeState.Hidden),
			new(30, false, 1, NodeState.Revealed),
			new(31, false, 0, NodeState.Revealed),

			new(32, false, 2, NodeState.Revealed),
			new(33, true, 0, NodeState.Flagged),
			new(34, false, 2, NodeState.Revealed),
			new(35, false, 0, NodeState.Revealed),
			new(36, false, 1, NodeState.Revealed),
			new(37, false, 0, NodeState.Hidden),
			new(38, false, 3, NodeState.Revealed),
			new(39, false, 2, NodeState.Revealed),

			new(40, false, 3, NodeState.Revealed),
			new(41, true, 0, NodeState.Flagged),
			new(42, false, 3, NodeState.Revealed),
			new(43, false, 1, NodeState.Revealed),
			new(44, false, 1, NodeState.Revealed),
			new(45, false, 2, NodeState.Revealed),
			new(46, false, 0, NodeState.Hidden),
			new(47, false, 0, NodeState.Hidden),

			new(48, false, 0, NodeState.Hidden),
			new(49, false, 0, NodeState.Hidden),
			new(50, false, 0, NodeState.Hidden),
			new(51, false, 0, NodeState.Hidden),
			new(52, false, 0, NodeState.Hidden),
			new(53, false, 0, NodeState.Hidden),
			new(54, false, 0, NodeState.Hidden),
			new(55, false, 0, NodeState.Hidden),

			new(56, false, 0, NodeState.Hidden),
			new(57, false, 0, NodeState.Hidden),
			new(58, false, 0, NodeState.Hidden),
			new(59, false, 0, NodeState.Hidden),
			new(60, false, 0, NodeState.Hidden),
			new(61, false, 0, NodeState.Hidden),
			new(62, false, 0, NodeState.Hidden),
			new(63, false, 0, NodeState.Hidden),
		};

		var buffs = new BufferKeeper
		{
			Turns = stackalloc Turn[nodeCount],
			RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
			AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
		};

		var turnCount = MatrixSolver.CalculateTurns(nodes, buffs, false);

		Assert.Equal(2, turnCount);
		Assert.Equal(new Turn(46, NodeOperation.Flag), buffs.Turns[0]);
		Assert.Equal(new Turn(47, NodeOperation.Flag), buffs.Turns[1]);
	}

	/// <summary>
	/// The reason we need float instead of sbyte/int for <see cref="Matrix{T}"/>
	/// </summary>
	[Fact]
	public void CalculatesTurnsWhenGaussianEliminationProducesNonIntegers()
	{
		const int nodeCount = 64;
		const int mineCount = 10;

		Span<Node> nodes = stackalloc Node[]
		{
			new(0, false, 1, NodeState.Revealed),
			new(1, false, 1, NodeState.Revealed),
			new(2, false, 1, NodeState.Revealed),
			new(3, true, 1, NodeState.Flagged),
			new(4, false, 2, NodeState.Revealed),
			new(5, false, 1, NodeState.Revealed),
			new(6, false, 1, NodeState.Revealed),
			new(7, false, 1, NodeState.Hidden),

			new(8, true, 1, NodeState.Flagged),
			new(9, false, 2, NodeState.Revealed),
			new(10, false, 3, NodeState.Revealed),
			new(11, false, 3, NodeState.Revealed),
			new(12, true, 2, NodeState.Hidden),
			new(13, false, 1, NodeState.Hidden),
			new(14, false, 1, NodeState.Revealed),
			new(15, true, 0, NodeState.Hidden),

			new(16, false, 2, NodeState.Revealed),
			new(17, true, 1, NodeState.Flagged),
			new(18, false, 2, NodeState.Revealed),
			new(19, true, 2, NodeState.Flagged),
			new(20, false, 3, NodeState.Hidden),
			new(21, false, 2, NodeState.Revealed),
			new(22, false, 1, NodeState.Revealed),
			new(23, false, 1, NodeState.Revealed),

			new(24, false, 1, NodeState.Revealed),
			new(25, false, 1, NodeState.Revealed),
			new(26, false, 3, NodeState.Revealed),
			new(27, false, 3, NodeState.Revealed),
			new(28, true, 2, NodeState.Hidden),
			new(29, false, 1, NodeState.Hidden),
			new(30, false, 0, NodeState.Hidden),
			new(31, false, 0, NodeState.Hidden),

			new(32, false, 0, NodeState.Revealed),
			new(33, false, 0, NodeState.Revealed),
			new(34, false, 1, NodeState.Revealed),
			new(35, true, 1, NodeState.Flagged),
			new(36, false, 2, NodeState.Revealed),
			new(37, false, 1, NodeState.Hidden),
			new(38, false, 0, NodeState.Hidden),
			new(39, false, 0, NodeState.Hidden),

			new(40, false, 0, NodeState.Revealed),
			new(41, false, 0, NodeState.Revealed),
			new(42, false, 1, NodeState.Revealed),
			new(43, false, 1, NodeState.Revealed),
			new(44, false, 2, NodeState.Revealed),
			new(45, false, 2, NodeState.Revealed),
			new(46, false, 2, NodeState.Hidden),
			new(47, false, 1, NodeState.Hidden),

			new(48, false, 0, NodeState.Revealed),
			new(49, false, 0, NodeState.Revealed),
			new(50, false, 0, NodeState.Revealed),
			new(51, false, 0, NodeState.Revealed),
			new(52, false, 1, NodeState.Revealed),
			new(53, true, 1, NodeState.Hidden),
			new(54, true, 1, NodeState.Hidden),
			new(55, false, 1, NodeState.Hidden),

			new(56, false, 0, NodeState.Revealed),
			new(57, false, 0, NodeState.Revealed),
			new(58, false, 0, NodeState.Revealed),
			new(59, false, 0, NodeState.Revealed),
			new(60, false, 1, NodeState.Revealed),
			new(61, false, 2, NodeState.Hidden),
			new(62, false, 2, NodeState.Hidden),
			new(63, false, 1, NodeState.Hidden),
		};

		var buffs = new BufferKeeper
		{
			Turns = stackalloc Turn[nodeCount],
			RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
			AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
		};

		var turnCount = MatrixSolver.CalculateTurns(nodes, buffs, false);

		Assert.Equal(2, turnCount);

		// changing float to sbyte will cause our solver to generate incorrect turns
		Assert.NotEqual(6, turnCount);

		Assert.Equal(new Turn(29, NodeOperation.Reveal), buffs.Turns[0]);
		Assert.Equal(new Turn(61, NodeOperation.Reveal), buffs.Turns[1]);
	}

	/// <summary>
	/// The intent of this test is to demonstrate that we must factor in all hidden nodes in
	/// the matrix in order to calculate any turns
	/// </summary>
	[Fact]
	public void CalculatesTurnsWhenFactoringAllHiddenNodes()
	{
		const int nodeCount = 64;
		const int mineCount = 10;

		Span<Node> nodes = stackalloc Node[]
		{
			new(0, false, 1, NodeState.Revealed),
			new(1, false, 1, NodeState.Revealed),
			new(2, false, 0, NodeState.Revealed),
			new(3, false, 0, NodeState.Revealed),
			new(4, false, 1, NodeState.Revealed),
			new(5, true, 0, NodeState.Flagged),
			new(6, false, 1, NodeState.Revealed),
			new(7, false, 0, NodeState.Revealed),

			new(8, true, 0, NodeState.Flagged),
			new(9, false, 2, NodeState.Revealed),
			new(10, false, 1, NodeState.Revealed),
			new(11, false, 2, NodeState.Revealed),
			new(12, false, 2, NodeState.Revealed),
			new(13, false, 2, NodeState.Revealed),
			new(14, false, 1, NodeState.Revealed),
			new(15, false, 0, NodeState.Revealed),

			new(16, false, 1, NodeState.Revealed),
			new(17, false, 2, NodeState.Revealed),
			new(18, true, 0, NodeState.Flagged),
			new(19, false, 2, NodeState.Revealed),
			new(20, true, 0, NodeState.Flagged),
			new(21, false, 2, NodeState.Revealed),
			new(22, false, 1, NodeState.Revealed),
			new(23, false, 1, NodeState.Revealed),

			new(24, false, 0, NodeState.Revealed),
			new(25, false, 1, NodeState.Revealed),
			new(26, false, 1, NodeState.Revealed),
			new(27, false, 3, NodeState.Revealed),
			new(28, false, 2, NodeState.Revealed),
			new(29, false, 3, NodeState.Revealed),
			new(30, true, 0, NodeState.Flagged),
			new(31, false, 1, NodeState.Revealed),

			new(32, false, 0, NodeState.Revealed),
			new(33, false, 1, NodeState.Revealed),
			new(34, false, 1, NodeState.Revealed),
			new(35, false, 2, NodeState.Revealed),
			new(36, true, 0, NodeState.Flagged),
			new(37, false, 2, NodeState.Revealed),
			new(38, false, 1, NodeState.Revealed),
			new(39, false, 1, NodeState.Revealed),

			new(40, false, 1, NodeState.Revealed),
			new(41, false, 2, NodeState.Revealed),
			new(42, true, 1, NodeState.Flagged),
			new(43, false, 2, NodeState.Revealed),
			new(44, false, 2, NodeState.Revealed),
			new(45, false, 2, NodeState.Revealed),
			new(46, false, 1, NodeState.Revealed),
			new(47, false, 0, NodeState.Revealed),

			new(48, false, 1, NodeState.Hidden),
			new(49, true, 1, NodeState.Hidden),
			new(50, false, 2, NodeState.Revealed),
			new(51, false, 1, NodeState.Revealed),
			new(52, false, 2, NodeState.Revealed),
			new(53, true, 1, NodeState.Flagged),
			new(54, false, 2, NodeState.Revealed),
			new(55, false, 0, NodeState.Revealed),

			new(56, false, 1, NodeState.Hidden),
			new(57, false, 1, NodeState.Hidden),
			new(58, false, 1, NodeState.Revealed),
			new(59, false, 0, NodeState.Revealed),
			new(60, false, 2, NodeState.Revealed),
			new(61, true, 1, NodeState.Flagged),
			new(62, false, 2, NodeState.Revealed),
			new(63, false, 0, NodeState.Revealed),
		};

		var buffs = new BufferKeeper
		{
			Turns = stackalloc Turn[nodeCount],
			RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
			AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
		};

		var partialHiddenNodeTurnCount = MatrixSolver.CalculateTurns(nodes, buffs, false);
		var fullHiddenNodeTurnCount = MatrixSolver.CalculateTurns(nodes, buffs, true);

		Assert.Equal(0, partialHiddenNodeTurnCount);
		Assert.Equal(2, fullHiddenNodeTurnCount);
		Assert.Equal(new Turn(56, NodeOperation.Reveal), buffs.Turns[0]);
		Assert.Equal(new Turn(57, NodeOperation.Reveal), buffs.Turns[1]);
	}
}

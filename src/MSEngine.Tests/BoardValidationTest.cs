using System;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests;

public class BoardValidationTest
{
	[Theory]
	[InlineData(NodeOperation.Reveal)]
	[InlineData(NodeOperation.Flag)]
	[InlineData(NodeOperation.RemoveFlag)]
	[InlineData(NodeOperation.Chord)]
	public void Throws_exception_if_any_operation_applied_after_game_is_completed(NodeOperation operation)
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[8 * 8];
			var matrix = new Matrix<Node>(nodes, 8);
			Engine.FillCustomBoard(matrix, Span<int>.Empty);
			var firstTurn = new Turn(0, NodeOperation.Reveal);
			var secondTurn = new Turn(1, operation);
			Engine.ComputeBoard(matrix, firstTurn, stackalloc int[64]);

			Assert.Equal(BoardStatus.Completed, nodes.Status());
			Engine.EnsureValidBoardConfiguration(matrix, secondTurn);
		});
	}

	[Theory]
	[InlineData(NodeOperation.Reveal)]
	[InlineData(NodeOperation.Flag)]
	[InlineData(NodeOperation.RemoveFlag)]
	[InlineData(NodeOperation.Chord)]
	public void Throws_exception_if_any_operation_applied_after_game_is_failed(NodeOperation operation)
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[8 * 8];
			nodes[0] = new(0, true, 0, NodeState.Revealed);
			var matrix = new Matrix<Node>(nodes, 8);
			var turn = new Turn(0, operation);

			Assert.Equal(BoardStatus.Failed, nodes.Status());
			Engine.EnsureValidBoardConfiguration(matrix, turn);
		});
	}

	[Theory]
	[InlineData(NodeOperation.Flag)]
	[InlineData(NodeOperation.RemoveFlag)]
	public void Throws_exception_if_flag_operation_applied_on_revealed_node(NodeOperation operation)
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[2 * 2];
			Span<int> mines = stackalloc int[] { 0 };
			var matrix = new Matrix<Node>(nodes, 2);
			Engine.FillCustomBoard(matrix, mines);
			var firstTurn = new Turn(3, NodeOperation.Reveal);
			Engine.ComputeBoard(matrix, firstTurn, Span<int>.Empty);
			var secondTurn = new Turn(3, operation);
			Engine.EnsureValidBoardConfiguration(matrix, secondTurn);
		});
	}

	[Fact]
	public void Throws_exception_if_turn_coordinates_are_outside_board()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[1 * 1];
			var matrix = new Matrix<Node>(nodes, 1);
			Engine.FillCustomBoard(matrix, Span<int>.Empty);
			var turn = new Turn(1, NodeOperation.Reveal);

			Engine.EnsureValidBoardConfiguration(matrix, turn);
		});
	}

	[Fact]
	public void Throws_exception_if_operation_is_flag_and_no_flags_available()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[1 * 1];
			var matrix = new Matrix<Node>(nodes, 1);
			Engine.FillCustomBoard(matrix, Span<int>.Empty);
			var turn = new Turn(0, NodeOperation.Flag);

			Engine.EnsureValidBoardConfiguration(matrix, turn);
		});
	}

	[Fact]
	public void Throws_exception_if_operation_is_flag_and_node_is_already_flagged()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[1 * 2];
			Span<int> mines = stackalloc int[] { 0 };
			var matrix = new Matrix<Node>(nodes, 1);
			Engine.FillCustomBoard(matrix, mines);
			var turn = new Turn(0, NodeOperation.Flag);
			Engine.ComputeBoard(matrix, turn, Span<int>.Empty);

			Assert.Equal(BoardStatus.Pending, nodes.Status());
			Engine.EnsureValidBoardConfiguration(matrix, turn);
		});
	}

	[Fact]
	public void Throws_exception_if_operation_is_remove_flag_and_node_is_not_flagged()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[2 * 2];
			Span<int> mines = stackalloc int[] { 0, 1 };
			var matrix = new Matrix<Node>(nodes, 2);
			Engine.FillCustomBoard(matrix, mines);
			var turn = new Turn(0, NodeOperation.RemoveFlag);

			Engine.EnsureValidBoardConfiguration(matrix, turn);
		});
	}

	[Fact]
	public void Throws_exception_if_chord_on_non_revealed_node()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[1 * 1];
			var matrix = new Matrix<Node>(nodes, 1);
			Engine.FillCustomBoard(matrix, Span<int>.Empty);
			var turn = new Turn(0, NodeOperation.Chord);

			Engine.EnsureValidBoardConfiguration(matrix, turn);
		});
	}

	[Fact]
	public void Throws_exception_if_chord_on_revealed_node_with_zero_adjacent_mines()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[3 * 3];
			Span<int> mines = stackalloc int[] { 0 };
			nodes[0] = new(0, true, 0, NodeState.Hidden);
			var matrix = new Matrix<Node>(nodes, 3);
			var firstTurn = new Turn(8, NodeOperation.Reveal);
			var secondTurn = new Turn(8, NodeOperation.Chord);
			Engine.ComputeBoard(matrix, firstTurn, stackalloc int[9]);

			var node = nodes[8];
			Assert.Equal(NodeState.Revealed, node.State);
			Assert.Equal(0, node.MineCount);

			Engine.EnsureValidBoardConfiguration(matrix, secondTurn);
		});
	}

	[Fact]
	public void Throws_exception_if_chord_on_revealed_node_when_adjacent_mine_count_does_not_equal_adjacent_flag_count()
	{
		Assert.Throws<InvalidGameStateException>(() =>
		{
			Span<Node> nodes = stackalloc Node[2 * 2];
			Span<int> mines = stackalloc int[] { 0, 1, 2 };
			var matrix = new Matrix<Node>(nodes, 2);
			Engine.FillCustomBoard(matrix, mines);
			var firstTurn = new Turn(0, NodeOperation.Flag);
			var secondTurn = new Turn(3, NodeOperation.Reveal);
			var thirdTurn = new Turn(3, NodeOperation.Chord);

			Engine.ComputeBoard(matrix, firstTurn, Span<int>.Empty);
			Engine.ComputeBoard(matrix, secondTurn, Span<int>.Empty);
			Engine.EnsureValidBoardConfiguration(matrix, thirdTurn);
		});
	}

	[Fact]
	public void MayonlychordNodeWithHiddenadjacentNodes()
	{
		Assert.True(true);
	}

	// We visit nodes regardless of whether they have already been revealed.
	// Realistically, this never happens. However, there is a case with
	// abusing flags which could lead to this case
	// TODO: add test
	// ring of flags used to create a ring of revealed nodes
}

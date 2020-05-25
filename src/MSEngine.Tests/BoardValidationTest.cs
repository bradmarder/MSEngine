using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
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
                Engine.Instance.FillCustomBoard(nodes, 8, 8, 0);
                var firstTurn = new Turn(0, NodeOperation.Reveal);
                var secondTurn = new Turn(1, operation);
                BoardStateMachine.Instance.ComputeBoard(nodes, firstTurn);

                Assert.Equal(BoardStatus.Completed, nodes.Status());
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, secondTurn);
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
                Engine.Instance.FillCustomBoard(nodes, 8, 8, 1);
                var firstTurn = new Turn(0, NodeOperation.Reveal);
                var secondTurn = new Turn(1, operation);
                BoardStateMachine.Instance.ComputeBoard(nodes, firstTurn);

                Assert.Equal(BoardStatus.Failed, nodes.Status());
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, secondTurn);
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
                Engine.Instance.FillCustomBoard(nodes, 2, 2, 1);
                var firstTurn = new Turn(3, NodeOperation.Reveal);
                BoardStateMachine.Instance.ComputeBoard(nodes, firstTurn);
                var secondTurn = new Turn(3, operation);
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, secondTurn);
            });
        }

        [Fact]
        public void Throws_exception_if_turn_coordinates_are_outside_board()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[1 * 1];
                Engine.Instance.FillCustomBoard(nodes, 1, 1, 0);
                var turn = new Turn(1, NodeOperation.Reveal);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_no_flags_available()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[1 * 1];
                Engine.Instance.FillCustomBoard(nodes, 1, 1, 0);
                var turn = new Turn(0, NodeOperation.Flag);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_operation_is_flag_and_node_is_already_flagged()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[1 * 2];
                Engine.Instance.FillCustomBoard(nodes, 1, 2, 1);
                var turn = new Turn(0, NodeOperation.Flag);
                BoardStateMachine.Instance.ComputeBoard(nodes, turn);

                Assert.Equal(BoardStatus.Pending, nodes.Status());
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_operation_is_remove_flag_and_node_is_not_flagged()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[2 * 2];
                Engine.Instance.FillCustomBoard(nodes, 2, 2, 2);
                var turn = new Turn(0, NodeOperation.RemoveFlag);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_chord_on_non_revealed_node()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[1 * 1];
                Engine.Instance.FillCustomBoard(nodes, 1, 1, 0);
                var turn = new Turn(0, NodeOperation.Chord);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
            });
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_node_with_zero_adjacent_mines()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[3 * 3];
                Engine.Instance.FillCustomBoard(nodes, 3, 3, 1);
                var firstTurn = new Turn(8, NodeOperation.Reveal);
                var secondTurn = new Turn(8, NodeOperation.Chord);
                BoardStateMachine.Instance.ComputeBoard(nodes, firstTurn);
                var node = nodes[8];

                Assert.Equal(NodeState.Revealed, node.State);
                Assert.Equal(0, node.MineCount);

                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, secondTurn);
            });
        }

        [Fact]
        public void Throws_exception_if_chord_on_revealed_node_when_adjacent_mine_count_does_not_equal_adjacent_flag_count()
        {
            Assert.Throws<InvalidGameStateException>(() =>
            {
                Span<Node> nodes = stackalloc Node[2 * 2];
                Engine.Instance.FillCustomBoard(nodes, 2, 2, 3);
                var firstTurn = new Turn(0, NodeOperation.Flag);
                var secondTurn = new Turn(3, NodeOperation.Reveal);
                var thirdTurn = new Turn(3, NodeOperation.Chord);

                BoardStateMachine.Instance.ComputeBoard(nodes, firstTurn);
                BoardStateMachine.Instance.ComputeBoard(nodes, secondTurn);
                BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, thirdTurn);
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
}

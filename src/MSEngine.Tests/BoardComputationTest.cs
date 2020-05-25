using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardComputationTest
    {
        [Fact]
        public void Zero_mine_count_with_one_reveal_completes_board()
        {
            Span<Node> nodes = stackalloc Node[8 * 8];
            Engine.PureInstance.FillCustomBoard(nodes, 8, 8, 0);
            var turn = new Turn(0, NodeOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(nodes, turn);

            Assert.Equal(BoardStatus.Completed, nodes.Status());
        }

        [Fact]
        public void All_mines_revealed_if_game_fails()
        {
            Span<Node> nodes = stackalloc Node[2 * 2];
            Engine.PureInstance.FillCustomBoard(nodes, 2, 2, 1);
            BoardStateMachine.FailBoard(nodes);
            var allNodesRevealed = nodes.ToArray().All(x => !x.HasMine || x.State == NodeState.Flagged || x.State == NodeState.Revealed);

            Assert.True(allNodesRevealed);
        }

        [Fact]
        public void Flagging_node_only_flags_single_node()
        {
            Span<Node> nodes = stackalloc Node[2 * 2];
            Engine.PureInstance.FillCustomBoard(nodes, 2, 2, 1);
            var board = nodes.ToArray();
            var turn = new Turn(0, NodeOperation.Flag);
            BoardStateMachine.Instance.ComputeBoard(nodes, turn);
            var node = nodes[0];
            var everyOtherNodeHasNotChanged = nodes.ToArray().All(x => x.Equals(node) || board.Contains(x));

            Assert.Equal(NodeState.Flagged, node.State);
            Assert.True(everyOtherNodeHasNotChanged);
        }

        [Fact]
        public void Revealing_node_with_no_mine_and_has_adjacent_mines_only_reveals_single_node()
        {
            Span<Node> nodes = stackalloc Node[2 * 2];
            Engine.PureInstance.FillCustomBoard(nodes, 2, 2, 1);
            var board = nodes.ToArray();
            var turn = new Turn(0, NodeOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(nodes, turn);
            var node = nodes[0];
            var everyOtherNodeHasNotChanged = nodes.ToArray().All(x => x.Equals(node) || board.Contains(x));

            Assert.Equal(NodeState.Revealed, node.State);
            Assert.True(everyOtherNodeHasNotChanged);
        }

        [Fact]
        public void Chording_node_reveals_surrounding_nodes()
        {
            Span<Node> nodes = stackalloc Node[2 * 2];
            Engine.PureInstance.FillCustomBoard(nodes, 2, 2, 1);
            Span<Turn> turns = stackalloc Turn[3]
            {
                new Turn(0, NodeOperation.Flag),
                new Turn(3, NodeOperation.Reveal),
                new Turn(3, NodeOperation.Chord)
            };
            BoardStateMachine.Instance.ComputeBoard(nodes, turns);

            // 0,0 has the only mine, so we flag it
            // revealing 1,1 shows a 1
            // chording 1,1 should reveal 0,1 and 1,0 - thus completing the board
            Assert.Equal(BoardStatus.Completed, nodes.Status());
        }

        [Fact]
        public void Revealing_node_without_mine_and_zero_adjacent_mines_triggers_chain_reaction()
        {
            Span<Node> nodes = stackalloc Node[3 * 3];
            Engine.PureInstance.FillCustomBoard(nodes, 3, 3, 1);
            var node = nodes[8];
            var firstTurn = new Turn(8, NodeOperation.Reveal);
            BoardStateMachine.Instance.ComputeBoard(nodes, firstTurn);

            Assert.False(node.HasMine);
            Assert.Equal(0, node.MineCount);
            Assert.True(nodes[0].HasMine);

            for (var i = 0; i < nodes.Length; i++)
            {
                if (i == 0) { continue; }
                Assert.Equal(NodeState.Revealed, nodes[i].State);
            }
        }

        [Fact]
        public void Chain_reaction_is_blocked_by_false_flag()
        {
            Span<Node> nodes = stackalloc Node[5 * 1];
            Engine.PureInstance.FillCustomBoard(nodes, 5, 1, 0);
            Span<Turn> turns = stackalloc Turn[2]
            {
                new Turn(2, NodeOperation.Flag),
                new Turn(4, NodeOperation.Reveal)
            };
            BoardStateMachine.Instance.ComputeBoard(nodes, turns);

            for (var i = 0; i < nodes.Length; i++)
            {
                var nodeState = nodes[i].State;
                var expectedState = i == 2 ? NodeState.Flagged
                    : i < 2 ? NodeState.Hidden
                    : NodeState.Revealed;

                Assert.Equal(expectedState, nodeState);
            }
            Assert.Equal(BoardStatus.Pending, nodes.Status());
        }
    }
}

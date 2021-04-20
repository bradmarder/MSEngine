using System;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class BoardComputationTest
    {
        [Fact]
        public void AllMinesRevealedIfMineExplodes()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, true, 0, NodeState.Flagged),
                new(1, true, 1, NodeState.Hidden)
            };

            Engine.RevealHiddenMines(nodes);

            Assert.Equal(NodeState.Flagged, nodes[0].State);
            Assert.Equal(NodeState.Revealed, nodes[1].State);
        }

        // 2x2 matrix, node 0 has mine and is flagged, node 1 is revealed.
        // Chording Node 1 should reveal Node 2/3
        [Fact]
        public void ChordingNodeRevealsAdjacentNodes()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, true, 0, NodeState.Flagged),
                new(1, false, 1, NodeState.Revealed),
                new(2, false, 1, NodeState.Hidden),
                new(3, false, 1, NodeState.Hidden)
            };
            var matrix = new Matrix<Node>(nodes, 2);
            var turn = new Turn(1, NodeOperation.Chord);

            Engine.ComputeBoard(matrix, turn, Span<int>.Empty);

            Assert.Equal(NodeState.Revealed, nodes[2].State);
            Assert.Equal(NodeState.Revealed, nodes[3].State);
        }

        // 3x3 matrix with zero mines, revealing the middle node should reveal entire matrix
        [Fact]
        public void RevealingNodeWithoutMineAndZeroAdjacentMinesTriggersChainReaction()
        {
            Span<Node> nodes = stackalloc Node[9];
            var matrix = new Matrix<Node>(nodes, 3);
            Engine.FillCustomBoard(matrix, stackalloc int[9]);
            var turn = new Turn(4, NodeOperation.Reveal);

            Engine.ComputeBoard(matrix, turn, Span<int>.Empty);

            foreach (var node in nodes)
            {
                Assert.Equal(NodeState.Revealed, node.State);
            }
        }

        // 1x3 matrix, middle node is flagged (and does not have a mine), end nodes are hidden
        [Fact]
        public void ChainReactionIsBlockedByFalseFlag()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, false, 0, NodeState.Hidden),
                new(1, false, 0, NodeState.Flagged),
                new(2, false, 0, NodeState.Hidden)
            };
            var matrix = new Matrix<Node>(nodes, 3);
            var turn = new Turn(0, NodeOperation.Reveal);

            Engine.ComputeBoard(matrix, turn, stackalloc int[3]);

            Assert.Equal(NodeState.Revealed, nodes[0].State);
            Assert.Equal(NodeState.Flagged, nodes[1].State);
            Assert.Equal(NodeState.Hidden, nodes[2].State);
        }

        // 1x3 matrix, middle node is already revealed, end nodes are hidden
        [Fact]
        public void ChainReactionIsNotBlockedByRevealedNode()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new(0, false, 0, NodeState.Hidden),
                new(1, false, 0, NodeState.Revealed),
                new(2, false, 0, NodeState.Hidden)
            };
            var matrix = new Matrix<Node>(nodes, 3);
            var turn = new Turn(0, NodeOperation.Reveal);

            Engine.ComputeBoard(matrix, turn, stackalloc int[9]);

            foreach (var node in nodes)
            {
                Assert.Equal(NodeState.Revealed, node.State);
            }
        }
    }
}
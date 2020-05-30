using MSEngine.Core;
using MSEngine.Solver;
using System;
using Xunit;

namespace MSEngine.Tests
{
    public class SolverTest
    {
        /// <summary>
        /// 01>__
        /// 011__
        /// 111__
        /// __3__
        /// _____
        /// </summary>
        [Fact]
        public void CalculatesTurnsForZeroAugmentColumnPriorToGaussianElimination()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),

                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),

                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),

                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),

                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden)
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];
            var matrix = new Matrix<Node>(nodes, 5);

            var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false);

            Assert.Equal(3, turnCount);
            Assert.Equal(new Turn(3, NodeOperation.Reveal), turns[0]);
            Assert.Equal(new Turn(8, NodeOperation.Reveal), turns[1]);
            Assert.Equal(new Turn(13, NodeOperation.Reveal), turns[2]);
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
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),

                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),

                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),

                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),

                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),

                new Node(false, 3, NodeState.Revealed),
                new Node(false, 0, NodeState.Flagged),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),

                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),


                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden)
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];
            var matrix = new Matrix<Node>(nodes, 8);

            var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false);

            Assert.Equal(2, turnCount);
            Assert.Equal(new Turn(46, NodeOperation.Flag), turns[0]);
            Assert.Equal(new Turn(47, NodeOperation.Flag), turns[1]);
        }

        /// <summary>
        /// The reason we need float instead of sbyte/int for <see cref="Matrix{T}"/>
        /// </summary>
        [Fact]
        public void CalculatesTurnsWhenGaussianEliminationProducesNonIntegers()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Hidden),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(true, 2, NodeState.Hidden),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 1, NodeState.Revealed),
                new Node(true, 0, NodeState.Hidden),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 2, NodeState.Flagged),
                new Node(false, 3, NodeState.Hidden),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(true, 2, NodeState.Hidden),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Hidden),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Hidden),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(true, 1, NodeState.Hidden),
                new Node(true, 1, NodeState.Hidden),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Hidden),
                new Node(false, 2, NodeState.Hidden),
                new Node(false, 1, NodeState.Hidden),
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];
            var matrix = new Matrix<Node>(nodes, 8);

            var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false);

            Assert.Equal(2, turnCount);

            // changing float to sbyte will cause our solver to generate incorrect turns
            Assert.NotEqual(6, turnCount);

            Assert.Equal(new Turn(29, NodeOperation.Reveal), turns[0]);
            Assert.Equal(new Turn(61, NodeOperation.Reveal), turns[1]);
        }

        /// <summary>
        /// The intent of this test is to demonstrate that we must factor in all hidden nodes in
        /// the matrix in order to calculate any turns
        /// </summary>
        [Fact]
        public void CalculatesTurnsWhenFactoringAllHiddenNodes()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 3, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 0, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Hidden),
                new Node(true, 1, NodeState.Hidden),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 1, NodeState.Hidden),
                new Node(false, 1, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
                new Node(false, 2, NodeState.Revealed),
                new Node(true, 1, NodeState.Flagged),
                new Node(false, 2, NodeState.Revealed),
                new Node(false, 0, NodeState.Revealed),
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];
            var matrix = new Matrix<Node>(nodes, 8);

            var partialHiddenNodeTurnCount = MatrixSolver.CalculateTurns(matrix, turns, false);
            var fullHiddenNodeTurnCount = MatrixSolver.CalculateTurns(matrix, turns, true);

            Assert.Equal(0, partialHiddenNodeTurnCount);
            Assert.Equal(2, fullHiddenNodeTurnCount);
            Assert.Equal(new Turn(56, NodeOperation.Reveal), turns[0]);
            Assert.Equal(new Turn(57, NodeOperation.Reveal), turns[1]);
        }
    }
}

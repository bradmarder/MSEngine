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
                new Node(0, false, 0, NodeState.Revealed),
                new Node(1, false, 1, NodeState.Revealed),
                new Node(2, true, 0, NodeState.Flagged),
                new Node(3, false, 0, NodeState.Hidden),
                new Node(4, false, 0, NodeState.Hidden),

                new Node(5, false, 0, NodeState.Revealed),
                new Node(6, false, 1, NodeState.Revealed),
                new Node(7, false, 1, NodeState.Revealed),
                new Node(8, false, 0, NodeState.Hidden),
                new Node(9, false, 0, NodeState.Hidden),

                new Node(10, false, 1, NodeState.Revealed),
                new Node(11, false, 1, NodeState.Revealed),
                new Node(12, false, 1, NodeState.Revealed),
                new Node(13, false, 0, NodeState.Hidden),
                new Node(14, false, 0, NodeState.Hidden),

                new Node(15, false, 0, NodeState.Hidden),
                new Node(16, false, 0, NodeState.Hidden),
                new Node(17, false, 3, NodeState.Revealed),
                new Node(18, false, 0, NodeState.Hidden),
                new Node(19, false, 0, NodeState.Hidden),

                new Node(20, false, 0, NodeState.Hidden),
                new Node(21, false, 0, NodeState.Hidden),
                new Node(22, false, 0, NodeState.Hidden),
                new Node(23, false, 0, NodeState.Hidden),
                new Node(24, false, 0, NodeState.Hidden)
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
                new Node(0, false, 0, NodeState.Revealed),
                new Node(1, false, 0, NodeState.Revealed),
                new Node(2, false, 0, NodeState.Revealed),
                new Node(3, false, 0, NodeState.Revealed),
                new Node(4, false, 1, NodeState.Revealed),
                new Node(5, false, 0, NodeState.Hidden),
                new Node(6, false, 1, NodeState.Revealed),
                new Node(7, false, 0, NodeState.Revealed),

                new Node(8, false, 1, NodeState.Revealed),
                new Node(9, false, 1, NodeState.Revealed),
                new Node(10, false, 2, NodeState.Revealed),
                new Node(11, false, 1, NodeState.Revealed),
                new Node(12, false, 2, NodeState.Revealed),
                new Node(13, false, 0, NodeState.Hidden),
                new Node(14, false, 1, NodeState.Revealed),
                new Node(15, false, 0, NodeState.Revealed),

                new Node(16, false, 1, NodeState.Revealed),
                new Node(17, false, 0, NodeState.Flagged),
                new Node(18, false, 2, NodeState.Revealed),
                new Node(19, false, 0, NodeState.Flagged),
                new Node(20, false, 2, NodeState.Revealed),
                new Node(21, false, 1, NodeState.Revealed),
                new Node(22, false, 1, NodeState.Revealed),
                new Node(23, false, 0, NodeState.Revealed),

                new Node(24, false, 2, NodeState.Revealed),
                new Node(25, false, 2, NodeState.Revealed),
                new Node(26, false, 3, NodeState.Revealed),
                new Node(27, false, 1, NodeState.Revealed),
                new Node(28, false, 2, NodeState.Revealed),
                new Node(29, false, 0, NodeState.Hidden),
                new Node(30, false, 1, NodeState.Revealed),
                new Node(31, false, 0, NodeState.Revealed),

                new Node(32, false, 2, NodeState.Revealed),
                new Node(33, false, 0, NodeState.Flagged),
                new Node(34, false, 2, NodeState.Revealed),
                new Node(35, false, 0, NodeState.Revealed),
                new Node(36, false, 1, NodeState.Revealed),
                new Node(37, false, 0, NodeState.Hidden),
                new Node(38, false, 3, NodeState.Revealed),
                new Node(39, false, 2, NodeState.Revealed),

                new Node(40, false, 3, NodeState.Revealed),
                new Node(41, false, 0, NodeState.Flagged),
                new Node(42, false, 3, NodeState.Revealed),
                new Node(43, false, 1, NodeState.Revealed),
                new Node(44, false, 1, NodeState.Revealed),
                new Node(45, false, 2, NodeState.Revealed),
                new Node(46, false, 0, NodeState.Hidden),
                new Node(47, false, 0, NodeState.Hidden),

                new Node(48, false, 0, NodeState.Hidden),
                new Node(49, false, 0, NodeState.Hidden),
                new Node(50, false, 0, NodeState.Hidden),
                new Node(51, false, 0, NodeState.Hidden),
                new Node(52, false, 0, NodeState.Hidden),
                new Node(53, false, 0, NodeState.Hidden),
                new Node(54, false, 0, NodeState.Hidden),
                new Node(55, false, 0, NodeState.Hidden),

                new Node(56, false, 0, NodeState.Hidden),
                new Node(57, false, 0, NodeState.Hidden),
                new Node(58, false, 0, NodeState.Hidden),
                new Node(59, false, 0, NodeState.Hidden),
                new Node(60, false, 0, NodeState.Hidden),
                new Node(61, false, 0, NodeState.Hidden),
                new Node(62, false, 0, NodeState.Hidden),
                new Node(63, false, 0, NodeState.Hidden)
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
                new Node(0, false, 1, NodeState.Revealed),
                new Node(1, false, 1, NodeState.Revealed),
                new Node(2, false, 1, NodeState.Revealed),
                new Node(3, true, 1, NodeState.Flagged),
                new Node(4, false, 2, NodeState.Revealed),
                new Node(5, false, 1, NodeState.Revealed),
                new Node(6, false, 1, NodeState.Revealed),
                new Node(7, false, 1, NodeState.Hidden),

                new Node(8, true, 1, NodeState.Flagged),
                new Node(9, false, 2, NodeState.Revealed),
                new Node(10, false, 3, NodeState.Revealed),
                new Node(11, false, 3, NodeState.Revealed),
                new Node(12, true, 2, NodeState.Hidden),
                new Node(13, false, 1, NodeState.Hidden),
                new Node(14, false, 1, NodeState.Revealed),
                new Node(15, true, 0, NodeState.Hidden),

                new Node(16, false, 2, NodeState.Revealed),
                new Node(17, true, 1, NodeState.Flagged),
                new Node(18, false, 2, NodeState.Revealed),
                new Node(19, true, 2, NodeState.Flagged),
                new Node(20, false, 3, NodeState.Hidden),
                new Node(21, false, 2, NodeState.Revealed),
                new Node(22, false, 1, NodeState.Revealed),
                new Node(23, false, 1, NodeState.Revealed),

                new Node(24, false, 1, NodeState.Revealed),
                new Node(25, false, 1, NodeState.Revealed),
                new Node(26, false, 3, NodeState.Revealed),
                new Node(27, false, 3, NodeState.Revealed),
                new Node(28, true, 2, NodeState.Hidden),
                new Node(29, false, 1, NodeState.Hidden),
                new Node(30, false, 0, NodeState.Hidden),
                new Node(31, false, 0, NodeState.Hidden),

                new Node(32, false, 0, NodeState.Revealed),
                new Node(33, false, 0, NodeState.Revealed),
                new Node(34, false, 1, NodeState.Revealed),
                new Node(35, true, 1, NodeState.Flagged),
                new Node(36, false, 2, NodeState.Revealed),
                new Node(37, false, 1, NodeState.Hidden),
                new Node(38, false, 0, NodeState.Hidden),
                new Node(39, false, 0, NodeState.Hidden),

                new Node(40, false, 0, NodeState.Revealed),
                new Node(41, false, 0, NodeState.Revealed),
                new Node(42, false, 1, NodeState.Revealed),
                new Node(43, false, 1, NodeState.Revealed),
                new Node(44, false, 2, NodeState.Revealed),
                new Node(45, false, 2, NodeState.Revealed),
                new Node(46, false, 2, NodeState.Hidden),
                new Node(47, false, 1, NodeState.Hidden),

                new Node(48, false, 0, NodeState.Revealed),
                new Node(49, false, 0, NodeState.Revealed),
                new Node(50, false, 0, NodeState.Revealed),
                new Node(51, false, 0, NodeState.Revealed),
                new Node(52, false, 1, NodeState.Revealed),
                new Node(53, true, 1, NodeState.Hidden),
                new Node(54, true, 1, NodeState.Hidden),
                new Node(55, false, 1, NodeState.Hidden),

                new Node(56, false, 0, NodeState.Revealed),
                new Node(57, false, 0, NodeState.Revealed),
                new Node(58, false, 0, NodeState.Revealed),
                new Node(59, false, 0, NodeState.Revealed),
                new Node(60, false, 1, NodeState.Revealed),
                new Node(61, false, 2, NodeState.Hidden),
                new Node(62, false, 2, NodeState.Hidden),
                new Node(63, false, 1, NodeState.Hidden)
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
                new Node(0, false, 1, NodeState.Revealed),
                new Node(1, false, 1, NodeState.Revealed),
                new Node(2, false, 0, NodeState.Revealed),
                new Node(3, false, 0, NodeState.Revealed),
                new Node(4, false, 1, NodeState.Revealed),
                new Node(5, true, 0, NodeState.Flagged),
                new Node(6, false, 1, NodeState.Revealed),
                new Node(7, false, 0, NodeState.Revealed),

                new Node(8, true, 0, NodeState.Flagged),
                new Node(9, false, 2, NodeState.Revealed),
                new Node(10, false, 1, NodeState.Revealed),
                new Node(11, false, 2, NodeState.Revealed),
                new Node(12, false, 2, NodeState.Revealed),
                new Node(13, false, 2, NodeState.Revealed),
                new Node(14, false, 1, NodeState.Revealed),
                new Node(15, false, 0, NodeState.Revealed),

                new Node(16, false, 1, NodeState.Revealed),
                new Node(17, false, 2, NodeState.Revealed),
                new Node(18, true, 0, NodeState.Flagged),
                new Node(19, false, 2, NodeState.Revealed),
                new Node(20, true, 0, NodeState.Flagged),
                new Node(21, false, 2, NodeState.Revealed),
                new Node(22, false, 1, NodeState.Revealed),
                new Node(23, false, 1, NodeState.Revealed),

                new Node(24, false, 0, NodeState.Revealed),
                new Node(25, false, 1, NodeState.Revealed),
                new Node(26, false, 1, NodeState.Revealed),
                new Node(27, false, 3, NodeState.Revealed),
                new Node(28, false, 2, NodeState.Revealed),
                new Node(29, false, 3, NodeState.Revealed),
                new Node(30, true, 0, NodeState.Flagged),
                new Node(31, false, 1, NodeState.Revealed),

                new Node(32, false, 0, NodeState.Revealed),
                new Node(33, false, 1, NodeState.Revealed),
                new Node(34, false, 1, NodeState.Revealed),
                new Node(35, false, 2, NodeState.Revealed),
                new Node(36, true, 0, NodeState.Flagged),
                new Node(37, false, 2, NodeState.Revealed),
                new Node(38, false, 1, NodeState.Revealed),
                new Node(39, false, 1, NodeState.Revealed),

                new Node(40, false, 1, NodeState.Revealed),
                new Node(41, false, 2, NodeState.Revealed),
                new Node(42, true, 1, NodeState.Flagged),
                new Node(43, false, 2, NodeState.Revealed),
                new Node(44, false, 2, NodeState.Revealed),
                new Node(45, false, 2, NodeState.Revealed),
                new Node(46, false, 1, NodeState.Revealed),
                new Node(47, false, 0, NodeState.Revealed),

                new Node(48, false, 1, NodeState.Hidden),
                new Node(49, true, 1, NodeState.Hidden),
                new Node(50, false, 2, NodeState.Revealed),
                new Node(51, false, 1, NodeState.Revealed),
                new Node(52, false, 2, NodeState.Revealed),
                new Node(53, true, 1, NodeState.Flagged),
                new Node(54, false, 2, NodeState.Revealed),
                new Node(55, false, 0, NodeState.Revealed),

                new Node(56, false, 1, NodeState.Hidden),
                new Node(57, false, 1, NodeState.Hidden),
                new Node(58, false, 1, NodeState.Revealed),
                new Node(59, false, 0, NodeState.Revealed),
                new Node(60, false, 2, NodeState.Revealed),
                new Node(61, true, 1, NodeState.Flagged),
                new Node(62, false, 2, NodeState.Revealed),
                new Node(63, false, 0, NodeState.Revealed),
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

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
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(true, 0, NodeOperation.Flag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),

                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),

                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),

                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),

                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag)
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];

            var turnCount = MatrixSolver.CalculateTurns(nodes, ref turns, 5);

            Assert.Equal(3, turnCount);
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
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),

                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),

                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),

                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),

                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),

                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Flag),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),

                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),


                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag)
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];

            var turnCount = MatrixSolver.CalculateTurns(nodes, ref turns, 8);

            Assert.Equal(2, turnCount);
            Assert.Equal(new Turn(46, NodeOperation.Flag), turns[0]);
            Assert.Equal(new Turn(47, NodeOperation.Flag), turns[1]);
        }

        /// <summary>
        /// The reason we need float instead of sbyte for <see cref="FlatMatrix{T}"/>
        /// </summary>
        [Fact]
        public void CalculatesTurnsWhenGaussianEliminationProducesNonIntegers()
        {
            Span<Node> nodes = stackalloc Node[]
            {
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(true, 1, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.RemoveFlag),
                new Node(true, 1, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(true, 2, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(true, 0, NodeOperation.RemoveFlag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(true, 1, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(true, 2, NodeOperation.Flag),
                new Node(false, 3, NodeOperation.RemoveFlag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(false, 3, NodeOperation.Reveal),
                new Node(true, 2, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(true, 1, NodeOperation.Flag),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(true, 1, NodeOperation.RemoveFlag),
                new Node(true, 1, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.RemoveFlag),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 0, NodeOperation.Reveal),
                new Node(false, 1, NodeOperation.Reveal),
                new Node(false, 2, NodeOperation.RemoveFlag),
                new Node(false, 2, NodeOperation.RemoveFlag),
                new Node(false, 1, NodeOperation.RemoveFlag),
            };
            Span<Turn> turns = stackalloc Turn[nodes.Length];

            var turnCount = MatrixSolver.CalculateTurns(nodes, ref turns, 8);

            Assert.Equal(2, turnCount);

            // changing float to sbyte will cause our solver to generate incorrect turns
            Assert.NotEqual(6, turnCount);

            Assert.Equal(new Turn(29, NodeOperation.Reveal), turns[0]);
            Assert.Equal(new Turn(61, NodeOperation.Reveal), turns[1]);
        }
    }
}

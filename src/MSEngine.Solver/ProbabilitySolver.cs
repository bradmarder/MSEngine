using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public static class ProbabilitySolver
    {
        public static Turn ComputeTurn(Matrix<Node> nodeMatrix)
        {
            var nodes = nodeMatrix.Nodes;
            Debug.Assert(nodes.Length > 0);

            Span<int> buffer = stackalloc int[8];
            Span<int> revealedAMCNodes = stackalloc int[nodes.Length];
            Span<int> hiddenNodes = stackalloc int[nodes.Length];

            #region Revealed Nodes with AMC > 0

            var revealedAMCNodeCount = 0;
            foreach (var node in nodes)
            {
                if (node.State == NodeState.Revealed && node.MineCount > 0 && Utilities.HasHiddenAdjacentNodes(nodeMatrix, buffer, node.Index))
                {
                    revealedAMCNodes[revealedAMCNodeCount] = node.Index;
                    revealedAMCNodeCount++;
                }
            }

            revealedAMCNodes = revealedAMCNodes.Slice(0, revealedAMCNodeCount);

            #endregion

            #region Hidden Nodes

            var hiddenNodeCount = 0;

            foreach (var node in nodes)
            {
                if (node.State != NodeState.Hidden) { continue; }

                var hasAHC = false;
                buffer.FillAdjacentNodeIndexes(nodes.Length, node.Index, nodeMatrix.ColumnCount);

                foreach (var x in buffer)
                {
                    if (x == -1) { continue; }

                    var adjNode = nodes[x];
                    if (adjNode.State == NodeState.Revealed && adjNode.MineCount > 0)
                    {
                        hasAHC = true;
                        break;
                    }
                }

                if (hasAHC)
                {
                    hiddenNodes[hiddenNodeCount] = node.Index;
                    hiddenNodeCount++;
                }
            }

            hiddenNodes = hiddenNodes.Slice(0, hiddenNodeCount);

            #endregion

            #region Matrix Generation

            var rows = revealedAMCNodeCount;
            var columns = hiddenNodeCount;

            if (columns > 128)
            {
                throw new Exception("Must use BigInteger type instead of long = " + columns);
            }

            Span<ulong> vectors = stackalloc ulong[rows];
            Span<int> augments = stackalloc int[rows]; // possibly make these bytes?

            for (var row = 0; row < rows; row++)
            {
                var nodeIndex = revealedAMCNodes[row];

                for (var column = 0; column < columns; column++)
                {
                    if (column == columns - 1)
                    {
                        augments[row] = nodes[nodeIndex].MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodeMatrix, buffer, nodeIndex);
                    }
                    if (Utilities.IsAdjacentTo(buffer, nodes.Length, nodeMatrix.ColumnCount, nodeIndex, hiddenNodes[column]))
                    {
                        vectors[row] |= (ulong)1 << column;
                    }
                }
            }

            #endregion

            #region Testing Vector Visitation

            Span<float> foo = stackalloc float[revealedAMCNodeCount * (hiddenNodeCount + 1)];
            var matrix = new Matrix<float>(foo, hiddenNodeCount + 1);
            for (var row = 0; row < rows; row++)
            {
                var nodeIndex = revealedAMCNodes[row];
                for (var column = 0; column < columns + 1; column++)
                {
                    var isAugmentedColumn = column == columns;

                    matrix[row, column] = isAugmentedColumn
                        ? nodes[nodeIndex].MineCount - Utilities.GetAdjacentFlaggedNodeCount(nodeMatrix, buffer, nodeIndex)
                        : Utilities.IsAdjacentTo(buffer, nodes.Length, nodeMatrix.ColumnCount, nodeIndex, hiddenNodes[column]) ? 1 : 0;
                }
            }

            //Console.WriteLine("MATRIX");
            //Console.WriteLine(matrix.ToString());
            //Console.WriteLine("GAUSS");
            //matrix.GaussEliminate();
            //Console.WriteLine(matrix.ToString());

            // REMOVE ROWS OF ALL ZEROS
            // REMOVE ROW IF SUM of values equals augment column
            var maxColumns = 5;
            var bar = MathNet.Numerics.LinearAlgebra.CreateMatrix.Dense<float>(3, maxColumns)!;
            bar[0, 0] = 1;
            bar[0, 1] = 0;
            bar[0, 2] = 0;
            bar[0, 3] = 1;
            bar[0, 4] = 1;

            bar[1, 0] = 0;
            bar[1, 1] = 1;
            bar[1, 2] = 0;
            bar[1, 3] = 1;
            bar[1, 4] = 1;

            bar[2, 0] = 0;
            bar[2, 1] = 0;
            bar[2, 2] = 1;
            bar[2, 3] = 1;
            bar[2, 4] = 1;

            Span<ulong> vecs = stackalloc ulong[3];
            vecs[0] = 0b1001;
            vecs[1] = 0b0101;
            vecs[2] = 0b0011;

            Span<int> augs = stackalloc int[3];
            augments[0] = 1;
            augments[1] = 1;
            augments[2] = 1;

            Span<ulong> solutions = stackalloc ulong[999];
            Span<ulong> partials = stackalloc ulong[vecs.Length]; // used to keep track of selecting leftmost nth bits
            var solutionCount = 0;
            ulong solution = 0;
            ulong ignoreBits = 0; // bit set implies it is NOT a mine

            VisitVector(vecs, augs, 0, ref solution, ref ignoreBits, solutions, ref solutionCount);

            #endregion

            #region Grouping

            // remove duplicate groups???
            Span<ulong> groups = stackalloc ulong[vectors.Length];

            for (var row = 0; row < rows; row++)
            {
                var vector = vectors[row];
                ref var group = ref groups[row];
                group = (ulong)1 << row;

                for (var ir = 0; ir < rows; ir++)
                {
                    if (row == ir) { continue; }

                    var vector2 = vectors[ir];

                    // if 2 vectors share any bit, they become grouped
                    if ((vector & vector2) != 0)
                    {
                        group |= (ulong)1 << ir;
                        vector |= vector2;

                        // every time we find a vector that shares a bit, we must restart our loop
                        ir = -1;
                    }
                }
            }

            #endregion

            #region Fail Group Processing

#if NETCOREAPP3_1
            foreach (var group in groups)
            {
                var vectorCount = 0;
                var bits = BitOperations.PopCount(group);
                Span<ulong> vectorz = stackalloc ulong[bits];

                for (var i = 0; i < 64; i++)
                {
                    var bit = (ulong)1 << i;
                    var hasBit = (group & bit) != 0;

                    if (!hasBit) { continue; }

                    vectorz[vectorCount] = vectors[i]; // INCORRECT! 
                    vectorCount++;

                    // short-circuit loop because we've acquired all vectors from this group
                    if (vectorCount == bits) { break; }
                }
                // the "bit" set represents the row index
                // unpack "groups" into their vectors
                // for each vector, find all possible combinations
                // take "1" from each set to create a possible solution to the group
                // iterate over vectors in the group
                // solution & vector , bitcount should be augment column, if not, the solution is invalid
                // end result may be 1 or many solutions
            }
#endif

            // for each group, the # of bits set represents the # of vectors we must consider probabilities for
            // generate matrix of all possible solutions (HOW?)
            // calculate probabably of 1 appears in a column out of N rows

            #endregion

            return new Turn(0, NodeOperation.Chord);
        }

        internal static void VisitVector(ReadOnlySpan<ulong> vecs, ReadOnlySpan<int> augments, int row, ref ulong solution, ref ulong ignoreBits, Span<ulong> solutions, ref int solutionCount)
        {
            // select the vector
            var vector = vecs[row];

            // count the shared bits between the solution/ignoreBits and the vector
            var sharedSolutionBitCount = BitOperations.PopCount(vector & solution);
            var ignoredBitCount = BitOperations.PopCount(vector & ignoreBits);

            // select the computed augment
            var augment = augments[row]
                - sharedSolutionBitCount
                - ignoredBitCount;

            // if the augment is zero, we essentially "skip" this row since our current solution satisfies
            if (augment > 0)
            {
                // mask the vector with the current solution???

                // how many bits are available to choose from?
                var bits = BitOperations.PopCount(vector)
                    - sharedSolutionBitCount
                    - ignoredBitCount;

                Debug.Assert(bits > 0, "The augment > 0 condition above should have already filtered");

                var _solution = GetVectorSolution(vector, bits, augment);

                // if a bit is NOT selected, we OR it with our notMines vector
                var nonSelectedBits = ulong.MaxValue;
                ignoreBits = nonSelectedBits & ignoreBits;
            }

            // if we are on the last row, get ALL solutions for this row, then backtrack
            if (row == vecs.Length - 1)
            {
                solutions[solutionCount] = solution;
                solutionCount++;
                // adjust solution AND ignoreBits
                // ???????? how do we know we just backtracked and now should select the leftmost N+1 bits?????
                VisitVector(vecs, augments, row - 1, ref solution, ref ignoreBits, solutions, ref solutionCount);
            }
            else
            {
                VisitVector(vecs, augments, row + 1, ref solution, ref ignoreBits, solutions, ref solutionCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsBitSet(ulong b, int pos)
            => (b & ((ulong)1 << pos)) != 0;

        internal static ulong GetVectorSolution(ulong vector, int bits, int augment)
        {
            var maxVectorSolutions = MathNet.Numerics.Combinatorics.Combinations(bits, augment);
            Span<ulong> solutionSums = stackalloc ulong[(int)maxVectorSolutions];

            for (var c = 0; c < maxVectorSolutions; c++)
            {
                var bitsAvailable = bits;
                ulong sum = 0;

                for (var i = 0; i < sizeof(ulong) * 8; i++)
                {
                    // WE MUST USE C TO GET UNIQUE SETS OF BITS....HOW?
                    if (IsBitSet(vector, i))
                    {
                        sum += (ulong)1 << i;
                        bitsAvailable--;
                    }
                    if (bitsAvailable == 0) { break; }
                }
                solutionSums[c] = sum; // take SUM of N bit combinations
            }

            return solutionSums[0]; // TODO
        }
    }
}

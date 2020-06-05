using MSEngine.Core;
using System;
using System.Diagnostics;
using System.Numerics;

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

            //bool IsBitSet(byte b, int pos)
            //{
            //    return (b & (1 << pos)) != 0;
            //}

            if (columns > 128)
            {
                throw new Exception("Must use BigInteger type instead of long = " + columns);
            }

            Span<ulong> groups = stackalloc ulong[rows];
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

                        // did the new vector contain a bit that ISN"T part of our vector? 
                        // get new bits from vector2
                        // loop over all rows again and check if they share any bits with newBits
                        // OPTION 2) just loop over all rows again, ignore newBits
                        //var newBits = vector ^ vector2 & vector2;
                    }
                }
            }

            // remove duplicate groups???



#if NETCOREAPP3_1
            foreach (var group in groups)
            {
                var bits = BitOperations.PopCount(group);
                Span<ulong> vectorz = stackalloc ulong[bits];

                // unpack "groups" into their vectors
                // what vectors (+ augment) ?
                // how many possible "mine" combinations?
            }
#endif

                // for each group, the # of bits set represents the # of vectors we must consider probabilities for
                // generate matrix of all possible solutions (HOW?)
                // calculate probabably of 1 appears in a column out of N rows

                #endregion





                return new Turn(0, NodeOperation.Chord);
        }
    }
}

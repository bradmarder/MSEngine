using System;
using System.Diagnostics;
using System.Security.Cryptography;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class EducatedGuessStrategy
    {
        public static Turn UseStrategy(Span<Node> nodes)
        {
            Debug.Assert(nodes.Length > 0);

            int i;
            do
            {
                i = RandomNumberGenerator.GetInt32(nodes.Length);
            } while (nodes[i].State != NodeState.Hidden);

            return new Turn(i, NodeOperation.Reveal);
        }
    }
}

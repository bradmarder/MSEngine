using System;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class EducatedGuessStrategy
    {
        public static Turn UseStrategy(Span<Node> nodes)
        {
            var maxIndex = -1;
            var maxHash = int.MinValue;

            for (int i = 0, l = nodes.Length; i < l; i++)
            {
                var node = nodes[i];
                var hash = node.GetHashCode();
                if (node.State == NodeState.Hidden && hash > maxHash)
                {
                    maxIndex = i;
                    maxHash = hash;
                }
            }

            return new Turn(maxIndex, NodeOperation.Reveal);
        }
    }
}

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
                if (nodes[i].State != NodeState.Hidden) { continue; }

                var hash = i.GetHashCode();
                if (hash > maxHash)
                {
                    maxIndex = i;
                    maxHash = hash;
                }
            }

            return new Turn(maxIndex, NodeOperation.Reveal);
        }
    }
}

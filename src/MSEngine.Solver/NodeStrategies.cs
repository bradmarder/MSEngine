﻿using System;
using System.Diagnostics;
using System.Security.Cryptography;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class NodeStrategies
    {
        public static Turn RevealFirstHiddenNode(ReadOnlySpan<Node> nodes)
        {
            Debug.Assert(nodes.Length > 0);

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.State == NodeState.Hidden)
                {
                    return new Turn(i, NodeOperation.Reveal);
                }
            }

            throw new Exception();
        }

        public static Turn RevealRandomHiddenNode(ReadOnlySpan<Node> nodes)
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

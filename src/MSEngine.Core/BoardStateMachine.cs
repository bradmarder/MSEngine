﻿using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class BoardStateMachine : IBoardStateMachine
    {
        public static IBoardStateMachine Instance { get; } = new BoardStateMachine();

        public virtual void EnsureValidBoardConfiguration(ReadOnlySpan<Node> nodes, Turn turn)
        {
            if (nodes.Status() == BoardStatus.Completed || nodes.Status() == BoardStatus.Failed)
            {
                throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
            }
            if (turn.NodeIndex > nodes.Length)
            {
                throw new InvalidGameStateException("Turn has index outside the matrix");
            }
            if (turn.Operation == NodeOperation.Flag && nodes.FlagsAvailable() == 0)
            {
                throw new InvalidGameStateException("No more flags available");
            }

            var node = nodes[turn.NodeIndex];
            if (node.State == NodeState.Revealed && turn.Operation != NodeOperation.Chord && turn.Operation != NodeOperation.Reveal)
            {
                throw new InvalidGameStateException("Only chord/reveal operations are allowed on revealed nodes");
            }
            if (node.State == NodeState.Flagged && turn.Operation == NodeOperation.Flag)
            {
                throw new InvalidGameStateException("May not flag a node that is already flagged");
            }
            if (turn.Operation == NodeOperation.RemoveFlag && node.State != NodeState.Flagged)
            {
                throw new InvalidGameStateException("Impossible to remove flag from un-flagged node");
            }
            if (turn.Operation == NodeOperation.Chord)
            {
                if (node.State != NodeState.Revealed)
                {
                    throw new InvalidGameStateException("May only chord a revealed node");
                }
                if (node.MineCount == 0)
                {
                    throw new InvalidGameStateException("May only chord a node that has adjacent mines");
                }

                var nodeAdjacentFlagCount = 0;
                var nodeAdjacentHiddenCount = 0;
                Span<int> adjacentIndexes = stackalloc int[8];
                adjacentIndexes.FillAdjacentNodeIndexes(nodes.Length, turn.NodeIndex, 8);

                foreach (var i in adjacentIndexes)
                {
                    var adjacentNode = nodes[i];
                    if (adjacentNode.State == NodeState.Flagged) { nodeAdjacentFlagCount++; }
                    if (adjacentNode.State == NodeState.Hidden) { nodeAdjacentHiddenCount++; }
                }

                if (node.MineCount != nodeAdjacentFlagCount)
                {
                    throw new InvalidGameStateException("May only chord a node when adjacent mine count equals adjacent node flag count");
                }
                if (nodeAdjacentHiddenCount == 0)
                {
                    throw new InvalidGameStateException("May only chord a node that has hidden adjacent nodes");
                }
            }
        }
        public virtual void ComputeBoard(Span<Node> nodes, ReadOnlySpan<Turn> turns)
        {
            foreach (var x in turns)
            {
                ComputeBoard(nodes, x);
            }
        }
        public virtual void ComputeBoard(Span<Node> nodes, Turn turn)
        {
            var node = nodes[turn.NodeIndex];

            // If a node is already revealed, we return instead of throwing an exception
            // This is because the solver generates batches of turns at a time, and any turn
            // may trigger a chain reaction and auto-reveal other nodes
            if (node.State == NodeState.Revealed && turn.Operation == NodeOperation.Reveal)
            {
                return;
            }

            // these cases will only affect a single node
            if (turn.Operation == NodeOperation.Flag || turn.Operation == NodeOperation.RemoveFlag || (turn.Operation == NodeOperation.Reveal && !node.HasMine && node.MineCount > 0))
            {
                nodes[turn.NodeIndex] = new Node(node.HasMine, node.MineCount, turn.Operation);
                return;
            }

            if (turn.Operation == NodeOperation.Reveal)
            {
                if (node.HasMine)
                {
                    FailBoard(nodes);
                }
                else
                {
                    ChainReaction(nodes, turn.NodeIndex);
                }
                return;
            }

            if (turn.Operation == NodeOperation.Chord)
            {
                Chord(nodes, turn.NodeIndex);
                return;
            }
        }

        internal static void FailBoard(Span<Node> nodes)
        {
            // should we show false flags?
            foreach (ref var node in nodes)
            {
                if (node.HasMine && node.State == NodeState.Hidden)
                {
                    node = new Node(node.HasMine, node.MineCount, NodeOperation.Reveal);
                }
            }
        }

        internal static void ChainReaction(Span<Node> nodes, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);

            Span<int> visitedIndexes = stackalloc int[nodes.Length];
            Span<int> revealIndexes = stackalloc int[nodes.Length];

            visitedIndexes.Fill(-1);
            revealIndexes.Fill(-1);

            var visitedIndexCount = 0;
            var revealIndexCount = 0;

            VisitNode(nodes, visitedIndexes, revealIndexes, nodeIndex, ref visitedIndexCount, ref revealIndexCount);

            for (int i = 0, l = nodes.Length; i < l; i++)
            {
                if (nodeIndex == i || revealIndexes.IndexOf(i) != -1)
                {
                    var node = nodes[i];
                    nodes[i] = new Node(node.HasMine, node.MineCount, NodeOperation.Reveal);
                }
            }
        }

        // we recursively visit nodes
        internal static void VisitNode(
            ReadOnlySpan<Node> nodes,
            Span<int> visitedIndexes,
            Span<int> revealIndexes,
            int nodeIndex,
            ref int visitedIndexCount,
            ref int revealIndexCount)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(visitedIndexCount >= 0);
            Debug.Assert(revealIndexCount >= 0);

            const int columnCount = 8;
            Span<int> adjacentIndexes = stackalloc int[8];
            adjacentIndexes.FillAdjacentNodeIndexes(nodes.Length, nodeIndex, columnCount);

            visitedIndexes[visitedIndexCount] = nodeIndex;
            visitedIndexCount++;

            foreach (var i in adjacentIndexes)
            {
                if (i == -1) { continue; }
                if (revealIndexes.IndexOf(i) != -1) { continue; }

                var node = nodes[i];

                // if an adjacent node has a "false flag", it does not expand revealing
                if (node.State != NodeState.Hidden) { continue; }

                revealIndexes[revealIndexCount] = i;
                revealIndexCount++;

                if (node.MineCount == 0 && visitedIndexes.IndexOf(i) == -1)
                {
                    VisitNode(nodes, visitedIndexes, revealIndexes, i, ref visitedIndexCount, ref revealIndexCount);
                }
            }
        }

        internal void Chord(Span<Node> nodes, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeIndex < nodes.Length);

            Span<int> adjacentIndexes = stackalloc int[8];
            adjacentIndexes.FillAdjacentNodeIndexes(nodes.Length, nodeIndex, 8);

            foreach (var i in adjacentIndexes)
            {
                if (i == -1) { continue; }
                if (nodes[i].State != NodeState.Hidden) { continue; }

                var turn = new Turn(i, NodeOperation.Reveal);
                ComputeBoard(nodes, turn);
            }
        }
    }
}
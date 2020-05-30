using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class BoardStateMachine : IBoardStateMachine
    {
        public static IBoardStateMachine Instance { get; } = new BoardStateMachine();

        public virtual void EnsureValidBoardConfiguration(Matrix<Node> matrix, Turn turn)
        {
            var nodes = matrix.Nodes;
            if (nodes.Status() == BoardStatus.Completed || nodes.Status() == BoardStatus.Failed)
            {
                throw new InvalidGameStateException("Turns are not allowed if board status is completed/failed");
            }
            if (turn.NodeIndex >= nodes.Length)
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
                adjacentIndexes.FillAdjacentNodeIndexes(nodes.Length, turn.NodeIndex, matrix.ColumnCount);

                foreach (var i in adjacentIndexes)
                {
                    if (i == -1) { continue; }

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
        public virtual void ComputeBoard(Matrix<Node> matrix, Turn turn)
        {
            var node = matrix.Nodes[turn.NodeIndex];

            // If a node is already revealed, we return instead of throwing an exception
            // This is because the solver generates batches of turns at a time, and any turn
            // may trigger a chain reaction and auto-reveal other nodes
            if (node.State == NodeState.Revealed && turn.Operation == NodeOperation.Reveal)
            {
                return;
            }

            // these cases will only affect the singular node
            if (turn.Operation == NodeOperation.Flag || turn.Operation == NodeOperation.RemoveFlag || (turn.Operation == NodeOperation.Reveal && !node.HasMine && node.MineCount > 0))
            {
                matrix.Nodes[turn.NodeIndex] = new Node(node.HasMine, node.MineCount, turn.Operation);
                return;
            }

            if (turn.Operation == NodeOperation.Reveal)
            {
                if (node.HasMine)
                {
                    RevealHiddenMines(matrix.Nodes);
                }
                else
                {
                    matrix.Nodes[turn.NodeIndex] = new Node(node.HasMine, node.MineCount, NodeOperation.Reveal);
                    ChainReaction(matrix, turn.NodeIndex);
                }
                return;
            }

            if (turn.Operation == NodeOperation.Chord)
            {
                Chord(matrix, turn.NodeIndex);
                return;
            }
        }

        internal static void RevealHiddenMines(Span<Node> nodes)
        {
            // should we show false flags?
            foreach (ref var node in nodes)
            {
                if (node.HasMine && node.State == NodeState.Hidden)
                {
                    node = new Node(true, node.MineCount, NodeOperation.Reveal);
                }
            }
        }

        internal static void ChainReaction(Matrix<Node> matrix, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);

            var visitedIndexCount = 0;
            Span<int> visitedIndexes = stackalloc int[matrix.Nodes.Length]; //  subtract nodes.MineCount() ?
            visitedIndexes.Fill(-1);

            VisitNode(matrix, nodeIndex, visitedIndexes, ref visitedIndexCount);
        }

        // Recursively visits and reveals nodes
        internal static void VisitNode(Matrix<Node> matrix, int nodeIndex, Span<int> visitedIndexes, ref int visitedIndexCount)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(visitedIndexCount >= 0);

            visitedIndexes[visitedIndexCount] = nodeIndex;
            visitedIndexCount++;

            Span<int> buffer = stackalloc int[8];
            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            foreach (var i in buffer)
            {
                if (i == -1) { continue; }

                var node = matrix.Nodes[i];

                if (node.State == NodeState.Flagged) { continue; }
 
                if (node.State == NodeState.Hidden)
                {
                    matrix.Nodes[i] = new Node(false, node.MineCount, NodeOperation.Reveal);
                }

                if (node.MineCount == 0 && visitedIndexes.IndexOf(i) == -1)
                {
                    VisitNode(matrix, i, visitedIndexes, ref visitedIndexCount);
                }
            }
        }

        internal void Chord(Matrix<Node> matrix, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeIndex < matrix.Nodes.Length);

            Span<int> buffer = stackalloc int[8];
            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            foreach (var i in buffer)
            {
                if (i == -1) { continue; }
                if (matrix.Nodes[i].State != NodeState.Hidden) { continue; }

                var turn = new Turn(i, NodeOperation.Reveal);
                ComputeBoard(matrix, turn);
            }
        }
    }
}
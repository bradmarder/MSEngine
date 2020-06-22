using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public class Engine : IEngine
    {
        public static IEngine Instance { get; } = new Engine();
        public const byte MaxNodeEdges = 8;

        public virtual void FillBeginnerBoard(Span<Node> nodes) => FillCustomBoard(nodes, 10, 9);
        public virtual void FillIntermediateBoard(Span<Node> nodes) => FillCustomBoard(nodes, 40, 16);
        public virtual void FillExpertBoard(Span<Node> nodes) => FillCustomBoard(nodes, 99, 30);
        public virtual void FillCustomBoard(Span<Node> nodes, int mineCount, byte columns)
        {
            Span<int> mines = stackalloc int[mineCount];
            mines.Scatter(nodes.Length);

            FillCustomBoard(nodes, mines, columns);
        }
        public virtual void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns)
        {
            Debug.Assert(columns > 0);
            Debug.Assert(nodes.Length > mines.Length);
            Debug.Assert(nodes.Length % columns == 0);

            Span<int> buffer = stackalloc int[MaxNodeEdges];

            for (var i = 0; i < nodes.Length; i++)
            {
                var hasMine = mines.Contains(i);
                var mineCount = Utilities.GetAdjacentMineCount(mines, buffer, i, nodes.Length, columns);
                
                nodes[i] = new Node(i, hasMine, mineCount);
            }
        }

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
                Span<int> adjacentIndexes = stackalloc int[MaxNodeEdges];
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

            switch (turn.Operation)
            {
                case NodeOperation.Reveal:
                    if (node.State == NodeState.Revealed)
                    {
                        break;
                    }
                    if (node.HasMine)
                    {
                        RevealHiddenMines(matrix.Nodes);
                    }
                    else
                    {
                        matrix.Nodes[turn.NodeIndex] = new Node(node, NodeState.Revealed);
                        if (node.MineCount == 0)
                        {
                            TriggerChainReaction(matrix, turn.NodeIndex);
                        }
                    }
                    break;
                case NodeOperation.Flag:
                    if (node.State == NodeState.Flagged)
                    {
                        break;
                    }

                    matrix.Nodes[turn.NodeIndex] = new Node(node, NodeState.Flagged);
                    break;
                case NodeOperation.RemoveFlag:
                    matrix.Nodes[turn.NodeIndex] = new Node(node, NodeState.Hidden);
                    break;
                case NodeOperation.Chord:
                    Chord(matrix, turn.NodeIndex);
                    break;
                default:
                    Debug.Fail(turn.Operation.ToString());
                    break;
            }
        }
        internal void Chord(Matrix<Node> matrix, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(nodeIndex < matrix.Nodes.Length);

            Span<int> buffer = stackalloc int[MaxNodeEdges];
            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            foreach (var i in buffer)
            {
                if (i == -1) { continue; }
                if (matrix.Nodes[i].State != NodeState.Hidden) { continue; }

                var turn = new Turn(i, NodeOperation.Reveal);
                ComputeBoard(matrix, turn);
            }
        }

        internal static void RevealHiddenMines(Span<Node> nodes)
        {
            foreach (ref var node in nodes)
            {
                if (node.HasMine && node.State == NodeState.Hidden)
                {
                    node = new Node(node, NodeState.Revealed);
                }
            }
        }

        internal static void TriggerChainReaction(Matrix<Node> matrix, int nodeIndex)
        {
            Debug.Assert(nodeIndex >= 0);

            Span<int> visitedIndexes = stackalloc int[matrix.Nodes.Length]; //  subtract nodes.MineCount() ?
            visitedIndexes.Fill(-1);

            VisitNode(matrix, nodeIndex, visitedIndexes, visitedIndexes.GetEnumerator());
        }

        internal static void VisitNode(Matrix<Node> matrix, int nodeIndex, ReadOnlySpan<int> visitedIndexes, Span<int>.Enumerator enumerator)
        {
            Debug.Assert(nodeIndex >= 0);

            var pass = enumerator.MoveNext();
            Debug.Assert(pass);
            enumerator.Current = nodeIndex;

            Span<int> buffer = stackalloc int[MaxNodeEdges];
            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            foreach (var i in buffer)
            {
                if (i == -1) { continue; }

                ref var node = ref matrix.Nodes[i];

                if (node.State == NodeState.Flagged) { continue; }

                if (node.State == NodeState.Hidden)
                {
                    node = new Node(node, NodeState.Revealed);
                }

                if (node.MineCount == 0 && !visitedIndexes.Contains(i))
                {
                    VisitNode(matrix, i, visitedIndexes, enumerator);
                }
            }
        }
    }
}
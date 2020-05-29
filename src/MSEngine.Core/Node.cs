using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSEngine.Core
{
    public readonly struct Node : IEquatable<Node>
    {
        private static readonly IReadOnlyDictionary<NodeOperation, NodeState> _operationToStateMap = new Dictionary<NodeOperation, NodeState>
        {
            [NodeOperation.Flag] = NodeState.Flagged,
            [NodeOperation.RemoveFlag] = NodeState.Hidden,
            [NodeOperation.Reveal] = NodeState.Revealed
        };

        public Node(bool hasMine, int mineCount, NodeOperation op)
        {
            Debug.Assert(Enum.IsDefined(typeof(NodeOperation), op));

            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = _operationToStateMap[op];
        }
        public Node(bool hasMine, int mineCount, NodeState state)
        {
            Debug.Assert(Enum.IsDefined(typeof(NodeState), state));

            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = state;
        }
        internal Node(bool hasMine, int mineCount)
        {
            Debug.Assert(mineCount >= 0);
            Debug.Assert(mineCount <= 8);
            
            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = NodeState.Hidden;
        }

        public bool HasMine { get; }
        public byte MineCount { get; }
        public NodeState State { get; }

        public bool HasMineExploded => HasMine && State == NodeState.Revealed;
        public bool SatisfiesWinningCriteria =>
            HasMine
                ? State == NodeState.Flagged
                : State == NodeState.Revealed;

        public override string ToString() =>
            $"{nameof(HasMine)}: {HasMine}, {nameof(State)}: {State}, {nameof(MineCount)}: {MineCount}";
        public override int GetHashCode() => HashCode.Combine(HasMine, State, MineCount);
        public override bool Equals(object? obj) => obj is Node x && Equals(x);
        public bool Equals(Node other) =>
            HasMine == other.HasMine
            && State == other.State
            && MineCount == other.MineCount;
        public static bool operator ==(Node c1, Node c2) => c1.Equals(c2);
        public static bool operator !=(Node c1, Node c2) => !(c1 == c2);
    }
}

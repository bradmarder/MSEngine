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

        internal Node(bool hasMine, int mineCount)
        {
            Debug.Assert(mineCount >= 0);
            Debug.Assert(mineCount <= 8);
            
            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = NodeState.Hidden;
        }

        internal Node(bool hasMine, int mineCount, NodeOperation operation)
        {
            Debug.Assert(mineCount >= 0);
            Debug.Assert(mineCount <= 8);
            Debug.Assert(Enum.IsDefined(typeof(NodeOperation), operation));

            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = _operationToStateMap[operation];
        }

        public bool HasMine { get; }
        public NodeState State { get; }

        /// <summary>
        /// Range from 0 to 8
        /// </summary>
        public byte MineCount { get; }

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
        public static bool operator ==(in Node c1, in Node c2) => c1.Equals(c2);
        public static bool operator !=(in Node c1, in Node c2) => !(c1 == c2);
    }
}

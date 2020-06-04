using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public readonly struct Node : IEquatable<Node>
    {
        internal Node(in Node node, NodeOperation op)
        {
            Debug.Assert(Enum.IsDefined(typeof(NodeOperation), op));

            Index = node.Index;
            HasMine = node.HasMine;
            MineCount = node.MineCount;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
            State = op switch
            {
                NodeOperation.Reveal => NodeState.Revealed,
                NodeOperation.Flag => NodeState.Flagged,
                NodeOperation.RemoveFlag => NodeState.Hidden
            };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        }
        internal Node(int index, bool hasMine, int mineCount, NodeState state)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(mineCount >= 0);
            Debug.Assert(mineCount <= 8);
            Debug.Assert(Enum.IsDefined(typeof(NodeState), state));

            Index = index;
            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = state;
        }
        internal Node(int index, bool hasMine, int mineCount)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(mineCount >= 0);
            Debug.Assert(mineCount <= 8);

            Index = index;
            HasMine = hasMine;
            MineCount = (byte)mineCount;
            State = NodeState.Hidden;
        }

        public int Index { get; }
        public bool HasMine { get; }
        public byte MineCount { get; }
        public NodeState State { get; }

        public override string ToString() =>
            $"{nameof(Index)}: {Index}, {nameof(HasMine)}: {HasMine}, {nameof(State)}: {State}, {nameof(MineCount)}: {MineCount}";
        public override int GetHashCode() => HashCode.Combine(Index, HasMine, State, MineCount);
        public override bool Equals(object? obj) => obj is Node x && Equals(x);
        public bool Equals(Node other) =>
            Index == other.Index
            && HasMine == other.HasMine
            && State == other.State
            && MineCount == other.MineCount;
        public static bool operator ==(in Node c1, in Node c2) => c1.Equals(c2);
        public static bool operator !=(in Node c1, in Node c2) => !(c1 == c2);
    }
}

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Core
{
    public readonly struct Node : IEquatable<Node>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Node(int index, bool hasMine, byte mineCount) : this(index, hasMine, mineCount, NodeState.Hidden) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Node(in Node node, NodeState state) : this(node.Index, node.HasMine, node.MineCount, state) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Node(int index, bool hasMine, byte mineCount, NodeState state)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(mineCount >= 0);
            Debug.Assert(mineCount <= 8);
            Debug.Assert(Enum.IsDefined(typeof(NodeState), state));

            Index = index;
            HasMine = hasMine;
            MineCount = mineCount;
            State = state;
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

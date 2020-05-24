using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public readonly struct Turn : IEquatable<Turn>
    {
        public Turn(int nodeIndex, NodeOperation operation)
        {
            Debug.Assert(nodeIndex >= 0);

            // Despite being a public API, we assert instead of throw because otherwise this method will allocate on the heap
            Debug.Assert(Enum.IsDefined(typeof(NodeOperation), operation));

            NodeIndex = nodeIndex;
            Operation = operation;
        }

        public int NodeIndex { get; }
        public NodeOperation Operation { get; }

        public override string ToString() => $"{nameof(NodeIndex)}: {NodeIndex}, {nameof(Operation)}: {Operation}";
        public override int GetHashCode() => HashCode.Combine(NodeIndex, Operation);
        public override bool Equals(object? obj) => obj is Turn x && Equals(x);
        public bool Equals(Turn other) => NodeIndex == other.NodeIndex && Operation == other.Operation;
        public static bool operator ==(Turn c1, Turn c2) => c1.Equals(c2);
        public static bool operator !=(Turn c1, Turn c2) => !(c1 == c2);
    }
}

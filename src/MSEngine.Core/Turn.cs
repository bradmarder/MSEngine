using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MSEngine.Core
{
    public readonly record struct Turn
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Turn(int nodeIndex, NodeOperation operation)
        {
            Debug.Assert(nodeIndex >= 0);
            Debug.Assert(Enum.IsDefined(operation));

            NodeIndex = nodeIndex;
            Operation = operation;
        }

        public int NodeIndex { get; init; }
        public NodeOperation Operation { get; init; }

        public string NewTurnCtor() => $"new {nameof(Turn)}({NodeIndex}, {nameof(NodeOperation)}.{Operation}),";
    }
}

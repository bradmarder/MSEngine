using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public readonly struct Turn : IEquatable<Turn>
    {
        public Turn(int tileIndex, TileOperation operation)
        {
            Debug.Assert(tileIndex >= 0);

            // Despite being a public API, we assert instead of throw because otherwise this method will allocate on the heap
            Debug.Assert(Enum.IsDefined(typeof(TileOperation), operation));

            TileIndex = tileIndex;
            Operation = operation;
        }

        public int TileIndex { get; }
        public TileOperation Operation { get; }

        public override string ToString() => $"{nameof(TileIndex)}: {TileIndex}, {nameof(Operation)}: {Operation}";
        public override int GetHashCode() => HashCode.Combine(TileIndex, Operation);
        public override bool Equals(object? obj) => obj is Turn x && Equals(x);
        public bool Equals(Turn other) => TileIndex == other.TileIndex && Operation == other.Operation;
        public static bool operator ==(Turn c1, Turn c2) => c1.Equals(c2);
        public static bool operator !=(Turn c1, Turn c2) => !(c1 == c2);
    }
}

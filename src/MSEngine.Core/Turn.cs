using System;
using System.Diagnostics;

namespace MSEngine.Core
{
    public readonly struct Turn : IEquatable<Turn>
    {
        public Turn(byte x, byte y, TileOperation operation) : this(new Coordinates(x, y), operation) { }

        public Turn(Coordinates coordinates, TileOperation operation)
        {
            // Despite being a public API, we assert instead of throw because otherwise this method will allocate on the heap
            Debug.Assert(Enum.IsDefined(typeof(TileOperation), operation));

            Coordinates = coordinates;
            Operation = operation;
        }

        public Coordinates Coordinates { get; }
        public TileOperation Operation { get; }

        public override string ToString() => $"{nameof(Coordinates)}: {Coordinates}, {nameof(Operation)}: {Operation}";
        public override int GetHashCode() => HashCode.Combine(Coordinates, Operation);
        public override bool Equals(object? obj) => obj is Turn x && Equals(x);
        public bool Equals(Turn other) => Coordinates == other.Coordinates && Operation == other.Operation;
        public static bool operator ==(Turn c1, Turn c2) => c1.Equals(c2);
        public static bool operator !=(Turn c1, Turn c2) => !(c1 == c2);
    }
}

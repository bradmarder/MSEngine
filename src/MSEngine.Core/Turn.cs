using System;

namespace MSEngine.Core
{
    public readonly struct Turn : IEquatable<Turn>
    {
        public Turn(byte x, byte y, TileOperation operation) : this(new Coordinates(x, y), operation) { }

        public Turn(Coordinates coordinates, TileOperation operation)
        {
            if (!Enum.IsDefined(typeof(TileOperation), operation)) { throw new ArgumentOutOfRangeException(nameof(operation)); }

            Coordinates = coordinates;
            Operation = operation;
        }

        public Coordinates Coordinates { get; }
        public TileOperation Operation { get; }

        public override string ToString() => $"{nameof(Coordinates)}: {Coordinates}, {nameof(Operation)}: {Operation}";
        public override int GetHashCode() => (Coordinates, Operation).GetHashCode();
        public override bool Equals(object obj) => obj is Turn x && Equals(x);
        public bool Equals(Turn other) => Coordinates == other.Coordinates && Operation == other.Operation;
        public static bool operator ==(Turn c1, Turn c2) => c1.Equals(c2);
        public static bool operator !=(Turn c1, Turn c2) => !(c1 == c2);
    }
}

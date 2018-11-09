using System;

namespace MSEngine.Core
{
    public readonly struct Coordinates : IEquatable<Coordinates>
    {
        internal Coordinates(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public byte X { get; }
        public byte Y { get; }

        public override string ToString() => $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
        public override int GetHashCode() => (X, Y).GetHashCode();
        public override bool Equals(object obj) => obj is Coordinates x && Equals(x);
        public bool Equals(Coordinates other) => X == other.X && Y == other.Y;
        public static bool operator ==(Coordinates c1, Coordinates c2) => c1.Equals(c2);
        public static bool operator !=(Coordinates c1, Coordinates c2) => !(c1 == c2);
    }
}

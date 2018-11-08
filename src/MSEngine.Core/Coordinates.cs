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

        // TODO: use HashCode.Combine(X, Y) with netstandard 2.1 
        // copy/pasted from https://stackoverflow.com/a/1646913/2089286
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X.GetHashCode();
                hash = hash * 31 + Y.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj) => obj is Coordinates x && Equals(x);
        public bool Equals(Coordinates other) => X == other.X && Y == other.Y;
        public static bool operator ==(Coordinates c1, Coordinates c2) => c1.Equals(c2);
        public static bool operator !=(Coordinates c1, Coordinates c2) => !(c1 == c2);
    }
}

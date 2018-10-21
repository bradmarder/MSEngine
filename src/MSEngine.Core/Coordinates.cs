using System;
using System.Linq;

namespace MSEngine.Core
{
    public readonly struct Coordinates : IEquatable<Coordinates>
    {
        public Coordinates(in byte x, in byte y)
        {
            X = x;
            Y = y;
        }

        public byte X { get; }
        public byte Y { get; }

        public override int GetHashCode() => X ^ Y;
        public override bool Equals(object obj) => base.Equals(obj);
        public bool Equals(Coordinates other) => X == other.X && Y == other.Y;
        public static bool operator ==(Coordinates c1, Coordinates c2) => c1.Equals(c2);
        public static bool operator !=(Coordinates c1, Coordinates c2) => !c1.Equals(c2);
    }
}

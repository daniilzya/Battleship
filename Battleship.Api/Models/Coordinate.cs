using System;

namespace Battleship.Api.Models
{
    internal struct Coordinate : IEquatable<Coordinate>
    {
        public Coordinate(int x, int y) 
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public static bool operator == (Coordinate left, Coordinate right) => Equals(left, right);

        public static bool operator != (Coordinate left, Coordinate right) => !Equals(left, right);

        public override bool Equals(object obj) => (obj is Coordinate metrics) && Equals(metrics);

        public bool Equals(Coordinate other) => (X, Y) == (other.X, other.Y);

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}

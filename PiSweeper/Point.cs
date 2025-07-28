using System;
using System.Globalization;

namespace PiSweeper;

public readonly struct Point(int x, int y) : IEquatable<Point>
{
    public int X => x;
    public int Y => y;
    
    public static bool operator ==(Point left, Point right) => left.Equals(right);

    public static bool operator !=(Point left, Point right) => !(left == right);

    public override bool Equals(object? obj) => obj is Point other && Equals(other);

    public bool Equals(Point other) => X == other.X && Y == other.Y;

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 23) + x.GetHashCode();
            hash = (hash * 23) + y.GetHashCode();
            return hash;
        }
    }

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0}|{1})", X, Y);
}
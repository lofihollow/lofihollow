using SadRogue.Primitives;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LofiHollow.DataTypes {
    public class Point3D : IEquatable<Point3D> {
        public string WorldArea;
        public int X;
        public int Y;
        public int Z;

        [JsonConstructor]
        public Point3D() { }

        public Point3D(int x, int y, int z) {
            WorldArea = "Overworld";
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D(string w, int x, int y, int z) {
            WorldArea = w;
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj) {
            if (!(obj is Point3D))
                return false;

            return Equals((Point3D)obj);
        }

        public bool Equals(Point3D other) {
            if (Z != other.Z)
                return false;

            return (Y == other.Y && X == other.X && WorldArea == other.WorldArea);
        }

        public static bool operator ==(Point3D point1, Point3D point2) {
            return point1.Equals(point2);
        }

        public static bool operator !=(Point3D point1, Point3D point2) {
            return !point1.Equals(point2);
        }

        public static Point3D operator+ (Point3D a, Point3D b) {
            return new Point3D(a.WorldArea, a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3D operator- (Point3D a, Point3D b) {
            return new Point3D(a.WorldArea, a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public override int GetHashCode() {
            return X ^ Y ^ Z;
        }

        public override string ToString() {
            return WorldArea + ";" + X + ";" + Y + ";" + Z;
        } 
    }
}

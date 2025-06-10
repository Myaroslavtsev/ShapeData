/// Data structure. Defines some classes to store different geometric data

using System;
using System.Numerics;

namespace ShapeData.Geometry
{
    public class Direction // Вектор в 3D-пространстве, направленный в плоскости X-Z под углом A к оси Z
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double A { get; set; } // rotation around Y axis

        public Direction(double x = 0, double y = 0, double z = 0, double a = 0)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
        }
    }

    public class Trajectory  : IComparable
    {
        public double Straight { get; set; }
        public double Radius { get; set; }
        public double Angle { get; set; }

        public Trajectory(double straight = 0, double radius = 0, double angle = 0)
        {
            Straight = straight;
            Radius = radius;
            Angle = angle;
        }

        public double Length => Math.Abs(Straight) + Math.Abs(Radius * Angle * Math.PI / 180);

        public static bool operator >(Trajectory a, Trajectory b)
        {
            return a.Length > b.Length;
        }

        public static bool operator <(Trajectory a, Trajectory b)
        {
            return a.Length < b.Length;
        }

        public int CompareTo(object obj)
        {
            return Length.CompareTo(obj);
        }

        public static Trajectory operator +(Trajectory a, Trajectory b)
        {
            if (a.Radius == b.Radius)
                return new Trajectory(a.Straight + b.Straight, a.Radius, a.Angle + b.Angle);
            else
                return new Trajectory();
        }

        public static Trajectory operator -(Trajectory a, Trajectory b)
        {
            if (a.Radius == b.Radius)
                return new Trajectory(a.Straight - b.Straight, a.Radius, a.Angle - b.Angle);
            else
                return new Trajectory();
        }

        public static Trajectory operator *(Trajectory a, double scale) =>
            new(a.Straight * scale, a.Radius, a.Angle * scale);

        public static Trajectory operator /(Trajectory a, double scale) =>
            new(a.Straight / scale, a.Radius, a.Angle / scale);

        public static Trajectory operator %(Trajectory a, double scale) =>
            new(a.Straight % scale, a.Radius, a.Angle / scale);
    }

    public class PlaneVectors
    {
        public Vector3 Origin;
        public Vector3 Xdir;
        public Vector3 Ydir;
        public Vector3 Normal;

        public PlaneVectors(Vector3 origin, Vector3 xdir, Vector3 ydir, Vector3 normal)
        {
            Origin = origin;
            Xdir = xdir;
            Ydir = ydir;
            Normal = normal;
        }
    }
}

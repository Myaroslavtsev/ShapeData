using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ShapeData.Geometry
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point(double x = 0, double y = 0, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void ReplaceBy(Point point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }
    }

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

    public class Trajectory
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

        public double Length
        {
            get {
                if (Radius == 0)
                    return Straight;
                else
                    return Radius * Angle * Math.PI / 180;
            }
        }
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

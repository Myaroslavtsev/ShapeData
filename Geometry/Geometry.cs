using System;
using System.Collections.Generic;
using System.Numerics;

namespace ShapeData.Geometry
{
    class Geometry
    {
        public static Direction FindEndDirection(Trajectory trajectory, Direction startDirection)
        {
            // Из какой точки в каком направлении будет смотреть вектор направления,
            // если из начального направления пройти по заданной траектории

            // 0. Перевод угла в радианы
            double startAngle = Deg2Rad(startDirection.A);

            // 1. Координаты конца прямого отрезка
            double straightEndX = startDirection.X - trajectory.Straight * Math.Sin(startAngle);
            double straightEndZ = startDirection.Z + trajectory.Straight * Math.Cos(startAngle);

            // 2. Направление к центру дуги (вектор нормали)
            double curveSide = (trajectory.Angle > 0) ? -1 : 1; // Влево (CCW) или вправо (CW)
            double normal_angle_rad = startAngle + curveSide * Math.PI / 2.0;

            // 3. Центр дуги
            double curveCenterX = straightEndX + trajectory.Radius * Math.Sin(normal_angle_rad);
            double curveCenterZ = straightEndZ - trajectory.Radius * Math.Cos(normal_angle_rad);

            // 4. Угол между началом дуги и концом дуги
            double curveAngle = Deg2Rad(trajectory.Angle);

            // 5. Начальный угол дуги относительно центра
            double curveStartAngle = Math.Atan2(straightEndZ - curveCenterZ, straightEndX - curveCenterX);

            // 6. Конечный угол дуги
            double curveEndAngle = curveStartAngle + curveAngle;

            // 7. Конечная точка дуги
            double Xend = curveCenterX + trajectory.Radius * Math.Cos(curveEndAngle);
            double Zend = curveCenterZ + trajectory.Radius * Math.Sin(curveEndAngle);

            // 8. Конечный угол касательной
            double endAngle = startAngle + curveAngle;
            double Aend = Rad2Deg(endAngle) % 360.0;
            if (Aend < 0) Aend += 360.0;

            // 9. Вывод результатов
            return new Direction(Xend, startDirection.Y, Zend, Aend);
        }

        public static void TransposePoint(Point point, Direction direction)
        {
            var rotAngle = -Deg2Rad(direction.A);

            var newX = direction.X + point.X * Math.Cos(rotAngle) - point.Z * Math.Sin(rotAngle);
            var newY = direction.Y + point.Y;
            var newZ = direction.Z + point.X * Math.Sin(rotAngle) + point.Z * Math.Cos(rotAngle);

            point.X = newX;
            point.Y = newY;
            point.Z = newZ;
        }

        public static void BendPoint(Point point, Trajectory trajectory, double scaleFactor)
        {
            if (trajectory.Radius == 0)
                point.Z *= scaleFactor;
            else
            {
                double radius = trajectory.Radius;

                double angle = point.Z * scaleFactor / radius;

                var newX = (radius - Math.Cos(angle) * (radius + point.X)) * Math.Sign(trajectory.Angle);
                var newZ = Math.Sin(angle) * (radius + point.X);

                point.X = newX;
                point.Z = newZ;
            }
        }

        public static List<(double U, double V)> MakeSomeUVcoords(List<Point> points) =>
            ScaleToUnitSquare(ProjectPointsToPlane(points, MakePlaneFromFirstPoints(points)));

        private static PlaneVectors MakePlaneFromFirstPoints(List<Point> points)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("At least 3 points required to set a plane.");

            var origin = ToVector(points[0]);   // Центр координат
            var uDir = Vector3.Normalize(ToVector(points[1]) - origin); // Направление оси U

            // Вектор, задающий наклон плоскости
            var tiltVec = ToVector(points[2]) - origin;

            // Вектор нормали к плоскости
            var normal = Vector3.Normalize(Vector3.Cross(uDir, tiltVec));

            // Направление оси V — перпендикулярно U и normal (против часовой стрелки)
            var vDir = Vector3.Normalize(Vector3.Cross(uDir, normal));

            return new PlaneVectors(origin, uDir, vDir, normal);
        }

        private static List<(double U, double V)> ProjectPointsToPlane(List<Point> points, PlaneVectors plane)
        {
            var result = new List<(double U, double V)>();

            for (int i = 0; i < points.Count; i++)
            {
                var p = ToVector(points[i]);
                var vecToP = p - plane.Origin;

                // Проекция вектора на плоскость через вычитание компоненты вдоль нормали
                var distanceToPlane = Vector3.Dot(vecToP, plane.Normal);
                var projection = p - distanceToPlane * plane.Normal;
                var projectedVec = projection - plane.Origin;

                // Координаты в системе U-V
                double u = Vector3.Dot(projectedVec, plane.Xdir);
                double v = Vector3.Dot(projectedVec, plane.Ydir);

                result.Add((u, v));
            }

            return result;
        }


        private static List<(double U, double V)> ScaleToUnitSquare(List<(double U, double V)> dots)
        {
            var minU = double.MaxValue;
            var minV = double.MaxValue;
            var maxU = double.MinValue;
            var maxV = double.MinValue;

            foreach (var (U, V) in dots)
            {
                if (U < minU) minU = U;
                if (V < minV) minV = V;
                if (U > maxU) maxU = U;
                if (V > maxV) maxV = V;
            }

            var maxBound = Math.Max(maxU - minU, maxV - minV);
            if (maxBound < 1e-3)
                maxBound = 1;

            var result = new List<(double U, double V)>();

            foreach (var (U, V) in dots)
                result.Add(((U - minU) / maxBound, (V - minV) / maxBound));

            return result;
        }

        private static Vector3 ToVector(Point p) => new((float)p.X, (float)p.Y, (float)p.Z);

        public static double Deg2Rad(double degrees) => degrees * Math.PI / 180;

        public static double Rad2Deg(double radians) => radians * 180 / Math.PI;

    }
}

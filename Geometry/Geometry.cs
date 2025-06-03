using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData.Geometry
{
    class Geometry
    {
        public static Direction FindEndDirection(Trajectory trajectory, Direction startDirection)
        {
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

            point.X = direction.X + point.X * Math.Cos(rotAngle) - point.Z * Math.Sin(rotAngle);
            point.Y = direction.Y + point.Y;
            point.Z = direction.Z + point.X * Math.Sin(rotAngle) + point.Z * Math.Cos(rotAngle);
        }

        public static void BendPoint(Point point, Trajectory trajectory, double scaleFactor)
        {
            if (trajectory.Radius == 0)
                point.Z *= scaleFactor;
            else
            {
                double radius = trajectory.Radius;

                double angle = point.Z * scaleFactor / radius;

                point.X = (radius - Math.Cos(angle) * (radius + point.X)) * Math.Sign(trajectory.Angle);
                point.Z = Math.Sin(angle) * (radius + point.X);
            }
        }

        public static double Deg2Rad(double degrees) => degrees * Math.PI / 180;

        public static double Rad2Deg(double radians) => radians * 180 / Math.PI;

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Extensions
{
    public static class Vector2Extensions
    {
        public static Point AsPoint(this Vector2 pos)
        {
            return new Point((int) pos.X, (int) pos.Y);
        }

        public static double Pow(this float x)
        {
            return Math.Pow(x, 2);
        }

        public static float AngleBetween(this Vector2 self, Vector2 v1, Vector2 v2)
        {
            var p1 = (-self + v1);
            var p2 = (-self + v2);

            var theta = p1.Polar() - p2.Polar();

            if (theta < 0)
            {
                theta += 180;
            }

            if (theta > 180)
            {
                theta = 360 - theta;
            }

            return theta;
        }

        public static float Polar(this Vector2 v)
        {
            if (Math.Abs(v.X) < float.Epsilon)
            {
                if (v.Y > 0)
                {
                    return 90;
                }

                return v.Y < 0 ? 270 : 0;
            }

            var theta = Math.Atan(v.Y / v.X) * Math.PI / 180;

            if (v.X < 0)
            {
                theta += 180;
            }
            if (theta < 0)
            {
                theta += 360;
            }

            return (float) theta;
        }
    }

}

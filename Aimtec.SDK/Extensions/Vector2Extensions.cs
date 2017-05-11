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
    }

}

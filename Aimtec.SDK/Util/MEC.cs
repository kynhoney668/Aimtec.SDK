// ReSharper disable RedundantAssignment
namespace Aimtec.SDK.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Class MEC.
    /// </summary>
    /// <remarks>
    ///     Best Case  O(1) - 3 points or less
    ///     Worst Case O(n) - Greater than 3 points
    /// </remarks>
    public class Mec
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Finds the minimum enclosing circle.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>MinimumEnclosingCircleResult.</returns>
        /// <exception cref="ArgumentNullException">points</exception>
        /// <exception cref="ArgumentException">Cannot find the MEC of an empty array.</exception>
        public static MinimumEnclosingCircleResult FindMinimumEnclosingCircle(Vector2[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (points.Length <= 0)
            {
                throw new ArgumentException("Cannot find the MEC of an empty array.");
            }

            return points.Length >= 3
                ? FindEnclosingCircleWithThreePointsOrLess(points)
                : InternalGetMinimumEnclosingCircle(points);
        }

        /// <summary>
        ///     Finds the minimum enclosing circle.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>MinimumEnclosingCircleResult.</returns>
        public static MinimumEnclosingCircleResult FindMinimumEnclosingCircle(IEnumerable<Vector2> points)
        {
            return FindMinimumEnclosingCircle(points.ToArray());
        }

        #endregion

        #region Methods

        private static float CalculateL2Norm(Vector2 v)
        {
            return (float) Math.Sqrt(Math.Pow(v.X, 2) + Math.Pow(v.Y, 2));
        }

        private static MinimumEnclosingCircleResult FindCircle3Pts(IList<Vector2> pts)
        {
            var center = new Vector2();
            var radius = 0f;

            var v1 = pts[1] - pts[0];
            var v2 = pts[2] - pts[0];

            if (Math.Abs(Vector2.Dot(v1, v2)) < float.Epsilon)
            {
                var d1 = CalculateL2Norm(pts[0] - pts[1]);
                var d2 = CalculateL2Norm(pts[0] - pts[2]);
                var d3 = CalculateL2Norm(pts[1] - pts[2]);

                if (d1 >= d2 && d1 >= d3)
                {
                    center = (pts[0] + pts[1]) / 2f;
                    radius = d1 / 2f;
                }
                else if (d2 >= d1 && d2 >= d3)
                {
                    center = (pts[0] + pts[2]) / 2f;
                    radius = d2 / 2f;
                }
                else if (d3 >= d1 && d3 >= d2)
                {
                    center = (pts[1] + pts[2]) / 2f;
                    radius = d3 / 2f;
                }
            }
            else
            {
                var midPoint1 = (pts[0] + pts[1]) / 2f;
                var c1 = midPoint1.X * v1.X + midPoint1.Y * v1.Y;

                var midPoint2 = (pts[0] + pts[2]) / 2f;
                var c2 = midPoint2.X * v2.X + midPoint2.Y * v2.Y;

                var det = v1.X * v2.Y - v1.Y * v2.X;
                var cx = (c1 * v2.Y - c2 * v1.Y) / det;
                var cy = (v1.X * c2 - v2.X * c1) / det;

                center.X = cx;
                center.Y = cy;

                cx -= pts[0].X;
                cy -= pts[0].Y;

                radius = (float) Math.Sqrt(cx * cx + cy * cy);
            }

            return new MinimumEnclosingCircleResult(center, radius);
        }

        private static MinimumEnclosingCircleResult FindEnclosingCircleWithThreePointsOrLess(IList<Vector2> points)
        {
            var center = new Vector2();
            var radius = 0f;

            switch (points.Count)
            {
                case 1:
                    center = points[0];
                    radius = 0f;
                    break;
                case 2:
                    center.X = (points[0].X + points[1].X) / 2f;
                    center.Y = (points[0].Y + points[1].Y) / 2f;
                    radius = Vector2.Distance(points[0], points[1]) / 2f;
                    break;
                case 3: return FindCircle3Pts(points);
            }

            return new MinimumEnclosingCircleResult(center, radius);
        }

        private static void FindSecondPoint(IList<Vector2> pts, int i, ref Vector2 center, ref float radius)
        {
            center.X = (pts[0].X + pts[i].X) / 2f;
            center.Y = (pts[0].Y + pts[i].Y) / 2f;

            var dx = pts[0].X - pts[i].X;
            var dy = pts[0].Y - pts[i].Y;

            radius = CalculateL2Norm(new Vector2(dx, dy)) / 2f;

            for (var j = 1; j < i; ++j)
            {
                dx = center.X - pts[j].X;
                dy = center.Y - pts[j].Y;

                if (CalculateL2Norm(new Vector2(dx, dy)) < radius)
                {
                    continue;
                }

                FindThirdPoint(pts, i, j, ref center, ref radius);
            }
        }

        private static void FindThirdPoint(IList<Vector2> pts, int i, int j, ref Vector2 center, ref float radius)
        {
            center.X = (pts[j].X + pts[i].X) / 2f;
            center.Y = (pts[j].Y + pts[i].Y) / 2f;

            var dx = pts[j].X - pts[i].X;
            var dy = pts[j].Y - pts[i].Y;

            radius = CalculateL2Norm(new Vector2(dx, dy)) / 2f;

            for (var k = 0; k < j; ++k)
            {
                dx = center.X - pts[k].X;
                dy = center.Y - pts[k].Y;

                if (CalculateL2Norm(new Vector2(dx, dy)) < radius)
                {
                    continue;
                }

                var ptsf = new Vector2[3];
                ptsf[0] = pts[i];
                ptsf[1] = pts[j];
                ptsf[2] = pts[k];

                var result = FindCircle3Pts(ptsf);
                center = result.Center;
                radius = result.Radius;
            }
        }

        private static MinimumEnclosingCircleResult InternalGetMinimumEnclosingCircle(IList<Vector2> pts)
        {
            Vector2 center;

            center.X = (pts[0].X + pts[1].X) / 2f;
            center.Y = (pts[0].Y + pts[1].Y) / 2f;

            var dx = pts[0].X - pts[1].X;
            var dy = pts[0].Y - pts[1].Y;

            var radius = CalculateL2Norm(new Vector2(dx, dy)) / 2f;

            for (var i = 2; i < pts.Count; ++i)
            {
                dx = pts[i].X - center.X;
                dy = pts[i].Y - center.Y;

                var d = CalculateL2Norm(new Vector2(dx, dy));

                if (d < radius)
                {
                    continue;
                }

                FindSecondPoint(pts, i, ref center, ref radius);
            }

            return new MinimumEnclosingCircleResult(center, radius);
        }

        #endregion
    }

    /// <summary>
    ///     Struct MinimumEnclosingCircleResult
    /// </summary>
    public struct MinimumEnclosingCircleResult
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MinimumEnclosingCircleResult" /> struct.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        internal MinimumEnclosingCircleResult(Vector2 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the center.
        /// </summary>
        /// <value>The center.</value>
        public Vector2 Center { get; set; }

        /// <summary>
        ///     Gets or sets the radius.
        /// </summary>
        /// <value>The radius.</value>
        public float Radius { get; set; }

        #endregion
    }
}
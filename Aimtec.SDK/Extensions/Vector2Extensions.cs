namespace Aimtec.SDK.Extensions
{
    using System;
    using System.Drawing;
    using System.Linq;

    /// <summary>
    /// Class Vector2Extensions.
    /// </summary>
    public static class Vector2Extensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the angle between <paramref name="self" />, the center, and two points.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <returns>System.Single.</returns>
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

        /// <summary>
        ///     Ases the point.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>Point.</returns>
        public static Point AsPoint(this Vector2 pos)
        {
            return new Point((int) pos.X, (int) pos.Y);
        }

        /// <summary>
        ///     Gets distance squared from the segments.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="segmentStart">The segment start.</param>
        /// <param name="segmentEnd">The segment end.</param>
        /// <param name="onlyIfOnSegment">if set to <c>true</c> [only if on segment].</param>
        /// <returns>System.Single.</returns>
        public static float DistanceSquared(
            this Vector2 point,
            Vector2 segmentStart,
            Vector2 segmentEnd,
            bool onlyIfOnSegment = false)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);

            return (objects.IsOnSegment || onlyIfOnSegment == false)
                ? Vector2.DistanceSquared(objects.SegmentPoint, point)
                : float.MaxValue;
        }

        /// <summary>
        ///     Intersects two line segments.
        /// </summary>
        /// <param name="lineSegment1Start">Line Segment 1 (Start)</param>
        /// <param name="lineSegment1End">Line Segment 1 (End)</param>
        /// <param name="lineSegment2Start">Line Segment 2 (Start)></param>
        /// <param name="lineSegment2End">Line Segment 2 (End)</param>
        /// <returns>The intersection result, <seealso cref="IntersectionResult" /></returns>
        public static IntersectionResult Intersection(
            this Vector2 lineSegment1Start,
            Vector2 lineSegment1End,
            Vector2 lineSegment2Start,
            Vector2 lineSegment2End)
        {
            double deltaACy = lineSegment1Start.Y - lineSegment2Start.Y;
            double deltaDCx = lineSegment2End.X - lineSegment2Start.X;
            double deltaACx = lineSegment1Start.X - lineSegment2Start.X;
            double deltaDCy = lineSegment2End.Y - lineSegment2Start.Y;
            double deltaBAx = lineSegment1End.X - lineSegment1Start.X;
            double deltaBAy = lineSegment1End.Y - lineSegment1Start.Y;

            var denominator = (deltaBAx * deltaDCy) - (deltaBAy * deltaDCx);
            var numerator = (deltaACy * deltaDCx) - (deltaACx * deltaDCy);

            if (Math.Abs(denominator) < float.Epsilon)
            {
                if (Math.Abs(numerator) < float.Epsilon)
                {
                    // collinear. Potentially infinite intersection points.
                    // Check and return one of them.
                    if (lineSegment1Start.X >= lineSegment2Start.X && lineSegment1Start.X <= lineSegment2End.X)
                    {
                        return new IntersectionResult(true, lineSegment1Start);
                    }

                    if (lineSegment2Start.X >= lineSegment1Start.X && lineSegment2Start.X <= lineSegment1End.X)
                    {
                        return new IntersectionResult(true, lineSegment2Start);
                    }

                    return default(IntersectionResult);
                }

                // parallel
                return default(IntersectionResult);
            }

            var r = numerator / denominator;
            if (r < 0 || r > 1)
            {
                return default(IntersectionResult);
            }

            var s = ((deltaACy * deltaBAx) - (deltaACx * deltaBAy)) / denominator;
            if (s < 0 || s > 1)
            {
                return default(IntersectionResult);
            }

            return new IntersectionResult(
                true,
                new Vector2(
                    (float) (lineSegment1Start.X + (r * deltaBAx)),
                    (float) (lineSegment1Start.Y + (r * deltaBAy))));
        }

        /// <summary>
        ///     Creates a new vector perpendicular to <paramref name="vector2" /> and shifted by the specified offset.
        /// </summary>
        /// <param name="vector2">The vector2.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Vector2.</returns>
        public static Vector2 Perpendicular(this Vector2 vector2, int offset = 0)
        {
            return (offset == 0) ? new Vector2(-vector2.Y, vector2.X) : new Vector2(vector2.Y, -vector2.X);
        }

        /// <summary>
        ///     Gets the polar angle.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>System.Single.</returns>
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

        /// <summary>
        ///     Raises <paramref name="x" /> to the second power.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>System.Double.</returns>
        public static double Pow(this float x)
        {
            return Math.Pow(x, 2);
        }

        public static bool PointUnderEnemyTurret(this Vector2 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsEnemy && Vector2.Distance(Point, t.Position.To2D()) < 950f + ObjectManager.GetLocalPlayer().BoundingRadius + t.BoundingRadius);

            return EnemyTurrets;
        }

 
        /// <summary>
        ///     Returns the projection of the Vector2 on the segment.
        /// </summary>
        /// <param name="point">The Point</param>
        /// <param name="segmentStart">Start of Segment</param>
        /// <param name="segmentEnd">End of Segment</param>
        /// <returns><see cref="ProjectionInfo" /> containing the projection.</returns>
        public static ProjectionInfo ProjectOn(this Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            var cx = point.X;
            var cy = point.Y;
            var ax = segmentStart.X;
            var ay = segmentStart.Y;
            var bx = segmentEnd.X;
            var by = segmentEnd.Y;
            var rL = (((cx - ax) * (bx - ax)) + ((cy - ay) * (by - ay)))
                / ((float) Math.Pow(bx - ax, 2) + (float) Math.Pow(by - ay, 2));
            var pointLine = new Vector2(ax + (rL * (bx - ax)), ay + (rL * (by - ay)));
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }

            var isOnSegment = Math.Abs(rS - rL) < float.Epsilon;
            var pointSegment = isOnSegment ? pointLine : new Vector2(ax + (rS * (bx - ax)), ay + (rS * (by - ay)));
            return new ProjectionInfo(isOnSegment, pointSegment, pointLine);
        }

        /// <summary>
        ///     Rotates the Vector2 to a set angle.
        /// </summary>
        /// <param name="vector2">Extended SharpDX Vector2</param>
        /// <param name="angle">Angle (in radians)</param>
        /// <returns>Rotated Vector2</returns>
        public static Vector2 Rotated(this Vector2 vector2, float angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Vector2(
                (float) ((vector2.X * cos) - (vector2.Y * sin)),
                (float) ((vector2.Y * cos) + (vector2.X * sin)));
        }

        /// <summary>
        ///     Extends a Vector2 to another Vector2.
        /// </summary>
        /// <param name="vector2">Extended SharpDX Vector2 (From)</param>
        /// <param name="toVector2">SharpDX Vector2 (To)</param>
        /// <param name="distance">Distance (float units)</param>
        /// <returns>Extended Vector2</returns>
        public static Vector2 Extend(this Vector2 vector2, Vector2 toVector2, float distance)
        {
            return vector2 + distance * (toVector2 - vector2).Normalized();
        }

        /// <summary>
        ///     Extends a Vector2 to a Vector3.
        /// </summary>
        /// <param name="vector2">Extended SharpDX Vector2 (From)</param>
        /// <param name="toVector3">SharpDX Vector3 (To)</param>
        /// <param name="distance">Distance (float units)</param>
        /// <returns>Extended Vector2</returns>
        public static Vector2 Extend(this Vector2 vector2, Vector3 toVector3, float distance)
        {
            return vector2 + distance * ((Vector2)toVector3 - vector2).Normalized();
        }

        /// <summary>
        ///     Normalizes a Vector2.
        /// </summary>
        /// <param name="vector2">SharpDX Vector2</param>
        /// <returns>Normalized Vector2</returns>
        public static Vector2 Normalized(this Vector2 vector2)
        {
            vector2.Normalize();
            return vector2;
        }

        #endregion

        /// <summary>
        ///     Struct IntersectionResult
        /// </summary>
        public struct IntersectionResult
        {
            #region Fields

            /// <summary>
            ///     Returns if both of the points intersect.
            /// </summary>
            public bool Intersects;

            /// <summary>
            ///     Intersection point
            /// </summary>
            public Vector2 Point;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="IntersectionResult" /> struct.
            ///     Constructor for Intersection Result
            /// </summary>
            /// <param name="intersects">
            ///     Intersection of input
            /// </param>
            /// <param name="point">
            ///     Intersection Point
            /// </param>
            public IntersectionResult(bool intersects = false, Vector2 point = default(Vector2))
            {
                this.Intersects = intersects;
                this.Point = point;
            }

            #endregion
        }

        /// <summary>
        ///     Holds info for the ProjectOn method.
        /// </summary>
        public struct ProjectionInfo
        {
            #region Fields

            /// <summary>
            ///     Returns if the point is on the segment
            /// </summary>
            public bool IsOnSegment;

            /// <summary>
            ///     Line point
            /// </summary>
            public Vector2 LinePoint;

            /// <summary>
            ///     Segment point
            /// </summary>
            public Vector2 SegmentPoint;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="ProjectionInfo" /> struct.
            /// </summary>
            /// <param name="isOnSegment">
            ///     Is on Segment
            /// </param>
            /// <param name="segmentPoint">
            ///     Segment point
            /// </param>
            /// <param name="linePoint">
            ///     Line point
            /// </param>
            internal ProjectionInfo(bool isOnSegment, Vector2 segmentPoint, Vector2 linePoint)
            {
                this.IsOnSegment = isOnSegment;
                this.SegmentPoint = segmentPoint;
                this.LinePoint = linePoint;
            }

            #endregion
        }
    }
}

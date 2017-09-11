/*******************************************************************************
*                                                                              *
* Author    :  Angus Johnson                                                   *
* Version   :  6.4.2                                                           *
* Date      :  27 February 2017                                                *
* Website   :  http://www.angusj.com                                           *
* Copyright :  Angus Johnson 2010-2017                                         *
*                                                                              *
* License:                                                                     *
* Use, modification & distribution is subject to Boost Software License Ver 1. *
* http://www.boost.org/LICENSE_1_0.txt                                         *
*                                                                              *
* Attributions:                                                                *
* The code in this library is an extension of Bala Vatti's clipping algorithm: *
* "A generic solution to polygon clipping"                                     *
* Communications of the ACM, Vol 35, Issue 7 (July 1992) pp 56-63.             *
* http://portal.acm.org/citation.cfm?id=129906                                 *
*                                                                              *
* Computer graphics and geometric modeling: implementation and algorithms      *
* By Max K. Agoston                                                            *
* Springer; 1 edition (January 4, 2005)                                        *
* http://books.google.com/books?q=vatti+clipping+agoston                       *
*                                                                              *
* See also:                                                                    *
* "Polygon Offsetting by Computing Winding Numbers"                            *
* Paper no. DETC2005-85513 pp. 565-575                                         *
* ASME 2005 International Design Engineering Technical Conferences             *
* and Computers and Information in Engineering Conference (IDETC/CIE2005)      *
* September 24-28, 2005 , Long Beach, California, USA                          *
* http://www.me.berkeley.edu/~mcmains/pubs/DAC05OffsetPolygon.pdf              *
*                                                                              *
*******************************************************************************/

/*******************************************************************************
*                                                                              *
* This is a translation of the Delphi Clipper library and the naming style     *
* used has retained a Delphi flavour.                                          *
*                                                                              *
*******************************************************************************/

//use_int32: When enabled 32bit ints are used instead of 64bit ints. This
//improve performance but coordinate values are limited to the range +/- 46340
//#define use_int32

//use_xyz: adds a Z member to IntPoint. Adds a minor cost to performance.

#define use_xyz

//use_lines: Enables open path clipping. Adds a very minor cost to performance.
#define use_lines



//using System.Text;          //for Int128.AsString() & StringBuilder
//using System.IO;            //debugging with streamReader & StreamWriter
//using System.Windows.Forms; //debugging to clipboard

namespace Aimtec.SDK.Util.ThirdParty
{
#if use_int32
  using cInt = Int32;
#else
    using cInt = System.Int64;
#endif
    using System;
    using System.Collections.Generic;

    using Path = System.Collections.Generic.List<IntPoint>;
    using Paths = System.Collections.Generic.List<System.Collections.Generic.List<IntPoint>>;

    public struct DoublePoint
    {
        #region Fields

        public double X;

        public double Y;

        #endregion

        #region Constructors and Destructors

        public DoublePoint(double x = 0, double y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public DoublePoint(DoublePoint dp)
        {
            this.X = dp.X;
            this.Y = dp.Y;
        }

        public DoublePoint(IntPoint ip)
        {
            this.X = ip.X;
            this.Y = ip.Y;
        }

        #endregion
    }

    //------------------------------------------------------------------------------
    // PolyTree & PolyNode classes
    //------------------------------------------------------------------------------

    public class PolyTree : PolyNode
    {
        #region Fields

        internal List<PolyNode> MAllPolys = new List<PolyNode>();

        #endregion

        #region Public Properties

        public int Total
        {
            get
            {
                var result = this.MAllPolys.Count;
                //with negative offsets, ignore the hidden outer polygon ...
                if (result > 0 && this.MChilds[0] != this.MAllPolys[0])
                    result--;
                return result;
            }
        }

        #endregion

        #region Public Methods and Operators

        //The GC probably handles this cleanup more efficiently ...
        //~PolyTree(){Clear();}

        public void Clear()
        {
            for (var i = 0; i < this.MAllPolys.Count; i++)
                this.MAllPolys[i] = null;
            this.MAllPolys.Clear();
            this.MChilds.Clear();
        }

        public PolyNode GetFirst()
        {
            if (this.MChilds.Count > 0)
                return this.MChilds[0];
            return null;
        }

        #endregion
    }

    public class PolyNode
    {
        #region Fields

        internal List<PolyNode> MChilds = new List<PolyNode>();

        internal EndType MEndtype;

        internal int MIndex;

        internal JoinType MJointype;

        internal PolyNode MParent;

        internal Path MPolygon = new Path();

        #endregion

        #region Public Properties

        public int ChildCount
        {
            get
            {
                return this.MChilds.Count;
            }
        }

        public List<PolyNode> Childs
        {
            get
            {
                return this.MChilds;
            }
        }

        public Path Contour
        {
            get
            {
                return this.MPolygon;
            }
        }

        public bool IsHole
        {
            get
            {
                return this.IsHoleNode();
            }
        }

        public bool IsOpen { get; set; }

        public PolyNode Parent
        {
            get
            {
                return this.MParent;
            }
        }

        #endregion

        #region Public Methods and Operators

        public PolyNode GetNext()
        {
            if (this.MChilds.Count > 0)
                return this.MChilds[0];
            return this.GetNextSiblingUp();
        }

        #endregion

        #region Methods

        internal void AddChild(PolyNode child)
        {
            var cnt = this.MChilds.Count;
            this.MChilds.Add(child);
            child.MParent = this;
            child.MIndex = cnt;
        }

        internal PolyNode GetNextSiblingUp()
        {
            if (this.MParent == null)
                return null;
            if (this.MIndex == this.MParent.MChilds.Count - 1)
                return this.MParent.GetNextSiblingUp();
            return this.MParent.MChilds[this.MIndex + 1];
        }

        private bool IsHoleNode()
        {
            var result = true;
            var node = this.MParent;
            while (node != null)
            {
                result = !result;
                node = node.MParent;
            }
            return result;
        }

        #endregion
    }

    //------------------------------------------------------------------------------
    // Int128 struct (enables safe math on signed 64bit integers)
    // eg Int128 val1((Int64)9223372036854775807); //ie 2^63 -1
    //    Int128 val2((Int64)9223372036854775807);
    //    Int128 val3 = val1 * val2;
    //    val3.ToString => "85070591730234615847396907784232501249" (8.5e+37)
    //------------------------------------------------------------------------------

    internal struct Int128
    {
        #region Fields

        private cInt hi;

        private UInt64 lo;

        #endregion

        #region Constructors and Destructors

        public Int128(Int64 _lo)
        {
            this.lo = (UInt64) _lo;
            if (_lo < 0)
                this.hi = -1;
            else
                this.hi = 0;
        }

        public Int128(Int64 _hi, UInt64 _lo)
        {
            this.lo = _lo;
            this.hi = _hi;
        }

        public Int128(Int128 val)
        {
            this.hi = val.hi;
            this.lo = val.lo;
        }

        #endregion

        #region Public Methods and Operators

        //nb: Constructing two new Int128 objects every time we want to multiply longs  
        //is slow. So, although calling the Int128Mul method doesn't look as clean, the 
        //code runs significantly faster than if we'd used the * operator.

        public static Int128 Int128Mul(Int64 lhs, Int64 rhs)
        {
            var negate = lhs < 0 != rhs < 0;
            if (lhs < 0)
                lhs = -lhs;
            if (rhs < 0)
                rhs = -rhs;
            var int1Hi = (UInt64) lhs >> 32;
            var int1Lo = (UInt64) lhs & 0xFFFFFFFF;
            var int2Hi = (UInt64) rhs >> 32;
            var int2Lo = (UInt64) rhs & 0xFFFFFFFF;

            //nb: see comments in clipper.pas
            var a = int1Hi * int2Hi;
            var b = int1Lo * int2Lo;
            var c = int1Hi * int2Lo + int1Lo * int2Hi;

            UInt64 lo;
            Int64 hi;
            hi = (Int64) (a + (c >> 32));

            unchecked
            {
                lo = (c << 32) + b;
            }
            if (lo < b)
                hi++;
            var result = new Int128(hi, lo);
            return negate ? -result : result;
        }

        public static Int128 operator +(Int128 lhs, Int128 rhs)
        {
            lhs.hi += rhs.hi;
            lhs.lo += rhs.lo;
            if (lhs.lo < rhs.lo)
                lhs.hi++;
            return lhs;
        }

        public static bool operator ==(Int128 val1, Int128 val2)
        {
            if ((object) val1 == (object) val2)
                return true;
            if ((object) val1 == null || (object) val2 == null)
                return false;
            return val1.hi == val2.hi && val1.lo == val2.lo;
        }

        public static explicit operator double(Int128 val)
        {
            const double Shift64 = 18446744073709551616.0; //2^64
            if (val.hi < 0)
            {
                if (val.lo == 0)
                    return val.hi * Shift64;
                return -(~val.lo + ~val.hi * Shift64);
            }
            return val.lo + val.hi * Shift64;
        }

        public static bool operator >(Int128 val1, Int128 val2)
        {
            if (val1.hi != val2.hi)
                return val1.hi > val2.hi;
            return val1.lo > val2.lo;
        }

        public static bool operator !=(Int128 val1, Int128 val2)
        {
            return !(val1 == val2);
        }

        public static bool operator <(Int128 val1, Int128 val2)
        {
            if (val1.hi != val2.hi)
                return val1.hi < val2.hi;
            return val1.lo < val2.lo;
        }

        public static Int128 operator -(Int128 lhs, Int128 rhs)
        {
            return lhs + -rhs;
        }

        public static Int128 operator -(Int128 val)
        {
            if (val.lo == 0)
                return new Int128(-val.hi, 0);
            return new Int128(~val.hi, ~val.lo + 1);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Int128))
                return false;
            var i128 = (Int128) obj;
            return i128.hi == this.hi && i128.lo == this.lo;
        }

        public override int GetHashCode()
        {
            return this.hi.GetHashCode() ^ this.lo.GetHashCode();
        }

        public bool IsNegative()
        {
            return this.hi < 0;
        }

        #endregion
    }

    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------

    public struct IntPoint
    {
        public cInt X;

        public cInt Y;
#if use_xyz
        public cInt Z;

        public IntPoint(cInt x, cInt y, cInt z = 0)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public IntPoint(double x, double y, double z = 0)
        {
            this.X = (cInt) x;
            this.Y = (cInt) y;
            this.Z = (cInt) z;
        }

        public IntPoint(DoublePoint dp)
        {
            this.X = (cInt) dp.X;
            this.Y = (cInt) dp.Y;
            this.Z = 0;
        }

        public IntPoint(IntPoint pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = pt.Z;
        }
#else
        public IntPoint(cInt X, cInt Y)
        {
            this.X = X; this.Y = Y;
        }
        public IntPoint(double x, double y)
        {
            this.X = (cInt)x; this.Y = (cInt)y;
        }

        public IntPoint(IntPoint pt)
        {
            this.X = pt.X; this.Y = pt.Y;
        }
#endif

        public static bool operator ==(IntPoint a, IntPoint b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(IntPoint a, IntPoint b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is IntPoint)
            {
                var a = (IntPoint) obj;
                return this.X == a.X && this.Y == a.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            //simply prevents a compiler warning
            return base.GetHashCode();
        }
    } // end struct IntPoint

    public struct IntRect
    {
        #region Fields

        public cInt Bottom;

        public cInt Left;

        public cInt Right;

        public cInt Top;

        #endregion

        #region Constructors and Destructors

        public IntRect(cInt l, cInt t, cInt r, cInt b)
        {
            this.Left = l;
            this.Top = t;
            this.Right = r;
            this.Bottom = b;
        }

        public IntRect(IntRect ir)
        {
            this.Left = ir.Left;
            this.Top = ir.Top;
            this.Right = ir.Right;
            this.Bottom = ir.Bottom;
        }

        #endregion
    }

    public enum ClipType
    {
        CtIntersection,

        CtUnion,

        CtDifference,

        CtXor
    }

    public enum PolyType
    {
        PtSubject,

        PtClip
    }

    //By far the most widely used winding rules for polygon filling are
    //EvenOdd & NonZero (GDI, GDI+, XLib, OpenGL, Cairo, AGG, Quartz, SVG, Gr32)
    //Others rules include Positive, Negative and ABS_GTR_EQ_TWO (only in OpenGL)
    //see http://glprogramming.com/red/chapter11.html
    public enum PolyFillType
    {
        PftEvenOdd,

        PftNonZero,

        PftPositive,

        PftNegative
    }

    public enum JoinType
    {
        JtSquare,

        JtRound,

        JtMiter
    }

    public enum EndType
    {
        EtClosedPolygon,

        EtClosedLine,

        EtOpenButt,

        EtOpenSquare,

        EtOpenRound
    }

    internal enum EdgeSide
    {
        EsLeft,

        EsRight
    }

    internal enum Direction
    {
        DRightToLeft,

        DLeftToRight
    }

    internal class Edge
    {
        #region Fields

        internal IntPoint Bot;

        internal IntPoint Curr; //current (updated for every new scanbeam)

        internal IntPoint Delta;

        internal double Dx;

        internal Edge Next;

        internal Edge NextInAel;

        internal Edge NextInLml;

        internal Edge NextInSel;

        internal int OutIdx;

        internal PolyType PolyTyp;

        internal Edge Prev;

        internal Edge PrevInAel;

        internal Edge PrevInSel;

        internal EdgeSide Side; //side only refers to current side of solution poly

        internal IntPoint Top;

        internal int WindCnt;

        internal int WindCnt2; //winding count of the opposite polytype

        internal int WindDelta; //1 or -1 depending on winding direction

        #endregion
    }

    public class IntersectNode
    {
        #region Fields

        internal Edge Edge1;

        internal Edge Edge2;

        internal IntPoint Pt;

        #endregion
    }

    public class MyIntersectNodeSort : IComparer<IntersectNode>
    {
        #region Public Methods and Operators

        public int Compare(IntersectNode node1, IntersectNode node2)
        {
            var i = node2.Pt.Y - node1.Pt.Y;
            if (i > 0)
                return 1;
            if (i < 0)
                return -1;
            return 0;
        }

        #endregion
    }

    internal class LocalMinima
    {
        #region Fields

        internal Edge LeftBound;

        internal LocalMinima Next;

        internal Edge RightBound;

        internal cInt Y;

        #endregion
    }

    internal class Scanbeam
    {
        #region Fields

        internal Scanbeam Next;

        internal cInt Y;

        #endregion
    }

    internal class Maxima
    {
        #region Fields

        internal Maxima Next;

        internal Maxima Prev;

        internal cInt X;

        #endregion
    }

    //OutRec: contains a path in the clipping solution. Edges in the AEL will
    //carry a pointer to an OutRec when they are part of the clipping solution.
    internal class OutRec
    {
        #region Fields

        internal OutPt BottomPt;

        internal OutRec FirstLeft; //see comments in clipper.pas

        internal int Idx;

        internal bool IsHole;

        internal bool IsOpen;

        internal PolyNode PolyNode;

        internal OutPt Pts;

        #endregion
    }

    internal class OutPt
    {
        #region Fields

        internal int Idx;

        internal OutPt Next;

        internal OutPt Prev;

        internal IntPoint Pt;

        #endregion
    }

    internal class Join
    {
        #region Fields

        internal IntPoint OffPt;

        internal OutPt OutPt1;

        internal OutPt OutPt2;

        #endregion
    }

    public class ClipperBase
    {
        internal const double Horizontal = -3.4E+38;

        internal const int Skip = -2;

        internal const int Unassigned = -1;

        internal const double Tolerance = 1.0E-20;

        internal static bool NearZero(double val)
        {
            return val > -Tolerance && val < Tolerance;
        }

#if use_int32
    public const cInt loRange = 0x7FFF;
    public const cInt hiRange = 0x7FFF;
#else
        public const cInt LoRange = 0x3FFFFFFF;
        public const cInt HiRange = 0x3FFFFFFFFFFFFFFFL;
#endif

        internal LocalMinima MMinimaList;

        internal LocalMinima MCurrentLm;

        internal List<List<Edge>> MEdges = new List<List<Edge>>();

        internal Scanbeam MScanbeam;

        internal List<OutRec> MPolyOuts;

        internal Edge MActiveEdges;

        internal bool MUseFullRange;

        internal bool MHasOpenPaths;

        //------------------------------------------------------------------------------

        public bool PreserveCollinear { get; set; }
        //------------------------------------------------------------------------------

        public void Swap(ref cInt val1, ref cInt val2)
        {
            var tmp = val1;
            val1 = val2;
            val2 = tmp;
        }
        //------------------------------------------------------------------------------

        internal static bool IsHorizontal(Edge e)
        {
            return e.Delta.Y == 0;
        }
        //------------------------------------------------------------------------------

        internal bool PointIsVertex(IntPoint pt, OutPt pp)
        {
            var pp2 = pp;
            do
            {
                if (pp2.Pt == pt)
                    return true;
                pp2 = pp2.Next;
            }
            while (pp2 != pp);
            return false;
        }
        //------------------------------------------------------------------------------

        internal bool PointOnLineSegment(IntPoint pt, IntPoint linePt1, IntPoint linePt2, bool useFullRange)
        {
            if (useFullRange)
                return pt.X == linePt1.X && pt.Y == linePt1.Y || pt.X == linePt2.X && pt.Y == linePt2.Y
                    || pt.X > linePt1.X == pt.X < linePt2.X && pt.Y > linePt1.Y == pt.Y < linePt2.Y
                    && Int128.Int128Mul(pt.X - linePt1.X, linePt2.Y - linePt1.Y)
                    == Int128.Int128Mul(linePt2.X - linePt1.X, pt.Y - linePt1.Y);
            return pt.X == linePt1.X && pt.Y == linePt1.Y || pt.X == linePt2.X && pt.Y == linePt2.Y
                   || pt.X > linePt1.X == pt.X < linePt2.X && pt.Y > linePt1.Y == pt.Y < linePt2.Y
                   && (pt.X - linePt1.X) * (linePt2.Y - linePt1.Y) == (linePt2.X - linePt1.X) * (pt.Y - linePt1.Y);
        }
        //------------------------------------------------------------------------------

        internal bool PointOnPolygon(IntPoint pt, OutPt pp, bool useFullRange)
        {
            var pp2 = pp;
            while (true)
            {
                if (this.PointOnLineSegment(pt, pp2.Pt, pp2.Next.Pt, useFullRange))
                    return true;
                pp2 = pp2.Next;
                if (pp2 == pp)
                    break;
            }
            return false;
        }
        //------------------------------------------------------------------------------

        internal static bool SlopesEqual(Edge e1, Edge e2, bool useFullRange)
        {
            if (useFullRange)
                return Int128.Int128Mul(e1.Delta.Y, e2.Delta.X) == Int128.Int128Mul(e1.Delta.X, e2.Delta.Y);
            return e1.Delta.Y * e2.Delta.X == e1.Delta.X * e2.Delta.Y;
        }
        //------------------------------------------------------------------------------

        internal static bool SlopesEqual(IntPoint pt1, IntPoint pt2, IntPoint pt3, bool useFullRange)
        {
            if (useFullRange)
                return Int128.Int128Mul(pt1.Y - pt2.Y, pt2.X - pt3.X) == Int128.Int128Mul(pt1.X - pt2.X, pt2.Y - pt3.Y);
            return (pt1.Y - pt2.Y) * (pt2.X - pt3.X) - (pt1.X - pt2.X) * (pt2.Y - pt3.Y) == 0;
        }
        //------------------------------------------------------------------------------

        internal static bool SlopesEqual(IntPoint pt1, IntPoint pt2, IntPoint pt3, IntPoint pt4, bool useFullRange)
        {
            if (useFullRange)
                return Int128.Int128Mul(pt1.Y - pt2.Y, pt3.X - pt4.X) == Int128.Int128Mul(pt1.X - pt2.X, pt3.Y - pt4.Y);
            return (pt1.Y - pt2.Y) * (pt3.X - pt4.X) - (pt1.X - pt2.X) * (pt3.Y - pt4.Y) == 0;
        }
        //------------------------------------------------------------------------------

        internal ClipperBase() //constructor (nb: no external instantiation)
        {
            this.MMinimaList = null;
            this.MCurrentLm = null;
            this.MUseFullRange = false;
            this.MHasOpenPaths = false;
        }
        //------------------------------------------------------------------------------

        public virtual void Clear()
        {
            this.DisposeLocalMinimaList();
            for (var i = 0; i < this.MEdges.Count; ++i)
            {
                for (var j = 0; j < this.MEdges[i].Count; ++j)
                    this.MEdges[i][j] = null;
                this.MEdges[i].Clear();
            }
            this.MEdges.Clear();
            this.MUseFullRange = false;
            this.MHasOpenPaths = false;
        }
        //------------------------------------------------------------------------------

        private void DisposeLocalMinimaList()
        {
            while (this.MMinimaList != null)
            {
                var tmpLm = this.MMinimaList.Next;
                this.MMinimaList = null;
                this.MMinimaList = tmpLm;
            }
            this.MCurrentLm = null;
        }
        //------------------------------------------------------------------------------

        void RangeTest(IntPoint pt, ref bool useFullRange)
        {
            if (useFullRange)
            {
                if (pt.X > HiRange || pt.Y > HiRange || -pt.X > HiRange || -pt.Y > HiRange)
                    throw new ClipperException("Coordinate outside allowed range");
            }
            else if (pt.X > LoRange || pt.Y > LoRange || -pt.X > LoRange || -pt.Y > LoRange)
            {
                useFullRange = true;
                this.RangeTest(pt, ref useFullRange);
            }
        }
        //------------------------------------------------------------------------------

        private void InitEdge(Edge e, Edge eNext, Edge ePrev, IntPoint pt)
        {
            e.Next = eNext;
            e.Prev = ePrev;
            e.Curr = pt;
            e.OutIdx = Unassigned;
        }
        //------------------------------------------------------------------------------

        private void InitEdge2(Edge e, PolyType polyType)
        {
            if (e.Curr.Y >= e.Next.Curr.Y)
            {
                e.Bot = e.Curr;
                e.Top = e.Next.Curr;
            }
            else
            {
                e.Top = e.Curr;
                e.Bot = e.Next.Curr;
            }
            this.SetDx(e);
            e.PolyTyp = polyType;
        }
        //------------------------------------------------------------------------------

        private Edge FindNextLocMin(Edge e)
        {
            Edge e2;
            for (;;)
            {
                while (e.Bot != e.Prev.Bot || e.Curr == e.Top)
                    e = e.Next;
                if (e.Dx != Horizontal && e.Prev.Dx != Horizontal)
                    break;
                while (e.Prev.Dx == Horizontal)
                    e = e.Prev;
                e2 = e;
                while (e.Dx == Horizontal)
                    e = e.Next;
                if (e.Top.Y == e.Prev.Bot.Y)
                    continue; //ie just an intermediate horz.
                if (e2.Prev.Bot.X < e.Bot.X)
                    e = e2;
                break;
            }
            return e;
        }
        //------------------------------------------------------------------------------

        private Edge ProcessBound(Edge e, bool leftBoundIsForward)
        {
            Edge eStart, result = e;
            Edge horz;

            if (result.OutIdx == Skip)
            {
                //check if there are edges beyond the skip edge in the bound and if so
                //create another LocMin and calling ProcessBound once more ...
                e = result;
                if (leftBoundIsForward)
                {
                    while (e.Top.Y == e.Next.Bot.Y)
                        e = e.Next;
                    while (e != result && e.Dx == Horizontal)
                        e = e.Prev;
                }
                else
                {
                    while (e.Top.Y == e.Prev.Bot.Y)
                        e = e.Prev;
                    while (e != result && e.Dx == Horizontal)
                        e = e.Next;
                }
                if (e == result)
                {
                    if (leftBoundIsForward)
                        result = e.Next;
                    else
                        result = e.Prev;
                }
                else
                {
                    //there are more edges in the bound beyond result starting with E
                    if (leftBoundIsForward)
                        e = result.Next;
                    else
                        e = result.Prev;
                    var locMin = new LocalMinima();
                    locMin.Next = null;
                    locMin.Y = e.Bot.Y;
                    locMin.LeftBound = null;
                    locMin.RightBound = e;
                    e.WindDelta = 0;
                    result = this.ProcessBound(e, leftBoundIsForward);
                    this.InsertLocalMinima(locMin);
                }
                return result;
            }

            if (e.Dx == Horizontal)
            {
                //We need to be careful with open paths because this may not be a
                //true local minima (ie E may be following a skip edge).
                //Also, consecutive horz. edges may start heading left before going right.
                if (leftBoundIsForward)
                    eStart = e.Prev;
                else
                    eStart = e.Next;
                if (eStart.Dx == Horizontal) //ie an adjoining horizontal skip edge
                {
                    if (eStart.Bot.X != e.Bot.X && eStart.Top.X != e.Bot.X)
                        this.ReverseHorizontal(e);
                }
                else if (eStart.Bot.X != e.Bot.X)
                    this.ReverseHorizontal(e);
            }

            eStart = e;
            if (leftBoundIsForward)
            {
                while (result.Top.Y == result.Next.Bot.Y && result.Next.OutIdx != Skip)
                    result = result.Next;
                if (result.Dx == Horizontal && result.Next.OutIdx != Skip)
                {
                    //nb: at the top of a bound, horizontals are added to the bound
                    //only when the preceding edge attaches to the horizontal's left vertex
                    //unless a Skip edge is encountered when that becomes the top divide
                    horz = result;
                    while (horz.Prev.Dx == Horizontal)
                        horz = horz.Prev;
                    if (horz.Prev.Top.X > result.Next.Top.X)
                        result = horz.Prev;
                }
                while (e != result)
                {
                    e.NextInLml = e.Next;
                    if (e.Dx == Horizontal && e != eStart && e.Bot.X != e.Prev.Top.X)
                        this.ReverseHorizontal(e);
                    e = e.Next;
                }
                if (e.Dx == Horizontal && e != eStart && e.Bot.X != e.Prev.Top.X)
                    this.ReverseHorizontal(e);
                result = result.Next; //move to the edge just beyond current bound
            }
            else
            {
                while (result.Top.Y == result.Prev.Bot.Y && result.Prev.OutIdx != Skip)
                    result = result.Prev;
                if (result.Dx == Horizontal && result.Prev.OutIdx != Skip)
                {
                    horz = result;
                    while (horz.Next.Dx == Horizontal)
                        horz = horz.Next;
                    if (horz.Next.Top.X == result.Prev.Top.X || horz.Next.Top.X > result.Prev.Top.X)
                        result = horz.Next;
                }

                while (e != result)
                {
                    e.NextInLml = e.Prev;
                    if (e.Dx == Horizontal && e != eStart && e.Bot.X != e.Next.Top.X)
                        this.ReverseHorizontal(e);
                    e = e.Prev;
                }
                if (e.Dx == Horizontal && e != eStart && e.Bot.X != e.Next.Top.X)
                    this.ReverseHorizontal(e);
                result = result.Prev; //move to the edge just beyond current bound
            }
            return result;
        }
        //------------------------------------------------------------------------------

        public bool AddPath(Path pg, PolyType polyType, bool closed)
        {
#if use_lines
            if (!closed && polyType == PolyType.PtClip)
                throw new ClipperException("AddPath: Open paths must be subject.");
#else
      if (!Closed)
        throw new ClipperException("AddPath: Open paths have been disabled.");
#endif

            var highI = pg.Count - 1;
            if (closed)
                while (highI > 0 && pg[highI] == pg[0])
                    --highI;
            while (highI > 0 && pg[highI] == pg[highI - 1])
                --highI;
            if (closed && highI < 2 || !closed && highI < 1)
                return false;

            //create a new edge array ...
            var edges = new List<Edge>(highI + 1);
            for (var i = 0; i <= highI; i++)
                edges.Add(new Edge());

            var isFlat = true;

            //1. Basic (first) edge initialization ...
            edges[1].Curr = pg[1];
            this.RangeTest(pg[0], ref this.MUseFullRange);
            this.RangeTest(pg[highI], ref this.MUseFullRange);
            this.InitEdge(edges[0], edges[1], edges[highI], pg[0]);
            this.InitEdge(edges[highI], edges[0], edges[highI - 1], pg[highI]);
            for (var i = highI - 1; i >= 1; --i)
            {
                this.RangeTest(pg[i], ref this.MUseFullRange);
                this.InitEdge(edges[i], edges[i + 1], edges[i - 1], pg[i]);
            }
            var eStart = edges[0];

            //2. Remove duplicate vertices, and (when closed) collinear edges ...
            Edge e = eStart, eLoopStop = eStart;
            for (;;)
            {
                //nb: allows matching start and end points when not Closed ...
                if (e.Curr == e.Next.Curr && (closed || e.Next != eStart))
                {
                    if (e == e.Next)
                        break;
                    if (e == eStart)
                        eStart = e.Next;
                    e = this.RemoveEdge(e);
                    eLoopStop = e;
                    continue;
                }
                if (e.Prev == e.Next)
                    break; //only two vertices
                if (closed && SlopesEqual(e.Prev.Curr, e.Curr, e.Next.Curr, this.MUseFullRange)
                    && (!this.PreserveCollinear || !this.Pt2IsBetweenPt1AndPt3(e.Prev.Curr, e.Curr, e.Next.Curr)))
                {
                    //Collinear edges are allowed for open paths but in closed paths
                    //the default is to merge adjacent collinear edges into a single edge.
                    //However, if the PreserveCollinear property is enabled, only overlapping
                    //collinear edges (ie spikes) will be removed from closed paths.
                    if (e == eStart)
                        eStart = e.Next;
                    e = this.RemoveEdge(e);
                    e = e.Prev;
                    eLoopStop = e;
                    continue;
                }
                e = e.Next;
                if (e == eLoopStop || !closed && e.Next == eStart)
                    break;
            }

            if (!closed && e == e.Next || closed && e.Prev == e.Next)
                return false;

            if (!closed)
            {
                this.MHasOpenPaths = true;
                eStart.Prev.OutIdx = Skip;
            }

            //3. Do second stage of edge initialization ...
            e = eStart;
            do
            {
                this.InitEdge2(e, polyType);
                e = e.Next;
                if (isFlat && e.Curr.Y != eStart.Curr.Y)
                    isFlat = false;
            }
            while (e != eStart);

            //4. Finally, add edge bounds to LocalMinima list ...

            //Totally flat paths must be handled differently when adding them
            //to LocalMinima list to avoid endless loops etc ...
            if (isFlat)
            {
                if (closed)
                    return false;
                e.Prev.OutIdx = Skip;
                var locMin = new LocalMinima();
                locMin.Next = null;
                locMin.Y = e.Bot.Y;
                locMin.LeftBound = null;
                locMin.RightBound = e;
                locMin.RightBound.Side = EdgeSide.EsRight;
                locMin.RightBound.WindDelta = 0;
                for (;;)
                {
                    if (e.Bot.X != e.Prev.Top.X)
                        this.ReverseHorizontal(e);
                    if (e.Next.OutIdx == Skip)
                        break;
                    e.NextInLml = e.Next;
                    e = e.Next;
                }
                this.InsertLocalMinima(locMin);
                this.MEdges.Add(edges);
                return true;
            }

            this.MEdges.Add(edges);
            bool leftBoundIsForward;
            Edge eMin = null;

            //workaround to avoid an endless loop in the while loop below when
            //open paths have matching start and end points ...
            if (e.Prev.Bot == e.Prev.Top)
                e = e.Next;

            for (;;)
            {
                e = this.FindNextLocMin(e);
                if (e == eMin)
                    break;
                if (eMin == null)
                    eMin = e;

                //E and E.Prev now share a local minima (left aligned if horizontal).
                //Compare their slopes to find which starts which bound ...
                var locMin = new LocalMinima();
                locMin.Next = null;
                locMin.Y = e.Bot.Y;
                if (e.Dx < e.Prev.Dx)
                {
                    locMin.LeftBound = e.Prev;
                    locMin.RightBound = e;
                    leftBoundIsForward = false; //Q.nextInLML = Q.prev
                }
                else
                {
                    locMin.LeftBound = e;
                    locMin.RightBound = e.Prev;
                    leftBoundIsForward = true; //Q.nextInLML = Q.next
                }
                locMin.LeftBound.Side = EdgeSide.EsLeft;
                locMin.RightBound.Side = EdgeSide.EsRight;

                if (!closed)
                    locMin.LeftBound.WindDelta = 0;
                else if (locMin.LeftBound.Next == locMin.RightBound)
                    locMin.LeftBound.WindDelta = -1;
                else
                    locMin.LeftBound.WindDelta = 1;
                locMin.RightBound.WindDelta = -locMin.LeftBound.WindDelta;

                e = this.ProcessBound(locMin.LeftBound, leftBoundIsForward);
                if (e.OutIdx == Skip)
                    e = this.ProcessBound(e, leftBoundIsForward);

                var e2 = this.ProcessBound(locMin.RightBound, !leftBoundIsForward);
                if (e2.OutIdx == Skip)
                    e2 = this.ProcessBound(e2, !leftBoundIsForward);

                if (locMin.LeftBound.OutIdx == Skip)
                    locMin.LeftBound = null;
                else if (locMin.RightBound.OutIdx == Skip)
                    locMin.RightBound = null;
                this.InsertLocalMinima(locMin);
                if (!leftBoundIsForward)
                    e = e2;
            }
            return true;
        }
        //------------------------------------------------------------------------------

        public bool AddPaths(Paths ppg, PolyType polyType, bool closed)
        {
            var result = false;
            for (var i = 0; i < ppg.Count; ++i)
                if (this.AddPath(ppg[i], polyType, closed))
                    result = true;
            return result;
        }
        //------------------------------------------------------------------------------

        internal bool Pt2IsBetweenPt1AndPt3(IntPoint pt1, IntPoint pt2, IntPoint pt3)
        {
            if (pt1 == pt3 || pt1 == pt2 || pt3 == pt2)
                return false;
            if (pt1.X != pt3.X)
                return pt2.X > pt1.X == pt2.X < pt3.X;
            return pt2.Y > pt1.Y == pt2.Y < pt3.Y;
        }
        //------------------------------------------------------------------------------

        Edge RemoveEdge(Edge e)
        {
            //removes e from double_linked_list (but without removing from memory)
            e.Prev.Next = e.Next;
            e.Next.Prev = e.Prev;
            var result = e.Next;
            e.Prev = null; //flag as removed (see ClipperBase.Clear)
            return result;
        }
        //------------------------------------------------------------------------------

        private void SetDx(Edge e)
        {
            e.Delta.X = e.Top.X - e.Bot.X;
            e.Delta.Y = e.Top.Y - e.Bot.Y;
            if (e.Delta.Y == 0)
                e.Dx = Horizontal;
            else
                e.Dx = (double) e.Delta.X / e.Delta.Y;
        }
        //---------------------------------------------------------------------------

        private void InsertLocalMinima(LocalMinima newLm)
        {
            if (this.MMinimaList == null)
            {
                this.MMinimaList = newLm;
            }
            else if (newLm.Y >= this.MMinimaList.Y)
            {
                newLm.Next = this.MMinimaList;
                this.MMinimaList = newLm;
            }
            else
            {
                var tmpLm = this.MMinimaList;
                while (tmpLm.Next != null && newLm.Y < tmpLm.Next.Y)
                    tmpLm = tmpLm.Next;
                newLm.Next = tmpLm.Next;
                tmpLm.Next = newLm;
            }
        }
        //------------------------------------------------------------------------------

        internal Boolean PopLocalMinima(cInt y, out LocalMinima current)
        {
            current = this.MCurrentLm;
            if (this.MCurrentLm != null && this.MCurrentLm.Y == y)
            {
                this.MCurrentLm = this.MCurrentLm.Next;
                return true;
            }
            return false;
        }
        //------------------------------------------------------------------------------

        private void ReverseHorizontal(Edge e)
        {
            //swap horizontal edges' top and bottom x's so they follow the natural
            //progression of the bounds - ie so their xbots will align with the
            //adjoining lower edge. [Helpful in the ProcessHorizontal() method.]
            this.Swap(ref e.Top.X, ref e.Bot.X);
#if use_xyz
            this.Swap(ref e.Top.Z, ref e.Bot.Z);
#endif
        }
        //------------------------------------------------------------------------------

        internal virtual void Reset()
        {
            this.MCurrentLm = this.MMinimaList;
            if (this.MCurrentLm == null)
                return; //ie nothing to process

            //reset all edges ...
            this.MScanbeam = null;
            var lm = this.MMinimaList;
            while (lm != null)
            {
                this.InsertScanbeam(lm.Y);
                var e = lm.LeftBound;
                if (e != null)
                {
                    e.Curr = e.Bot;
                    e.OutIdx = Unassigned;
                }
                e = lm.RightBound;
                if (e != null)
                {
                    e.Curr = e.Bot;
                    e.OutIdx = Unassigned;
                }
                lm = lm.Next;
            }
            this.MActiveEdges = null;
        }
        //------------------------------------------------------------------------------

        public static IntRect GetBounds(Paths paths)
        {
            int i = 0, cnt = paths.Count;
            while (i < cnt && paths[i].Count == 0)
                i++;
            if (i == cnt)
                return new IntRect(0, 0, 0, 0);
            var result = new IntRect();
            result.Left = paths[i][0].X;
            result.Right = result.Left;
            result.Top = paths[i][0].Y;
            result.Bottom = result.Top;
            for (; i < cnt; i++)
            for (var j = 0; j < paths[i].Count; j++)
            {
                if (paths[i][j].X < result.Left)
                    result.Left = paths[i][j].X;
                else if (paths[i][j].X > result.Right)
                    result.Right = paths[i][j].X;
                if (paths[i][j].Y < result.Top)
                    result.Top = paths[i][j].Y;
                else if (paths[i][j].Y > result.Bottom)
                    result.Bottom = paths[i][j].Y;
            }
            return result;
        }
        //------------------------------------------------------------------------------

        internal void InsertScanbeam(cInt y)
        {
            //single-linked list: sorted descending, ignoring dups.
            if (this.MScanbeam == null)
            {
                this.MScanbeam = new Scanbeam();
                this.MScanbeam.Next = null;
                this.MScanbeam.Y = y;
            }
            else if (y > this.MScanbeam.Y)
            {
                var newSb = new Scanbeam();
                newSb.Y = y;
                newSb.Next = this.MScanbeam;
                this.MScanbeam = newSb;
            }
            else
            {
                var sb2 = this.MScanbeam;
                while (sb2.Next != null && y <= sb2.Next.Y)
                    sb2 = sb2.Next;
                if (y == sb2.Y)
                    return; //ie ignores duplicates
                var newSb = new Scanbeam();
                newSb.Y = y;
                newSb.Next = sb2.Next;
                sb2.Next = newSb;
            }
        }
        //------------------------------------------------------------------------------

        internal Boolean PopScanbeam(out cInt y)
        {
            if (this.MScanbeam == null)
            {
                y = 0;
                return false;
            }
            y = this.MScanbeam.Y;
            this.MScanbeam = this.MScanbeam.Next;
            return true;
        }
        //------------------------------------------------------------------------------

        internal Boolean LocalMinimaPending()
        {
            return this.MCurrentLm != null;
        }
        //------------------------------------------------------------------------------

        internal OutRec CreateOutRec()
        {
            var result = new OutRec();
            result.Idx = Unassigned;
            result.IsHole = false;
            result.IsOpen = false;
            result.FirstLeft = null;
            result.Pts = null;
            result.BottomPt = null;
            result.PolyNode = null;
            this.MPolyOuts.Add(result);
            result.Idx = this.MPolyOuts.Count - 1;
            return result;
        }
        //------------------------------------------------------------------------------

        internal void DisposeOutRec(int index)
        {
            var outRec = this.MPolyOuts[index];
            outRec.Pts = null;
            outRec = null;
            this.MPolyOuts[index] = null;
        }
        //------------------------------------------------------------------------------

        internal void UpdateEdgeIntoAel(ref Edge e)
        {
            if (e.NextInLml == null)
                throw new ClipperException("UpdateEdgeIntoAEL: invalid call");
            var aelPrev = e.PrevInAel;
            var aelNext = e.NextInAel;
            e.NextInLml.OutIdx = e.OutIdx;
            if (aelPrev != null)
                aelPrev.NextInAel = e.NextInLml;
            else
                this.MActiveEdges = e.NextInLml;
            if (aelNext != null)
                aelNext.PrevInAel = e.NextInLml;
            e.NextInLml.Side = e.Side;
            e.NextInLml.WindDelta = e.WindDelta;
            e.NextInLml.WindCnt = e.WindCnt;
            e.NextInLml.WindCnt2 = e.WindCnt2;
            e = e.NextInLml;
            e.Curr = e.Bot;
            e.PrevInAel = aelPrev;
            e.NextInAel = aelNext;
            if (!IsHorizontal(e))
                this.InsertScanbeam(e.Top.Y);
        }
        //------------------------------------------------------------------------------

        internal void SwapPositionsInAel(Edge edge1, Edge edge2)
        {
            //check that one or other edge hasn't already been removed from AEL ...
            if (edge1.NextInAel == edge1.PrevInAel || edge2.NextInAel == edge2.PrevInAel)
                return;

            if (edge1.NextInAel == edge2)
            {
                var next = edge2.NextInAel;
                if (next != null)
                    next.PrevInAel = edge1;
                var prev = edge1.PrevInAel;
                if (prev != null)
                    prev.NextInAel = edge2;
                edge2.PrevInAel = prev;
                edge2.NextInAel = edge1;
                edge1.PrevInAel = edge2;
                edge1.NextInAel = next;
            }
            else if (edge2.NextInAel == edge1)
            {
                var next = edge1.NextInAel;
                if (next != null)
                    next.PrevInAel = edge2;
                var prev = edge2.PrevInAel;
                if (prev != null)
                    prev.NextInAel = edge1;
                edge1.PrevInAel = prev;
                edge1.NextInAel = edge2;
                edge2.PrevInAel = edge1;
                edge2.NextInAel = next;
            }
            else
            {
                var next = edge1.NextInAel;
                var prev = edge1.PrevInAel;
                edge1.NextInAel = edge2.NextInAel;
                if (edge1.NextInAel != null)
                    edge1.NextInAel.PrevInAel = edge1;
                edge1.PrevInAel = edge2.PrevInAel;
                if (edge1.PrevInAel != null)
                    edge1.PrevInAel.NextInAel = edge1;
                edge2.NextInAel = next;
                if (edge2.NextInAel != null)
                    edge2.NextInAel.PrevInAel = edge2;
                edge2.PrevInAel = prev;
                if (edge2.PrevInAel != null)
                    edge2.PrevInAel.NextInAel = edge2;
            }

            if (edge1.PrevInAel == null)
                this.MActiveEdges = edge1;
            else if (edge2.PrevInAel == null)
                this.MActiveEdges = edge2;
        }
        //------------------------------------------------------------------------------

        internal void DeleteFromAel(Edge e)
        {
            var aelPrev = e.PrevInAel;
            var aelNext = e.NextInAel;
            if (aelPrev == null && aelNext == null && e != this.MActiveEdges)
                return; //already deleted
            if (aelPrev != null)
                aelPrev.NextInAel = aelNext;
            else
                this.MActiveEdges = aelNext;
            if (aelNext != null)
                aelNext.PrevInAel = aelPrev;
            e.NextInAel = null;
            e.PrevInAel = null;
        }

        //------------------------------------------------------------------------------
    } //end ClipperBase

    public class Clipper : ClipperBase
    {
        #region Constants

        public const int IoPreserveCollinear = 4;

        //InitOptions that can be passed to the constructor ...
        public const int IoReverseSolution = 1;

        public const int IoStrictlySimple = 2;

        #endregion

        #region Fields

        private PolyFillType mClipFillType;

        private ClipType mClipType;

        private bool mExecuteLocked;

        private List<Join> mGhostJoins;

        private List<IntersectNode> mIntersectList;

        IComparer<IntersectNode> mIntersectNodeComparer;

        private List<Join> mJoins;

        private Maxima mMaxima;

        private Edge mSortedEdges;

        private PolyFillType mSubjFillType;

        private bool mUsingPolyTree;

        #endregion

        #region Constructors and Destructors

        public Clipper(int initOptions = 0)
            //constructor
        {
            this.MScanbeam = null;
            this.mMaxima = null;
            this.MActiveEdges = null;
            this.mSortedEdges = null;
            this.mIntersectList = new List<IntersectNode>();
            this.mIntersectNodeComparer = new MyIntersectNodeSort();
            this.mExecuteLocked = false;
            this.mUsingPolyTree = false;
            this.MPolyOuts = new List<OutRec>();
            this.mJoins = new List<Join>();
            this.mGhostJoins = new List<Join>();
            this.ReverseSolution = (IoReverseSolution & initOptions) != 0;
            this.StrictlySimple = (IoStrictlySimple & initOptions) != 0;
            this.PreserveCollinear = (IoPreserveCollinear & initOptions) != 0;
#if use_xyz
            this.ZFillFunction = null;
#endif
        }

        #endregion

        #region Enums

        //------------------------------------------------------------------------------

        internal enum NodeType
        {
            NtAny,

            NtOpen,

            NtClosed
        }

        #endregion

        #region Public Properties

        //------------------------------------------------------------------------------

        public bool ReverseSolution { get; set; }
        //------------------------------------------------------------------------------

        public bool StrictlySimple { get; set; }

        #endregion

        #region Public Methods and Operators

        //------------------------------------------------------------------------------

        public static double Area(Path poly)
        {
            var cnt = poly.Count;
            if (cnt < 3)
                return 0;
            double a = 0;
            for (int i = 0, j = cnt - 1; i < cnt; ++i)
            {
                a += ((double) poly[j].X + poly[i].X) * ((double) poly[j].Y - poly[i].Y);
                j = i;
            }
            return -a * 0.5;
        }
        //------------------------------------------------------------------------------

        public static Path CleanPolygon(Path path, double distance = 1.415)
        {
            //distance = proximity in units/pixels below which vertices will be stripped. 
            //Default ~= sqrt(2) so when adjacent vertices or semi-adjacent vertices have 
            //both x & y coords within 1 unit, then the second vertex will be stripped.

            var cnt = path.Count;

            if (cnt == 0)
                return new Path();

            var outPts = new OutPt[cnt];
            for (var i = 0; i < cnt; ++i)
                outPts[i] = new OutPt();

            for (var i = 0; i < cnt; ++i)
            {
                outPts[i].Pt = path[i];
                outPts[i].Next = outPts[(i + 1) % cnt];
                outPts[i].Next.Prev = outPts[i];
                outPts[i].Idx = 0;
            }

            var distSqrd = distance * distance;
            var op = outPts[0];
            while (op.Idx == 0 && op.Next != op.Prev)
            {
                if (PointsAreClose(op.Pt, op.Prev.Pt, distSqrd))
                {
                    op = ExcludeOp(op);
                    cnt--;
                }
                else if (PointsAreClose(op.Prev.Pt, op.Next.Pt, distSqrd))
                {
                    ExcludeOp(op.Next);
                    op = ExcludeOp(op);
                    cnt -= 2;
                }
                else if (SlopesNearCollinear(op.Prev.Pt, op.Pt, op.Next.Pt, distSqrd))
                {
                    op = ExcludeOp(op);
                    cnt--;
                }
                else
                {
                    op.Idx = 1;
                    op = op.Next;
                }
            }

            if (cnt < 3)
                cnt = 0;
            var result = new Path(cnt);
            for (var i = 0; i < cnt; ++i)
            {
                result.Add(op.Pt);
                op = op.Next;
            }
            outPts = null;
            return result;
        }
        //------------------------------------------------------------------------------

        public static Paths CleanPolygons(Paths polys, double distance = 1.415)
        {
            var result = new Paths(polys.Count);
            for (var i = 0; i < polys.Count; i++)
                result.Add(CleanPolygon(polys[i], distance));
            return result;
        }
        //------------------------------------------------------------------------------

        public static Paths ClosedPathsFromPolyTree(PolyTree polytree)
        {
            var result = new Paths();
            result.Capacity = polytree.Total;
            AddPolyNodeToPaths(polytree, NodeType.NtClosed, result);
            return result;
        }
        //------------------------------------------------------------------------------

        public static Paths MinkowskiDiff(Path poly1, Path poly2)
        {
            var paths = Minkowski(poly1, poly2, false, true);
            var c = new Clipper();
            c.AddPaths(paths, PolyType.PtSubject, true);
            c.Execute(ClipType.CtUnion, paths, PolyFillType.PftNonZero, PolyFillType.PftNonZero);
            return paths;
        }
        //------------------------------------------------------------------------------

        public static Paths MinkowskiSum(Path pattern, Path path, bool pathIsClosed)
        {
            var paths = Minkowski(pattern, path, true, pathIsClosed);
            var c = new Clipper();
            c.AddPaths(paths, PolyType.PtSubject, true);
            c.Execute(ClipType.CtUnion, paths, PolyFillType.PftNonZero, PolyFillType.PftNonZero);
            return paths;
        }
        //------------------------------------------------------------------------------

        public static Paths MinkowskiSum(Path pattern, Paths paths, bool pathIsClosed)
        {
            var solution = new Paths();
            var c = new Clipper();
            for (var i = 0; i < paths.Count; ++i)
            {
                var tmp = Minkowski(pattern, paths[i], true, pathIsClosed);
                c.AddPaths(tmp, PolyType.PtSubject, true);
                if (pathIsClosed)
                {
                    var path = TranslatePath(paths[i], pattern[0]);
                    c.AddPath(path, PolyType.PtClip, true);
                }
            }
            c.Execute(ClipType.CtUnion, solution, PolyFillType.PftNonZero, PolyFillType.PftNonZero);
            return solution;
        }
        //------------------------------------------------------------------------------

        public static Paths OpenPathsFromPolyTree(PolyTree polytree)
        {
            var result = new Paths();
            result.Capacity = polytree.ChildCount;
            for (var i = 0; i < polytree.ChildCount; i++)
                if (polytree.Childs[i].IsOpen)
                    result.Add(polytree.Childs[i].MPolygon);
            return result;
        }
        //------------------------------------------------------------------------------

        public static bool Orientation(Path poly)
        {
            return Area(poly) >= 0;
        }
        //----------------------------------------------------------------------

        public static int PointInPolygon(IntPoint pt, Path path)
        {
            //returns 0 if false, +1 if true, -1 if pt ON polygon boundary
            //See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
            //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
            int result = 0, cnt = path.Count;
            if (cnt < 3)
                return 0;
            var ip = path[0];
            for (var i = 1; i <= cnt; ++i)
            {
                var ipNext = i == cnt ? path[0] : path[i];
                if (ipNext.Y == pt.Y)
                {
                    if (ipNext.X == pt.X || ip.Y == pt.Y && ipNext.X > pt.X == ip.X < pt.X)
                        return -1;
                }
                if (ip.Y < pt.Y != ipNext.Y < pt.Y)
                {
                    if (ip.X >= pt.X)
                    {
                        if (ipNext.X > pt.X)
                            result = 1 - result;
                        else
                        {
                            var d = (double) (ip.X - pt.X) * (ipNext.Y - pt.Y)
                                - (double) (ipNext.X - pt.X) * (ip.Y - pt.Y);
                            if (d == 0)
                                return -1;
                            if (d > 0 == ipNext.Y > ip.Y)
                                result = 1 - result;
                        }
                    }
                    else
                    {
                        if (ipNext.X > pt.X)
                        {
                            var d = (double) (ip.X - pt.X) * (ipNext.Y - pt.Y)
                                - (double) (ipNext.X - pt.X) * (ip.Y - pt.Y);
                            if (d == 0)
                                return -1;
                            if (d > 0 == ipNext.Y > ip.Y)
                                result = 1 - result;
                        }
                    }
                }
                ip = ipNext;
            }
            return result;
        }

        public static Paths PolyTreeToPaths(PolyTree polytree)
        {
            var result = new Paths();
            result.Capacity = polytree.Total;
            AddPolyNodeToPaths(polytree, NodeType.NtAny, result);
            return result;
        }
        //------------------------------------------------------------------------------

        public static void ReversePaths(Paths polys)
        {
            foreach (var poly in polys)
            {
                poly.Reverse();
            }
        }

        //------------------------------------------------------------------------------
        // SimplifyPolygon functions ...
        // Convert self-intersecting polygons into simple polygons
        //------------------------------------------------------------------------------

        public static Paths SimplifyPolygon(Path poly, PolyFillType fillType = PolyFillType.PftEvenOdd)
        {
            var result = new Paths();
            var c = new Clipper();
            c.StrictlySimple = true;
            c.AddPath(poly, PolyType.PtSubject, true);
            c.Execute(ClipType.CtUnion, result, fillType, fillType);
            return result;
        }
        //------------------------------------------------------------------------------

        public static Paths SimplifyPolygons(Paths polys, PolyFillType fillType = PolyFillType.PftEvenOdd)
        {
            var result = new Paths();
            var c = new Clipper();
            c.StrictlySimple = true;
            c.AddPaths(polys, PolyType.PtSubject, true);
            c.Execute(ClipType.CtUnion, result, fillType, fillType);
            return result;
        }
        //------------------------------------------------------------------------------

        public bool Execute(ClipType clipType, Paths solution, PolyFillType fillType = PolyFillType.PftEvenOdd)
        {
            return this.Execute(clipType, solution, fillType, fillType);
        }
        //------------------------------------------------------------------------------

        public bool Execute(ClipType clipType, PolyTree polytree, PolyFillType fillType = PolyFillType.PftEvenOdd)
        {
            return this.Execute(clipType, polytree, fillType, fillType);
        }
        //------------------------------------------------------------------------------

        public bool Execute(ClipType clipType, Paths solution, PolyFillType subjFillType, PolyFillType clipFillType)
        {
            if (this.mExecuteLocked)
                return false;
            if (this.MHasOpenPaths)
                throw new ClipperException("Error: PolyTree struct is needed for open path clipping.");

            this.mExecuteLocked = true;
            solution.Clear();
            this.mSubjFillType = subjFillType;
            this.mClipFillType = clipFillType;
            this.mClipType = clipType;
            this.mUsingPolyTree = false;
            bool succeeded;
            try
            {
                succeeded = this.ExecuteInternal();
                //build the return polygons ...
                if (succeeded)
                    this.BuildResult(solution);
            }
            finally
            {
                this.DisposeAllPolyPts();
                this.mExecuteLocked = false;
            }
            return succeeded;
        }
        //------------------------------------------------------------------------------

        public bool Execute(ClipType clipType, PolyTree polytree, PolyFillType subjFillType, PolyFillType clipFillType)
        {
            if (this.mExecuteLocked)
                return false;
            this.mExecuteLocked = true;
            this.mSubjFillType = subjFillType;
            this.mClipFillType = clipFillType;
            this.mClipType = clipType;
            this.mUsingPolyTree = true;
            bool succeeded;
            try
            {
                succeeded = this.ExecuteInternal();
                //build the return polygons ...
                if (succeeded)
                    this.BuildResult2(polytree);
            }
            finally
            {
                this.DisposeAllPolyPts();
                this.mExecuteLocked = false;
            }
            return succeeded;
        }

        #endregion

        #region Methods

        //------------------------------------------------------------------------------

        internal static void AddPolyNodeToPaths(PolyNode polynode, NodeType nt, Paths paths)
        {
            var match = true;
            switch (nt)
            {
                case NodeType.NtOpen: return;
                case NodeType.NtClosed:
                    match = !polynode.IsOpen;
                    break;
                default: break;
            }

            if (polynode.MPolygon.Count > 0 && match)
                paths.Add(polynode.MPolygon);
            foreach (var pn in polynode.Childs)
                AddPolyNodeToPaths(pn, nt, paths);
        }
        //------------------------------------------------------------------------------

        internal static Paths Minkowski(Path pattern, Path path, bool isSum, bool isClosed)
        {
            var delta = isClosed ? 1 : 0;
            var polyCnt = pattern.Count;
            var pathCnt = path.Count;
            var result = new Paths(pathCnt);
            if (isSum)
                for (var i = 0; i < pathCnt; i++)
                {
                    var p = new Path(polyCnt);
                    foreach (var ip in pattern)
                        p.Add(new IntPoint(path[i].X + ip.X, path[i].Y + ip.Y));
                    result.Add(p);
                }
            else
                for (var i = 0; i < pathCnt; i++)
                {
                    var p = new Path(polyCnt);
                    foreach (var ip in pattern)
                        p.Add(new IntPoint(path[i].X - ip.X, path[i].Y - ip.Y));
                    result.Add(p);
                }

            var quads = new Paths((pathCnt + delta) * (polyCnt + 1));
            for (var i = 0; i < pathCnt - 1 + delta; i++)
            for (var j = 0; j < polyCnt; j++)
            {
                var quad = new Path(4);
                quad.Add(result[i % pathCnt][j % polyCnt]);
                quad.Add(result[(i + 1) % pathCnt][j % polyCnt]);
                quad.Add(result[(i + 1) % pathCnt][(j + 1) % polyCnt]);
                quad.Add(result[i % pathCnt][(j + 1) % polyCnt]);
                if (!Orientation(quad))
                    quad.Reverse();
                quads.Add(quad);
            }
            return quads;
        }
        //------------------------------------------------------------------------------

        internal static cInt Round(double value)
        {
            return value < 0 ? (cInt) (value - 0.5) : (cInt) (value + 0.5);
        }
        //------------------------------------------------------------------------------

        internal double Area(OutRec outRec)
        {
            return this.Area(outRec.Pts);
        }
        //------------------------------------------------------------------------------

        internal double Area(OutPt op)
        {
            var opFirst = op;
            if (op == null)
                return 0;
            double a = 0;
            do
            {
                a = a + (op.Prev.Pt.X + op.Pt.X) * (double) (op.Prev.Pt.Y - op.Pt.Y);
                op = op.Next;
            }
            while (op != opFirst);
            return a * 0.5;
        }
        //------------------------------------------------------------------------------

        internal void FixHoleLinkage(OutRec outRec)
        {
            //skip if an outermost polygon or
            //already already points to the correct FirstLeft ...
            if (outRec.FirstLeft == null || outRec.IsHole != outRec.FirstLeft.IsHole && outRec.FirstLeft.Pts != null)
                return;

            var orfl = outRec.FirstLeft;
            while (orfl != null && (orfl.IsHole == outRec.IsHole || orfl.Pts == null))
                orfl = orfl.FirstLeft;
            outRec.FirstLeft = orfl;
        }
        //------------------------------------------------------------------------------

        internal Edge GetMaximaPair(Edge e)
        {
            if (e.Next.Top == e.Top && e.Next.NextInLml == null)
                return e.Next;
            if (e.Prev.Top == e.Top && e.Prev.NextInLml == null)
                return e.Prev;
            return null;
        }
        //------------------------------------------------------------------------------

        internal Edge GetMaximaPairEx(Edge e)
        {
            //as above but returns null if MaxPair isn't in AEL (unless it's horizontal)
            var result = this.GetMaximaPair(e);
            if (result == null || result.OutIdx == Skip
                || result.NextInAel == result.PrevInAel && !IsHorizontal(result))
                return null;
            return result;
        }
        //------------------------------------------------------------------------------

        internal Boolean PopEdgeFromSel(out Edge e)
        {
            //Pop edge from front of SEL (ie SEL is a FILO list)
            e = this.mSortedEdges;
            if (e == null)
                return false;
            var oldE = e;
            this.mSortedEdges = e.NextInSel;
            if (this.mSortedEdges != null)
                this.mSortedEdges.PrevInSel = null;
            oldE.NextInSel = null;
            oldE.PrevInSel = null;
            return true;
        }
        //------------------------------------------------------------------------------

#if use_xyz
        internal void SetZ(ref IntPoint pt, Edge e1, Edge e2)
        {
            if (pt.Z != 0 || this.ZFillFunction == null)
                return;
            if (pt == e1.Bot)
                pt.Z = e1.Bot.Z;
            else if (pt == e1.Top)
                pt.Z = e1.Top.Z;
            else if (pt == e2.Bot)
                pt.Z = e2.Bot.Z;
            else if (pt == e2.Top)
                pt.Z = e2.Top.Z;
            else
                this.ZFillFunction(e1.Bot, e1.Top, e2.Bot, e2.Top, ref pt);
        }
        //------------------------------------------------------------------------------
#endif
        //------------------------------------------------------------------------------

        internal void SwapPoints(ref IntPoint pt1, ref IntPoint pt2)
        {
            var tmp = new IntPoint(pt1);
            pt1 = pt2;
            pt2 = tmp;
        }
        //------------------------------------------------------------------------------

        private static double DistanceFromLineSqrd(IntPoint pt, IntPoint ln1, IntPoint ln2)
        {
            //The equation of a line in general form (Ax + By + C = 0)
            //given 2 points (x¹,y¹) & (x²,y²) is ...
            //(y¹ - y²)x + (x² - x¹)y + (y² - y¹)x¹ - (x² - x¹)y¹ = 0
            //A = (y¹ - y²); B = (x² - x¹); C = (y² - y¹)x¹ - (x² - x¹)y¹
            //perpendicular distance of point (x³,y³) = (Ax³ + By³ + C)/Sqrt(A² + B²)
            //see http://en.wikipedia.org/wiki/Perpendicular_distance
            double a = ln1.Y - ln2.Y;
            double b = ln2.X - ln1.X;
            var c = a * ln1.X + b * ln1.Y;
            c = a * pt.X + b * pt.Y - c;
            return c * c / (a * a + b * b);
        }
        //------------------------------------------------------------------------------

        private static double DistanceSqrd(IntPoint pt1, IntPoint pt2)
        {
            var dx = (double) pt1.X - pt2.X;
            var dy = (double) pt1.Y - pt2.Y;
            return dx * dx + dy * dy;
        }
        //------------------------------------------------------------------------------

        private static OutPt ExcludeOp(OutPt op)
        {
            var result = op.Prev;
            result.Next = op.Next;
            op.Next.Prev = result;
            result.Idx = 0;
            return result;
        }
        //------------------------------------------------------------------------------

        private static int IntersectNodeSort(IntersectNode node1, IntersectNode node2)
        {
            //the following typecast is safe because the differences in Pt.Y will
            //be limited to the height of the scanbeam.
            return (int) (node2.Pt.Y - node1.Pt.Y);
        }
        //----------------------------------------------------------------------

        private static OutRec ParseFirstLeft(OutRec firstLeft)
        {
            while (firstLeft != null && firstLeft.Pts == null)
                firstLeft = firstLeft.FirstLeft;
            return firstLeft;
        }
        //------------------------------------------------------------------------------

        //See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
        //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
        private static int PointInPolygon(IntPoint pt, OutPt op)
        {
            //returns 0 if false, +1 if true, -1 if pt ON polygon boundary
            var result = 0;
            var startOp = op;
            cInt ptx = pt.X, pty = pt.Y;
            cInt poly0X = op.Pt.X, poly0Y = op.Pt.Y;
            do
            {
                op = op.Next;
                cInt poly1X = op.Pt.X, poly1Y = op.Pt.Y;

                if (poly1Y == pty)
                {
                    if (poly1X == ptx || poly0Y == pty && poly1X > ptx == poly0X < ptx)
                        return -1;
                }
                if (poly0Y < pty != poly1Y < pty)
                {
                    if (poly0X >= ptx)
                    {
                        if (poly1X > ptx)
                            result = 1 - result;
                        else
                        {
                            var d = (double) (poly0X - ptx) * (poly1Y - pty) - (double) (poly1X - ptx) * (poly0Y - pty);
                            if (d == 0)
                                return -1;
                            if (d > 0 == poly1Y > poly0Y)
                                result = 1 - result;
                        }
                    }
                    else
                    {
                        if (poly1X > ptx)
                        {
                            var d = (double) (poly0X - ptx) * (poly1Y - pty) - (double) (poly1X - ptx) * (poly0Y - pty);
                            if (d == 0)
                                return -1;
                            if (d > 0 == poly1Y > poly0Y)
                                result = 1 - result;
                        }
                    }
                }
                poly0X = poly1X;
                poly0Y = poly1Y;
            }
            while (startOp != op);
            return result;
        }
        //------------------------------------------------------------------------------

        private static bool PointsAreClose(IntPoint pt1, IntPoint pt2, double distSqrd)
        {
            var dx = (double) pt1.X - pt2.X;
            var dy = (double) pt1.Y - pt2.Y;
            return dx * dx + dy * dy <= distSqrd;
        }
        //------------------------------------------------------------------------------

        private static bool Poly2ContainsPoly1(OutPt outPt1, OutPt outPt2)
        {
            var op = outPt1;
            do
            {
                //nb: PointInPolygon returns 0 if false, +1 if true, -1 if pt on polygon
                var res = PointInPolygon(op.Pt, outPt2);
                if (res >= 0)
                    return res > 0;
                op = op.Next;
            }
            while (op != outPt1);
            return true;
        }
        //---------------------------------------------------------------------------

        private static bool SlopesNearCollinear(IntPoint pt1, IntPoint pt2, IntPoint pt3, double distSqrd)
        {
            //this function is more accurate when the point that's GEOMETRICALLY 
            //between the other 2 points is the one that's tested for distance.  
            //nb: with 'spikes', either pt1 or pt3 is geometrically between the other pts                    
            if (Math.Abs(pt1.X - pt2.X) > Math.Abs(pt1.Y - pt2.Y))
            {
                if (pt1.X > pt2.X == pt1.X < pt3.X)
                    return DistanceFromLineSqrd(pt1, pt2, pt3) < distSqrd;
                if (pt2.X > pt1.X == pt2.X < pt3.X)
                    return DistanceFromLineSqrd(pt2, pt1, pt3) < distSqrd;
                return DistanceFromLineSqrd(pt3, pt1, pt2) < distSqrd;
            }
            if (pt1.Y > pt2.Y == pt1.Y < pt3.Y)
                return DistanceFromLineSqrd(pt1, pt2, pt3) < distSqrd;
            if (pt2.Y > pt1.Y == pt2.Y < pt3.Y)
                return DistanceFromLineSqrd(pt2, pt1, pt3) < distSqrd;
            return DistanceFromLineSqrd(pt3, pt1, pt2) < distSqrd;
        }
        //------------------------------------------------------------------------------

        private static void SwapPolyIndexes(Edge edge1, Edge edge2)
        {
            var outIdx = edge1.OutIdx;
            edge1.OutIdx = edge2.OutIdx;
            edge2.OutIdx = outIdx;
        }
        //------------------------------------------------------------------------------

        private static void SwapSides(Edge edge1, Edge edge2)
        {
            var side = edge1.Side;
            edge1.Side = edge2.Side;
            edge2.Side = side;
        }
        //------------------------------------------------------------------------------

        private static cInt TopX(Edge edge, cInt currentY)
        {
            if (currentY == edge.Top.Y)
                return edge.Top.X;
            return edge.Bot.X + Round(edge.Dx * (currentY - edge.Bot.Y));
        }
        //------------------------------------------------------------------------------

        private static Path TranslatePath(Path path, IntPoint delta)
        {
            var outPath = new Path(path.Count);
            for (var i = 0; i < path.Count; i++)
                outPath.Add(new IntPoint(path[i].X + delta.X, path[i].Y + delta.Y));
            return outPath;
        }
        //------------------------------------------------------------------------------

        private void AddEdgeToSel(Edge edge)
        {
            //SEL pointers in PEdge are use to build transient lists of horizontal edges.
            //However, since we don't need to worry about processing order, all additions
            //are made to the front of the list ...
            if (this.mSortedEdges == null)
            {
                this.mSortedEdges = edge;
                edge.PrevInSel = null;
                edge.NextInSel = null;
            }
            else
            {
                edge.NextInSel = this.mSortedEdges;
                edge.PrevInSel = null;
                this.mSortedEdges.PrevInSel = edge;
                this.mSortedEdges = edge;
            }
        }
        //------------------------------------------------------------------------------

        private void AddGhostJoin(OutPt op, IntPoint offPt)
        {
            var j = new Join();
            j.OutPt1 = op;
            j.OffPt = offPt;
            this.mGhostJoins.Add(j);
        }
        //------------------------------------------------------------------------------

        private void AddJoin(OutPt op1, OutPt op2, IntPoint offPt)
        {
            var j = new Join();
            j.OutPt1 = op1;
            j.OutPt2 = op2;
            j.OffPt = offPt;
            this.mJoins.Add(j);
        }
        //------------------------------------------------------------------------------

        private void AddLocalMaxPoly(Edge e1, Edge e2, IntPoint pt)
        {
            this.AddOutPt(e1, pt);
            if (e2.WindDelta == 0)
                this.AddOutPt(e2, pt);
            if (e1.OutIdx == e2.OutIdx)
            {
                e1.OutIdx = Unassigned;
                e2.OutIdx = Unassigned;
            }
            else if (e1.OutIdx < e2.OutIdx)
                this.AppendPolygon(e1, e2);
            else
                this.AppendPolygon(e2, e1);
        }
        //------------------------------------------------------------------------------

        private OutPt AddLocalMinPoly(Edge e1, Edge e2, IntPoint pt)
        {
            OutPt result;
            Edge e, prevE;
            if (IsHorizontal(e2) || e1.Dx > e2.Dx)
            {
                result = this.AddOutPt(e1, pt);
                e2.OutIdx = e1.OutIdx;
                e1.Side = EdgeSide.EsLeft;
                e2.Side = EdgeSide.EsRight;
                e = e1;
                if (e.PrevInAel == e2)
                    prevE = e2.PrevInAel;
                else
                    prevE = e.PrevInAel;
            }
            else
            {
                result = this.AddOutPt(e2, pt);
                e1.OutIdx = e2.OutIdx;
                e1.Side = EdgeSide.EsRight;
                e2.Side = EdgeSide.EsLeft;
                e = e2;
                if (e.PrevInAel == e1)
                    prevE = e1.PrevInAel;
                else
                    prevE = e.PrevInAel;
            }

            if (prevE != null && prevE.OutIdx >= 0 && prevE.Top.Y < pt.Y && e.Top.Y < pt.Y)
            {
                var xPrev = TopX(prevE, pt.Y);
                var xE = TopX(e, pt.Y);
                if (xPrev == xE && e.WindDelta != 0 && prevE.WindDelta != 0 && SlopesEqual(
                    new IntPoint(xPrev, pt.Y),
                    prevE.Top,
                    new IntPoint(xE, pt.Y),
                    e.Top,
                    this.MUseFullRange))
                {
                    var outPt = this.AddOutPt(prevE, pt);
                    this.AddJoin(result, outPt, e.Top);
                }
            }
            return result;
        }
        //------------------------------------------------------------------------------

        private OutPt AddOutPt(Edge e, IntPoint pt)
        {
            if (e.OutIdx < 0)
            {
                var outRec = this.CreateOutRec();
                outRec.IsOpen = e.WindDelta == 0;
                var newOp = new OutPt();
                outRec.Pts = newOp;
                newOp.Idx = outRec.Idx;
                newOp.Pt = pt;
                newOp.Next = newOp;
                newOp.Prev = newOp;
                if (!outRec.IsOpen)
                    this.SetHoleState(e, outRec);
                e.OutIdx = outRec.Idx; //nb: do this after SetZ !
                return newOp;
            }
            else
            {
                var outRec = this.MPolyOuts[e.OutIdx];
                //OutRec.Pts is the 'Left-most' point & OutRec.Pts.Prev is the 'Right-most'
                var op = outRec.Pts;
                var toFront = e.Side == EdgeSide.EsLeft;
                if (toFront && pt == op.Pt)
                    return op;
                if (!toFront && pt == op.Prev.Pt)
                    return op.Prev;

                var newOp = new OutPt();
                newOp.Idx = outRec.Idx;
                newOp.Pt = pt;
                newOp.Next = op;
                newOp.Prev = op.Prev;
                newOp.Prev.Next = newOp;
                op.Prev = newOp;
                if (toFront)
                    outRec.Pts = newOp;
                return newOp;
            }
        }
        //------------------------------------------------------------------------------

        private void AppendPolygon(Edge e1, Edge e2)
        {
            var outRec1 = this.MPolyOuts[e1.OutIdx];
            var outRec2 = this.MPolyOuts[e2.OutIdx];

            OutRec holeStateRec;
            if (this.OutRec1RightOfOutRec2(outRec1, outRec2))
                holeStateRec = outRec2;
            else if (this.OutRec1RightOfOutRec2(outRec2, outRec1))
                holeStateRec = outRec1;
            else
                holeStateRec = this.GetLowermostRec(outRec1, outRec2);

            //get the start and ends of both output polygons and
            //join E2 poly onto E1 poly and delete pointers to E2 ...
            var p1Lft = outRec1.Pts;
            var p1Rt = p1Lft.Prev;
            var p2Lft = outRec2.Pts;
            var p2Rt = p2Lft.Prev;

            //join e2 poly onto e1 poly and delete pointers to e2 ...
            if (e1.Side == EdgeSide.EsLeft)
            {
                if (e2.Side == EdgeSide.EsLeft)
                {
                    //z y x a b c
                    this.ReversePolyPtLinks(p2Lft);
                    p2Lft.Next = p1Lft;
                    p1Lft.Prev = p2Lft;
                    p1Rt.Next = p2Rt;
                    p2Rt.Prev = p1Rt;
                    outRec1.Pts = p2Rt;
                }
                else
                {
                    //x y z a b c
                    p2Rt.Next = p1Lft;
                    p1Lft.Prev = p2Rt;
                    p2Lft.Prev = p1Rt;
                    p1Rt.Next = p2Lft;
                    outRec1.Pts = p2Lft;
                }
            }
            else
            {
                if (e2.Side == EdgeSide.EsRight)
                {
                    //a b c z y x
                    this.ReversePolyPtLinks(p2Lft);
                    p1Rt.Next = p2Rt;
                    p2Rt.Prev = p1Rt;
                    p2Lft.Next = p1Lft;
                    p1Lft.Prev = p2Lft;
                }
                else
                {
                    //a b c x y z
                    p1Rt.Next = p2Lft;
                    p2Lft.Prev = p1Rt;
                    p1Lft.Prev = p2Rt;
                    p2Rt.Next = p1Lft;
                }
            }

            outRec1.BottomPt = null;
            if (holeStateRec == outRec2)
            {
                if (outRec2.FirstLeft != outRec1)
                    outRec1.FirstLeft = outRec2.FirstLeft;
                outRec1.IsHole = outRec2.IsHole;
            }
            outRec2.Pts = null;
            outRec2.BottomPt = null;

            outRec2.FirstLeft = outRec1;

            var okIdx = e1.OutIdx;
            var obsoleteIdx = e2.OutIdx;

            e1.OutIdx = Unassigned; //nb: safe because we only get here via AddLocalMaxPoly
            e2.OutIdx = Unassigned;

            var e = this.MActiveEdges;
            while (e != null)
            {
                if (e.OutIdx == obsoleteIdx)
                {
                    e.OutIdx = okIdx;
                    e.Side = e1.Side;
                    break;
                }
                e = e.NextInAel;
            }
            outRec2.Idx = outRec1.Idx;
        }
        //------------------------------------------------------------------------------

        private void BuildIntersectList(cInt topY)
        {
            if (this.MActiveEdges == null)
                return;

            //prepare for sorting ...
            var e = this.MActiveEdges;
            this.mSortedEdges = e;
            while (e != null)
            {
                e.PrevInSel = e.PrevInAel;
                e.NextInSel = e.NextInAel;
                e.Curr.X = TopX(e, topY);
                e = e.NextInAel;
            }

            //bubblesort ...
            var isModified = true;
            while (isModified && this.mSortedEdges != null)
            {
                isModified = false;
                e = this.mSortedEdges;
                while (e.NextInSel != null)
                {
                    var eNext = e.NextInSel;
                    if (e.Curr.X > eNext.Curr.X)
                    {
                        this.IntersectPoint(e, eNext, out IntPoint pt);
                        if (pt.Y < topY)
                            pt = new IntPoint(TopX(e, topY), topY);
                        var newNode = new IntersectNode();
                        newNode.Edge1 = e;
                        newNode.Edge2 = eNext;
                        newNode.Pt = pt;
                        this.mIntersectList.Add(newNode);

                        this.SwapPositionsInSel(e, eNext);
                        isModified = true;
                    }
                    else
                        e = eNext;
                }
                if (e.PrevInSel != null)
                    e.PrevInSel.NextInSel = null;
                else
                    break;
            }
            this.mSortedEdges = null;
        }
        //------------------------------------------------------------------------------

        private void BuildResult(Paths polyg)
        {
            polyg.Clear();
            polyg.Capacity = this.MPolyOuts.Count;
            for (var i = 0; i < this.MPolyOuts.Count; i++)
            {
                var outRec = this.MPolyOuts[i];
                if (outRec.Pts == null)
                    continue;
                var p = outRec.Pts.Prev;
                var cnt = this.PointCount(p);
                if (cnt < 2)
                    continue;
                var pg = new Path(cnt);
                for (var j = 0; j < cnt; j++)
                {
                    pg.Add(p.Pt);
                    p = p.Prev;
                }
                polyg.Add(pg);
            }
        }
        //------------------------------------------------------------------------------

        private void BuildResult2(PolyTree polytree)
        {
            polytree.Clear();

            //add each output polygon/contour to polytree ...
            polytree.MAllPolys.Capacity = this.MPolyOuts.Count;
            for (var i = 0; i < this.MPolyOuts.Count; i++)
            {
                var outRec = this.MPolyOuts[i];
                var cnt = this.PointCount(outRec.Pts);
                if (outRec.IsOpen && cnt < 2 || !outRec.IsOpen && cnt < 3)
                    continue;
                this.FixHoleLinkage(outRec);
                var pn = new PolyNode();
                polytree.MAllPolys.Add(pn);
                outRec.PolyNode = pn;
                pn.MPolygon.Capacity = cnt;
                var op = outRec.Pts.Prev;
                for (var j = 0; j < cnt; j++)
                {
                    pn.MPolygon.Add(op.Pt);
                    op = op.Prev;
                }
            }

            //fixup PolyNode links etc ...
            polytree.MChilds.Capacity = this.MPolyOuts.Count;
            for (var i = 0; i < this.MPolyOuts.Count; i++)
            {
                var outRec = this.MPolyOuts[i];
                if (outRec.PolyNode == null)
                    continue;
                if (outRec.IsOpen)
                {
                    outRec.PolyNode.IsOpen = true;
                    polytree.AddChild(outRec.PolyNode);
                }
                else if (outRec.FirstLeft != null && outRec.FirstLeft.PolyNode != null)
                    outRec.FirstLeft.PolyNode.AddChild(outRec.PolyNode);
                else
                    polytree.AddChild(outRec.PolyNode);
            }
        }
        //------------------------------------------------------------------------------

        private void CopyAeltoSel()
        {
            var e = this.MActiveEdges;
            this.mSortedEdges = e;
            while (e != null)
            {
                e.PrevInSel = e.PrevInAel;
                e.NextInSel = e.NextInAel;
                e = e.NextInAel;
            }
        }
        //------------------------------------------------------------------------------

        private void DeleteFromSel(Edge e)
        {
            var selPrev = e.PrevInSel;
            var selNext = e.NextInSel;
            if (selPrev == null && selNext == null && e != this.mSortedEdges)
                return; //already deleted
            if (selPrev != null)
                selPrev.NextInSel = selNext;
            else
                this.mSortedEdges = selNext;
            if (selNext != null)
                selNext.PrevInSel = selPrev;
            e.NextInSel = null;
            e.PrevInSel = null;
        }
        //------------------------------------------------------------------------------

        private void DisposeAllPolyPts()
        {
            for (var i = 0; i < this.MPolyOuts.Count; ++i)
                this.DisposeOutRec(i);
            this.MPolyOuts.Clear();
        }
        //------------------------------------------------------------------------------

        private void DoMaxima(Edge e)
        {
            var eMaxPair = this.GetMaximaPairEx(e);
            if (eMaxPair == null)
            {
                if (e.OutIdx >= 0)
                    this.AddOutPt(e, e.Top);
                this.DeleteFromAel(e);
                return;
            }

            var eNext = e.NextInAel;
            while (eNext != null && eNext != eMaxPair)
            {
                this.IntersectEdges(e, eNext, e.Top);
                this.SwapPositionsInAel(e, eNext);
                eNext = e.NextInAel;
            }

            if (e.OutIdx == Unassigned && eMaxPair.OutIdx == Unassigned)
            {
                this.DeleteFromAel(e);
                this.DeleteFromAel(eMaxPair);
            }
            else if (e.OutIdx >= 0 && eMaxPair.OutIdx >= 0)
            {
                if (e.OutIdx >= 0)
                    this.AddLocalMaxPoly(e, eMaxPair, e.Top);
                this.DeleteFromAel(e);
                this.DeleteFromAel(eMaxPair);
            }
#if use_lines
            else if (e.WindDelta == 0)
            {
                if (e.OutIdx >= 0)
                {
                    this.AddOutPt(e, e.Top);
                    e.OutIdx = Unassigned;
                }
                this.DeleteFromAel(e);

                if (eMaxPair.OutIdx >= 0)
                {
                    this.AddOutPt(eMaxPair, e.Top);
                    eMaxPair.OutIdx = Unassigned;
                }
                this.DeleteFromAel(eMaxPair);
            }
#endif
            else
                throw new ClipperException("DoMaxima error");
        }
        //------------------------------------------------------------------------------

        private void DoSimplePolygons()
        {
            var i = 0;
            while (i < this.MPolyOuts.Count)
            {
                var outrec = this.MPolyOuts[i++];
                var op = outrec.Pts;
                if (op == null || outrec.IsOpen)
                    continue;
                do //for each Pt in Polygon until duplicate found do ...
                {
                    var op2 = op.Next;
                    while (op2 != outrec.Pts)
                    {
                        if (op.Pt == op2.Pt && op2.Next != op && op2.Prev != op)
                        {
                            //split the polygon into two ...
                            var op3 = op.Prev;
                            var op4 = op2.Prev;
                            op.Prev = op4;
                            op4.Next = op;
                            op2.Prev = op3;
                            op3.Next = op2;

                            outrec.Pts = op;
                            var outrec2 = this.CreateOutRec();
                            outrec2.Pts = op2;
                            this.UpdateOutPtIdxs(outrec2);
                            if (Poly2ContainsPoly1(outrec2.Pts, outrec.Pts))
                            {
                                //OutRec2 is contained by OutRec1 ...
                                outrec2.IsHole = !outrec.IsHole;
                                outrec2.FirstLeft = outrec;
                                if (this.mUsingPolyTree)
                                    this.FixupFirstLefts2(outrec2, outrec);
                            }
                            else if (Poly2ContainsPoly1(outrec.Pts, outrec2.Pts))
                            {
                                //OutRec1 is contained by OutRec2 ...
                                outrec2.IsHole = outrec.IsHole;
                                outrec.IsHole = !outrec2.IsHole;
                                outrec2.FirstLeft = outrec.FirstLeft;
                                outrec.FirstLeft = outrec2;
                                if (this.mUsingPolyTree)
                                    this.FixupFirstLefts2(outrec, outrec2);
                            }
                            else
                            {
                                //the 2 polygons are separate ...
                                outrec2.IsHole = outrec.IsHole;
                                outrec2.FirstLeft = outrec.FirstLeft;
                                if (this.mUsingPolyTree)
                                    this.FixupFirstLefts1(outrec, outrec2);
                            }
                            op2 = op; //ie get ready for the next iteration
                        }
                        op2 = op2.Next;
                    }
                    op = op.Next;
                }
                while (op != outrec.Pts);
            }
        }
        //------------------------------------------------------------------------------

        OutPt DupOutPt(OutPt outPt, bool insertAfter)
        {
            var result = new OutPt();
            result.Pt = outPt.Pt;
            result.Idx = outPt.Idx;
            if (insertAfter)
            {
                result.Next = outPt.Next;
                result.Prev = outPt;
                outPt.Next.Prev = result;
                outPt.Next = result;
            }
            else
            {
                result.Prev = outPt.Prev;
                result.Next = outPt;
                outPt.Prev.Next = result;
                outPt.Prev = result;
            }
            return result;
        }
        //----------------------------------------------------------------------

        private bool E2InsertsBeforeE1(Edge e1, Edge e2)
        {
            if (e2.Curr.X == e1.Curr.X)
            {
                if (e2.Top.Y > e1.Top.Y)
                    return e2.Top.X < TopX(e1, e2.Top.Y);
                return e1.Top.X > TopX(e2, e1.Top.Y);
            }
            return e2.Curr.X < e1.Curr.X;
        }
        //------------------------------------------------------------------------------

        private bool EdgesAdjacent(IntersectNode inode)
        {
            return inode.Edge1.NextInSel == inode.Edge2 || inode.Edge1.PrevInSel == inode.Edge2;
        }
        //------------------------------------------------------------------------------

        private bool ExecuteInternal()
        {
            try
            {
                this.Reset();
                this.mSortedEdges = null;
                this.mMaxima = null;
                if (!this.PopScanbeam(out long botY))
                    return false;
                this.InsertLocalMinimaIntoAel(botY);
                while (this.PopScanbeam(out long topY) || this.LocalMinimaPending())
                {
                    this.ProcessHorizontals();
                    this.mGhostJoins.Clear();
                    if (!this.ProcessIntersections(topY))
                        return false;
                    this.ProcessEdgesAtTopOfScanbeam(topY);
                    botY = topY;
                    this.InsertLocalMinimaIntoAel(botY);
                }

                //fix orientations ...
                foreach (var outRec in this.MPolyOuts)
                {
                    if (outRec.Pts == null || outRec.IsOpen)
                        continue;
                    if ((outRec.IsHole ^ this.ReverseSolution) == this.Area(outRec) > 0)
                        this.ReversePolyPtLinks(outRec.Pts);
                }

                this.JoinCommonEdges();

                foreach (var outRec in this.MPolyOuts)
                {
                    if (outRec.Pts == null)
                        continue;
                    if (outRec.IsOpen)
                        this.FixupOutPolyline(outRec);
                    else
                        this.FixupOutPolygon(outRec);
                }

                if (this.StrictlySimple)
                    this.DoSimplePolygons();
                return true;
            }
            //catch { return false; }
            finally
            {
                this.mJoins.Clear();
                this.mGhostJoins.Clear();
            }
        }
        //---------------------------------------------------------------------------

        private bool FirstIsBottomPt(OutPt btmPt1, OutPt btmPt2)
        {
            var p = btmPt1.Prev;
            while (p.Pt == btmPt1.Pt && p != btmPt1)
                p = p.Prev;
            var dx1P = Math.Abs(this.GetDx(btmPt1.Pt, p.Pt));
            p = btmPt1.Next;
            while (p.Pt == btmPt1.Pt && p != btmPt1)
                p = p.Next;
            var dx1N = Math.Abs(this.GetDx(btmPt1.Pt, p.Pt));

            p = btmPt2.Prev;
            while (p.Pt == btmPt2.Pt && p != btmPt2)
                p = p.Prev;
            var dx2P = Math.Abs(this.GetDx(btmPt2.Pt, p.Pt));
            p = btmPt2.Next;
            while (p.Pt == btmPt2.Pt && p != btmPt2)
                p = p.Next;
            var dx2N = Math.Abs(this.GetDx(btmPt2.Pt, p.Pt));

            if (Math.Max(dx1P, dx1N) == Math.Max(dx2P, dx2N) && Math.Min(dx1P, dx1N) == Math.Min(dx2P, dx2N))
                return this.Area(btmPt1) > 0; //if otherwise identical use orientation
            return dx1P >= dx2P && dx1P >= dx2N || dx1N >= dx2P && dx1N >= dx2N;
        }
        //----------------------------------------------------------------------

        private void FixupFirstLefts1(OutRec oldOutRec, OutRec newOutRec)
        {
            foreach (var outRec in this.MPolyOuts)
            {
                var firstLeft = ParseFirstLeft(outRec.FirstLeft);
                if (outRec.Pts != null && firstLeft == oldOutRec)
                {
                    if (Poly2ContainsPoly1(outRec.Pts, newOutRec.Pts))
                        outRec.FirstLeft = newOutRec;
                }
            }
        }
        //----------------------------------------------------------------------

        private void FixupFirstLefts2(OutRec innerOutRec, OutRec outerOutRec)
        {
            //A polygon has split into two such that one is now the inner of the other.
            //It's possible that these polygons now wrap around other polygons, so check
            //every polygon that's also contained by OuterOutRec's FirstLeft container
            //(including nil) to see if they've become inner to the new inner polygon ...
            var orfl = outerOutRec.FirstLeft;
            foreach (var outRec in this.MPolyOuts)
            {
                if (outRec.Pts == null || outRec == outerOutRec || outRec == innerOutRec)
                    continue;
                var firstLeft = ParseFirstLeft(outRec.FirstLeft);
                if (firstLeft != orfl && firstLeft != innerOutRec && firstLeft != outerOutRec)
                    continue;
                if (Poly2ContainsPoly1(outRec.Pts, innerOutRec.Pts))
                    outRec.FirstLeft = innerOutRec;
                else if (Poly2ContainsPoly1(outRec.Pts, outerOutRec.Pts))
                    outRec.FirstLeft = outerOutRec;
                else if (outRec.FirstLeft == innerOutRec || outRec.FirstLeft == outerOutRec)
                    outRec.FirstLeft = orfl;
            }
        }
        //----------------------------------------------------------------------

        private void FixupFirstLefts3(OutRec oldOutRec, OutRec newOutRec)
        {
            //same as FixupFirstLefts1 but doesn't call Poly2ContainsPoly1()
            foreach (var outRec in this.MPolyOuts)
            {
                var firstLeft = ParseFirstLeft(outRec.FirstLeft);
                if (outRec.Pts != null && firstLeft == oldOutRec)
                    outRec.FirstLeft = newOutRec;
            }
        }
        //------------------------------------------------------------------------------

        private bool FixupIntersectionOrder()
        {
            //pre-condition: intersections are sorted bottom-most first.
            //Now it's crucial that intersections are made only between adjacent edges,
            //so to ensure this the order of intersections may need adjusting ...
            this.mIntersectList.Sort(this.mIntersectNodeComparer);

            this.CopyAeltoSel();
            var cnt = this.mIntersectList.Count;
            for (var i = 0; i < cnt; i++)
            {
                if (!this.EdgesAdjacent(this.mIntersectList[i]))
                {
                    var j = i + 1;
                    while (j < cnt && !this.EdgesAdjacent(this.mIntersectList[j]))
                        j++;
                    if (j == cnt)
                        return false;

                    var tmp = this.mIntersectList[i];
                    this.mIntersectList[i] = this.mIntersectList[j];
                    this.mIntersectList[j] = tmp;
                }
                this.SwapPositionsInSel(this.mIntersectList[i].Edge1, this.mIntersectList[i].Edge2);
            }
            return true;
        }
        //------------------------------------------------------------------------------

        private void FixupOutPolygon(OutRec outRec)
        {
            //FixupOutPolygon() - removes duplicate points and simplifies consecutive
            //parallel edges by removing the middle vertex.
            OutPt lastOk = null;
            outRec.BottomPt = null;
            var pp = outRec.Pts;
            var preserveCol = this.PreserveCollinear || this.StrictlySimple;
            for (;;)
            {
                if (pp.Prev == pp || pp.Prev == pp.Next)
                {
                    outRec.Pts = null;
                    return;
                }
                //test for duplicate points and collinear edges ...
                if (pp.Pt == pp.Next.Pt || pp.Pt == pp.Prev.Pt
                    || SlopesEqual(pp.Prev.Pt, pp.Pt, pp.Next.Pt, this.MUseFullRange) && (!preserveCol
                        || !this.Pt2IsBetweenPt1AndPt3(pp.Prev.Pt, pp.Pt, pp.Next.Pt)))
                {
                    lastOk = null;
                    pp.Prev.Next = pp.Next;
                    pp.Next.Prev = pp.Prev;
                    pp = pp.Prev;
                }
                else if (pp == lastOk)
                    break;
                else
                {
                    if (lastOk == null)
                        lastOk = pp;
                    pp = pp.Next;
                }
            }
            outRec.Pts = pp;
        }
        //------------------------------------------------------------------------------

        private void FixupOutPolyline(OutRec outrec)
        {
            var pp = outrec.Pts;
            var lastPp = pp.Prev;
            while (pp != lastPp)
            {
                pp = pp.Next;
                if (pp.Pt == pp.Prev.Pt)
                {
                    if (pp == lastPp)
                        lastPp = pp.Prev;
                    var tmpPp = pp.Prev;
                    tmpPp.Next = pp.Next;
                    pp.Next.Prev = tmpPp;
                    pp = tmpPp;
                }
            }
            if (pp == pp.Prev)
                outrec.Pts = null;
        }
        //------------------------------------------------------------------------------

        private OutPt GetBottomPt(OutPt pp)
        {
            OutPt dups = null;
            var p = pp.Next;
            while (p != pp)
            {
                if (p.Pt.Y > pp.Pt.Y)
                {
                    pp = p;
                    dups = null;
                }
                else if (p.Pt.Y == pp.Pt.Y && p.Pt.X <= pp.Pt.X)
                {
                    if (p.Pt.X < pp.Pt.X)
                    {
                        dups = null;
                        pp = p;
                    }
                    else
                    {
                        if (p.Next != pp && p.Prev != pp)
                            dups = p;
                    }
                }
                p = p.Next;
            }
            if (dups != null)
            {
                //there appears to be at least 2 vertices at bottomPt so ...
                while (dups != p)
                {
                    if (!this.FirstIsBottomPt(p, dups))
                        pp = dups;
                    dups = dups.Next;
                    while (dups.Pt != pp.Pt)
                        dups = dups.Next;
                }
            }
            return pp;
        }
        //------------------------------------------------------------------------------

        private double GetDx(IntPoint pt1, IntPoint pt2)
        {
            if (pt1.Y == pt2.Y)
                return Horizontal;
            return (double) (pt2.X - pt1.X) / (pt2.Y - pt1.Y);
        }
        //------------------------------------------------------------------------------

        void GetHorzDirection(Edge horzEdge, out Direction dir, out cInt left, out cInt right)
        {
            if (horzEdge.Bot.X < horzEdge.Top.X)
            {
                left = horzEdge.Bot.X;
                right = horzEdge.Top.X;
                dir = Direction.DLeftToRight;
            }
            else
            {
                left = horzEdge.Top.X;
                right = horzEdge.Bot.X;
                dir = Direction.DRightToLeft;
            }
        }
        //------------------------------------------------------------------------------

        private OutPt GetLastOutPt(Edge e)
        {
            var outRec = this.MPolyOuts[e.OutIdx];
            if (e.Side == EdgeSide.EsLeft)
                return outRec.Pts;
            return outRec.Pts.Prev;
        }
        //------------------------------------------------------------------------------

        private OutRec GetLowermostRec(OutRec outRec1, OutRec outRec2)
        {
            //work out which polygon fragment has the correct hole state ...
            if (outRec1.BottomPt == null)
                outRec1.BottomPt = this.GetBottomPt(outRec1.Pts);
            if (outRec2.BottomPt == null)
                outRec2.BottomPt = this.GetBottomPt(outRec2.Pts);
            var bPt1 = outRec1.BottomPt;
            var bPt2 = outRec2.BottomPt;
            if (bPt1.Pt.Y > bPt2.Pt.Y)
                return outRec1;
            if (bPt1.Pt.Y < bPt2.Pt.Y)
                return outRec2;
            if (bPt1.Pt.X < bPt2.Pt.X)
                return outRec1;
            if (bPt1.Pt.X > bPt2.Pt.X)
                return outRec2;
            if (bPt1.Next == bPt1)
                return outRec2;
            if (bPt2.Next == bPt2)
                return outRec1;
            if (this.FirstIsBottomPt(bPt1, bPt2))
                return outRec1;
            return outRec2;
        }
        //------------------------------------------------------------------------------

        private Edge GetNextInAel(Edge e, Direction direction)
        {
            return direction == Direction.DLeftToRight ? e.NextInAel : e.PrevInAel;
        }
        //------------------------------------------------------------------------------

        private OutRec GetOutRec(int idx)
        {
            var outrec = this.MPolyOuts[idx];
            while (outrec != this.MPolyOuts[outrec.Idx])
                outrec = this.MPolyOuts[outrec.Idx];
            return outrec;
        }
        //------------------------------------------------------------------------------

        bool GetOverlap(cInt a1, cInt a2, cInt b1, cInt b2, out cInt left, out cInt right)
        {
            if (a1 < a2)
            {
                if (b1 < b2)
                {
                    left = Math.Max(a1, b1);
                    right = Math.Min(a2, b2);
                }
                else
                {
                    left = Math.Max(a1, b2);
                    right = Math.Min(a2, b1);
                }
            }
            else
            {
                if (b1 < b2)
                {
                    left = Math.Max(a2, b1);
                    right = Math.Min(a1, b2);
                }
                else
                {
                    left = Math.Max(a2, b2);
                    right = Math.Min(a1, b1);
                }
            }
            return left < right;
        }
        //------------------------------------------------------------------------------

        private bool HorzSegmentsOverlap(cInt seg1A, cInt seg1B, cInt seg2A, cInt seg2B)
        {
            if (seg1A > seg1B)
                this.Swap(ref seg1A, ref seg1B);
            if (seg2A > seg2B)
                this.Swap(ref seg2A, ref seg2B);
            return seg1A < seg2B && seg2A < seg1B;
        }
        //------------------------------------------------------------------------------

        private void InsertEdgeIntoAel(Edge edge, Edge startEdge)
        {
            if (this.MActiveEdges == null)
            {
                edge.PrevInAel = null;
                edge.NextInAel = null;
                this.MActiveEdges = edge;
            }
            else if (startEdge == null && this.E2InsertsBeforeE1(this.MActiveEdges, edge))
            {
                edge.PrevInAel = null;
                edge.NextInAel = this.MActiveEdges;
                this.MActiveEdges.PrevInAel = edge;
                this.MActiveEdges = edge;
            }
            else
            {
                if (startEdge == null)
                    startEdge = this.MActiveEdges;
                while (startEdge.NextInAel != null && !this.E2InsertsBeforeE1(startEdge.NextInAel, edge))
                    startEdge = startEdge.NextInAel;
                edge.NextInAel = startEdge.NextInAel;
                if (startEdge.NextInAel != null)
                    startEdge.NextInAel.PrevInAel = edge;
                edge.PrevInAel = startEdge;
                startEdge.NextInAel = edge;
            }
        }

        private void InsertLocalMinimaIntoAel(cInt botY)
        {
            while (this.PopLocalMinima(botY, out LocalMinima lm))
            {
                var lb = lm.LeftBound;
                var rb = lm.RightBound;

                OutPt op1 = null;
                if (lb == null)
                {
                    this.InsertEdgeIntoAel(rb, null);
                    this.SetWindingCount(rb);
                    if (this.IsContributing(rb))
                        op1 = this.AddOutPt(rb, rb.Bot);
                }
                else if (rb == null)
                {
                    this.InsertEdgeIntoAel(lb, null);
                    this.SetWindingCount(lb);
                    if (this.IsContributing(lb))
                        op1 = this.AddOutPt(lb, lb.Bot);
                    this.InsertScanbeam(lb.Top.Y);
                }
                else
                {
                    this.InsertEdgeIntoAel(lb, null);
                    this.InsertEdgeIntoAel(rb, lb);
                    this.SetWindingCount(lb);
                    rb.WindCnt = lb.WindCnt;
                    rb.WindCnt2 = lb.WindCnt2;
                    if (this.IsContributing(lb))
                        op1 = this.AddLocalMinPoly(lb, rb, lb.Bot);
                    this.InsertScanbeam(lb.Top.Y);
                }

                if (rb != null)
                {
                    if (IsHorizontal(rb))
                    {
                        if (rb.NextInLml != null)
                            this.InsertScanbeam(rb.NextInLml.Top.Y);
                        this.AddEdgeToSel(rb);
                    }
                    else
                        this.InsertScanbeam(rb.Top.Y);
                }

                if (lb == null || rb == null)
                    continue;

                //if output polygons share an Edge with a horizontal rb, they'll need joining later ...
                if (op1 != null && IsHorizontal(rb) && this.mGhostJoins.Count > 0 && rb.WindDelta != 0)
                {
                    for (var i = 0; i < this.mGhostJoins.Count; i++)
                    {
                        //if the horizontal Rb and a 'ghost' horizontal overlap, then convert
                        //the 'ghost' join to a real join ready for later ...
                        var j = this.mGhostJoins[i];
                        if (this.HorzSegmentsOverlap(j.OutPt1.Pt.X, j.OffPt.X, rb.Bot.X, rb.Top.X))
                            this.AddJoin(j.OutPt1, op1, j.OffPt);
                    }
                }

                if (lb.OutIdx >= 0 && lb.PrevInAel != null && lb.PrevInAel.Curr.X == lb.Bot.X
                    && lb.PrevInAel.OutIdx >= 0
                    && SlopesEqual(lb.PrevInAel.Curr, lb.PrevInAel.Top, lb.Curr, lb.Top, this.MUseFullRange)
                    && lb.WindDelta != 0 && lb.PrevInAel.WindDelta != 0)
                {
                    var op2 = this.AddOutPt(lb.PrevInAel, lb.Bot);
                    this.AddJoin(op1, op2, lb.Top);
                }

                if (lb.NextInAel != rb)
                {
                    if (rb.OutIdx >= 0 && rb.PrevInAel.OutIdx >= 0
                        && SlopesEqual(rb.PrevInAel.Curr, rb.PrevInAel.Top, rb.Curr, rb.Top, this.MUseFullRange)
                        && rb.WindDelta != 0 && rb.PrevInAel.WindDelta != 0)
                    {
                        var op2 = this.AddOutPt(rb.PrevInAel, rb.Bot);
                        this.AddJoin(op1, op2, rb.Top);
                    }

                    var e = lb.NextInAel;
                    if (e != null)
                        while (e != rb)
                        {
                            //nb: For calculating winding counts etc, IntersectEdges() assumes
                            //that param1 will be to the right of param2 ABOVE the intersection ...
                            this.IntersectEdges(rb, e, lb.Curr); //order important here
                            e = e.NextInAel;
                        }
                }
            }
        }
        //------------------------------------------------------------------------------

        private void InsertMaxima(cInt x)
        {
            //double-linked list: sorted ascending, ignoring dups.
            var newMax = new Maxima();
            newMax.X = x;
            if (this.mMaxima == null)
            {
                this.mMaxima = newMax;
                this.mMaxima.Next = null;
                this.mMaxima.Prev = null;
            }
            else if (x < this.mMaxima.X)
            {
                newMax.Next = this.mMaxima;
                newMax.Prev = null;
                this.mMaxima = newMax;
            }
            else
            {
                var m = this.mMaxima;
                while (m.Next != null && x >= m.Next.X)
                    m = m.Next;
                if (x == m.X)
                    return; //ie ignores duplicates (& CG to clean up newMax)
                //insert newMax between m and m.Next ...
                newMax.Next = m.Next;
                newMax.Prev = m;
                if (m.Next != null)
                    m.Next.Prev = newMax;
                m.Next = newMax;
            }
        }
        //------------------------------------------------------------------------------

        private void IntersectEdges(Edge e1, Edge e2, IntPoint pt)
        {
            //e1 will be to the left of e2 BELOW the intersection. Therefore e1 is before
            //e2 in AEL except when e1 is being inserted at the intersection point ...

            var e1Contributing = e1.OutIdx >= 0;
            var e2Contributing = e2.OutIdx >= 0;

#if use_xyz
            this.SetZ(ref pt, e1, e2);
#endif

#if use_lines
            //if either edge is on an OPEN path ...
            if (e1.WindDelta == 0 || e2.WindDelta == 0)
            {
                //ignore subject-subject open path intersections UNLESS they
                //are both open paths, AND they are both 'contributing maximas' ...
                if (e1.WindDelta == 0 && e2.WindDelta == 0)
                    return;
                //if intersecting a subj line with a subj poly ...
                if (e1.PolyTyp == e2.PolyTyp && e1.WindDelta != e2.WindDelta
                    && this.mClipType == ClipType.CtUnion)
                {
                    if (e1.WindDelta == 0)
                    {
                        if (e2Contributing)
                        {
                            this.AddOutPt(e1, pt);
                            if (e1Contributing)
                                e1.OutIdx = Unassigned;
                        }
                    }
                    else
                    {
                        if (e1Contributing)
                        {
                            this.AddOutPt(e2, pt);
                            if (e2Contributing)
                                e2.OutIdx = Unassigned;
                        }
                    }
                }
                else if (e1.PolyTyp != e2.PolyTyp)
                {
                    if (e1.WindDelta == 0 && Math.Abs(e2.WindCnt) == 1
                        && (this.mClipType != ClipType.CtUnion || e2.WindCnt2 == 0))
                    {
                        this.AddOutPt(e1, pt);
                        if (e1Contributing)
                            e1.OutIdx = Unassigned;
                    }
                    else if (e2.WindDelta == 0 && Math.Abs(e1.WindCnt) == 1
                             && (this.mClipType != ClipType.CtUnion || e1.WindCnt2 == 0))
                    {
                        this.AddOutPt(e2, pt);
                        if (e2Contributing)
                            e2.OutIdx = Unassigned;
                    }
                }
                return;
            }
#endif

            //update winding counts...
            //assumes that e1 will be to the Right of e2 ABOVE the intersection
            if (e1.PolyTyp == e2.PolyTyp)
            {
                if (this.IsEvenOddFillType(e1))
                {
                    var oldE1WindCnt = e1.WindCnt;
                    e1.WindCnt = e2.WindCnt;
                    e2.WindCnt = oldE1WindCnt;
                }
                else
                {
                    if (e1.WindCnt + e2.WindDelta == 0)
                        e1.WindCnt = -e1.WindCnt;
                    else
                        e1.WindCnt += e2.WindDelta;
                    if (e2.WindCnt - e1.WindDelta == 0)
                        e2.WindCnt = -e2.WindCnt;
                    else
                        e2.WindCnt -= e1.WindDelta;
                }
            }
            else
            {
                if (!this.IsEvenOddFillType(e2))
                    e1.WindCnt2 += e2.WindDelta;
                else
                    e1.WindCnt2 = e1.WindCnt2 == 0 ? 1 : 0;
                if (!this.IsEvenOddFillType(e1))
                    e2.WindCnt2 -= e1.WindDelta;
                else
                    e2.WindCnt2 = e2.WindCnt2 == 0 ? 1 : 0;
            }

            PolyFillType e1FillType, e2FillType, e1FillType2, e2FillType2;
            if (e1.PolyTyp == PolyType.PtSubject)
            {
                e1FillType = this.mSubjFillType;
                e1FillType2 = this.mClipFillType;
            }
            else
            {
                e1FillType = this.mClipFillType;
                e1FillType2 = this.mSubjFillType;
            }
            if (e2.PolyTyp == PolyType.PtSubject)
            {
                e2FillType = this.mSubjFillType;
                e2FillType2 = this.mClipFillType;
            }
            else
            {
                e2FillType = this.mClipFillType;
                e2FillType2 = this.mSubjFillType;
            }

            int e1Wc, e2Wc;
            switch (e1FillType)
            {
                case PolyFillType.PftPositive:
                    e1Wc = e1.WindCnt;
                    break;
                case PolyFillType.PftNegative:
                    e1Wc = -e1.WindCnt;
                    break;
                default:
                    e1Wc = Math.Abs(e1.WindCnt);
                    break;
            }
            switch (e2FillType)
            {
                case PolyFillType.PftPositive:
                    e2Wc = e2.WindCnt;
                    break;
                case PolyFillType.PftNegative:
                    e2Wc = -e2.WindCnt;
                    break;
                default:
                    e2Wc = Math.Abs(e2.WindCnt);
                    break;
            }

            if (e1Contributing && e2Contributing)
            {
                if (e1Wc != 0 && e1Wc != 1 || e2Wc != 0 && e2Wc != 1
                    || e1.PolyTyp != e2.PolyTyp && this.mClipType != ClipType.CtXor)
                {
                    this.AddLocalMaxPoly(e1, e2, pt);
                }
                else
                {
                    this.AddOutPt(e1, pt);
                    this.AddOutPt(e2, pt);
                    SwapSides(e1, e2);
                    SwapPolyIndexes(e1, e2);
                }
            }
            else if (e1Contributing)
            {
                if (e2Wc == 0 || e2Wc == 1)
                {
                    this.AddOutPt(e1, pt);
                    SwapSides(e1, e2);
                    SwapPolyIndexes(e1, e2);
                }
            }
            else if (e2Contributing)
            {
                if (e1Wc == 0 || e1Wc == 1)
                {
                    this.AddOutPt(e2, pt);
                    SwapSides(e1, e2);
                    SwapPolyIndexes(e1, e2);
                }
            }
            else if ((e1Wc == 0 || e1Wc == 1) && (e2Wc == 0 || e2Wc == 1))
            {
                //neither edge is currently contributing ...
                cInt e1Wc2, e2Wc2;
                switch (e1FillType2)
                {
                    case PolyFillType.PftPositive:
                        e1Wc2 = e1.WindCnt2;
                        break;
                    case PolyFillType.PftNegative:
                        e1Wc2 = -e1.WindCnt2;
                        break;
                    default:
                        e1Wc2 = Math.Abs(e1.WindCnt2);
                        break;
                }
                switch (e2FillType2)
                {
                    case PolyFillType.PftPositive:
                        e2Wc2 = e2.WindCnt2;
                        break;
                    case PolyFillType.PftNegative:
                        e2Wc2 = -e2.WindCnt2;
                        break;
                    default:
                        e2Wc2 = Math.Abs(e2.WindCnt2);
                        break;
                }

                if (e1.PolyTyp != e2.PolyTyp)
                {
                    this.AddLocalMinPoly(e1, e2, pt);
                }
                else if (e1Wc == 1 && e2Wc == 1)
                    switch (this.mClipType)
                    {
                        case ClipType.CtIntersection:
                            if (e1Wc2 > 0 && e2Wc2 > 0)
                                this.AddLocalMinPoly(e1, e2, pt);
                            break;
                        case ClipType.CtUnion:
                            if (e1Wc2 <= 0 && e2Wc2 <= 0)
                                this.AddLocalMinPoly(e1, e2, pt);
                            break;
                        case ClipType.CtDifference:
                            if (e1.PolyTyp == PolyType.PtClip && e1Wc2 > 0 && e2Wc2 > 0
                                || e1.PolyTyp == PolyType.PtSubject && e1Wc2 <= 0 && e2Wc2 <= 0)
                                this.AddLocalMinPoly(e1, e2, pt);
                            break;
                        case ClipType.CtXor:
                            this.AddLocalMinPoly(e1, e2, pt);
                            break;
                    }
                else
                    SwapSides(e1, e2);
            }
        }
        //------------------------------------------------------------------------------

        private void IntersectPoint(Edge edge1, Edge edge2, out IntPoint ip)
        {
            ip = new IntPoint();
            double b1, b2;
            //nb: with very large coordinate values, it's possible for SlopesEqual() to 
            //return false but for the edge.Dx value be equal due to double precision rounding.
            if (edge1.Dx == edge2.Dx)
            {
                ip.Y = edge1.Curr.Y;
                ip.X = TopX(edge1, ip.Y);
                return;
            }

            if (edge1.Delta.X == 0)
            {
                ip.X = edge1.Bot.X;
                if (IsHorizontal(edge2))
                {
                    ip.Y = edge2.Bot.Y;
                }
                else
                {
                    b2 = edge2.Bot.Y - edge2.Bot.X / edge2.Dx;
                    ip.Y = Round(ip.X / edge2.Dx + b2);
                }
            }
            else if (edge2.Delta.X == 0)
            {
                ip.X = edge2.Bot.X;
                if (IsHorizontal(edge1))
                {
                    ip.Y = edge1.Bot.Y;
                }
                else
                {
                    b1 = edge1.Bot.Y - edge1.Bot.X / edge1.Dx;
                    ip.Y = Round(ip.X / edge1.Dx + b1);
                }
            }
            else
            {
                b1 = edge1.Bot.X - edge1.Bot.Y * edge1.Dx;
                b2 = edge2.Bot.X - edge2.Bot.Y * edge2.Dx;
                var q = (b2 - b1) / (edge1.Dx - edge2.Dx);
                ip.Y = Round(q);
                if (Math.Abs(edge1.Dx) < Math.Abs(edge2.Dx))
                    ip.X = Round(edge1.Dx * q + b1);
                else
                    ip.X = Round(edge2.Dx * q + b2);
            }

            if (ip.Y < edge1.Top.Y || ip.Y < edge2.Top.Y)
            {
                if (edge1.Top.Y > edge2.Top.Y)
                    ip.Y = edge1.Top.Y;
                else
                    ip.Y = edge2.Top.Y;
                if (Math.Abs(edge1.Dx) < Math.Abs(edge2.Dx))
                    ip.X = TopX(edge1, ip.Y);
                else
                    ip.X = TopX(edge2, ip.Y);
            }
            //finally, don't allow 'ip' to be BELOW curr.Y (ie bottom of scanbeam) ...
            if (ip.Y > edge1.Curr.Y)
            {
                ip.Y = edge1.Curr.Y;
                //better to use the more vertical edge to derive X ...
                if (Math.Abs(edge1.Dx) > Math.Abs(edge2.Dx))
                    ip.X = TopX(edge2, ip.Y);
                else
                    ip.X = TopX(edge1, ip.Y);
            }
        }
        //------------------------------------------------------------------------------

        private bool IsContributing(Edge edge)
        {
            PolyFillType pft, pft2;
            if (edge.PolyTyp == PolyType.PtSubject)
            {
                pft = this.mSubjFillType;
                pft2 = this.mClipFillType;
            }
            else
            {
                pft = this.mClipFillType;
                pft2 = this.mSubjFillType;
            }

            switch (pft)
            {
                case PolyFillType.PftEvenOdd:
                    //return false if a subj line has been flagged as inside a subj polygon
                    if (edge.WindDelta == 0 && edge.WindCnt != 1)
                        return false;
                    break;
                case PolyFillType.PftNonZero:
                    if (Math.Abs(edge.WindCnt) != 1)
                        return false;
                    break;
                case PolyFillType.PftPositive:
                    if (edge.WindCnt != 1)
                        return false;
                    break;
                default: //PolyFillType.pftNegative
                    if (edge.WindCnt != -1)
                        return false;
                    break;
            }

            switch (this.mClipType)
            {
                case ClipType.CtIntersection:
                    switch (pft2)
                    {
                        case PolyFillType.PftEvenOdd:
                        case PolyFillType.PftNonZero: return edge.WindCnt2 != 0;
                        case PolyFillType.PftPositive: return edge.WindCnt2 > 0;
                        default: return edge.WindCnt2 < 0;
                    }
                case ClipType.CtUnion:
                    switch (pft2)
                    {
                        case PolyFillType.PftEvenOdd:
                        case PolyFillType.PftNonZero: return edge.WindCnt2 == 0;
                        case PolyFillType.PftPositive: return edge.WindCnt2 <= 0;
                        default: return edge.WindCnt2 >= 0;
                    }
                case ClipType.CtDifference:
                    if (edge.PolyTyp == PolyType.PtSubject)
                        switch (pft2)
                        {
                            case PolyFillType.PftEvenOdd:
                            case PolyFillType.PftNonZero: return edge.WindCnt2 == 0;
                            case PolyFillType.PftPositive: return edge.WindCnt2 <= 0;
                            default: return edge.WindCnt2 >= 0;
                        }
                    else
                        switch (pft2)
                        {
                            case PolyFillType.PftEvenOdd:
                            case PolyFillType.PftNonZero: return edge.WindCnt2 != 0;
                            case PolyFillType.PftPositive: return edge.WindCnt2 > 0;
                            default: return edge.WindCnt2 < 0;
                        }
                case ClipType.CtXor:
                    if (edge.WindDelta == 0) //XOr always contributing unless open
                        switch (pft2)
                        {
                            case PolyFillType.PftEvenOdd:
                            case PolyFillType.PftNonZero: return edge.WindCnt2 == 0;
                            case PolyFillType.PftPositive: return edge.WindCnt2 <= 0;
                            default: return edge.WindCnt2 >= 0;
                        }
                    else
                        return true;
            }
            return true;
        }
        //------------------------------------------------------------------------------

        private bool IsEvenOddAltFillType(Edge edge)
        {
            if (edge.PolyTyp == PolyType.PtSubject)
                return this.mClipFillType == PolyFillType.PftEvenOdd;
            return this.mSubjFillType == PolyFillType.PftEvenOdd;
        }
        //------------------------------------------------------------------------------

        private bool IsEvenOddFillType(Edge edge)
        {
            if (edge.PolyTyp == PolyType.PtSubject)
                return this.mSubjFillType == PolyFillType.PftEvenOdd;
            return this.mClipFillType == PolyFillType.PftEvenOdd;
        }
        //------------------------------------------------------------------------------

        private bool IsIntermediate(Edge e, double y)
        {
            return e.Top.Y == y && e.NextInLml != null;
        }
        //------------------------------------------------------------------------------

        private bool IsMaxima(Edge e, double y)
        {
            return e != null && e.Top.Y == y && e.NextInLml == null;
        }
        //------------------------------------------------------------------------------

        private bool IsMinima(Edge e)
        {
            return e != null && e.Prev.NextInLml != e && e.Next.NextInLml != e;
        }
        //------------------------------------------------------------------------------

        private void JoinCommonEdges()
        {
            for (var i = 0; i < this.mJoins.Count; i++)
            {
                var join = this.mJoins[i];

                var outRec1 = this.GetOutRec(join.OutPt1.Idx);
                var outRec2 = this.GetOutRec(join.OutPt2.Idx);

                if (outRec1.Pts == null || outRec2.Pts == null)
                    continue;
                if (outRec1.IsOpen || outRec2.IsOpen)
                    continue;

                //get the polygon fragment with the correct hole state (FirstLeft)
                //before calling JoinPoints() ...
                OutRec holeStateRec;
                if (outRec1 == outRec2)
                    holeStateRec = outRec1;
                else if (this.OutRec1RightOfOutRec2(outRec1, outRec2))
                    holeStateRec = outRec2;
                else if (this.OutRec1RightOfOutRec2(outRec2, outRec1))
                    holeStateRec = outRec1;
                else
                    holeStateRec = this.GetLowermostRec(outRec1, outRec2);

                if (!this.JoinPoints(join, outRec1, outRec2))
                    continue;

                if (outRec1 == outRec2)
                {
                    //instead of joining two polygons, we've just created a new one by
                    //splitting one polygon into two.
                    outRec1.Pts = join.OutPt1;
                    outRec1.BottomPt = null;
                    outRec2 = this.CreateOutRec();
                    outRec2.Pts = join.OutPt2;

                    //update all OutRec2.Pts Idx's ...
                    this.UpdateOutPtIdxs(outRec2);

                    if (Poly2ContainsPoly1(outRec2.Pts, outRec1.Pts))
                    {
                        //outRec1 contains outRec2 ...
                        outRec2.IsHole = !outRec1.IsHole;
                        outRec2.FirstLeft = outRec1;

                        if (this.mUsingPolyTree)
                            this.FixupFirstLefts2(outRec2, outRec1);

                        if ((outRec2.IsHole ^ this.ReverseSolution) == this.Area(outRec2) > 0)
                            this.ReversePolyPtLinks(outRec2.Pts);
                    }
                    else if (Poly2ContainsPoly1(outRec1.Pts, outRec2.Pts))
                    {
                        //outRec2 contains outRec1 ...
                        outRec2.IsHole = outRec1.IsHole;
                        outRec1.IsHole = !outRec2.IsHole;
                        outRec2.FirstLeft = outRec1.FirstLeft;
                        outRec1.FirstLeft = outRec2;

                        if (this.mUsingPolyTree)
                            this.FixupFirstLefts2(outRec1, outRec2);

                        if ((outRec1.IsHole ^ this.ReverseSolution) == this.Area(outRec1) > 0)
                            this.ReversePolyPtLinks(outRec1.Pts);
                    }
                    else
                    {
                        //the 2 polygons are completely separate ...
                        outRec2.IsHole = outRec1.IsHole;
                        outRec2.FirstLeft = outRec1.FirstLeft;

                        //fixup FirstLeft pointers that may need reassigning to OutRec2
                        if (this.mUsingPolyTree)
                            this.FixupFirstLefts1(outRec1, outRec2);
                    }
                }
                else
                {
                    //joined 2 polygons together ...

                    outRec2.Pts = null;
                    outRec2.BottomPt = null;
                    outRec2.Idx = outRec1.Idx;

                    outRec1.IsHole = holeStateRec.IsHole;
                    if (holeStateRec == outRec2)
                        outRec1.FirstLeft = outRec2.FirstLeft;
                    outRec2.FirstLeft = outRec1;

                    //fixup FirstLeft pointers that may need reassigning to OutRec1
                    if (this.mUsingPolyTree)
                        this.FixupFirstLefts3(outRec2, outRec1);
                }
            }
        }
        //------------------------------------------------------------------------------

        bool JoinHorz(OutPt op1, OutPt op1B, OutPt op2, OutPt op2B, IntPoint pt, bool discardLeft)
        {
            var dir1 = op1.Pt.X > op1B.Pt.X ? Direction.DRightToLeft : Direction.DLeftToRight;
            var dir2 = op2.Pt.X > op2B.Pt.X ? Direction.DRightToLeft : Direction.DLeftToRight;
            if (dir1 == dir2)
                return false;

            //When DiscardLeft, we want Op1b to be on the Left of Op1, otherwise we
            //want Op1b to be on the Right. (And likewise with Op2 and Op2b.)
            //So, to facilitate this while inserting Op1b and Op2b ...
            //when DiscardLeft, make sure we're AT or RIGHT of Pt before adding Op1b,
            //otherwise make sure we're AT or LEFT of Pt. (Likewise with Op2b.)
            if (dir1 == Direction.DLeftToRight)
            {
                while (op1.Next.Pt.X <= pt.X && op1.Next.Pt.X >= op1.Pt.X && op1.Next.Pt.Y == pt.Y)
                    op1 = op1.Next;
                if (discardLeft && op1.Pt.X != pt.X)
                    op1 = op1.Next;
                op1B = this.DupOutPt(op1, !discardLeft);
                if (op1B.Pt != pt)
                {
                    op1 = op1B;
                    op1.Pt = pt;
                    op1B = this.DupOutPt(op1, !discardLeft);
                }
            }
            else
            {
                while (op1.Next.Pt.X >= pt.X && op1.Next.Pt.X <= op1.Pt.X && op1.Next.Pt.Y == pt.Y)
                    op1 = op1.Next;
                if (!discardLeft && op1.Pt.X != pt.X)
                    op1 = op1.Next;
                op1B = this.DupOutPt(op1, discardLeft);
                if (op1B.Pt != pt)
                {
                    op1 = op1B;
                    op1.Pt = pt;
                    op1B = this.DupOutPt(op1, discardLeft);
                }
            }

            if (dir2 == Direction.DLeftToRight)
            {
                while (op2.Next.Pt.X <= pt.X && op2.Next.Pt.X >= op2.Pt.X && op2.Next.Pt.Y == pt.Y)
                    op2 = op2.Next;
                if (discardLeft && op2.Pt.X != pt.X)
                    op2 = op2.Next;
                op2B = this.DupOutPt(op2, !discardLeft);
                if (op2B.Pt != pt)
                {
                    op2 = op2B;
                    op2.Pt = pt;
                    op2B = this.DupOutPt(op2, !discardLeft);
                }
                ;
            }
            else
            {
                while (op2.Next.Pt.X >= pt.X && op2.Next.Pt.X <= op2.Pt.X && op2.Next.Pt.Y == pt.Y)
                    op2 = op2.Next;
                if (!discardLeft && op2.Pt.X != pt.X)
                    op2 = op2.Next;
                op2B = this.DupOutPt(op2, discardLeft);
                if (op2B.Pt != pt)
                {
                    op2 = op2B;
                    op2.Pt = pt;
                    op2B = this.DupOutPt(op2, discardLeft);
                }
                ;
            }
            ;

            if (dir1 == Direction.DLeftToRight == discardLeft)
            {
                op1.Prev = op2;
                op2.Next = op1;
                op1B.Next = op2B;
                op2B.Prev = op1B;
            }
            else
            {
                op1.Next = op2;
                op2.Prev = op1;
                op1B.Prev = op2B;
                op2B.Next = op1B;
            }
            return true;
        }
        //------------------------------------------------------------------------------

        private bool JoinPoints(Join j, OutRec outRec1, OutRec outRec2)
        {
            OutPt op1 = j.OutPt1, op1B;
            OutPt op2 = j.OutPt2, op2B;

            //There are 3 kinds of joins for output polygons ...
            //1. Horizontal joins where Join.OutPt1 & Join.OutPt2 are vertices anywhere
            //along (horizontal) collinear edges (& Join.OffPt is on the same horizontal).
            //2. Non-horizontal joins where Join.OutPt1 & Join.OutPt2 are at the same
            //location at the Bottom of the overlapping segment (& Join.OffPt is above).
            //3. StrictlySimple joins where edges touch but are not collinear and where
            //Join.OutPt1, Join.OutPt2 & Join.OffPt all share the same point.
            var isHorizontal = j.OutPt1.Pt.Y == j.OffPt.Y;

            if (isHorizontal && j.OffPt == j.OutPt1.Pt && j.OffPt == j.OutPt2.Pt)
            {
                //Strictly Simple join ...
                if (outRec1 != outRec2)
                    return false;
                op1B = j.OutPt1.Next;
                while (op1B != op1 && op1B.Pt == j.OffPt)
                    op1B = op1B.Next;
                var reverse1 = op1B.Pt.Y > j.OffPt.Y;
                op2B = j.OutPt2.Next;
                while (op2B != op2 && op2B.Pt == j.OffPt)
                    op2B = op2B.Next;
                var reverse2 = op2B.Pt.Y > j.OffPt.Y;
                if (reverse1 == reverse2)
                    return false;
                if (reverse1)
                {
                    op1B = this.DupOutPt(op1, false);
                    op2B = this.DupOutPt(op2, true);
                    op1.Prev = op2;
                    op2.Next = op1;
                    op1B.Next = op2B;
                    op2B.Prev = op1B;
                    j.OutPt1 = op1;
                    j.OutPt2 = op1B;
                    return true;
                }
                op1B = this.DupOutPt(op1, true);
                op2B = this.DupOutPt(op2, false);
                op1.Next = op2;
                op2.Prev = op1;
                op1B.Prev = op2B;
                op2B.Next = op1B;
                j.OutPt1 = op1;
                j.OutPt2 = op1B;
                return true;
            }
            if (isHorizontal)
            {
                //treat horizontal joins differently to non-horizontal joins since with
                //them we're not yet sure where the overlapping is. OutPt1.Pt & OutPt2.Pt
                //may be anywhere along the horizontal edge.
                op1B = op1;
                while (op1.Prev.Pt.Y == op1.Pt.Y && op1.Prev != op1B && op1.Prev != op2)
                    op1 = op1.Prev;
                while (op1B.Next.Pt.Y == op1B.Pt.Y && op1B.Next != op1 && op1B.Next != op2)
                    op1B = op1B.Next;
                if (op1B.Next == op1 || op1B.Next == op2)
                    return false; //a flat 'polygon'

                op2B = op2;
                while (op2.Prev.Pt.Y == op2.Pt.Y && op2.Prev != op2B && op2.Prev != op1B)
                    op2 = op2.Prev;
                while (op2B.Next.Pt.Y == op2B.Pt.Y && op2B.Next != op2 && op2B.Next != op1)
                    op2B = op2B.Next;
                if (op2B.Next == op2 || op2B.Next == op1)
                    return false; //a flat 'polygon'
                //Op1 -. Op1b & Op2 -. Op2b are the extremites of the horizontal edges
                if (!this.GetOverlap(op1.Pt.X, op1B.Pt.X, op2.Pt.X, op2B.Pt.X, out long left, out long right))
                    return false;

                //DiscardLeftSide: when overlapping edges are joined, a spike will created
                //which needs to be cleaned up. However, we don't want Op1 or Op2 caught up
                //on the discard Side as either may still be needed for other joins ...
                IntPoint pt;
                bool discardLeftSide;
                if (op1.Pt.X >= left && op1.Pt.X <= right)
                {
                    pt = op1.Pt;
                    discardLeftSide = op1.Pt.X > op1B.Pt.X;
                }
                else if (op2.Pt.X >= left && op2.Pt.X <= right)
                {
                    pt = op2.Pt;
                    discardLeftSide = op2.Pt.X > op2B.Pt.X;
                }
                else if (op1B.Pt.X >= left && op1B.Pt.X <= right)
                {
                    pt = op1B.Pt;
                    discardLeftSide = op1B.Pt.X > op1.Pt.X;
                }
                else
                {
                    pt = op2B.Pt;
                    discardLeftSide = op2B.Pt.X > op2.Pt.X;
                }
                j.OutPt1 = op1;
                j.OutPt2 = op2;
                return this.JoinHorz(op1, op1B, op2, op2B, pt, discardLeftSide);
            }
            //nb: For non-horizontal joins ...
            //    1. Jr.OutPt1.Pt.Y == Jr.OutPt2.Pt.Y
            //    2. Jr.OutPt1.Pt > Jr.OffPt.Y

            //make sure the polygons are correctly oriented ...
            op1B = op1.Next;
            while (op1B.Pt == op1.Pt && op1B != op1)
                op1B = op1B.Next;
            var Reverse1 = op1B.Pt.Y > op1.Pt.Y || !SlopesEqual(op1.Pt, op1B.Pt, j.OffPt, this.MUseFullRange);
            if (Reverse1)
            {
                op1B = op1.Prev;
                while (op1B.Pt == op1.Pt && op1B != op1)
                    op1B = op1B.Prev;
                if (op1B.Pt.Y > op1.Pt.Y || !SlopesEqual(op1.Pt, op1B.Pt, j.OffPt, this.MUseFullRange))
                    return false;
            }
            ;
            op2B = op2.Next;
            while (op2B.Pt == op2.Pt && op2B != op2)
                op2B = op2B.Next;
            var Reverse2 = op2B.Pt.Y > op2.Pt.Y || !SlopesEqual(op2.Pt, op2B.Pt, j.OffPt, this.MUseFullRange);
            if (Reverse2)
            {
                op2B = op2.Prev;
                while (op2B.Pt == op2.Pt && op2B != op2)
                    op2B = op2B.Prev;
                if (op2B.Pt.Y > op2.Pt.Y || !SlopesEqual(op2.Pt, op2B.Pt, j.OffPt, this.MUseFullRange))
                    return false;
            }

            if (op1B == op1 || op2B == op2 || op1B == op2B || outRec1 == outRec2 && Reverse1 == Reverse2)
                return false;

            if (Reverse1)
            {
                op1B = this.DupOutPt(op1, false);
                op2B = this.DupOutPt(op2, true);
                op1.Prev = op2;
                op2.Next = op1;
                op1B.Next = op2B;
                op2B.Prev = op1B;
                j.OutPt1 = op1;
                j.OutPt2 = op1B;
                return true;
            }
            op1B = this.DupOutPt(op1, true);
            op2B = this.DupOutPt(op2, false);
            op1.Next = op2;
            op2.Prev = op1;
            op1B.Prev = op2B;
            op2B.Next = op1B;
            j.OutPt1 = op1;
            j.OutPt2 = op1B;
            return true;
        }
        //------------------------------------------------------------------------------

        bool OutRec1RightOfOutRec2(OutRec outRec1, OutRec outRec2)
        {
            do
            {
                outRec1 = outRec1.FirstLeft;
                if (outRec1 == outRec2)
                    return true;
            }
            while (outRec1 != null);
            return false;
        }
        //------------------------------------------------------------------------------

        private int PointCount(OutPt pts)
        {
            if (pts == null)
                return 0;
            var result = 0;
            var p = pts;
            do
            {
                result++;
                p = p.Next;
            }
            while (p != pts);
            return result;
        }
        //------------------------------------------------------------------------------

        private void ProcessEdgesAtTopOfScanbeam(cInt topY)
        {
            var e = this.MActiveEdges;
            while (e != null)
            {
                //1. process maxima, treating them as if they're 'bent' horizontal edges,
                //   but exclude maxima with horizontal edges. nb: e can't be a horizontal.
                var isMaximaEdge = this.IsMaxima(e, topY);

                if (isMaximaEdge)
                {
                    var eMaxPair = this.GetMaximaPairEx(e);
                    isMaximaEdge = eMaxPair == null || !IsHorizontal(eMaxPair);
                }

                if (isMaximaEdge)
                {
                    if (this.StrictlySimple)
                        this.InsertMaxima(e.Top.X);
                    var ePrev = e.PrevInAel;
                    this.DoMaxima(e);
                    if (ePrev == null)
                        e = this.MActiveEdges;
                    else
                        e = ePrev.NextInAel;
                }
                else
                {
                    //2. promote horizontal edges, otherwise update Curr.X and Curr.Y ...
                    if (this.IsIntermediate(e, topY) && IsHorizontal(e.NextInLml))
                    {
                        this.UpdateEdgeIntoAel(ref e);
                        if (e.OutIdx >= 0)
                            this.AddOutPt(e, e.Bot);
                        this.AddEdgeToSel(e);
                    }
                    else
                    {
                        e.Curr.X = TopX(e, topY);
                        e.Curr.Y = topY;
#if use_xyz
                        if (e.Top.Y == topY)
                            e.Curr.Z = e.Top.Z;
                        else if (e.Bot.Y == topY)
                            e.Curr.Z = e.Bot.Z;
                        else
                            e.Curr.Z = 0;
#endif
                    }
                    //When StrictlySimple and 'e' is being touched by another edge, then
                    //make sure both edges have a vertex here ...
                    if (this.StrictlySimple)
                    {
                        var ePrev = e.PrevInAel;
                        if (e.OutIdx >= 0 && e.WindDelta != 0 && ePrev != null && ePrev.OutIdx >= 0
                            && ePrev.Curr.X == e.Curr.X && ePrev.WindDelta != 0)
                        {
                            var ip = new IntPoint(e.Curr);
#if use_xyz
                            this.SetZ(ref ip, ePrev, e);
#endif
                            var op = this.AddOutPt(ePrev, ip);
                            var op2 = this.AddOutPt(e, ip);
                            this.AddJoin(op, op2, ip); //StrictlySimple (type-3) join
                        }
                    }

                    e = e.NextInAel;
                }
            }

            //3. Process horizontals at the Top of the scanbeam ...
            this.ProcessHorizontals();
            this.mMaxima = null;

            //4. Promote intermediate vertices ...
            e = this.MActiveEdges;
            while (e != null)
            {
                if (this.IsIntermediate(e, topY))
                {
                    OutPt op = null;
                    if (e.OutIdx >= 0)
                        op = this.AddOutPt(e, e.Top);
                    this.UpdateEdgeIntoAel(ref e);

                    //if output polygons share an edge, they'll need joining later ...
                    var ePrev = e.PrevInAel;
                    var eNext = e.NextInAel;
                    if (ePrev != null && ePrev.Curr.X == e.Bot.X && ePrev.Curr.Y == e.Bot.Y && op != null
                        && ePrev.OutIdx >= 0 && ePrev.Curr.Y > ePrev.Top.Y
                        && SlopesEqual(e.Curr, e.Top, ePrev.Curr, ePrev.Top, this.MUseFullRange) && e.WindDelta != 0
                        && ePrev.WindDelta != 0)
                    {
                        var op2 = this.AddOutPt(ePrev, e.Bot);
                        this.AddJoin(op, op2, e.Top);
                    }
                    else if (eNext != null && eNext.Curr.X == e.Bot.X && eNext.Curr.Y == e.Bot.Y && op != null
                        && eNext.OutIdx >= 0 && eNext.Curr.Y > eNext.Top.Y
                        && SlopesEqual(e.Curr, e.Top, eNext.Curr, eNext.Top, this.MUseFullRange) && e.WindDelta != 0
                        && eNext.WindDelta != 0)
                    {
                        var op2 = this.AddOutPt(eNext, e.Bot);
                        this.AddJoin(op, op2, e.Top);
                    }
                }
                e = e.NextInAel;
            }
        }
        //------------------------------------------------------------------------

        private void ProcessHorizontal(Edge horzEdge)
        {
            var isOpen = horzEdge.WindDelta == 0;

            this.GetHorzDirection(horzEdge, out Direction dir, out long horzLeft, out long horzRight);

            Edge eLastHorz = horzEdge, eMaxPair = null;
            while (eLastHorz.NextInLml != null && IsHorizontal(eLastHorz.NextInLml))
                eLastHorz = eLastHorz.NextInLml;
            if (eLastHorz.NextInLml == null)
                eMaxPair = this.GetMaximaPair(eLastHorz);

            var currMax = this.mMaxima;
            if (currMax != null)
            {
                //get the first maxima in range (X) ...
                if (dir == Direction.DLeftToRight)
                {
                    while (currMax != null && currMax.X <= horzEdge.Bot.X)
                        currMax = currMax.Next;
                    if (currMax != null && currMax.X >= eLastHorz.Top.X)
                        currMax = null;
                }
                else
                {
                    while (currMax.Next != null && currMax.Next.X < horzEdge.Bot.X)
                        currMax = currMax.Next;
                    if (currMax.X <= eLastHorz.Top.X)
                        currMax = null;
                }
            }

            OutPt op1 = null;
            for (;;) //loop through consec. horizontal edges
            {
                var isLastHorz = horzEdge == eLastHorz;
                var e = this.GetNextInAel(horzEdge, dir);
                while (e != null)
                {
                    //this code block inserts extra coords into horizontal edges (in output
                    //polygons) whereever maxima touch these horizontal edges. This helps
                    //'simplifying' polygons (ie if the Simplify property is set).
                    if (currMax != null)
                    {
                        if (dir == Direction.DLeftToRight)
                        {
                            while (currMax != null && currMax.X < e.Curr.X)
                            {
                                if (horzEdge.OutIdx >= 0 && !isOpen)
                                    this.AddOutPt(horzEdge, new IntPoint(currMax.X, horzEdge.Bot.Y));
                                currMax = currMax.Next;
                            }
                        }
                        else
                        {
                            while (currMax != null && currMax.X > e.Curr.X)
                            {
                                if (horzEdge.OutIdx >= 0 && !isOpen)
                                    this.AddOutPt(horzEdge, new IntPoint(currMax.X, horzEdge.Bot.Y));
                                currMax = currMax.Prev;
                            }
                        }
                    }
                    ;

                    if (dir == Direction.DLeftToRight && e.Curr.X > horzRight
                        || dir == Direction.DRightToLeft && e.Curr.X < horzLeft)
                        break;

                    //Also break if we've got to the end of an intermediate horizontal edge ...
                    //nb: Smaller Dx's are to the right of larger Dx's ABOVE the horizontal.
                    if (e.Curr.X == horzEdge.Top.X && horzEdge.NextInLml != null && e.Dx < horzEdge.NextInLml.Dx)
                        break;

                    if (horzEdge.OutIdx >= 0 && !isOpen) //note: may be done multiple times
                    {
#if use_xyz
                        if (dir == Direction.DLeftToRight)
                            this.SetZ(ref e.Curr, horzEdge, e);
                        else
                            this.SetZ(ref e.Curr, e, horzEdge);
#endif

                        op1 = this.AddOutPt(horzEdge, e.Curr);
                        var eNextHorz = this.mSortedEdges;
                        while (eNextHorz != null)
                        {
                            if (eNextHorz.OutIdx >= 0 && this.HorzSegmentsOverlap(
                                horzEdge.Bot.X,
                                horzEdge.Top.X,
                                eNextHorz.Bot.X,
                                eNextHorz.Top.X))
                            {
                                var op2 = this.GetLastOutPt(eNextHorz);
                                this.AddJoin(op2, op1, eNextHorz.Top);
                            }
                            eNextHorz = eNextHorz.NextInSel;
                        }
                        this.AddGhostJoin(op1, horzEdge.Bot);
                    }

                    //OK, so far we're still in range of the horizontal Edge  but make sure
                    //we're at the last of consec. horizontals when matching with eMaxPair
                    if (e == eMaxPair && isLastHorz)
                    {
                        if (horzEdge.OutIdx >= 0)
                            this.AddLocalMaxPoly(horzEdge, eMaxPair, horzEdge.Top);
                        this.DeleteFromAel(horzEdge);
                        this.DeleteFromAel(eMaxPair);
                        return;
                    }

                    if (dir == Direction.DLeftToRight)
                    {
                        var pt = new IntPoint(e.Curr.X, horzEdge.Curr.Y);
                        this.IntersectEdges(horzEdge, e, pt);
                    }
                    else
                    {
                        var pt = new IntPoint(e.Curr.X, horzEdge.Curr.Y);
                        this.IntersectEdges(e, horzEdge, pt);
                    }
                    var eNext = this.GetNextInAel(e, dir);
                    this.SwapPositionsInAel(horzEdge, e);
                    e = eNext;
                } //end while(e != null)

                //Break out of loop if HorzEdge.NextInLML is not also horizontal ...
                if (horzEdge.NextInLml == null || !IsHorizontal(horzEdge.NextInLml))
                    break;

                this.UpdateEdgeIntoAel(ref horzEdge);
                if (horzEdge.OutIdx >= 0)
                    this.AddOutPt(horzEdge, horzEdge.Bot);
                this.GetHorzDirection(horzEdge, out dir, out horzLeft, out horzRight);
            } //end for (;;)

            if (horzEdge.OutIdx >= 0 && op1 == null)
            {
                op1 = this.GetLastOutPt(horzEdge);
                var eNextHorz = this.mSortedEdges;
                while (eNextHorz != null)
                {
                    if (eNextHorz.OutIdx >= 0 && this.HorzSegmentsOverlap(
                        horzEdge.Bot.X,
                        horzEdge.Top.X,
                        eNextHorz.Bot.X,
                        eNextHorz.Top.X))
                    {
                        var op2 = this.GetLastOutPt(eNextHorz);
                        this.AddJoin(op2, op1, eNextHorz.Top);
                    }
                    eNextHorz = eNextHorz.NextInSel;
                }
                this.AddGhostJoin(op1, horzEdge.Top);
            }

            if (horzEdge.NextInLml != null)
            {
                if (horzEdge.OutIdx >= 0)
                {
                    op1 = this.AddOutPt(horzEdge, horzEdge.Top);

                    this.UpdateEdgeIntoAel(ref horzEdge);
                    if (horzEdge.WindDelta == 0)
                        return;
                    //nb: HorzEdge is no longer horizontal here
                    var ePrev = horzEdge.PrevInAel;
                    var eNext = horzEdge.NextInAel;
                    if (ePrev != null && ePrev.Curr.X == horzEdge.Bot.X && ePrev.Curr.Y == horzEdge.Bot.Y
                        && ePrev.WindDelta != 0 && ePrev.OutIdx >= 0 && ePrev.Curr.Y > ePrev.Top.Y
                        && SlopesEqual(horzEdge, ePrev, this.MUseFullRange))
                    {
                        var op2 = this.AddOutPt(ePrev, horzEdge.Bot);
                        this.AddJoin(op1, op2, horzEdge.Top);
                    }
                    else if (eNext != null && eNext.Curr.X == horzEdge.Bot.X && eNext.Curr.Y == horzEdge.Bot.Y
                        && eNext.WindDelta != 0 && eNext.OutIdx >= 0 && eNext.Curr.Y > eNext.Top.Y
                        && SlopesEqual(horzEdge, eNext, this.MUseFullRange))
                    {
                        var op2 = this.AddOutPt(eNext, horzEdge.Bot);
                        this.AddJoin(op1, op2, horzEdge.Top);
                    }
                }
                else
                    this.UpdateEdgeIntoAel(ref horzEdge);
            }
            else
            {
                if (horzEdge.OutIdx >= 0)
                    this.AddOutPt(horzEdge, horzEdge.Top);
                this.DeleteFromAel(horzEdge);
            }
        }
        //------------------------------------------------------------------------------

        private void ProcessHorizontals()
        {
            //m_SortedEdges;
            while (this.PopEdgeFromSel(out Edge horzEdge))
                this.ProcessHorizontal(horzEdge);
        }
        //------------------------------------------------------------------------------

        private bool ProcessIntersections(cInt topY)
        {
            if (this.MActiveEdges == null)
                return true;
            try
            {
                this.BuildIntersectList(topY);
                if (this.mIntersectList.Count == 0)
                    return true;
                if (this.mIntersectList.Count == 1 || this.FixupIntersectionOrder())
                    this.ProcessIntersectList();
                else
                    return false;
            }
            catch
            {
                this.mSortedEdges = null;
                this.mIntersectList.Clear();
                throw new ClipperException("ProcessIntersections error");
            }
            this.mSortedEdges = null;
            return true;
        }
        //------------------------------------------------------------------------------

        private void ProcessIntersectList()
        {
            for (var i = 0; i < this.mIntersectList.Count; i++)
            {
                var iNode = this.mIntersectList[i];
                {
                    this.IntersectEdges(iNode.Edge1, iNode.Edge2, iNode.Pt);
                    this.SwapPositionsInAel(iNode.Edge1, iNode.Edge2);
                }
            }
            this.mIntersectList.Clear();
        }
        //------------------------------------------------------------------------------

        private void ReversePolyPtLinks(OutPt pp)
        {
            if (pp == null)
                return;
            OutPt pp1;
            OutPt pp2;
            pp1 = pp;
            do
            {
                pp2 = pp1.Next;
                pp1.Next = pp1.Prev;
                pp1.Prev = pp2;
                pp1 = pp2;
            }
            while (pp1 != pp);
        }
        //------------------------------------------------------------------------------

        private void SetHoleState(Edge e, OutRec outRec)
        {
            var e2 = e.PrevInAel;
            Edge eTmp = null;
            while (e2 != null)
            {
                if (e2.OutIdx >= 0 && e2.WindDelta != 0)
                {
                    if (eTmp == null)
                        eTmp = e2;
                    else if (eTmp.OutIdx == e2.OutIdx)
                        eTmp = null; //paired               
                }
                e2 = e2.PrevInAel;
            }

            if (eTmp == null)
            {
                outRec.FirstLeft = null;
                outRec.IsHole = false;
            }
            else
            {
                outRec.FirstLeft = this.MPolyOuts[eTmp.OutIdx];
                outRec.IsHole = !outRec.FirstLeft.IsHole;
            }
        }
        //------------------------------------------------------------------------------

        private void SetWindingCount(Edge edge)
        {
            var e = edge.PrevInAel;
            //find the edge of the same polytype that immediately preceeds 'edge' in AEL
            while (e != null && (e.PolyTyp != edge.PolyTyp || e.WindDelta == 0))
                e = e.PrevInAel;
            if (e == null)
            {
                PolyFillType pft;
                pft = edge.PolyTyp == PolyType.PtSubject ? this.mSubjFillType : this.mClipFillType;
                if (edge.WindDelta == 0)
                    edge.WindCnt = pft == PolyFillType.PftNegative ? -1 : 1;
                else
                    edge.WindCnt = edge.WindDelta;
                edge.WindCnt2 = 0;
                e = this.MActiveEdges; //ie get ready to calc WindCnt2
            }
            else if (edge.WindDelta == 0 && this.mClipType != ClipType.CtUnion)
            {
                edge.WindCnt = 1;
                edge.WindCnt2 = e.WindCnt2;
                e = e.NextInAel; //ie get ready to calc WindCnt2
            }
            else if (this.IsEvenOddFillType(edge))
            {
                //EvenOdd filling ...
                if (edge.WindDelta == 0)
                {
                    //are we inside a subj polygon ...
                    var inside = true;
                    var e2 = e.PrevInAel;
                    while (e2 != null)
                    {
                        if (e2.PolyTyp == e.PolyTyp && e2.WindDelta != 0)
                            inside = !inside;
                        e2 = e2.PrevInAel;
                    }
                    edge.WindCnt = inside ? 0 : 1;
                }
                else
                {
                    edge.WindCnt = edge.WindDelta;
                }
                edge.WindCnt2 = e.WindCnt2;
                e = e.NextInAel; //ie get ready to calc WindCnt2
            }
            else
            {
                //nonZero, Positive or Negative filling ...
                if (e.WindCnt * e.WindDelta < 0)
                {
                    //prev edge is 'decreasing' WindCount (WC) toward zero
                    //so we're outside the previous polygon ...
                    if (Math.Abs(e.WindCnt) > 1)
                    {
                        //outside prev poly but still inside another.
                        //when reversing direction of prev poly use the same WC 
                        if (e.WindDelta * edge.WindDelta < 0)
                            edge.WindCnt = e.WindCnt;
                        //otherwise continue to 'decrease' WC ...
                        else
                            edge.WindCnt = e.WindCnt + edge.WindDelta;
                    }
                    else
                        //now outside all polys of same polytype so set own WC ...
                        edge.WindCnt = edge.WindDelta == 0 ? 1 : edge.WindDelta;
                }
                else
                {
                    //prev edge is 'increasing' WindCount (WC) away from zero
                    //so we're inside the previous polygon ...
                    if (edge.WindDelta == 0)
                        edge.WindCnt = e.WindCnt < 0 ? e.WindCnt - 1 : e.WindCnt + 1;
                    //if wind direction is reversing prev then use same WC
                    else if (e.WindDelta * edge.WindDelta < 0)
                        edge.WindCnt = e.WindCnt;
                    //otherwise add to WC ...
                    else
                        edge.WindCnt = e.WindCnt + edge.WindDelta;
                }
                edge.WindCnt2 = e.WindCnt2;
                e = e.NextInAel; //ie get ready to calc WindCnt2
            }

            //update WindCnt2 ...
            if (this.IsEvenOddAltFillType(edge))
            {
                //EvenOdd filling ...
                while (e != edge)
                {
                    if (e.WindDelta != 0)
                        edge.WindCnt2 = edge.WindCnt2 == 0 ? 1 : 0;
                    e = e.NextInAel;
                }
            }
            else
            {
                //nonZero, Positive or Negative filling ...
                while (e != edge)
                {
                    edge.WindCnt2 += e.WindDelta;
                    e = e.NextInAel;
                }
            }
        }
        //------------------------------------------------------------------------------

        private void SwapPositionsInSel(Edge edge1, Edge edge2)
        {
            if (edge1.NextInSel == null && edge1.PrevInSel == null)
                return;
            if (edge2.NextInSel == null && edge2.PrevInSel == null)
                return;

            if (edge1.NextInSel == edge2)
            {
                var next = edge2.NextInSel;
                if (next != null)
                    next.PrevInSel = edge1;
                var prev = edge1.PrevInSel;
                if (prev != null)
                    prev.NextInSel = edge2;
                edge2.PrevInSel = prev;
                edge2.NextInSel = edge1;
                edge1.PrevInSel = edge2;
                edge1.NextInSel = next;
            }
            else if (edge2.NextInSel == edge1)
            {
                var next = edge1.NextInSel;
                if (next != null)
                    next.PrevInSel = edge2;
                var prev = edge2.PrevInSel;
                if (prev != null)
                    prev.NextInSel = edge1;
                edge1.PrevInSel = prev;
                edge1.NextInSel = edge2;
                edge2.PrevInSel = edge1;
                edge2.NextInSel = next;
            }
            else
            {
                var next = edge1.NextInSel;
                var prev = edge1.PrevInSel;
                edge1.NextInSel = edge2.NextInSel;
                if (edge1.NextInSel != null)
                    edge1.NextInSel.PrevInSel = edge1;
                edge1.PrevInSel = edge2.PrevInSel;
                if (edge1.PrevInSel != null)
                    edge1.PrevInSel.NextInSel = edge1;
                edge2.NextInSel = next;
                if (edge2.NextInSel != null)
                    edge2.NextInSel.PrevInSel = edge2;
                edge2.PrevInSel = prev;
                if (edge2.PrevInSel != null)
                    edge2.PrevInSel.NextInSel = edge2;
            }

            if (edge1.PrevInSel == null)
                this.mSortedEdges = edge1;
            else if (edge2.PrevInSel == null)
                this.mSortedEdges = edge2;
        }
        //------------------------------------------------------------------------------

        private void UpdateOutPtIdxs(OutRec outrec)
        {
            var op = outrec.Pts;
            do
            {
                op.Idx = outrec.Idx;
                op = op.Prev;
            }
            while (op != outrec.Pts);
        }

        #endregion

#if use_xyz
        public delegate void ZFillCallback(IntPoint bot1, IntPoint top1, IntPoint bot2, IntPoint top2, ref IntPoint pt);

        public ZFillCallback ZFillFunction { get; set; }
#endif
        //------------------------------------------------------------------------------
    } //end Clipper

    public class ClipperOffset
    {
        #region Constants

        private const double DefArcTolerance = 0.25;

        private const double TwoPi = Math.PI * 2;

        #endregion

        #region Fields

        private double mDelta, mSinA, mSin, mCos;

        private Path mDestPoly;

        private Paths mDestPolys;

        private IntPoint mLowest;

        private double mMiterLim, mStepsPerRad;

        private List<DoublePoint> mNormals = new List<DoublePoint>();

        private PolyNode mPolyNodes = new PolyNode();

        private Path mSrcPoly;

        #endregion

        #region Constructors and Destructors

        public ClipperOffset(double miterLimit = 2.0, double arcTolerance = DefArcTolerance)
        {
            this.MiterLimit = miterLimit;
            this.ArcTolerance = arcTolerance;
            this.mLowest.X = -1;
        }

        #endregion

        #region Public Properties

        public double ArcTolerance { get; set; }

        public double MiterLimit { get; set; }

        #endregion

        #region Public Methods and Operators

        //------------------------------------------------------------------------------

        public void AddPath(Path path, JoinType joinType, EndType endType)
        {
            var highI = path.Count - 1;
            if (highI < 0)
                return;
            var newNode = new PolyNode();
            newNode.MJointype = joinType;
            newNode.MEndtype = endType;

            //strip duplicate points from path and also get index to the lowest point ...
            if (endType == EndType.EtClosedLine || endType == EndType.EtClosedPolygon)
                while (highI > 0 && path[0] == path[highI])
                    highI--;
            newNode.MPolygon.Capacity = highI + 1;
            newNode.MPolygon.Add(path[0]);
            int j = 0, k = 0;
            for (var i = 1; i <= highI; i++)
                if (newNode.MPolygon[j] != path[i])
                {
                    j++;
                    newNode.MPolygon.Add(path[i]);
                    if (path[i].Y > newNode.MPolygon[k].Y || path[i].Y == newNode.MPolygon[k].Y
                        && path[i].X < newNode.MPolygon[k].X)
                        k = j;
                }
            if (endType == EndType.EtClosedPolygon && j < 2)
                return;

            this.mPolyNodes.AddChild(newNode);

            //if this path's lowest pt is lower than all the others then update m_lowest
            if (endType != EndType.EtClosedPolygon)
                return;
            if (this.mLowest.X < 0)
                this.mLowest = new IntPoint(this.mPolyNodes.ChildCount - 1, k);
            else
            {
                var ip = this.mPolyNodes.Childs[(int) this.mLowest.X].MPolygon[(int) this.mLowest.Y];
                if (newNode.MPolygon[k].Y > ip.Y || newNode.MPolygon[k].Y == ip.Y && newNode.MPolygon[k].X < ip.X)
                    this.mLowest = new IntPoint(this.mPolyNodes.ChildCount - 1, k);
            }
        }
        //------------------------------------------------------------------------------

        public void AddPaths(Paths paths, JoinType joinType, EndType endType)
        {
            foreach (var p in paths)
                this.AddPath(p, joinType, endType);
        }
        //------------------------------------------------------------------------------

        public void Clear()
        {
            this.mPolyNodes.Childs.Clear();
            this.mLowest.X = -1;
        }
        //------------------------------------------------------------------------------

        public void Execute(ref Paths solution, double delta)
        {
            solution.Clear();
            this.FixOrientations();
            this.DoOffset(delta);
            //now clean up 'corners' ...
            var clpr = new Clipper();
            clpr.AddPaths(this.mDestPolys, PolyType.PtSubject, true);
            if (delta > 0)
            {
                clpr.Execute(ClipType.CtUnion, solution, PolyFillType.PftPositive, PolyFillType.PftPositive);
            }
            else
            {
                var r = ClipperBase.GetBounds(this.mDestPolys);
                var outer = new Path(4);

                outer.Add(new IntPoint(r.Left - 10, r.Bottom + 10));
                outer.Add(new IntPoint(r.Right + 10, r.Bottom + 10));
                outer.Add(new IntPoint(r.Right + 10, r.Top - 10));
                outer.Add(new IntPoint(r.Left - 10, r.Top - 10));

                clpr.AddPath(outer, PolyType.PtSubject, true);
                clpr.ReverseSolution = true;
                clpr.Execute(ClipType.CtUnion, solution, PolyFillType.PftNegative, PolyFillType.PftNegative);
                if (solution.Count > 0)
                    solution.RemoveAt(0);
            }
        }
        //------------------------------------------------------------------------------

        public void Execute(ref PolyTree solution, double delta)
        {
            solution.Clear();
            this.FixOrientations();
            this.DoOffset(delta);

            //now clean up 'corners' ...
            var clpr = new Clipper();
            clpr.AddPaths(this.mDestPolys, PolyType.PtSubject, true);
            if (delta > 0)
            {
                clpr.Execute(ClipType.CtUnion, solution, PolyFillType.PftPositive, PolyFillType.PftPositive);
            }
            else
            {
                var r = ClipperBase.GetBounds(this.mDestPolys);
                var outer = new Path(4);

                outer.Add(new IntPoint(r.Left - 10, r.Bottom + 10));
                outer.Add(new IntPoint(r.Right + 10, r.Bottom + 10));
                outer.Add(new IntPoint(r.Right + 10, r.Top - 10));
                outer.Add(new IntPoint(r.Left - 10, r.Top - 10));

                clpr.AddPath(outer, PolyType.PtSubject, true);
                clpr.ReverseSolution = true;
                clpr.Execute(ClipType.CtUnion, solution, PolyFillType.PftNegative, PolyFillType.PftNegative);
                //remove the outer PolyNode rectangle ...
                if (solution.ChildCount == 1 && solution.Childs[0].ChildCount > 0)
                {
                    var outerNode = solution.Childs[0];
                    solution.Childs.Capacity = outerNode.ChildCount;
                    solution.Childs[0] = outerNode.Childs[0];
                    solution.Childs[0].MParent = solution;
                    for (var i = 1; i < outerNode.ChildCount; i++)
                        solution.AddChild(outerNode.Childs[i]);
                }
                else
                    solution.Clear();
            }
        }

        #endregion

        #region Methods

        //------------------------------------------------------------------------------

        internal static DoublePoint GetUnitNormal(IntPoint pt1, IntPoint pt2)
        {
            double dx = pt2.X - pt1.X;
            double dy = pt2.Y - pt1.Y;
            if (dx == 0 && dy == 0)
                return new DoublePoint();

            var f = 1 * 1.0 / Math.Sqrt(dx * dx + dy * dy);
            dx *= f;
            dy *= f;

            return new DoublePoint(dy, -dx);
        }
        //------------------------------------------------------------------------------

        internal static cInt Round(double value)
        {
            return value < 0 ? (cInt) (value - 0.5) : (cInt) (value + 0.5);
        }
        //------------------------------------------------------------------------------

        internal void DoMiter(int j, int k, double r)
        {
            var q = this.mDelta / r;
            this.mDestPoly.Add(
                new IntPoint(
                    Round(this.mSrcPoly[j].X + (this.mNormals[k].X + this.mNormals[j].X) * q),
                    Round(this.mSrcPoly[j].Y + (this.mNormals[k].Y + this.mNormals[j].Y) * q)));
        }
        //------------------------------------------------------------------------------

        internal void DoRound(int j, int k)
        {
            var a = Math.Atan2(
                this.mSinA,
                this.mNormals[k].X * this.mNormals[j].X + this.mNormals[k].Y * this.mNormals[j].Y);
            var steps = Math.Max((int) Round(this.mStepsPerRad * Math.Abs(a)), 1);

            double x = this.mNormals[k].X, y = this.mNormals[k].Y, x2;
            for (var i = 0; i < steps; ++i)
            {
                this.mDestPoly.Add(
                    new IntPoint(
                        Round(this.mSrcPoly[j].X + x * this.mDelta),
                        Round(this.mSrcPoly[j].Y + y * this.mDelta)));
                x2 = x;
                x = x * this.mCos - this.mSin * y;
                y = x2 * this.mSin + y * this.mCos;
            }
            this.mDestPoly.Add(
                new IntPoint(
                    Round(this.mSrcPoly[j].X + this.mNormals[j].X * this.mDelta),
                    Round(this.mSrcPoly[j].Y + this.mNormals[j].Y * this.mDelta)));
        }
        //------------------------------------------------------------------------------

        internal void DoSquare(int j, int k)
        {
            var dx = Math.Tan(
                Math.Atan2(
                    this.mSinA,
                    this.mNormals[k].X * this.mNormals[j].X + this.mNormals[k].Y * this.mNormals[j].Y) / 4);
            this.mDestPoly.Add(
                new IntPoint(
                    Round(this.mSrcPoly[j].X + this.mDelta * (this.mNormals[k].X - this.mNormals[k].Y * dx)),
                    Round(this.mSrcPoly[j].Y + this.mDelta * (this.mNormals[k].Y + this.mNormals[k].X * dx))));
            this.mDestPoly.Add(
                new IntPoint(
                    Round(this.mSrcPoly[j].X + this.mDelta * (this.mNormals[j].X + this.mNormals[j].Y * dx)),
                    Round(this.mSrcPoly[j].Y + this.mDelta * (this.mNormals[j].Y - this.mNormals[j].X * dx))));
        }
        //------------------------------------------------------------------------------

        private void DoOffset(double delta)
        {
            this.mDestPolys = new Paths();
            this.mDelta = delta;

            //if Zero offset, just copy any CLOSED polygons to m_p and return ...
            if (ClipperBase.NearZero(delta))
            {
                this.mDestPolys.Capacity = this.mPolyNodes.ChildCount;
                for (var i = 0; i < this.mPolyNodes.ChildCount; i++)
                {
                    var node = this.mPolyNodes.Childs[i];
                    if (node.MEndtype == EndType.EtClosedPolygon)
                        this.mDestPolys.Add(node.MPolygon);
                }
                return;
            }

            //see offset_triginometry3.svg in the documentation folder ...
            if (this.MiterLimit > 2)
                this.mMiterLim = 2 / (this.MiterLimit * this.MiterLimit);
            else
                this.mMiterLim = 0.5;

            double y;
            if (this.ArcTolerance <= 0.0)
                y = DefArcTolerance;
            else if (this.ArcTolerance > Math.Abs(delta) * DefArcTolerance)
                y = Math.Abs(delta) * DefArcTolerance;
            else
                y = this.ArcTolerance;
            //see offset_triginometry2.svg in the documentation folder ...
            var steps = Math.PI / Math.Acos(1 - y / Math.Abs(delta));
            this.mSin = Math.Sin(TwoPi / steps);
            this.mCos = Math.Cos(TwoPi / steps);
            this.mStepsPerRad = steps / TwoPi;
            if (delta < 0.0)
                this.mSin = -this.mSin;

            this.mDestPolys.Capacity = this.mPolyNodes.ChildCount * 2;
            for (var i = 0; i < this.mPolyNodes.ChildCount; i++)
            {
                var node = this.mPolyNodes.Childs[i];
                this.mSrcPoly = node.MPolygon;

                var len = this.mSrcPoly.Count;

                if (len == 0 || delta <= 0 && (len < 3 || node.MEndtype != EndType.EtClosedPolygon))
                    continue;

                this.mDestPoly = new Path();

                if (len == 1)
                {
                    if (node.MJointype == JoinType.JtRound)
                    {
                        double x = 1.0, Y = 0.0;
                        for (var j = 1; j <= steps; j++)
                        {
                            this.mDestPoly.Add(
                                new IntPoint(
                                    Round(this.mSrcPoly[0].X + x * delta),
                                    Round(this.mSrcPoly[0].Y + Y * delta)));
                            var x2 = x;
                            x = x * this.mCos - this.mSin * Y;
                            Y = x2 * this.mSin + Y * this.mCos;
                        }
                    }
                    else
                    {
                        double x = -1.0, Y = -1.0;
                        for (var j = 0; j < 4; ++j)
                        {
                            this.mDestPoly.Add(
                                new IntPoint(
                                    Round(this.mSrcPoly[0].X + x * delta),
                                    Round(this.mSrcPoly[0].Y + Y * delta)));
                            if (x < 0)
                                x = 1;
                            else if (Y < 0)
                                Y = 1;
                            else
                                x = -1;
                        }
                    }
                    this.mDestPolys.Add(this.mDestPoly);
                    continue;
                }

                //build m_normals ...
                this.mNormals.Clear();
                this.mNormals.Capacity = len;
                for (var j = 0; j < len - 1; j++)
                    this.mNormals.Add(GetUnitNormal(this.mSrcPoly[j], this.mSrcPoly[j + 1]));
                if (node.MEndtype == EndType.EtClosedLine || node.MEndtype == EndType.EtClosedPolygon)
                    this.mNormals.Add(GetUnitNormal(this.mSrcPoly[len - 1], this.mSrcPoly[0]));
                else
                    this.mNormals.Add(new DoublePoint(this.mNormals[len - 2]));

                if (node.MEndtype == EndType.EtClosedPolygon)
                {
                    var k = len - 1;
                    for (var j = 0; j < len; j++)
                        this.OffsetPoint(j, ref k, node.MJointype);
                    this.mDestPolys.Add(this.mDestPoly);
                }
                else if (node.MEndtype == EndType.EtClosedLine)
                {
                    var k = len - 1;
                    for (var j = 0; j < len; j++)
                        this.OffsetPoint(j, ref k, node.MJointype);
                    this.mDestPolys.Add(this.mDestPoly);
                    this.mDestPoly = new Path();
                    //re-build m_normals ...
                    var n = this.mNormals[len - 1];
                    for (var j = len - 1; j > 0; j--)
                        this.mNormals[j] = new DoublePoint(-this.mNormals[j - 1].X, -this.mNormals[j - 1].Y);
                    this.mNormals[0] = new DoublePoint(-n.X, -n.Y);
                    k = 0;
                    for (var j = len - 1; j >= 0; j--)
                        this.OffsetPoint(j, ref k, node.MJointype);
                    this.mDestPolys.Add(this.mDestPoly);
                }
                else
                {
                    var k = 0;
                    for (var j = 1; j < len - 1; ++j)
                        this.OffsetPoint(j, ref k, node.MJointype);

                    IntPoint pt1;
                    if (node.MEndtype == EndType.EtOpenButt)
                    {
                        var j = len - 1;
                        pt1 = new IntPoint(
                            Round(this.mSrcPoly[j].X + this.mNormals[j].X * delta),
                            Round(this.mSrcPoly[j].Y + this.mNormals[j].Y * delta));
                        this.mDestPoly.Add(pt1);
                        pt1 = new IntPoint(
                            Round(this.mSrcPoly[j].X - this.mNormals[j].X * delta),
                            Round(this.mSrcPoly[j].Y - this.mNormals[j].Y * delta));
                        this.mDestPoly.Add(pt1);
                    }
                    else
                    {
                        var j = len - 1;
                        k = len - 2;
                        this.mSinA = 0;
                        this.mNormals[j] = new DoublePoint(-this.mNormals[j].X, -this.mNormals[j].Y);
                        if (node.MEndtype == EndType.EtOpenSquare)
                            this.DoSquare(j, k);
                        else
                            this.DoRound(j, k);
                    }

                    //re-build m_normals ...
                    for (var j = len - 1; j > 0; j--)
                        this.mNormals[j] = new DoublePoint(-this.mNormals[j - 1].X, -this.mNormals[j - 1].Y);

                    this.mNormals[0] = new DoublePoint(-this.mNormals[1].X, -this.mNormals[1].Y);

                    k = len - 1;
                    for (var j = k - 1; j > 0; --j)
                        this.OffsetPoint(j, ref k, node.MJointype);

                    if (node.MEndtype == EndType.EtOpenButt)
                    {
                        pt1 = new IntPoint(
                            Round(this.mSrcPoly[0].X - this.mNormals[0].X * delta),
                            Round(this.mSrcPoly[0].Y - this.mNormals[0].Y * delta));
                        this.mDestPoly.Add(pt1);
                        pt1 = new IntPoint(
                            Round(this.mSrcPoly[0].X + this.mNormals[0].X * delta),
                            Round(this.mSrcPoly[0].Y + this.mNormals[0].Y * delta));
                        this.mDestPoly.Add(pt1);
                    }
                    else
                    {
                        k = 1;
                        this.mSinA = 0;
                        if (node.MEndtype == EndType.EtOpenSquare)
                            this.DoSquare(0, 1);
                        else
                            this.DoRound(0, 1);
                    }
                    this.mDestPolys.Add(this.mDestPoly);
                }
            }
        }
        //------------------------------------------------------------------------------

        private void FixOrientations()
        {
            //fixup orientations of all closed paths if the orientation of the
            //closed path with the lowermost vertex is wrong ...
            if (this.mLowest.X >= 0 && !Clipper.Orientation(this.mPolyNodes.Childs[(int) this.mLowest.X].MPolygon))
            {
                for (var i = 0; i < this.mPolyNodes.ChildCount; i++)
                {
                    var node = this.mPolyNodes.Childs[i];
                    if (node.MEndtype == EndType.EtClosedPolygon || node.MEndtype == EndType.EtClosedLine
                        && Clipper.Orientation(node.MPolygon))
                        node.MPolygon.Reverse();
                }
            }
            else
            {
                for (var i = 0; i < this.mPolyNodes.ChildCount; i++)
                {
                    var node = this.mPolyNodes.Childs[i];
                    if (node.MEndtype == EndType.EtClosedLine && !Clipper.Orientation(node.MPolygon))
                        node.MPolygon.Reverse();
                }
            }
        }
        //------------------------------------------------------------------------------

        void OffsetPoint(int j, ref int k, JoinType jointype)
        {
            //cross product ...
            this.mSinA = this.mNormals[k].X * this.mNormals[j].Y - this.mNormals[j].X * this.mNormals[k].Y;

            if (Math.Abs(this.mSinA * this.mDelta) < 1.0)
            {
                //dot product ...
                var cosA = this.mNormals[k].X * this.mNormals[j].X + this.mNormals[j].Y * this.mNormals[k].Y;
                if (cosA > 0) // angle ==> 0 degrees
                {
                    this.mDestPoly.Add(
                        new IntPoint(
                            Round(this.mSrcPoly[j].X + this.mNormals[k].X * this.mDelta),
                            Round(this.mSrcPoly[j].Y + this.mNormals[k].Y * this.mDelta)));
                    return;
                }
                //else angle ==> 180 degrees   
            }
            else if (this.mSinA > 1.0)
                this.mSinA = 1.0;
            else if (this.mSinA < -1.0)
                this.mSinA = -1.0;

            if (this.mSinA * this.mDelta < 0)
            {
                this.mDestPoly.Add(
                    new IntPoint(
                        Round(this.mSrcPoly[j].X + this.mNormals[k].X * this.mDelta),
                        Round(this.mSrcPoly[j].Y + this.mNormals[k].Y * this.mDelta)));
                this.mDestPoly.Add(this.mSrcPoly[j]);
                this.mDestPoly.Add(
                    new IntPoint(
                        Round(this.mSrcPoly[j].X + this.mNormals[j].X * this.mDelta),
                        Round(this.mSrcPoly[j].Y + this.mNormals[j].Y * this.mDelta)));
            }
            else
                switch (jointype)
                {
                    case JoinType.JtMiter:
                    {
                        var r = 1 + (this.mNormals[j].X * this.mNormals[k].X
                            + this.mNormals[j].Y * this.mNormals[k].Y);
                        if (r >= this.mMiterLim)
                            this.DoMiter(j, k, r);
                        else
                            this.DoSquare(j, k);
                        break;
                    }
                    case JoinType.JtSquare:
                        this.DoSquare(j, k);
                        break;
                    case JoinType.JtRound:
                        this.DoRound(j, k);
                        break;
                }
            k = j;
        }

        #endregion

        //------------------------------------------------------------------------------
    }

    class ClipperException : Exception
    {
        #region Constructors and Destructors

        public ClipperException(string description)
            : base(description)
        {
        }

        #endregion
    }

    //------------------------------------------------------------------------------
} //end ClipperLib namespace
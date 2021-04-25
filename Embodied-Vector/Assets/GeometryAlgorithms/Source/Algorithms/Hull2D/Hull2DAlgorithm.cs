using Jobberwocky.GeometryAlgorithms.Source.Core;
using System;
using System.Collections.Generic;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Hull2D
{
    /// <summary>
    /// Class that contains the algorithm for creating a hull around a set of points
    /// </summary>
    public class Hull2DAlgorithm
    {
        private readonly double constAngleCos = Math.Cos(90.0 / (180.0 / Math.PI));

        /// <summary>
        /// Constructor
        /// </summary>
        public Hull2DAlgorithm()
        {

        }

        /// <summary>
        /// Calculates the cross product of three vectors
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double Cross(Vertex o, Vertex a, Vertex b)
        {
            return (a.Position.x - o.Position.x) * (b.Position.y - o.Position.y)
                - (a.Position.y - o.Position.y) * (b.Position.x - o.Position.x);
        }

        /// <summary>
        /// Calculates the point set in the upper tangent
        /// </summary>
        /// <param name="pointSet"></param>
        /// <returns></returns>
        private Vertex[] UpperTangent(Vertex[] pointSet)
        {
            var lower = new List<Vertex>();

            for (var l = 0; l < pointSet.Length; l++)
            {
                while (lower.Count >= 2 && (Cross(lower[lower.Count - 2], lower[lower.Count - 1], pointSet[l]) <= 0))
                {
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(pointSet[l]);
            }
            lower.RemoveAt(lower.Count - 1);

            return lower.ToArray();
        }

        /// <summary>
        /// Calculates the point set for the lower tangent
        /// </summary>
        /// <param name="pointSet"></param>
        /// <returns></returns>
        private Vertex[] LowerTangent(ref Vertex[] pointSet)
        {
            Vertex[] reversed = new Vertex[pointSet.Length];

            var upper = new List<Vertex>();

            for (var u = 0; u < pointSet.Length; u++)
            {
                reversed[u] = (pointSet[pointSet.Length - 1 - u]);

                while (upper.Count >= 2 && (Cross(upper[upper.Count - 2], upper[upper.Count - 1], reversed[u]) <= 0))
                {
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(reversed[u]);
            }
            upper.RemoveAt(upper.Count - 1);

            pointSet = reversed;

            return upper.ToArray();
        }

        /// <summary>
        /// Calculates the convex hull of a given input point set
        /// </summary>
        /// <param name="pointSet"></param>
        /// <returns></returns>
        private List<Vertex> Convex(ref Vertex[] pointSet)
        {
            var upper = UpperTangent(pointSet);
            var lower = LowerTangent(ref pointSet);
            var convexHull = new List<Vertex>(lower.Length + upper.Length + 1);
            convexHull.AddRange(lower);
            convexHull.AddRange(upper);
            convexHull.Add(pointSet[0]);

            return convexHull;
        }

        /// <summary>
        /// Are the vectors counter clock wise?
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private bool Ccw(Vertex p1, Vertex p2, Vertex p3)
        {
            var cw = ((p3.Position.y - p1.Position.y) * (p2.Position.x - p1.Position.x))
                - ((p2.Position.y - p1.Position.y) * (p3.Position.x - p1.Position.x));
            return cw > 0 ? true : cw < 0 ? false : true;
        }

        /// <summary>
        /// Check whether the two segments intersect
        /// </summary>
        /// <param name="seg1"></param>
        /// <param name="seg2"></param>
        /// <returns></returns>
        private bool Intersect(Vertex seg1P1, Vertex seg1P2, Vertex seg2P1, Vertex seg2P2)
        {
            return Ccw(seg1P1, seg2P1, seg2P2) != Ccw(seg1P2, seg2P1, seg2P2)
                && Ccw(seg1P1, seg1P2, seg2P1) != Ccw(seg1P1, seg1P2, seg2P2);
        }

        /// <summary>
        /// Sort the point set by the X coordinate and then by the Y coordinate
        /// </summary>
        /// <param name="pointSet"></param>
        private void SortByX(Vertex[] pointSet)
        {
            Array.Sort(pointSet, delegate (Vertex p1, Vertex p2)
            {
                if (p1.Position.x == p2.Position.x)
                {
                    return p1.Position.y.CompareTo(p2.Position.y);
                }
                else
                {
                    return p1.Position.x.CompareTo(p2.Position.x);
                }
            });
        }

        /// <summary>
        /// Calculate the square length between two vectors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double SqLength(Vertex a, Vertex b)
        {
            return (b.Position.x - a.Position.x) * (b.Position.x - a.Position.x)
                + (b.Position.y - a.Position.y) * (b.Position.y - a.Position.y);
        }

        /// <summary>
        /// Calculates the angle
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double Cos(Vertex o, Vertex a, Vertex b)
        {
            var aShiftedX = a.Position.x - o.Position.x;
            var aShiftedY = a.Position.y - o.Position.y;
            var bShiftedX = b.Position.x - o.Position.x;
            var bShiftedY = b.Position.y - o.Position.y;

            var sqALen = SqLength(o, a);
            var sqBLen = SqLength(o, b);
            var dot = aShiftedX * bShiftedX + aShiftedY * bShiftedY;

            return dot / Math.Sqrt(sqALen * sqBLen);
        }

        /// <summary>
        /// Check whether the segment intersects with the point set
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="pointSet"></param>
        /// <returns></returns>
        private bool Intersect(Vertex seg1P1, Vertex seg1P2, List<Vertex> pointSet)
        {
            Vertex seg2P1, seg2P2;
            for (var i = 0; i < pointSet.Count - 1; i++)
            {
                seg2P1 = pointSet[i];
                seg2P2 = pointSet[i + 1];
                if (seg1P1.Equals(seg2P1) || seg1P1.Equals(seg2P2))
                {
                    continue;
                }
                if (Intersect(seg1P1, seg1P2, seg2P1, seg2P2))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the occupied area of a given point set
        /// </summary>
        /// <param name="pointSet"></param>
        /// <returns></returns>
        private double[] OccupiedArea(Vertex[] pointSet)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            for (var i = pointSet.Length - 1; i >= 0; i--)
            {
                if (pointSet[i].Position.x < minX)
                {
                    minX = pointSet[i].Position.x;
                }
                if (pointSet[i].Position.y < minY)
                {
                    minY = pointSet[i].Position.y;
                }
                if (pointSet[i].Position.x > maxX)
                {
                    maxX = pointSet[i].Position.x;
                }
                if (pointSet[i].Position.y > maxY)
                {
                    maxY = pointSet[i].Position.y;
                }
            }

            return new double[2] { maxX - minX, maxY - minY };
        }

        /// <summary>
        /// Calculates the bounding box of an edge
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private double[] BboxAround(Vertex[] edge)
        {
            return new double[4] {
                Math.Min(edge[0].Position.x, edge[1].Position.x),
                Math.Min(edge[0].Position.y, edge[1].Position.y),
                Math.Max(edge[0].Position.x, edge[1].Position.x),
                Math.Max(edge[0].Position.y, edge[1].Position.y)
            };
        }

        /// <summary>
        /// Calculates if one of the innerpoints could further detail the given edge
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="innerPoints"></param>
        /// <param name="convex"></param>
        /// <returns></returns>
        private Vertex MidPoint(Vertex[] edge, List<Vertex> innerPoints, List<Vertex> hullPoints)
        {
            var angle1Cos = constAngleCos;
            var angle2Cos = constAngleCos;
            double a1Cos, a2Cos;
            Vertex midPoint = null, point = null;
            for (var i = 0; i < innerPoints.Count; i++)
            {
                point = innerPoints[i];
                a1Cos = Cos(edge[0], edge[1], point);
                a2Cos = Cos(edge[1], edge[0], point);

                if (a1Cos > angle1Cos && a2Cos > angle2Cos &&
                    !Intersect(edge[0], point, hullPoints) &&
                    !Intersect(edge[1], point, hullPoints))
                {
                    angle1Cos = a1Cos;
                    angle2Cos = a2Cos;
                    midPoint = point;
                }
            }

            return midPoint;
        }

        /// <summary>
        /// Calculates the concave hull based on an exsiting convex hull
        /// </summary>
        /// <param name="convex"></param>
        /// <param name="maxSqEdgeLen"></param>
        /// <param name="maxSearchArea"></param>
        /// <param name="grid"></param>
        /// <param name="edgeSkipList"></param>
        /// <returns></returns>
        private Vertex[] Concave(List<Vertex> convex, double maxSqEdgeLen, double[] maxSearchArea, Grid grid)
        {
            bool midPointInserted, hasValue;
            int scaleFactor;
            double bboxWidth, bboxHeight;
            double[] bboxMax = new double[4];
            Vertex midPoint;
            Vertex[] edge = new Vertex[2];
            EdgeKey keyInSkipList;
            Dictionary<EdgeKey, bool> edgeSkipList = new Dictionary<EdgeKey, bool>();

            do
            {
                midPointInserted = false;

                for (var i = 0; i < convex.Count - 1; i++)
                {
                    midPoint = null;

                    edge[0] = convex[i];
                    edge[1] = convex[i + 1];

                    keyInSkipList.point1 = edge[0].Id;
                    keyInSkipList.point2 = edge[1].Id;

                    if (SqLength(edge[0], edge[1]) < maxSqEdgeLen ||
                        (edgeSkipList.TryGetValue(keyInSkipList, out hasValue) == true)) { continue; }

                    scaleFactor = 0;
                    bboxMax = BboxAround(edge);
                    do
                    {
                        grid.ExtendBbox(bboxMax, scaleFactor);
                        bboxWidth = bboxMax[2] - bboxMax[0];
                        bboxHeight = bboxMax[3] - bboxMax[1];

                        midPoint = MidPoint(edge, grid.RangePoints(bboxMax), convex);
                        scaleFactor++;

                    } while (midPoint == null && (maxSearchArea[0] > bboxWidth || maxSearchArea[1] > bboxHeight));

                    if (bboxWidth >= maxSearchArea[0] && bboxHeight >= maxSearchArea[1])
                    {
                        edgeSkipList[keyInSkipList] = true;
                    }

                    if (midPoint != null)
                    {
                        convex.Insert(i + 1, midPoint);
                        grid.RemovePoint(midPoint);
                        midPointInserted = true;
                    }
                }
            }
            while (midPointInserted);

            return convex.ToArray();
        }

        /// <summary>
        /// Calculates the hull for a given input set and given the max edge length
        /// </summary>
        /// <param name="pointSet"></param>
        /// <param name="concavity"></param>
        /// <returns></returns>
        public Vertex[] GenerateHull(Vertex[] points, double concavity)
        {
            // When we have a very small data set, just return the points
            if (points.Length < 4)
            {
                return points;
            }

            // Sort the points by their X coordinate
            SortByX(points);

            // Calculate the occupied area of all the points and the relative search area
            var occupiedArea = OccupiedArea(points);
            var maxSearchArea = new double[2] { occupiedArea[0] * 0.6, occupiedArea[1] * 0.6 };

            // Create the Convex hull
            List<Vertex> convex = Convex(ref points);

            // Calculate metrics and store the points in a spatial grid structure
            var maxEdgeLen = concavity * concavity;
            var cellSize = (int)Math.Ceiling(1 / (points.Length / (occupiedArea[0] * occupiedArea[1])));
            var grid = new Grid(points, cellSize);

            // Remove the convex hull points from the grid
            for (var i = 0; i < convex.Count; i++)
            {
                grid.RemovePoint(convex[i]);
            }

            // Generate the concave hull
            var concaveHull = Concave(convex, maxEdgeLen, maxSearchArea, grid);

            return concaveHull;
        }
    }

    /// <summary>
    /// Grid class used to store points for calculating the hull
    /// </summary>
    public class Grid
    {

        private Dictionary<GridKey, List<Vertex>> Cells;
        private int CellSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points"></param>
        /// <param name="cellSize"></param>
        public Grid(Vertex[] points, int cellSize)
        {
            Cells = new Dictionary<GridKey, List<Vertex>>();
            CellSize = cellSize;

            GridKey key = new GridKey();

            for (var i = 0; i < points.Length; i++)
            {
                key = Point2CellXY(points[i].Position.x, points[i].Position.y);

                if (!Cells.ContainsKey(key))
                {
                    Cells.Add(key, new List<Vertex>());
                }

                Cells[key].Add(points[i]);
            }
        }

        /// <summary>
        /// Get the point stored in a given cell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void CellPoints(GridKey key, ref List<Vertex> points)
        {
            if (Cells.ContainsKey(key))
            {
                points.AddRange(Cells[key]);
            }
        }

        /// <summary>
        /// Get all the point within the given bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public List<Vertex> RangePoints(double[] bbox)
        {
            GridKey keyTl = Point2CellXY(bbox[0], bbox[1]);
            GridKey keyBr = Point2CellXY(bbox[2], bbox[3]);

            GridKey key = new GridKey();
            var gridPoints = new List<Vertex>();

            for (var x = keyTl.X; x <= keyBr.X; x += 1)
            {
                for (var y = keyTl.Y; y <= keyBr.Y; y += 1)
                {
                    key.X = x;
                    key.Y = y;
                    CellPoints(key, ref gridPoints);
                }
            }

            return gridPoints;
        }

        /// <summary>
        /// Remove a given point from the grid
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public void RemovePoint(Vertex point)
        {
            GridKey key = Point2CellXY(point.Position.x, point.Position.y);
            var cell = Cells[key];
            Vertex v;
            for (var i = 0; i < cell.Count; i++)
            {
                v = cell[i];
                if (point.Equals(v))
                {
                    cell.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Get the indices in the grid from a given input point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public GridKey Point2CellXY(double px, double py)
        {
            return new GridKey((int)(px / CellSize), (int)(py / CellSize));
        }

        /// <summary>
        /// Extend a bounding box with a given scale factor
        /// </summary>
        /// <param name="bbox"></param>
        /// <param name="scaleFactor"></param>
        /// <returns></returns>
        public void ExtendBbox(double[] bbox, float scaleFactor)
        {
            bbox[0] -= (scaleFactor * CellSize);
            bbox[1] -= (scaleFactor * CellSize);
            bbox[2] += (scaleFactor * CellSize);
            bbox[3] += (scaleFactor * CellSize);
        }

    }

    public struct GridKey : IEquatable<GridKey>
    {
        public int X;
        public int Y;

        public GridKey(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridKey key)
        {
            return X == key.X && Y == key.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() * 17 + Y.GetHashCode();
        }
    }

    public struct EdgeKey : IEquatable<EdgeKey>
    {
        public int point1;
        public int point2;

        public EdgeKey(int p1, int p2)
        {
            point1 = p1;
            point2 = p2;
        }

        public bool Equals(EdgeKey key)
        {
            return point2 == key.point2 && point1 == key.point1;
        }

        public override int GetHashCode()
        {
            return point1.GetHashCode() * 17 + point2.GetHashCode();
        }

    }
}

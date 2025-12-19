using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public struct Triangle
    {
        public Vector2 A, B, C;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool ContainsVertex(Vector2 v)
        {
            return Vector2.Distance(A, v) < 0.001f ||
                   Vector2.Distance(B, v) < 0.001f ||
                   Vector2.Distance(C, v) < 0.001f;
        }

        public bool CircumcircleContains(Vector2 point)
        {
            float ax = A.x - point.x;
            float ay = A.y - point.y;
            float bx = B.x - point.x;
            float by = B.y - point.y;
            float cx = C.x - point.x;
            float cy = C.y - point.y;

            float det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                      - (bx * bx + by * by) * (ax * cy - cx * ay)
                      + (cx * cx + cy * cy) * (ax * by - bx * ay);

            // Check if triangle vertices are in counter-clockwise order
            float ccw = (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);

            return ccw > 0 ? det > 0 : det < 0;
        }
    }

    public struct Edge
    {
        public Vector2 A, B;

        public Edge(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public bool Equals(Edge other)
        {
            return (Vector2.Distance(A, other.A) < 0.001f && Vector2.Distance(B, other.B) < 0.001f) ||
                   (Vector2.Distance(A, other.B) < 0.001f && Vector2.Distance(B, other.A) < 0.001f);
        }
    }

    public static class DelaunayTriangulation
    {
        /// <summary>
        /// Computes Delaunay triangulation using Bowyer-Watson algorithm.
        /// Returns list of edges connecting adjacent points.
        /// </summary>
        public static List<Edge> Triangulate(List<Vector2> points)
        {
            if (points.Count < 2)
                return new List<Edge>();

            if (points.Count == 2)
                return new List<Edge> { new Edge(points[0], points[1]) };

            // Create super-triangle that contains all points
            Triangle superTriangle = CreateSuperTriangle(points);

            List<Triangle> triangles = new List<Triangle> { superTriangle };

            // Add each point one at a time
            foreach (var point in points)
            {
                List<Triangle> badTriangles = new List<Triangle>();

                // Find all triangles whose circumcircle contains the point
                foreach (var triangle in triangles)
                {
                    if (triangle.CircumcircleContains(point))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                List<Edge> polygon = new List<Edge>();

                // Find the boundary of the polygonal hole
                foreach (var triangle in badTriangles)
                {
                    Edge[] edges = {
                        new Edge(triangle.A, triangle.B),
                        new Edge(triangle.B, triangle.C),
                        new Edge(triangle.C, triangle.A)
                    };

                    foreach (var edge in edges)
                    {
                        bool isShared = false;
                        foreach (var other in badTriangles)
                        {
                            if (triangle.Equals(other)) continue;

                            Edge[] otherEdges = {
                                new Edge(other.A, other.B),
                                new Edge(other.B, other.C),
                                new Edge(other.C, other.A)
                            };

                            foreach (var otherEdge in otherEdges)
                            {
                                if (edge.Equals(otherEdge))
                                {
                                    isShared = true;
                                    break;
                                }
                            }
                            if (isShared) break;
                        }

                        if (!isShared)
                        {
                            polygon.Add(edge);
                        }
                    }
                }

                // Remove bad triangles
                foreach (var bad in badTriangles)
                {
                    triangles.Remove(bad);
                }

                // Re-triangulate the hole
                foreach (var edge in polygon)
                {
                    triangles.Add(new Triangle(edge.A, edge.B, point));
                }
            }

            // Remove triangles that share vertices with super-triangle
            triangles.RemoveAll(t =>
                t.ContainsVertex(superTriangle.A) ||
                t.ContainsVertex(superTriangle.B) ||
                t.ContainsVertex(superTriangle.C));

            // Extract unique edges from remaining triangles
            List<Edge> result = new List<Edge>();
            foreach (var triangle in triangles)
            {
                AddUniqueEdge(result, new Edge(triangle.A, triangle.B));
                AddUniqueEdge(result, new Edge(triangle.B, triangle.C));
                AddUniqueEdge(result, new Edge(triangle.C, triangle.A));
            }

            return result;
        }

        private static Triangle CreateSuperTriangle(List<Vector2> points)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var p in points)
            {
                minX = Mathf.Min(minX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x);
                maxY = Mathf.Max(maxY, p.y);
            }

            float dx = maxX - minX;
            float dy = maxY - minY;
            float dmax = Mathf.Max(dx, dy) * 2;

            float midX = (minX + maxX) / 2;
            float midY = (minY + maxY) / 2;

            return new Triangle(
                new Vector2(midX - dmax * 2, midY - dmax),
                new Vector2(midX, midY + dmax * 2),
                new Vector2(midX + dmax * 2, midY - dmax)
            );
        }

        private static void AddUniqueEdge(List<Edge> edges, Edge edge)
        {
            foreach (var e in edges)
            {
                if (e.Equals(edge)) return;
            }
            edges.Add(edge);
        }
    }
}

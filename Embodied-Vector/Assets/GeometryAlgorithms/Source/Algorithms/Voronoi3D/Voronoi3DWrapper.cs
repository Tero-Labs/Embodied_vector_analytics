using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using Jobberwocky.MIConvexHull;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Voronoi3D
{
    public class Voronoi3DWrapper
    {
        public Voronoi3DWrapper()
        {
        }

        public Geometry Voronoi3D(Voronoi3DParameters parameters)
        {
            return Voronoi3DBase(parameters);
        }


        /// <summary>
        /// The base method for the creation of a 3D voronoi diagram
        /// </summary>
        /// /// <param name="parameters"></param>
        /// <returns></returns>
        private Geometry Voronoi3DBase(Voronoi3DParameters parameters)
        {
            var geometry = new Geometry();
            if (parameters == null)
            {
                parameters = new Voronoi3DParameters();
            }

            var points = parameters.Points;
            if (points != null && points.Length > 3)
            {
                // Translates the unity vector points to vertices
                var pointVertices = VectorToVertex(points, parameters.Order);

                float minX = Mathf.Infinity, minY = Mathf.Infinity, minZ = Mathf.Infinity;
                float maxX = Mathf.NegativeInfinity, maxY = Mathf.NegativeInfinity, maxZ = Mathf.NegativeInfinity;
                foreach (var point in points)
                {
                    minX = minX > point.x ? point.x : minX;
                    minY = minY > point.y ? point.y : minY;
                    minZ = minZ > point.z ? point.z : minZ;
                    maxX = maxX < point.x ? point.x : maxX;
                    maxY = maxY < point.y ? point.y : maxY;
                    maxZ = maxZ < point.z ? point.z : maxZ;
                }


                //// Creates the necessary information for a 3D voronoi diagram
                var voronoi = VoronoiMesh.Create(pointVertices);

                var vertices = new Dictionary<string, Vertex>();
                var indices = new List<int>();
                int index = 0;

                //// Create the voronoi edges by connecting the circumcenters of the tetrahydrons
                foreach (var voronoiEdge in voronoi.Edges)
                {
                    var circumcenterSource = GetCircumcenter(voronoiEdge.Source.Vertices);
                    var circumcenterTarget = GetCircumcenter(voronoiEdge.Target.Vertices);

                    if (circumcenterSource.Position.x < minX || circumcenterSource.Position.y < minY ||
                        circumcenterSource.Position.z < minZ || circumcenterSource.Position.x > maxX ||
                        circumcenterSource.Position.y > maxY || circumcenterSource.Position.z > maxZ ||
                        circumcenterTarget.Position.x < minX || circumcenterTarget.Position.y < minY ||
                        circumcenterTarget.Position.z < minZ || circumcenterTarget.Position.x > maxX ||
                        circumcenterTarget.Position.y > maxY || circumcenterTarget.Position.z > maxZ)
                    {
                        continue;
                    }

                    var sourceID = GetUniqueID(voronoiEdge.Source.Vertices);
                    if (!vertices.ContainsKey(sourceID))
                    {
                        circumcenterSource.Index = index++;
                        vertices.Add(sourceID, circumcenterSource);
                    }
                    else
                    {
                        circumcenterSource = vertices[sourceID];
                    }

                    var targetID = GetUniqueID(voronoiEdge.Target.Vertices);
                    if (!vertices.ContainsKey(targetID))
                    {
                        circumcenterTarget.Index = index++;
                        vertices.Add(targetID, circumcenterTarget);
                    }
                    else
                    {
                        circumcenterTarget = vertices[targetID];
                    }

                    indices.Add(circumcenterSource.Index);
                    indices.Add(circumcenterTarget.Index);
                }

                /*if (false)
                {
                    // Create the voronoi edges going outwards
                    foreach (var cell in voronoi.Vertices)
                    {
                        for (int i = 0; i < cell.Adjacency.Length; i++)
                        {
                            // An outward edge is only created if a tetrahydron is at the boundary of the 3D mesh
                            if (cell.Adjacency[i] == null)
                            {
                                var from = GetCircumcenter(cell.Vertices);
                                var t = cell.Vertices.Where((_, j) => j != i).ToArray();
                                var to = new Vertex();
                                for (int j = 0; j < t.Length; j++)
                                {
                                    to.Position += new Vector3((float) t[j].Position[0], (float) t[j].Position[1],
                                        (float) t[j].Position[2]) / t.Length;
                                }

                                Matrix4x4 matrix = new Matrix4x4();
                                int row = 0;
                                foreach (var v in cell.Vertices)
                                {
                                    matrix.SetRow(row,
                                        new Vector4((float) v.Position[0], (float) v.Position[1], (float) v.Position[2],
                                            1));
                                    row++;
                                }

                                matrix.SetRow(i, new Vector4(from.Position.x, from.Position.y, from.Position.z, 1));
                                var d = matrix.determinant;

                                // we only create an outward edge when the circumcenter lies within the tetrahydron
                                if (d > 0)
                                {
                                    from.Index = index++;
                                    to.Index = index++;
                                    vertices.Add(from);
                                    vertices.Add(to);
                                    indices.Add(from.Index);
                                    indices.Add(to.Index);
                                }
                            }
                        }
                    }
                }*/

                geometry.Vertices = vertices.Values.ToArray();
                geometry.Indices = indices.ToArray();
                geometry.Topology = MeshTopology.Lines;
            }

            return geometry;
        }


        private string GetUniqueID(VertexId[] vertices)
        {
            var id = "";

            foreach (var vertex in vertices)
            {
                id += vertex.Id.ToString();
            }

            return id;
        }

        private Vertex GetCircumcenter(VertexId[] vertices)
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumsphere.html

            var points = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                points[i] = new Vector3((float) vertices[i].Position[0], (float) vertices[i].Position[1],
                    (float) vertices[i].Position[2]);
            }

            Matrix4x4 m = new Matrix4x4();

            // x, y, z, 1
            for (int i = 0; i < 4; i++)
            {
                m.SetRow(i, new Vector4(points[i].x, points[i].y, points[i].z, 1));
            }

            var a = m.determinant;

            // size, y, z, 1
            for (int i = 0; i < 4; i++)
            {
                m[i, 0] = points[i].sqrMagnitude;
            }

            var dx = m.determinant;

            // size, x, z, 1
            for (int i = 0; i < 4; i++)
            {
                m[i, 1] = points[i].x;
            }

            var dy = -m.determinant;

            // size, x, y, 1
            for (int i = 0; i < 4; i++)
            {
                m[i, 2] = points[i].y;
            }

            var dz = m.determinant;

            // size, x, y, z
            for (int i = 0; i < 4; i++)
            {
                m[i, 3] = points[i].z;
            }

            var s = 1.0f / (2.0f * a);
            return new Vertex(s * dx, s * dy, s * dz);
        }

        /// <summary>
        /// Transforms a vector3 array to a vertex array that is usable for the miconvexhull library
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        private VertexId[] VectorToVertex(Vector3[] vectors, Order order)
        {
            VertexId[] vertices = new VertexId[vectors.Length];

            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = Utils.ChangeVectorCoordinateOrder(vectors[i], order);

                vertices[i] = new VertexId
                {
                    Position = new double[3] {vector.x, vector.y, vector.z},
                    Id = i
                };
            }

            return vertices;
        }

        private class VertexId : DefaultVertex
        {
            public int Id { get; set; }
        }
    }
}
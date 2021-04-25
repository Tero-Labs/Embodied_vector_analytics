using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using Jobberwocky.MIConvexHull;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Triangulation3D
{
    public class Triangulation3DWrapper
    {

        public Triangulation3DWrapper()
        {

        }

        /// <summary>
        /// Creates a 3D triangulation of the given input points.
        /// Note that this method assumes that the 3D shape is convex and without holes.
        /// This means that concave shapes are triangulated to a convex shape and holes are removed.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        public Geometry Triangulate3D(Triangulation3DParameters parameters)
        {
            var geometry = Triangulate3DBase(parameters);

            return geometry;
        }

        /// <summary>
        /// The base method for the 3D triangulation
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        private Geometry Triangulate3DBase(Triangulation3DParameters parameters)
        {
            var geometry = new Geometry();
            if (parameters == null)
            {
                parameters = new Triangulation3DParameters();
            }

            var points = parameters.Points;
            // We only triangulate when there are enough points available
            if (points != null && points.Length > 3)
            {
                VertexId[] pointVertices = VectorToVertex(points, parameters.Order);

                var triangulation = Triangulation.CreateDelaunay(pointVertices);
                // A 3D triangulation returns tetrahedrons instead of triangles
                var tetrahedrons = triangulation.Cells.ToArray();

                var vertices = new Dictionary<int, Vertex>();
                var indices = new List<int>();
                
                int index = 0;
                for (int i = 0; i < tetrahedrons.Length; i++)
                {
                    var tetrahedron = tetrahedrons[i];
                    var tetrahedronIndices = new int[tetrahedron.Vertices.Length];
                    for (int j = 0; j < tetrahedron.Vertices.Length; j++)
                    {
                        Vertex vertex = new Vertex(
                            tetrahedron.Vertices[j].Position[0],
                            tetrahedron.Vertices[j].Position[1],
                            tetrahedron.Vertices[j].Position[2],
                            tetrahedron.Vertices[j].Id);

                        if (!vertices.ContainsKey(vertex.Id))
                        {
                            vertex.Index = index++;
                            vertices.Add(vertex.Id, vertex);
                        }
                        else
                        {
                            vertex.Index = vertices[vertex.Id].Index;
                        }

                        tetrahedronIndices[j] = vertex.Index;
                    }

                    // Now are going to extract the triangles from the tetrahedrons
                    for (int j = 0; j < tetrahedron.Adjacency.Length; j++)
                    {
                        // Create the triangles, if we only want the boundaries of the mesh, 
                        // we only have to create a triangle when a face of a tetrahedron does not have a neighbor
                        if (!parameters.BoundaryOnly || tetrahedron.Adjacency[j] == null)
                        {
                            switch (j)
                            {
                                case 0:
                                    indices.Add(tetrahedronIndices[2]);
                                    indices.Add(tetrahedronIndices[1]);
                                    indices.Add(tetrahedronIndices[3]);
                                    break;
                                case 1:
                                    indices.Add(tetrahedronIndices[0]);
                                    indices.Add(tetrahedronIndices[2]);
                                    indices.Add(tetrahedronIndices[3]);
                                    break;
                                case 2:
                                    indices.Add(tetrahedronIndices[3]);
                                    indices.Add(tetrahedronIndices[1]);
                                    indices.Add(tetrahedronIndices[0]);
                                    break;
                                case 3:
                                    indices.Add(tetrahedronIndices[1]);
                                    indices.Add(tetrahedronIndices[2]);
                                    indices.Add(tetrahedronIndices[0]);
                                    break;
                            }

                            // Reverse order if we only want the back side
                            if (parameters.Side == Side.Back)
                            {
                                var tempIndex = indices[indices.Count - 3];
                                indices[indices.Count - 3] = indices[indices.Count - 1];
                                indices[indices.Count - 1] = tempIndex;
                            }

                            // Add reverse order as well if we want both sides
                            if (parameters.Side == Side.Double)
                            {
                                indices.Add(indices[indices.Count - 1]);
                                indices.Add(indices[indices.Count - 2]);
                                indices.Add(indices[indices.Count - 3]);
                            }
                        }
                    }
                }

                geometry.Vertices = vertices.Values.ToArray();
                geometry.Indices = indices.ToArray();
            }

            return geometry;
        }

        /// <summary>
        /// Transforms a vector3 array to a vertex array that is usable for the miconvexhull library
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private VertexId[] VectorToVertex(Vector3[] vectors, Order order)
        {
            VertexId[] vertices = new VertexId[vectors.Length];

            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = Utils.ChangeVectorCoordinateOrder(vectors[i], order);

                vertices[i] = new VertexId
                {
                    Position = new double[3] { vector.x, vector.y, vector.z },
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
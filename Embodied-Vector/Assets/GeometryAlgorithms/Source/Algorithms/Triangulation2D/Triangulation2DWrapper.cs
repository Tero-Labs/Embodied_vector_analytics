using System.Linq;
using UnityEngine;
using Jobberwocky.TriangleNet.Meshing;
using Jobberwocky.TriangleNet.Geometry;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Triangulation2D
{
    public class Triangulation2DWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Triangulation2DWrapper()
        {

        }

        /// <summary>
        /// Creates a 2D triangulation of the given input. It is important to note that
        /// the vector3 points input should not include the boundary and holes points.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        public Geometry Triangulate2D(Triangulation2DParameters parameters)
        {
            var geometry = new Geometry();
            if (parameters == null)
            {
                parameters = new Triangulation2DParameters();
            }

            // We only triangulate if the are enough points or boundary points available
            if ((parameters.Points != null && parameters.Points.Length > 2) || (parameters.Boundary != null && parameters.Boundary.Length > 2))
            {
                // Create triangulation
                IMesh triangulatedMesh = Triangulate2DBase(parameters);
                // Somtimes points are removed from the triangulation process, so we need to make sure
                // that these points are removed the index numbering
                triangulatedMesh.Renumber();

                var vertices = triangulatedMesh.Vertices.ToArray();
                var triangles = triangulatedMesh.Triangles.ToArray();
                geometry.Vertices = new Core.Vertex[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    TriangleNet.Geometry.Vertex vertex = vertices[i];
                    geometry.Vertices[vertex.ID] = new Core.Vertex(vertex.X, vertex.Y, vertex.Z, vertex.ID); ;
                }

                var indices = new int[triangles.Length * 3 * (parameters.Side == Side.Double ? 2 : 1)];

                for (int i = 0; i < triangles.Length; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        indices[(i * 3) + j] = triangles[i].GetVertexID(j);
                    }

                    if (parameters.Side == Side.Back)
                    {
                        var tempVertex = indices[(i * 3) + 0];
                        indices[(i * 3) + 0] = indices[(i * 3) + 2];
                        indices[(i * 3) + 2] = tempVertex;
                    }
                }

                if (parameters.Side == Side.Double)
                {
                    for (int i = 0; i < indices.Length / 2; i++)
                    {
                        indices[indices.Length / 2 + i] = indices[indices.Length / 2 - 1 - i];
                    }
                }

                geometry.Indices = indices;
            }

            return geometry;
        }

        /// <summary>
        /// The base method for creating the 2D triangulation. 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>IMesh</returns>
        private IMesh Triangulate2DBase(Triangulation2DParameters parameters)
        {
            // Setting up objects for triangulation
            GenericMesher genericMesher = new GenericMesher();
            ConstraintOptions cOptions = new ConstraintOptions();

            // Assume as default value that the shape is convex
            cOptions.Convex = true;
            cOptions.ConformingDelaunay = parameters.Delaunay;

            // We use a polygon for the triangulation of the 2D data 
            Polygon polygon = new Polygon();

            // Check whether the regular points are not null 
            // else add these points to the polygon
            var points = parameters.Points;
            if (points != null)
            {
                TriangleNet.Geometry.Vertex[] pointVertices = VectorToVertex(points, parameters.Order);

                for (var i = 0; i < pointVertices.Length; i++)
                {
                    polygon.Add(pointVertices[i]);
                }
            }

            var boundary = parameters.Boundary;
            if (boundary != null)
            {
                TriangleNet.Geometry.Vertex[] boundVertices = VectorToVertex(boundary, parameters.Order);

                polygon.Add(new Contour(boundVertices), false);

                // Assume that the shape is not convex anymore when we use boundary points
                cOptions.Convex = false;
            }

            var holes = parameters.Holes;
            if (holes != null)
            {
                for (var i = 0; i < holes.Length; i++)
                {
                    TriangleNet.Geometry.Vertex[] holeVertices = VectorToVertex(holes[i], parameters.Order);

                    polygon.Add(new Contour(holeVertices), true);
                }
            }

            // Create triangulation
            return genericMesher.Triangulate(polygon, cOptions);
        }

        /// <summary>
        /// Transforms Vector data to Vertex points which can be used by the TriangleNet library. 
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        private TriangleNet.Geometry.Vertex[] VectorToVertex(Vector3[] vectors, Order order)
        {
            TriangleNet.Geometry.Vertex[] vertices = new TriangleNet.Geometry.Vertex[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
            {
                var vector = Utils.ChangeVectorCoordinateOrder(vectors[i], order);

                var vertex = new TriangleNet.Geometry.Vertex(vector.x, vector.y);
                vertex.Z = vector.z;
                vertices[i] = vertex;
            }

            return vertices;
        }
    }
}

using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using Jobberwocky.TriangleNet.Geometry;
using Jobberwocky.TriangleNet.Meshing;
using Jobberwocky.TriangleNet.Voronoi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Voronoi2D
{
    public class Voronoi2DWrapper
    {
        public Voronoi2DWrapper()
        {
        }

        public Geometry Voronoi2D(Voronoi2DParameters parameters)
        {
            return Voronoi2DBase(parameters);
        }

        /// <summary>
        /// The base algorithm for the creation of a 2D voronoi diagram.
        /// </summary>
        /// <param name="points"></param>
        /// <returns>Geometry</returns>
        private Geometry Voronoi2DBase(Voronoi2DParameters parameters)
        {
            var geometry = new Geometry();
            if (parameters == null)
            {
                parameters = new Voronoi2DParameters();
            }

            var points = parameters.Points;
            if (points != null && points.Length > 2)
            {
                var inputVertices = VectorToVertex(points, parameters.Order);

                var polygon = new Polygon();
                for (var i = 0; i < inputVertices.Length; i++)
                {
                    polygon.Add(inputVertices[i]);
                }
                ConstraintOptions cOptions = new ConstraintOptions() { ConformingDelaunay = true };

                var triangulationMesh = (TriangleNet.Mesh)polygon.Triangulate(cOptions);
                // Create the voronoi from the triangulated mesh 
                var voronoi = new StandardVoronoi(triangulationMesh);

                // Extract all the data from the voronoi object
                var voronoiVertices = voronoi.Vertices.ToArray();
                var voronoiEdges = voronoi.Edges.ToArray();
                var voronoiHalfEdges = voronoi.HalfEdges.ToArray();
                var voronoiFaces = voronoi.Faces.ToArray();

                // First we retrieve all the voronoi vertices, e.g. the start and end point of each voronoi line
                var vertices = new Core.Vertex[voronoiVertices.Length];
                for (int i = 0; i < voronoiVertices.Length; i++)
                {
                    var vertex = voronoiVertices[i];
                    vertices[vertex.ID] = new Core.Vertex(vertex.X, vertex.Y, vertex.Z, vertex.ID); ;
                }

                // Set the indices to get all the voronoi lines
                var indices = new int[voronoiEdges.Length * 2];
                for (int i = 0; i < voronoiEdges.Length; i++)
                {
                    var edge = voronoiEdges[i];
                    indices[(i * 2) + 0] = edge.P0;
                    indices[(i * 2) + 1] = edge.P1;
                }

                // For each voronoi face (voronoi cell) we want to know which (unique) vertices it contains
                var faceVertices = new Dictionary<int, Core.Vertex>[voronoiFaces.Length];
                for (var i = 0; i < voronoiFaces.Length; i++)
                {
                    faceVertices[i] = new Dictionary<int, Core.Vertex>();
                }

                for (var i = 0; i < voronoiHalfEdges.Length; i++)
                {
                    var edge = voronoiHalfEdges[i];
                    var dictionary = faceVertices[edge.Face.ID];
                    if (!dictionary.ContainsKey(edge.Origin.ID))
                    {
                        dictionary.Add(edge.Origin.ID, vertices[edge.Origin.ID]);
                    }
                    if (!dictionary.ContainsKey(edge.Twin.Origin.ID))
                    {
                        dictionary.Add(edge.Twin.Origin.ID, vertices[edge.Twin.Origin.ID]);
                    }
                }

                var cells = new Geometry[voronoiFaces.Length];
                for (var i = 0; i < faceVertices.Length; i++)
                {
                    cells[i] = new Geometry() { Vertices = faceVertices[i].Values.ToArray() };
                }

                geometry.Vertices = vertices;
                geometry.Indices = indices;
                geometry.Cells = cells;
                geometry.Topology = MeshTopology.Lines;
            }
            return geometry;
        }

        /// <summary>
        /// Transforms Vector data to Vertex points. 
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

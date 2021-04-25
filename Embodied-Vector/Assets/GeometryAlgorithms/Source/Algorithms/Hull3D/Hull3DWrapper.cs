using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using Jobberwocky.MIConvexHull;
using System.Linq;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Hull3D
{
    public class Hull3DWrapper
    {
        public Hull3DWrapper()
        {

        }

        /// <summary>
        /// Creates the convex hull for 3D points
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Geometry Hull3D(Hull3DParameters parameters)
        {
            return Hull3DBase(parameters);
        }

        /// <summary>
        /// Creates the convex hull for 3D points
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Geometry Hull3DBase(Hull3DParameters parameters)
        {
            Geometry geometry = new Geometry();
            if (parameters == null)
            {
                parameters = new Hull3DParameters();
            }

            var points = parameters.Points;
            if (points != null && points.Length > 3)
            {
                var hull = ConvexHull.Create(VectorToVertex(points, parameters.Order));

                var vertices = new Vertex[hull.Result.Points.Count()];
                var indices = new int[hull.Result.Faces.Count() * 3];

                // assign an unique id to each point
                var newId = 0;
                foreach (var point in hull.Result.Points)
                {
                    point.Id = newId;
                    vertices[newId] = new Vertex(point.Position[0], point.Position[1], point.Position[2], newId);
                    newId++;
                }

                // translates vertices and faces to unity vertices and triangles
                int index = 0;
                foreach (var face in hull.Result.Faces)
                {
                    foreach (var vertex in face.Vertices)
                    {
                        indices[index] = vertex.Id;
                        index++;
                    }
                }

                geometry.Vertices = vertices;
                geometry.Indices = indices;
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

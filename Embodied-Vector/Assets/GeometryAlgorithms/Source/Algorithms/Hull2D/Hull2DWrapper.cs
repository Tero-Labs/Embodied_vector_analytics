using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Algorithms.Hull2D
{
    public class Hull2DWrapper
    {
        public Hull2DWrapper()
        {

        }

        /// <summary>
        /// Create a hull from a set of points based on the concavity defined in the parameters object
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Geometry Hull2D(Hull2DParameters parameters)
        {
            return Hull2DBase(parameters);
        }

        /// <summary>
        /// Create a hull from a set of points based on the concavity defined in the parameters object
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Geometry Hull2DBase(Hull2DParameters parameters)
        {
            Geometry geometry = new Geometry();

            if (parameters == null)
            {
                parameters = new Hull2DParameters();
            }

            var points = parameters.Points;
            if (points != null && points.Length > 2)
            {
                var hull2DAlgorithm = new Hull2DAlgorithm();

                var hull = hull2DAlgorithm.GenerateHull(VectorToVertex(points, parameters.Order), parameters.Concavity);

                geometry.Vertices = hull;
                geometry.Indices = new int[(hull.Length - 1) * 2];
                geometry.Topology = MeshTopology.Lines;

                for (int i = 0; i < hull.Length - 1; i++)
                {
                    geometry.Indices[i * 2 + 0] = i;
                    geometry.Indices[i * 2 + 1] = i + 1 % (hull.Length - 1);
                }
            }
            return geometry;
        }

        /// <summary>
        /// Transforms a Vector3 array to and Vertex array
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Vertex[] VectorToVertex(Vector3[] vectors, Order order)
        {
            Vertex[] vertices = new Vertex[vectors.Length];

            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = Utils.ChangeVectorCoordinateOrder(vectors[i], order);

                vertices[i] = new Vertex(vector.x, vector.y, vector.z, i);
            }

            return vertices;
        }
    }
}

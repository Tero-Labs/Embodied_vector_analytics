using UnityEngine;
using Jobberwocky.GeometryAlgorithms.Examples.Data;

namespace Jobberwocky.GeometryAlgorithms.Examples
{
    public abstract class ExampleGeometryAlgorithms : MonoBehaviour
    {
        /// <summary>
        /// Creates the point spheres from a list of vertices
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="scale"></param>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <param name="parent"></param>
        protected void CreatePointSpheres(Vector3[] vertices, float scale, Mesh mesh, Material material, GameObject parent)
        {
            var spherePoints = new GameObject[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                spherePoints[i] = new GameObject("Point " + i);
                spherePoints[i].transform.parent = parent.transform;
                spherePoints[i].transform.localPosition = vertices[i];
                spherePoints[i].transform.localScale = new Vector3(scale, scale, scale);

                var meshFilter = spherePoints[i].AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;
                var meshRenderer = spherePoints[i].AddComponent<MeshRenderer>();
                meshRenderer.material = material;
            }
        }

        /// <summary>
        /// Creates the line cylinders from a list of vertices
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="scale"></param>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <param name="parent"></param>
        protected void CreateLineCylinders(Vector3[] vertices, float scale, Mesh mesh, Material material, GameObject parent)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var startVertex = vertices[i];
                var endVertex = vertices[(i + 1) % vertices.Length];

                var cylinder = new GameObject(parent.name + " Cylinder " + i);
                cylinder.transform.parent = parent.transform;
                cylinder.transform.localPosition = (endVertex - startVertex) / 2.0f + startVertex;
                cylinder.transform.localScale = new Vector3(scale, (endVertex - startVertex).magnitude / 2.0f, scale);
                cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, (endVertex - startVertex));

                var meshFilter = cylinder.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;
                var meshRenderer = cylinder.AddComponent<MeshRenderer>();
                meshRenderer.material = material;
            }
        }

        /// <summary>
        /// Creates the boundary of a shape with line cylinders
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="scale"></param>
        /// <param name="material"></param>
        /// <param name="parent"></param>
        protected void CreateBoundaries(Shape shape, float scale, Mesh mesh, Material material, GameObject parent)
        {
            var boundaryCount = shape.GetBoundaryPointCount();
            if (boundaryCount > 0)
            {
                CreateLineCylinders(shape.Boundary, scale, mesh, material, parent);
            }

            var holeCount = shape.GetHoleCount();
            if (holeCount > 0)
            {
                var holes = shape.Holes;
                for (int i = 0; i < holeCount; i++)
                {
                    CreateLineCylinders(holes[i], scale, mesh, material, parent);
                }
            }

        }

        /// <summary>
        /// Create the wireframe of a mesh with line cylinders
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="scale"></param>
        /// <param name="material"></param>
        /// <param name="parent"></param>
        protected void CreateWireframe(Mesh mesh, float scale, Mesh wireframeMesh, Material material, GameObject parent)
        {
            var indices = mesh.GetIndices(0);
            var vertices = mesh.vertices;

            var triangleVertices = new Vector3[3];
            for (var i = 0; i < indices.Length; i += 3)
            {
                for (var j = 0; j < 3; j++)
                {
                    triangleVertices[j] = vertices[indices[i + j]];
                }

                CreateLineCylinders(triangleVertices, scale, wireframeMesh, material, parent);
            }
        }
    }
}

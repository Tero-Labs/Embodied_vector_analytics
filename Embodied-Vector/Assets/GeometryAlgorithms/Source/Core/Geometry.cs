using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Core
{
    public class Geometry
    {
        public Vertex[] Vertices { get; set; }
        public int[] Indices { get; set; }
        public MeshTopology Topology { get; set; }
        public Geometry[] Cells { get; set; }

        public Geometry()
        {
            Topology = MeshTopology.Triangles;
        }

        /// <summary>
        /// Creates a unity mesh from the geometry objects
        /// </summary>
        /// <returns></returns>
        public Mesh ToUnityMesh()
        {
            var mesh = new Mesh();

            if (Vertices != null)
            {
                var vectors = new Vector3[Vertices.Length];

                for (int i = 0; i < vectors.Length; i++)
                {
                    vectors[i] = Vertices[i].Position;
                }

                mesh.vertices = vectors;
                if (Indices != null)
                {
                    mesh.SetIndices(Indices, Topology, 0);

                    if (Topology == MeshTopology.Triangles)
                    {
                        mesh.RecalculateNormals();
                    }
                }
            }

            return mesh;
        }
    }

    public class Vertex
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public Vector3 Position { get; set; }

        public Vertex(double x, double y, double z, int id) : this(x, y, z)
        {
            Id = id;
        }

        public Vertex(double x, double y, double z)
        {
            Position = new Vector3((float)x, (float)y, (float)z);
            Index = -1;
        }

        public Vertex() : this(0, 0, 0)
        {

        }

        public bool Equals(Vertex v)
        {
            return Id == v.Id;
        }
    }
}

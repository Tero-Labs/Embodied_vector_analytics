using Jobberwocky.GeometryAlgorithms.Source.Core;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Parameters
{
    public class Triangulation2DParameters : Parameters
    {
        /// <summary>
        /// Regular points for triangulation
        /// </summary>
        public Vector3[] Points { get; set; }
        /// <summary>
        /// The boundary points if any for the triangulation
        /// </summary>
        public Vector3[] Boundary { get; set; }

        /// <summary>
        /// The holes and hole points if any for the triangulation
        /// </summary>
        public Vector3[][] Holes { get; set; }

        /// <summary>
        /// Whether the triangulation should be Delaunay. 
        /// Extra points could be added automatically to confirm Delaunay
        /// </summary>
        public bool Delaunay { get; set; }

        /// <summary>
        /// Defines the side that should be triangulated
        /// </summary>
        public Side Side { get; set; }

        public Triangulation2DParameters() : base()
        {
            Delaunay = false;
            Side = Side.Front;
        }
    }
}

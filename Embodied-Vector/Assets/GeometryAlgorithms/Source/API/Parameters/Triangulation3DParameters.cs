using Jobberwocky.GeometryAlgorithms.Source.Core;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Parameters
{
    public class Triangulation3DParameters : Parameters
    {
        /// <summary>
        /// Points used for the triangulation
        /// </summary>
        public Vector3[] Points { get; set; }
        /// <summary>
        /// Whether only the boundary should be triangulated
        /// </summary>
        public bool BoundaryOnly { get; set; }
        /// <summary>
        /// Defines which side should be triangulated
        /// </summary>
        public Side Side { get; set; }

        public Triangulation3DParameters() : base()
        {
            BoundaryOnly = false;
            Side = Side.Front;
        }
    }
}

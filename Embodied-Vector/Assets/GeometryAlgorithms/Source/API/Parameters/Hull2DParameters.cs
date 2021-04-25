using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Parameters
{
    public class Hull2DParameters : Parameters
    {
        /// <summary>
        /// Points used for the hull generation
        /// </summary>
        public Vector3[] Points { get; set; }
        /// <summary>
        /// The concavity or max edge length that is allowed when a hull is generated
        /// Provding the max value for the maxEdgeLength results in the convex hull of the input points.
        /// A lower value results in a tighter concave hull, but also increases the calculation time.
        /// </summary>
        public double Concavity { get; set; }

        public Hull2DParameters() : base()
        {
            Concavity = double.MaxValue;
        }
    }
}

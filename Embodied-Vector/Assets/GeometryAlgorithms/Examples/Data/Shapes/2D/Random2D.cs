using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Random2D : Shape
    {
        public Random2D()
        {
            Points = CreateRandomPoints2D(100, 12, 12);

            CameraPoint = new Vector3(0, 0, -12);
            CameraRotation = new Quaternion(0, 0, 0, 1);
        }

        /// <summary>
        /// Generate points on random locations in 2D
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateRandomPoints2D(int nPoints, float rangeWidth, float rangeHeight)
        {
            Vector3[] points = new Vector3[nPoints];
            float x, y;
            for (int i = 0; i < nPoints; i++)
            {
                x = Random.Range(rangeWidth * -.5f, rangeWidth * .5f);
                y = Random.Range(rangeHeight * -.5f, rangeHeight * .5f);
                points[i] = new Vector3(x, y, 0);
            }

            return points;
        }
    }
}

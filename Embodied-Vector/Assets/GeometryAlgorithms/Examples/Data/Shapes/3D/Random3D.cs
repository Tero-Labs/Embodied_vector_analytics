using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Random3D : Shape
    {
        public Random3D()
        {
            Points = CreateRandomPoints3D(100, 10, 5, 8);

            CameraPoint = new Vector3(-4.5f, 2, -10);
            CameraRotation = Quaternion.Euler(15, 30, 0);
        }

        /// <summary>
        /// Generate points on random locations in 3D
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateRandomPoints3D(int nPoints, float rangeWidth, float rangeHeight, float rangeDepth)
        {
            Random.InitState(11);

            Vector3[] points = new Vector3[nPoints];
            float x, y, z;
            for (int i = 0; i < nPoints; i++)
            {
                x = Random.Range(rangeWidth * -.5f, rangeWidth * .5f);
                y = Random.Range(rangeHeight * -.5f, rangeHeight * .5f);
                z = Random.Range(rangeDepth * -.5f, rangeDepth * .5f);
                points[i] = new Vector3(x, y, z);
            }

            return points;
        }
    }
}

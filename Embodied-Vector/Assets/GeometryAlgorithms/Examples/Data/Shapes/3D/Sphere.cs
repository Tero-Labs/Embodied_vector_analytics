using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Sphere : Shape
    {
        public Sphere()
        {
            Points = CreateSphere(10, 16, 16);

            CameraPoint = new Vector3(-12, 11, -16);
            CameraRotation = Quaternion.Euler(30, 45, 0);
        }

        /// <summary>
        /// Method for the generation of points that represent a sphere
        /// </summary>
        /// <param name="r"></param>
        /// <param name="lats"></param>
        /// <param name="longs"></param>
        /// <returns></returns>
        private Vector3[] CreateSphere(float r, int lats, int longs)
        {
            Vector3[] points = new Vector3[(lats + 1) * (longs + 1) * 2];

            int i, j;
            int index = 0;
            for (i = 0; i <= lats; i++)
            {
                float lat0 = Mathf.PI * (-0.5f + (i - 1) / (float)lats);
                float z0 = Mathf.Sin(lat0);
                float zr0 = Mathf.Cos(lat0);

                float lat1 = Mathf.PI * (-0.5f + i / lats);
                float z1 = Mathf.Sin(lat1);
                float zr1 = Mathf.Cos(lat1);

                for (j = 0; j <= longs; j++)
                {
                    float lng = 2 * Mathf.PI * (j - 1) / longs;
                    float x = Mathf.Cos(lng);
                    float y = Mathf.Sin(lng);

                    points[index] = new Vector3(x * zr0, y * zr0, z0) * r;
                    index++;
                    points[index] = new Vector3(x * zr1, y * zr1, z1) * r;
                    index++;
                }
            }

            return points;
        }
    }
}

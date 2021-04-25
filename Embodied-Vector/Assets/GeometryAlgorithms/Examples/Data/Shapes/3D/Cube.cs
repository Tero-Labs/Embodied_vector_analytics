using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Cube : Shape
    {
        public Cube()
        {
            Points = CreateCube(7, 6, 4, 2.9f);

            Random.InitState(1);
            for (int i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                
                p.Set(p.x + Random.value, p.y + Random.value, p.z + Random.value);

                Points[i] = p;
            }

            CameraPoint = new Vector3(-15, 10, -20);
            CameraRotation = Quaternion.Euler(30, 45, 0);
        }

        /// <summary>
        /// Method to generate the points for a cube
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateCube(int nPointsWidth, int nPointsHeight, int nPointsDepth, float scale)
        {
            Vector3[] points = new Vector3[nPointsWidth * nPointsHeight * nPointsDepth];

            int index = 0;
            float x, y, z;
            for (int i = 0; i < nPointsWidth; i++)
            {
                x = (i - nPointsWidth * .5f) * scale;
                for (int j = 0; j < nPointsHeight; j++)
                {
                    y = (j - nPointsHeight * .5f) * scale;
                    for (int k = 0; k < nPointsDepth; k++)
                    {
                        z = (k - nPointsDepth * .5f) * scale;
                        points[index] = new Vector3(x, y, z);
                        index++;
                    }
                }
            }

            return points;
        }
    }
}

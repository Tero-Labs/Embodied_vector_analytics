using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class ShapeGenerator
    {
        public ShapeGenerator()
        {

        }

        // http://answers.unity3d.com/questions/944228/creating-a-smooth-round-flat-circle.html
        /// <summary>
        /// Generate points in a circular shape
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateCircle(float scale, int nPoints)
        {
            Vector3[] points = new Vector3[nPoints];

            float angleStep = 360.0f / nPoints;
            Quaternion quaternion = Quaternion.Euler(0.0f, 0.0f, angleStep);
            points[0] = scale * new Vector3(0.0f, 0.5f, 0.0f);  // 1. First vertex on circle outline (radius = 0.5f)

            for (int i = 0; i < nPoints - 1; i++)
            {
                points[i + 1] = quaternion * points[i];
            }

            return points;
        }

        /// <summary>
        /// Generate points in a grid form
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateGrid(int nPointsWidth, int nPointsHeight, float scale)
        {
            Vector3[] points = new Vector3[nPointsWidth * nPointsHeight];

            int index = 0;
            float x, y;
            for (int i = 0; i < nPointsWidth; i++)
            {
                x = (i - nPointsWidth * .5f) * scale;
                for (int j = 0; j < nPointsHeight; j++)
                {
                    y = (j - nPointsHeight * .5f) * scale;
                    points[index] = new Vector3(x, y, 0);
                    index++;
                }
            }

            return points;
        }

        /// <summary>
        /// Generate points on random locations
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateRandomPoints2D(int nPoints, float rangeWidth, float rangeHeight)
        {
            Random.InitState(1);
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

        /// <summary>
        /// Generate points on random locations
        /// </summary>
        /// <returns></returns>
        public Vector3[] CreateRandomPoints3D(int nPoints, float rangeWidth, float rangeHeight, float rangeDepth)
        {
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

        /// <summary>
        /// Method that generates points for a star pattern
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Vector3[] CreateStar(float scale)
        {
            Vector3[] points = new Vector3[16] {
                scale * (new Vector3(0, 0.5f * (2 - Mathf.Sqrt(2)), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0.5f * (2 - Mathf.Sqrt(2)), 0.5f * (2 - Mathf.Sqrt(2)), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0.5f * (2 - Mathf.Sqrt(2)), 0, 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0.5f, (0.5f * (2 - Mathf.Sqrt(2))) * (2.0f / 3.0f), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1 - (0.5f * (2 - Mathf.Sqrt(2))), 0, 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1 - (0.5f * (2 - Mathf.Sqrt(2))), 0.5f * (2 - Mathf.Sqrt(2)), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1, 0.5f * (2 - Mathf.Sqrt(2)), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1 - ((0.5f * (2 - Mathf.Sqrt(2))) * (2f / 3f)), 0.5f, 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1, 1 - (0.5f * (2 - Mathf.Sqrt(2))), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1 - (0.5f * (2 - Mathf.Sqrt(2))), 1 - (0.5f * (2 - Mathf.Sqrt(2))), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(1 - (0.5f * (2 - Mathf.Sqrt(2))), 1, 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0.5f, 1 - ((0.5f * (2 - Mathf.Sqrt(2))) * (2f / 3f)), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0.5f * (2 - Mathf.Sqrt(2)), 1, 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0.5f * (2 - Mathf.Sqrt(2)), 1 - (0.5f * (2 - Mathf.Sqrt(2))), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3(0, 1 - (0.5f * (2 - Mathf.Sqrt(2))), 0) - new Vector3(0.5f, 0.5f)),
                scale * (new Vector3((0.5f * (2 - Mathf.Sqrt(2))) * (2f / 3f), 0.5f, 0) - new Vector3(0.5f, 0.5f)),
            };

            return points;
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

        /// <summary>
        /// Method for the generation of points that represent a sphere
        /// </summary>
        /// <param name="r"></param>
        /// <param name="lats"></param>
        /// <param name="longs"></param>
        /// <returns></returns>
        public Vector3[] CreateSphere(float r, int lats, int longs)
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

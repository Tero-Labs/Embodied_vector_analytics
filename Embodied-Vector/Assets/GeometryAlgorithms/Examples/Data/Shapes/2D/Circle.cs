using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Circle : Shape
    {
        public Circle()
        {
            float scale = 60;

            var tempPoints = new List<Vector3>();
            for (var i = 2; i < 7; i++)
            {
                tempPoints.AddRange(CreateCircle(scale * i, 72));
            }

            Points = tempPoints.ToArray();
            Holes = new Vector3[1][];
            Holes[0] = CreateCircle(scale, 72);

            CameraPoint = new Vector3(100, 0, -500);
            CameraRotation = new Quaternion(0, 0, 0, 1);
        }

        // http://answers.unity3d.com/questions/944228/creating-a-smooth-round-flat-circle.html
        /// <summary>
        /// Generate points in a circular shape
        /// </summary>
        /// <returns></returns>
        private Vector3[] CreateCircle(float scale, int nPoints)
        {
            Vector3[] points = new Vector3[nPoints];

            float angleStep = 360.0f / nPoints;
            Quaternion quaternion = Quaternion.Euler(0.0f, 0.0f, angleStep);

            points[0] = scale * new Vector3(0.0f, 0.5f, 0.0f);  // 1. First vertex on circle outline (radius = 0.5f)
            points[1] = quaternion * points[0] * 0.95f;

            quaternion.eulerAngles *= 2;
            for (int i = 1; i < nPoints - 1; i++)
            {
                points[i + 1] = quaternion * points[i - 1];
            }

            return points;
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Shape
    {
        public Vector3[] Points { get; set; }
        public Vector3[] Boundary { get; set; }
        public Vector3[][] Holes { get; set; }

        public Vector3 CameraPoint { get; set; }
        public Quaternion CameraRotation { get; set; }

        /// <summary>
        /// Returns an array with all the points, holes, and boundary points
        /// </summary>
        /// <returns>Vector3[]</returns>
        public Vector3[] GetAllPoints()
        {
            var pointCount = GetPointCount();
            var boundaryPointCount = GetBoundaryPointCount();
            var holesPointCount = GetHolesPointCount();
            var totalPoints = pointCount + boundaryPointCount + holesPointCount;

            Vector3[] allPoints = null;
            if (totalPoints > 0)
            {
                allPoints = new Vector3[totalPoints];


                var index = 0;
                for (var i = 0; i < pointCount; i++)
                {
                    allPoints[index++] = Points[i];
                }

                for (var i = 0; i < boundaryPointCount; i++)
                {
                    allPoints[index++] = Boundary[i];
                }

                for (var i = 0; i < GetHoleCount(); i++)
                {
                    for (var j = 0; j < Holes[i].Length; j++)
                    {
                        allPoints[index++] = Holes[i][j];
                    }
                }
            }

            return allPoints;
        }


        /// <summary>
        /// Returns the number of boundary points
        /// </summary>
        /// <returns>int</returns>
        public int GetBoundaryPointCount()
        {
            int count = 0;
            if (Boundary != null)
            {
                count += Boundary.Length;
            }

            return count;
        }

        /// <summary>
        /// Returns the total number of hole points
        /// </summary>
        /// <returns>int</returns>
        public int GetHolesPointCount()
        {
            int count = 0;
            for (var i = 0; i < GetHoleCount(); i++)
            {
                count += Holes[i].Length;
            }


            return count;
        }

        /// <summary>
        /// Return the point count
        /// </summary>
        /// <returns>int</returns>
        public int GetPointCount()
        {
            int count = 0;
            if (Points != null)
            {
                count += Points.Length;
            }

            return count;
        }

        /// <summary>
        /// Returns the number of holes
        /// </summary>
        /// <returns>int</returns>
        public int GetHoleCount()
        {
            int count = 0;
            if (Holes != null)
            {
                count += Holes.Length;
            }

            return count;
        }

        public void LoadDataFromFile(string path)
        {
            var textFile = File.ReadAllText(path);

            var textParts = textFile.Split('#');
            for (var i = 0; i < textParts.Length; i++)
            {
                var text = textParts[i];
                if (text.StartsWith("Boundary"))
                {
                    Boundary = GetPointsFromString(text);
                }
                else if (text.StartsWith("Holes"))
                {
                    var textHoles = text.Split('&');

                    var holes = new Vector3[textHoles.Length][];
                    for (var j = 0; j < textHoles.Length; j++)
                    {
                        holes[j] = GetPointsFromString(textHoles[j]);
                    }

                    Holes = holes;
                }
                else if (text.StartsWith("Points"))
                {
                    Points = GetPointsFromString(text);
                }
                else
                {
                    continue;
                }
            }
        }

        private Vector3[] GetPointsFromString(string textPoints)
        {
            var listOfTextPoints = textPoints.Split('\n');

            var points = new List<Vector3>(listOfTextPoints.Length);
            for (var i = 0; i < listOfTextPoints.Length; i++)
            {
                var point = new Vector3();
                var textPoint = listOfTextPoints[i];
                var coordinatesOfTextPoint = textPoint.Split(' ');
                if (coordinatesOfTextPoint.Length < 2)
                {
                    continue;
                }

                for (var j = 0; j < coordinatesOfTextPoint.Length; j++)
                {
                    if (j == 0)
                    {
                        point.x = float.Parse(coordinatesOfTextPoint[j]);
                    }
                    else if (j == 1)
                    {
                        point.y = float.Parse(coordinatesOfTextPoint[j]);
                    }
                    else if (j == 2)
                    {
                        point.z = float.Parse(coordinatesOfTextPoint[j]);
                    }
                }

                points.Add(point);
            }

            return points.ToArray();
        }
    }
}
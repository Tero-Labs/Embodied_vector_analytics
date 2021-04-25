using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Horse13k : Shape
    {
        public Horse13k()
        {
            LoadDataFromFile("Assets/GeometryAlgorithms/Examples/Data/Shapes/2D/Horse13k.txt");

            CameraPoint = new Vector3(375, 400, -700);
            CameraRotation = Quaternion.Euler(0, 0, 180);
        }

    }
}

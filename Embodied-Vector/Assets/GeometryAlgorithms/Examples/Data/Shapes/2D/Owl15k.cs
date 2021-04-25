using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Owl15k : Shape
    {
        public Owl15k()
        {
            LoadDataFromFile("Assets/GeometryAlgorithms/Examples/Data/Shapes/2D/Owl15k.txt");

            CameraPoint = new Vector3(325, 530, -820);
            CameraRotation = Quaternion.Euler(0, 0, 180);
        }
    }
}

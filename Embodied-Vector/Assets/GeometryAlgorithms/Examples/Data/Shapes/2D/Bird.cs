using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Bird : Shape
    {
        public Bird()
        {
            LoadDataFromFile("Assets/GeometryAlgorithms/Examples/Data/Shapes/2D/Bird.txt");

            CameraPoint = new Vector3(150, 0, -800);
            CameraRotation = new Quaternion(0, 0, 0, 1);
        }
    }
}

using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Tank : Shape
    {
        public Tank()
        {
            LoadDataFromFile("Assets/GeometryAlgorithms/Examples/Data/Shapes/2D/Tank.txt");

            CameraPoint = new Vector3(375, 0, -600);
            CameraRotation = new Quaternion(0, 0, 0, 1);
        }
    }
}

using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Examples.Data
{
    public class Dude : Shape
    {
        public Dude()
        {
            LoadDataFromFile("Assets/GeometryAlgorithms/Examples/Data/Shapes/2D/Dude.txt");
            
            CameraPoint = new Vector3(380, 0, -320);
            CameraRotation = new Quaternion(0, 0, 0, 1);
        }
    }
}

using UnityEngine;

namespace Jobberwocky.GeometryAlgorithms.Source.Core
{
    class Utils
    {
        public static Vector3 ChangeVectorCoordinateOrder(Vector3 vector, Order order)
        {
            var newVector = new Vector3();

            switch (order)
            {
                case Order.XZY:
                    newVector = new Vector3(vector.x, vector.z, vector.y);
                    break;
                case Order.YXZ:
                    newVector = new Vector3(vector.y, vector.x, vector.z);
                    break;
                case Order.YZX:
                    newVector = new Vector3(vector.y, vector.z, vector.x);
                    break;
                case Order.ZXY:
                    newVector = new Vector3(vector.z, vector.x, vector.y);
                    break;
                case Order.ZYX:
                    newVector = new Vector3(vector.z, vector.y, vector.x);
                    break;
                case Order.XYZ:
                default:
                    newVector = vector;
                    break;
            }

            return newVector;
        }
    }
}

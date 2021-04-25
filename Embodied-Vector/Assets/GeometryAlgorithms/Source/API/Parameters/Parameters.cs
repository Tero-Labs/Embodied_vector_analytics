using Jobberwocky.GeometryAlgorithms.Source.Core;

namespace Jobberwocky.GeometryAlgorithms.Source.Parameters
{
    public abstract class Parameters : IParameters
    {
        public Order Order { get; set; }

        public Parameters()
        {
            Order = Order.XYZ;
        }
    }
}

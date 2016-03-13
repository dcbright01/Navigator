using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Navigator.Primitives;

namespace Navigator.Pathfinding
{
    /// <summary>
    /// Class allowing for simple translation to the graph (includes floor its on)
    /// </summary>
    public class GraphLocatable : Vector2
    {
        public int Floor { get; private set; }
        public GraphLocatable(float x, float y, int floor) : base(x, y)
        {
            Floor = floor;
        }

        public GraphLocatable(string pointString, int floor) : base(pointString)
        {
            Floor = floor;
        }

        public GraphLocatable(float[] values, int floor) : base(values)
        {
            Floor = floor;
        }
    }
}

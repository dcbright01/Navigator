using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Navigator.Pathfinding.Graph
{
    public class Vertex : GraphIdentifiable, IComparable
    {
        public Vertex()
        {
            Room = "";
            X = 0;
            Y = 0;
        }

        [XmlAttribute("RoomName")]
        public string Room { get; set; }

        [XmlAttribute("X")]
        public int X { get; set; }

        [XmlAttribute("Y")]
        public int Y { get; set; }

        public int CompareTo(object obj)
        {
            var other = (Vertex)obj;
            if (other.getGraphId() == getGraphId())
                return 0;
            return 1;
        }

        public override string getGraphId()
        {
            return string.Format("{0}-{1}", X, Y);
        }

        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} Room:{2}", X, Y, Room);
        }

        public double DistanceTo(Vertex other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }
}

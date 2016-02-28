using System.Collections.Generic;
using System.Xml.Serialization;
using QuickGraph;

namespace Navigator.Pathfinding
{
    public class GraphData
    {
        public List<UndirEdge> Edges = new List<UndirEdge>();
        public List<string> Vertices = new List<string>();
        public List<Room> Rooms = new List<Room>();
    }

    [XmlRoot("E")]
    public class UndirEdge : UndirectedEdge<string>
    {
        public UndirEdge() : base("", "")
        {
        }

        public UndirEdge(string source, string target) : base(source, target)
        {
            Source = source;
            Target = target;
        }

        [XmlAttribute("Source")]
        public new string Source { get; set; }

        [XmlAttribute("Target")]
        public new string Target { get; set; }
    }

    public class Room {

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Position")]
        public string Position { get; set; }
    }
}
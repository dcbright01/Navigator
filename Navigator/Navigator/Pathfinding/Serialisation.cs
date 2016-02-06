using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using QuickGraph;

namespace Navigator.Pathfinding
{
    public class GraphData
    {
        public List<string> Vertices = new List<string>();
        public List<UndirEdge> Edges = new List<UndirEdge>();
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
}

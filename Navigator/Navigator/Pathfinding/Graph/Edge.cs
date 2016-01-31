using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Navigator.Pathfinding.Graph
{
    public class Edge : GraphIdentifiable
    {
        public Edge()
        {
            Source = "";
            Target = "";
            Weight = 1;
        }

        [XmlAttribute("Source")]
        public string Source { get; set; }

        [XmlAttribute("Target")]
        public string Target { get; set; }

        [XmlAttribute("Weight")]
        public int Weight { get; set; }

        public override string getGraphId()
        {
            return string.Format("{0}-{1}", Source, Target);
        }
    }
}

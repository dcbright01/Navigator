using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using QuickGraph;
using Navigator.Primitives;

namespace Navigator.Pathfinding
{
    public class GraphData
    {
        public List<UndirEdge> Edges = new List<UndirEdge>();
        public List<string> Vertices = new List<string>();
        public List<Room> Rooms = new List<Room>();
    }

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
        public List<RoomProperty> Properties { get; set; }

        [XmlIgnore]
        public bool IsStairs {
            get { return Properties.Any(x => x.Type == RoomPropertyType.Stairs && x.Value == "true"); }}

        [XmlIgnore]
        public int Floor
        {
            get { return int.Parse(Properties.First(x => x.Type == RoomPropertyType.Floor).Value); }
        }

		[XmlIgnore]
		public Vector2 Position
		{
			get { return new Vector2(Properties.First(x => x.Type == RoomPropertyType.Position).Value); }
		}

		[XmlIgnore]
		public string Name
		{
			get { return Properties.First(x => x.Type == RoomPropertyType.Name).Value; }
		}

        public Room() {
            Properties = new List<RoomProperty>();
        }
    }

    public class RoomProperty {

        [XmlAttribute("type")]
        public RoomPropertyType Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }


        public RoomProperty(RoomPropertyType type, string val) {
            Type = type;
            Value = val;
        }

        public RoomProperty() {
            Type = RoomPropertyType.None;
            Value = "";
        }
    }
}
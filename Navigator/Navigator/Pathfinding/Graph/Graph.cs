using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace Navigator.Pathfinding.Graph
{
    public class Graph
    {
        [XmlIgnore]
        private UndirectedGraph<Vertex, UndirectedEdge<Vertex>> _pathfindingGraph;

        public SerializeableKeyValue<string, string>[] Rooms
        {
            get
            {
                var list = new List<SerializeableKeyValue<string, string>>();
                if (RoomsDict != null)
                {
                    list.AddRange(RoomsDict.Keys.Select(key => new SerializeableKeyValue<string, string>() { Key = key, Value = RoomsDict[key] }));
                }
                return list.ToArray();
            }
            set
            {
                RoomsDict = new Dictionary<string, string>();
                foreach (var item in value)
                {
                    RoomsDict.Add(item.Key, item.Value);
                }
            }
        }

        public List<Edge> Edges = new List<Edge>();
        [XmlIgnore]
        public Dictionary<string, string> RoomsDict = new Dictionary<string, string>();

        public Graph()
        {
            // Hardcoded for now, will fix it after testing
            RoomsDict.Add("Color [A=255, R=57, G=191, B=4]", "Hallway");
            RoomsDict.Add("Color [A=255, R=168, G=176, B=236]", "CS0.01");
            RoomsDict.Add("Color [A=255, R=110, G=112, B=109]", "CS0.02");
            RoomsDict.Add("Color [A=255, R=208, G=212, B=206]", "CS0.03");
            RoomsDict.Add("Color [A=255, R=156, G=159, B=155]", "CS0.04");
            RoomsDict.Add("Color [A=255, R=244, G=58, B=58]", "CS0.05");
            RoomsDict.Add("Color [A=255, R=246, G=142, B=142]", "CS0.06");
            RoomsDict.Add("Color [A=255, R=255, G=102, B=9]", "CS0.07");
            RoomsDict.Add("Color [A=255, R=255, G=137, B=65]", "CS0.08");
            RoomsDict.Add("Color [A=255, R=162, G=160, B=8]", "CS0.09");
            RoomsDict.Add("Color [A=255, R=121, G=120, B=4]", "CS0.10");
            RoomsDict.Add("Color [A=255, R=246, G=243, B=18]", "Cupboard");
            RoomsDict.Add("Color [A=255, R=244, G=242, B=17]", "Cupboard");
            RoomsDict.Add("Color [A=255, R=241, G=241, B=17]", "Cupboard");
            RoomsDict.Add("Color [A=255, R=237, G=17, B=17]", "Elevator");
            RoomsDict.Add("Color [A=255, R=53, G=54, B=53]", "Lab 3");
            RoomsDict.Add("Color [A=255, R=54, G=54, B=54]", "Lab 3");
            RoomsDict.Add("Color [A=255, R=122, G=17, B=17]", "Staircase");
            RoomsDict.Add("Color [A=255, R=16, G=40, B=222]", "Male toilets");
            RoomsDict.Add("Color [A=255, R=105, G=121, B=241]", "Female toilets");
            RoomsDict.Add("Color [A=255, R=54, G=77, B=243]", "Disabled toilets");
            RoomsDict.Add("Color [A=255, R=0, G=215, B=152]", "Test1");
            RoomsDict.Add("Color [A=255, R=134, G=171, B=196]", "Test2");
        }

        public List<Vertex> Vertices = new List<Vertex>();

        public void SetUpPathfinding()
        {
            _pathfindingGraph = new UndirectedGraph<Vertex, UndirectedEdge<Vertex>>();
            foreach (var vertex in Vertices)
            {
                _pathfindingGraph.AddVertex(vertex);
            }
            foreach (var edge in Edges)
            {
                var source = Vertices.First(x => x.getGraphId() == edge.Source);
                var target = Vertices.First(x => x.getGraphId() == edge.Target);
                _pathfindingGraph.AddEdge(new UndirectedEdge<Vertex>(source, target));
            }
        }

        /// <summary>
        /// Stream needs to be passed in from specific device as they differ 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static Graph Load(Stream inputStream)
        {
            var reader = new XmlSerializer(typeof(Graph));
            var deserialized =  reader.Deserialize(inputStream) as Graph;
            deserialized.SetUpPathfinding();
            return deserialized;
        }

        /// <summary>
        /// Finds a path between start and end vertices. It can return NULL if no path if found , check your output
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<UndirectedEdge<Vertex>> FindPath(Vertex start, Vertex end)
        {
            Func<UndirectedEdge<Vertex>, double> edgeCost = e => 1; // constant cost
                                                                    // We want to use Dijkstra on this graph
            var dijkstra = new UndirectedDijkstraShortestPathAlgorithm<Vertex, UndirectedEdge<Vertex>>(_pathfindingGraph, edge => (double)edge.Source.DistanceTo(edge.Target));
            var observer = new UndirectedVertexPredecessorRecorderObserver<Vertex, UndirectedEdge<Vertex>>();
            observer.Attach(dijkstra);
            dijkstra.Compute(start);
            IEnumerable<UndirectedEdge<Vertex>> path = null;
            try
            {
                observer.TryGetPath(end, out path);
            }
            catch { }
            return path.ToList();
        }
    }
}

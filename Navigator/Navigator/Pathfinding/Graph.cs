using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace Navigator.Pathfinding
{
	public interface IGraph 
	{
		List<UndirEdge> FindPath (string start, string end);
		Tuple<int, int> FindClosestNode (float searchX, float searchY, int searchDistance);

	}

	public class Graph : UndirectedGraph<string, UndirEdge>, IGraph
    {
        
        public Graph()
        {

        }

        /// <summary>
        /// Stream needs to be passed in from specific device as they differ 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static Graph Load(Stream inputStream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(GraphData));
            GraphData data = new GraphData();
            using (XmlReader reader = XmlReader.Create(inputStream))
            {
                data = (GraphData)ser.Deserialize(reader);
            }
            Graph g = new Graph();
            
            g.AddVertexRange(data.Vertices);
            
            List<UndirEdge> ed = new List<UndirEdge>();
            foreach (var undirEdge in data.Edges)
            {
                ed.Add(new UndirEdge(undirEdge.Source, undirEdge.Target));
            }
            g.AddEdgeRange(ed);

            return g;
        }

        /// <summary>
        /// Finds a path between start and end vertices. It can return NULL if no path if found , check your output
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<UndirEdge> FindPath(string start, string end)
        {
            var dijkstra = new UndirectedDijkstraShortestPathAlgorithm<string, UndirEdge>(this, edge => 1);
            var observer = new UndirectedVertexPredecessorRecorderObserver<string, UndirEdge>();
            observer.Attach(dijkstra);
            dijkstra.Compute(start);
            IEnumerable<UndirEdge> path = null;
            try
            {
                observer.TryGetPath(end, out path);
            }
            catch { }
            return path.ToList();
        }

		public Tuple<int, int> FindClosestNode(float searchX, float searchY, int searchDistance){
			Tuple<int, int> tempNode = new Tuple<int, int>(-1, -1);
			double distanceFromTempToReal = -1;
			double a, b, newDistance;

			string nodeCoords;

			//there is a more effecient search that would spiral outwards to a set point before searching the corners of the sqaure that this gets, can implement if need be
			for (int x = (int) searchX - searchDistance; x <= searchX + searchDistance; x++)
			{
				for(int y = (int) searchY - searchDistance; y <= searchY + searchDistance; y++)
				{
					nodeCoords = x.ToString() + "-" + y.ToString();

					string nodeCheck = Vertices.FirstOrDefault(node => node == nodeCoords);
					if(nodeCheck != null)
					{
						a = y - searchY;
						b = x - searchX;
						newDistance = Math.Sqrt((a * a) + (b * b));
						if (distanceFromTempToReal == -1 || newDistance < distanceFromTempToReal)
						{//first node come across is set as the tempNode or if newDistance is smaller
							tempNode = Tuple.Create(x, y);
							distanceFromTempToReal = newDistance;
						} //shouldn't need to deal with case where you are the same distance from multiple nodes, as it won't really effect navigation and rules, if required can be added
					}                                        
				}
			}
			return tempNode;

		}
    }
}

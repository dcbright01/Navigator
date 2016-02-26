using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Navigator.Primitives;
using QuickGraph;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;

namespace Navigator.Pathfinding
{
    public interface IGraph
    {
        List<UndirEdge> FindPath(string start, string end);
        Vector2 FindClosestNode(float searchX, float searchY, int searchDistance);
    }

    public class Graph : UndirectedGraph<string, UndirEdge>, IGraph
    {
        /// <summary>
        ///     Finds a path between start and end vertices. It can return NULL if no path if found , check your output
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<UndirEdge> FindPath(string start, string end)
        {
			var containsStart = Vertices.Contains(start);
			var containsEnd = Vertices.Contains(end);

            var dijkstra = new UndirectedDijkstraShortestPathAlgorithm<string, UndirEdge>(this, edge => 1);
            var observer = new UndirectedVertexPredecessorRecorderObserver<string, UndirEdge>();
            observer.Attach(dijkstra);
            dijkstra.Compute(start);
            IEnumerable<UndirEdge> path = null;
            try
            {
                observer.TryGetPath(end, out path);
            }
            catch
            {
            }
            return path.ToList();
        }

        public Vector2 FindClosestNode(float searchX, float searchY, int searchDistance)
        {
            var tempNode = new Vector2(-1, -1);
            double distanceFromTempToReal = -1;
            double a, b, newDistance;
			int tempX, tempY;
            string nodeCoords;

			int moduloX = (int)searchX % 10;
			int moduloY = (int)searchY % 10;

			if (moduloX == 6)
				tempX = (int)searchX;
			else if (moduloX > 1)
				tempX = (int)searchX - moduloX + 6;
			else
				tempX = (int)searchX - moduloX - 4;

			if (moduloY == 0)
				tempY = (int)searchY;
			else if (moduloY >= 5)
				tempY = (int)searchY + (10 - moduloY);
			else
				tempY = (int)searchY - moduloY;

			tempNode = new Vector2 (tempX, tempY);

			return tempNode;


            //there is a more effecient search that would spiral outwards to a set point before searching the corners of the sqaure that this gets, can implement if need be
            
			/*for (var x = (int) searchX - searchDistance; x <= searchX + searchDistance; x++)
            {
                for (var y = (int) searchY - searchDistance; y <= searchY + searchDistance; y++)
                {
                    nodeCoords = x + "-" + y;

                    var nodeCheck = Vertices.FirstOrDefault(node => node == nodeCoords);
                    if (nodeCheck != null)
                    {
                        a = y - searchY;
                        b = x - searchX;
                        newDistance = Math.Sqrt(a*a + b*b);
                        if (distanceFromTempToReal == -1 || newDistance < distanceFromTempToReal)
                        {
//first node come across is set as the tempNode or if newDistance is smaller
                            tempNode = new Vector2(x, y);
                            distanceFromTempToReal = newDistance;
                        }
                            //shouldn't need to deal with case where you are the same distance from multiple nodes, as it won't really effect navigation and rules, if required can be added
                    }
                }
            }
            return tempNode;
            */
        }

        /// <summary>
        ///     Stream needs to be passed in from specific device as they differ
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static Graph Load(Stream inputStream)
        {
            var ser = new XmlSerializer(typeof (GraphData));
            var data = new GraphData();
            using (var reader = XmlReader.Create(inputStream))
            {
                data = (GraphData) ser.Deserialize(reader);
            }
            var g = new Graph();

            g.AddVertexRange(data.Vertices);

            var ed = new List<UndirEdge>();
            foreach (var undirEdge in data.Edges)
            {
                ed.Add(new UndirEdge(undirEdge.Source, undirEdge.Target));
            }
            g.AddEdgeRange(ed);

            return g;
        }

        public string FindClosestNode(int x, int y)
        {
            return Vertices.OrderBy(n => NodeDistance(string.Format("{0}-{1}", x, y), n)).First();
        }

        private float NodeDistance(string n1, string n2)
        {
            var node1 = new Vector2(n1);
            var node2 = new Vector2(n2);
            return node1.Distance2D(node2);
        }
    }
}
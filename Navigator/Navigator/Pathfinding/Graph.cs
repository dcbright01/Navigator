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
using System.Diagnostics;

namespace Navigator.Pathfinding
{
    public interface IGraph
    {
        List<UndirEdge> FindPath(string start, string end);
        Vector2 FindClosestNode(float searchX, float searchY);
		bool isLinked (Vector2 pos1, Vector2 pos2);
    }

    public class Graph : UndirectedGraph<string, UndirEdge>, IGraph
    {
        /// <summary>
        ///     Finds a path between start and end vertices. It can return NULL if no path if found , check your output
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>

        public List<Room> Rooms { get; set; }

        public List<UndirEdge> FindPath(string start, string end)
        {
			var containsStart = Vertices.Contains(start);
			var containsEnd = Vertices.Contains(end);

            var dijkstra = new UndirectedDijkstraShortestPathAlgorithm<string, UndirEdge>(this, edge => 1);
            var observer = new UndirectedVertexPredecessorRecorderObserver<string, UndirEdge>();
            observer.Attach(dijkstra);

            var nodeCheckX = Vertices.FirstOrDefault(node => node == start);
            var nodeCheckY = Vertices.FirstOrDefault(node => node == end);

            if (nodeCheckX != null && nodeCheckY != null) {
                dijkstra.Compute (start);
                IEnumerable<UndirEdge> path = null;
                try {
                    observer.TryGetPath (end, out path) ;
				} catch(Exception ex) {
					ex.ToString ();
                }


                return path.ToList ();
            } else {
                return null;
            }
        }
			
        public Vector2 FindClosestNode(float searchX, float searchY)
        {
            var tempNode = new Vector2(-1, -1);
			int tempX, tempY;

			int moduloX = (int)searchX % 20;
			int moduloY = (int)searchY % 20;

			if (moduloX == 6)
				tempX = (int)searchX;
			else if (moduloX > 16)
				tempX = (int)searchX - moduloX + 26;
			else
				tempX = (int)searchX - moduloX + 6;

			if (moduloY == 0)
				tempY = (int)searchY;
			else if (moduloY >= 10)
				tempY = (int)searchY + (20 - moduloY);
			else
				tempY = (int)searchY - moduloY;

			tempNode = new Vector2 (tempX, tempY);

			if (this.ContainsVertex (tempNode.ToPointString ()))
				return tempNode;
			else
				return Vector2.Invalid;

        }

		public bool isLinked(Vector2 pos1, Vector2 pos2)
		{
			var sw = new Stopwatch ();
			sw.Start ();
			var path = FindPath (pos1.ToPointString (), pos2.ToPointString ());
			sw.Stop ();
			long timeTaken = sw.ElapsedMilliseconds;
			if (path.Count <= 3)
				return true;
			else
				return false;
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

            g.Rooms = data.Rooms;

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
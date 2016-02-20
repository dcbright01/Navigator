﻿using System;
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
    public class Graph : UndirectedGraph<string, UndirEdge>
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
    }
}
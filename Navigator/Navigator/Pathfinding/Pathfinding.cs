using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Navigator.Pathfinding
{
    /// <summary>
    /// Class that will be responsible for pathfinding (cross floors and all that) It will also handle loading and unloading of resources
    /// </summary>
    public class Pathfinding
    {
        public int CurrentFloor { get; set; }
        private readonly Dictionary<int,Graph> _floorGraphs = new Dictionary<int, Graph>(); 
        public List<Room> Rooms { get; private set; }
        /// <summary>
        /// Boolean expressing whether all resources are loaded 
        /// </summary>
        public bool Ready { get; private set; }

        public Pathfinding(Dictionary<int, Stream> pathResources,Stream roomsResource)
        {
            Task.Run(() =>
            {
                // Release UI thread as we dont need it to do this
                // Deserialize floors
                foreach (var resource in pathResources)
                {
                    _floorGraphs.Add(resource.Key,Graph.Load(resource.Value));
                }
                // Deserialize rooms
                var ser = new XmlSerializer(typeof(List<Room>));
                try
                {
                    using (var reader = XmlReader.Create(roomsResource))
                    {
                        Rooms = (List<Room>) ser.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    
                }
                Ready = true;
            });
        }

        /// <summary>
        /// Function that will return a path between two points on separate graphs
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Dictionary<int, List<UndirEdge>> FindPath(GraphLocatable from, GraphLocatable to)
        {
            var path = new Dictionary<int, List<UndirEdge>>();
            // Detect if we are chaing floors
            if (from.Floor != to.Floor)
            {
                // Floor change 

                // Find the stairs on current floor
                var stairsCurrentFloor = Rooms.Find(x => x.IsStairs && x.Floor == CurrentFloor);
                var stairsTargetFloor = Rooms.Find(x => x.IsStairs && x.Floor == to.Floor);

                if(stairsCurrentFloor == null || stairsTargetFloor == null)
                    throw new Exception("Couldnt locate any stairs to transition");

                path.Add(from.Floor, _floorGraphs[from.Floor].FindPath(from.ToPointString(), stairsCurrentFloor.Position.ToPointString()));
                path.Add(to.Floor, _floorGraphs[to.Floor].FindPath(stairsTargetFloor.Position.ToPointString(), to.ToPointString()));
            }
            else
            {
                // Same floor navigation
                path.Add(from.Floor, _floorGraphs[from.Floor].FindPath(from.ToPointString(), to.ToPointString()));
            }
            return path;
        }    
    }
}

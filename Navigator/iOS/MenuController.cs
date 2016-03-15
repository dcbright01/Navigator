using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Reflection;
using Navigator.Pathfinding;
using Navigator.Primitives;

namespace Navigator.iOS
{
	partial class MenuController : UIViewController
	{
		public MenuController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad() {

                        //Graph loading code
            var assembly = Assembly.GetExecutingAssembly();
            var asset = assembly.GetManifestResourceStream("Navigator.iOS.Resources.dcsfloorWideDoors.xml");
            Graph floorPlanGraph = Graph.Load(asset);

            string[] roomNames = new string[floorPlanGraph.Rooms.Count];
            for(int i = 0; i < roomNames.Length; i++)
            {
                var properties = floorPlanGraph.Rooms[i].Properties;
                foreach(RoomProperty p in properties)
                {
                    if(p.Type == RoomPropertyType.Name)
                        roomNames[i] = p.Value;
                }
            }

            //string[] tableItems = new string[] {"Select Starting Point","Select Destination","About This App"};
            //menuTable.Source = new TableSource(roomNames);

        
        }
	} 
}

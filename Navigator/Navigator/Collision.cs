using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Navigator.Pathfinding;
using Navigator.Helpers;

namespace Navigator
{
    //enum for sensor type

    //delegates to define the output for events
	public delegate void PositionChangedHandler(object sender, PositionChangedHandlerEventArgs args);
    //public delegate void HeadingHandler(int nHeading);

	public enum SensorType
	{
		Accel,
		Gryo,
		Mag
	};

	public interface ICollision
	{
		event PositionChangedHandler PositionChanged;
		void SetLocation (float startX, float startY);
		void PassSensorReadings (SensorType s, double xVal, double yVal, double zVal);
		void PassHeading (float nHeading);
		void StepTaken (bool startFromStat);
	}

	public class Collision : ICollision
    {

        //Values that are being tracked
        private Tuple<float, float> realPosition;
        private Tuple<int, int> nearestGraphNode;
        private float Heading;

        //Other values
        private const float strideLength = 12.0f;
        private const int searchDistance = 6;

        //path holder
		private Queue<Tuple<int, int>> _graphPath;

        //graph information
		private IGraph _graph;

		//StepDetector Class
		private IStepDetector _stepDetector;
        
        //Require interfaces that values are passed to
        //Additional Interfaces added here
        //StepDetector Interface
        //Heading Interface
        //These have events that this class is subscribed to, upon them being triggered the information should then be used.

        //Event for passing info back to the platforms
        //seperated heading and validMove as unsure what will happen if both interfaces trigger at the same time and try to send the call twice
        //also means that on the platform specific level, you know which has happened rather than having to check the values if you want the info
        //remerging is simple if this isn't needed/the concern is invalid
		public event PositionChangedHandler PositionChanged;
        //public event HeadingHandler newHeading;

		public Collision(IStepDetector stepDetector, IGraph graph)
        {
			_stepDetector = stepDetector;
			_stepDetector.OnStep += StepTaken;
			_graph = graph;
			_graphPath = new Queue<Tuple<int, int>>();

        }

        public void SetLocation(float startX, float startY)
        {
            realPosition = Tuple.Create(startX, startY);
			CalculateNearestNode();
        }

        public void PassSensorReadings(SensorType s, double xVal, double yVal, double zVal)
        {
			switch (s) {
			case SensorType.Accel:
				_stepDetector.passValue (xVal, yVal, zVal);
				break;
			}
        }

		public void PassHeading(float nHeading)
		{
			Heading = nHeading;
		}
			
			
        private int CalculateNearestNode()
        {
			var tempNode = _graph.FindClosestNode (realPosition.Item1, realPosition.Item2, searchDistance);


            //case where this is the initial position, figure out how for initial to avoid wall hopping
            if(nearestGraphNode == null)
            {
                if (tempNode.Item1 != -1 && tempNode.Item2 != -1){
                    nearestGraphNode = tempNode;
                    _graphPath.Enqueue(tempNode);
                    return 0;
                }
            } else if(!nearestGraphNode.Equals(tempNode)) // for the case where its not the initial and the previous value is different from current
            {
                /*start = nearestGraphNode.Item1.ToString() + "-" + nearestGraphNode.Item2.ToString();
                end = tempNode.Item1.ToString() + "-" + tempNode.Item2.ToString();
                var path = g.FindPath(start, end);  
                if(path.Count < 3)
                {*/
                    if(_graphPath.Count != null)
                    {
                        if (_graphPath.Count == 5)
                        {
                            _graphPath.Dequeue();
                        }
                    }
                    if (tempNode.Item1 != -1 && tempNode.Item2 != -1) {
                        _graphPath.Enqueue(tempNode);
                        nearestGraphNode = tempNode;
                        return 0;
                    }
                //} 
            }
            if (tempNode.Item1 == -1 && tempNode.Item2 == -1)
            {
                return -1;
            } else
            {
                return 0;
            }

        }

		//method is public for now inorder to manually trigger steps on iOS for testing. 
		//Should be made private and removed from interface eventually
		public void StepTaken(bool startFromStat){
			testStepTrigger ();
			var args = new PositionChangedHandlerEventArgs (realPosition.Item1, realPosition.Item2);

			PositionChanged(this, args);
		}

        //replace with StepDetection Event trigger
        private Tuple<float, float> testStepTrigger()
        {
            string start, end;
            float x, y;


			float nHeading = (float) ((Math.PI/2) - Heading);
			x = realPosition.Item1 + (strideLength * (float) Math.Cos(nHeading));
			y = realPosition.Item2 - (strideLength * (float) Math.Sin(nHeading));
            

            Tuple<float, float> newPosition = new Tuple<float, float>(x, y);

            Tuple<float, float> realHolder = realPosition;
            Tuple<int, int> nearestHolder = nearestGraphNode;

            realPosition = newPosition;
			int check = CalculateNearestNode();
            if (check != -1)
            {
                if (!nearestHolder.Equals(nearestGraphNode))
                {
                    start = nearestGraphNode.Item1.ToString() + "-" + nearestGraphNode.Item2.ToString();
                    end = nearestHolder.Item1.ToString() + "-" + nearestHolder.Item2.ToString();
                    var path = _graph.FindPath(start, end);
                    if (path.Count > 2)
                    {
                        realPosition = realHolder;
                        nearestGraphNode = nearestHolder;
                    }
                }
            } else
            {
                realPosition = realHolder;
            }
            return realPosition;
        }

        
        
    }

	public class PositionChangedHandlerEventArgs : EventArgs {
		public float newX;
		public float newY;

		public PositionChangedHandlerEventArgs (float newX, float newY)
		{
			this.newX = newX;
			this.newY = newY;
		}
	}
}

using System;
using System.Collections.Generic;
using Navigator.Pathfinding;
using Navigator.Primitives;
using System.Linq;
using Navigator.Helpers;

namespace Navigator
{
    //enum for sensor type

    //delegates to define the output for events
    public delegate void PositionChangedHandler(object sender, PositionChangedHandlerEventArgs args);

    //public delegate void HeadingHandler(int nHeading);

    public enum CollisionSensorType
    {
        Accelometer,
        Gyroscope,
        Magnetometer
    }

    public interface ICollision
    {
        event PositionChangedHandler PositionChanged;
        void SetLocation(float startX, float startY);
        void PassSensorReadings(CollisionSensorType s, double xVal, double yVal, double zVal);
        void PassHeading(float nHeading);
        void StepTaken(bool startFromStat);
    }

    public class Collision : ICollision
    {
        //Other values
        private float totalStride = 30.0f;
        private float strideLength = 12.0f;
        private const int searchDistance = 6;
        //graph information
        private readonly IGraph _graph;

        //StepDetector Class
        public readonly IStepDetector StepDetector;
        private float Heading;
        private Vector2 nearestGraphNode;

        //Values that are being tracked
        private Vector2 realPosition;
        //public event HeadingHandler newHeading;

        public WallCollision WallCol;

		public Collision(IGraph graph, IStepDetector stepDetector)
        {
			StepDetector = stepDetector;
            StepDetector.OnStep += StepTaken;
            _graph = graph;
        }

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

        public void SetLocation(float startX, float startY)
        {
            realPosition = new Vector2(startX, startY);
			nearestGraphNode = CalculateNearestNode(realPosition);
        }

        public void PassSensorReadings(CollisionSensorType s, double xVal, double yVal, double zVal)
        {
            switch (s)
            {
                case CollisionSensorType.Accelometer:
                    StepDetector.passValue(xVal, yVal, zVal);
                    break;
            }
        }

        public void PassHeading(float nHeading)
        {
            Heading = nHeading;
        }

        //method is public for now inorder to manually trigger steps on iOS for testing. 
        //Should be made private and removed from interface eventually
        public void StepTaken(bool startFromStat)
        {
            int stepIterations = (int)Math.Floor (totalStride / strideLength);

            float extraStep = totalStride % strideLength;

            var args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);

            for (int i = 0; i < stepIterations; i++) {
            
                if (i == 0) {
                    testStepTrigger ();
                    args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                } else {
                    testStepTrigger();
                    args = new PositionChangedHandlerEventArgs(realPosition.X, realPosition.Y);
                }
                PositionChanged(this, args);
            }

            if (extraStep != 0) {
                strideLength = extraStep;
                testStepTrigger();
                args = new PositionChangedHandlerEventArgs(realPosition.X, realPosition.Y);
                PositionChanged(this, args);
                strideLength = 12.0f;
            }
        }

		private Vector2 CalculateNearestNode(Vector2 position)
        {
			var tempNode = _graph.FindClosestNode(position.X, position.Y);
			return tempNode;
        }

        //replace with StepDetection Event trigger
        private void testStepTrigger()
        {
            var nHeading = (float) (Math.PI/2 - Heading);

            var tmpX = realPosition.X + strideLength*(float) Math.Cos(nHeading);
			var tmpY = realPosition.Y - strideLength*(float) Math.Sin(nHeading);
            var tmpPosition = new Vector2(tmpX, tmpY);
			var tmpNearestNode = CalculateNearestNode(tmpPosition);

			if (nearestGraphNode.Equals (tmpNearestNode)) {
				realPosition = tmpPosition;
			} else if (tmpNearestNode != Vector2.Invalid) {
				if (_graph.isLinked(nearestGraphNode, tmpNearestNode)) {
					realPosition = tmpPosition;
					nearestGraphNode = tmpNearestNode;

				}
			}
        }
    }

    public class PositionChangedHandlerEventArgs : EventArgs
    {
        public float newX;
        public float newY;

        public PositionChangedHandlerEventArgs(float newX, float newY)
		{
			this.newX = newX;
			this.newY = newY;
		}
    }
}
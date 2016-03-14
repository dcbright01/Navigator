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

        //path holder
        private readonly Queue<Vector2> _graphPath;

        //StepDetector Class
        public readonly IStepDetector StepDetector;
        private float Heading;
        private Vector2 nearestGraphNode;

        //Values that are being tracked
        private Vector2 realPosition;
        //public event HeadingHandler newHeading;
        private FixedSizeQueue<float> headingQueue = new FixedSizeQueue<float>(15);
        private double currentTime = Math.Round(DateTime.Now.ToUniversalTime().Subtract(
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        ).TotalMilliseconds, 0);
        private double referenceTime = Math.Round(DateTime.Now.ToUniversalTime().Subtract(
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        ).TotalMilliseconds, 0);

        public WallCollision WallCol;

		public Collision(IGraph graph, IStepDetector stepDetector)
        {
			StepDetector = stepDetector;
            StepDetector.OnStep += StepTaken;
            _graph = graph;
            _graphPath = new Queue<Vector2>();
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
            CalculateNearestNode();
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
            currentTime = Math.Round(DateTime.Now.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds, 0);
            Heading = nHeading;
            double test = currentTime - referenceTime;

            //if (currentTime - referenceTime > 100) {
            headingQueue.Enqueue (nHeading);
            referenceTime = currentTime;
            //}

        }

        //method is public for now inorder to manually trigger steps on iOS for testing. 
        //Should be made private and removed from interface eventually
        public void StepTaken(bool startFromStat)
        {
            int stepIterations = (int)Math.Floor (totalStride / strideLength);

            float extraStep = totalStride % strideLength;

            var args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
            //StepCounter = 1;
            if (startFromStat == false) {

                for (int i = 0; i < stepIterations; i++) {

                    if (i == 0) {
                        testStepTrigger (Heading);
                        args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                    } else {
                        testStepTrigger (Heading);
                        args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                    }
                    PositionChanged (this, args);
                }

                if (extraStep != 0) {
                    strideLength = extraStep;
                    testStepTrigger (Heading);
                    args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                    PositionChanged (this, args);
                    strideLength = 12.0f;
                }
            } else {
                float[] headings = new float[headingQueue.Count];
                headingQueue.CopyTo (headings,0);

                for (int j = 0; j < 3; j++) {
                    for (int i = 0; i < stepIterations; i++) {

                        if (i == 0) {
                            testStepTrigger (headings [4+ (j*4)]);
                            args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                        } else {
                            testStepTrigger (headings [4 + (j*4)]);
                            args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                        }
                        PositionChanged (this, args);
                    }
                    if (extraStep != 0) {
                        strideLength = extraStep;
                        testStepTrigger (headings [4 + (j*4)]);
                        args = new PositionChangedHandlerEventArgs (realPosition.X, realPosition.Y);
                        PositionChanged (this, args);
                        strideLength = 12.0f;
                    }
                }

            }
        }

        private int CalculateNearestNode()
        {
            var tempNode = _graph.FindClosestNode(realPosition.X, realPosition.Y, searchDistance);
			//var contains = ((Graph) _graph).Vertices.Contains(tempNode.ToPointString());

            //case where this is the initial position, figure out how for initial to avoid wall hopping
            if (nearestGraphNode == null)
            {
                if (tempNode.X != -1 && tempNode.Y != -1)
                {
                    nearestGraphNode = tempNode;
                    _graphPath.Enqueue(tempNode);
                    return 0;
                }
            }
            else if (!nearestGraphNode.Equals(tempNode))
                // for the case where its not the initial and the previous value is different from current
            {
                /*start = nearestGraphNode.Item1.ToString() + "-" + nearestGraphNode.Item2.ToString();
                end = tempNode.Item1.ToString() + "-" + tempNode.Item2.ToString();
                var path = g.FindPath(start, end);  
                if(path.Count < 3)
                {*/
                if (_graphPath.Count != 0)
                {
                    if (_graphPath.Count == 5)
                    {
                        _graphPath.Dequeue();
                    }
                }
                if (tempNode.IsValidCoordinate)
                {
                    _graphPath.Enqueue(tempNode);
                    nearestGraphNode = tempNode;
                    return 0;
                }
                //} 
            }
            if (!tempNode.IsValidCoordinate)
            {
                return -1;
            }
            return 0;
        }

        //replace with StepDetection Event trigger
        private Vector2 testStepTrigger(float _heading)
        {
            string start, end;
            float x, y;

            var nHeading = (float) (Math.PI/2 - _heading);
            x = realPosition.X + strideLength*(float) Math.Cos(nHeading);
            y = realPosition.Y - strideLength*(float) Math.Sin(nHeading);


            var newPosition = new Vector2(x, y);

            var realHolder = realPosition;
            var nearestHolder = nearestGraphNode;

            realPosition = newPosition;
            var check = CalculateNearestNode();
            if (check != -1)
            {
                if (!nearestHolder.Equals(nearestGraphNode))
                {
                    start = nearestGraphNode.ToPointString();
					end = nearestHolder.ToPointString();

                    if (start != end) {

                        if (!WallCol.IsValidStep ((int)nearestGraphNode.X, (int)nearestGraphNode.Y, (int)nearestHolder.X, (int)nearestHolder.Y)) {

                            realPosition = realHolder;
                            nearestGraphNode = nearestHolder;

                        }

                    } else {
                        //realPosition = realHolder;
                    }

                }
            }
            else
            {
                realPosition = realHolder;
            }
            return realPosition;
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

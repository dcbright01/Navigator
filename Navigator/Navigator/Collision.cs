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
    public delegate void CollisionHandler(float realX, float realY);
    //public delegate void HeadingHandler(int nHeading);

   public class Collision
    {

        public enum Sensor
        {
            Accel,
            Gryo,
            Mag
        };

        //Values that are being tracked
        private Tuple<float, float> realPosition;
        private Tuple<int, int> nearestGraphNode;
        private int heading;

        //Other values
        private const float strideLength = 3.0f;
        private const int searchDistance = 6;

        //path holder
        Queue<Tuple<int, int>> graphPath;

        //graph information
        Graph g;
        
        //Require interfaces that values are passed to
        //Additional Interfaces added here
        //StepDetector Interface
        //Heading Interface
        //These have events that this class is subscribed to, upon them being triggered the information should then be used.

        //Event for passing info back to the platforms
        //seperated heading and validMove as unsure what will happen if both interfaces trigger at the same time and try to send the call twice
        //also means that on the platform specific level, you know which has happened rather than having to check the values if you want the info
        //remerging is simple if this isn't needed/the concern is invalid
        public event CollisionHandler validMove;
        //public event HeadingHandler newHeading;

        public Collision()
        {
            //constructor
            graphPath = new Queue<Tuple<int, int>>();

        }

        public void addGraph(Graph nGraph)
        {
            g = nGraph;
        }

        public void GiveStartingLocation(float startX, float startY)
        {
            realPosition = Tuple.Create(startX, startY);
            int x = nearestNode();
        }

        public void PassSensorReadings(Sensor s, double xVal, double yVal, double zVal)
        {
            //give these values to whatever interfaces/components require
        }

        public virtual void ValidMoveMade()
        {
            if (validMove != null)
                validMove(realPosition.Item1, realPosition.Item2);
        }

        /*public virtual void HeadingChange()
        {
            if (newHeading != null)
                newHeading(heading);
        }*/

        private int nearestNode()
        {
            Tuple<int, int> tempNode = new Tuple<int, int>(-1, -1);
            double distanceFromTempToReal = -1;
            double a, b, newDistance;

            string nodeCoords;

            //there is a more effecient search that would spiral outwards to a set point before searching the corners of the sqaure that this gets, can implement if need be
            for (int x = (int) realPosition.Item1 - searchDistance; x <= realPosition.Item1 + searchDistance; x++)
            {
                for(int y = (int) realPosition.Item2 - searchDistance; y <= realPosition.Item2 + searchDistance; y++)
                {
                    nodeCoords = x.ToString() + "-" + y.ToString();

                    string nodeCheck = g.Vertices.FirstOrDefault(node => node == nodeCoords);
                    if(nodeCheck != null)
                    {
                        a = y - realPosition.Item2;
                        b = x - realPosition.Item1;
                        newDistance = Math.Sqrt((a * a) + (b * b));
                        if (distanceFromTempToReal == -1 || newDistance < distanceFromTempToReal)
                        {//first node come across is set as the tempNode or if newDistance is smaller
                            tempNode = Tuple.Create(x, y);
                            distanceFromTempToReal = newDistance;
                        } //shouldn't need to deal with case where you are the same distance from multiple nodes, as it won't really effect navigation and rules, if required can be added
                    }                                        
                }
            }

            string start, end;

            //case where this is the initial position, figure out how for initial to avoid wall hopping
            if(nearestGraphNode == null)
            {
                if (tempNode.Item1 != -1 && tempNode.Item2 != -1){
                    nearestGraphNode = tempNode;
                    graphPath.Enqueue(tempNode);
                    return 0;
                }
            } else if(!nearestGraphNode.Equals(tempNode)) // for the case where its not the initial and the previous value is different from current
            {
                /*start = nearestGraphNode.Item1.ToString() + "-" + nearestGraphNode.Item2.ToString();
                end = tempNode.Item1.ToString() + "-" + tempNode.Item2.ToString();
                var path = g.FindPath(start, end);  
                if(path.Count < 3)
                {*/
                    if(graphPath.Count != null)
                    {
                        if (graphPath.Count == 5)
                        {
                            graphPath.Dequeue();
                        }
                    }
                    if (tempNode.Item1 != -1 && tempNode.Item2 != -1) {
                        graphPath.Enqueue(tempNode);
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

        //replace with StepDetection Event trigger
        public Tuple<float, float> testStepTrigger()
        {
            string start, end;
            int nHeading;
            float x, y;
            double headRadians;

            if(heading < 90)
            {
                headRadians = VarunMaths.RadianToDegree(heading);
                x = realPosition.Item1 + (strideLength * (float) Math.Sin(headRadians));
                y = realPosition.Item2 - (strideLength * (float) Math.Cos(headRadians));
            } else if(heading >= 90 && heading < 180)
            {
                nHeading = heading - 90;
                headRadians = VarunMaths.RadianToDegree(nHeading);
                x = realPosition.Item1 + (strideLength * (float)Math.Cos(headRadians));
                y = realPosition.Item2 + (strideLength * (float)Math.Sin(headRadians));
            } else if(heading >= 180 && heading < 270)
            {
                nHeading = heading - 180;
                headRadians = VarunMaths.RadianToDegree(nHeading);
                x = realPosition.Item1 - (strideLength * (float)Math.Sin(headRadians));
                y = realPosition.Item2 + (strideLength * (float)Math.Cos(headRadians));
            } else
            {
                nHeading = heading - 270;
                headRadians = VarunMaths.RadianToDegree(nHeading);
                x = realPosition.Item1 - (strideLength * (float)Math.Cos(headRadians));
                y = realPosition.Item2 - (strideLength * (float)Math.Sin(headRadians));
            }

            Tuple<float, float> newPosition = new Tuple<float, float>(x, y);

            Tuple<float, float> realHolder = realPosition;
            Tuple<int, int> nearestHolder = nearestGraphNode;

            realPosition = newPosition;
            int check = nearestNode();
            if (check != -1)
            {
                if (!nearestHolder.Equals(nearestGraphNode))
                {
                    start = nearestGraphNode.Item1.ToString() + "-" + nearestGraphNode.Item2.ToString();
                    end = nearestHolder.Item1.ToString() + "-" + nearestHolder.Item2.ToString();
                    var path = g.FindPath(start, end);
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

        
        public void passHeading(int nHeading)
        {
            heading = nHeading;
            //HeadingChange();
        }
    }
}

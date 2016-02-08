using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{

    public delegate void CollisionHandler(float realX, float realY);
    public delegate void HeadingHandler(int nHeading);

    class Collision
    {
        //Values that are being tracked
        private Tuple<float, float> realPosition;
        private Tuple<int, int> nearestGraphNode;
        private int heading; 
        
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
        public event HeadingHandler newHeading;

        public Collision()
        {
            //constructor
        }

        public void giveStartingLocation(float startX, float startY)
        {
            realPosition = Tuple.Create(startX, startY);
            nearestNode();
        }

        public void passSensorReadings(double accelX, double accelY, double accelZ, double gyroX, double gyroY, double gyroZ, double magX, double magY, double magZ)
        {
            //give these values to whatever interfaces/components require
        }

        public virtual void validMoveMade()
        {
            if (validMove != null)
                validMove(realPosition.Item1, realPosition.Item2);
        }

        public virtual void headingChange()
        {
            if (newHeading != null)
                newHeading(heading);
        }

        private void nearestNode()
        {
            //work out the nearest node from changed real position
        }

        //replace with StepDetection Event trigger
        private void testStepTrigger()
        {
            //if move valid update positions, trigger validMove event and recalculate the nearestNode
            //else do nothing
        }

        //replace with Heading Event trigger
        private void testHeadingTrigger(int nHeading)
        {
            heading = nHeading;
            headingChange();
        }
    }
}

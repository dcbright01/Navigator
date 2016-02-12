using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{
    public delegate void StepHandler(int stepsTaken);

    public class StepDetector
    {

        private double[] accelValues = new double[3];
        private double lastPeakValue = -1;
        private double lastTroughValue = -1;
        private int stepCounter = 0;
		private long initialMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		private ButterworthLowPassFilter lowPassFilter = new ButterworthLowPassFilter(); 

        public event StepHandler OnStep;

        public StepDetector()
        {
            //constructor
        }

        private void stepCheck()
        {
            if (isPeak())
            {
                if (lastTroughValue == -1)
                {
                    // if we've hit a peak without hitting a trough, store this peak value for future use
                    // accelValues[1] because that's the current value
                    lastPeakValue = accelValues[1];
                    return;
                }

				double difference = Math.Abs((accelValues[1] - lastTroughValue));

                // totalOfDifferences += difference; 
                // numberOfDifferences++; 
                // average = (totalOfDifferences / numberOfDifferences); 

				long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                // the thresholds that we use in order to filter out false positives
				if ((currentMilliseconds - initialMilliseconds) > 500 && (currentMilliseconds - initialMilliseconds) < 900)
                {
					if (difference > 2.5 && difference < 8)  
					{
						lastPeakValue = accelValues [1];
						stepCounter++;
						OnStepTaken ();
					}
                }

				initialMilliseconds = currentMilliseconds; 
            }

            if (isTrough())
            {
                lastTroughValue = accelValues[1];
            }
            //When you normally increase the counter, call onStepCheck
        }

        private bool isTrough()
        {
            double oldDifference = accelValues[1] - accelValues[0];
            double newDifference = accelValues[2] - accelValues[1];

            if (newDifference > 0 && oldDifference < 0)
            {
                return true;
            }

            return false;
        }

        private bool isPeak()
        {
            double oldDifference = accelValues[1] - accelValues[0];
            double newDifference = accelValues[2] - accelValues[1];

            // look at slopes between future/current point and current/past points
            if (newDifference < 0 && oldDifference > 0)
            {
                return true;
            }

            return false;
        }

        public void reset()
        {
            //reset all local variables
        }

        private int functionCalledCounter = 0;

        public void passValue(double accelValueX, double accelValueY, double accelValueZ)
        {
            if (functionCalledCounter < 3)
            {
                // we have a window of 3 values, so for the first 3 values just fill in the window
				accelValues[functionCalledCounter] = getFilteredMagnitude(accelValueX, accelValueY, accelValueZ);
                functionCalledCounter++;
                if (functionCalledCounter == 2)
                {
                    stepCheck();
                }
                return;
            }

            // last 2 values of previous window become first 2 of new window
            Array.Copy(accelValues, 1, accelValues, 0, accelValues.Length - 1);
            // last value of new window is the filtered vector magnitude
			accelValues[2] = getFilteredMagnitude(accelValueX, accelValueY, accelValueZ);
            functionCalledCounter++;
            stepCheck();
        }

		public double getFilteredMagnitude(double accelValueX, double accelValueY, double accelValueZ)
        {
			double magnitude = Math.Sqrt(Math.Pow(accelValueX, 2) + Math.Pow(accelValueY, 2) + Math.Pow(accelValueZ, 2));
			return lowPassFilter.getNewFilteredValue(magnitude); 
        }


        public virtual void OnStepTaken()
        {
            if (OnStep != null)
                OnStep(stepCounter);
        }
    }
}
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

                double difference = Math.Abs(accelValues[1] - lastTroughValue);

                // totalOfDifferences += difference; 
                // numberOfDifferences++; 
                // average = (totalOfDifferences / numberOfDifferences); 

                // the thresholds that we use in order to filter out false positives
                if (difference > 1.5 && difference < 3)
                {
					long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
					if ((currentMilliseconds - initialMilliseconds) > 500) 
					{
						initialMilliseconds = currentMilliseconds; 
						lastPeakValue = accelValues [1];
						stepCounter++;
						OnStepTaken ();
					}
                }
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

        private double[] filteredAccelVals = null;

        public double getFilteredMagnitude(double accelValueX, double accelValueY, double accelValueZ)
        {
            double[] newAccelVals = { accelValueX, accelValueY, accelValueZ };

            filteredAccelVals = lowPass(newAccelVals, filteredAccelVals);

			// only looking at the Z value for now
			return filteredAccelVals[2];
        }

        public static double[] lowPass(double[] input, double[] output)
        {

            double ALPHA = 0.25;

            if (output == null)
            {
                return input;
            }

            // the new output is the output from the previous step + a small difference caused by the new input
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = output[i] + ALPHA * (input[i] - output[i]);
            }

            return output;
        }

        public virtual void OnStepTaken()
        {
            if (OnStep != null)
                OnStep(stepCounter);
        }
    }
}
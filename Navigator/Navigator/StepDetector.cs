using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{
	public class StepDetector
	{

		private double[] accelValues = new double[3];
		private double lastPeakValue = -1;
		private double lastTroughValue = -1;

		public event EventHandler Taken;

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
				if (difference > 0.5 && difference < 2.2)
				{
					Taken(this, new EventArgs());
					lastPeakValue = accelValues[1];
				}
			}

			if (isTrough())
			{
				lastTroughValue = accelValues[1];
			}
			//When you normally increase the counter, call onStepCheck
		}

		private Boolean isTrough()
		{
			double oldDifference = accelValues[1] - accelValues[0];
			double newDifference = accelValues[2] - accelValues[1];

			if (newDifference > 0 && oldDifference < 0)
			{
				return true;
			}

			return false;
		}

		private Boolean isPeak()
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
				accelValues[functionCalledCounter] = getFilteredVectorMagnitude(accelValueX, accelValueY, accelValueZ);
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
			accelValues[2] = getFilteredVectorMagnitude(accelValueX, accelValueY, accelValueZ);
			functionCalledCounter++;
			stepCheck();
		}

		private double[] filteredAccelVals = null;

		public double getFilteredVectorMagnitude(double accelValueX, double accelValueY, double accelValueZ)
		{
			double[] newAccelVals = { accelValueX, accelValueY, accelValueZ };

			filteredAccelVals = lowPass(newAccelVals, filteredAccelVals);

			return Math.Sqrt(Math.Pow(filteredAccelVals[0], 2) + Math.Pow(filteredAccelVals[1], 2) + Math.Pow(filteredAccelVals[2], 2));
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
	}
}
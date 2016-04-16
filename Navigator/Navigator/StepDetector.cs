using System;

namespace Navigator
{
    public delegate void StepHandler(bool startFromStat);

	public interface IStepDetector{
		event StepHandler OnStep;
		void passValue (double accelValueX, double accelValueY, double accelValueZ) ;
	}

	public class StepDetector : IStepDetector
    {
        private readonly double[] accelValues = new double[3];

        private int functionCalledCounter;
        private long initialMilliseconds = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
		private long iMilli = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        private double lastPeakValue = -1;
        private double lastTroughValue = -1;
        private readonly ButterworthLowPassFilter lowPassFilter = new ButterworthLowPassFilter();
        private bool stationaryStart;
        public int StepCounter;
		public long stepMilli;
        private double troughToPeakDifference = -1;

        public event StepHandler OnStep;

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

                // set last peak to impossible negative value in case step conditions below are not satisifed 
                lastPeakValue = -1;

                troughToPeakDifference = Math.Abs(accelValues[1] - lastTroughValue);

                // the thresholds that we use in order to filter out false positives
                if (troughToPeakDifference > 2 && troughToPeakDifference < 6)
                {
                    lastPeakValue = accelValues[1];
                }
            }

            if (isTrough())
            {
                lastTroughValue = accelValues[1];
                // only check for steps if min. difference condition above has been satisfied 
                if (lastPeakValue != -1)
                {
                    var peakToTroughDifference = Math.Abs(lastTroughValue - lastPeakValue);
                    // check if peak to trough value is within +-20% of trough to peak one
                    if (peakToTroughDifference > 0.7*troughToPeakDifference &&
                        peakToTroughDifference < 1.3*troughToPeakDifference)
                    {
                        var currentMilliseconds = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
                        if (currentMilliseconds - initialMilliseconds > 200 &&
                            currentMilliseconds - initialMilliseconds < 2500)
                        {
							if (StepCounter == 0)
							{
								iMilli = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
							}
							stepMilli = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - iMilli;
                            initialMilliseconds = currentMilliseconds;
                            StepCounter++;
                            stationaryStart = false;
                            OnStepTaken();
                        }
                        // for gaps of more than 2 seconds we can assume user starts from stationary position, so add the 2 steps that the
                        // filter misses out on initially 
                        else if (currentMilliseconds - initialMilliseconds >= 2500)
                        {
							if (StepCounter == 0) 
							{
								iMilli = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
							}
							stepMilli = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - iMilli;
                            initialMilliseconds = currentMilliseconds;
                            StepCounter++;
                            StepCounter = StepCounter + 2;
                            stationaryStart = true;
                            OnStepTaken();
                        }
                    }
                }
            }
        }

        private bool isTrough()
        {
            var oldDifference = accelValues[1] - accelValues[0];
            var newDifference = accelValues[2] - accelValues[1];

            if (newDifference > 0 && oldDifference < 0)
            {
                return true;
            }

            return false;
        }

        private bool isPeak()
        {
            var oldDifference = accelValues[1] - accelValues[0];
            var newDifference = accelValues[2] - accelValues[1];

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
            // double magnitude = Math.Sqrt(Math.Pow(accelValueX, 2) + Math.Pow(accelValueY, 2) + Math.Pow(accelValueZ, 2));
            var magnitude = accelValueZ;
            return lowPassFilter.getNewFilteredValue(magnitude);
        }


        public virtual void OnStepTaken()
        {
            if (OnStep != null)
                OnStep(stationaryStart);
        }
    }
}
using System;
using Navigator.Primitives; 

namespace Navigator
{
	public class HeadingDetector
	{
		public HeadingDetector()
		{
            //constructor
		}

        public void reset()
        {
            //reset all local variables
        }

		public void passValue(Vector3 accelerometer, Vector3 magnetometer)
        {
            //receive input from

        }



		private float[] lowPass(float[] input, float[] output) 
		{
			float ALPHA = 0.03f;

			if(output == null) 
			{
				return input;
			}

			// the new output is the output from the previous step + a small difference caused by the new input
			for(int i =  0; i < input.Length; i++) 
			{
				output[i] = output[i] + ALPHA * (input[i] - output[i]);
			}

			return output;
		}

		private double RadianToDegree(double angle) 
		{
			return angle * (180.0 / Math.PI); 
		}
    }
}



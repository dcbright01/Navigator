namespace Navigator
{
	public class ButterworthLowPassFilter
	{
		double ALPHA = 0.05;
		double output = 0; 

		public double getNewFilteredValue(double input)
		{
			output = ALPHA * input + (1 - ALPHA) * output; 
			return output; 
		}
	}
}
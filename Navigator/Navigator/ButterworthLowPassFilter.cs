namespace Navigator
{
    public class ButterworthLowPassFilter
    {
        private const int NZEROS = 5;
        private const int NPOLES = 5;
		private const double GAIN = 1.79846116966069e+005;

        private static readonly double[] xv = new double[NZEROS + 1];
        private static readonly double[] yv = new double[NPOLES + 1];

        public double getNewFilteredValue(double input)
        {
            xv[0] = xv[1];
            xv[1] = xv[2];
            xv[2] = xv[3];
            xv[3] = xv[4];
            xv[4] = xv[5];
			xv[5] = input/GAIN;
            yv[0] = yv[1];
            yv[1] = yv[2];
            yv[2] = yv[3];
            yv[3] = yv[4];
            yv[4] = yv[5];

			yv [5] = xv [0] + xv [5] + 5 * (xv [1] + xv [4]) + 10 * (xv [2] + xv [3])
					 + 0.54275137493347 * yv [0] - 3.04468530918026 * yv [1]
					 + 6.85434935089591 * yv [2] - 7.74286954080103 * yv [3]
					 + 4.39027619426085 * yv [4];

            return yv[5];
        }
    }
}
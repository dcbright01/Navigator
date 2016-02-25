namespace Navigator
{
    public class ButterworthLowPassFilter
    {
        private const int NZEROS = 10;
        private const int NPOLES = 10;
        private const double GAIN = 3.210431373e+10;

        private static readonly double[] xv = new double[NZEROS + 1];
        private static readonly double[] yv = new double[NPOLES + 1];

        public double getNewFilteredValue(double input)
        {
            xv[0] = xv[1];
            xv[1] = xv[2];
            xv[2] = xv[3];
            xv[3] = xv[4];
            xv[4] = xv[5];
            xv[5] = xv[6];
            xv[6] = xv[7];
            xv[7] = xv[8];
            xv[8] = xv[9];
            xv[9] = xv[10];
            xv[10] = input/GAIN;
            yv[0] = yv[1];
            yv[1] = yv[2];
            yv[2] = yv[3];
            yv[3] = yv[4];
            yv[4] = yv[5];
            yv[5] = yv[6];
            yv[6] = yv[7];
            yv[7] = yv[8];
            yv[8] = yv[9];
            yv[9] = yv[10];

            yv[10] = xv[0] + xv[10] + 10*(xv[1] + xv[9]) + 45*(xv[2] + xv[8])
                     + 120*(xv[3] + xv[7]) + 210*(xv[4] + xv[6]) + 252*xv[5]
                     + -0.2990054779*yv[0] + 3.3503054451*yv[1]
                     + -16.9167227920*yv[2] + 50.6918184340*yv[3]
                     + -99.8350790400*yv[4] + 135.0355428800*yv[5]
                     + -127.0429438600*yv[6] + 82.0958968460*yv[7]
                     + -34.8749828190*yv[8] + 8.7951703476*yv[9];

            return yv[10];
        }
    }
}
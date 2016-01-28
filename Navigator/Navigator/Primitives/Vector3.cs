using System;

namespace Navigator.Primitives
{
    public class Vector3
    {
        public static Vector3 Zero = new Vector3 {X = 0, Y = 0, Z = 0};

        public Vector3()
        {
        }

        public Vector3(float[] values)
        {
            if (values.Length < 3)
                throw new Exception("Not enough values to construct a Vector3");
            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override string ToString()
        {
            return string.Format(" X : {0} \n Y : {1} \n Z : {2}", X, Y, Z);
        }

        public string ToCSV()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }
    }
}
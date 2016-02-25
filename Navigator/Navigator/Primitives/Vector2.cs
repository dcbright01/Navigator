using System;

namespace Navigator.Primitives
{
    public class Vector2
    {
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(string pointString)
        {
            try
            {
                var split = pointString.Split('-');
                X = float.Parse(split[0]);
                Y = float.Parse(split[1]);
            }
            catch
            {
                throw new Exception("Unable to parse a Vector2 from the string");
            }
        }

        public Vector2(float[] values)
        {
            if (values.Length < 2)
                throw new Exception("Not enough values to construct Vector2");
            X = values[0];
            Y = values[1];
        }

        public float X { get; set; }
        public float Y { get; set; }

        public bool IsValidCoordinate { get { return X != -1 && Y != -1; } }

        public float Distance2D(Vector2 otherVector)
        {
            return (float) Math.Sqrt(Math.Pow(X - otherVector.X, 2) + Math.Pow(Y - otherVector.Y, 2));
        }

        public string ToPointString()
        {
            return string.Format("{0}-{1}", X, Y);
        }

        public static implicit operator float[](Vector2 instance)
        {
            return new[] {instance.X, instance.Y};
        }
    }
}
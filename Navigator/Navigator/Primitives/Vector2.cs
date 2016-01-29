using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator.Primitives
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vector2(float[] values)
        {
            if(values.Length < 2)
                throw  new Exception("Not enough values to construct Vector2");
            X = values[0];
            Y = values[1];
        }

        public float DistanceTo(Vector2 otherVector)
        {
            return (float) Math.Sqrt(Math.Pow(X - otherVector.X, 2) + Math.Pow(Y - otherVector.Y, 2));
        }
    }
}

using System;

namespace Navigator.Primitives
{
    public class Vector3 : Vector2
    {
        public Vector3(float x, float y, float z) : base(x, y)
        {
            Z = z;
        }

        public Vector3(float[] values) : base(values)
        {
            if (values.Length < 3)
                throw new Exception("Not enough values to construct Vector3");
            Z = values[2];
        }

        public float Z { get; set; }

        public float Distance3D(Vector3 otherVector)
        {
            return
                (float)
                    Math.Sqrt(Math.Pow(X - otherVector.X, 2) + Math.Pow(Y - otherVector.Y, 2) +
                              Math.Pow(Z - otherVector.Z, 2));
        }

        public static implicit operator float[](Vector3 instance)
        {
            return new[] {instance.X, instance.Y, instance.Z};
        }
    }
}
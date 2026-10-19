using System;
using UnityEngine;

namespace AeroSpace.Core
{
    [Serializable]
    public struct Vector3d
    {
        public double x, y, z;

        public static readonly Vector3d zero = new(0, 0, 0);

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double sqrMagnitude => x * x + y * y + z * z;
        public double magnitude => Math.Sqrt(sqrMagnitude);

        public Vector3d normalized
        {
            get
            {
                double m = magnitude;
                return m > 0 ? this / m : zero;
            }
        }

        public static Vector3d operator +(Vector3d a, Vector3d b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3d operator -(Vector3d a, Vector3d b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3d operator -(Vector3d a) => new(-a.x, -a.y, -a.z);
        public static Vector3d operator *(Vector3d a, double d) => new(a.x * d, a.y * d, a.z * d);
        public static Vector3d operator *(double d, Vector3d a) => new(a.x * d, a.y * d, a.z * d);
        public static Vector3d operator /(Vector3d a, double d) => new(a.x / d, a.y / d, a.z / d);

        public static Vector3d operator *(Quaternion q, Vector3d v)
        {
            var u = new Vector3d(q.x, q.y, q.z);
            var t = 2.0 * Cross(u, v);
            return v + q.w * t + Cross(u, t);
        }

        public static double Dot(Vector3d a, Vector3d b) => a.x * b.x + a.y * b.y + a.z * b.z;

        public static Vector3d Cross(Vector3d a, Vector3d b) =>
            new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

        public static explicit operator Vector3(Vector3d v) => new((float)v.x, (float)v.y, (float)v.z);
        public static implicit operator Vector3d(Vector3 v) => new(v.x, v.y, v.z);

        public override string ToString() => $"({x:F1}, {y:F1}, {z:F1})";
    }
}

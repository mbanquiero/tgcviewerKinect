using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.DirectX;

namespace Examples.Kinect
{
    /// <summary>
    /// Utilidades varias de kinect
    /// </summary>
    public class TgcKinectUtils
    {
        /// <summary>
        /// Multriplicar por un escalar
        /// </summary>
        public static SkeletonPoint mul(SkeletonPoint p, float scale)
        {
            SkeletonPoint p2 = new SkeletonPoint();
            p2.X = p.X * scale;
            p2.Y = p.Y * scale;
            p2.Z = p.Z * scale;
            return p2;
        }

        /// <summary>
        /// Sumar un escalar
        /// </summary>
        public static SkeletonPoint sum(SkeletonPoint p, float n)
        {
            SkeletonPoint p2 = new SkeletonPoint();
            p2.X = p.X + n;
            p2.Y = p.Y + n;
            p2.Z = p.Z + n;
            return p2;
        }

        /// <summary>
        /// Convierte de un SkeletonPoint a un Vector3
        /// </summary>
        public static Vector3 toVector3(SkeletonPoint p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        public static float ScaleVector(int length, float position)
        {
            float value = (((((float)length) / 1f) / 2f) * position) + (length / 2);
            if (value > length)
            {
                return (float)length;
            }
            if (value < 0f)
            {
                return 0f;
            }
            return value;
        }

    }
}

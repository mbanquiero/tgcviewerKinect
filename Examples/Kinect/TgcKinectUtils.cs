using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.DirectX;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

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

        /// <summary>
        /// Convierte de un SkeletonPoint a un Vector2, ignorando Z
        /// </summary>
        public static Vector2 toVector2(SkeletonPoint p)
        {
            return new Vector2(p.X, p.Y);
        }

        /// <summary>
        /// Buscar bounding-box 2D del conjunto de puntos
        /// </summary>
        public static RectangleF computeScreenRect(SkeletonPoint[] points)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].X < minX)
                {
                    minX = points[i].X;
                }
                if (points[i].Y < minY)
                {
                    minY = points[i].Y;
                }
                if (points[i].X > maxX)
                {
                    maxX = points[i].X;
                }
                if (points[i].Y > maxY)
                {
                    maxY = points[i].Y;
                }
            }
            
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Clampear puntos a extremos del rectangulo
        /// </summary>
        public static Vector2 clampToRect(Vector2 p, RectangleF rect)
        {
            Vector2 p2 = new Vector2(p.X, p.Y);

            if (p2.X < rect.X)
            {
                p2.X = rect.X;
            }
            if (p2.X >= rect.X + rect.Width)
            {
                p2.X = rect.X + rect.Width;
            }
            if (p2.Y < rect.Y)
            {
                p2.Y = rect.Y;
            }
            if (p2.Y >= rect.Y + rect.Height)
            {
                p2.Y = rect.Y + rect.Height;
            }

            return p2;
        }

        /// <summary>
        /// Mapear punto p que está dentro de rect a la pantalla screenViewport
        /// </summary>
        public static Vector2 mapPointToScreen(Vector2 p, RectangleF rect, Viewport screenViewport, Vector2 cursorSize)
        {
            Vector2 q = new Vector2(p.X, p.Y);

            q.X -= rect.X;
            q.Y -= rect.Y;

            q.X /= rect.Width;
            q.Y /= rect.Height;

            q.X *= (screenViewport.Width - cursorSize.X);
            q.Y = (1 - q.Y) * (screenViewport.Height - cursorSize.Y);

            return q;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;

namespace Examples.Kinect
{
    /// <summary>
    /// Datos del Skeleton de Kinect adaptados a TGC
    /// </summary>
    public class TgcKinectSkeletonData
    {
        /// <summary>
        /// Indice de mano derecha
        /// </summary>
        public const int RIGHT_HAND = 0;

        /// <summary>
        /// Indice de mano izquierda
        /// </summary>
        public const int LEFT_HAND = 1;


        bool active;
        /// <summary>
        /// Indica si la informacion esta ok para ser leida. Si esta en false significa que no se pudo trackear el esqueleto en este cuadro
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        Frame current;
        /// <summary>
        /// Datos de frame actual
        /// </summary>
        public Frame Current
        {
            get { return current; }
            set { current = value; }
        }


        Frame previous;
        /// <summary>
        /// Datos del frame anterior
        /// </summary>
        public Frame Previous
        {
            get { return previous; }
            set { previous = value; }
        }

        LinkedList<HandFrame> handsFrames;
        /// <summary>
        /// Ultimos N frames trackeados de las dos manos del esqueleto
        /// </summary>
        public LinkedList<HandFrame> HandsFrames
        {
            get { return handsFrames; }
        }

        AnalysisData[] handsAnalysisData;
        /// <summary>
        /// Analisis estadistico para cada mano. Right=0 y Left=1
        /// </summary>
        public AnalysisData[] HandsAnalysisData
        {
          get { return handsAnalysisData; }
        }


        public TgcKinectSkeletonData()
        {
            this.previous = new Frame();
            this.current = new Frame();
            this.handsFrames = new LinkedList<HandFrame>();
            this.handsAnalysisData = new AnalysisData[] { new AnalysisData(), new AnalysisData() };
        }

        /// <summary>
        /// Analisis estadistico de un conjunto de cuadros para los tres ejes (X, Y, Z) de un hueso particular
        /// </summary>
        public class AnalysisData
        {
            AxisAnalysisData x;
            /// <summary>
            /// Datos estadisticos de eje X
            /// </summary>
            public AxisAnalysisData X
            {
              get { return x; }
            }

            AxisAnalysisData y;
            /// <summary>
            /// Datos estadisticos de eje Y
            /// </summary>
            public AxisAnalysisData Y
            {
              get { return y; }
            }

            AxisAnalysisData z;
            /// <summary>
            /// Datos estadisticos de eje Z
            /// </summary>
            public AxisAnalysisData Z
            {
              get { return z; }

            }

            /// <summary>
            /// Acceder a los valores de X=0 Y=1 y Z=2
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public AxisAnalysisData this[int i]
            {
                get
                {
                    return i == 0 ? x : (i == 1 ? y : z);
                }
            }

            public AnalysisData()
            {
                x = new AxisAnalysisData();
                y = new AxisAnalysisData();
                z = new AxisAnalysisData();
            }
        }

        /// <summary>
        /// Analisis estadistico de un conjunto de cuadros, para el movimiento en un eje de un hueso particular
        /// </summary>
        public class AxisAnalysisData
        {
            float min;
            /// <summary>
            /// Valor minimo de todos los cuadros
            /// </summary>
            public float Min
            {
                get { return min; }
                set { min = value; }
            }

            float max;
            /// <summary>
            /// Valor máximo de todos los cuadros
            /// </summary>
            public float Max
            {
                get { return max; }
                set { max = value; }
            }

            float avg;
            /// <summary>
            /// Promedio de todos los cuadros
            /// </summary>
            public float Avg
            {
                get { return avg; }
                set { avg = value; }
            }

            float variance;
            /// <summary>
            /// Varianza de todos los cuadros
            /// </summary>
            public float Variance
            {
                get { return variance; }
                set { variance = value; }
            }

            float diffAvg;
            /// <summary>
            /// Promedio de los diferenciales entre todos los
            /// </summary>
            public float DiffAvg
            {
                get { return diffAvg; }
                set { diffAvg = value; }
            }

            public AxisAnalysisData()
            {
            }
        }

        /// <summary>
        /// Informacion de tracking de un cuadro para las dos manos del esqueleto
        /// </summary>
        public struct HandFrame
        {
            Vector3[] pos3D;
            /// <summary>
            /// Posicion 3D de mano derecha [0] y mano izquieda [1]
            /// </summary>
            public Vector3[] Pos3D
            {
                get { return pos3D; }
                set { pos3D = value; }
            }

            Vector2[] pos2D;
            /// <summary>
            /// Posicion 2D (screen-pos) de mano derecha [0] y mano izquieda [1]
            /// </summary>
            public Vector2[] Pos2D
            {
                get { return pos2D; }
                set { pos2D = value; }
            }

            /// <summary>
            /// Obtener valor X o Y o Z de una de las dos manos, a partir del indice (0, 1, 2)
            /// </summary>
            public float get3DValue(int handIndex, int axisIndex)
            {
                Vector3 v = pos3D[handIndex];
                return axisIndex == 0 ? v.X : (axisIndex == 1 ? v.Y : v.Z);
            }
        }








        /// <summary>
        /// Informacion de Kinect para un frame particular
        /// </summary>
        public class Frame
        {
            Skeleton kinectSkeleton;
            /// <summary>
            /// Esqueleto de Kinect
            /// </summary>
            public Skeleton KinectSkeleton
            {
                get { return kinectSkeleton; }
                set { kinectSkeleton = value; }
            }

            TgcBoundingSphere rightHandSphere;
            /// <summary>
            /// BoundingSphere de la mano derecha del esqueleto
            /// </summary>
            public TgcBoundingSphere RightHandSphere
            {
                get { return rightHandSphere; }
                set { rightHandSphere = value; }
            }

            TgcBoundingSphere leftHandSphere;
            /// <summary>
            /// BoundingSphere de la mano izquierda del esqueleto
            /// </summary>
            public TgcBoundingSphere LeftHandSphere
            {
                get { return leftHandSphere; }
                set { leftHandSphere = value; }
            }

            Vector2 rightHandPos;
            /// <summary>
            /// Posicion 2D (screen-pos) de la mano derecha
            /// </summary>
            public Vector2 RightHandPos
            {
                get { return rightHandPos; }
                set { rightHandPos = value; }
            }

            Vector2 lefttHandPos;
            /// <summary>
            /// Posicion 2D (screen-pos) de la mano izquierda
            /// </summary>
            public Vector2 LefttHandPos
            {
                get { return lefttHandPos; }
                set { lefttHandPos = value; }
            }


            public Frame()
            {
                this.kinectSkeleton = new Skeleton();
                this.rightHandSphere = new TgcBoundingSphere(new Vector3(0, 0, 0), 3);
                this.leftHandSphere = new TgcBoundingSphere(new Vector3(0, 0, 0), 3);
                this.rightHandPos = new Vector2(0, 0);
                this.lefttHandPos = new Vector2(0, 0);
            }

        }

    }
}

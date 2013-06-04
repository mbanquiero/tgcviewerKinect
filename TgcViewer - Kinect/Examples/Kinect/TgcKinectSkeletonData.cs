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


        public TgcKinectSkeletonData()
        {
            this.previous = new Frame();
            this.current = new Frame();
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


            public Frame()
            {
                this.kinectSkeleton = new Skeleton();
                this.rightHandSphere = new TgcBoundingSphere(new Vector3(0, 0, 0), 5);
                this.leftHandSphere = new TgcBoundingSphere(new Vector3(0, 0, 0), 5);
            }

        }

    }
}

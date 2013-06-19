using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.Kinect;
using Microsoft.DirectX;
using System.Drawing;

namespace Examples.Kinect
{
    /// <summary>
    /// Utilidad para dibujar el esqueleto de Kinect en modo debug
    /// </summary>
    public class TgcKinectDebugSkeleton
    {

        public const float JOINT_SIZE = 0.5f;

        TgcBox[] jointBoxes;
        TgcLine[] jointLines;

        public TgcKinectDebugSkeleton()
        {

        }

        /// <summary>
        /// Crear todas las estructuras de debug
        /// </summary>
        public void init()
        {
            int bonesCount = Enum.GetValues(typeof(JointType)).Length;

            //Crear joints
            jointBoxes = new TgcBox[bonesCount];
            for (int i = 0; i < jointBoxes.Length; i++)
            {
                jointBoxes[i] = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(JOINT_SIZE, JOINT_SIZE, JOINT_SIZE), Color.Red);
            }

            //Crear bones
            jointLines = new TgcLine[bonesCount];
            for (int i = 0; i < jointLines.Length; i++)
            {
                jointLines[i] = new TgcLine();
                jointLines[i].Color = Color.Green;
            }
        }

        /// <summary>
        /// Dibujar esqueleto de debug
        /// Llamar a init() la primera vez.
        /// </summary>
        public void render(Skeleton skeleton)
        {
            //Actualizar datos
            if (skeleton != null)
            {
                //Obtener la posicion de todos los joints
                int idx = -1;
                foreach (Joint joint in skeleton.Joints)
                {
                    idx++;

                    //Obtener posicion
                    jointBoxes[idx].Position = TgcKinectUtils.toVector3(joint.Position);

                    //Setear color segun estado del joint
                    Color jointColor = joint.TrackingState == JointTrackingState.Tracked ? Color.Red : Color.Yellow;
                    if (jointColor != jointBoxes[idx].Color)
                    {
                        jointBoxes[idx].Color = jointColor;
                        jointBoxes[idx].updateValues();
                    }

                }

                //Armar los huesos entre dos joints
                idx = -1;
                foreach (BoneOrientation bone in skeleton.BoneOrientations)
                {
                    idx++;

                    //Indice de origen y de destino
                    int n1 = (int)bone.StartJoint;
                    int n2 = (int)bone.EndJoint;

                    //Actualizar posiciones
                    jointLines[idx].PStart = jointBoxes[n1].Position;
                    jointLines[idx].PEnd = jointBoxes[n2].Position;
                    jointLines[idx].updateValues();
                }

            }

            //Render
            for (int i = 0; i < jointLines.Length; i++)
            {
                jointLines[i].render();
            }

            for (int i = 0; i < jointBoxes.Length; i++)
            {
                jointBoxes[i].render();
            }
        }

        
        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void dispose()
        {
            if (jointBoxes != null)
            {
                for (int i = 0; i < jointLines.Length; i++)
                {
                    jointLines[i].dispose();
                }

                for (int i = 0; i < jointBoxes.Length; i++)
                {
                    jointBoxes[i].dispose();
                }
            }
            
        }


    }
}

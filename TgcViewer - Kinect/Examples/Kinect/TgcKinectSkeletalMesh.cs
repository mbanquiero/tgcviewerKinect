using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSkeletalAnimation;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using Microsoft.Kinect;

namespace Examples.Kinect
{
    public class TgcKinectSkeletalMesh : TgcSkeletalMesh
    {

        Vector3[] kinectBonePos;
        /// <summary>
        /// Posiciones relativas a la pelvis de cada hueso.
        /// Sacadas del esqueleto de Kinect.
        /// Tienen que matchear con los huesos del esqueleto del mesh (no con los de kinect)
        /// </summary>
        public Vector3[] KinectBonePos
        {
            get { return kinectBonePos; }
            set { kinectBonePos = value; }
        }

        Skeleton kinectSkeleton;
        /// <summary>
        /// Esqueleto de kinect
        /// </summary>
        public Skeleton KinectSkeleton
        {
            get { return kinectSkeleton; }
            set { kinectSkeleton = value; }
        }

        List<Tuple<JointType, int>> kinectBonesMapping;
        /// <summary>
        /// Mapeo de huesos de kinect con huesos del mesh
        /// </summary>
        public List<Tuple<JointType, int>> KinectBonesMapping
        {
            get { return kinectBonesMapping; }
            set { kinectBonesMapping = value; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public TgcKinectSkeletalMesh(Mesh mesh, string name, MeshRenderType renderType, TgcSkeletalBone[] bones)
            : base(mesh, name, renderType, bones)
        {
            kinectBonesMapping = new List<Tuple<JointType, int>>();
            this.currentAnimation = new TgcSkeletalAnimation("kinectAnimation", 30, 2, null, null);
        }


        /// <summary>
        /// Actualiza el cuadro actual de animacion y renderiza la malla.
        /// Es equivalente a llamar a updateAnimation() y luego a render()
        /// </summary>
        public new void animateAndRender()
        {
            if (!enabled)
                return;

            updateAnimation();
            render();
        }

        /// <summary>
        /// Actualiza el cuadro actual de la animacion.
        /// Debe ser llamado en cada cuadro antes de render()
        /// </summary>
        public new void updateAnimation()
        {
            Device device = GuiController.Instance.D3dDevice;
            float elapsedTime = GuiController.Instance.ElapsedTime;

            //Ver que haya transcurrido cierta cantidad de tiempo
            if (elapsedTime < 0.0f)
            {
                return;
            }

            //Sumo el tiempo transcurrido
            currentTime += elapsedTime;

            //Se termino la animacion
            if (currentTime > animationTimeLenght)
            {
                //Ver si hacer loop
                if (playLoop)
                {
                    //Dejar el remanente de tiempo transcurrido para el proximo loop
                    currentTime = currentTime % animationTimeLenght;
                    //setSkleletonLastPose();
                    //updateMeshVertices();
                }
                else
                {

                    //TODO: Puede ser que haya que quitar este stopAnimation() y solo llamar al Listener (sin cargar isAnimating = false)

                    stopAnimation();
                }
            }

            //La animacion continua
            else
            {
                //Actualizar esqueleto y malla
                updateKinectData();
                updateSkeleton();
                updateMeshVertices();
            }
        }

        /// <summary>
        /// Tomar datos de esqueleto de kinect
        /// </summary>
        protected void updateKinectData()
        {
            for (int i = 0; i < kinectBonesMapping.Count; i++)
            {
                Tuple<JointType, int> mapping = kinectBonesMapping[i];
                SkeletonPoint p = kinectSkeleton.Joints[mapping.Item1].Position;
                Vector3 bonePos = new Vector3(p.X, p.Y, p.Z);

                kinectBonePos[mapping.Item2] = bonePos;
            }
        }


        /// <summary>
        /// Actualiza la posicion de cada hueso del esqueleto segun las posiciones absolutas cargadas en kinectBonePos 
        /// </summary>
        protected new void updateSkeleton()
        {
            //Actualizar huesos del esqueleto segun lo que viene de kinect
            for (int i = 0; i < kinectBonePos.Length; i++)
            {
                TgcSkeletalBone bone = bones[i];

                Matrix localM = Matrix.Translation(kinectBonePos[i]);

                //Multiplicar por la matriz del padre, si tiene
                if (bone.ParentBone != null)
                {
                    bone.MatFinal = localM * bone.ParentBone.MatFinal;
                }
                else
                {
                    bone.MatFinal = localM;
                }
            }
        }


        /// <summary>
        /// Factory para crear una instancia de TgcKinectSkeletalMesh
        /// </summary>
        public class MeshFactory : TgcSkeletalLoader.IMeshFactory
        {
            public TgcSkeletalMesh createNewMesh(Mesh d3dMesh, string meshName, TgcSkeletalMesh.MeshRenderType renderType, TgcSkeletalBone[] bones)
            {
                return new TgcKinectSkeletalMesh(d3dMesh, meshName, renderType, bones);
            }
        }


    }
}

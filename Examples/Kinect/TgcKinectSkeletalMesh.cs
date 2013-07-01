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
    /// <summary>
    /// Mesh con animacion esqueletica adaptado para Kinect
    /// </summary>
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
        }

        float boneScale;
        /// <summary>
        /// Escala para huesos
        /// </summary>
        public float BoneScale
        {
            get { return boneScale; }
            set { boneScale = value; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public TgcKinectSkeletalMesh(Mesh mesh, string name, MeshRenderType renderType, TgcSkeletalBone[] bones)
            : base(mesh, name, renderType, bones)
        {
            kinectBonesMapping = new List<Tuple<JointType, int>>();
            this.currentAnimation = new TgcSkeletalAnimation("kinectAnimation", 30, 2, null, null);
            this.playLoop = true;
            this.kinectBonePos = new Vector3[bones.Length];
        }

        /// <summary>
        /// Cargar el mapeo de huesos del mesh con huesos de kinect
        /// </summary>
        /// <param name="mapping">Relaciona un hueso de kinect con el nombre del hueso del mesh</param>
        public void setBonesMapping(List<Mapping> mapping)
        {
            foreach (Mapping m in mapping)
            {
                bool found = false;
                foreach (TgcSkeletalBone bone in bones)
                {
                    if (bone.Name == m.MeshBone)
                    {
                        found = true;
                        this.kinectBonesMapping.Add(new Tuple<JointType, int>(m.KinectBone, bone.Index));
                        break;
                    }
                }
                if (!found)
                    throw new Exception("No se encontro el hueso con el nombre: " + m.MeshBone);
            }
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

            //Actualizar esqueleto y malla
            updateKinectData();
            updateSkeleton();
            updateMeshVertices();
        }

        /// <summary>
        /// Tomar datos de esqueleto de kinect
        /// </summary>
        protected void updateKinectData()
        {
            for (int i = 0; i < kinectBonesMapping.Count; i++)
            {
                Tuple<JointType, int> mapping = kinectBonesMapping[i];
                Vector3 kBonePos = TgcKinectUtils.toVector3(kinectSkeleton.Joints[mapping.Item1].Position);
                kinectBonePos[mapping.Item2] = kBonePos;
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
                Vector3 kinectPos = kinectBonePos[i];
                Vector3 diff = kinectPos - bone.StartPosition;

                Matrix localM = Matrix.Translation(diff);

                bone.MatLocal = localM;
                bone.MatFinal = localM;

            }
        }

        /// <summary>
        /// Actualizar los vertices de la malla segun las posiciones del los huesos del esqueleto
        /// </summary>
        protected void updateMeshVertices()
        {
            /*
            //Precalcular la multiplicación para llevar a un vertice a Bone-Space y luego transformarlo segun el hueso
            //Estas matrices se envian luego al Vertex Shader para hacer skinning en GPU
            for (int i = 0; i < bones.Length; i++)
            {
                TgcSkeletalBone bone = bones[i];
                boneSpaceFinalTransforms[i] = bone.MatFinal;
            }*/
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

        /// <summary>
        /// Mapeo entre un hueso del mesh y uno de kinect
        /// </summary>
        public class Mapping
        {
            string meshBone;
            /// <summary>
            /// Nombre del hueso del mesh
            /// </summary>
            public string MeshBone
            {
              get { return meshBone; }
              set { meshBone = value; }
            }

            JointType kinectBone;
            /// <summary>
            /// Hueso de kinect
            /// </summary>
            public JointType KinectBone
            {
                get { return kinectBone; }
                set { kinectBone = value; }
            }

            public Mapping(string meshBone, JointType kinectBone)
            {
                this.meshBone = meshBone;
                this.kinectBone = kinectBone;
            }
        }

    }
}

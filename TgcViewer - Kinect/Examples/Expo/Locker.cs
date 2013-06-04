using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using Microsoft.DirectX;

namespace Examples.Expo
{
    /// <summary>
    /// Cajon que solo se mueve en Z
    /// </summary>
    public class Locker
    {

        TgcMesh mesh;
        /// <summary>
        /// Mesh del cajon
        /// </summary>
        public TgcMesh Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }

        TgcBoundingSphere handleSphere;
        /// <summary>
        /// BoundingSphere de colision para la manija del cajon
        /// </summary>
        public TgcBoundingSphere HandleSphere
        {
            get { return handleSphere; }
            set { handleSphere = value; }
        }

        float handleMaxZ;
        /// <summary>
        /// Maximo valor de Z al que puede llegar el cajon
        /// </summary>
        public float HandleMaxZ
        {
            get { return handleMaxZ; }
            set { handleMaxZ = value; }
        }

        float timeToCatch;
        /// <summary>
        /// Tiempo necesario que tiene que colisionar la mano para empezar el agarra
        /// </summary>
        public float TimeToCatch
        {
            get { return timeToCatch; }
            set { timeToCatch = value; }
        }

        bool caught;
        /// <summary>
        /// Indica si esta agarrado
        /// </summary>
        public bool Caught
        {
            get { return caught; }
        }

        TgcBoundingSphere boneSphere;
        /// <summary>
        /// BoundingSphere del hueso que esta agarrando el cajon
        /// </summary>
        public TgcBoundingSphere BoneSphere
        {
            get { return boneSphere; }
        }

        float handleMinZ;
        bool colliding;
        float initCollisionTime;
        float lastBonePosZ;

        public Locker()
        {
            timeToCatch = 0.25f;
        }

        public void init()
        {
            caught = false;
            colliding = false;
            handleMinZ = handleSphere.Center.Z;
        }

        public void update(TgcBoundingSphere someBoneSphere)
        {
            bool collisionTest = TgcCollisionUtils.testSphereSphere(someBoneSphere, handleSphere);

            //No esta agarrado
            if (!caught)
            {
                if (collisionTest)
                {
                    if (!colliding)
                    {
                        colliding = true;
                        initCollisionTime = 0;
                        boneSphere = someBoneSphere;
                    }
                    if (colliding && someBoneSphere.Equals(boneSphere))
                    {
                        initCollisionTime += GuiController.Instance.ElapsedTime;
                    }
                    

                    if (colliding && initCollisionTime >= timeToCatch)
                    {
                        caught = true;
                        lastBonePosZ = boneSphere.Center.Z;
                    }

                }
            }
            //Agarrado
            else
            {
                if (!collisionTest)
                {
                    colliding = false;
                    caught = false;
                    boneSphere = null;
                }
                else
                {
                    float diffZ = boneSphere.Center.Z - lastBonePosZ;
                    lastBonePosZ = boneSphere.Center.Z;
                    float currentZ = handleSphere.Center.Z;
                    float nextZ = currentZ + diffZ;

                    //Chequear umbrales
                    if (nextZ < handleMinZ)
                    {
                        diffZ = handleMinZ - currentZ;
                    }
                    else if (nextZ > handleMaxZ)
                    {
                        diffZ = handleMaxZ - currentZ;
                        caught = false;
                        boneSphere = null;
                        colliding = false;
                    }

                    //Mover
                    mesh.move(0, 0, diffZ);
                    handleSphere.moveCenter(new Vector3(0, 0, diffZ));

                }
            }


                
        }
        


    }
}

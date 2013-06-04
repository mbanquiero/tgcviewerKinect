using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Examples.Kinect;
using TgcViewer;
using Microsoft.DirectX;

namespace Examples.Expo
{
    /// <summary>
    /// Cajon que solo se mueve en Z, con gesto
    /// </summary>
    public class GestureLocker
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

        int gestureFrames;
        /// <summary>
        /// Cantidad de cuadros necesarios para trackear como valido los gestos de abrir o cerrar
        /// </summary>
        public int GestureFrames
        {
            get { return gestureFrames; }
            set { gestureFrames = value; }
        }

        float movementSpeed;
        /// <summary>
        /// Velocidad de apertura/cierre de cajon
        /// </summary>
        public float MovementSpeed
        {
            get { return movementSpeed; }
            set { movementSpeed = value; }
        }

        /// <summary>
        /// Estados del cajon
        /// </summary>
        public enum LockerState
        {
            Closed,
            OpeningGesture,
            Opening,
            Opened,
            ClosingGesture,
            Closing,
        }

        LockerState currentState;
        float handleMinZ;
        bool rightHand;
        int gestureDetectedFrames;

        public GestureLocker()
        {
            gestureFrames = 5;
            movementSpeed = 10;
        }


        public void init()
        {
            currentState = LockerState.Closed;
            handleMinZ = handleSphere.Center.Z;
        }

        public void update(TgcKinectSkeletonData data)
        {
            bool collisionTest;
            TgcBoundingSphere handCurrent;
            TgcBoundingSphere handPrevious;
            float diffZ;
            float distX;
            float distY;
            float maxDist = 2 * handleSphere.Radius;
            float movement;
            float correction;

            switch (currentState)
            {
                //Cajon cerrado: vemos si hay colision que inicie el gesto de abrir cajon
                case LockerState.Closed:

                    //Ver si hay colision con mano derecha
                    collisionTest = TgcCollisionUtils.testSphereSphere(data.Current.RightHandSphere, handleSphere);
                    if (collisionTest)
                    {
                        currentState = LockerState.OpeningGesture;
                        rightHand = true;
                        gestureDetectedFrames = 0;
                    }
                    else
                    {
                        //Ver si hay colision con mano izquierda
                        collisionTest = TgcCollisionUtils.testSphereSphere(data.Current.LeftHandSphere, handleSphere);
                        if (collisionTest)
                        {
                            currentState = LockerState.OpeningGesture;
                            rightHand = false;
                            gestureDetectedFrames = 0;
                        }
                    }
                    break;

                //Analizando gesto de abrir cajon
                case LockerState.OpeningGesture:
                    handCurrent = rightHand ? data.Current.RightHandSphere : data.Current.LeftHandSphere;
                    handPrevious = rightHand ? data.Previous.RightHandSphere : data.Previous.LeftHandSphere;
                    diffZ = handCurrent.Center.Z - handPrevious.Center.Z;
                    distX = FastMath.Abs(handCurrent.Center.X - handleSphere.Center.X);
                    distY = FastMath.Abs(handCurrent.Center.Y - handleSphere.Center.Y);

                    //Ver si esta haciendo un movimiento positivo en Z, dentro de un rango fijo de XY
                    if (diffZ >= 0 && distX <= maxDist && distY <= maxDist)
                    {
                        gestureDetectedFrames++;
                        //Ver si se cumplieron los frames necesarios
                        if (gestureDetectedFrames == gestureFrames)
                        {
                            currentState = LockerState.Opening;
                        }
                    }
                    else
                    {
                        //Volver a estado cerrado
                        currentState = LockerState.Closed;
                    }
                    break;

                //Hacer animacion de abrir cajon
                case LockerState.Opening:
                    movement = movementSpeed * GuiController.Instance.ElapsedTime;

                    //Mover
                    mesh.move(0, 0, movement);
                    handleSphere.moveCenter(new Vector3(0, 0, movement));

                    //Ver si llegamos al limite
                    if (handleSphere.Center.Z >= handleMaxZ)
                    {
                        //Corregir lo que nos pasamos
                        correction = handleSphere.Center.Z - handleMaxZ;
                        mesh.move(0, 0, -correction);
                        handleSphere.moveCenter(new Vector3(0, 0, -correction));

                        //Pasar a estado abierto
                        currentState = LockerState.Opened;
                    }

                    break;

                //Cajon abierto: vemos si hay colision que inicie el gesto de cerrar cajon
                case LockerState.Opened:

                    //Ver si hay colision con mano derecha
                    collisionTest = TgcCollisionUtils.testSphereSphere(data.Current.RightHandSphere, handleSphere);
                    if (collisionTest)
                    {
                        currentState = LockerState.ClosingGesture;
                        rightHand = true;
                        gestureDetectedFrames = 0;
                    }
                    else
                    {
                        //Ver si hay colision con mano izquierda
                        collisionTest = TgcCollisionUtils.testSphereSphere(data.Current.LeftHandSphere, handleSphere);
                        if (collisionTest)
                        {
                            currentState = LockerState.ClosingGesture;
                            rightHand = false;
                            gestureDetectedFrames = 0;
                        }
                    }
                    break;

                //Analizando gesto de cerrar cajon
                case LockerState.ClosingGesture:
                    handCurrent = rightHand ? data.Current.RightHandSphere : data.Current.LeftHandSphere;
                    handPrevious = rightHand ? data.Previous.RightHandSphere : data.Previous.LeftHandSphere;
                    diffZ = handCurrent.Center.Z - handPrevious.Center.Z;
                    distX = FastMath.Abs(handCurrent.Center.X - handleSphere.Center.X);
                    distY = FastMath.Abs(handCurrent.Center.Y - handleSphere.Center.Y);

                    //Ver si esta haciendo un movimiento negativo en Z, dentro de un rango fijo de XY
                    if (diffZ <= 0 && distX <= maxDist && distY <= maxDist)
                    {
                        gestureDetectedFrames++;
                        //Ver si se cumplieron los frames necesarios
                        if (gestureDetectedFrames == gestureFrames)
                        {
                            currentState = LockerState.Closing;
                        }
                    }
                    else
                    {
                        //Volver a estado abierto
                        currentState = LockerState.Opened;
                    }
                    break;

                //Hacer animacion de cerrar cajon
                case LockerState.Closing:
                     movement = -movementSpeed * GuiController.Instance.ElapsedTime;

                    //Mover
                    mesh.move(0, 0, movement);
                    handleSphere.moveCenter(new Vector3(0, 0, movement));

                    //Ver si llegamos al limite
                    if (handleSphere.Center.Z <= handleMinZ)
                    {
                        //Corregir lo que nos pasamos
                        correction = handleSphere.Center.Z - handleMinZ;
                        mesh.move(0, 0, -correction);
                        handleSphere.moveCenter(new Vector3(0, 0, -correction));

                        //Pasar a estado cerrado
                        currentState = LockerState.Closed;
                    }
                    break;
            }
        }

    }
}

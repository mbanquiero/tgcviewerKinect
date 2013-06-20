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
        /// <summary>
        /// Tiempo de espera luego de haberse abierto o cerrado
        /// </summary>
        const float WAIT_TIME = 1;

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
            Opening,
            Opened,
            Closing,
        }

        LockerState currentState;
        float handleMinZ;
        float waintElapsedTime;


        public GestureLocker()
        {
            movementSpeed = 100;
        }


        public void init()
        {
            currentState = LockerState.Closed;
            handleMinZ = handleSphere.Center.Z;
            waintElapsedTime = 0;
        }

        /// <summary>
        /// Abrir cajon
        /// </summary>
        public void open()
        {
            currentState = LockerState.Opening;
        }

        /// <summary>
        /// Cerrar cajon
        /// </summary>
        public void close()
        {
            currentState = LockerState.Closing;
        }

        /// <summary>
        /// Actualizar estado
        /// </summary>
        public void update()
        {
            float maxDist = 2 * handleSphere.Radius;
            float movement;
            float correction;

            switch (currentState)
            {
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
                        waintElapsedTime = 0;
                    }

                    break;

                //Abierto
                case LockerState.Opened:
                    waintElapsedTime += GuiController.Instance.ElapsedTime;
                    if (waintElapsedTime > WAIT_TIME)
                    {
                        waintElapsedTime = WAIT_TIME;
                    }
                    break;

                //Cerrado
                case LockerState.Closed:
                    waintElapsedTime += GuiController.Instance.ElapsedTime;
                    if (waintElapsedTime > WAIT_TIME)
                    {
                        waintElapsedTime = WAIT_TIME;
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
                        waintElapsedTime = 0;
                    }
                    break;
            }
        }

        /// <summary>
        /// Indica si el gesto es valido para el cajon
        /// </summary>
        public bool validateGesture(Gesture gesture)
        {
            bool result = false;
            switch (gesture.Type)
            {
                //Abrir
                case GestureType.OpenZ:
                    //Ver que este en el estado correcto y que el gesto haya sido cerca de la manija
                    if (currentState == LockerState.Closed 
                        && waintElapsedTime >= WAIT_TIME
                        && FastMath.Abs(gesture.Pos.X - handleSphere.Center.X) < handleSphere.Radius
                        && FastMath.Abs(gesture.Pos.Y - handleSphere.Center.Y) < handleSphere.Radius)
                    {
                        result = true;
                    }
                    break;

                //Cerrar
                case GestureType.CloseZ:
                    //Ver que este en el estado correcto y que el gesto haya sido cerca de la manija
                    if (currentState == LockerState.Opened
                        && waintElapsedTime >= WAIT_TIME
                        && FastMath.Abs(gesture.Pos.X - handleSphere.Center.X) < handleSphere.Radius
                        && FastMath.Abs(gesture.Pos.Y - handleSphere.Center.Y) < handleSphere.Radius)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

    }
}

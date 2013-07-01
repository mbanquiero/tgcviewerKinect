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
    /// Puerta que se abre de derecha a izquierda
    /// </summary>
    public class GestureDoorLeft
    {
        /// <summary>
        /// Tiempo de espera luego de haberse abierto o cerrado
        /// </summary>
        const float WAIT_TIME = 1;

        TgcMesh mesh;
        /// <summary>
        /// Mesh de la puerta
        /// </summary>
        public TgcMesh Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }

        float maxOpenAngle;
        /// <summary>
        /// Maximo angulo al que se puede abrir la puerta (en radianes)
        /// </summary>
        public float MaxOpenAngle
        {
            get { return maxOpenAngle; }
            set { maxOpenAngle = value; }
        }

        float rotationSpeed;
        /// <summary>
        /// Velocidad de apertura/cierre de puerta, en angulos en radianes
        /// </summary>
        public float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        /// <summary>
        /// Estados de la puerta
        /// </summary>
        public enum DoorState
        {
            Closed,
            Opening,
            Opened,
            Closing,
        }

        DoorState currentState;
        float waintElapsedTime;


        public GestureDoorLeft()
        {
            rotationSpeed = FastMath.PI / 10f;
        }


        public void init()
        {
            currentState = DoorState.Closed;
            waintElapsedTime = 0;
            mesh.AutoTransformEnable = false;
        }

        /// <summary>
        /// Abrir puerta
        /// </summary>
        public void open()
        {
            currentState = DoorState.Opening;
        }

        /// <summary>
        /// Cerrar puerta
        /// </summary>
        public void close()
        {
            currentState = DoorState.Closing;
        }

        /// <summary>
        /// Actualizar estado
        /// </summary>
        public void update()
        {
            float rotation;
            float lastRotY;

            switch (currentState)
            {
                //Hacer animacion de abrir cajon
                case DoorState.Opening:
                    rotation = -rotationSpeed * GuiController.Instance.ElapsedTime;
                    lastRotY = mesh.Rotation.Y;
                    mesh.Rotation = new Vector3(0, lastRotY + rotation, 0);

                    //Ver si llegamos al limite
                    if (mesh.Rotation.Y <= -maxOpenAngle)
                    {
                        mesh.Rotation = new Vector3(0, maxOpenAngle - lastRotY, 0);

                        //Pasar a estado abierto
                        currentState = DoorState.Opened;
                        waintElapsedTime = 0;
                    }

                    //Rotar
                    //Vector3 meshExtents = mesh.BoundingBox.calculateAxisRadius();
                    mesh.Transform = Matrix.Translation(-mesh.Position) 
                        * Matrix.RotationY(rotation)
                        * Matrix.Translation(mesh.Position);
                        
                    break;

                //Abierto
                case DoorState.Opened:
                    waintElapsedTime += GuiController.Instance.ElapsedTime;
                    if (waintElapsedTime > WAIT_TIME)
                    {
                        waintElapsedTime = WAIT_TIME;
                    }
                    break;

                //Cerrado
                case DoorState.Closed:
                    waintElapsedTime += GuiController.Instance.ElapsedTime;
                    if (waintElapsedTime > WAIT_TIME)
                    {
                        waintElapsedTime = WAIT_TIME;
                    }
                    break;

                //Hacer animacion de cerrar cajon
                case DoorState.Closing:
                    rotation = rotationSpeed * GuiController.Instance.ElapsedTime;
                    lastRotY = mesh.Rotation.Y;
                    mesh.Rotation = new Vector3(0, lastRotY + rotation, 0);

                    //Ver si llegamos al limite
                    if (mesh.Rotation.Y >= 0)
                    {
                        mesh.Rotation = new Vector3(0, 0 - lastRotY, 0);

                        //Pasar a estado abierto
                        currentState = DoorState.Opened;
                        waintElapsedTime = 0;
                    }

                    //Rotar
                    //Vector3 meshExtents = mesh.BoundingBox.calculateAxisRadius();
                    mesh.Transform = Matrix.Translation(-mesh.Position) 
                        * Matrix.RotationY(rotation)
                        * Matrix.Translation(mesh.Position);
                        
                    break;
            }
        }

        /// <summary>
        /// Indica si el gesto es valido para el cajon
        /// </summary>
        public bool validateGesture(Gesture gesture)
        {
            bool result = false;
            Vector3 doorCenter = mesh.BoundingBox.calculateBoxCenter();

            switch (gesture.Type)
            {
                //Abrir
                case GestureType.OpenLeft:
                    //Ver que este en el estado correcto y que el gesto haya sido en el centro de la puerta
                    if (currentState == DoorState.Closed 
                        && waintElapsedTime >= WAIT_TIME
                        && FastMath.Abs(gesture.Pos.X - doorCenter.X) < 10f
                        && FastMath.Abs(gesture.Pos.Y - doorCenter.Y) < 10f)
                    {
                        result = true;
                    }
                    break;

                //Cerrar
                case GestureType.OpenRight:
                    //Ver que este en el estado correcto y que el gesto haya sido en el centro de la puerta
                    if (currentState == DoorState.Opened
                        && waintElapsedTime >= WAIT_TIME
                        && FastMath.Abs(gesture.Pos.X - doorCenter.X) < 10f
                        && FastMath.Abs(gesture.Pos.Y - doorCenter.Y) < 10f)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

    }
}

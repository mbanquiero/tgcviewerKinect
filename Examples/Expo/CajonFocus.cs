using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Examples.Kinect;
using TgcViewer;
using Microsoft.DirectX;
using Examples.Focus;

namespace Examples.Expo
{
    /// <summary>
    /// Cajon de Focus que se mueve
    /// </summary>
    public class CajonFocus
    {
        /// <summary>
        /// Tiempo de espera luego de haberse abierto o cerrado
        /// </summary>
        const float WAIT_TIME = 1;

        FocusSet conjunto;
        /// Conjunto del cajon
        /// </summary>
        public FocusSet Conjunto
        {
            get { return conjunto; }
            set { conjunto = value; }
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
        public enum State
        {
            Closed,
            Opening,
            Opened,
            Closing,
        }

        State currentState;
        /// <summary>
        /// Estado
        /// </summary>
        public State CurrentState
        {
            get { return currentState; }
        }


        float waintElapsedTime;
        float currentMovement;
        Vector3 originalPos;
        Vector3 conjuntoCenter;


        public CajonFocus()
        {
            movementSpeed = 10f;
        }


        public void init()
        {
            currentState = State.Closed;
            waintElapsedTime = 0;
            currentMovement = 0;
            originalPos = conjunto.container.Position;

            TgcBoundingBox aabb = conjunto.container.createBoundingBox();
            conjuntoCenter = aabb.calculateBoxCenter();
        }

        /// <summary>
        /// Abrir cajon
        /// </summary>
        public void open()
        {
            currentState = State.Opening;
        }

        /// <summary>
        /// Cerrar cajon
        /// </summary>
        public void close()
        {
            currentState = State.Closing;
        }

        /// <summary>
        /// Actualizar estado
        /// </summary>
        public void update()
        {
            float movement;
            Vector3 absVector = getMovementVec();

            switch (currentState)
            {
                //Hacer animacion de abrir cajon
                case State.Opening:
                    movement = movementSpeed * conjunto.Max * GuiController.Instance.ElapsedTime;
                    currentMovement += movement;

                    //Llegamos al umbral
                    if (currentMovement > conjunto.Max)
                    {
                        //Ajustar mesh hasta el final
                        conjunto.container.Position = conjunto.Max * absVector;

                        //Pasar a estado abierto
                        currentState = State.Opened;
                        waintElapsedTime = 0;
                        currentMovement = 0;
                    }
                    else
                    {
                        //Mover
                        conjunto.container.move(movement * absVector);
                    }

                    break;

                //Abierto
                case State.Opened:
                    waintElapsedTime += GuiController.Instance.ElapsedTime;
                    if (waintElapsedTime > WAIT_TIME)
                    {
                        waintElapsedTime = WAIT_TIME;
                    }
                    break;

                //Cerrado
                case State.Closed:
                    waintElapsedTime += GuiController.Instance.ElapsedTime;
                    if (waintElapsedTime > WAIT_TIME)
                    {
                        waintElapsedTime = WAIT_TIME;
                    }
                    break;

                //Hacer animacion de cerrar cajon
                case State.Closing:
                     movement = movementSpeed * conjunto.Max * GuiController.Instance.ElapsedTime;
                    currentMovement += movement;

                    //Llegamos al umbral
                    if (currentMovement > conjunto.Max)
                    {
                        //Ajustar mesh hasta el inicio
                        conjunto.container.Position = originalPos;

                        //Pasar a estado cerrado
                        currentState = State.Closed;
                        waintElapsedTime = 0;
                        currentMovement = 0;
                    }
                    else
                    {
                        //Mover
                        conjunto.container.move(-movement * absVector);
                    }
                    break;
            }
        }


        public void render()
        {
            conjunto.container.render();
        }

        /// <summary>
        /// Vector de movimiento del cajon
        /// </summary>
        public Vector3 getMovementVec()
        {
            return new Vector3(conjunto.Vector.X * conjunto.Dir.X + conjunto.Vector.Z * conjunto.Normal.X, conjunto.Vector.Y, conjunto.Vector.X * conjunto.Dir.Z + conjunto.Vector.Z * conjunto.Normal.Z);
        }

        
        /// <summary>
        /// Devuelve el centro del BoundingBox de todo el conjunto de meshes del cajon proyectado a la pantalla.
        /// </summary>
        public Vector2 getScreenCenter()
        {
            //Mover el centro del conjunto si esta abierto
            Vector3 pos = conjuntoCenter;
            if (currentState == State.Opened || currentState == State.Opening)
            {
                pos = getMovementVec() * conjunto.Max;
            }

            return TgcKinectUtils.projectPoint(pos);
        }
        

        /*
        /// <summary>
        /// Distancia entre la pos XY (en 3D) del gesto y la pos XY (en 3D) del centro del conjunto
        /// </summary>
        public float distanceSq(Vector2 pos)
        {
            //Mover el centro del conjunto si esta abierto
            Vector3 posConjunto = conjuntoCenter;
            if (currentState == State.Opened || currentState == State.Opening)
            {
                posConjunto = getMovementVec() * conjunto.Max;
            }

            Vector2 posConjunto2D = new Vector2(posConjunto.X, posConjunto.Y);
            return Vector2.LengthSq(posConjunto2D - pos);
        }
        */

        /*
        public float distanceSq(Vector3 pos)
        {
            //Mover el centro del conjunto si esta abierto
            Vector3 posConjunto = conjuntoCenter;
            if (currentState == State.Opened || currentState == State.Opening)
            {
                posConjunto = getMovementVec() * conjunto.Max;
            }

            return Vector3.LengthSq(posConjunto - pos);
        }
        */
    }
}

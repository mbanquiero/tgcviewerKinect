using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using Examples.Kinect;

namespace TgcViewer.Utils.Gui
{
    
    // interface con la kinect
    public struct st_hand
    {
        public Vector3 position;
        public bool gripping;
        public bool visible;
    }

    public enum Gesture
    {
        Nothing,
        Pressing,
    }


    public class kinect_input
    {
        // posicion y status de cada mano
        public st_hand left_hand = new st_hand();
        public st_hand right_hand = new st_hand();
        public bool right_hand_sel = true;
        public Gesture currentGesture = Gesture.Nothing;
        public TgcKinectSkeletonData kinectData;
        public bool hay_sensor = false;             // indica si hay una kinect connectada
        public int MOUSE_SNAP = 40;
        public DXGui gui;

        public kinect_input(DXGui p_gui)
        {
            gui = p_gui;

            left_hand.position = new Vector3(0,0,0);
            left_hand.gripping = false;
            left_hand.visible = true;

            right_hand.position = new Vector3(0, 0, 0);
            right_hand.gripping = false;
            right_hand.visible = true;
        }


        public void SetCursorPos()
        {
            // llevo el mouse a la posicion de la mano
            Point cursorPos;
            if (right_hand_sel)
                cursorPos = new Point((int)right_hand.position.X, (int)right_hand.position.Y);
            else
                cursorPos = new Point((int)left_hand.position.X, (int)left_hand.position.Y);
            System.Windows.Forms.Cursor.Position = GuiController.Instance.Panel3d.PointToScreen(cursorPos);
        }

        public void GetInputFromMouse()
        {
            if (hay_sensor && kinectData != null)
            {
                // Verifico si el mouse esta sobre un boton seleccionable
                if (gui.IsHotRegion((int)kinectData.Current.RightHandPos.X, (int)kinectData.Current.RightHandPos.X))
                {
                    int dx = (int)Math.Abs(right_hand.position.X - kinectData.Current.RightHandPos.X);
                    int dy = (int)Math.Abs(right_hand.position.Y - kinectData.Current.RightHandPos.Y);
                    if (dx >= MOUSE_SNAP || dy >= MOUSE_SNAP)
                    {
                        if (dx > dy)
                            right_hand.position.X = kinectData.Current.RightHandPos.X;
                        else
                            right_hand.position.Y = kinectData.Current.RightHandPos.Y;
                    }
                }
                else
                {
                    // Movimiento continuo
                    right_hand.position.X = kinectData.Current.RightHandPos.X;
                    right_hand.position.Y = kinectData.Current.RightHandPos.Y;
                }
                right_hand.position.Z = 1;

                left_hand.position.X = kinectData.Current.LefttHandPos.X;
                left_hand.position.Y = kinectData.Current.LefttHandPos.Y;
                left_hand.position.Z = 1;
            }
            else
            {
                // Simula el sensor con el mouse y el teclado
                // boton derecho cambia de mano
                if (GuiController.Instance.D3dInput.buttonPressed(Input.TgcD3dInput.MouseButtons.BUTTON_RIGHT))
                {
                    right_hand_sel = !right_hand_sel;
                    // llevo el mouse a la posicion de la mano
                    SetCursorPos();
                    // y termino de procesar
                    return;
                }

                //st_hand hand = right_hand_sel ? right_hand : left_hand;

                float sx = GuiController.Instance.D3dInput.Xpos;
                float sy = GuiController.Instance.D3dInput.Ypos;
                float sz = GuiController.Instance.D3dInput.WheelPos;

                if (right_hand_sel)
                {
                    right_hand.position.X = sx;
                    right_hand.position.Y = sy;
                    right_hand.position.Z = sz;
                }
                else
                {
                    left_hand.position.X = sx;
                    left_hand.position.Y = sy;
                    left_hand.position.Z = sz;
                }
            }
        }

        public void GestureRecognition()
        {
            currentGesture = Gesture.Nothing;

            if (hay_sensor && kinectData != null)
            {
                //Ver en que mano chequear gesto
                Microsoft.Kinect.JointType handIdx = right_hand_sel ? Microsoft.Kinect.JointType.HandRight : Microsoft.Kinect.JointType.HandLeft;
            }
            else
            {
                // Espacio para abrir / cerrar la mano
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                {
                    if (right_hand_sel)
                        right_hand.gripping = !right_hand.gripping;
                    else
                        left_hand.gripping = !left_hand.gripping;
                }

                // el wheel del mouse, representa la mano hacia atras, en el movimiento de seleccion
                if ((right_hand_sel && right_hand.position.Z > 0) || (!right_hand_sel && left_hand.position.Z > 0))
                    currentGesture = Gesture.Pressing;
            }
        }

    }


}

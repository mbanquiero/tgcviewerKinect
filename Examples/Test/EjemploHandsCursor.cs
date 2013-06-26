using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using Examples.Kinect;
using Microsoft.Kinect;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcSceneLoader;
using Examples.Expo;

namespace Examples.Test
{

    public class EjemploHandsCursor : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBoundingBox bounds;
        TgcBox center;
        TgcSprite leftHandPointer;
        TgcSprite rightHandPointer;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Hands Cursor";
        }

        public override string getDescription()
        {
            return "Ejemplo Hands Cursor";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Iniciar Kinect
            try
            {
                tgcKinect = new TgcKinect();
                tgcKinect.init();
                tgcKinect.DebugSkeleton.init();
            }
            catch (Exception)
            {
                GuiController.Instance.Logger.logError("Kinect not found");
            }
            


            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-9.1315f, 22.5574f, -41.5821f), new Vector3(-8.9167f, 22.2222f, -40.6648f));


            //Centro y tamaño de escena
            center = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
            bounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));

            //Imagenes para puntero del mouse
            leftHandPointer = new TgcSprite();
            leftHandPointer.Texture = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "left_pointer.png");
            rightHandPointer = new TgcSprite();
            rightHandPointer.Texture = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "right_pointer.png");

            GuiController.Instance.Modifiers.addFloat("speedX", 0.5f, 10f, 1f);
            GuiController.Instance.Modifiers.addFloat("speedY", 0.5f, 10f, 1f);
            GuiController.Instance.Modifiers.addBoolean("showValues", "showValues", false);
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Velocidad de movimiento 2D
            tgcKinect.Hands2dSpeed = new Vector2((float)GuiController.Instance.Modifiers["speedX"], (float)GuiController.Instance.Modifiers["speedY"]);

            //Actualizar estado de kinect
            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                //Render de esqueleto debug
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);


                //Debug de pos de manos
                bool showValues = (bool)GuiController.Instance.Modifiers["showValues"];
                if (showValues)
                {
                    BigLogger.log("RightHandPos", data.Current.RightHandPos);
                    BigLogger.log("LefttHandPos", data.Current.LefttHandPos);
                    BigLogger.renderLog();
                }
                


                //Dibujar cursores
                rightHandPointer.Position = data.Current.RightHandPos;
                leftHandPointer.Position = data.Current.LefttHandPos;
                GuiController.Instance.Drawer2D.beginDrawSprite();
                rightHandPointer.render();
                leftHandPointer.render();
                GuiController.Instance.Drawer2D.endDrawSprite();
            }


            //Dibujar limites de escena
            center.render();
            bounds.render();
        }

        




        public override void close()
        {
            tgcKinect.dispose();
            center.dispose();
            bounds.dispose();
            leftHandPointer.dispose();
            rightHandPointer.dispose();
        }

    }
}

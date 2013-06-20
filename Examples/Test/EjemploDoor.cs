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

    public class EjemploDoor : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBox mueble;
        GestureDoorLeft door;
        GestureAnalizer gestureAnalizer;
        TgcBoundingBox sceneBounds;
        Vector3 sceneCenter;
        TgcBox sceneCenterBox;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Door";
        }

        public override string getDescription()
        {
            return "Ejemplo Door";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Iniciar kinect
            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();

            //Analizador de gestos
            gestureAnalizer = new GestureAnalizer();
            sceneBounds = new TgcBoundingBox(new Vector3(-50, -40, 230), new Vector3(80, 50, 290));
            gestureAnalizer.setSceneBounds(sceneBounds);
            sceneCenter = sceneBounds.calculateBoxCenter();

            sceneCenterBox = TgcBox.fromSize(sceneCenter, new Vector3(30, 30, 30), Color.Blue);

            //Crear mueble de fondo
            mueble = TgcBox.fromSize(new Vector3(200, 100, 50), Color.SandyBrown);
            mueble.Position = new Vector3(20, 20, 200);

            //Crear puerta
            door = new GestureDoorLeft();
            door.Mesh = TgcBox.fromSize(new Vector3(30, 40, 4), Color.Green).toMesh("door");
            door.Mesh.Position = mueble.Position + new Vector3(-30, 20, 25);


            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1.5467f, 54.7247f, 401.1074f), new Vector3(1.4672f, 54.4561f, 400.1474f));
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Tomar tracking de kinect
            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                //Render de esqueleto
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);

                if (Vector3.Length(data.Current.CenterPos - sceneCenter) <= 30f)
                {
                    sceneCenterBox.Color = Color.Red;
                    sceneCenterBox.updateValues();

                    //Analizar gestos
                    Gesture gesture;
                    if (gestureAnalizer.analize(data, out gesture))
                    {
                        switch (gesture.Type)
                        {
                            case GestureType.OpenLeft:
                                if (door.validateGesture(gesture))
                                {
                                    door.open();
                                }
                                break;
                            case GestureType.OpenRight:
                                if (door.validateGesture(gesture))
                                {
                                    door.close();
                                }
                                break;
                        }
                    }


                }
                else
                {
                    sceneCenterBox.Color = Color.Blue;
                    sceneCenterBox.updateValues();
                }
            }

                


            //Dibujar puerta
            door.update();
            door.Mesh.render();

            //Dibujar BoundingSphere de manos del esqueleto
            data.Current.RightHandSphere.render();
            data.Current.LeftHandSphere.render(); 

            //Dibujar mueble
            mueble.render();

            sceneBounds.render();
            sceneCenterBox.render();
        }






        public override void close()
        {
            tgcKinect.dispose();
        }

    }
}

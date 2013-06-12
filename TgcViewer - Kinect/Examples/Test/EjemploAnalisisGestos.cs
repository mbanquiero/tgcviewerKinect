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

    public class EjemploAnalisisGestos : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBox mueble;
        List<GestureLocker2> cajones;
        GestureAnalizer gestureAnalizer;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Analisis Gestos";
        }

        public override string getDescription()
        {
            return "Ejemplo Analisis Gestos";
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

            //Crear mueble de fondo
            mueble = TgcBox.fromSize(new Vector3(200, 100, 50), Color.SandyBrown);
            mueble.Position = new Vector3(20, 20, 200);

            //Crear algunos cajones
            Vector3 muebleCenterPos = mueble.Position;
            cajones = new List<GestureLocker2>();
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(-30, 0, 2), new Vector3(50, 25, 50)));
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(0, 30, 2), new Vector3(50, 25, 50)));
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(30, 0, 2), new Vector3(50, 25, 50)));


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

                //Analizar gestos
                Gesture gesture;
                if (gestureAnalizer.analize(data, out gesture))
                {
                    switch (gesture.Type)
                    {
                        //Gesto de abrir cajon
                        case GestureType.OpenZ:
                            //Buscar si fue cerca de algun cajon valido
                            foreach (GestureLocker2 cajon in cajones)
                            {
                                if (cajon.validateGesture(gesture))
                                {
                                    cajon.open();
                                    break;
                                }
                            }
                            break;

                        //Gesto de cerrar cajon
                        case GestureType.CloseZ:
                            //Buscar si fue cerca de algun cajon valido
                            foreach (GestureLocker2 cajon in cajones)
                            {
                                if (cajon.validateGesture(gesture))
                                {
                                    cajon.close();
                                    break;
                                }
                            }
                            break;
                    }
                }

                
            }


            //Dibujar cajones
            foreach (GestureLocker2 l in cajones)
            {
                l.update();
                l.HandleSphere.render();
                l.Mesh.render();
                l.Mesh.BoundingBox.render();
            }

            //Dibujar BoundingSphere de manos del esqueleto
            data.Current.RightHandSphere.render();
            data.Current.LeftHandSphere.render(); 

            //Dibujar mueble
            mueble.render();
        }

        /// <summary>
        /// Utilidad para crear cajones
        /// </summary>
        private GestureLocker2 crearCajon(Vector3 pos, Vector3 size)
        {
            TgcBox cajonMesh = TgcBox.fromSize(size, Color.Green);
            cajonMesh.Position = pos;

            GestureLocker2 locker = new GestureLocker2();
            locker.HandleSphere = new TgcBoundingSphere(cajonMesh.Position + new Vector3(0, 0, cajonMesh.Size.Z / 2), 15);
            locker.HandleMaxZ = locker.HandleSphere.Center.Z + cajonMesh.Size.Z;
            locker.Mesh = cajonMesh.toMesh("cajon");
            locker.init();

            cajonMesh.dispose();
            return locker;
        }




        public override void close()
        {
            tgcKinect.dispose();
        }

    }
}

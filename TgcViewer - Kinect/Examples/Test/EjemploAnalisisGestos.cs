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
        List<GestureLocker> cajones;
        GestureAnalizer gestureAnalizer;
        TgcBoundingBox sceneBounds;
        TgcBox center;


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
            sceneBounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));
            gestureAnalizer.setSceneBounds(sceneBounds);

            //Crear mueble de fondo
            mueble = TgcBox.fromSize(new Vector3(20, 20, 5), Color.SandyBrown);
            mueble.Position = new Vector3(0, 10, -10);

            //Crear algunos cajones
            Vector3 muebleCenterPos = mueble.Position;
            cajones = new List<GestureLocker>();
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(-3, 0, 0.25f), new Vector3(5, 2, 5)));
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(0, 3, 0.25f), new Vector3(5, 2, 5)));
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(3, 0, 0.25f), new Vector3(5, 2, 5)));


            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-3.5508f, 16.5873f, 13.2958f), new Vector3(-3.535f, 16.3069f, 12.336f));

            center = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
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
                            foreach (GestureLocker cajon in cajones)
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
                            foreach (GestureLocker cajon in cajones)
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
            foreach (GestureLocker l in cajones)
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


            center.render();
            sceneBounds.render();
        }

        /// <summary>
        /// Utilidad para crear cajones
        /// </summary>
        private GestureLocker crearCajon(Vector3 pos, Vector3 size)
        {
            TgcBox cajonMesh = TgcBox.fromSize(size, Color.Green);
            cajonMesh.Position = pos;

            GestureLocker locker = new GestureLocker();
            locker.HandleSphere = new TgcBoundingSphere(cajonMesh.Position + new Vector3(0, 0, cajonMesh.Size.Z / 2), 1f);
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

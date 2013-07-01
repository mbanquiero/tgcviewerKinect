using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using Examples.Kinect;
using Microsoft.Kinect;
using TgcViewer.Utils.TgcGeometry;
using Examples.Expo;
using Examples.Focus;
using TgcViewer.Utils.TgcSceneLoader;
using System.Collections.Generic;
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace Examples.Test
{

    public class EjemploEsqueletoEnFocus : TgcExample
    {
        //Distancia minima en 2D (pixels) que se considera suficientemente proxima para detectar gesto sobre cajon/puerta
        const float CAJON_MIN_DIST_GESTO = 200;


        TgcKinect tgcKinect;
        TgcBoundingBox bounds;
        TgcBox center;
        private List<TgcMesh> _meshes;
        private FocusSet[] _conjuntos;
        GestureAnalizer gestureAnalizer;
        List<CajonFocus> cajones;


        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Esqueleto en Focus";
        }

        public override string getDescription()
        {
            return "Ejemplo Esqueleto en Focus";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Loader de kinect
            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();


            //Loader de focus
            FocusParser.TEXTURE_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            FocusParser.MESH_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            FocusParser loader = new FocusParser();
            string fileScene = GuiController.Instance.ExamplesMediaDir + "Focus\\escena1.dat";
            loader.FromFile(fileScene);
            _meshes = loader.Escene;
            _conjuntos = loader._focusSets;

            // Bounding box de la escena
            // Calculo el bounding box de la escena
            float x0 = 10000;
            float y0 = 10000;
            float z0 = 10000;
            float x1 = -10000;
            float y1 = -10000;
            float z1 = -10000;
            foreach (TgcMesh m in _meshes)
            {
                TgcBoundingBox box = m.BoundingBox;
                if (box.PMin.X < x0)
                    x0 = box.PMin.X;
                if (box.PMin.Y < y0)
                    y0 = box.PMin.Y;
                if (box.PMin.Z < z0)
                    z0 = box.PMin.Z;

                if (box.PMax.X > x1)
                    x1 = box.PMax.X;
                if (box.PMax.Y > y1)
                    y1 = box.PMax.Y;
                if (box.PMax.Z > z1)
                    z1 = box.PMax.Z;
            }

            bounds = new TgcBoundingBox(new Vector3(x0, y0, z0), new Vector3(x1, y1, z1));
            Vector3 c = bounds.calculateBoxCenter();
            c.Y = 0;
            center = TgcBox.fromSize(c, new Vector3(100, 100, 100), Color.Blue);


            //Escalas y centro de la escena para kinect
            tgcKinect.PositionScale = 1000;
            tgcKinect.sceneCenter = c;
            tgcKinect.skeletonOffsetY = 0.75f;


            //Analizador de gestos
            gestureAnalizer = new GestureAnalizer();
            gestureAnalizer.setSceneBounds(bounds);



            //Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed *= 10;
            GuiController.Instance.FpsCamera.JumpSpeed *= 10;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(2000f, 1600f, -4000f), new Vector3(2000f, 500f, 0));
            //GuiController.Instance.FpsCamera.setCamera(c, c + new Vector3(0, 0, 1));



            //Separar cajones del resto del FocusSet
            cajones = new List<CajonFocus>();
            foreach (FocusSet conjunto in _conjuntos)
            {
                if (conjunto.Tipo == FocusSet.TRASLACION)
                {
                    CajonFocus cajon = new CajonFocus();
                    cajon.Conjunto = conjunto;
                    cajon.init();
                    cajones.Add(cajon);
                }
            }


        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                //render esqueleto kinect
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);


                //Analizar gestos
                Gesture gesture;
                if (gestureAnalizer.analize(data, out gesture))
                {
                    switch (gesture.Type)
                    {
                        //Gesto de abrir cajon
                        case GestureType.OpenZ:
                            abrirCajon(gesture.Pos);
                            break;

                        //Gesto de cerrar cajon
                        case GestureType.CloseZ:
                            cerrarCajon(gesture.Pos);
                            break;
                    }
                }

                
            }


            foreach (TgcMesh m in _meshes)
            {
                m.render();
            }


            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Z))
            {
                foreach (CajonFocus cajon in cajones)
                {
                    cajon.open();
                }
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.X))
            {
                foreach (CajonFocus cajon in cajones)
                {
                    cajon.close();
                }
            }


            foreach (CajonFocus cajon in cajones)
            {
                cajon.update();
                cajon.render();
            }
            


            /*
            foreach (FocusSet c in _conjuntos)
            {
                //c.animate();
                //c.Render();
            }
            */


            



            
            center.render();
            bounds.render();
        }

        /// <summary>
        /// Buscar el cajon mas cerca de donde se produzco el gesto y abrirlo
        /// </summary>
        private void abrirCajon(Vector3 pos)
        {
            //Proyectar posicion del gesto a 2D
            Vector2 pos2D = TgcKinectUtils.projectPoint(pos);
            float minDist = float.MaxValue;
            CajonFocus cajonMasCerca = null;

            //Buscar el cajon que esté mas cerca
            foreach (CajonFocus c in cajones)
            {
                //Que este cerrado
                if (c.CurrentState == CajonFocus.State.Closed)
                {
                    //Ver distancia en 2D
                    float dist = Vector2.Length(pos2D - c.getScreenCenter());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        cajonMasCerca = c;
                    }
                }
            }

            //Ver si encontramos uno suficientemente cerca
            if (cajonMasCerca != null && minDist < CAJON_MIN_DIST_GESTO)
            {
                cajonMasCerca.open();
            }


        }

        private void cerrarCajon(Vector3 pos)
        {
            //Proyectar posicion del gesto a 2D
            Vector2 pos2D = TgcKinectUtils.projectPoint(pos);
            float minDist = float.MaxValue;
            CajonFocus cajonMasCerca = null;

            //Buscar el cajon que esté mas cerca
            foreach (CajonFocus c in cajones)
            {
                //Que este abierto
                if (c.CurrentState == CajonFocus.State.Opened)
                {
                    //Ver distancia en 2D
                    float dist = Vector2.Length(pos2D - c.getScreenCenter());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        cajonMasCerca = c;
                    }
                }
            }

            //Ver si encontramos uno suficientemente cerca
            if (cajonMasCerca != null && minDist < CAJON_MIN_DIST_GESTO)
            {
                cajonMasCerca.close();
            }
        }

        


        public override void close()
        {
            tgcKinect.dispose();
        }

    }
}

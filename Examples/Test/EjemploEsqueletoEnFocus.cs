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

namespace Examples.Test
{

    public class EjemploEsqueletoEnFocus : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBoundingBox bounds;
        TgcBox center;
        private List<TgcMesh> _meshes;
        private FocusSet[] _conjuntos;        

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


            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();


            GuiController.Instance.FpsCamera.Enable = true;

            center = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
            bounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));


            //Loader de focus
            FocusParser.TEXTURE_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            FocusParser.MESH_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            FocusParser loader = new FocusParser();
            string fileScene = GuiController.Instance.ExamplesMediaDir + "Focus\\escena1.dat";
            loader.FromFile(fileScene);
            _meshes = loader.Escene;
            _conjuntos = loader._focusSets;


            //Achicar y mover toda la escena
            //Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
            //Vector3 translate = new Vector3(0, 0, 0);
            //foreach (TgcMesh m in _meshes)
            //{
            //    m.Scale = scale;
            //    m.move(translate);
            //}
            //foreach (FocusSet c in _conjuntos)
            //{
            //    c.container.Scale = scale;
            //    c.container.move(translate);
            //    foreach (TgcMesh m in c.container.Childs)
            //    {
            //        m.Scale = scale;
            //        m.move(translate);
            //    }
            //}
        }


        bool primera = true;
        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            
            TgcKinectSkeletonData data = tgcKinect.update();

            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);

                if (primera)
                {
                    if (data.Current.KinectSkeleton.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked)
                    {
                        Vector3 pos = new Vector3(2000 - data.Current.KinectSkeleton.Joints[JointType.HipCenter].Position.X, 100, -2000 - data.Current.KinectSkeleton.Joints[JointType.HipCenter].Position.Z);
                         tgcKinect.PositionTranslate = pos;

                        primera = false;
                    }
                }

            }

            foreach (TgcMesh m in _meshes)
            {
                m.render();
            }
            foreach (FocusSet c in _conjuntos)
            {
                //c.animate();
                c.Render();
            }




            center.render();
            bounds.render();
        }

        




        public override void close()
        {
            tgcKinect.dispose();
        }

    }
}

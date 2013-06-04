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

namespace Examples.Test
{

    public class EjemploEsqueleto : TgcExample
    {

        TgcKinect tgcKinect;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Esqueleto";
        }

        public override string getDescription()
        {
            return "Ejemplo Esqueleto";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();


            GuiController.Instance.FpsCamera.Enable = true;


            GuiController.Instance.UserVars.addVar("tracking", "false");
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);
            }


            GuiController.Instance.UserVars["tracking"] = data.Active.ToString();

        }

        




        public override void close()
        {
            tgcKinect.dispose();
        }

    }
}

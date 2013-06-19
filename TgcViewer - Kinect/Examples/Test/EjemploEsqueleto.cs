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

    public class EjemploEsqueleto : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBoundingBox bounds;
        TgcBox center;
        TgcSprite mousePointer;

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


            center = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
            bounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));


            mousePointer = new TgcSprite();
            mousePointer.Texture = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "pointer.jpg");
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);


                Vector3 headPos = TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[JointType.Head].Position);
                Vector3 centerPos = TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[JointType.HipCenter].Position);
                float length = Vector3.Length(headPos - centerPos);
                BigLogger.log("Length", length);

                BigLogger.log("HipCenter", data.Current.CenterPos);
                BigLogger.log("RightHandPos", data.Current.RightHandPos);
                BigLogger.log("LefttHandPos", data.Current.LefttHandPos);
                BigLogger.renderLog();


                mousePointer.Position = data.Current.RightHandPos;
                GuiController.Instance.Drawer2D.beginDrawSprite();
                mousePointer.render();
                GuiController.Instance.Drawer2D.endDrawSprite();
            }


            GuiController.Instance.UserVars["tracking"] = data.Active.ToString();



            center.render();
            bounds.render();
        }

        




        public override void close()
        {
            tgcKinect.dispose();
        }

    }
}

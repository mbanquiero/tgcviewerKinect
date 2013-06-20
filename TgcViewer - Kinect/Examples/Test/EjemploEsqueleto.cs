using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using Examples.Kinect;
using Microsoft.Kinect;
using TgcViewer.Utils.TgcGeometry;
using Examples.Expo;

namespace Examples.Test
{

    public class EjemploEsqueleto : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBoundingBox bounds;
        TgcBox center;

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

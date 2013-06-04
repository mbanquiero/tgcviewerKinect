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

    public class EjemploCajon : TgcExample
    {

        TgcKinect tgcKinect;
        TgcBox mueble;
        List<Locker> cajones;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Cajon";
        }

        public override string getDescription()
        {
            return "Ejemplo Cajon";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();



            mueble = TgcBox.fromSize(new Vector3(200, 100, 20), Color.SandyBrown);
            mueble.Position = new Vector3(20, 20, 220);

            Vector3 muebleCenterPos = mueble.Position /*+ Vector3.Scale(mueble.Size, 0.5f)*/;
            cajones = new List<Locker>();
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(-30, 0, 2), new Vector3(50, 25, 20)));
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(0, 30, 2), new Vector3(50, 25, 20)));
            cajones.Add(crearCajon(muebleCenterPos + new Vector3(30, 0, 2), new Vector3(50, 25, 20)));



            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1.5467f, 54.7247f, 401.1074f), new Vector3(1.4672f, 54.4561f, 400.1474f));


            GuiController.Instance.UserVars.addVar("tracking", "false");
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);


                foreach (Locker l in cajones)
                {
                    l.update(data.Current.RightHandSphere);
                    //l.update(data.Current.LeftHandSphere);
                }
            }






            GuiController.Instance.UserVars["tracking"] = data.Active.ToString();


            foreach (Locker l in cajones)
            {
                l.HandleSphere.setRenderColor(l.Caught ? Color.Blue : Color.Yellow);
                l.HandleSphere.render();
                l.Mesh.render();
            }


            data.Current.RightHandSphere.render();
            data.Current.LeftHandSphere.render(); 
            mueble.render();
        }

        private Locker crearCajon(Vector3 pos, Vector3 size)
        {
            TgcBox cajonMesh = TgcBox.fromSize(size, Color.Green);
            cajonMesh.Position = pos;

            Locker locker = new Locker();
            locker.HandleSphere = new TgcBoundingSphere(cajonMesh.Position + new Vector3(0, 0, cajonMesh.Size.Z / 2), 10);
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

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
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace Examples.Test
{

    public class EjemploBasico : TgcExample
    {

        TgcKinect tgcKinect;
        TgcKinectSkeletalMesh mesh;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Basico";
        }

        public override string getDescription()
        {
            return "Ejemplo Basico";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            loader.MeshFactory = new TgcKinectSkeletalMesh.MeshFactory();
            mesh = (TgcKinectSkeletalMesh)loader.loadMeshFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml");



            tgcKinect = new TgcKinect();
            tgcKinect.init();
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                mesh.KinectSkeleton = data.Current.KinectSkeleton;
                mesh.animateAndRender();
            }

        }

        




        public override void close()
        {
            mesh.dispose();
        }

    }
}

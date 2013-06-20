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

    public class EjemploAvatar : TgcExample
    {

        TgcKinect tgcKinect;
        TgcKinectSkeletalMesh mesh;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Avatar";
        }

        public override string getDescription()
        {
            return "Ejemplo Avatar";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar mesh
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            loader.MeshFactory = new TgcKinectSkeletalMesh.MeshFactory();
            mesh = (TgcKinectSkeletalMesh)loader.loadMeshFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml");

            //Hacer mapping de huesos
            /*
            <bone id='0' name='Bip01' parentId='-1' pos='[0.212447,26.2833,1.14834]' rotQuat='[0.0,0.707106,0.0,0.707107]'/>
			<bone id='1' name='Bip01 Pelvis' parentId='0' pos='[0,0,0]' rotQuat='[0.483492,0.475215,0.52165,0.517965]'/>
			<bone id='2' name='Bip01 Spine' parentId='1' pos='[2.73985,-0.0318408,-0.0188154]' rotQuat='[0.0404528,0.00189839,-0.00599698,0.999162]'/>
			<bone id='3' name='Bip01 Spine1' parentId='2' pos='[6.19498,0,-0.00343716]' rotQuat='[0.0,0.0,0.0,1.0]'/>
			<bone id='4' name='Bip01 Neck' parentId='3' pos='[6.41617,0.0869015,-0.0035038]' rotQuat='[0.0,0.0,0.0,1.0]'/>
			<bone id='5' name='Bip01 Head' parentId='4' pos='[2.30035,0,0]' rotQuat='[0.0,0.000398831,0.0,1.0]'/>
			<bone id='6' name='Bip01 L Clavicle' parentId='4' pos='[-2.09962,0.747039,0.0035013]' rotQuat='[-0.707107,-0.707107,0.000282522,0.00028056]'/>
			<bone id='7' name='Bip01 L UpperArm' parentId='6' pos='[4.14776,0,0]' rotQuat='[-0.0074924,-0.063934,-0.0389218,0.997167]'/>
			<bone id='8' name='Bip01 L Forearm' parentId='7' pos='[9.01669,0,0]' rotQuat='[0.0,0.0946719,0.0,0.995508]'/>
			<bone id='9' name='Bip01 L Hand' parentId='8' pos='[8.40198,0,0]' rotQuat='[0.706825,0.0,0.0,0.707388]'/>
			<bone id='10' name='Bip01 R Clavicle' parentId='4' pos='[-2.09962,-0.920842,0.00350595]' rotQuat='[0.707107,-0.707107,-0.00028056,0.000282522]'/>
			<bone id='11' name='Bip01 R UpperArm' parentId='10' pos='[4.14776,0,0]' rotQuat='[0.00584129,-0.058849,0.0143832,0.998146]'/>
			<bone id='12' name='Bip01 R Forearm' parentId='11' pos='[8.50615,0,0]' rotQuat='[0.0,0.085631,0.0,0.996327]'/>
			<bone id='13' name='Bip01 R Hand' parentId='12' pos='[8.54264,0,0]' rotQuat='[-0.706825,0.0,0.0,0.707388]'/>
			<bone id='14' name='Bip01 L Thigh' parentId='1' pos='[0,3.89689,0]' rotQuat='[-0.0413224,0.0440018,-0.997924,-0.0224664]'/>
			<bone id='15' name='Bip01 L Calf' parentId='14' pos='[11.9134,0,0]' rotQuat='[0.0,0.0998335,0.0,0.995004]'/>
			<bone id='16' name='Bip01 L Foot' parentId='15' pos='[11.9134,0,0]' rotQuat='[0.00158055,-0.0615842,0.0182989,0.997933]'/>
			<bone id='17' name='Bip01 L Toe0' parentId='16' pos='[2.74009,0,3.52638]' rotQuat='[0.0,-0.707107,0.0,0.707107]'/>
			<bone id='18' name='Bip01 R Thigh' parentId='1' pos='[0,-3.89689,0]' rotQuat='[-0.0261322,0.0341116,-0.998212,0.04156]'/>
			<bone id='19' name='Bip01 R Calf' parentId='18' pos='[11.9134,0,0]' rotQuat='[0.0,0.0998335,0.0,0.995004]'/>
			<bone id='20' name='Bip01 R Foot' parentId='19' pos='[11.9134,0,0]' rotQuat='[-0.00187365,-0.0741152,-0.046938,0.996143]'/>
			<bone id='21' name='Bip01 R Toe0' parentId='20' pos='[2.74009,0,3.52638]' rotQuat='[0.0,-0.707107,0.0,0.707107]'/>
             */
            //http://www.codeproject.com/KB/dotnet/KinectGettingStarted/7.png
            List<TgcKinectSkeletalMesh.Mapping> mapping = new List<TgcKinectSkeletalMesh.Mapping>();
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01", JointType.HipCenter));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 Pelvis", JointType.HipCenter)); //????
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 Spine", JointType.Spine));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 Spine1", JointType.ShoulderCenter)); //????
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 Neck", JointType.ShoulderCenter));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 Head", JointType.Head));

            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Clavicle", JointType.ShoulderLeft));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L UpperArm", JointType.ElbowLeft));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Forearm", JointType.WristLeft));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Hand", JointType.HandLeft));

            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Clavicle", JointType.ShoulderRight));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R UpperArm", JointType.ElbowRight));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Forearm", JointType.WristRight));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Hand", JointType.HandRight));

            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Thigh", JointType.HipLeft));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Calf", JointType.KneeLeft));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Foot", JointType.AnkleLeft));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 L Toe0", JointType.FootLeft));

            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Thigh", JointType.HipRight));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Calf", JointType.KneeRight));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Foot", JointType.AnkleRight));
            mapping.Add(new TgcKinectSkeletalMesh.Mapping("Bip01 R Toe0", JointType.FootRight));

            mesh.setBonesMapping(mapping);


            //Iniciar kinect
            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();


            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1.5467f, 54.7247f, 401.1074f), new Vector3(1.4672f, 54.4561f, 400.1474f));

            GuiController.Instance.Modifiers.addBoolean("mesh", "mesh", true);
            GuiController.Instance.Modifiers.addBoolean("skeleton", "skeleton", true);
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Tomar tracking de kinect
            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                //Aplicar datos de kinect a mesh
                mesh.KinectSkeleton = data.Current.KinectSkeleton;
                bool renderMesh = (bool)GuiController.Instance.Modifiers["mesh"];
                if (renderMesh)
                {
                    mesh.animateAndRender();
                }
                

                //Render de esqueleto
                bool renderSkeleton = (bool)GuiController.Instance.Modifiers["skeleton"];
                if (renderSkeleton)
                {
                    tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);
                }
                
            }

        }

        




        public override void close()
        {
            mesh.dispose();
            tgcKinect.dispose();
        }

    }
}

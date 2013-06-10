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
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Test
{

    public class EjemploGesto : TgcExample
    {

        TgcKinect tgcKinect;
        TgcText2d text;
        bool gestoDetectado;
        float acumTime;


        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Gesto";
        }

        public override string getDescription()
        {
            return "Ejemplo Gesto";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            try
            {
                tgcKinect = new TgcKinect();
                tgcKinect.init();
                tgcKinect.DebugSkeleton.init();
            }
            catch (Exception)
            {
                GuiController.Instance.Logger.logError("No se detecto KINECT");
            }
            


            text = new TgcText2d();
            text.Position = new Point(30, 30);
            text.Color = Color.Red;
            text.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 36, FontStyle.Bold));
            text.Text = "Nada";
            text.Size = new Size(300, 100);

            gestoDetectado = false;
            acumTime = 0;

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(10.2881f, 1f, 9.6917f), new Vector3(10.2427f, 1.0175f, 10.6906f));
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);

                //Buscar gesto en mano derecha
                TgcKinectSkeletonData.AnalysisData rAnalysisData = data.HandsAnalysisData[TgcKinectSkeletonData.RIGHT_HAND];

                GuiController.Instance.Text3d.drawText("Diff AvgZ: " + rAnalysisData.Z.DiffAvg + ", AvgZ: " + rAnalysisData.Z.Avg + ", varX: " + rAnalysisData.X.Variance + "varY: " + rAnalysisData.Y.Variance, 50, 150, Color.Yellow);


                if (gestoDetectado)
                {
                    acumTime += elapsedTime;
                    if (acumTime > 3)
                    {
                        gestoDetectado = false;
                        text.Color = Color.Red;
                        text.Text = "Nada";
                    }
                }
                else
                {
                    if ((rAnalysisData.Z.Max - rAnalysisData.Z.Min) > 10f)
                    {
                        gestoDetectado = true;
                        acumTime = 0;
                        text.Color = Color.Green;
                        text.Text = "Abriendo cajon";
                    }

                    /*
                    //Gesto de abrir cajon
                    if (rAnalysisData.Z.DiffAvg > 1 && FastMath.Abs(rAnalysisData.X.Variance) < 0.5f && FastMath.Abs(rAnalysisData.Y.Variance) < 0.5f)
                    {
                        gestoDetectado = true;
                        acumTime = 0;
                        text.Color = Color.Green;
                        text.Text = "Abriendo cajon";
                    }*/
                }
            }



            text.render();

        }

        




        public override void close()
        {

        }

    }
}

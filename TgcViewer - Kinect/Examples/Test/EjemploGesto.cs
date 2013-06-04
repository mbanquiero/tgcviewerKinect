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
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);


                if (gestoDetectado)
                {
                    acumTime += elapsedTime;
                    if (acumTime > 5)
                    {
                        gestoDetectado = false;
                        text.Color = Color.Red;
                        text.Text = "Nada";
                    }
                }
                else
                {
                    //Buscar gesto en mano derecha
                    TgcKinectSkeletonData.AnalysisData rAnalysisData = data.HandsAnalysisData[TgcKinectSkeletonData.RIGHT_HAND];

                    //Gesto de abrir cajon
                    if (rAnalysisData.Z.DiffAvg > 0 && rAnalysisData.X.Variance < 1f && rAnalysisData.Y.Variance < 1f)
                    {
                        gestoDetectado = true;
                        acumTime = 0;
                        text.Color = Color.Green;
                        text.Text = "Abriendo cajon";
                    }
                }
            }



            text.render();

        }

        




        public override void close()
        {

        }

    }
}

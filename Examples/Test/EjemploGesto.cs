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
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Test
{

    public class EjemploGesto : TgcExample
    {

        TgcKinect tgcKinect;
        TgcText2d text;
        bool gestoDetectado;
        float acumTime;
        float showAcumTime;
        string estadisticas;
        string posicion;

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
            showAcumTime = 0;

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(10.2881f, 1f, 9.6917f), new Vector3(10.2427f, 1.0175f, 10.6906f));

            GuiController.Instance.Modifiers.addFloat("diff", -1f, -0.1f, -0.37f);
        }

        




        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            showAcumTime += elapsedTime;

            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);

                //Buscar gesto en mano derecha
                TgcKinectSkeletonData.AnalysisData rAnalysisData = data.HandsAnalysisData[TgcKinectSkeletonData.RIGHT_HAND];


                if (showAcumTime > 0.3f)
                {
                    showAcumTime = 0;
                    estadisticas = "Diff AvgZ: " + printFloat(rAnalysisData.Z.DiffAvg) + ", AvgZ: " + printFloat(rAnalysisData.Z.Avg) + ", varX: " + printFloat(rAnalysisData.X.Variance) + " varY: " + printFloat(rAnalysisData.Y.Variance);
                    posicion = "Pos rHand: " + TgcParserUtils.printVector3(data.Current.RightHandSphere.Center);
                }
                GuiController.Instance.Text3d.drawText(estadisticas, 50, 150, Color.Yellow);
                GuiController.Instance.Text3d.drawText(posicion, 50, 200, Color.Yellow);
                

                if (gestoDetectado)
                {
                    acumTime += elapsedTime;
                    if (acumTime > 1)
                    {
                        gestoDetectado = false;
                        text.Color = Color.Red;
                        text.Text = "Nada";
                    }
                }
                else
                {
                    /*
                    if ((rAnalysisData.Z.Max - rAnalysisData.Z.Min) > 10f)
                    {
                        gestoDetectado = true;
                        acumTime = 0;
                        text.Color = Color.Green;
                        text.Text = "Abriendo cajon";
                    }
                    */


                    float diff = (float)GuiController.Instance.Modifiers["diff"];
                    
                    //Gesto de abrir cajon
                    if (rAnalysisData.Z.DiffAvg < diff && FastMath.Abs(rAnalysisData.X.Variance) < 5f && FastMath.Abs(rAnalysisData.Y.Variance) < 10f)
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




        public string printFloat(float n)
        {
            if (n < 0.1f && n > -0.1f) return "0";
            return string.Format("{0:0.##}", n);
        }



        public override void close()
        {

        }

    }
}

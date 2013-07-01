using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examples.Kinect;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;

namespace Examples.Expo
{
    /// <summary>
    /// Analizador de gestos necesarios para la cocina
    /// </summary>
    public class GestureAnalizer
    {
        //Segundos que hay que esperar entre gesto y gesto para seguir analizando
        const float TIME_BEETWEEN_GESTURES = 1f;

        TgcBoundingBox sceneBounds;
        Vector3 sceneCenter;
        Vector3 sceneExtents;
        float lastGestureElapsedTime;

        public GestureAnalizer()
        {
            lastGestureElapsedTime = 0;
        }

        public void setSceneBounds(TgcBoundingBox sceneBounds)
        {
            this.sceneBounds = sceneBounds;
            this.sceneCenter = sceneBounds.calculateBoxCenter();
            this.sceneExtents = sceneBounds.calculateAxisRadius();
        }

        /// <summary>
        /// Buscar si alguna de las dos manos hizo un gesto reconocible
        /// </summary>
        /// <param name="data">Datos de tracking</param>
        /// <param name="gesture">Gesto reconocido. Solo es valido si devolvio true</param>
        /// <returns>True si se reconocio un gesto</returns>
        public bool analize(TgcKinectSkeletonData data, out Gesture gesture)
        {
            lastGestureElapsedTime += GuiController.Instance.ElapsedTime;
            if (lastGestureElapsedTime < TIME_BEETWEEN_GESTURES)
            {
                gesture = new Gesture();
                return false;
            }  

            //Ver mano derecha
            if(doAnalize(data.HandsAnalysisData[TgcKinectSkeletonData.RIGHT_HAND], out gesture))
            {
                lastGestureElapsedTime = 0;
                return true;
            }
            //Ver mano izquierda
            if (doAnalize(data.HandsAnalysisData[TgcKinectSkeletonData.LEFT_HAND], out gesture))
            {
                lastGestureElapsedTime = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Analizar gestos para una mano particular
        /// </summary>
        private bool doAnalize(TgcKinectSkeletonData.AnalysisData data, out Gesture gesture)
        {
            //Abrir cajon en Z
            if (data.Z.DiffAvg > 4f && data.X.Variance < 60f && data.Y.Variance < 60f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, data.Z.Max), GestureType.OpenZ);
                return true;
            }

            //Cerrar cajon en Z
            if (data.Z.DiffAvg < -4f && data.X.Variance < 60f && data.Y.Variance < 60f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, data.Z.Min), GestureType.CloseZ);
                return true;
            }


            /*
            //Abrir cajon en Z
            if (data.Z.DiffAvg < -0.037f && data.X.Variance < 0.5f && data.Y.Variance < 1f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.OpenZ);
                return true;
            }
            
            //Cerrar cajon en Z
            if (data.Z.DiffAvg > 0.037f && data.X.Variance < 0.5f && data.Y.Variance < 1f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.CloseZ);
                return true;
            }

            //Open left
            if (data.X.DiffAvg > 0.037f && data.Y.Variance < 0.05f && data.Z.Variance < 1f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.OpenLeft);
                return true;
            }

            //Open right
            if (data.X.DiffAvg < -0.037f && data.Y.Variance < 0.5f && data.Z.Variance < 1f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.OpenRight);
                return true;
            }

            //Press button
            if (data.X.Variance < 0.1f && data.Y.Variance < 0.1f && data.Z.Variance < 0.1f && data.Z.Avg <= sceneBounds.PMin.Z)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.PressButton);
                return true;
            }

            //Go Left
            if (data.X.Variance < 0.1f && data.Y.Variance < 0.1f && data.Z.Variance < 0.1f &&
                FastMath.Abs(data.Y.Avg - sceneCenter.Y) <= 1f && data.X.Avg <= sceneBounds.PMin.X)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.GoLeft);
                return true;
            }

            //Go Right
            if (data.X.Variance < 0.1f && data.Y.Variance < 0.1f && data.Z.Variance < 0.1f &&
                FastMath.Abs(data.Y.Avg - sceneCenter.Y) <= 1f && data.X.Avg <= sceneBounds.PMin.X)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.GoRight);
                return true;
            }
            */

            gesture = new Gesture();
            return false;
        }

    }
}

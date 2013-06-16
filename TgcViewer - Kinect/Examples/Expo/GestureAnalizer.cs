using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examples.Kinect;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Expo
{
    /// <summary>
    /// Analizador de gestos necesarios para la cocina
    /// </summary>
    public class GestureAnalizer
    {
        TgcBoundingBox sceneBounds;
        Vector3 sceneCenter;
        Vector3 sceneExtents;

        public GestureAnalizer()
        {
            
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
            //Ver mano derecha
            if(doAnalize(data.HandsAnalysisData[TgcKinectSkeletonData.RIGHT_HAND], out gesture))
            {
                return true;
            }
            //Ver mano izquierda
            if (doAnalize(data.HandsAnalysisData[TgcKinectSkeletonData.LEFT_HAND], out gesture))
            {
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
            if (data.Z.DiffAvg < -0.37f && data.X.Variance < 5f && data.Y.Variance < 10f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.OpenZ);
                return true;
            }

            //Cerrar cajon en Z
            if (data.Z.DiffAvg > 0.37f && data.X.Variance < 5f && data.Y.Variance < 10f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.CloseZ);
                return true;
            }

            //Open left
            if (data.X.DiffAvg > 0.37f && data.Y.Variance < 0.5f && data.Z.Variance < 10f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.OpenLeft);
                return true;
            }

            //Open right
            if (data.X.DiffAvg < -0.37f && data.Y.Variance < 5f && data.Z.Variance < 10f)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.OpenRight);
                return true;
            }

            //Press button
            if (data.X.Variance < 1f && data.Y.Variance < 1f && data.Z.Variance < 1f && data.Z.Avg <= sceneBounds.PMin.Z)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.PressButton);
                return true;
            }

            //Go Left
            if (data.X.Variance < 1f && data.Y.Variance < 1f && data.Z.Variance < 1f &&
                FastMath.Abs(data.Y.Avg - sceneCenter.Y) <= 10f && data.X.Avg <= sceneBounds.PMin.X)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.GoLeft);
                return true;
            }

            //Go Right
            if (data.X.Variance < 1f && data.Y.Variance < 1f && data.Z.Variance < 1f &&
                FastMath.Abs(data.Y.Avg - sceneCenter.Y) <= 10f && data.X.Avg <= sceneBounds.PMin.X)
            {
                gesture = new Gesture(new Vector3(data.X.Avg, data.Y.Avg, 0), GestureType.GoRight);
                return true;
            }


            gesture = new Gesture();
            return false;
        }

    }
}

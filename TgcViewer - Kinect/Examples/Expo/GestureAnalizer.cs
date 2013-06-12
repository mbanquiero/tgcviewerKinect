using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examples.Kinect;
using Microsoft.DirectX;

namespace Examples.Expo
{
    /// <summary>
    /// Analizador de gestos necesarios para la cocina
    /// </summary>
    public class GestureAnalizer
    {
        public GestureAnalizer()
        {

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


            gesture = new Gesture();
            return false;
        }

    }
}

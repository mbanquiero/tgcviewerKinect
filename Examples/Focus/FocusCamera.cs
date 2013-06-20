using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.Input
{
    public class FocusCamera : TgcCamera
    {
        static readonly Vector3 UP_VECTOR = new Vector3(0, 1, 0);

        public Vector3 LookFrom;
        public Vector3 LookAt;
        Matrix viewMatrix;

        bool enable;
        /// <summary>
        /// Habilita o no el uso de la camara
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;

                //Si se habilito la camara, cargar como la cámara actual
                if (value)
                {
                    GuiController.Instance.CurrentCamera = this;
                }
            }
        }


        public Vector3 getPosition()
        {
            return LookFrom;
        }

        public Vector3 getLookAt()
        {
            return LookAt;
        }

        public void updateCamera()
        {
            viewMatrix = Matrix.LookAtLH(LookFrom, LookAt, UP_VECTOR);
        }

        public void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            if (!enable)
            {
                return;
            }

            d3dDevice.Transform.View = viewMatrix;

        }

        public Vector3 getViewDir()
        {
            Vector3 viewDir = LookAt - LookFrom;
            viewDir.Normalize();
            return viewDir;
        }
    }
}

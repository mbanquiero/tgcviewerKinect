using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using Examples.Focus;

namespace Examples.Test
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploFocus : TgcExample
    {

        private List<TgcMesh> _meshes;
        private FocusSet [] _conjuntos;

        public override string getCategory()
        {
            return "Test";
        }

        public override string getName()
        {
            return "Ejemplo Focus Loader";
        }

        public override string getDescription()
        {
            return "Ejemplo Focus Loader";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(1140.182f, 1509.073f, -3860.18f), new Vector3(1140.724f, 1508.912f, -3859.356f));
            GuiController.Instance.FpsCamera.MovementSpeed *= 10;
            GuiController.Instance.FpsCamera.JumpSpeed *= 10;

            string fileScene = GuiController.Instance.ExamplesMediaDir + "\\Focus\\escena.dat";

            FocusParser.TEXTURE_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            FocusParser.MESH_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            var loader = new FocusParser();
            loader.FromFile(fileScene);
            _meshes = loader.Escene;
            _conjuntos = loader._focusSets;
        }


        bool renderizar = true;
        public override void render(float elapsedTime)
        {
            if (!renderizar)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            try
            {
                foreach (TgcMesh m in _meshes)
                {
                    m.render();
                }

                foreach (FocusSet f in _conjuntos)
                {
                    f.animate();
                    f.Render();
                }
            }
            catch (Exception e)
            {
                GuiController.Instance.Logger.log(e.StackTrace);
                renderizar = false;
            }


        }


        /// </summary>
        public override void close()
        {
            foreach (TgcMesh m in _meshes)
            {
                m.dispose();
            }
        }


        
    }
}

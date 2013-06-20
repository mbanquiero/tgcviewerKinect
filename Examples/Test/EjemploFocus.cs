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
using TgcViewer.Utils.TgcGeometry;
using Examples.Kinect;
using TgcViewer.Utils.Gui;
using TgcViewer.Utils.Input;

namespace Examples.Test
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploFocus : TgcExample
    {

        private List<TgcMesh> _meshes;
        private FocusSet [] _conjuntos;
        TgcBoundingBox bounds;
        TgcBox center;
        TgcKinect tgcKinect;

        // gui
        DXGui gui = new DXGui();
        FocusCamera camera = new FocusCamera();



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


            center = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
            bounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));


            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();


            // levanto el GUI
            gui.Create();

            // Inicio un dialogo modalless
            gui.InitDialog(true);
            int x0 = 10;
            int y0 = 10;
            int dy = 25;
            int dx = 200;
            gui.InsertMenuItem(100, "Abrir Proyecto", x0, y0, dx, dy);
            gui.InsertMenuItem(101, "Grabar Proyecto", x0, y0 += 40, dx, dy);
            gui.InsertMenuItem(102, "Modo Navegacion", x0, y0 += 40, dx, dy);
            gui.InsertMenuItem(103, "Modificar Texturas", x0, y0 += 40, dx, dy);
            gui.InsertMenuItem(104, "Cambiar Empujadores", x0, y0 += 40, dx, dy);
            gui.InsertMenuItem(105, "Salir", x0, y0 += 40, dx, dy);

            // Camara para 3d support
            gui.camera = camera;


        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {
                tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);
            }


            foreach (TgcMesh m in _meshes)
            {
                m.render();
            }

            foreach (FocusSet f in _conjuntos)
            {
                f.animate();
                f.Render();
            }



            center.render();
            bounds.render();


            // ------------------------------------------------
            GuiMessage msg = gui.Update(elapsedTime);
            Vector3 ViewDir = camera.LookAt - camera.LookFrom;
            // proceso el msg
            switch (msg.message)
            {
                case MessageType.WM_COMMAND:
                    switch (msg.id)
                    {
                        case 0:
                        case 1:
                            // Resultados OK, y CANCEL del ultimo messagebox
                            gui.EndDialog();
                            break;

                        case 100:
                            // Abrir Proyecto
                            OpenFileDlg();
                            break;

                        case 101:
                            // Grabar Proyecto
                            gui.MessageBox("Proyecto Grabado", "Focus Kinect Interaction");
                            break;

                        case 102:
                            // Modo navegacion
                            gui.MessageBox("Modo navegación Activado", "Focus Kinect Interaction");
                            break;

                        case 103:
                            // Cambiar Textura
                            TexturaDlg();
                            break;

                        case 104:
                            // Cambiar Mesh
                            MeshDlg();
                            break;

                        case 200:
                            // Play, alejo el punto de vista
                            camera.LookFrom = camera.LookFrom - ViewDir * 0.1f;
                            camera.updateCamera();
                            break;
                        case 201:
                            // Play, alejo el punto de vista
                            camera.LookFrom = camera.LookFrom + ViewDir * 0.1f;
                            camera.updateCamera();
                            break;
                        default:
                            if (msg.id >= 1000)
                            {
                                // Selecciono una textura
                                System.Windows.Forms.MessageBox.Show("Textura Nro:" + (msg.id - 1000 + 1), "Textura seleccionada",
                                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                                // Termino el dialogo
                                gui.EndDialog();
                            }
                            break;
                    }
                    break;
                default:
                    break;
            }
            camera.Enable = true;
            gui.Render();
            GuiController.Instance.FpsCamera.Enable = true;


        }

        public void OpenFileDlg()
        {
            gui.InitDialog(false, false);

            int x0 = -20;
            int y0 = 10;
            int dy = 400;
            int dx = 1000;
            int tdx = 200;
            int tdy = 150;

            gui.InsertFrame("Seleccione el Proyecto", x0, y0, dx, dy, Color.FromArgb(60, 120, 60));
            x0 += 50;
            y0 += 80;

            List<string> lista = new List<string>();
            lista.Add("escenas\\escena1.png");
            lista.Add("escenas\\escena2.png");
            lista.Add("escenas\\escena3.png");
            lista.Add("escenas\\escena4.png");

            int cant_texturas = lista.Count;
            for (int t = 0; t < cant_texturas; ++t)
            {
                String s = "" + (t + 1);
                gui.InsertKinectTileButton(1000 + t, s, lista[t], x0 + t * (tdx + 20), y0, tdx, tdy);
            }
            gui.InsertKinectScrollButton(0, "scroll_left.png", x0, y0 + dy - tdy - 40, dx / 2 - 40, 80);
            gui.InsertKinectScrollButton(1, "scroll_right.png", x0 + dx / 2 + 20, y0 + dy - tdy - 40, dx / 2 - 40, 80);


        }

        public void TexturaDlg()
        {
            // Inicio un dialogo modalless
            gui.InitDialog(false, false);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            int x0 = -20;
            int y0 = 10;
            int dy = 400;
            int dx = W + 40;
            int tdx = 200;
            int tdy = 150;

            gui.InsertFrame("Seleccione la textura", x0, y0, dx, dy, Color.FromArgb(192, 192, 192));

            List<string> lista = new List<string>();
            lista.Add("maderas\\09-guindo.jpg");
            lista.Add("masisa\\acacia.jpg");
            lista.Add("masisa\\roble mi.jpg");
            lista.Add("metales\\cromado.jpg");
            lista.Add("metales\\Aco.jpg");
            lista.Add("colores\\dorado.jpg");

            int cant_texturas = lista.Count;
            for (int t = 0; t < cant_texturas; ++t)
            {
                String s = "" + (t + 1);
                gui.InsertKinectTileButton(1000 + t, s, lista[t], x0 + 50 + t * (tdx + 20), y0 + 50, tdx, tdy);
            }

            gui.InsertKinectScrollButton(0, "scroll_left.png", x0 + 40, y0 + dy - 100, (dx - 40) / 2 - 40, 80);
            gui.InsertKinectScrollButton(1, "scroll_right.png", x0 + dx / 2 + 20, y0 + dy - 100, (dx - 49) / 2 - 40, 80);

        }


        public void MeshDlg()
        {
            gui.InitDialog(false, false);

            int x0 = -20;
            int y0 = 10;
            int dy = 400;
            int dx = 1000;
            int tdx = 200;
            int tdy = 150;

            gui.InsertFrame("Seleccione el empujador", x0, y0, dx, dy, Color.Honeydew);
            x0 += 50;
            y0 += 80;

            List<string> lista = new List<string>();
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\10089945.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\10090267.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\bachas\\msh\\405 E.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\adornos\\msh\\adorno13.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\griferia\\msh\\griferia7.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\microondas\\msh\\microondas7.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\10090267.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\bachas\\msh\\405 E.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\adornos\\msh\\adorno13.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\griferia\\msh\\griferia7.y");

            int cant_texturas = lista.Count;
            int t = 0;
            for (int i = 0; i < 4 && t < cant_texturas; ++i)
                for (int j = 0; j < 2 && t < cant_texturas; ++j)
                {
                    String s = "" + (t + 1);
                    gui.InsertMeshButton(1000 + t, s, lista[t], x0 + i * (tdx + 20), y0 + j * (tdy + 20), tdx, tdy);
                    ++t;
                }

        }


        /// </summary>
        public override void close()
        {
            foreach (TgcMesh m in _meshes)
            {
                m.dispose();
            }
            gui.Dispose();

        }
        
    }
}

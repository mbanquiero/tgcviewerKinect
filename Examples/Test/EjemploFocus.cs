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
using Examples.Expo;

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
        TexturasFocus texturasFocus;

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

            //Path de Focus
            FocusParser.TEXTURE_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";
            FocusParser.MESH_FOLDER = GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\";


            center = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
            bounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));

            //Iniciar kinect
            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();


            // levanto el GUI
            gui.Create();

            //Configurar todas las texturas que se pueden elegir para cambiar
            texturasFocus = new TexturasFocus();


            // Inicio un dialogo modalless
            gui.InitDialog(true);
            int x0 = 10;
            int y0 = 10;
            int dy = 35;
            int dx = 300;
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

            if (_meshes != null)
            {
                // Hay escena
                foreach (TgcMesh m in _meshes)
                {
                    m.render();
                }


                /*
                 * foreach (FocusSet f in _conjuntos)
                {
                    f.animate();
                    f.Render();
                }
                */

            }
            else
            {
                // Solo hay gui, dibujo un fondo de presentacion
                gui.DrawImage(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\fondo.png", 0, 0,
                    GuiController.Instance.Panel3d.Width, GuiController.Instance.Panel3d.Height);

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
                            texturasFocus.TextureGroupDlg(gui);
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

                            //Cambiar de escena
                            if (msg.id >= 3000)
                            {
                                disposeScene();

                                // Selecciono una escena Cargo la escena y Termino el dialogo
                                var loader = new FocusParser();
                                int nro_escena = msg.id - 3000 + 1;
                                string fileScene = GuiController.Instance.ExamplesMediaDir + "Focus\\escena" + nro_escena + ".dat";
                                loader.FromFile(fileScene);
                                _meshes = loader.Escene;
                                _conjuntos = loader._focusSets;
                                gui.EndDialog();
                            }
                            
                            //Cambiar de textura
                            else if (msg.id >= 2000)
                            {
                                texturasFocus.applyTextureChange(_meshes, _conjuntos, msg.id);
                                gui.EndDialog();
                            }

                            //Seleccionar categoria de textura
                            else if (msg.id >= 1000)
                            {
                                gui.EndDialog();
                                texturasFocus.TextureDlg(gui, msg.id);
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
                gui.InsertKinectTileButton(3000 + t, s, lista[t], x0 + t * (tdx + 20), y0, tdx, tdy);
            }
            gui.InsertKinectScrollButton(0, "scroll_left.png", x0, y0 + dy - tdy - 40, dx / 2 - 40, 80);
            gui.InsertKinectScrollButton(1, "scroll_right.png", x0 + dx / 2 + 20, y0 + dy - tdy - 40, dx / 2 - 40, 80);


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

        /// <summary>
        /// Limpiar toda la escena
        /// </summary>
        private void disposeScene()
        {
            if (_meshes != null)
            {
                foreach (TgcMesh m in _meshes)
                {
                    m.dispose();
                }
                foreach (FocusSet c in _conjuntos)
                {
                    c.dispose();
                }
                _meshes = null;
                _conjuntos = null;
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

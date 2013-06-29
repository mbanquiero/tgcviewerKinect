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
using Microsoft.Kinect;

namespace Examples.Test
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploFocus : TgcExample
    {

        // Defines
        public const int IDOK = 0;
        public const int IDCANCEL = 1;
        public const int ID_FILE_OPEN = 100;
        public const int ID_FILE_SAVE = 101;
        public const int ID_MODO_NAVEGACION = 102;
        public const int ID_CAMBIAR_TEXTURAS = 103;
        public const int ID_CAMBIAR_EMPUJADORES = 104;
        public const int ID_APP_EXIT = 105;
        public const int ID_CAMBIAR_MATERIALES = 106;
        public const int ID_PROGRESS1 = 107;
        public const int ID_RESET_CAMARA = 108;

        private List<TgcMesh> _meshes;
        private FocusSet [] _conjuntos;
        TgcBoundingBox bounds;
        TgcKinect tgcKinect;
        TexturasFocus texturasFocus;
        TgcBox lightMesh;

        Vector3 ant_LA, ant_LF;


        // gui
        DXGui gui = new DXGui();
        //FocusCamera camera = new FocusCamera();
        public bool blocked = false;
        public gui_skeleton esqueleto2d = null;


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


            bounds = new TgcBoundingBox(new Vector3(-10, 0, -10), new Vector3(10, 20, 10));

            //Crear caja para indicar ubicacion de la luz
            lightMesh = TgcBox.fromSize(new Vector3(300, 100, 300), Color.Yellow);


            //Iniciar kinect
            tgcKinect = new TgcKinect();
            tgcKinect.init();
            tgcKinect.DebugSkeleton.init();
            gui.kinect.hay_sensor = !tgcKinect.sin_sensor;


            // levanto el GUI
            gui.Create();

            //Configurar todas las texturas que se pueden elegir para cambiar
            texturasFocus = new TexturasFocus();
            gui.InitDialog(true);
            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;
            int x0 = 70;
            int y0 = 10;
            int dy = 120;
            int dy2 = dy;
            int dx = 400;
            gui.InsertMenuItem(ID_FILE_OPEN, "Abrir Proyecto", "open.png", x0, y0, dx, dy);
            gui.InsertMenuItem(ID_MODO_NAVEGACION, "Modo Navegacion", "navegar.png", x0, y0 += dy2, dx, dy);
//            gui.InsertMenuItem(ID_CAMBIAR_MATERIALES, "Modificar Materiales", "editmat.png", x0, y0 += dy2, dx, dy);
            gui.InsertMenuItem(ID_CAMBIAR_TEXTURAS, "Modificar Texturas","edit_tex.png", x0, y0 += dy2, dx, dy);
            gui.InsertMenuItem(ID_CAMBIAR_EMPUJADORES, "Modificar Manijas", "manijas.png",x0, y0 += dy2, dx, dy);
            gui.InsertMenuItem(ID_APP_EXIT, "Salir", "salir.png", x0, y0 += dy2, dx, dy);

            int sdx = 200;
            int sdy = 300;
            esqueleto2d = gui.InsertKinectSkeletonControl(W - sdx, H - sdy, sdx- 4, sdy-4);
            esqueleto2d.pir_min_x = tgcKinect.right_pir.x_min;
            esqueleto2d.pir_min_y = tgcKinect.right_pir.y_min;
            esqueleto2d.pir_max_x = tgcKinect.right_pir.x_max;
            esqueleto2d.pir_max_y = tgcKinect.right_pir.y_max;

            // Camara para 3d support
            gui.camera = GuiController.Instance.FpsCamera;
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string status_text = "Untracked";
            TgcKinectSkeletonData data = tgcKinect.update();
            if (data.Active)
            {

                if (data.Current.KinectSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                {
                    tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);
                    gui.kinect.kinectData = data;
                    /*
                    Vector3 handPos = TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[Microsoft.Kinect.JointType.HandRight].Position);
                    Vector3 hipPos = TgcKinectUtils.toVector3(data.Current.KinectSkeleton.Joints[Microsoft.Kinect.JointType.HipCenter].Position);
                    Vector3 diff = handPos - hipPos;
                    BigLogger.log("diff", diff.Z);
                     */

                    status_text = "Hand Tracked:" + tgcKinect.raw_pos_mano.ToString();
                    if(tgcKinect.skeleton_sel!=-1)
                        esqueleto2d.SkeletonUpdate(tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel]);
                } 
            }


            if (_meshes != null)
            {
                // Hay escena
                Effect currentShader = GuiController.Instance.Shaders.TgcMeshPhongShader;
                d3dDevice.SetRenderState(RenderStates.MultisampleAntiAlias, true);

                foreach (TgcMesh m in _meshes)
                {
                    //Aplicar al mesh el shader actual
                    m.Effect = currentShader;
                    //El Technique depende del tipo RenderType del mesh
                    m.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(m.RenderType);

                    //Cargar variables shader
                    m.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightMesh.Position));
                    m.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
                    m.Effect.SetValue("ambientColor", ColorValue.FromColor(Color.FromArgb(64, 64, 64)));

                    // Coef. de luz diffuse (para todos los layers)
                    int kd = (int)(0.7f * 255.0f);
                    m.Effect.SetValue("diffuseColor", ColorValue.FromColor(Color.FromArgb(kd,kd,kd)));

                    // Coef. de luz specular (para todos los layers)
                    int ks = (int)(0.3f * 255.0f);
                    m.Effect.SetValue("specularColor", ColorValue.FromColor(Color.FromArgb(ks, ks, ks)));
                    m.Effect.SetValue("specularExp", 5.0f);

                    m.render();
                }
            }
            else
            {
                // Solo hay gui, dibujo un fondo de presentacion
                gui.DrawImage(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\fondo.png", 0, 0,
                    GuiController.Instance.Panel3d.Width, GuiController.Instance.Panel3d.Height);

            }
            // bounding box de la escna
            bounds.render();
            //Renderizar mesh de luz
            lightMesh.render();


            // ------------------------------------------------
            GuiMessage msg = gui.Update(elapsedTime);
            // proceso el msg
            switch (msg.message)
            {
                case MessageType.WM_COMMAND:
                    switch (msg.id)
                    {
                        case IDOK:
                        case IDCANCEL:
                            // Resultados OK, y CANCEL del ultimo messagebox
                            gui.EndDialog();
                            break;

                        case ID_FILE_OPEN:
                            // Abrir Proyecto
                            OpenFileDlg();
                            break;

                        case ID_FILE_SAVE:
                            // Grabar Proyecto
                            gui.MessageBox("Proyecto Grabado", "Focus Kinect Interaction");
                            break;

                        case ID_MODO_NAVEGACION:
                            // Paso a modo navegacion
                            ModoNavegacion();
                            break;

                        case ID_APP_EXIT:
                            // Salir
                            gui.MessageBox("Desea Salir del Sistema?", "Focus Kinect Interaction");
                            break;

                        case ID_CAMBIAR_TEXTURAS:
                            // Cambiar Texturas
                            texturasFocus.TextureGroupDlg(gui);
                            break;

                        /*
                         * deprecado
                        case ID_CAMBIAR_MATERIALES:
                            // Cambiar Materiales
                            MaterialesDlg();
                            break;
                        case 400:
                            // Cambiar material abiertos
                            MaterialesGabDlg(true);
                            break;
                        case 401:
                            // Cambiar material cerrados
                            MaterialesGabDlg();
                            break;

                        case 402:
                            // Cambiar material de Puertas
                            // No implementado en verdad
                            texturasFocus.TextureDlg(gui, 1000);        // 1000 = maderas
                            break;

                        case 403:
                            // Cambiar material de Cajones
                            // No implementado en verdad
                            texturasFocus.TextureDlg(gui, 1000);        // 1000 = maderas
                            break;
                        case 500:
                        case 501:
                        case 502:
                        case 503:
                            // Cambia textura de fondo gabinetes abiertos
                            // No implementado en verdad
                            texturasFocus.TextureDlg(gui, 1000);        // 1000 = maderas
                            break;
                         */

                        case 406:
                        case ID_CAMBIAR_EMPUJADORES:
                            // Cambiar empujador de cajon
                            // No implementado en verdad
                            EmpujadorDlg();
                            break;

                        case ID_RESET_CAMARA:
                            GuiController.Instance.FpsCamera.setCamera(ant_LF, ant_LA);
                            break;

                        default:
                            if (msg.id >= 4000)
                            {
                                //Cambiar de Empujador
                                // Termino el dialogo                                                                
                                gui.EndDialog();
                            }
                            else
                            if (msg.id >= 3000)
                            {
                                //Cambiar de escena
                                // Termino el dialogo                                                                
                                gui.EndDialog();
                                   
                                // libero la escena anterior
                                disposeScene();
                                // Cargo la escena 
                                blocked = true;
                                ProgressBarDlg();
                                FocusParser loader = new FocusParser();
                                loader.progress_bar = (gui_progress_bar)gui.GetDlgItem(ID_PROGRESS1);
                                int nro_escena = msg.id - 3000 + 1;
                                string fileScene = GuiController.Instance.ExamplesMediaDir + "Focus\\escena" + nro_escena + ".dat";
                                loader.FromFile(fileScene);
                                _meshes = loader.Escene;
                                _conjuntos = loader._focusSets;
                                gui.EndDialog();            // progress bar dialog

                                // Habilito los items de menu 
                                gui.EnableItem(ID_FILE_SAVE);
                                gui.EnableItem(ID_CAMBIAR_TEXTURAS); 
                                gui.EnableItem(ID_CAMBIAR_MATERIALES);


                                // Bounding box de la escena
                                // Calculo el bounding box de la escena
                                float x0 = 10000;
                                float y0 = 10000;
                                float z0 = 10000;
                                float x1 = -10000;
                                float y1 = -10000;
                                float z1 = -10000;
                                foreach (TgcMesh m in _meshes)
                                {
                                    TgcBoundingBox box = m.BoundingBox;
                                    if (box.PMin.X < x0)
                                        x0 = box.PMin.X;
                                    if (box.PMin.Y < y0)
                                        y0 = box.PMin.Y;
                                    if (box.PMin.Z < z0)
                                        z0 = box.PMin.Z;

                                    if (box.PMax.X > x1)
                                        x1 = box.PMax.X;
                                    if (box.PMax.Y > y1)
                                        y1 = box.PMax.Y;
                                    if (box.PMax.Z > z1)
                                        z1 = box.PMax.Z;
                                }

                                bounds = new TgcBoundingBox(new Vector3(x0, y0, z0), new Vector3(x1, y1, z1));
                                // pongo una luz en el medio de la cocina, y a la altura del techo
                                lightMesh.Position = new Vector3((x0 + x1) / 2, y1-200, (z0 + z1) / 2);

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

            gui.Render();
            //gui.TextOut(50, 50,status_text);
            //BigLogger.renderLog();

        }

        public void OpenFileDlg()
        {
            gui.InitDialog(false, false);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;


            int x0 = -20;
            int y0 = 40;
            int dy = 600;
            int dx = W + 40;

            gui.InsertFrame("Seleccione el Proyecto", x0, y0, dx, dy, Color.FromArgb(60, 120, 60),frameBorder.sin_borde);
            int sdx = 500;
            int sdy = 120;
            gui.InsertKinectScrollButton(0, "scroll_left.png", x0+40, y0 + dy - sdy - 50, sdx, sdy);
            gui.InsertKinectScrollButton(1, "scroll_right.png", x0+40 + sdx + 20, y0 + dy - sdy - 50, sdx, sdy);

            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
                    y0 + dy - gui.KINECT_BUTTON_SIZE_X - 50, gui.KINECT_BUTTON_SIZE_X);
            cancel_btn.scrolleable = false;      // fijo el boton de cancelar


            x0 += 40;
            y0 += 140;
            int tdx = 280;
            int tdy = 200;

            List<string> lista = new List<string>();
            lista.Add("escenas\\escena1.png");
            lista.Add("escenas\\escena2.png");
            lista.Add("escenas\\escena3.png");
            lista.Add("escenas\\escena4.png");

            int cant_texturas = lista.Count;
            for (int t = 0; t < cant_texturas; ++t)
            {
                String s = "" + (t + 1);
                gui.InsertKinectTileButton(3000 + t, s, lista[t], x0 + t * (tdx + 40), y0, tdx, tdy);
            }


        }

        /*
         * deprecado
        public void MaterialesDlg()
        {
            gui.InitDialog(false, false);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            int x0 = 10;
            int y0 = 10;
            int dy = H - 20;
            int dx = W - 20;
            int r = 200;
            int r2 = 300;

            gui.InsertFrame("Materiales utilizados en el Proyecto", x0, y0, dx, dy, Color.FromArgb(60, 120, 60),
                    frameBorder.redondeado);

            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
                    y0 + dy - gui.KINECT_BUTTON_SIZE_X - 50, gui.KINECT_BUTTON_SIZE_X);
            cancel_btn.scrolleable = false;      // fijo el boton de cancelar

            y0 += 100;
            x0 = 80;

            gui.InsertKinectCircleButton(400, "Abiertos", "abiertos.png", x0, y0, r);
            gui.InsertKinectCircleButton(401, "Cerrados", "cerrados.png", x0 += r2, y0, r);
            gui.InsertKinectCircleButton(402, "Puertas", "puertas.png", x0 += r2, y0, r);
            gui.InsertKinectCircleButton(403, "Cajones", "cajones.png", x0 += r2, y0, r);
            y0 += r + 120;
            x0 = 80;
            gui.InsertKinectCircleButton(404, "Zocalo", "zocalo.png", x0, y0, r);
            gui.InsertKinectCircleButton(405, "Patas", "patas.png", x0 += r2, y0, r);
            gui.InsertKinectCircleButton(406, "Manijas", "manijas.png", x0 += r2, y0, r);

        }
         

        public void MaterialesGabDlg(bool abiertos=false)
        {
            gui.InitDialog(false, false,true);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            int x0 = 20;
            int y0 = 20;
            int dy = 600;
            int dx = W - 20;
            int r = 300;
            int r2 = 350;

            gui.InsertFrame("Materiales Gabinetes " + (abiertos ? "abiertos" : "cerrados"),
                    x0, y0, dx, dy, Color.FromArgb(192, 192, 192));
            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
                    y0 + dy - gui.KINECT_BUTTON_SIZE_X - 50, gui.KINECT_BUTTON_SIZE_X);
            cancel_btn.scrolleable = false;      // fijo el boton de cancelar

            x0 = 80;
            y0 += 80;

            gui.InsertKinectTileButton(500, "Standard", "Maderas\\Blanco.jpg", x0, y0, r,r);
            gui.InsertKinectTileButton(501, "Fondo", "Maderas\\09-guindo.jpg", x0 += r2, y0, r, r);
            gui.InsertKinectTileButton(502, "Canto", "Maderas\\09-guindo.jpg", x0 += r2, y0, r, r);


        }
        */


        public void EmpujadorDlg()
        {
            gui.InitDialog(false, false);
            //gui.hoover_enabled = false;             // deshabilito el hoover para esta pantalla
            
            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            int x0 = -20;
            int y0 = 40;
            int dy = 600;
            int dx = W + 50;
            int tdx = 260;
            int tdy = 200;

            gui.InsertFrame("Seleccione el empujador", x0, y0, dx, dy, Color.FromArgb(240, 240, 240),frameBorder.sin_borde);
            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
                    y0 + dy - gui.KINECT_BUTTON_SIZE_X - 50, gui.KINECT_BUTTON_SIZE_X);
            cancel_btn.scrolleable = false;      // fijo el boton de cancelar
            x0 += 50;
            y0 += 80;

            List<string> lista = new List<string>();
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\10089945.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\10090267.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\16053.y");
            lista.Add(GuiController.Instance.ExamplesMediaDir + "Focus\\texturas\\dxf\\manija modulos\\msh\\117_97.y");

            int cant_texturas = lista.Count;
            int t = 0;
            for (int j = 0; j < 2 && t < cant_texturas; ++j)
                for (int i = 0; i < 4 && t < cant_texturas; ++i)
                {
                    String s = "" + (t + 1);
                    gui.InsertMeshButton(4000 + t, s, lista[t], x0 + i * (tdx + 20), y0 + j * (tdy + 20), tdx, tdy);
                    ++t;
                }
            
        }


        public void ModoNavegacion()
        {
            ant_LA = GuiController.Instance.FpsCamera.getLookAt();
            ant_LF = GuiController.Instance.FpsCamera.getPosition();

            gui.InitDialog(false, false);
            int dx = 250;
            int W = (int)(GuiController.Instance.Panel3d.Width / gui.ex);
            int H = (int)(GuiController.Instance.Panel3d.Height / gui.ey);
            gui.InsertNavigationControl(_meshes,W-dx-5,5,dx,dx);
            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png",
                W / 2, H - gui.KINECT_BUTTON_SIZE_X - 40, gui.KINECT_BUTTON_SIZE_X);
            gui.InsertKinectCircleButton(ID_RESET_CAMARA, "Reset", "cancel.png",
                W - gui.KINECT_BUTTON_SIZE_X-40, H - gui.KINECT_BUTTON_SIZE_X - 40, gui.KINECT_BUTTON_SIZE_X);

        }


        public void ProgressBarDlg()
        {
            gui.InitDialog(false, false);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;
            int x0 = -20;
            int y0 = 100;
            int dy = 350;
            int dx = W + 50;

            gui_item frame = gui.InsertFrame("Cargando escena", x0, y0, dx, dy, Color.FromArgb(240, 240, 240),frameBorder.sin_borde);
            frame.c_font = Color.FromArgb(0, 0, 0);
            gui_progress_bar progress_bar = gui.InsertProgressBar(ID_PROGRESS1, 50, y0+150, W - 100, 60);
            progress_bar.SetPos(1);
            
            //gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
              //      H - gui.KINECT_BUTTON_SIZE_X - 40, gui.KINECT_BUTTON_SIZE_X);

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

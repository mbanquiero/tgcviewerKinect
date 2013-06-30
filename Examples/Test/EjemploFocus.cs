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
    public struct st_joint
    {
        public Vector3 Position;
        public JointType JointType;
        public float radio;
        public TgcDXMesh p_mesh;
    }

    public struct st_bone
    {
        public JointType StartJoint;
        public JointType EndJoint;
        public float radio;
    }

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
        TgcPickingRay ray = new TgcPickingRay();
        bool modo_picking = true;                   // modo picking o modo navergar

        Vector3 ant_LA, ant_LF;

        // Esqueleto 3d
        private TgcDXMesh elipsoid;
        private TgcDXMesh bola;
        private TgcDXMesh culo;
        private TgcDXMesh torso;
        private TgcDXMesh cabeza;
        Vector3 hip0 = new Vector3(float.MaxValue, 0, 0);       // posicion inicial del esqueleto
        Vector3 center = new Vector3();                         // centro de la escena
        bool hay_escena = false;

        // gui
        DXGui gui = new DXGui();
        //FocusCamera camera = new FocusCamera();
        public bool blocked = false;
        public gui_skeleton esqueleto2d = null;
        // Copia privada del esqueleto actual
        st_bone []_bones = new st_bone[26];
        public int _cant_bones = 0;
        st_joint []_joints = new st_joint [26];
        public int _cant_joints = 0;



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

            // Mallas .X para el esqueleto 3d
            elipsoid = new TgcDXMesh();
            elipsoid.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\pieza.x");
            bola = new TgcDXMesh();
            bola.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\ball.x");
            culo = new TgcDXMesh();
            culo.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\culo.x");
            torso = new TgcDXMesh();
            torso.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\torso.x");
            cabeza = new TgcDXMesh();
            cabeza.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\cabeza.x");
            InitSkeletonData();

        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string status_text = "Untracked";
            TgcKinectSkeletonData data = tgcKinect.update();

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


                if (modo_picking)
                {
                    //Actualizar Ray de colisión en base a posición de la mano
                    ray.updateRay(gui.kinect.right_hand.position.X,gui.kinect.right_hand.position.Y);

                    //Testear Ray contra el AABB de todos los meshes
                    foreach (FocusSet f in _conjuntos)
                    {
                        TgcBoundingBox aabb = f.container.BoundingBox;

                        //Ejecutar test, si devuelve true se carga el punto de colision collisionPoint
                        Vector3 collisionPoint = new Vector3();
                        bool selected = TgcCollisionUtils.intersectRayAABB(ray.Ray, aabb, out collisionPoint);
                        if (selected)
                        {
                            f.animate();
                            //break;
                        }
                    }
                }

                foreach (FocusSet f in _conjuntos)
                {
                    f.Render();
                    //if(modo_picking)
                      //  f.container.BoundingBox.render();
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
            // Renderizar el esqueleto 3d
            UpdateSkeleton();
            renderSkeletonMesh();


            if (data.Active)
            {
                if (data.Current.KinectSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                {
                    // Bindeo datos de la kinect al gui
                    //tgcKinect.DebugSkeleton.render(data.Current.KinectSkeleton);
                    gui.kinect.kinectData = data;

                    // Debug, Esqueleto en 2D 
                    status_text = "Hand Tracked:" + tgcKinect.raw_pos_mano.ToString();
                    if (tgcKinect.skeleton_sel != -1)
                        esqueleto2d.SkeletonUpdate(tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel]);
                }

            }




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
                            // si cancelo el modo navegacion, paso a modo picking
                            modo_picking = true;
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
                                // El centro de la escena (sobre el nivel del piso + la altura del personaje / 2)
                                center = new Vector3((x0 + x1) / 2, 950, (z0 + z1) / 2);
                                hay_escena = true;

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
            modo_picking = false;
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


        public void InitSkeletonData()
        {
            // Inicializa la primera vez los valores hardcodeados del esqueleto 
            _joints[(int)JointType.AnkleLeft].radio = _joints[(int)JointType.AnkleRight].radio  = 50f;      // tobillo 
            _joints[(int)JointType.FootLeft].radio = _joints[(int)JointType.FootRight].radio = 150f;        // Pie
            _joints[(int)JointType.KneeLeft].radio = _joints[(int)JointType.KneeRight].radio = 90f;         // Rodilla
            _joints[(int)JointType.HipLeft].radio = _joints[(int)JointType.HipRight].radio = 90f;           // Cadera
            _joints[(int)JointType.HipCenter].radio = 0;                                                  // Centro del cuerpo
            _joints[(int)JointType.Spine].radio = 0;                                                      // spina
            _joints[(int)JointType.ShoulderCenter].radio = 150;                                             // centro del torso
            _joints[(int)JointType.ShoulderLeft].radio = _joints[(int)JointType.ShoulderRight].radio = 80;  // hombros
            _joints[(int)JointType.ElbowLeft].radio = _joints[(int)JointType.ElbowRight].radio = 70;        // codo
            _joints[(int)JointType.WristLeft].radio = _joints[(int)JointType.WristRight].radio = 40;        // muñeca
            _joints[(int)JointType.HandLeft].radio = _joints[(int)JointType.HandRight].radio = 70;          // mano
            _joints[(int)JointType.Head].radio = 250;                                                       // cabeza


            _joints[(int)JointType.AnkleLeft].p_mesh = _joints[(int)JointType.AnkleRight].p_mesh = bola;      // tobillo 
            _joints[(int)JointType.FootLeft].p_mesh = _joints[(int)JointType.FootRight].p_mesh = bola;        // Pie
            _joints[(int)JointType.KneeLeft].p_mesh = _joints[(int)JointType.KneeRight].p_mesh = bola;         // Rodilla
            _joints[(int)JointType.HipLeft].p_mesh = _joints[(int)JointType.HipRight].p_mesh = bola;           // Cadera
            _joints[(int)JointType.HipCenter].p_mesh = culo;                                                  // Centro del cuerpo
            _joints[(int)JointType.Spine].p_mesh = null;                                                      // spina
            _joints[(int)JointType.ShoulderCenter].p_mesh = null;                                             // centro del torso
            _joints[(int)JointType.ShoulderLeft].p_mesh = _joints[(int)JointType.ShoulderRight].p_mesh = bola;  // hombros
            _joints[(int)JointType.ElbowLeft].p_mesh = _joints[(int)JointType.ElbowRight].p_mesh = bola;        // codo
            _joints[(int)JointType.WristLeft].p_mesh = _joints[(int)JointType.WristRight].p_mesh = bola;        // muñeca
            _joints[(int)JointType.HandLeft].p_mesh = _joints[(int)JointType.HandRight].p_mesh = bola;          // mano
            _joints[(int)JointType.Head].p_mesh = cabeza;                                                       // cabeza

            _bones[(int)JointType.AnkleLeft].radio = _bones[(int)JointType.AnkleRight].radio = 40f;      // Pie pp dicho 
            _bones[(int)JointType.FootLeft].radio = _bones[(int)JointType.FootRight].radio = 120f;        // Pantorilla
            _bones[(int)JointType.KneeLeft].radio = _bones[(int)JointType.KneeRight].radio = 140f;         // Mulso
            _bones[(int)JointType.HipLeft].radio = _bones[(int)JointType.HipRight].radio = 80f;           // Cadera
            _bones[(int)JointType.HipCenter].radio = 80;                                                  // Centro del cuerpo
            _bones[(int)JointType.Spine].radio = 80;                                                      // spina
            _bones[(int)JointType.ShoulderCenter].radio = 0;                                              // hueso interno
            _bones[(int)JointType.ShoulderLeft].radio = _bones[(int)JointType.ShoulderRight].radio = 40;  // hueso interno
            _bones[(int)JointType.ElbowLeft].radio = _bones[(int)JointType.ElbowRight].radio = 150;        // Brazp
            _bones[(int)JointType.WristLeft].radio = _bones[(int)JointType.WristRight].radio = 120;        // antebrazo
            _bones[(int)JointType.HandLeft].radio = _bones[(int)JointType.HandRight].radio = 60;          // mano pp dicha
            _bones[(int)JointType.Head].radio = 70;                                                        // cuello

        }

        // Copia los datos de la kinect al esqueleto privado del ejemplo
        public void UpdateSkeleton()
        {
            // Solo copia y pisa los datos si estan todos los huesos trackeados
            if (tgcKinect.skeleton_sel == -1 || !hay_escena)
                return;
            Skeleton skeleton = tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel];
            bool all_tracked = true;
            for (int i = 0; i < skeleton.Joints.Count && all_tracked; i++)
                if (skeleton.Joints[(JointType)i].TrackingState == JointTrackingState.NotTracked)
                    all_tracked = false;

            if (all_tracked)
            {
                // actualizo los datos privados
                for (int i = 0; i < skeleton.Joints.Count; i++)
                {
                    _joints[i].Position = new Vector3(skeleton.Joints[(JointType)i].Position.X, skeleton.Joints[(JointType)i].Position.Y, skeleton.Joints[(JointType)i].Position.Z);
                    _joints[i].JointType = (JointType)i;
                }
                _cant_joints = skeleton.Joints.Count;
                // Caso particular, el torso, creo un joint virtual en el punto intermedio entre el centro de hombros y la espina.
                _joints[_cant_joints].radio = 350;                                                                        
                _joints[_cant_joints].p_mesh = torso;                                                                     
                _joints[_cant_joints].Position = (_joints[(int)JointType.Spine].Position + _joints[(int)JointType.ShoulderCenter].Position) * 0.5f;
               _cant_joints++;

                for (int i = 0; i < skeleton.BoneOrientations.Count; i++)
                {
                    _bones[i].StartJoint = skeleton.BoneOrientations[(JointType)i].StartJoint;
                    _bones[i].EndJoint = skeleton.BoneOrientations[(JointType)i].EndJoint;
                }
                _cant_bones = skeleton.BoneOrientations.Count;
            }
        }

        public void renderSkeletonMesh()
        {
            if (!hay_escena )
                return;

            // Inicializacion en caliente
            if (hip0.X == float.MaxValue)
            {
                if (tgcKinect.skeleton_sel == -1)
                    return;

                Skeleton skeleton = tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel];
                if (skeleton.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked)
                {
                    // Es la primera vez que trackea el hueso, que se toma como referencia para todo el esqueleto
                    hip0.X = skeleton.Joints[JointType.HipCenter].Position.X;
                    hip0.Y = skeleton.Joints[JointType.HipCenter].Position.Y;
                    hip0.Z = skeleton.Joints[JointType.HipCenter].Position.Z;
                }
                else
                    return;         // todavia no tiene marco de referencia para dibujar
            }

            //Renderizar Joint
            Device device = GuiController.Instance.D3dDevice;
            Matrix ant_view = device.Transform.View * Matrix.Identity;

            // Uso el area de memoria propia y no la de la kinect ya que si hay joints no trackeados,
            // conviene usar el ultimo que tengo disponible
            Vector3[] pos_joint = new Vector3[26];
            for (int i = 0; i < _cant_joints; i++)
            {
                // LLevo el punto al espacio del esqueleto, luego lo escalo a milimetros y lo traslado al centro de la escena
                pos_joint[i] = (_joints[i].Position- hip0) * 1000 + center;
                // Pero solo lo renderizo si tiene radio y mesh asociado. 
                if (_joints[i].radio > 0 && _joints[i].p_mesh != null)
                {
                    float k = _joints[i].radio / _joints[i].p_mesh.size.Y;
                    _joints[i].p_mesh.transform = Matrix.Translation(-_joints[i].p_mesh.center) * Matrix.Scaling(k, k, k) * Matrix.Translation(pos_joint[i]);
                    _joints[i].p_mesh.render();
                }
            }

            for (int t = 0; t < _cant_bones; t++)
            if (_bones[t].radio>0)
            {
                int PStart = (int)_bones[t].StartJoint;
                int PEnd = (int)_bones[t].EndJoint;
                elipsoid.transform = calcularMatriz(elipsoid.size, pos_joint[PStart], pos_joint[PEnd],_bones[t].radio);
                elipsoid.render();
            }

            device.Transform.View = ant_view * Matrix.Identity;

        }


        public Matrix calcularMatriz(Vector3 mesh_size, Vector3 PStart, Vector3 PEnd,float boneR )
        {
            Vector3 N = PEnd - PStart;
            N.Normalize();
            Vector3 VUP;
            if (Math.Abs(N.X) <= Math.Abs(N.Y) && Math.Abs(N.X) <= Math.Abs(N.Z))
                VUP = new Vector3(1, 0, 0);
            else
                if (Math.Abs(N.Y) <= Math.Abs(N.X) && Math.Abs(N.Y) <= Math.Abs(N.Z))
                    VUP = new Vector3(0, 1, 0);
                else
                    VUP = new Vector3(0, 0, 1);

            Vector3 U = Vector3.Cross(N, VUP);
            Vector3 V = Vector3.Cross(U, N);

            Matrix transform = new Matrix();

            // Determino la escala para que la dimension en altura vaya desde PStart a PEnd
            float l = (PStart - PEnd).Length();
            float ky = l / mesh_size.Y;
            float kx = boneR / mesh_size.X;
            float kz = boneR / mesh_size.Z;

            // X
            transform.M11 = U.X;
            transform.M12 = U.Y;
            transform.M13 = U.Z;

            // Y
            transform.M21 = N.X;
            transform.M22 = N.Y;
            transform.M23 = N.Z;

            // Z
            transform.M31 = V.X;
            transform.M32 = V.Y;
            transform.M33 = V.Z;

            // Traslacion
            transform.M41 = PStart.X;
            transform.M42 = PStart.Y;
            transform.M43 = PStart.Z;

            // W
            transform.M14 = 0;
            transform.M24 = 0;
            transform.M34 = 0;
            transform.M44 = 1.0f;

            return Matrix.Scaling(kx, ky, kz) * transform;
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

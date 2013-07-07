using System;
using System.Collections.Generic;
using System.IO;
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


// fullScreenMode=true defaultExampleName="Ejemplo Focus Loader" defaultExampleCategory="Test" showModifiersPanel=false title="Kinect Focus Interaction" showTitleBar=false
namespace Examples.Test
{
    public struct st_joint
    {
        public Vector3 Position;
        public JointType JointType;
        public float radio;
        public TgcDXMesh p_mesh;
        public Vector3 WorldPosition;

    }

    public struct st_bone
    {
        public JointType StartJoint;
        public JointType EndJoint;
        public Vector2 size;
        public Matrix T;
        public Vector3 dir;
        public float angulo;
        public TgcDXMesh p_mesh;
        public float k;
        public Vector3 desf;
    }

    public enum ModoNavegacion
    {
        Gui,
        Camera,
        Picking,
        Avataring,
        Esqueleto,
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

        public const int ID_NAVEGACION_CAMARA = 200;
        public const int ID_NAVEGACION_PICKING = 201;
        public const int ID_NAVEGACION_AVATARING = 202;
        public const int ID_NAVEGACION_ESQUELETO = 203;

        public const int ID_WALK_LEFT = 400;
        public const int ID_WALK_RIGHT = 401;
        public const int ID_WALK_FOWARD = 402;
        public const int ID_WALK_BACK = 403;

        public const int ID_TOOGLE_PAN = 404;

        private List<TgcMesh> _meshes;
        private List<TgcMesh> _manijas;
        private FocusSet [] _conjuntos;
        TgcBoundingBox bounds;
        TgcKinect tgcKinect;
        TexturasFocus texturasFocus;
        TgcBox lightMesh;
        TgcPickingRay ray = new TgcPickingRay();
        ModoNavegacion modo_navegacion = ModoNavegacion.Gui;
        bool camara_pan = false;
        gui_navigate nav_btn;
        Vector3 ant_LA, ant_LF;

        // Esqueleto 3d
        private TgcDXMesh elipsoid;
        private TgcDXMesh bola;
        private TgcDXMesh culo;
        private TgcDXMesh torso;
        private TgcDXMesh cabeza;
        private TgcDXMesh pierna;
        private TgcDXMesh pantorrilla;
        private TgcDXMesh mano_der;
        private TgcDXMesh mano_izq;
        private TgcDXMesh pie_der;
        private TgcDXMesh pie_izq;
        private TgcDXMesh brazo;
        private TgcDXMesh antebrazo;
        private TgcDXMesh cabeza_dummy;
        private TgcDXMesh culo_dummy;
        private TgcDXMesh torso_dummy;
        private TgcDXMesh hueso;
        private TgcDXMesh disco;

        Vector3 hip0 = new Vector3(float.MaxValue, 0, 0);       // posicion inicial del esqueleto
        float altura_cadera = 750f;
        Vector3 center = new Vector3();                         // centro de la escena
        Vector3 center_original = new Vector3();                // centro de la escena (original)
        bool hay_escena = false;
        int tipo_avataring = 1;
        public float global_time = 0.0f;

        public bool msg_box_app_exit = false;

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
            //tgcKinect.EnableNearModeSkeletalTracking();
        

            // levanto el GUI
            gui.Create();
            loadConfig();

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
            gui.InsertMenuItem(ID_CAMBIAR_TEXTURAS, "Modificar Texturas","edit_tex.png", x0, y0 += dy2, dx, dy);
            gui.InsertMenuItem(ID_CAMBIAR_EMPUJADORES, "Modificar Manijas", "manijas.png",x0, y0 += dy2, dx, dy);
            gui.InsertMenuItem(ID_APP_EXIT, "Salir", "salir.png", x0, y0 += dy2, dx, dy);

            int sdx = 200;
            int sdy = 250;
            esqueleto2d = gui.InsertKinectSkeletonControl(W - sdx, H - sdy, sdx- 4, sdy-4);
            esqueleto2d.pir_min_x = tgcKinect.right_pir.x_min;
            esqueleto2d.pir_min_y = tgcKinect.right_pir.y_min;
            esqueleto2d.pir_max_x = tgcKinect.right_pir.x_max;
            esqueleto2d.pir_max_y = tgcKinect.right_pir.y_max;
            esqueleto2d.siempre_visible = true;

            // Camara para 3d support
            gui.camera = GuiController.Instance.FpsCamera;

            // Mallas .X para el esqueleto 3d
            elipsoid = new TgcDXMesh();
            elipsoid.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\pieza.x");
            hueso = new TgcDXMesh();
            hueso.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\hueso.x");
            bola = new TgcDXMesh();
            bola.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\ball.x");
            disco = new TgcDXMesh();
            disco.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\disco.x");
            culo = new TgcDXMesh();
            culo.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\wculo.x");
            torso = new TgcDXMesh();
            torso.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\wcuerpo.x");
            cabeza = new TgcDXMesh();
            cabeza.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\wcabeza.x");

            pierna = new TgcDXMesh();
            pierna.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\pierna.x");
            pantorrilla = new TgcDXMesh();
            pantorrilla.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\pantorrilla.x");
            mano_der = new TgcDXMesh();
            mano_der.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\right_hand.x");
            mano_izq = new TgcDXMesh();
            mano_izq.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\left_hand.x");
            pie_der = new TgcDXMesh();
            pie_der.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\right_foot.x");
            pie_izq = new TgcDXMesh();
            pie_izq.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\left_foot.x");
            brazo = new TgcDXMesh();
            brazo.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\brazo.x");
            antebrazo = new TgcDXMesh();
            antebrazo.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\antebrazo.x");


            // dummy
            cabeza_dummy = new TgcDXMesh();
            cabeza_dummy.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\cabeza.x");
            culo_dummy = new TgcDXMesh();
            culo_dummy.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\culo.x");
            torso_dummy = new TgcDXMesh();
            torso_dummy.loadMesh(GuiController.Instance.ExamplesMediaDir + "ModelosX\\torso.x");






        }


        public void InitSkeletonData()
        {
            switch (tipo_avataring)
            {
                case 0:
                    InitSkeletonData0();
                    break;
                case 1:
                    InitSkeletonData1();
                    break;
                case 2:
                    InitSkeletonData2();
                    break;
            }

        }

        public void renderSkeletonMesh()
        {
            switch (tipo_avataring)
            {
                case 0:
                    renderSkeletonMesh0();
                    break;
                case 1:
                    renderSkeletonMesh1();
                    break;
                case 2:
                    renderSkeletonMesh2();
                    break;
            }
         }



        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            global_time += elapsedTime;

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


                if (modo_navegacion == ModoNavegacion.Camera && gui.kinect.left_hand.position.Y < 200)
                {
                    //Actualizar Ray de colisi�n en base a posici�n de la mano
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

            if (modo_navegacion == ModoNavegacion.Avataring)
            {
                renderSkeletonMesh();
                // Verifico si la mano toca contra algun cojunto de focus
                Vector3 hand_size = new Vector3(100, 100, 100);
                Vector3 pos_mano_der = _joints[(int)JointType.HandRight].WorldPosition;
                TgcBoundingBox righthand = new TgcBoundingBox(pos_mano_der - hand_size, pos_mano_der + hand_size);
                //righthand.render();
                Vector3 pos_mano_izq = _joints[(int)JointType.HandLeft].WorldPosition;
                TgcBoundingBox lefthand = new TgcBoundingBox(pos_mano_izq - hand_size, pos_mano_izq + hand_size);
                //lefthand.render();

                foreach (FocusSet f in _conjuntos)
                {
                    TgcBoundingBox aabb = f.container.BoundingBox;
                    //Ejecutar test, si devuelve true se carga el punto de colision collisionPoint
                    bool selected = TgcCollisionUtils.testAABBAABB(aabb, righthand);
                    if (!selected)
                        selected = TgcCollisionUtils.testAABBAABB(aabb, lefthand);

                    if (selected)
                    {
                        f.animate();
                        //break;
                    }
                }

            }

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
                            // si cancelo el modo navegacion, paso a modo gui
                            modo_navegacion  = ModoNavegacion.Gui;
                            if (msg_box_app_exit)
                            {
                                // Es la resupuesta a un messagebox de salir del sistema
                                if (msg.id == IDOK)
                                {
                                    // Salgo del sistema
                                    GuiController.Instance.shutDown();
                                }
                            }
                            msg_box_app_exit = false;
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
                            // Selecciono el modo de navegacion
                            ModoNavegacionDlg();
                            break;
                        case ID_NAVEGACION_CAMARA:
                            ModoCamara();
                            break;
                        case ID_NAVEGACION_PICKING:
                            ModoPicking();
                            break;
                        case ID_NAVEGACION_AVATARING:
                            ModoAvataring(1);
                            break;
                        case ID_NAVEGACION_ESQUELETO:
                            ModoAvataring(2);
                            break;

                        case ID_TOOGLE_PAN:
                            {
                                camara_pan = !camara_pan;
                                gui_kinect_circle_button btn = (gui_kinect_circle_button)gui.GetDlgItem(ID_TOOGLE_PAN);
                                btn.text = camara_pan ? "PAN ON" : "PAN OFF";
                                nav_btn.modo_pan = camara_pan;
                            }
                            break;

                        // mover avatar
                        case ID_WALK_LEFT:
                            {
                                center.X -= 10;
                                Vector3 dp = new Vector3(-10, 0, 0);
                                Vector3 LF = GuiController.Instance.FpsCamera.getPosition();
                                Vector3 LA = GuiController.Instance.FpsCamera.getLookAt();
                                GuiController.Instance.FpsCamera.setCamera(LF+dp, LA+dp);
                            }
                            break;
                        case ID_WALK_RIGHT:
                            {
                                center.X += 10;
                                Vector3 dp = new Vector3(10, 0, 0);
                                Vector3 LF = GuiController.Instance.FpsCamera.getPosition();
                                Vector3 LA = GuiController.Instance.FpsCamera.getLookAt();
                                GuiController.Instance.FpsCamera.setCamera(LF + dp, LA + dp);
                            }
                            break;
                        case ID_WALK_FOWARD:
                            {
                                center.Z -= 10;
                                Vector3 dp = new Vector3(0, 0, -10);
                                Vector3 LF = GuiController.Instance.FpsCamera.getPosition();
                                Vector3 LA = GuiController.Instance.FpsCamera.getLookAt();
                                GuiController.Instance.FpsCamera.setCamera(LF + dp, LA + dp);
                            }
    
                            break;
                        case ID_WALK_BACK:
                            {
                                center.Z += 10;
                                Vector3 dp = new Vector3(0, 0, 10);
                                Vector3 LF = GuiController.Instance.FpsCamera.getPosition();
                                Vector3 LA = GuiController.Instance.FpsCamera.getLookAt();
                                GuiController.Instance.FpsCamera.setCamera(LF + dp, LA + dp);
                            }
                            break;

                        case ID_APP_EXIT:
                            // Salir
                            gui.MessageBox("Desea Salir del Sistema?", "Focus Kinect Interaction");
                            msg_box_app_exit = true;
                            break;

                        case ID_CAMBIAR_TEXTURAS:
                            // Cambiar Texturas
                            texturasFocus.TextureGroupDlg(gui);
                            break;

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
                                gui_mesh_button btn = (gui_mesh_button)gui.GetDlgItem(msg.id);
                                
                                TgcMesh tgcmesh = btn.mesh;
                                Mesh dxmesh = btn.mesh.D3dMesh;
                                dxmesh = dxmesh.Clone(MeshFlags.Use32Bit | MeshFlags.Managed, dxmesh.Declaration, dxmesh.Device);

                                int index = msg.id - 4000;
                                int[] alturaManijas = { 20, 20, 10, 12 };
                                int altura = alturaManijas[index];


                                //cambio todos los mesh de manija
                                foreach (TgcMesh m in _manijas)
                                {
                                    //recalculo la matriz
                                    Vector3 pmin = Vector3.TransformCoordinate(tgcmesh.BoundingBox.PMin, m.Transform);
                                    Vector3 pmax = Vector3.TransformCoordinate(tgcmesh.BoundingBox.PMax, m.Transform);
                                    pmin = Vector3.Minimize(pmax, pmin);
                                    pmax = Vector3.Maximize(pmax, pmin);
                                    float scale = altura / (pmax - pmin).Y;

                                    m.Transform = Matrix.Scaling(scale, scale, scale) *
                                        m.Transform;

                                    pmin = Vector3.TransformCoordinate(tgcmesh.BoundingBox.PMin, m.Transform);
                                    pmax = Vector3.TransformCoordinate(tgcmesh.BoundingBox.PMax, m.Transform);
                                    pmin = Vector3.Minimize(pmax, pmin);
                                    pmax = Vector3.Maximize(pmax, pmin);

                                    Vector3 center_m = (pmin + pmax) * 0.5f;
                                    
                                    m.D3dMesh = dxmesh;
                                    m.Materials = tgcmesh.Materials;
                                    m.DiffuseMaps = tgcmesh.DiffuseMaps;
                                    m.Transform = m.Transform *
                                        Matrix.Translation(m.BoundingBox.calculateBoxCenter() - center_m);
                                }

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
                                _manijas = loader.Manijas;
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
                                //center = new Vector3((x0 + x1) / 2, 1100, (z0 + z1) / 2);
                                center = new Vector3(x1-1300, altura_cadera, z1 - 1300);
                                center_original = center * 1.0f;

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


        public void ModoNavegacionDlg()
        {
            gui.InitDialog(false, false);
            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;
            int x0 = -20;
            int y0 = 40;
            int dy = 600;
            int dx = W + 40;

            gui.InsertFrame("Interactuar con la Cocina", x0, y0, dx, dy, Color.FromArgb(115, 40, 150),frameBorder.sin_borde);
            x0 += 40;
            y0 += 140;
            int r = 200;

            gui.InsertKinectCircleButton(ID_NAVEGACION_CAMARA, "Recorrer","camara.png", x0 , y0, r);
            gui.InsertKinectCircleButton(ID_NAVEGACION_AVATARING, "Avatar", "avatar.png", x0 += (r + 40), y0, r);
            gui.InsertKinectCircleButton(ID_NAVEGACION_ESQUELETO, "Dummy", "dummy.png", x0 += (r + 40), y0, r);
            gui.InsertKinectCircleButton(IDCANCEL, "Volver", "salir.png", x0 += (r + 40), y0, r);

        }


        public void ModoCamara()
        {
            modo_navegacion = ModoNavegacion.Camera;
            ant_LA = GuiController.Instance.FpsCamera.getLookAt();
            ant_LF = GuiController.Instance.FpsCamera.getPosition();
            gui.InitDialog(false, false);
            int dx = 250;
            int W = (int)(GuiController.Instance.Panel3d.Width / gui.ex);
            int H = (int)(GuiController.Instance.Panel3d.Height / gui.ey);
            nav_btn = gui.InsertNavigationControl(_meshes, W - dx - 5, 5, dx, dx);
            int pos_y = 50;
            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png",
                50, pos_y, gui.KINECT_BUTTON_SIZE_X);

            gui.InsertKinectCircleButton(ID_TOOGLE_PAN, "Pan", "pan.png",
                50, pos_y += gui.KINECT_BUTTON_SIZE_X + 50, gui.KINECT_BUTTON_SIZE_X);

            gui.InsertKinectCircleButton(ID_RESET_CAMARA, "Reset", "reset_camera.png",
                50, pos_y += gui.KINECT_BUTTON_SIZE_X + 50, gui.KINECT_BUTTON_SIZE_X);
        }


        public void ModoPicking()
        {
            modo_navegacion = ModoNavegacion.Picking;
            gui.InitDialog(false, false);
            int W = (int)(GuiController.Instance.Panel3d.Width / gui.ex);
            int H = (int)(GuiController.Instance.Panel3d.Height / gui.ey);
            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png",
                W - gui.KINECT_BUTTON_SIZE_X - 40 , 50, gui.KINECT_BUTTON_SIZE_X);
        }

        public void ModoAvataring(int tipo=1)
        {
            modo_navegacion = ModoNavegacion.Avataring;
            tipo_avataring = tipo;
            // x las dudas reseteo el centro de la escena
            center = center_original * 1.0f;
            InitSkeletonData();
            gui.InitDialog(false, false);
            int W = (int)(GuiController.Instance.Panel3d.Width / gui.ex);
            int H = (int)(GuiController.Instance.Panel3d.Height / gui.ey);

            int pos_y = 50;
            gui_item item = (gui_item)gui.InsertKinectCircleButton(ID_WALK_LEFT, "", "izquierda.png", 40, pos_y,  gui.KINECT_BUTTON_SIZE_X);
            item.auto_seleccionable = true;
            item = (gui_item)gui.InsertKinectCircleButton(ID_WALK_RIGHT, "", "derecha.png", 40, pos_y += gui.KINECT_BUTTON_SIZE_X + 30, gui.KINECT_BUTTON_SIZE_X);
            item.auto_seleccionable = true;
            item = (gui_item)gui.InsertKinectCircleButton(ID_WALK_FOWARD, "", "abajo.png", 40, pos_y += gui.KINECT_BUTTON_SIZE_X + 30, gui.KINECT_BUTTON_SIZE_X);
            item.auto_seleccionable = true;
            item = (gui_item)gui.InsertKinectCircleButton(ID_WALK_BACK, "", "arriba.png", 40, pos_y += gui.KINECT_BUTTON_SIZE_X + 30, gui.KINECT_BUTTON_SIZE_X);
            item.auto_seleccionable = true;

            gui_item cancel_btn = gui.InsertKinectCircleButton(IDCANCEL, "Cancel", "cancel.png",
                W - gui.KINECT_BUTTON_SIZE_X - 40, 50, gui.KINECT_BUTTON_SIZE_X);
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
        }

        // Determina si el esqueleto es valido. Tiene que cumplir con ciertas limitaciones fisicas de un esqueleto
        public bool IsSkeletonValid()
        {
            // De momento tienen que estar todos las articulaciones traqueadas o inferidas
            if (tgcKinect.skeleton_sel == -1)
                return false;

            Skeleton skeleton = tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel];
            bool all_tracked = true;
            int cant_inferidos = 0;

            for (int i = 0; i < skeleton.Joints.Count && all_tracked; i++)
                if (skeleton.Joints[(JointType)i].TrackingState == JointTrackingState.NotTracked)
                    all_tracked = false;
                else
                    if (skeleton.Joints[(JointType)i].TrackingState == JointTrackingState.Inferred)
                        ++cant_inferidos;


            if(!all_tracked/* || cant_inferidos>=6*/)
                return false;

            return true;
        }


        // Copia los datos de la kinect al esqueleto privado del ejemplo
        public void UpdateSkeleton()
        {
            if (!IsSkeletonValid() ||!hay_escena)
                return;
            Skeleton skeleton = tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel];
            // actualizo los datos privados
            for (int i = 0; i < skeleton.Joints.Count; i++)
            {

                Vector3 pos = new Vector3(skeleton.Joints[(JointType)i].Position.X, skeleton.Joints[(JointType)i].Position.Y, skeleton.Joints[(JointType)i].Position.Z);
                if(skeleton.Joints[(JointType)i].TrackingState==JointTrackingState.Tracked)
                    _joints[i].Position = pos;
                else
                    // La posicion esta inferida....
                    _joints[i].Position = _joints[i].Position*0.75f + pos*0.25f;

                _joints[i].JointType = (JointType)i;
                // LLevo el punto al espacio del esqueleto, luego lo escalo a milimetros y lo traslado al centro de la escena

                Vector3 modelPosition = _joints[i].Position - hip0;
                modelPosition.Z = -modelPosition.Z;
                _joints[i].WorldPosition = modelPosition * 1000 + center;
            }
            _cant_joints = skeleton.Joints.Count;

            for (int i = 0; i < skeleton.BoneOrientations.Count; i++)
            {
                _bones[i].StartJoint = skeleton.BoneOrientations[(JointType)i].StartJoint;
                _bones[i].EndJoint = skeleton.BoneOrientations[(JointType)i].EndJoint;
                Microsoft.Kinect.Vector4 Q = skeleton.BoneOrientations[(JointType)i].AbsoluteRotation.Quaternion;
                _bones[i].dir = new Vector3(Q.X, Q.Y, -Q.Z);
                _bones[i].angulo =  Q.W;
                Matrix4 T = skeleton.BoneOrientations[(JointType)i].AbsoluteRotation.Matrix;
                _bones[i].T = new Matrix();
                _bones[i].T.M11 = T.M11;
                _bones[i].T.M12 = T.M12;
                _bones[i].T.M13 = -T.M13;
                _bones[i].T.M14 = T.M14;

                _bones[i].T.M21 = T.M21;
                _bones[i].T.M22 = T.M22;
                _bones[i].T.M23 = -T.M23;
                _bones[i].T.M24 = T.M34;

                _bones[i].T.M31 = T.M31;
                _bones[i].T.M32 = T.M32;
                _bones[i].T.M33 = -T.M33;
                _bones[i].T.M34 = T.M34;

                _bones[i].T.M41 = T.M41;
                _bones[i].T.M42 = T.M42;
                _bones[i].T.M43 = T.M43;
                _bones[i].T.M44 = T.M44;

            }
            _cant_bones = skeleton.BoneOrientations.Count;
        }

        //
        // METODO 0
        //
        // Dibuja la malla usando la posicion y orientacion que viene de la kinect pero escala proporcional
        // resptando la malla original. El artifact es que los huesos pueden no quedar unidos si las posiciones no coinciden 
        // con la malla original
        public void InitSkeletonData0()
        {

            _bones[(int)JointType.FootLeft].p_mesh = pie_izq;                  // Pie pp dicho
            _bones[(int)JointType.FootRight].p_mesh = pie_izq;
            _bones[(int)JointType.AnkleLeft].p_mesh = _bones[(int)JointType.AnkleRight].p_mesh = pantorrilla;          // Pantoriilla
            _bones[(int)JointType.KneeLeft].p_mesh = _bones[(int)JointType.KneeRight].p_mesh = pierna;         // Mulso
            _bones[(int)JointType.HipLeft].p_mesh = _bones[(int)JointType.HipRight].p_mesh = null;           // Cadera
            _bones[(int)JointType.HipCenter].p_mesh = null;                                                  // Centro del cuerpo
            _bones[(int)JointType.Spine].p_mesh = null;                                                      // spina
            _bones[(int)JointType.ShoulderCenter].p_mesh = null;                                              // hueso interno
            _bones[(int)JointType.ShoulderLeft].p_mesh = _bones[(int)JointType.ShoulderRight].p_mesh = null;  // hueso interno
            _bones[(int)JointType.ElbowLeft].p_mesh = _bones[(int)JointType.ElbowRight].p_mesh = brazo;        // Brazp
            _bones[(int)JointType.WristLeft].p_mesh = _bones[(int)JointType.WristRight].p_mesh = antebrazo;        // antebrazo
            _bones[(int)JointType.HandLeft].p_mesh = mano_izq;                                                      // manos pp dicha
            _bones[(int)JointType.HandRight].p_mesh = mano_der;
            _bones[(int)JointType.Head].p_mesh = null;                                                        // cuello

            // Escala global y desfasaje global
            float escala_global = 1100f;
            for (int i = 0; i < 26; ++i)
            {
                _bones[i].k = escala_global;
                _bones[i].desf = new Vector3(0, 0, 0);
            }
            _bones[(int)JointType.Head].k = 1000f;

        }


        // Inicializa ciertos valores por primera y unica vez, si lo puede hacer devuelve true
        public bool IsSkeletonReady()
        {
            bool skeleton_ready = hip0.X == float.MaxValue ? false : true;
            // No hay escena
            if (!hay_escena)
                return skeleton_ready;
            // No hay esqueleto trackeado
            if (tgcKinect.skeleton_sel == -1)
                return skeleton_ready;

            // Inicializacion en caliente
            Skeleton skeleton = tgcKinect.auxSkeletonData[tgcKinect.skeleton_sel];
            bool hip_tracked = false;
            if (!skeleton_ready)
            {
                if (skeleton.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked)
                {
                    // Es la primera vez que trackea el hueso, que se toma como referencia para todo el esqueleto
                    hip0.X = skeleton.Joints[JointType.HipCenter].Position.X;
                    hip0.Y = skeleton.Joints[JointType.HipCenter].Position.Y;
                    hip0.Z = skeleton.Joints[JointType.HipCenter].Position.Z;
                    hip_tracked = true;
                    skeleton_ready = true;

                }
                else
                    return false;         // todavia no tiene marco de referencia para dibujar
            }

            return skeleton_ready;
        }

        public void renderSkeletonMesh0()
        {
            if (!IsSkeletonReady())
                return;

            Device device = GuiController.Instance.D3dDevice;
            Matrix ant_view = device.Transform.View * Matrix.Identity;

            for (int t = 0; t < _cant_bones; t++)
                if (_bones[t].p_mesh != null)
                {

                    int PStart = (int)_bones[t].StartJoint;
                    int PEnd = (int)_bones[t].EndJoint;
                    Vector3 bone_center = (_joints[PStart].WorldPosition + _joints[PEnd].WorldPosition) * 0.5f;
                    TgcDXMesh p_mesh = _bones[t].p_mesh;
                    p_mesh.transform = calcularMatriz(p_mesh.center, bone_center, _bones[t].k , _bones[t].T);
                    p_mesh.render();
                }


            // Cabeza 
            {
                // empiezo en el cuello y termino en en centro de la cabeza
                Matrix T = _bones[(int)JointType.HipCenter].T;
                Vector3 PCenter = _joints[(int)JointType.Head].WorldPosition;
                Vector3 HeadDir = PCenter - _joints[(int)JointType.ShoulderCenter].WorldPosition;
                HeadDir.Normalize();
                Vector3 PStart = PCenter + HeadDir * 70f;
                Vector3 PEnd = PCenter - HeadDir * 250f;
                Vector3 bone_center = (PStart + PEnd) * 0.5f;
                cabeza.transform = calcularMatriz(cabeza.center, bone_center, _bones[(int)JointType.Head].k, T);
                cabeza.render();
            }

            // Cuerpo
            {
                // Empieza en el centro de la cadera hasta el centro de los hombros
                Matrix T = _bones[(int)JointType.HipCenter].T;
                Vector3 PStart = _joints[(int)JointType.HipCenter].WorldPosition;
                Vector3 PEnd = _joints[(int)JointType.ShoulderCenter].WorldPosition;
                Vector3 TorsoDir = PEnd - PStart;
                TorsoDir.Normalize();
                PEnd = PEnd - 20 * TorsoDir;
                Vector3 bone_center = (PStart + PEnd) * 0.5f;
                torso.transform = calcularMatriz(torso.center, bone_center, _bones[(int)JointType.HipCenter].k, T);
                torso.render();
            }

            // culo
            {
                Matrix T = _bones[(int)JointType.HipCenter].T;
                Vector3 PStart = _joints[(int)JointType.HipCenter].WorldPosition;
                Vector3 CuloDir = new Vector3(T.M21, T.M22, T.M23);
                Vector3 PEnd = PStart - CuloDir * 200f;
                PStart = PStart + CuloDir * 100f;
                Vector3 bone_center = (PStart + PEnd) * 0.5f;
                culo.transform = calcularMatriz(culo.center, bone_center, _bones[(int)JointType.HipCenter].k, T);
                culo.render();
            }

            device.Transform.View = ant_view * Matrix.Identity;

        }

        //
        // METODO 1
        //
        // Dibuja la malla usando las posiciones Desde - Hasta de la kinect y escalando el resto desde un valor harcodeado
        // El artifact es que pueden cambiar de tama�o las partes del cuerpo
        public void InitSkeletonData1()
        {
            Vector2 cero = new Vector2(0, 0);
            // Inicializa la primera vez los valores hardcodeados del esqueleto 
            _joints[(int)JointType.AnkleLeft].radio = _joints[(int)JointType.AnkleRight].radio  = 40f;      // tobillo 
            _joints[(int)JointType.FootLeft].radio = _joints[(int)JointType.FootRight].radio = 100;        // Pie
            _joints[(int)JointType.KneeLeft].radio = _joints[(int)JointType.KneeRight].radio = 90f;         // Rodilla
            _joints[(int)JointType.HipLeft].radio = _joints[(int)JointType.HipRight].radio = 0f;           // Cadera
            _joints[(int)JointType.HipCenter].radio = 200;                                                  // Centro del cuerpo
            _joints[(int)JointType.Spine].radio = 0;                                                      // spina
            _joints[(int)JointType.ShoulderCenter].radio = 150;                                             // centro del torso
            _joints[(int)JointType.ShoulderLeft].radio = _joints[(int)JointType.ShoulderRight].radio = 0;  // hombros
            _joints[(int)JointType.ElbowLeft].radio = _joints[(int)JointType.ElbowRight].radio = 70;        // codo
            _joints[(int)JointType.WristLeft].radio = _joints[(int)JointType.WristRight].radio = 40;        // mu�eca
            _joints[(int)JointType.HandLeft].radio = _joints[(int)JointType.HandRight].radio = 70;          // mano
            _joints[(int)JointType.Head].radio = 0;                                                       // cabeza


            _joints[(int)JointType.FootLeft].p_mesh = bola;
            _joints[(int)JointType.FootRight].p_mesh = bola;        // Pie
            _joints[(int)JointType.AnkleLeft].p_mesh = _joints[(int)JointType.AnkleRight].p_mesh = bola;      // tobillo 
            _joints[(int)JointType.KneeLeft].p_mesh = _joints[(int)JointType.KneeRight].p_mesh = bola;         // Rodilla
            _joints[(int)JointType.HipLeft].p_mesh = _joints[(int)JointType.HipRight].p_mesh = null;           // Cadera
            _joints[(int)JointType.HipCenter].p_mesh = null;                                                  // Centro del cuerpo
            _joints[(int)JointType.Spine].p_mesh = null;                                                      // spina
            _joints[(int)JointType.ShoulderCenter].p_mesh = null;                                             // centro del torso
            _joints[(int)JointType.ShoulderLeft].p_mesh = _joints[(int)JointType.ShoulderRight].p_mesh = bola;  // hombros
            _joints[(int)JointType.ElbowLeft].p_mesh = _joints[(int)JointType.ElbowRight].p_mesh = bola;        // codo
            _joints[(int)JointType.WristLeft].p_mesh = _joints[(int)JointType.WristRight].p_mesh = bola;        // mu�eca
            _joints[(int)JointType.HandLeft].p_mesh = _joints[(int)JointType.HandRight].p_mesh = bola;          // mano
            _joints[(int)JointType.Head].p_mesh = null;                                                       // cabeza



            _bones[(int)JointType.FootLeft].size = _bones[(int)JointType.FootRight].size = new Vector2(70f,110f);        // Pie pp dicho
            _bones[(int)JointType.AnkleLeft].size = _bones[(int)JointType.AnkleRight].size = new Vector2(100f,100f);          // Pantoriilla
            _bones[(int)JointType.KneeLeft].size = _bones[(int)JointType.KneeRight].size = new Vector2(180f,120f);         // Mulso
            _bones[(int)JointType.HipLeft].size = _bones[(int)JointType.HipRight].size = cero;           // Cadera
            _bones[(int)JointType.HipCenter].size = cero;                                                  // Centro del cuerpo
            _bones[(int)JointType.Spine].size = cero;                                                      // spina
            _bones[(int)JointType.ShoulderCenter].size = cero;                                              // hueso interno
            _bones[(int)JointType.ShoulderLeft].size = _bones[(int)JointType.ShoulderRight].size= new Vector2(20f,20f);  // hueso interno
            _bones[(int)JointType.ElbowLeft].size = _bones[(int)JointType.ElbowRight].size = new Vector2(90f,90f);        // Brazp
            _bones[(int)JointType.WristLeft].size = _bones[(int)JointType.WristRight].size = new Vector2(80f,80f);        // antebrazo
            _bones[(int)JointType.HandLeft].size = _bones[(int)JointType.HandRight].size = new Vector2(80f,50f);          // mano pp dicha
            _bones[(int)JointType.Head].size = new Vector2(40f,40f);                                                        // cuello

            _bones[(int)JointType.FootLeft].p_mesh = null;                  // Pie pp dicho
            _bones[(int)JointType.FootRight].p_mesh = null;
            _bones[(int)JointType.AnkleLeft].p_mesh = _bones[(int)JointType.AnkleRight].p_mesh = pantorrilla;          // Pantoriilla
            _bones[(int)JointType.KneeLeft].p_mesh = _bones[(int)JointType.KneeRight].p_mesh = pierna;         // Mulso
            _bones[(int)JointType.HipLeft].p_mesh = _bones[(int)JointType.HipRight].p_mesh = null;           // Cadera
            _bones[(int)JointType.HipCenter].p_mesh = null;                                                  // Centro del cuerpo
            _bones[(int)JointType.Spine].p_mesh = null;                                                      // spina
            _bones[(int)JointType.ShoulderCenter].p_mesh = null;                                              // hueso interno
            _bones[(int)JointType.ShoulderLeft].p_mesh = _bones[(int)JointType.ShoulderRight].p_mesh = null;  // hueso interno
            _bones[(int)JointType.ElbowLeft].p_mesh = _bones[(int)JointType.ElbowRight].p_mesh = brazo;        // Brazp
            _bones[(int)JointType.WristLeft].p_mesh = _bones[(int)JointType.WristRight].p_mesh = antebrazo;        // antebrazo
            _bones[(int)JointType.HandLeft].p_mesh = mano_izq;                                                      // manos pp dicha
            _bones[(int)JointType.HandRight].p_mesh = mano_der;
            _bones[(int)JointType.Head].p_mesh = null;                                                        // cuello

        }

        public void renderSkeletonMesh1()
        {
            if (!IsSkeletonReady())
                return;

            //Renderizar Joint
            Device device = GuiController.Instance.D3dDevice;
            Matrix ant_view = device.Transform.View * Matrix.Identity;

            float K = 1.0f;

            // Uso el area de memoria propia y no la de la kinect ya que si hay joints no trackeados,
            // conviene usar el ultimo que tengo disponible
            for (int i = 0; i < _cant_joints; i++)
            {
                // Pero solo lo renderizo si tiene radio y mesh asociado. 
                if (_joints[i].radio > 0 && _joints[i].p_mesh != null)
                {
                    float k = _joints[i].radio / _joints[i].p_mesh.size.Y * K;
                    _joints[i].p_mesh.transform = Matrix.Translation(-_joints[i].p_mesh.center) * Matrix.Scaling(k, k, k) 
                            * Matrix.Translation(_joints[i].WorldPosition);
                    _joints[i].p_mesh.render();
                }
            }

            for (int t = 0; t < _cant_bones; t++)
                if (_bones[t].size.X > 0 && _bones[t].p_mesh != null)
                {

                    int PStart = (int)_bones[t].StartJoint;
                    int PEnd = (int)_bones[t].EndJoint;

                    TgcDXMesh p_mesh = _bones[t].p_mesh;
                    p_mesh.transform = calcularMatriz(p_mesh.bb_p0, p_mesh.bb_p1,
                            _joints[PStart].WorldPosition, _joints[PEnd].WorldPosition, 
                                _bones[t].size.X * K, _bones[t].size.Y * K, _bones[t].T);
                    p_mesh.render();
                }

            // Casos particulares
            // Cabeza 
            {
                // empiezo en el cuello y termino en en centro de la cabeza
                Vector3 PCenter = _joints[(int)JointType.Head].WorldPosition;
                Vector3 HeadDir = PCenter - _joints[(int)JointType.ShoulderCenter].WorldPosition;
                HeadDir.Normalize();
                Vector3 PStart = PCenter + HeadDir * 50f * K;
                Vector3 PEnd = PCenter - HeadDir * 270f * K;
                cabeza.transform = calcularMatriz(cabeza.bb_p0, cabeza.bb_p1, PStart, PEnd,
                        180 * K, 170 * K, _bones[(int)JointType.Head].T);
                cabeza.render();

                elipsoid.transform = calcularMatriz(elipsoid.bb_p0, elipsoid.bb_p1, PStart, PEnd,
                        30 * K, 30 * K, _bones[(int)JointType.Head].T);
                elipsoid.render();
            }


            // Cuerpo
            {
                // Empieza en el centro de la cadera hasta el centro de los hombros
                Vector3 PStart = _joints[(int)JointType.HipCenter].WorldPosition;
                Vector3 PEnd = _joints[(int)JointType.ShoulderCenter].WorldPosition;
                Vector3 TorsoDir = PEnd - PStart;
                TorsoDir.Normalize();
                PEnd = PEnd - 20 * TorsoDir * K;
                torso.transform = calcularMatriz(torso.bb_p0, torso.bb_p1, PStart, PEnd,
                        380 * K, 230 * K, _bones[(int)JointType.HipCenter].T);
                torso.render();
            }

            // culo
            {
                Matrix T = _bones[(int)JointType.HipCenter].T;
                Vector3 PStart = _joints[(int)JointType.HipCenter].WorldPosition;
                Vector3 CuloDir = new Vector3(T.M21, T.M22, T.M23);
                Vector3 PEnd = PStart - CuloDir * 200f * K;
                PStart = PStart + CuloDir * 100f * K;
                culo.transform = calcularMatriz(culo.bb_p0, culo.bb_p1, PStart, PEnd, 350 * K, 230 * K, T);
                culo.render();
            }

            device.Transform.View = ant_view * Matrix.Identity;

        }


        //
        // METODO 2
        //
        // dibuja el esqueleto directamente desde la info de la kinect, ubicando distintas mallas genericas
        // como bolas y partes de cuerpo pero sin skining. 
        public void InitSkeletonData2()
        {
            Vector2 cero = new Vector2(0, 0);
            // Inicializa la primera vez los valores hardcodeados del esqueleto 
            _joints[(int)JointType.AnkleLeft].radio = _joints[(int)JointType.AnkleRight].radio = 50f;      // tobillo 
            _joints[(int)JointType.FootLeft].radio = _joints[(int)JointType.FootRight].radio = 50f;        // Pie
            _joints[(int)JointType.KneeLeft].radio = _joints[(int)JointType.KneeRight].radio = 50f;         // Rodilla
            _joints[(int)JointType.HipLeft].radio = _joints[(int)JointType.HipRight].radio = 50f;           // Cadera
            _joints[(int)JointType.HipCenter].radio = 50;                                                  // Centro del cuerpo
            _joints[(int)JointType.Spine].radio = 50;                                                      // spina
            _joints[(int)JointType.ShoulderCenter].radio = 50;                                             // centro del torso
            _joints[(int)JointType.ShoulderLeft].radio = _joints[(int)JointType.ShoulderRight].radio = 140;  // hombros
            _joints[(int)JointType.ElbowLeft].radio = _joints[(int)JointType.ElbowRight].radio = 50;        // codo
            _joints[(int)JointType.WristLeft].radio = _joints[(int)JointType.WristRight].radio = 50;        // mu�eca
            _joints[(int)JointType.HandLeft].radio = _joints[(int)JointType.HandRight].radio = 0;          // mano
            _joints[(int)JointType.Head].radio = 0;                                                       // cabeza


            _joints[(int)JointType.FootLeft].p_mesh = _joints[(int)JointType.FootRight].p_mesh = bola;        // Pie
            _joints[(int)JointType.AnkleLeft].p_mesh = _joints[(int)JointType.AnkleRight].p_mesh = bola;      // tobillo 
            _joints[(int)JointType.KneeLeft].p_mesh = _joints[(int)JointType.KneeRight].p_mesh = bola;         // Rodilla
            _joints[(int)JointType.HipLeft].p_mesh = _joints[(int)JointType.HipRight].p_mesh = null;           // Cadera
            _joints[(int)JointType.HipCenter].p_mesh = null;                                                  // Centro del cuerpo
            _joints[(int)JointType.Spine].p_mesh = null;                                                      // spina
            _joints[(int)JointType.ShoulderCenter].p_mesh = null;                                             // centro del torso
            _joints[(int)JointType.ShoulderLeft].p_mesh = _joints[(int)JointType.ShoulderRight].p_mesh = bola;  // hombros
            _joints[(int)JointType.ElbowLeft].p_mesh = _joints[(int)JointType.ElbowRight].p_mesh = bola;        // codo
            _joints[(int)JointType.WristLeft].p_mesh = _joints[(int)JointType.WristRight].p_mesh = bola;        // mu�eca
            _joints[(int)JointType.HandLeft].p_mesh = _joints[(int)JointType.HandRight].p_mesh = null;          // mano
            _joints[(int)JointType.Head].p_mesh = null;                                                       // cabeza



            _bones[(int)JointType.FootLeft].size = _bones[(int)JointType.FootRight].size = new Vector2(70f, 110f);        // Pie pp dicho
            _bones[(int)JointType.AnkleLeft].size = _bones[(int)JointType.AnkleRight].size = new Vector2(100f, 100f);          // Pantoriilla
            _bones[(int)JointType.KneeLeft].size = _bones[(int)JointType.KneeRight].size = new Vector2(140f, 100f);         // Mulso
            _bones[(int)JointType.HipLeft].size = _bones[(int)JointType.HipRight].size = cero;           // Cadera
            _bones[(int)JointType.HipCenter].size = cero;                                                  // Centro del cuerpo
            _bones[(int)JointType.Spine].size = cero;                                                      // spina
            _bones[(int)JointType.ShoulderCenter].size = cero;                                              // hueso interno
            _bones[(int)JointType.ShoulderLeft].size = _bones[(int)JointType.ShoulderRight].size = new Vector2(20f, 20f);  // hueso interno
            _bones[(int)JointType.ElbowLeft].size = _bones[(int)JointType.ElbowRight].size = new Vector2(100f, 100f);        // Brazp
            _bones[(int)JointType.WristLeft].size = _bones[(int)JointType.WristRight].size = new Vector2(60f, 60f);        // antebrazo
            _bones[(int)JointType.HandLeft].size = _bones[(int)JointType.HandRight].size = new Vector2(80f, 50f);          // mano pp dicha
            _bones[(int)JointType.Head].size = new Vector2(40f, 40f);                                                        // cuello

            _bones[(int)JointType.FootLeft].p_mesh = elipsoid;                  // Pie pp dicho
            _bones[(int)JointType.FootRight].p_mesh = elipsoid;
            _bones[(int)JointType.AnkleLeft].p_mesh = _bones[(int)JointType.AnkleRight].p_mesh = elipsoid;          // Pantoriilla
            _bones[(int)JointType.KneeLeft].p_mesh = _bones[(int)JointType.KneeRight].p_mesh = elipsoid;         // Mulso
            _bones[(int)JointType.HipLeft].p_mesh = _bones[(int)JointType.HipRight].p_mesh = null;           // Cadera
            _bones[(int)JointType.HipCenter].p_mesh = null;                                                  // Centro del cuerpo
            _bones[(int)JointType.Spine].p_mesh = null;                                                      // spina
            _bones[(int)JointType.ShoulderCenter].p_mesh = null;                                              // hueso interno
            _bones[(int)JointType.ShoulderLeft].p_mesh = _bones[(int)JointType.ShoulderRight].p_mesh = elipsoid;  // hueso interno
            _bones[(int)JointType.ElbowLeft].p_mesh = _bones[(int)JointType.ElbowRight].p_mesh = elipsoid;        // Brazp
            _bones[(int)JointType.WristLeft].p_mesh = _bones[(int)JointType.WristRight].p_mesh = elipsoid;        // antebrazo
            _bones[(int)JointType.HandLeft].p_mesh = mano_izq;                                                      // manos pp dicha
            _bones[(int)JointType.HandRight].p_mesh = mano_der;
            _bones[(int)JointType.Head].p_mesh = null;                                                        // cuello

        }

        public void renderSkeletonMesh2()
        {
            if (!IsSkeletonReady())
                return;

            //Renderizar Joint
            Device device = GuiController.Instance.D3dDevice;
            Matrix ant_view = device.Transform.View * Matrix.Identity;

            float K = 1.0f;


            bool ant_blend = device.RenderState.AlphaBlendEnable;
            device.RenderState.AlphaBlendEnable = true;
            Effect currentShader = GuiController.Instance.Shaders.TgcMeshPhongShader;

            // Huesos internos
            for (int t = 0; t < _cant_bones; t++)
            {

                int PStart = (int)_bones[t].StartJoint;
                int PEnd = (int)_bones[t].EndJoint;
                TgcDXMesh p_mesh = hueso;
                float escala_x = 40.0f / bola.size.X * K;
                float escala_z = 40.0f / bola.size.Z * K;
                p_mesh.transform = calcularMatriz(p_mesh.bb_p0, p_mesh.bb_p1,
                        _joints[PStart].WorldPosition, _joints[PEnd].WorldPosition, escala_x, escala_z, _bones[t].T);
                p_mesh.render();
            }

            currentShader.SetValue("global_alpha", 0.5f);

            for (int I = 0; I < 2; ++I)
            {
                device.RenderState.CullMode = I == 0 ? Cull.Clockwise : Cull.CounterClockwise;

                // Uso el area de memoria propia y no la de la kinect ya que si hay joints no trackeados,
                // conviene usar el ultimo que tengo disponible
                for (int i = 0; i < _cant_joints; i++)
                {
                    // solo lo dibujo si tiene mesh
                    TgcDXMesh p_mesh = _joints[i].p_mesh;
                    if (p_mesh != null)
                    {
                        float k = _joints[i].radio / p_mesh.size.Y * K;
                        p_mesh.transform = Matrix.Translation(-p_mesh.center) * Matrix.Scaling(k, k, k)
                            * Matrix.Translation(_joints[i].WorldPosition);
                        p_mesh.render();
                    }
                }

                for (int t = 0; t < _cant_bones; t++)
                    if (_bones[t].p_mesh != null)
                    {

                        int PStart = (int)_bones[t].StartJoint;
                        int PEnd = (int)_bones[t].EndJoint;
                        TgcDXMesh p_mesh = _bones[t].p_mesh;
                        float escala_x = _bones[t].size.X / bola.size.X * K;
                        float escala_z = _bones[t].size.Y / bola.size.Z * K;
                        p_mesh.transform = calcularMatriz(p_mesh.bb_p0, p_mesh.bb_p1,
                                _joints[PStart].WorldPosition, _joints[PEnd].WorldPosition, escala_x, escala_z, _bones[t].T);
                        p_mesh.render();
                    }

                // Casos particulares
                // cabeza
                {
                    // empiezo en el cuello y termino en en centro de la cabeza
                    Vector3 PCenter = _joints[(int)JointType.Head].WorldPosition;
                    Vector3 HeadDir = PCenter - _joints[(int)JointType.ShoulderCenter].WorldPosition;
                    HeadDir.Normalize();
                    Vector3 PStart = PCenter + HeadDir * 70f * K;
                    Vector3 PEnd = PCenter - HeadDir * 130f * K;
                    cabeza_dummy.transform = calcularMatriz(cabeza_dummy.bb_p0, cabeza_dummy.bb_p1, PStart, PEnd,
                            160 * K, 140 * K, _bones[(int)JointType.Head].T);
                    cabeza_dummy.render();

                    // Cuello
                    PStart = _joints[(int)JointType.Head].WorldPosition - HeadDir * 30f * K;
                    PEnd = _joints[(int)JointType.ShoulderCenter].WorldPosition - HeadDir * 60f * K;
                    elipsoid.transform = calcularMatriz(elipsoid.bb_p0, elipsoid.bb_p1, PStart, PEnd,
                            50 * K, 50 * K, _bones[(int)JointType.Head].T);
                    elipsoid.render();

                }

                // Cuerpo
                {
                    // Empieza en el centro de la cadera hasta el centro de los hombros
                    Vector3 PStart = _joints[(int)JointType.HipCenter].WorldPosition;
                    Vector3 PEnd = _joints[(int)JointType.ShoulderCenter].WorldPosition;
                    Vector3 TorsoDir = PEnd - PStart;
                    TorsoDir.Normalize();
                    PStart = PStart + 50 * TorsoDir * K;
                    PEnd = PEnd - 0 * TorsoDir * K;
                    torso_dummy.transform = calcularMatriz(torso_dummy.bb_p0, torso_dummy.bb_p1, PStart, PEnd,
                            380 * K, 230 * K, _bones[(int)JointType.HipCenter].T);
                    torso_dummy.render();
                }

                // culo
                {
                    Matrix T = _bones[(int)JointType.HipCenter].T;
                    Vector3 PStart = _joints[(int)JointType.HipCenter].WorldPosition;
                    Vector3 CuloDir = new Vector3(T.M21, T.M22, T.M23);
                    Vector3 PEnd = PStart - CuloDir * 200f * K;
                    PStart = PStart + CuloDir * 50f * K;
                    culo_dummy.transform = calcularMatriz(culo_dummy.bb_p0, culo_dummy.bb_p1, PStart, PEnd, 350 * K, 230 * K, T);
                    culo_dummy.render();
                }

            }
            // Dibujo un disco en el piso
            device.RenderState.CullMode = Cull.None;
            currentShader.SetValue("global_alpha", 1f);

            // Dibujo un disco sobre el piso en el lugar del dummy
            {
                // Tomo la cadera proyectada al piso
                Vector3 HipProj = new Vector3(_joints[(int)JointType.HipCenter].WorldPosition.X, 40, _joints[(int)JointType.HipCenter].WorldPosition.Z);
                float k = 600f / disco.size.X;
                disco.transform = Matrix.Translation(-disco.center) * Matrix.RotationY(global_time) * Matrix.Scaling(k, 0, k)
                    * Matrix.Translation(HipProj);
                disco.render();
            }



            /*
            debugJoint(JointType.KneeLeft, JointType.HipLeft, JointType.FootLeft, "rodi");
            debugJoint(JointType.KneeRight, JointType.HipRight, JointType.FootRight, "rodd");

            debugJoint(JointType.ElbowLeft, JointType.ShoulderLeft, JointType.WristLeft, "codi");
            debugJoint(JointType.ElbowRight, JointType.ShoulderRight, JointType.WristRight, "codd");
            for (int t = 0; t < _cant_bones; t++)
                if (_bones[t].p_mesh != null)
                {

                    int PStart = (int)_bones[t].StartJoint;
                    int PEnd = (int)_bones[t].EndJoint;
                    TgcArrow p_arrow = new TgcArrow();
                    p_arrow.PStart = _joints[PStart].WorldPosition;
                    p_arrow.PEnd = _joints[PEnd].WorldPosition;
                    p_arrow.HeadSize = new Vector2(10, 30);
                    p_arrow.BodyColor = Color.Red;
                    p_arrow.HeadColor = Color.Green;
                    p_arrow.Thickness = 4f;
                    p_arrow.updateValues();
                    p_arrow.render();
                    
                    float bone_size = (_joints[PEnd].WorldPosition - _joints[PStart].WorldPosition).Length();
                    Vector3 N = new Vector3(_bones[t].T.M21 , _bones[t].T.M22 , _bones[t].T.M23 );
                    p_arrow.PStart = _joints[PStart].WorldPosition;
                    p_arrow.PEnd = _joints[PStart].WorldPosition + N*bone_size;
                    p_arrow.HeadSize = new Vector2(10, 30);
                    p_arrow.BodyColor = Color.Blue;
                    p_arrow.HeadColor = Color.White;
                    p_arrow.Thickness = 4f;
                    p_arrow.updateValues();
                    p_arrow.render();


                    Vector3 Tg = new Vector3(_bones[t].T.M11, _bones[t].T.M12, _bones[t].T.M13);
                    p_arrow.PStart = _joints[PStart].WorldPosition;
                    p_arrow.PEnd = _joints[PStart].WorldPosition + Tg * 50;
                    p_arrow.HeadSize = new Vector2(5, 5);
                    p_arrow.BodyColor = Color.Red;
                    p_arrow.HeadColor = Color.Red;
                    p_arrow.Thickness = 4f;
                    p_arrow.updateValues();
                    p_arrow.render();

                    Vector3 BTg = new Vector3(_bones[t].T.M31, _bones[t].T.M32, _bones[t].T.M33);
                    p_arrow.PStart = _joints[PStart].WorldPosition;
                    p_arrow.PEnd = _joints[PStart].WorldPosition + BTg * 50;
                    p_arrow.HeadSize = new Vector2(5,5);
                    p_arrow.BodyColor = Color.Green;
                    p_arrow.HeadColor = Color.Green;
                    p_arrow.Thickness = 4f;
                    p_arrow.updateValues();
                    p_arrow.render();


                }
             */


            // Restauro 
            device.Transform.View = ant_view * Matrix.Identity;
            device.RenderState.AlphaBlendEnable = ant_blend;

        }

        public void debugJoint(JointType i, JointType padre,JointType hijo,string nombre)
        {
            Device device = GuiController.Instance.D3dDevice;

            Vector3 pA = _joints[(int)padre].Position;
            Vector3 pB = _joints[(int)i].Position;
            Vector3 pC = _joints[(int)hijo].Position;

            Vector3 JU = pA - pB;
            Vector3 JV = pC - pB;
            JU.Normalize();
            JV.Normalize();
            int an = (int)((float)Math.Acos((double)Vector3.Dot(JU, JV)) * 180.0f / 3.1415f);

            Vector3 pt = _joints[(int)i].WorldPosition;
            pt.Project(device.Viewport, device.Transform.Projection, device.Transform.View, Matrix.Identity);
            gui.TextOut((int)pt.X, (int)pt.Y, nombre + an.ToString());
        }

        public Matrix calcularMatriz(Vector3 mesh_center, Vector3 bone_center,float k,Matrix Rot)
        {
            return Matrix.Translation(-mesh_center) 
                    * Matrix.Scaling(k, k, k) 
                    * Rot 
                    * Matrix.Translation(bone_center);
        }
        
        
        public Matrix calcularMatriz(Vector3 mesh_p0, Vector3 mesh_p1, Vector3 PStart, Vector3 PEnd, float boneSizeX , float boneSizeZ,Matrix Rot)
        {
            Vector3 mesh_size = mesh_p1 - mesh_p0;
            Vector3 N = PEnd - PStart;
            Matrix transform = new Matrix();

            // Determino la escala para que la dimension en altura vaya desde PStart a PEnd
            float l = (PStart - PEnd).Length();
            float ky = l / mesh_size.Y;
            float kx = boneSizeX / mesh_size.X;
            float kz = boneSizeZ / mesh_size.Z;

            return Matrix.Translation(-mesh_p0 - mesh_size * 0.5f) * Matrix.Scaling(kx, ky, kz) * Rot * Matrix.Translation((PStart + PEnd) * 0.5f);
        }

        /*
        public Matrix calcularMatriz(Vector3 mesh_p0,Vector3 mesh_p1, Vector3 PStart, Vector3 PEnd,float boneR )
        {
            Vector3 mesh_size = mesh_p1 - mesh_p0;
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
            transform.M41 = (PStart.X + PEnd.X) * 0.5f;
            transform.M42 = (PStart.Y + PEnd.Y) * 0.5f;
            transform.M43 = (PStart.Z + PEnd.Z) * 0.5f;

            // W
            transform.M14 = 0;
            transform.M24 = 0;
            transform.M34 = 0;
            transform.M44 = 1.0f;

            //return Matrix.Translation(-mesh_p0-mesh_size*0.5f) * Matrix.Scaling(kx, ky, kz) * transform;
            return Matrix.Translation(-mesh_p0-mesh_size*0.5f) * Matrix.Scaling(kx, ky, kz)
                    * Matrix.Translation((PStart + PEnd)*0.5f);
        }
        */

        public void loadConfig()
        {
            string line = null;
            System.IO.TextReader readFile = new StreamReader("config.dat");
            while ((line = readFile.ReadLine())!=null)
            {
                int p = line.IndexOf("MOUSE_SNAP=");
                if (p != -1)
                    gui.kinect.MOUSE_SNAP = int.Parse(line.Substring(p + 11));

                p = line.IndexOf("ALTURA_CADERA=");
                if (p != -1)
                    altura_cadera = float.Parse(line.Substring(p + 14));

                p = line.IndexOf("TIME_PRESS=");
                if (p != -1)
                    gui.TIMER_QUIETO_PRESSING = float.Parse(line.Substring(p + 11));

            }

            readFile.Close();
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
            if(_meshes != null)
            foreach (TgcMesh m in _meshes)
            {
                if(m != null)
                    m.dispose();
            }
            gui.Dispose();

            elipsoid.Dispose();
            hueso.Dispose();
            bola.Dispose();
            disco.Dispose();
            culo.Dispose();
            torso.Dispose();
            cabeza.Dispose();
            pierna.Dispose();
            pantorrilla.Dispose();
            mano_der.Dispose();
            mano_izq.Dispose();
            pie_der.Dispose();
            pie_izq.Dispose();
            brazo.Dispose();
            antebrazo.Dispose();

            cabeza_dummy.Dispose();
            culo_dummy.Dispose();
            torso_dummy.Dispose();

        }
        
    }
}

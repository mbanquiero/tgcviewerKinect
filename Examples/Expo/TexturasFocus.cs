using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.Gui;
using TgcViewer;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using Examples.Focus;

namespace Examples.Expo
{
    /// <summary>
    /// Grupos de texturas de focus
    /// </summary>
    public class TexturasFocus
    {
        public List<Group> grupos;
        Group selectedGroup;


        public TexturasFocus()
        {
            grupos = new List<Group>();
            Group group;
            int g = 1000;
            int t = 2000;

            //Maderas
            group = new Group("Maderas", g++, "cerrados.png");
            group.add(new Texture(t++, "Maderas\\09-guindo.jpg"));
            group.add(new Texture(t++, "Maderas\\05-nogal.jpg"));
            group.add(new Texture(t++, "Maderas\\06-roble.jpg"));
            group.add(new Texture(t++, "Maderas\\12-wengue.jpg"));
            group.add(new Texture(t++, "Maderas\\Blanco.jpg"));
            group.add(new Texture(t++, "masisa\\amarillo.jpg"));
            group.add(new Texture(t++, "masisa\\acacia.jpg"));
            group.add(new Texture(t++, "masisa\\almendra.jpg"));
            group.add(new Texture(t++, "masisa\\blanco.jpg"));
            group.add(new Texture(t++, "masisa\\cerezo.jpg"));
            group.add(new Texture(t++, "masisa\\haya catedral.jpg"));
            group.add(new Texture(t++, "masisa\\laricina.jpg"));
            group.add(new Texture(t++, "masisa\\aluminio.jpg"));
            group.add(new Texture(t++, "masisa\\gris.jpg"));
            group.add(new Texture(t++, "masisa\\rojo colonial.jpg"));
            grupos.Add(group);

            /*
            //Metales
            group = new Group("Metales", g++, "metales\\AD 306 steel gray.jpg");
            group.add(new Texture(t++, "Metales\\AD 306 steel gray.jpg"));
            group.add(new Texture(t++, "Metales\\aluminio.jpg"));
            group.add(new Texture(t++, "Metales\\cromado.jpg"));
            group.add(new Texture(t++, "Metales\\metal1.jpg"));
            group.add(new Texture(t++, "Metales\\an. bronze.jpg"));
            grupos.Add(group);
             
            //Masisa
            grupos.Add(group);
             */

            //Piso
            group = new Group("Piso", g++, "piso.png");
            group.add(new Texture(t++, "piso\\ceramicas\\agrupadas\\agrupadas64.jpg"));
            group.add(new Texture(t++, "piso\\ceramicas\\agrupadas\\agrupadas01.jpg"));
            group.add(new Texture(t++, "piso\\ceramicas\\agrupadas\\agrupadas14.jpg"));
            group.add(new Texture(t++, "piso\\ceramicas\\agrupadas\\agrupadas57.jpg"));
            group.add(new Texture(t++, "piso\\ceramicas\\agrupadas\\agrupadas72.jpg"));
            group.add(new Texture(t++, "piso\\ceramicas\\agrupadas\\white-tile-texture.jpg"));
            group.add(new Texture(t++, "piso\\ceramicas\\porcelanato\\porcelanato (34).jpg"));
            grupos.Add(group);

            //Paredes
            group = new Group("Paredes", g++, "pared.png");
            group.add(new Texture(t++, "paredes\\revoques\\liso_blanco.jpg"));
            group.add(new Texture(t++, "paredes\\revoques\\liso_rojo.jpg"));
            group.add(new Texture(t++, "paredes\\revoques\\revoques (4).jpg"));
            group.add(new Texture(t++, "paredes\\venecitas\\venecita_oscura.jpg"));
            grupos.Add(group);

            //Marmol
            group = new Group("Marmol", g++, "mesada.png");
            group.add(new Texture(t++, "marmoles\\silestone\\eros stellar.jpg"));
            group.add(new Texture(t++, "marmoles\\silestone\\carbono.jpg"));
            group.add(new Texture(t++, "marmoles\\silestone\\negro anubis.jpg"));
            group.add(new Texture(t++, "marmoles\\santamargherita\\mar-grigio piave.jpg"));
            grupos.Add(group);

        }

        /// <summary>
        /// Dialog que muestra todas las categorias de texturas disponibles que hay.
        /// Al hacer clic te abre un segundo Dialog con las texturas dentro de esa categoria.
        /// </summary>
        public void TextureGroupDlg(DXGui gui)
        {
            // Inicio un dialogo modalless
            gui.InitDialog(false, false);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            int x0 = 20;
            int y0 = 50;
            int dy = 500;
            int dx = W - 40;
            int r = 250;

            gui.InsertFrame("Seleccione la categoría", x0, y0, dx, dy, Color.FromArgb(192, 192, 192), frameBorder.redondeado);

            //int sdx = 400;
            //int sdy = 120;
            //gui.InsertKinectScrollButton(0, "scroll_left.png", x0 + 40, y0 + dy - sdy - 50, sdx, sdy);
            //gui.InsertKinectScrollButton(1, "scroll_right.png", x0 + 40 + sdx + 20, y0 + dy - sdy - 50, sdx, sdy);
            gui_item cancel_btn = gui.InsertKinectCircleButton(1, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
                    y0 + 20, gui.KINECT_BUTTON_SIZE_X);
            cancel_btn.scrolleable = false;      // fijo el boton de cancelar

            //Crear un boton por cada grupo de textura
            for (int i = 0; i < grupos.Count; i++)
            {
                Group g = grupos[i];
                gui.InsertKinectCircleButton(g.guiId, g.name, g.iconPath, x0 + 50 + i * (r + 20), y0 + 160, r);
            }

        }

        /// <summary>
        /// Dialog que muestra todas las texturas dentro de una categoria especifica.
        /// Al seleccionar una textura la aplica a todos los mesh que tenga esa categoria 
        /// </summary>
        public void TextureDlg(DXGui gui, int groupId)
        {
            // Inicio un dialogo modalless
            gui.InitDialog(false, false);

            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            int x0 = -20;
            int y0 = 50;
            int dy = 600;
            int dx = W + 40;
            int tdx = 250;
            int tdy = 200;

            gui.InsertFrame("Seleccione la textura", x0, y0, dx, dy, Color.FromArgb(192, 192, 192), frameBorder.sin_borde);
            int sdx = 500;
            int sdy = 120;
            gui.InsertKinectScrollButton(0, "scroll_left.png", x0 + 40, y0 + dy - sdy - 50, sdx, sdy);
            gui.InsertKinectScrollButton(1, "scroll_right.png", x0 + 40 + sdx + 20, y0 + dy - sdy - 50, sdx, sdy);
            gui_item cancel_btn = gui.InsertKinectCircleButton(1, "Cancel", "cancel.png", W - gui.KINECT_BUTTON_SIZE_X - 40,
                    y0 + dy - gui.KINECT_BUTTON_SIZE_X - 50, gui.KINECT_BUTTON_SIZE_X);
            cancel_btn.scrolleable = false;      // fijo el boton de cancelar

            //Buscar grupo con ese id de gui
            selectedGroup = null;
            foreach (Group g in grupos)
            {
                if (g.guiId == groupId)
                {
                    selectedGroup = g;
                    break;
                }
            }


            //Crear un boton por cada textura dentro de este grupo
            for (int i = 0; i < selectedGroup.textures.Count; i++)
            {
                Texture t = selectedGroup.textures[i];
                gui.InsertKinectTileButton(t.guiId, (i + 1).ToString(), t.path, x0 + 50 + i * (tdx + 50), y0 + 100, tdx, tdy);
            }


        }

        /// <summary>
        /// Aplicar cambio de textura a todos los mesh
        /// </summary>
        public void applyTextureChange(List<TgcMesh> meshes, FocusSet[] conjuntos, int guiID)
        {
            //Obtener nueva textura a aplicar
            Texture newTexture = selectedGroup.getTexture(guiID);

            //Recorrer todos los mesh y cambiarle la textura los que tengan alguna textura perteneciente a este grupo
            foreach (TgcMesh mesh in meshes)
            {
                this.applyTextureChangeToMesh(mesh, newTexture, selectedGroup);
            }

            //Hacer lo mismo con los conjuntos
            foreach (FocusSet conj in conjuntos)
            {
                foreach (TgcMesh mesh in conj.container.Childs)
                {
                    this.applyTextureChangeToMesh(mesh, newTexture, selectedGroup);
                }
            }
        }

        /// <summary>
        /// Cambiar textura en mesh
        /// </summary>
        private void applyTextureChangeToMesh(TgcMesh mesh, Texture texture, Group group)
        {
            if (mesh.RenderType == TgcMesh.MeshRenderType.DIFFUSE_MAP)
            {
                for (int i = 0; i < mesh.DiffuseMaps.Length; i++)
                {
                    if (mesh.DiffuseMaps[i] != null)
                    {
                        //Ver si la textura del mesh pertenece al grupo
                        string textAbsolutePath = mesh.DiffuseMaps[i].FilePath;
                        if (group.isFromThisGroup(textAbsolutePath))
                        {
                            //Cambiar textura
                            mesh.DiffuseMaps[i].dispose();
                            mesh.DiffuseMaps[i] = TgcTexture.createTexture(FocusParser.TEXTURE_FOLDER + texture.path);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Textura y su ID de botón de gui
        /// </summary>
        public class Texture
        {
            public string path;
            public int guiId;

            public Texture(int guiId, string path)
            {
                this.guiId = guiId;
                this.path = path;
            }
        }

        /// <summary>
        /// Agrupa varias texturas del mismo tipo. Ejemplo: Madera
        /// </summary>
        public class Group
        {
            public string name;
            public int guiId;
            public string iconPath;
            public List<Texture> textures;

            public Group(string name, int guiId, string iconPath)
            {
                this.name = name;
                this.guiId = guiId;
                this.iconPath = iconPath;
                this.textures = new List<Texture>();
            }

            public void add(Texture t)
            {
                textures.Add(t);
            }


            public Texture getTexture(int guiID)
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    if (textures[i].guiId == guiID)
                    {
                        return textures[i];
                    }
                }
                throw new Exception("Texture not found: " + guiID);
            }

            /// <summary>
            /// Indica si la textura pertenece al grupo
            /// </summary>
            public bool isFromThisGroup(string textAbsolutePath)
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    if (textAbsolutePath.Contains(textures[i].path))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;
using Examples.Focus;

namespace TgcViewer.Utils.Gui
{
    

    // item generico, con soporte de texto, bitmap, etc
    public class gui_item
    {
        public int item_id;
	    public int nro_item;
		public int flags;
		public Rectangle rc;
		public String text;
        public Texture textura;
        public TgcMesh mesh;
		public float ftime;
		public Color c_fondo;		        // color del fondo
		public Color c_font;		        // color de los textos
        public Color c_selected;	        // color seleccionado
        public bool seleccionable;          // indica que el item es seleccionable
        public bool auto_seleccionable;     // no hace falta presionarlo para que genere eventos
        public bool scrolleable;            // indica que el item escrollea junto con el gui
        public bool item3d;
        public bool disabled;
        public itemState state;
		public Microsoft.DirectX.Direct3D.Font font;

        // auxiliares
        public Point center;
        public int len;

		public gui_item()
        {
            Clean();
        }
        public void Clean()
        {
            item_id = -1;
            ftime = 0;
            state = itemState.normal;
            seleccionable = false;
            auto_seleccionable = false;
            scrolleable = true;
            text = "";
            rc = Rectangle.Empty;
            textura  = null;
            len = 0;
            c_fondo = DXGui.c_fondo;
            c_font = DXGui.c_font;
            c_selected = DXGui.c_selected;
            center = Point.Empty;
            item3d = false;
            disabled = false;
        }

        public gui_item(DXGui gui, String s, int x, int y, int dx = 0, int dy = 0,int id=-1)
        {
            Clean();
            item_id = id;
            nro_item = gui.cant_items;
            font = gui.font;
            text = s;
            rc = new Rectangle(x, y, dx, dy);
            center = new Point(x + dx / 2, y + dy / 2);
            len = s.Length;
        }

        public void Dispose()
        {
            if (textura != null)
                textura.Dispose();
        }


		// interface:
		public bool pt_inside(DXGui gui,Point p)
        {
            Vector2 []Q = new Vector2[2];
            Q[0] = new Vector2(rc.X,rc.Y);
            Q[1] = new Vector2(rc.X +rc.Width , rc.Y+ rc.Height);
            gui.Transform(Q, 2);
            Rectangle r = new Rectangle((int)Q[0].X, (int)Q[0].Y, (int)(Q[1].X - Q[0].X), (int)(Q[1].Y - Q[0].Y));
            return r.Contains(p);
        }

		public virtual bool ProcessMsg() 
        {
            return false;
        }


        public virtual void Render(DXGui gui)
        {
	        bool sel = gui.sel==nro_item && !disabled?true:false;
	        Color color = sel ? c_selected :c_font;
            if (rc.Width == 0 || rc.Height == 0)
            {
                // Ajusta el rectangulo para que adapte al texto a dibujar
                Rectangle tw = gui.font.MeasureString(gui.sprite, text, DrawTextFormat.NoClip | DrawTextFormat.Top, color);
                rc.Width = tw.Width + 20;
                rc.Height = tw.Height + 10;
                rc.X -= 10;
                rc.Y -= 5;
                // Recalcula el centro 
                center = new Point(rc.X + rc.Width / 2, rc.Y + rc.Height / 2);

            }

            if (textura!=null)
            {
                Vector3 pos = new Vector3(rc.X - 64, rc.Y - 8, 0);
                gui.sprite.Draw(textura, Rectangle.Empty, Vector3.Empty, pos, Color.FromArgb(gui.alpha, 255, 255, 255));
            }

            if (sel)
            {
                gui.RoundRect(rc.Left - 8, rc.Top - 6, rc.Right + 8, rc.Bottom + 6, 6, 3, 
                    Color.FromArgb(gui.alpha, DXGui.c_selected_frame), false);
                int dy = rc.Height / 2;

                gui.line.Width = 2f;
                gui.line.Begin();


                byte r0 = DXGui.c_grad_inf_0.R;
                byte g0 = DXGui.c_grad_inf_0.G;
                byte b0 = DXGui.c_grad_inf_0.B;
                byte r1 = DXGui.c_grad_inf_1.R;
                byte g1 = DXGui.c_grad_inf_1.G;
                byte b1 = DXGui.c_grad_inf_1.B;

                // Gradiente de abajo
                for (int i = 0; i < dy; ++i)
                {

                    Vector2[] pt = new Vector2[2];
                    pt[0].X = rc.X - 3;
                    pt[1].X = rc.X + rc.Width + 3;
                    pt[1].Y = pt[0].Y = rc.Y + rc.Height / 2 - i;
                    gui.Transform(pt, 2);
                    float t = (float)i / (float)dy;
                    byte r = (byte)(r0 * t + r1 * (1 - t));
                    byte g = (byte)(g0 * t + g1 * (1 - t));
                    byte b = (byte)(b0 * t + b1 * (1 - t));
                    gui.line.Draw(pt, Color.FromArgb(gui.alpha, r, g, b));
                }

                // Gradiente de arriba
                r0 = DXGui.c_grad_sup_0.R;
                g0 = DXGui.c_grad_sup_0.G;
                b0 = DXGui.c_grad_sup_0.B;
                r1 = DXGui.c_grad_sup_1.R;
                g1 = DXGui.c_grad_sup_1.G;
                b1 = DXGui.c_grad_sup_1.B;

                for (int i = 0; i < dy; ++i)
                {

                    Vector2[] pt = new Vector2[2];
                    pt[0].X = rc.X - 3;
                    pt[1].X = rc.X + rc.Width + 3;
                    pt[1].Y = pt[0].Y = rc.Y + rc.Height / 2 + i;
                    gui.Transform(pt, 2);
                    float t = (float)i / (float)dy;
                    byte r = (byte)(r0 * t + r1 * (1 - t));
                    byte g = (byte)(g0 * t + g1 * (1 - t));
                    byte b = (byte)(b0 * t + b1 * (1 - t));
                    gui.line.Draw(pt, Color.FromArgb(gui.alpha, r, g, b));
                }
                gui.line.End();
            }

	        // dibujo el texto pp dicho
            gui.font.DrawText( gui.sprite, text, rc, DrawTextFormat.NoClip |DrawTextFormat.VerticalCenter,
                disabled ? Color.FromArgb(gui.alpha,DXGui.c_item_disabled) : sel ? Color.FromArgb(gui.alpha,0, 32, 128) : c_font);
        }
    }

    // menu item
    public class gui_menu_item : gui_item
    {

        public gui_menu_item(DXGui gui, String s, int id, int x, int y, int dx = 0, int dy = 0, bool penabled=true) :
            base(gui, s, x, y, dx, dy,id)
        {
            disabled = !penabled;
            seleccionable = true;
        }
    }


    // standard button
    public class gui_button : gui_item
    {

        public gui_button(DXGui gui, String s, int id,int x, int y, int dx = 0, int dy = 0) :
            base(gui, s, x, y, dx, dy,id)
        {
            seleccionable = true;
        }

        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true: false;
            if (textura!=null)
            {
                Vector3 pos = new Vector3(rc.Left - 64, rc.Top - 8, 0);
                gui.sprite.Draw(textura, Rectangle.Empty, Vector3.Empty, pos, Color.FromArgb(gui.alpha, 255, 255, 255));
            }


            // recuadro del boton
            gui.RoundRect(rc.Left, rc.Top + 5, rc.Right, rc.Bottom - 5, 15, 3, DXGui.c_buttom_frame);

            if (sel)
                // boton seleccionado: lleno el interior
                gui.RoundRect(rc.Left, rc.Top + 5, rc.Right, rc.Bottom - 5, 10, 1,DXGui.c_buttom_selected, true);

            // Texto del boton
            Color color = sel ? DXGui.c_buttom_sel_text : DXGui.c_buttom_text;
            gui.font.DrawText(gui.sprite, text, rc, DrawTextFormat.Top | DrawTextFormat.Center, color);
        }
    }


    public class gui_color :  gui_item
    {
        public override void Render(DXGui gui)
        {
        }
    }


    public class gui_edit :gui_item
    {
        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true: false;
            bool foco = gui.foco == nro_item ? true: false;

            // recuadro del edit
	        gui.RoundRect(rc.Left,rc.Top,rc.Right,rc.Bottom,11,2,Color.FromArgb(80,220,20));

	        if(foco)
		        // tiene foco
		        gui.RoundRect(rc.Left,rc.Top,rc.Right,rc.Bottom,8,1,Color.FromArgb(255,255,255,255),true);

	        // Texto del edit
	        Color color = foco?Color.FromArgb( 0,0,0):Color.FromArgb(130,255,130);
	        gui.font.DrawText( gui.sprite, text, rc, DrawTextFormat.Top |DrawTextFormat.Left, color);

	        if(foco)
	        {
		        // si esta vacio, le agrego una I para que cuente bien el alto del caracter
                String p = text;
                if(p.Length==0)
                    p += "I";
		        Rectangle tw = gui.font.MeasureString(gui.sprite, p,DrawTextFormat.Top |DrawTextFormat.NoClip,color);
                Rectangle rc2 = new Rectangle(rc.Right+tw.Width,rc.Top,12,rc.Height);
		        // dibujo el cursor titilando
		        int cursor = (int)(gui.time*5);
		        if(cursor%2!=0)
		        {
			        gui.line.Width = 8;
			        Vector2 []pt = new Vector2[2];
			        pt[0].X = rc2.Left;
			        pt[1].X = rc2.Right;
			        pt[1].Y = pt[0].Y = rc2.Bottom;

			        gui.Transform(pt,2);
                    gui.line.Begin();
                    gui.line.Draw(pt, Color.FromArgb(0, 64, 0));
                    gui.line.End();
		        }
	        }		

        }

    }

    // Rectangular frame
    public class gui_frame :gui_item
    {
        public frameBorder borde;
        public gui_frame(DXGui gui, String s, int x, int y, int dx , int dy , Color color , 
                frameBorder tipo_borde=frameBorder.rectangular)  :
            base(gui, s, x, y, dx, dy)
        {
            c_fondo = color;
            borde = tipo_borde;
        }


        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true: false;

            switch (borde)
            {
                case frameBorder.sin_borde:
                    // dibujo solo interior
                    gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 1, Color.FromArgb(gui.alpha, c_fondo), true);
                    break;

                case frameBorder.redondeado:
                    // Interior
                    gui.RoundRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 30, 6, Color.FromArgb(gui.alpha, c_fondo),true);
                    // Contorno
                    gui.RoundRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 30, 6, Color.FromArgb(gui.alpha, DXGui.c_frame_border));
                    break;

                case frameBorder.solapa:
                    {
                        float r = 40;
                        Vector2 []pt = new Vector2 [10];
                        pt[0].X = rc.X;
                        pt[0].Y = rc.Y + rc.Height;
                        pt[1].X = rc.X;
                        pt[1].Y = rc.Y;
                        pt[2].X = rc.X + rc.Width - r;
                        pt[2].Y = rc.Y;
                        pt[3].X = rc.X + rc.Width;
                        pt[3].Y = rc.Y + r;
                        pt[4].X = rc.X + rc.Width;
                        pt[4].Y = rc.Y + rc.Height;
                        pt[5].X = rc.X;
                        pt[5].Y = rc.Y + rc.Height;
                        pt[6] = pt[0];

                        gui.DrawSolidPoly(pt, 7, Color.FromArgb(gui.alpha, c_fondo),false);
                        gui.DrawPoly(pt,5, 6,DXGui.c_frame_border);
                    }

                    break;

                case frameBorder.rectangular:
                default:

                    // interior
                    gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 1, Color.FromArgb(gui.alpha, c_fondo), true);
                    // contorno
                    gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 6, Color.FromArgb(gui.alpha, DXGui.c_frame_border));
                    break;

            }

	        // Texto del frame
            Rectangle rc2 = new Rectangle(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height);
		    rc2.Y += 30;
		    rc2.X += 30;
		    Color color = sel?c_selected:c_font;
		    gui.font.DrawText( gui.sprite, text, rc2, DrawTextFormat.NoClip | DrawTextFormat.Top, Color.FromArgb(gui.alpha,color));
        }
    }


    // Irregular frame
    public class gui_iframe : gui_item
    {

        public gui_iframe(DXGui gui, String s, int x, int y, int dx, int dy, Color color) :
            base(gui, s, x, y, dx, dy)
        {
            c_fondo = color;
        }


        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;

            float M_PI = (float)Math.PI;
            Vector2[] pt = new Vector2[255];
            float da = M_PI / 8;
            float alfa;

            float x0 = rc.Left;
            float x1 = rc.Right;
            float y0 = rc.Top;
            float y1 = rc.Bottom;
            float r = 10;
            int t = 0;
            float x = x0;
            float y = y0;
            for (alfa = 0; alfa < M_PI / 2; alfa += da)
            {
                pt[t].X = (float)(x - r * Math.Cos(alfa));
                pt[t].Y = (float)(y - r * Math.Sin(alfa));
                ++t;
            }
            pt[t].X = x;
            pt[t].Y = y - r;
            ++t;

            pt[t].X = (x1 + x0) / 2;
            pt[t].Y = y - r;
            ++t;
            pt[t].X = (x1 + x0) / 2 + 50;
            pt[t].Y = y + 20 - r;
            ++t;

            x = x1;
            y = y0 + 20;
            for (alfa = M_PI / 2; alfa < M_PI; alfa += da)
            {
                pt[t].X = (float)(x - r * Math.Cos(alfa));
                pt[t].Y = (float)(y - r * Math.Sin(alfa));
                ++t;
            }
            pt[t].X = x + r;
            pt[t].Y = y;
            ++t;


            x = x1;
            y = y1;
            for (alfa = 0; alfa < M_PI / 2; alfa += da)
            {
                pt[t].X = (float)(x + r * Math.Cos(alfa));
                pt[t].Y = (float)(y + r * Math.Sin(alfa));
                ++t;
            }
            pt[t].X = x;
            pt[t].Y = y + r;
            ++t;

            pt[t].X = x0 + 150;
            pt[t].Y = y + r;

            ++t;
            pt[t].X = x0 + 100;
            pt[t].Y = y - 20 + r;
            ++t;

            x = x0;
            y = y - 20;
            for (alfa = M_PI / 2; alfa < M_PI; alfa += da)
            {
                pt[t].X = (float)(x + r * Math.Cos(alfa));
                pt[t].Y = (float)(y + r * Math.Sin(alfa));
                ++t;
            }
            pt[t++] = pt[0];

            // interior
            gui.DrawSolidPoly(pt, t, c_fondo);

            // contorno
            gui.DrawPoly(pt, t, 6, DXGui.c_frame_border);

            // Texto del frame
            Rectangle rc2 = new Rectangle(rc.Top, rc.Left, rc.Width, rc.Height);
            rc2.Y += 15;
            rc2.X += 30;
            Color color = sel ? c_selected : c_font;
            gui.font.DrawText(gui.sprite, text, rc2, DrawTextFormat.NoClip | DrawTextFormat.Top, color);
        }
    }

    public class gui_rect : gui_item
    {
	    public int radio;
        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true : false;
            Color color = sel ? Color.FromArgb(gui.alpha, 255, 220, 220) : Color.FromArgb(gui.alpha,130, 255, 130);
            gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, radio, 2, color, false);
        }
    }


    
    public class gui_kinect_tile_button : gui_item
    {
        public int image_width;
        public int image_height;
        public float ox, oy, ex, ey, k;
        public bool sel;
        public DXGui gui;
        public bool border;

        public gui_kinect_tile_button(DXGui gui, String s, String imagen, int id, int x, int y, int dx,int dy,bool bscrolleable=true) :
            base(gui, s, x, y, dx, dy, id)
        {
            seleccionable = true;
            scrolleable = bscrolleable;
            border = true;
            // Cargo la imagen en el gui
            if ((textura = DXGui.cargar_textura(imagen, true)) != null)
            {
                // Aprovecho para calcular el tamaño de la imagen del boton
                SurfaceDescription desc = textura.GetLevelDescription(0);
                image_width = desc.Width;
                image_height = desc.Height;
            }
        }


        public virtual void InitRender(DXGui p_gui)
        {
            // inicializacion comun a todos los controles
            gui = p_gui;

            // estado del control
            sel = state == itemState.hover;

            // Calcula la escala pp dicha
            ex = gui.ex;
            ey = gui.ey;
            ox = gui.ox;
            oy = gui.oy;
            if (scrolleable)
            {
                // como este boton es un item scrolleable, tiene que aplicar tambien el origen sox,soy
                ox += gui.sox;
                oy += gui.soy;
            }

            // sobre escala por estar seleccionado
            k = 1;
            if (sel)
            {
                // aumento las escala
                k = 1 + (float)(0.5 * (gui.delay_sel0 - gui.delay_sel));

                // Le aplico una matriz de escalado adicional, solo sobre el TEXTO. 
                // El glyph tiene su propia matriz

                // Este kilombo es porque una cosa es la escala global que se aplica uniformemente en todo el gui
                // y esta centrada en el origen. 
                // Pero esta escala es local, del texto, que se aplica centra en centro del texto, luego de haberlo
                // escalado por la escala global. 
                gui.sprite.Transform = gui.sprite.Transform * Matrix.Transformation2D(new Vector2((center.X + ox) * ex, (center.Y + oy) * ey), 0, new Vector2(k, k),
                        new Vector2(0, 0), 0, new Vector2(0, 0));
            }
        }

        public virtual void RenderText()
        {
            // dibujo el texto pp dicho
            String buffer = text;
            Color color = sel ? Color.FromArgb(gui.alpha, c_selected ): Color.FromArgb(gui.alpha, c_font);
            Rectangle pos_texto = new Rectangle((int)ox + rc.Left, (int)oy + rc.Bottom + 15, rc.Width, 32);
            gui.font.DrawText(gui.sprite, buffer, pos_texto, DrawTextFormat.NoClip | DrawTextFormat.Top | DrawTextFormat.Center,
                        sel ? Color.FromArgb(gui.alpha, 0, 32, 128) : Color.FromArgb(gui.alpha, c_font));
        }

        public virtual void RenderFrame()
        {
            // Dibujo un rectangulo
            int x0 = (int)(rc.Left + ox);
            int x1 = (int)(rc.Right + ox);
            int y0 = (int)(rc.Top + oy);
            int y1 = (int)(rc.Bottom + oy);

            if (sel)
            {
                int dmx = (int)(rc.Width * (k - 1) * 0.5);
                int dmy = (int)(rc.Height * (k - 1) * 0.5);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 2, Color.FromArgb(gui.alpha, 0, 0, 0), true);
            }
            else
            if (state == itemState.pressed)
            {
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(gui.alpha, 32, 140, 55));
                float k2 = 1 + (float)(0.5 * gui.delay_press);
                int dmx = (int)(rc.Width * (k2 - 1) * 1.1);
                int dmy = (int)(rc.Height * (k2 - 1) * 1.1);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 8, Color.FromArgb(gui.alpha, 255, 0, 0));
            }
            else
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(gui.alpha, 32, 140, 55));
        }

        public virtual void RenderGlyph()
        {
            // dibujo el glyph 
            if (textura != null)
            {
                Vector3 pos = new Vector3(center.X * ex, center.Y * ey, 0);
                Vector3 c0 = new Vector3(image_width / 2, image_height / 2, 0);
                // Determino la escala para que entre justo
                Vector2 scale = new Vector2(k * ex * (float)rc.Width / (float)image_width, k * ey * (float)rc.Height / (float)image_height);
                Vector2 offset = new Vector2(ox * ex, oy * ey);
                gui.sprite.Transform = Matrix.Transformation2D(new Vector2(center.X * ex, center.Y * ey), 0, scale, new Vector2(0, 0), 0, offset) * gui.RTQ;
                gui.sprite.Draw(textura, c0, pos, Color.FromArgb(gui.alpha, 255, 255, 255).ToArgb());
            }
        }

        public override void Render(DXGui gui)
        {
            // Guardo la Matrix anterior
            Matrix matAnt = gui.sprite.Transform * Matrix.Identity;
            // Inicializo escalas, matrices, estados
            InitRender(gui);
            // Secuencia standard: texto + Frame + Glyph
            RenderText();
            if(border)
                RenderFrame();
            RenderGlyph();
            // Restauro la transformacion del sprite
            gui.sprite.Transform = matAnt;
        }
        
    }


    public class gui_kinect_scroll_button : gui_kinect_tile_button
    {
        public int tipo_scroll;

        public gui_kinect_scroll_button(DXGui gui, String imagen, int tscroll, int x, int y, int dx, int dy) :
            base(gui, "",imagen,DXGui.EVENT_FIRST_SCROLL+tscroll,x,y,dx,dy,false)
        {
            seleccionable = false;
            auto_seleccionable = true;
        }
    }

    public class gui_kinect_circle_button : gui_kinect_tile_button
    {
        public Color c_border = Color.FromArgb(0, 0, 0);
        public Color c_interior_sel = Color.FromArgb(30, 240, 40);

        public gui_kinect_circle_button(DXGui gui, String s, String imagen, int id, int x, int y, int r) :
            base(gui, s,imagen,id, x, y, r, r)
        {
        }

        public override void RenderFrame()
        {
            float tr = (float)(4 * (gui.delay_sel0 - gui.delay_sel));
            // circulo 
            int R = (int)(rc.Width / 2 * k);

            gui.DrawCircle(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R, 10, Color.FromArgb(gui.alpha,c_border));

            // relleno
            if (sel)
                gui.DrawDisc(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R - 10,
                    Color.FromArgb((byte)(255 * tr), c_interior_sel.R, c_interior_sel.G, c_interior_sel.B));
            
            else
            if (state == itemState.pressed)
            {
                gui.DrawDisc(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), R-10, Color.FromArgb(255, 255, 0, 0));
                int R2 = (int)(rc.Width / 2 + gui.delay_press * 100);
                gui.DrawCircle(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy),R2, 10, Color.FromArgb(255, 120, 120));
            }
        }

    }

    public class gui_mesh_button : gui_item
    {
        public float size;
        public gui_mesh_button(DXGui gui, String s, String fname, int id, int x, int y, int dx, int dy) :
            base(gui,s , x, y, dx, dy,id)
        {

            //TgcSceneLoader.TgcSceneLoader loader = new TgcSceneLoader.TgcSceneLoader();
            //TgcSceneLoader.TgcScene currentScene = loader.loadSceneFromFile(fname);
            // mesh = currentScene.Meshes[0];

            YParser yparser = new YParser();
            yparser.FromFile(fname);
            mesh = yparser.Mesh;

            mesh.AutoTransformEnable = false;
            size = mesh.BoundingBox.calculateSize().Length();
            seleccionable = true;
            item3d = true;

        }

            

        public override void Render(DXGui gui)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            Vector3 LA = gui.camera.LookAt*1;
            Vector3 LF = gui.camera.LookFrom*1;

            bool sel = state == itemState.hover;
            float tr = (float)(4 * (gui.delay_sel0 - gui.delay_sel));
            float k = 1;

            float ox = gui.ox;
            float oy = gui.oy;
            float ex = gui.ex;
            float ey = gui.ey;

            if (sel)
                // aumento las escala
                k = 1 + (gui.delay_sel0 - gui.delay_sel)*1.0f;

            // Determino el rectangulo
            float xm = (rc.Left + rc.Right) * 0.5f;
            float ym = (rc.Top + rc.Bottom) * 0.5f;
            float rx = rc.Width * k * 0.5f;
            float ry = rc.Height * k * 0.5f;

            float x0 = xm - rx + ox;
            float y0 = ym - ry + oy;
            // El dx no deja poner viewport con origen negativo (pero si me puedo pasar por la derecha o por abajo)
            if (x0 < 0)
                x0 = 0;
            if (y0 < 0)
                y0 = 0;
            float x1 = x0 + 2*rx;
            float y1 = y0 + 2*ry;

            // Y roto la vista
            float an = sel ? ftime*1.5f : 0;
            Vector3 viewDir = new Vector3((float)Math.Sin(an), 0.3f, (float)Math.Cos(an));
            viewDir.Normalize();
            gui.camera.LookAt = mesh.Position;
            float dist = sel ? size * 1.2f : size * 1.5f;
            gui.camera.LookFrom = mesh.Position + viewDir * dist;
            gui.camera.updateCamera();
            gui.camera.updateViewMatrix(d3dDevice);

            Viewport ant_viewport = d3dDevice.Viewport;
            Viewport btn_viewport = new Viewport();
            btn_viewport.X = (int)(x0 * ex);
            btn_viewport.Y = (int)(y0 * ey);
            btn_viewport.Width = (int)((x1-x0)* ex);
            btn_viewport.Height = (int)((y1-y0)* ey);
            d3dDevice.Viewport = btn_viewport;
            d3dDevice.EndScene();
            d3dDevice.Clear(ClearFlags.ZBuffer | ClearFlags.Target, sel ? Color.FromArgb(240, 250, 240) : Color.FromArgb(192, 192, 192), 1.0f, 0);
            d3dDevice.BeginScene();
            d3dDevice.SetRenderState(RenderStates.ZEnable, true);
            mesh.render();

            gui.camera.LookAt = LA*1;
            gui.camera.LookFrom = LF*1;

            d3dDevice.Viewport = ant_viewport;
            d3dDevice.SetRenderState(RenderStates.ZEnable, false);

            // Dibujo un rectangulo
            gui.DrawRect((int)x0, (int)y0, (int)x1, (int)y1, sel?5:2, Color.FromArgb(gui.alpha, 32, 140, 55));

        }

        public void Dispose()
        {
            if (mesh != null)
                mesh.dispose();
            base.Dispose();
        }

    }
}

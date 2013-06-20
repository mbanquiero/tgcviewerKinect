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
	        bool sel = gui.sel==nro_item?true:false;
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
                gui.sprite.Draw(textura, Rectangle.Empty, Vector3.Empty, pos, Color.FromArgb(255, 255, 255, 255));
            }

            if (sel)
            {
                gui.RoundRect(rc.Left - 1, rc.Top - 3, rc.Right + 1, rc.Bottom + 3, 6, 2, DXGui.c_selected_frame, false);
                int dy = rc.Height / 2;

                gui.line.Width = 1f;
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
                    gui.line.Draw(pt, Color.FromArgb(255, r, g, b));
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
                    gui.line.Draw(pt, Color.FromArgb(255, r, g, b));
                }
                gui.line.End();
            }

	        // dibujo el texto pp dicho
            gui.font.DrawText( gui.sprite, text, rc, DrawTextFormat.NoClip |DrawTextFormat.VerticalCenter, 
                        sel?Color.FromArgb(0,32,128):c_font);
        }
    }

    // menu item
    public class gui_menu_item : gui_item
    {

        public gui_menu_item(DXGui gui, String s, int id, int x, int y, int dx = 0, int dy = 0) :
            base(gui, s, x, y, dx, dy,id)
        {
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
                gui.sprite.Draw(textura, Rectangle.Empty, Vector3.Empty, pos, Color.FromArgb(255,255,255,255));
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

        public gui_frame(DXGui gui, String s, int x, int y, int dx , int dy , Color color )  :
            base(gui, s, x, y, dx, dy)
        {
            c_fondo = color;
        }


        public override void Render(DXGui gui)
        {
            bool sel = gui.sel == nro_item ? true: false;

	        // interior
		    gui.DrawRect(rc.X,rc.Y,rc.X+rc.Width , rc.Y + rc.Height,1,c_fondo,true);

	        // contorno
            gui.DrawRect(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height, 6, DXGui.c_frame_border);

	        // Texto del frame
            Rectangle rc2 = new Rectangle(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height);
		    rc2.Y += 30;
		    rc2.X += 30;
		    Color color = sel?c_selected:c_font;
		    gui.font.DrawText( gui.sprite, text, rc2, DrawTextFormat.NoClip | DrawTextFormat.Top, color);
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
            Color color = sel ? Color.FromArgb(255, 220, 220) : Color.FromArgb(130, 255, 130);
            gui.RoundRect(rc.Left, rc.Top, rc.Right, rc.Bottom, radio, 2, color, false);
        }
    }


    public class gui_kinect_circle_button : gui_item
    {
        public int image_width;
        public int image_height;
        public Color c_border = Color.FromArgb(0, 0, 0);
        public Color c_interior_sel = Color.FromArgb(30, 240, 40);     



        public gui_kinect_circle_button(DXGui gui, String s, String imagen,int id,int x, int y, int r) :
            base(gui, s, x, y, r, r,id)
        {
            seleccionable = true;
            // Cargo la imagen en el gui
            if ((textura = DXGui.cargar_textura(imagen, true)) != null)
            {
                // Aprovecho para calcular el tamaño de la imagen del boton
                SurfaceDescription desc = textura.GetLevelDescription(0);
                image_width = desc.Width;
                image_height = desc.Height;
            }
        }

        public override void Render(DXGui gui)
        {
            bool sel = state==itemState.hover;
            float tr = (float)(4 * (gui.delay_sel0 - gui.delay_sel));
            Color color = sel ? c_selected : c_font;

            Matrix matAnt = gui.sprite.Transform * Matrix.Identity;
            float ex = gui.ex;
            float ey = gui.ey;
            // como este boton es un item scrolleable, tiene que aplicar tambien el origen sox,soy
            float ox = gui.ox + gui.sox;
            float oy = gui.oy + gui.soy;

            if (sel)
            {
                float k = 1 + (float)(0.5 * (gui.delay_sel0 - gui.delay_sel));
                ex *= k;
                ey *= k;
            }

            Vector2 scale = new Vector2(ex, ey);
            Vector2 offset = new Vector2(ox, oy);

            gui.sprite.Transform = Matrix.Transformation2D(new Vector2(center.X, center.Y), 0, scale, new Vector2(0, 0), 0, offset) * gui.RTQ;

            // dibujo el texto pp dicho
            String buffer = text;
            Rectangle pos_texto = new Rectangle(rc.Left, rc.Bottom+15, rc.Width, 32);
            gui.font.DrawText(gui.sprite, buffer, pos_texto, DrawTextFormat.NoClip | DrawTextFormat.Top| DrawTextFormat.Center,
                        sel ? c_selected : c_font);

            // circulo 
            gui.DrawCircle(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2+ oy), (int)(rc.Width / 2*ex), (int)(10*ex), c_border);

            // relleno
            if (sel)
                gui.DrawDisc(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), (int)((rc.Width / 2 - 10) * ex),
                    Color.FromArgb((byte)(255 * tr), c_interior_sel.R, c_interior_sel.G, c_interior_sel.B));
            else
            if (state == itemState.pressed)
            {
                 gui.DrawDisc(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), (int)((rc.Width / 2 - 10) * ex), Color.FromArgb(255, 255, 0, 0));
                 float radio = (rc.Width / 2 + gui.delay_press * 100) * ex;
                 gui.DrawCircle(new Vector2(rc.X + rc.Width / 2 + ox, rc.Y + rc.Height / 2 + oy), (int)radio, (int)(10 * ex), Color.FromArgb(255, 120, 120));
             }


            // dibujo el glyph 
            if (textura!=null)
            {
                Vector3 pos = new Vector3(center.X, center.Y, 0);
                Vector3 c0 = new Vector3(image_width / 2, image_height / 2, 0);
                gui.sprite.Draw(textura, Rectangle.Empty, c0, pos, Color.FromArgb(255, 255, 255, 255));
            }
            // Restauro la transformacion del sprite
            gui.sprite.Transform = matAnt;
        }
    }

    public class gui_kinect_tile_button : gui_item
    {
        public int image_width;
        public int image_height;

        public gui_kinect_tile_button(DXGui gui, String s, String imagen, int id, int x, int y, int dx,int dy,bool bscrolleable=true) :
            base(gui, s, x, y, dx, dy, id)
        {
            seleccionable = true;
            scrolleable = bscrolleable;
            // Cargo la imagen en el gui
            if ((textura = DXGui.cargar_textura(imagen, true)) != null)
            {
                // Aprovecho para calcular el tamaño de la imagen del boton
                SurfaceDescription desc = textura.GetLevelDescription(0);
                image_width = desc.Width;
                image_height = desc.Height;
            }
        }

        public override void Render(DXGui gui)
        {
            bool sel = state == itemState.hover;
            float tr = (float)(4 * (gui.delay_sel0 - gui.delay_sel));
            Color color = sel ? c_selected : c_font;

            Matrix matAnt = gui.sprite.Transform * Matrix.Identity;
            float ex = gui.ex;
            float ey = gui.ey;
            float ox = gui.ox;
            float oy = gui.oy;
            if (scrolleable)
            {
                // como este boton es un item scrolleable, tiene que aplicar tambien el origen sox,soy
                ox += gui.sox;
                oy += gui.soy;
            }
            float k = 1;
            if (sel)
            {
                // aumento las escala
                k = 1 + (float)(0.5 * (gui.delay_sel0 - gui.delay_sel));
                ex *= k;
                ey *= k;
            }

            Vector2 scale = new Vector2(ex, ey);
            Vector2 offset = new Vector2(ox, oy);

            gui.sprite.Transform = Matrix.Transformation2D(new Vector2(center.X, center.Y), 0, scale, new Vector2(0, 0), 0, offset) * gui.RTQ;

            // dibujo el texto pp dicho
            String buffer = text;
            Rectangle pos_texto = new Rectangle(rc.Left, rc.Bottom + 15, rc.Width, 32);
            gui.font.DrawText(gui.sprite, buffer, pos_texto, DrawTextFormat.NoClip | DrawTextFormat.Top | DrawTextFormat.Center,
                        sel ? Color.FromArgb(0, 32, 128) : c_font);

            // Dibujo un rectangulo
            int x0 = (int)(rc.Left+ox);
            int x1 = (int)(rc.Right+ox);
            int y0 = (int)(rc.Top+oy);
            int y1 = (int)(rc.Bottom+oy);

            if (sel)
            {
                int dmx = (int)(rc.Width * (k - 1) *0.5);
                int dmy = (int)(rc.Height * (k - 1) * 0.5);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 2, Color.FromArgb(0, 0, 0),true);
            }
            else
            if (state == itemState.pressed)
            {
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(32, 140, 55));
                float k2 = 1 + (float)(0.5 * gui.delay_press);
                int dmx = (int)(rc.Width * (k2 - 1) * 1.1);
                int dmy = (int)(rc.Height * (k2 - 1) * 1.1);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 8, Color.FromArgb(255, 0, 0));
            }
            else
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(32, 140, 55));

            // dibujo el glyph 
            if (textura != null)
            {
                Vector3 pos = new Vector3(center.X, center.Y, 0);
                Vector3 c0 = new Vector3(image_width / 2, image_height/ 2, 0);
                
                // Determino la escala para que entre justo
                Vector2 scale2 = new Vector2(ex * (float)rc.Width / (float)image_width, ex * (float)rc.Height / (float)image_height);
                gui.sprite.Transform = Matrix.Transformation2D(new Vector2(center.X, center.Y), 0, scale2, new Vector2(0, 0), 0, offset) * gui.RTQ;
                gui.sprite.Draw(textura, c0, pos,Color.FromArgb(255, 255, 255, 255).ToArgb());
            }
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

        public override void Render(DXGui gui)
        {
            bool sel = state == itemState.hover;
            float tr = (float)(4 * (gui.delay_sel0 - gui.delay_sel));
            Color color = sel ? c_selected : c_font;

            Matrix matAnt = gui.sprite.Transform * Matrix.Identity;
            float ex = gui.ex;
            float ey = gui.ey;
            float ox = gui.ox;
            float oy = gui.oy;
            float k = 1;
            if (sel)
            {
                // aumento las escala (muy levemente)
                k = 1 + (float)(0.1 * (gui.delay_sel0 - gui.delay_sel));
                ex *= k;
                ey *= k;
            }

            Vector2 scale = new Vector2(ex, ey);
            Vector2 offset = new Vector2(ox, oy);

            gui.sprite.Transform = Matrix.Transformation2D(new Vector2(center.X, center.Y), 0, scale, new Vector2(0, 0), 0, offset) * gui.RTQ;

            // Dibujo un rectangulo
            int x0 = (int)(rc.Left + ox);
            int x1 = (int)(rc.Right + ox);
            int y0 = (int)(rc.Top + oy);
            int y1 = (int)(rc.Bottom + oy);

            if (sel || state == itemState.pressed)
            {
                int dmx = (int)(rc.Width * (k - 1) * 0.5);
                int dmy = (int)(rc.Height * (k - 1) * 0.5);
                gui.RoundRect(x0 - dmx, y0 - dmy, x1 + dmx, y1 + dmy, 4, 2, Color.FromArgb(0, 0, 0));
            }
            else
                gui.RoundRect(x0, y0, x1, y1, 4, 2, Color.FromArgb(32, 140, 55));

            // dibujo el glyph 
            if (textura != null)
            {
                Vector3 pos = new Vector3(center.X, center.Y, 0);
                Vector3 c0 = new Vector3(image_width / 2, image_height / 2, 0);

                // Determino la escala para que entre justo
                Vector2 scale2 = new Vector2(ex * (float)rc.Width / (float)image_width, ex * (float)rc.Height / (float)image_height);
                gui.sprite.Transform = Matrix.Transformation2D(new Vector2(center.X, center.Y), 0, scale2, new Vector2(0, 0), 0, offset) * gui.RTQ;
                gui.sprite.Draw(textura, c0, pos, Color.FromArgb(255, 255, 255, 255).ToArgb());
            }
            // Restauro la transformacion del sprite
            gui.sprite.Transform = matAnt;
        }
    }


    public class gui_mesh_button : gui_item
    {
        public float size;
        public gui_mesh_button(DXGui gui, String s,String fname, int id, int x, int y, int dx, int dy) :
            base(gui,s , x, y, dx, dy,id)
        {
            //TgcSceneLoader.TgcSceneLoader loader = new TgcSceneLoader.TgcSceneLoader();
            //TgcSceneLoader.TgcScene currentScene = loader.loadSceneFromFile(fname);
            // mesh = currentScene.Meshes[0];

            YParser yparser = new YParser();
            yparser.FromFile(fname);
            mesh = yparser.Mesh;

            mesh.AutoTransformEnable = false;
            size = mesh.BoundingBox.calculateSize().Length() * 3;
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
            if (sel)
                // aumento las escala
                k = 1 / (1 + (float)(3 * (gui.delay_sel0 - gui.delay_sel)));

            Vector3 viewDir = new Vector3((float)Math.Sin(ftime),0,(float)Math.Cos(ftime));
            gui.camera.LookAt = mesh.Position;
            gui.camera.LookFrom = mesh.Position + viewDir*(size*k);
            gui.camera.updateCamera();
            gui.camera.updateViewMatrix(d3dDevice);

            Viewport ant_viewport = d3dDevice.Viewport;
            Viewport btn_viewport = new Viewport();
            btn_viewport.X = rc.X;
            btn_viewport.Y = rc.Y;
            btn_viewport.Width = rc.Width;
            btn_viewport.Height = rc.Height;
            d3dDevice.Viewport = btn_viewport;
            d3dDevice.EndScene();
            d3dDevice.Clear(ClearFlags.ZBuffer, 0, 1.0f, 0);
            d3dDevice.BeginScene();
            d3dDevice.SetRenderState(RenderStates.ZEnable, true);
            mesh.render();

            gui.camera.LookAt = LA*1;
            gui.camera.LookFrom = LF*1;

            d3dDevice.Viewport = ant_viewport;
            d3dDevice.SetRenderState(RenderStates.ZEnable, false);
        }

        public void Dispose()
        {
            if (mesh != null)
                mesh.dispose();
            base.Dispose();
        }

    }
}

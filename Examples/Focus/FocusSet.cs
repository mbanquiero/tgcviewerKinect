using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using TgcViewer;

namespace Examples.Focus
{
    public class FocusSet
    {
        public static readonly int TRASLACION = 0;
        public static readonly int ROTACION_Z = 1;
        public static readonly int ROTACION_Y = 2;
        public static readonly int ROTACION_X = 3;
        public static readonly int PLEGABLE_A = 4;
        public static readonly int PLEGABLE_B = 5;
        public static readonly int QUIETO = -1;

        public static readonly Vector3 NULL_PAR = new Vector3(-1, -1, -1);

        public int Tipo;
        public Vector3 Vector;
        public float Max;
        public Vector3 Offset;
        public Vector3 Dir;
        public Vector3 Normal;

        public MeshContainer container;

        public float Apertura;

        Vector3 sposition;
        Vector3 srotate;
        Vector3 sscale;

        Vector3 eposition;
        Vector3 erotate;
        Vector3 escale;

        //interpolator variables
        bool playing;
        bool loop = false;
        float deltaAcum;
        float AnimDur;

        public FocusSet()
        {
            container = new MeshContainer();
            container.Enabled = true;
            this.Offset = new Vector3(0, 0, 0);
            this.Normal = new Vector3(0, 0, -1);
            this.Dir = new Vector3(1, 0, 0);
            this.Tipo = QUIETO;
            this.Max = 0;
            this.Vector = new Vector3(0, 0, 0);
            this.Apertura = 0;
            this.AnimDur = 2;
        }

        public void animate()
		{
            if (playing)
                return;
			if(Apertura < 1)
				Apertura += 1.0f;
			else
				Apertura = 0;
			
			if(Apertura > 1)
				Apertura = 1;
			
			if(Apertura < 0)
				Apertura = 0;
			
			play();
		}
		
		public void play() 
		{
            Vector3 absVector = new Vector3(Vector.X * Dir.X + Vector.Z * Normal.X, Vector.Y, Vector.X * Dir.Z + Vector.Z * Normal.Z);
			
			if(Tipo == TRASLACION)
			{
				PlayAnim(absVector*(Apertura*Max), NULL_PAR, NULL_PAR);
			}
			
			if(Tipo == ROTACION_Z)
			{
				//if(pivotPoint.lengthSquared == 0)
					container.Pivot = Offset + absVector;

                    PlayAnim(NULL_PAR, new Vector3(0, Apertura * Max * (float)Math.PI / 180.0f, 0), NULL_PAR);
			}
			
			if(Tipo == ROTACION_Y)
			{
				//puerta basculante
				//if(pivotPoint.lengthSquared == 0)
				container.Pivot = Offset + absVector;
                float ang_x = Max * Dir.X;
                float ang_z = Max * Dir.Z;

                PlayAnim(NULL_PAR, new Vector3(Apertura * ang_x * (float)Math.PI / 180.0f, 0, Apertura * ang_z * (float)Math.PI / 180.0f), NULL_PAR);
			}
		}

        public void Render()
        {
            Update();
            container.render();
        }

        public void PlayAnim(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            sposition = container.Position;
            srotate = container.Rotation;
            sscale = container.Scale;

            eposition = pos == NULL_PAR ? sposition : pos;
            erotate = rot == NULL_PAR ? srotate : rot;
            escale = scale == NULL_PAR ? sscale : scale;
            playing = true;

        }

        public void Update()
        {
            if (!playing)
                return;

            float deltaTime = GuiController.Instance.ElapsedTime;
            deltaAcum += deltaTime;

            float deltaMove = deltaAcum / AnimDur;

            Vector3 pos = sposition * (1 - deltaMove) + eposition * deltaMove;
            Vector3 rot = srotate * (1 - deltaMove) + erotate * deltaMove;
            Vector3 scale = sscale * (1 - deltaMove) + escale * deltaMove;

            if (deltaMove >= 1)
            {
                if (deltaMove >= 2 || Apertura == 0)
                {
                    playing = loop;
                    deltaAcum = 0;
                    if(Apertura == 1)
                        animate();
                }

                pos = eposition;
                rot = erotate;
                scale = sscale;
                
            }

            container.Position = pos;
            container.Rotation = rot;
            container.Scale = scale;
        }

        public void dispose()
        {
            container.dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System.Net;
using TgcViewer.Utils.Gui;

namespace Examples.Focus
{
    class Face
    {
	    // flags de espejar
	    public const int OBJ_ESPEJAR_X = 1;
	    public const int OBJ_ESPEJAR_Y = 2;
	    public const int OBJ_ESPEJAR_Z = 4;
    	
	    // opciones del material (para mallas)
	    public const int MAT_PROPIO = 1; // usa los valores del objeto
	    public const int TEXTURA_PROPIA = 2; // usa la textura asociada al objeto
	    public const int COLOR_PROPIO = 4; // usa el color rgb del objeto
	    public const int SIN_TEXTURA = 16; // solo el color liso
	    public const int AJUSTAR_TEXTURA = 32; // ajusta el mapping de la textura a la extension del objeto
	    public const int CARA_INVERSA = 64; // la textura o el color estan del otro lado de la cara
	    public const int ROTAR_TEXTURA = 128; // contraveta
	    public const int TEXTURA_MAP_UV = 256; // Transformar uv, con t0 y angulo
	    public const int TEXTURA_UNIFORME = 512; // ajusta la textura segun la pos. ABSOLUTA del objeto en 3d util para una pared, que esta compuesta de varios objetos.
	    public const int ESPEJO_PLANO = 1024; // Implementa la reflexion como un stencil mirror (y no como un Environment map usual)


        public int Id;
        public int Borde;
        public int Tipo;
        public int CantLayers;
	    public Matrix MatWorld;
	    public float BmpK;
	    public int TexturaRotada;
	    public int FlagEspejar;
	    public int FlagMaterial;
	    public int NroMesh = -1;
	    public int NroTextura = -1;
	    public int NroConjunto = -1;

        public float kd;
        public float ks;
        public float kr;
        public float kt;
    	
	    public Layer [] Layers;
    	
    	
	    public Face(){}
    }

    class Layer
    {
        public int NroLayer;
        public int NroTextura = -1;
	    public Material Material;
    	
	    public Layer(){}
    }

    public class FocusParser
    {
		private int _version;
		private BinaryReader _byteData;
		private TgcSceneLoader.DiffuseMapVertex [] _vertices;
		private uint _color;
		private int _cantTextures;
		private int _cantMeshes;
		private int _cantFaces;
		private string [] _texturesId;
		private string [] _meshesId;
        private TgcMesh [] _meshes;
		private Face [] _faces;
		private TgcTexture [] _textures;

        public List<TgcMesh> Escene = new List<TgcMesh>();
        public FocusSet[] _focusSets;

        public static string TEXTURE_FOLDER = "c:\\lepton\\leptonpack\\texturas\\";
        public static string MESH_FOLDER = "c:\\texturasy\\";

        public static string WEB_TEXTURE_FOLDER = "http://lepton.com.ar/download/armarius/texturas/";
        public static string WEB_MESH_FOLDER = "http://lepton.com.ar/download/armarius/texturasy/";
        public static readonly TgcTexture NULL_TEXTURE = new TgcTexture("", "", null, false);

        public gui_progress_bar progress_bar;


		/**
		 * Loads a Viewer.dat Lepton File.
		 */
		public FocusParser()
		{
		    
		}
		
		public void FromFile(string path)
		{
            var fp = new FileStream(path, FileMode.Open, FileAccess.Read);
            _byteData = new BinaryReader(fp);
			
			ParseTextures();
			ParseMeshes();
		    ParseFaces();
			ParseSets();
			
            _byteData.Close();
            fp.Close();

            DownloadAssets();
		    LoadTextures();
            LoadMeshes();
            Build();
		}

        

        protected void ParseTextures()
		{
			//primero parseo el Header
            string head = ReadString(10); // aca tiene que decir LEPTONVIEW
			_version = _byteData.ReadInt32(); // version;
			
			//Texturas
			//cantidad de texturas
			_cantTextures = _byteData.ReadInt32();
            _texturesId = new string[_cantTextures];

            
			for(int i = 0 ; i < _cantTextures; i++)
			{

                //Texture Path
				
				//saco el fullpath
                string file = ReadString(260).ToLower();
				int bmp_k = _byteData.ReadInt32();
				
				file = file.Substring(file.LastIndexOf("texturas") + 9);
				
				_texturesId[i]="";
				
				string ext = Path.GetExtension(file).ToLower();

                if (ext == ".bmp")
                    file = Path.ChangeExtension(file, ".jpg");

				if(ext != ".msh" && ext != ".dxf" && ext != ".x")
				{
					_texturesId[i]= TEXTURE_FOLDER + file;
				}
				
			}
			
			//Fin Texturas
		}

        protected void ParseMeshes()
		{
			//Meshes
			//cantidad de meshes
			_cantMeshes = _byteData.ReadInt32();
            _meshesId = new string[_cantMeshes];


			for (int i = 0; i < _cantMeshes; i++)
			{

				//Meshes Path

                string file = ReadString(260);
				
				file = Path.ChangeExtension(file, ".y");
				//saco el fullpath
				file = file.Substring(file.LastIndexOf("texturas") + 9);
				file = MESH_FOLDER + file;
				_meshesId[i] = file;
			}
			//Fin Meshes
		}

        protected void ParseFaces()
		{
			//Faces
			// Cantidad de Faces
			
			_cantFaces = _byteData.ReadInt32();
            _faces = new Face[_cantFaces];
            _vertices = new TgcSceneLoader.DiffuseMapVertex[_cantFaces * 4];

			for (int i = 0; i < _cantFaces; i++)
			{

				var face = new Face();
				
				//tipo Face, Triangulo(3) o Rectangulo(1)
				face.Tipo = _byteData.ReadByte();
				
				//3 byte de relleno (alineacion)
				_byteData.ReadBytes(3);
				
				for (int j = 0; j < 4; j++)
				{
					//Vertices
                    var v = new TgcSceneLoader.DiffuseMapVertex();
					
					//Posicion
					v.Position = ParseVector3();
					//Normal
					v.Normal = ParseVector3();
					//color					
					_color = _byteData.ReadUInt32();
                    v.Color = (int)0xFFFFFF;
					//UV
				    v.Tu = _byteData.ReadSingle();
				    v.Tv = _byteData.ReadSingle();
				    _vertices[i*4 + j] = v;
				}
				
				//id
				face.Id = _byteData.ReadInt32();
				
				// Borde
				face.Borde = _byteData.ReadByte();
				
				//3 byte de relleno (alineacion)
				_byteData.ReadBytes(3);
				
				//nro_mesh, -1 si no es mesh
				face.NroMesh = _byteData.ReadInt32();
				
				//nro de textura
				int nroTextura = _byteData.ReadByte();
				
				//3 byte de relleno (alineacion)
				_byteData.ReadBytes(3);
				
				// parametros de iluminacion
				face.kd = _byteData.ReadSingle();
				face.ks = _byteData.ReadSingle();
				face.kr = _byteData.ReadSingle();
                face.kt = _byteData.ReadSingle();
				
				int nroConjunto = _byteData.ReadInt32();
				if(nroConjunto == 65535)
					nroConjunto = -1;
				
				face.NroConjunto = nroConjunto;
				
				if(face.NroMesh != -1)
				{
					//es un mesh
					//Cant Layers
					face.CantLayers = _byteData.ReadByte();
					
					//3 byte de relleno (alineacion)
					_byteData.ReadBytes(3);
					
					//WordMatrix
					face.MatWorld = ParseMatrix();
					
					face.BmpK = _byteData.ReadSingle();
					face.TexturaRotada = _byteData.ReadByte();
					face.FlagEspejar = _byteData.ReadByte();
					
					//2 byte de relleno (alineacion)
					_byteData.ReadBytes(2);
					
					face.FlagMaterial = _byteData.ReadInt32();
					
					//layers
                    face.Layers = new Layer[face.CantLayers];
					for(int j = 0; j < face.CantLayers; j++)
					{
						var layer = new Layer();
						
						//nro layer
						layer.NroLayer = _byteData.ReadByte();
						
						//3 byte de relleno (alineacion)
						_byteData.ReadBytes(3);
                        
                        //material
                        Material mat = layer.Material;
                        mat.DiffuseColor = new ColorValue(_byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle());
                        mat.AmbientColor = new ColorValue(_byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle());
                        mat.SpecularColor = new ColorValue(_byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle());
                        mat.EmissiveColor = new ColorValue(_byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle(), _byteData.ReadSingle());
                        mat.SpecularSharpness = _byteData.ReadSingle();
						
						//Coefcientes luz
						float transparencyLevel = _byteData.ReadSingle();
						float SpecularLevel = _byteData.ReadSingle();
						
						//textura Propia
						int textura_propia = _byteData.ReadByte();
						
						//Nro Textura
						int lNroTextura = _byteData.ReadByte();
						
						//2 byte de relleno (alineacion)
						_byteData.ReadBytes(2);
						
						//if(textura_propia == 0) //TODO revisar
						//	mat = null;

                        layer.NroTextura = textura_propia > 0 && lNroTextura != 255 ? lNroTextura : -1;
						layer.Material = mat;

                        if (layer.NroLayer >= face.Layers.Length)
                            throw new Exception("Nro de layer mayor a la cantidad de layers.");
                        //face.Layers[layer.NroLayer] = layer;
                        face.Layers[layer.NroLayer] = layer;
					}
					
					//Fin mesh
				}

				//Fin Face
				face.NroTextura = nroTextura;
				_faces[i] = face;
			}
		}

        protected Vector3 ParseVector3()
		{
			float x = _byteData.ReadSingle();
			float z = _byteData.ReadSingle();
			float y = _byteData.ReadSingle();

            return new Vector3(x, y, -z);
		}
		
		protected Matrix ParseMatrix()
        {
		    Matrix mat = new Matrix();
			float [,] rows = new float[4,4];
			
			for(int i = 0 ; i < 4 ; i++)
			{
				for(int j = 0 ; j < 4 ; j++)
				{
					rows[i,j] = _byteData.ReadSingle();
				}
			}
			
			mat.M11 = rows[0,0];
            mat.M12 = rows[0,2];
            mat.M13 = -rows[0,1];
            mat.M14 = rows[0,3];

            mat.M21 = rows[2,0];
            mat.M22 = rows[2,2];
            mat.M23 = -rows[2,1];
            mat.M24 = rows[1,3];

            mat.M31 = -rows[1,0];
            mat.M32 = -rows[1,2];
            mat.M33 = rows[1,1];
            mat.M34 = rows[2,3];

            mat.M41 = rows[3,0];
            mat.M42 = rows[3,2];
            mat.M43 = -rows[3,1];
            mat.M44 = rows[3,3];
            
            return mat;			
		}

		protected void ParseSets()
		{
			//leo la cantidad de conjuntos
			int cantSets = _byteData.ReadInt32();
            _focusSets = new FocusSet[cantSets];
			for(int i = 0; i < cantSets; i++)
			{
				FocusSet fs = new FocusSet();
                fs.Tipo = _byteData.ReadByte();
				
				//3 byte de relleno (alineacion)
				_byteData.ReadBytes(3);
				
				float vx = _byteData.ReadSingle();
				float vy = _byteData.ReadSingle();
				float vz = _byteData.ReadSingle();

                fs.Vector = new Vector3(vy,vz,vx);
			    fs.Max = _byteData.ReadSingle();

                float x = _byteData.ReadSingle();
                float y = _byteData.ReadSingle();
                float z = _byteData.ReadSingle();

                float dir_vx = _byteData.ReadSingle();
                float dir_vy = _byteData.ReadSingle();

                float dir_wx = _byteData.ReadSingle();
                float dir_wy = _byteData.ReadSingle();

                fs.Offset = new Vector3(y, z, -x);
                fs.Dir = new Vector3(dir_vx, 0, -dir_vy);
                fs.Normal = new Vector3(dir_wx, 0, -dir_wy);

                fs.play();

			    _focusSets[i] = fs;
			}
		}

        private void DownloadAssets()
        {
            if (progress_bar != null)
                progress_bar.SetRange(0, _meshesId.Length - 1, "Descargando archivos..");

            WebClient wc = new WebClient();
            for (int i = 0; i < _meshesId.Length; i++)
            {
                var mn = _meshesId[i];
                if (Path.GetFileNameWithoutExtension(mn).Length != 0 && !File.Exists(mn))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(mn));
                        string webpath = WEB_MESH_FOLDER + mn.Substring(MESH_FOLDER.Length).Replace('\\', '/').Replace(" ", "%20").ToLower();
                        //GuiController.Instance.Logger.log("Descargando archivo: " + webpath);
                        if (progress_bar != null)
                        {
                            progress_bar.SetPos(i);
                            progress_bar.text = "Descargando archivo: " + webpath;
                        }
                        GuiController.Instance.MessageLoop();

                        wc.DownloadFile(webpath, mn);
                    }
                    catch (Exception)
                    {
                        GuiController.Instance.Logger.log("Archivo: " + mn + " no se encuentra.");
                    }

                }
            }
        }

        private void LoadTextures()
        {
            if (progress_bar != null)
                progress_bar.SetRange(0, _cantTextures - 1, "Cargando texturas..");

            _textures = new TgcTexture[_cantTextures];
            for (int i = 0; i < _texturesId.Length; i++)
            {
                if (progress_bar != null)
                    progress_bar.SetPos(i);
                GuiController.Instance.MessageLoop();

                _textures[i] = FocusParser.getTexture(_texturesId[i]);
            }
        }
		
		private void LoadMeshes()
		{
			if(_meshesId.Length == 0)
				return;

            _meshes = new TgcMesh[_cantMeshes];
			for(int i = 0; i < _meshesId.Length; i++)
			{
                try
                {
                    var yparser = new YParser();
                    yparser.progress_bar = progress_bar;
			        yparser.FromFile(_meshesId[i]);
			        _meshes[i] = yparser.Mesh;
                }
                catch (Exception e)
                {
                    GuiController.Instance.Logger.log("Archivo: " + _meshesId[i] + " no se encuentra.");
                    //GuiController.Instance.Logger.log(e.Message);
                    //GuiController.Instance.Logger.log(e.StackTrace);
                }
                
			}
			
		}
		
		private void Build()
		{
			var materials = new Material[1];
		    materials[0] = new Material();

		    var index = new List<uint>();
            var vertex = new List<TgcSceneLoader.DiffuseMapVertex>();
            Vector3 pMin = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
            Vector3 pMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < _cantFaces; i++)
            {
                var f = _faces[i];
                if (f.NroMesh != -1)
                {
                    //el face tiene un mesh propio
                    ResolveMesh(i);
                    continue;
                }

                var offset = (uint)vertex.Count;
                index.Add(offset + 0);
                index.Add(offset + 1);
                index.Add(offset + 2);
                if (f.Tipo == 1)
                {
                    //rectangulo
                    index.Add(offset + 0);
                    index.Add(offset + 2);
                    index.Add(offset + 3);
                }

                for (int j = 0; j < 4; j++)
                {
                    var v = _vertices[i * 4 + j];
                    vertex.Add(v);
                    pMin = Vector3.Minimize(pMin, v.Position);
                    pMax = Vector3.Maximize(pMax, v.Position);
                }


                if (i == _cantFaces - 1 || f.Id != _faces[i + 1].Id)
                {
                    var m = new Mesh(index.Count / 3, vertex.Count, MeshFlags.Use32Bit | MeshFlags.Managed,
                                     TgcSceneLoader.DiffuseMapVertexElements, GuiController.Instance.D3dDevice);
                    m.SetIndexBufferData(index.ToArray(), LockFlags.None);
                    m.SetVertexBufferData(vertex.ToArray(), LockFlags.None);

                    var mesh = new TgcMesh(m, "Mesh" + i, TgcMesh.MeshRenderType.DIFFUSE_MAP);
                    mesh.Materials = materials;
                    if (f.NroTextura >= 0)
                        mesh.DiffuseMaps = new[] { _textures[f.NroTextura] };
                    else
                        mesh.DiffuseMaps = new[] { new TgcTexture("", "", null, false) };
                    mesh.Enabled = true;
                    mesh.BoundingBox = new TgcBoundingBox(pMin, pMax);
                    
                    mesh.kd = f.kd;
                    mesh.ks = f.ks;
                    mesh.kr = f.kr;
                    mesh.kt = f.kt;

                    if (f.NroConjunto == -1)
                        Escene.Add(mesh);
                    else
                        _focusSets[f.NroConjunto].container.Childs.Add(mesh);

                    index = new List<uint>();
                    vertex = new List<TgcSceneLoader.DiffuseMapVertex>();
                    pMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    pMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                }
            }


            foreach (FocusSet f in _focusSets)
            {
                f.container.createBoundingBox();
            }

		}
		

        private void ResolveMesh(int nroFace)
		{
            var face = _faces[nroFace];
            
            if (_meshes[face.NroMesh] == null)
                //hay un mesh no cargado
                return;

            var mesh = CloneTgcMesh(_meshes[face.NroMesh]);
			
			if(face.FlagEspejar > 0){
				float x = (face.FlagEspejar&Face.OBJ_ESPEJAR_X) > 0 ? -1 : 1;
				float y = (face.FlagEspejar&Face.OBJ_ESPEJAR_Y) > 0 ? -1 : 1;
				float z = (face.FlagEspejar&Face.OBJ_ESPEJAR_Z) > 0 ? -1 : 1;
			    Vector3 center = mesh.BoundingBox.calculateBoxCenter();
			    Matrix pMatrix = Matrix.Translation(-center)*
			                     Matrix.Scaling(x, y, z)*
			                     Matrix.Translation(center);
                face.MatWorld = pMatrix * face.MatWorld;
			}

            mesh.AutoTransformEnable = false;
			mesh.Transform = face.MatWorld;


            Vector3 pmin = Vector3.TransformCoordinate(mesh.BoundingBox.PMin, mesh.Transform);
            Vector3 pmax = Vector3.TransformCoordinate(mesh.BoundingBox.PMax, mesh.Transform);


            mesh.BoundingBox.setExtremes(Vector3.Minimize(pmin, pmax), Vector3.Maximize(pmax,pmin));

            mesh.kd = face.kd;
            mesh.ks = face.ks;
            mesh.kr = face.kr;
            mesh.kt = face.kt;

            var matLength = mesh.Materials.Length;
			for (int j = 0; j < matLength; j++) 
			{

                if (j > face.Layers.Length && face.Layers[j] != null)
                    continue;

                int nroTextura = face.Layers[j].NroTextura;
                Material mat = face.Layers[j].Material;//TODO: Ver mesh que no tienen texturas, pero si color
                if (nroTextura == -1 || nroTextura >= _textures.Length)
                    continue;
			    
                //tiene una textura propia el layer
			    mesh.DiffuseMaps[j] = _textures[nroTextura];
			}

            if(face.NroConjunto == -1)
                Escene.Add(mesh);
            else
                _focusSets[face.NroConjunto].container.Childs.Add(mesh);
            
		}

        private TgcMesh CloneTgcMesh(TgcMesh tgcMesh)
        {
            var m = new TgcMesh(tgcMesh.D3dMesh, tgcMesh.Name, tgcMesh.RenderType);
            m.Enabled = true;
            m.DiffuseMaps = (TgcTexture[])tgcMesh.DiffuseMaps.Clone();
            m.Materials = (Material[])tgcMesh.Materials.Clone();
            m.BoundingBox = tgcMesh.BoundingBox.clone();
            m.AlphaBlendEnable = tgcMesh.AlphaBlendEnable;

            return m;
        }

        private string ReadString(int size)
        {
            var str = System.Text.Encoding.ASCII.GetString(_byteData.ReadBytes(size));
            var end = str.IndexOf('\0');
            if (end != -1)
                str = str.Substring(0, end);
            return str;
        }

        public static TgcTexture getTexture(string path)
        {
            if (Path.GetFileNameWithoutExtension(path).Length == 0)
                return NULL_TEXTURE;

            string pngpath = Path.ChangeExtension(path, ".png");
            string jpgpath = Path.ChangeExtension(path, ".jpg");

            if (File.Exists(jpgpath))
                path = jpgpath;
            else
                path = pngpath;
            

            if (!downloadTexture(path))
            {
                path = Path.ChangeExtension(path, ".jpg");
                downloadTexture(path);
            }

            try
            {
                return TgcTexture.createTexture(GuiController.Instance.D3dDevice,
                                                 Path.GetFileName(path), path);
            }
            catch (Exception)
            {
                GuiController.Instance.Logger.log("Archivo: " + path + " no se encuentra.");
                return NULL_TEXTURE;
            }
        }

        public static bool downloadTexture(string path)
        {
            if (File.Exists(path))
                return true;

            try
            {
                WebClient wc = new WebClient();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string webpath = FocusParser.WEB_TEXTURE_FOLDER + path.Substring(FocusParser.TEXTURE_FOLDER.Length).Replace('\\', '/').Replace(" ", "%20").ToLower();
                wc.DownloadFile(webpath, path);
                GuiController.Instance.Logger.log("Descargado: " + webpath);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
		
	}
}

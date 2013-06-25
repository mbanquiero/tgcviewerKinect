using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System.Net;

namespace Examples.Focus
{
    public class YParser
    {
        private TgcMesh _mesh;
        private BinaryReader _byteData;
        private int _cantLayers;
        private int _cantFaces;
        private int _cantVertices;
        private int _sizeofVertex;
        private TgcSceneLoader.DiffuseMapVertex[] _vertices;
        private uint[] _indices;
        private int[] _atributos;
        private string[] _materialNames;
        private Material[] _materialData;
        private TgcTexture[] _textures;
        private Vector3 minVert = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 maxVert = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        public YParser()
        {
        }

        public TgcMesh Mesh
        {
            get { return _mesh; }
        }

        public void FromFile(string path)
        {
            var fp = new FileStream(path, FileMode.Open, FileAccess.Read);
            _byteData = new BinaryReader(fp);

            ParseHeader();
			ParseLayers();
			ParseVertices();
			ParseIndices();
			ParsedAtributos();

            _byteData.Close();
            fp.Close();

            DownloadAssets();
            LoadTextures();
			BuildMesh();
        }

        private void ParseHeader()
		{
			var header = new int[9];
			for (int i = 0; i < header.Length; i++) 
			{
				header[i] = _byteData.ReadInt32();
			}
			
			int version = header[6];
		}

        private void ParseLayers()
		{
            //cantidad de layers
			_cantLayers = _byteData.ReadInt32();
            _materialNames = new string[_cantLayers];
            _materialData = new Material[_cantLayers];
            for (int i = 0; i < _cantLayers; i++)
			{
				//nombre de la textura
				string name = ReadString(256);
				string path = FocusParser.TEXTURE_FOLDER + name;

                if (Path.GetExtension(path).ToLower() == ".bmp")
                    path = Path.ChangeExtension(path, ".png");
				
				_materialNames[i] = path;
				
				//material
				var mat = new Material();
				mat.DiffuseColor = new ColorValue(_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle());
				mat.AmbientColor = new ColorValue(_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle());
				mat.SpecularColor = new ColorValue(_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle());
				mat.EmissiveColor = new ColorValue(_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle(),_byteData.ReadSingle());
				mat.SpecularSharpness = _byteData.ReadSingle();
				
				_materialData[i] = mat;
			}
		}

        private void ParseVertices()
		{
			//numero de faces
			_cantFaces = _byteData.ReadInt32();
			
			//vertices
			_cantVertices = _byteData.ReadInt32();

            _vertices = new TgcSceneLoader.DiffuseMapVertex[_cantVertices];
			
			//bytes por vertice
			_sizeofVertex = _byteData.ReadInt32();
						
			for (int i = 0; i < _cantVertices; i++)
			{
                var p = new TgcSceneLoader.DiffuseMapVertex();
				float x = _byteData.ReadSingle();
				float z = _byteData.ReadSingle();
				float y = _byteData.ReadSingle();				
				//_vertices.push(new Vertex(x, y, z));
				p.Position = new Vector3(x,y,-z);

			    minVert = Vector3.Minimize(minVert, p.Position);
                maxVert = Vector3.Maximize(maxVert, p.Position);
				
				x = _byteData.ReadSingle();
				z = _byteData.ReadSingle();
				y = _byteData.ReadSingle();

				p.Normal = new Vector3(x,y,-z);
                p.Color = 0xFFFFFF;

				if(_sizeofVertex >= 32)
				{
					p.Tu = _byteData.ReadSingle();
                    p.Tv = _byteData.ReadSingle();
				}
				
				if(_sizeofVertex > 32)
					_byteData.ReadBytes(_sizeofVertex - 32);
			    _vertices[i] = p;
			}
		}

        private void ParseIndices()
		{
			int cantIndices = _cantFaces * 3;

            _indices = new uint[cantIndices];

            if (_byteData.BaseStream.Length - _byteData.BaseStream.Position < cantIndices * 4)
			{
				//es un mesh sin la informacion de los indices
                for (uint i = 0; i < cantIndices; i++)
				{
					_indices[i] = i;
				}
			}
			else
			{
                for (int i = 0; i < cantIndices; i++)
				{
					_indices[i] = (uint)_byteData.ReadInt32();
				}
			}
		}


        private void ParsedAtributos()
		{
			int cantAtrib = _cantFaces;

            _atributos = new int[cantAtrib];

            for (int i = 0; i < cantAtrib; i++)
			{
				if(_byteData.BaseStream.Length - _byteData.BaseStream.Position >= 4)
				{
					_atributos[i] = _byteData.ReadInt32();
				}
				else
				{
					_atributos[i] = _atributos[i-1];
				}
			}
		}

        private void DownloadAssets()
        {
            return;
            WebClient wc = new WebClient();
            for (int i = 0; i < _materialNames.Length; i++)
            {
                var mnpng = _materialNames[i];
                var mnjpg = Path.ChangeExtension(mnpng, ".jpg");
                if (Path.GetFileNameWithoutExtension(mnpng).Length != 0 && (!File.Exists(mnpng) && !File.Exists(mnjpg)))
                {

                    Directory.CreateDirectory(Path.GetDirectoryName(mnpng));
                    string webpath = FocusParser.WEB_TEXTURE_FOLDER + mnpng.Substring(FocusParser.TEXTURE_FOLDER.Length).Replace('\\', '/').Replace(" ", "%20").ToLower();
                    GuiController.Instance.Logger.log("Descargando archivo: " + webpath);
                    try
                    {
                            wc.DownloadFile(webpath, mnpng);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            wc.DownloadFile(Path.ChangeExtension(webpath,".jpg"), mnjpg);
                            _materialNames[i] = mnjpg;
                        }
                        catch (Exception)
                        {
                            GuiController.Instance.Logger.log("Archivo: " + mnjpg + " no se encuentra.");
                        }

                    }

                }
            }

        }
        private void LoadTextures()
        {
            _textures = new TgcTexture[_cantLayers];
            for (int i = 0; i < _materialNames.Length; i++)
            {
                var mn = _materialNames[i];
                _textures[i] = FocusParser.getTexture(mn);
            }

        }

        private void BuildMesh()
		{
            SortAtributes();
            var at = new AttributeRange[_cantLayers];
			for (int i = 0; i < _cantLayers; i++) 
			{
                var ar = new AttributeRange {AttributeId = i};
			    int fi = -1, li = -1;
			    for (int j = 0; j < _cantFaces; j++)
			    {
			        if(_atributos[j] == i)
			        {
                        if(fi == -1)
                            fi = j;

			            li = j;
			        }
			    }
                if(fi != -1)
                {
                    ar.FaceCount = li - fi + 1;
                    ar.FaceStart = fi;
                    ar.VertexStart = 0;
                    ar.VertexCount = _cantVertices;
                }

                at[i] = ar;
		    }

            var m = new Mesh(_cantFaces, _cantVertices, MeshFlags.Use32Bit | MeshFlags.Managed, TgcSceneLoader.DiffuseMapVertexElements,
                    GuiController.Instance.D3dDevice);
            
            m.SetAttributeTable(at);
            var atrib = m.LockAttributeBufferArray(LockFlags.None);
            _atributos.CopyTo(atrib,0);
            m.UnlockAttributeBuffer(atrib);
            m.SetVertexBufferData(_vertices, LockFlags.None);
            m.SetIndexBufferData(_indices, LockFlags.None);

            _mesh = new TgcMesh(m, "mesh", TgcMesh.MeshRenderType.DIFFUSE_MAP)
                        {
                            Materials = _materialData,
                            DiffuseMaps = _textures,
                            Enabled = true,
                            AlphaBlendEnable = false,
                        };

            _mesh.BoundingBox = new TgcBoundingBox(minVert,maxVert);
		}

        private void SortAtributes()
        {
            for (int i = 1; i < _atributos.Length; i++)
            {
                if(_atributos[i-1] > _atributos[i])
                    throw new Exception("Los atributos nos estan Ordenados. Implementar sort de atributos.");
            }
        }

        private string ReadString(int size)
        {
            var str = System.Text.Encoding.ASCII.GetString(_byteData.ReadBytes(size));
            var end = str.IndexOf('\0');
            if (end != -1)
                str = str.Substring(0, end);
            return str;
        }
    }
}

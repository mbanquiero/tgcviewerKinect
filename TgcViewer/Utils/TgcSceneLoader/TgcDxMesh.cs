

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using System.IO;

namespace TgcViewer.Utils.TgcSceneLoader
{
    public class TgcDXMesh
    {
        public Material[] meshMaterials;
        public Texture[] meshTextures;
        private Mesh mesh;
        public Vector3 bb_p0 = new Vector3();
        public Vector3 bb_p1 = new Vector3();
        public Vector3 size = new Vector3();
        public Vector3 center = new Vector3();
        public Matrix transform;

        public void loadMesh(string path)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            ExtendedMaterial[] mtrl;

            //Cargar mesh con utilidad de DirectX
            mesh = Mesh.FromFile(path, MeshFlags.Managed, d3dDevice, out mtrl);

            //Analizar todos los subset de la malla
            if ((mtrl != null) && (mtrl.Length > 0))
            {
                meshMaterials = new Material[mtrl.Length];
                meshTextures = new Texture[mtrl.Length];

                //Cargar los material y texturas en un array
                for (int i = 0; i < mtrl.Length; i++)
                {
                    //Cargar material
                    meshMaterials[i] = mtrl[i].Material3D;

                    //Si hay textura, intentar cargarla
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename !=
                        string.Empty))
                    {

                        String fname_aux = mtrl[i].TextureFilename;
                        // Verifico si esta el archivo con el path asi como viene
                        if (!File.Exists(fname_aux))
                        {
                            // Pruebo con la carpeta de texturas
                            fname_aux = GuiController.Instance.ExamplesMediaDir + "ModelosX\\" + mtrl[i].TextureFilename;
                            if (!File.Exists(fname_aux))
                                // Usa una textura gris para que al menos salga un color
                                fname_aux = GuiController.Instance.ExamplesMediaDir + "focus\\texturas\\gris.png";
                        }

                        //Cargar textura con TextureLoader
                        meshTextures[i] = TextureLoader.FromFile(d3dDevice, fname_aux);
                    }
                }
            }


            //Crear Bounding Sphere con herramienta de Geometry DirectX 
            using (VertexBuffer vb = mesh.VertexBuffer)
            {
                GraphicsStream vertexData = vb.Lock(0, 0, LockFlags.None);
                Geometry.ComputeBoundingBox(vertexData,mesh.NumberVertices,mesh.VertexFormat,out bb_p0,out bb_p1);
                vb.Unlock();
                size.X = (float)Math.Abs(bb_p0.X - bb_p1.X);
                size.Y = (float)Math.Abs(bb_p0.Y - bb_p1.Y);
                size.Z = (float)Math.Abs(bb_p0.Z - bb_p1.Z);

                center = (bb_p0 + bb_p1) * 0.5f;

            }

        }

        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            Effect effect = GuiController.Instance.Shaders.TgcMeshPhongShader;
            effect.Technique = "DIFFUSE_MAP";
            GuiController.Instance.Shaders.setShaderMatrix(effect, transform);
            //Cargar variables shader
            effect.SetValue("ambientColor", ColorValue.FromColor(Color.Gray));
            effect.SetValue("diffuseColor", ColorValue.FromColor(Color.LightBlue));
            effect.SetValue("specularColor", ColorValue.FromColor(Color.White));
            effect.SetValue("specularExp", 10f);


            //Iniciar Shader e iterar sobre sus Render Passes
            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                //Dibujar cada subset con su DiffuseMap correspondiente
                for (int i = 0; i < meshMaterials.Length; i++)
                {
                    //Setear textura en shader
                    effect.SetValue("texDiffuseMap",meshTextures[i]);

                    //Iniciar pasada de shader
                    effect.BeginPass(n);
                    mesh.DrawSubset(i);
                    effect.EndPass();
                }
            }
            //Finalizar shader
            effect.End();

        }

        public void close()
        {
            //Liberar recursos de la malla
            mesh.Dispose();
        }
    }
}

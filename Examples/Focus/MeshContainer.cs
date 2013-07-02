using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using System.Drawing;

namespace Examples.Focus
{
    public class MeshContainer : TgcMesh
    {

        protected List<TgcMesh> childs;
        protected Vector3 pivot;

        public List<TgcMesh> Childs
        {
            get { return childs; }
            set { childs = value; }
        }

        public Vector3 Pivot
        {
            get { return pivot; }
            set { pivot = value; }
        }

        /// <summary>
        /// Constructor vacio, para facilitar la herencia de esta clase.
        /// </summary>
        public MeshContainer() : this("MeshContainer")
        {

        }

        /// <summary>
        /// Crea una nueva malla.
        /// </summary>
        /// <param name="mesh">Mesh de DirectX</param>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="renderType">Formato de renderizado de la malla</param>
        public MeshContainer(string name)
        {
            childs = new List<TgcMesh>();
            pivot = new Vector3();
            initData(null, name, MeshRenderType.DIFFUSE_MAP);
        }

        /// <summary>
        /// Cargar datos iniciales
        /// </summary>
        protected new void initData(Mesh mesh, string name, MeshRenderType renderType)
        {
            this.d3dMesh = mesh;
            this.name = name;
            this.renderType = renderType;
            this.enabled = false;
            this.parentInstance = null;
            this.meshInstances = new List<TgcMesh>();
            this.alphaBlendEnable = false;

            this.autoTransformEnable = true;
            this.AutoUpdateBoundingBox = true;
            this.translation = new Vector3(0f, 0f, 0f);
            this.rotation = new Vector3(0f, 0f, 0f);
            this.scale = new Vector3(1f, 1f, 1f);
            this.transform = Matrix.Identity;
        }

        /// <summary>
        /// Renderiza la malla, si esta habilitada
        /// </summary>
        public new void render()
        {
            if (!enabled)
                return;

            //Aplicar transformaciones
            updateMeshTransform();

            foreach (TgcMesh child in childs)
            {
                child.updateMeshTransform();
                Matrix childTransform = child.Transform;
                child.AutoTransformEnable = false;

                child.Transform = childTransform * transform;
                child.render();
                //child.BoundingBox.render();
                child.Transform = childTransform;
            }

        }

        /// <summary>
        /// Aplicar transformaciones del mesh
        /// </summary>
        protected new void updateMeshTransform()
        {
            //Aplicar transformacion de malla
            if (autoTransformEnable)
            {
                this.transform = Matrix.Scaling(scale)
                    * Matrix.Translation(-pivot)
                    * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)                    
                    * Matrix.Translation(translation + pivot);
            }
        }

        /// <summary>
        /// Libera los recursos de la malla.
        /// Si la malla es una instancia se deshabilita pero no se liberan recursos.
        /// Si la malla es el original y tiene varias instancias adjuntadas, se hace dispose() también de las instancias.
        /// </summary>
        public new void dispose()
        {
            this.enabled = false;
            if (boundingBox != null)
            {
                boundingBox.dispose();
            }

            //Si es una instancia no liberar nada, lo hace el original.
            if (parentInstance != null)
            {
                parentInstance = null;
                return;
            }

            //hacer dispose de instancias
            foreach (TgcMesh meshInstance in meshInstances)
            {
                meshInstance.dispose();
            }
            meshInstances = null;

            //hacer dispose de los submeshes
            foreach (TgcMesh child in childs)
            {
                child.dispose();
            }

        }


        /// <summary>
        /// Devuelve un array con todas las posiciones de los vértices de la malla
        /// </summary>
        /// <returns>Array creado</returns>
        public new Vector3[] getVertexPositions()
        {
            return new Vector3[0];
        }

        /// <summary>
        /// Calcula el BoundingBox de la malla, en base a todos sus vertices.
        /// Llamar a este metodo cuando ha cambiado la estructura interna de la malla.
        /// </summary>
        public new TgcBoundingBox createBoundingBox()
        {

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            //hacer dispose de los submeshes
            foreach (TgcMesh child in childs)
            {
                min.Minimize(child.BoundingBox.PMin);
                max.Maximize(child.BoundingBox.PMax);
            }

            if(childs.Count > 0)
                this.boundingBox = new TgcBoundingBox(min,max);
            else
                this.boundingBox = new TgcBoundingBox();

            return this.boundingBox;
        }

        /// <summary>
        /// Actualiza el BoundingBox de la malla, en base a su posicion actual.
        /// Solo contempla traslacion y escalado
        /// </summary>
        public new void updateBoundingBox()
        {
            if (AutoUpdateBoundingBox)
            {
                this.boundingBox.scaleTranslate(this.translation, this.scale);
            }
        }

        /// <summary>
        /// Cambia el color de todos los vértices de la malla.
        /// En modelos complejos puede resultar una operación poco performante.
        /// </summary>
        /// <param name="color">Color nuevo</param>
        public new void setColor(Color color)
        {
        }
    }
}

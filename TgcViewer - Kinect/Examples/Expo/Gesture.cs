using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;

namespace Examples.Expo
{
    /// <summary>
    /// Representa un gesto detectado
    /// </summary>
    public struct Gesture
    {
        Vector3 pos;
        /// <summary>
        /// Posicion central del gesto.
        /// Depende cada caso. En abrir cajon por ejemplo solo importa XY y no la Z.
        /// </summary>
        public Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        GestureType type;
        /// <summary>
        /// Tipo de gesto
        /// </summary>
        public GestureType Type
        {
            get { return type; }
            set { type = value; }
        }

        public Gesture(Vector3 pos, GestureType type)
        {
            this.pos = pos;
            this.type = type;
        }

    }

    /// <summary>
    /// Tipos de gestos
    /// </summary>
    public enum GestureType
    {
        OpenZ,
        CloseZ,
    }

}
